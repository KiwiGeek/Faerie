using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Parsing;

/// <summary>
/// Turns a line of raw text into a <see cref="ParsedCommand"/>. Understands bare directions,
/// intransitive verbs ("look"), transitive verbs ("take key", "give condom") and ditransitive
/// verbs joined by a preposition ("give condom to girl", "put key in lock"). Articles and a few
/// filler words are ignored, and every object is matched through its synonyms and adjectives.
/// </summary>
public sealed class Parser(VerbLibrary verbs)
{
    private static readonly HashSet<string> Articles = ["a", "an", "the", "some", "any"];
    private static readonly HashSet<string> Noise = ["please"];

    private readonly VerbLibrary _verbs = verbs;

    public ParsedCommand Parse(string input, Scope scope, GameState state)
    {
        List<string> tokens = Tokenize(input);
        if (tokens.Count == 0) return ParsedCommand.Empty();

        // 1. A bare direction ("n", "north", "go up" handled below via the go verb).
        if (tokens.Count == 1 && DirectionExtensions.TryParse(tokens[0], out Direction bareDir))
        {
            Verb? goVerb = _verbs.FindById(StandardVerbIds.Go);
            return goVerb is null
                ? ParsedCommand.Unknown($"You can't go anywhere with \"{tokens[0]}\".")
                : new ParsedCommand { Status = ParseStatus.Ok, Verb = goVerb, Direction = bareDir };
        }

        // 2. Match the verb (longest phrase wins).
        if (!_verbs.TryMatchVerb(tokens, out Verb matched, out int consumed))
            return UnknownVerbFailure(tokens, scope);

        string phrase = string.Join(' ', tokens.Take(consumed));
        List<string> rest = tokens.Skip(consumed).Where(t => !Articles.Contains(t) && !Noise.Contains(t)).ToList();

        // A word like "move" can be both a movement verb and an object verb ("move north" vs
        // "move rug"). When several verbs share the typed word, pick based on the argument.
        Verb verb = ChooseVerb([.. _verbs.VerbsFor(phrase)], matched, rest);

        // 3. Movement verb: the remainder should be a direction.
        if (verb.Id == StandardVerbIds.Go)
        {
            if (rest.Count >= 1 && DirectionExtensions.TryParse(rest[^1], out Direction dir))
                return new ParsedCommand { Status = ParseStatus.Ok, Verb = verb, Direction = dir };
            if (rest.Count == 0)
                return ParsedCommand.NoObject("Go where?");
            (string dirMsg, string? dirInput) = UnknownDirectionMessage(verb, rest);
            return ParsedCommand.Unknown(dirMsg, dirInput);
        }

        // 4. No objects -> intransitive.
        if (rest.Count == 0)
            return new ParsedCommand { Status = ParseStatus.Ok, Verb = verb };

        // 5. Split on a preposition into direct / indirect object phrases.
        IReadOnlyList<string> preps = verb.Prepositions.Count > 0 ? verb.Prepositions : VerbLibrary.DefaultPrepositions;
        int prepIndex = rest.FindIndex(t => preps.Contains(t));

        List<string> dobjTokens;
        List<string> iobjTokens = [];
        string? preposition = null;

        if (prepIndex >= 0)
        {
            dobjTokens = rest.Take(prepIndex).ToList();
            preposition = rest[prepIndex];
            iobjTokens = rest.Skip(prepIndex + 1).ToList();
        }
        else
        {
            dobjTokens = rest;
        }

        // 6. Resolve the direct object (if any words remain).
        Thing? dobj = null;
        string? dobjText = dobjTokens.Count > 0 ? string.Join(' ', dobjTokens) : null;
        if (dobjTokens.Count > 0)
        {
            NounResolution r = scope.Resolve(dobjTokens);
            switch (r.Status)
            {
                case NounResolution.Kind.NotFound:
                {
                    (string msg, string? suggested) = WordSuggest.AppendSuggestion(
                        $"You can't see any {dobjText} here.",
                        dobjText!,
                        Vocabulary.VisibleNouns(scope),
                        commandPrefix: phrase);
                    return ParsedCommand.NoObject(msg, suggested);
                }
                case NounResolution.Kind.Ambiguous:
                    return ParsedCommand.Ambiguous(Disambiguation(r.Candidates), r.Candidates);
                case NounResolution.Kind.Single:
                    dobj = r.Thing;
                    break;
            }
        }

        // 7. Resolve the indirect object.
        Thing? iobj = null;
        string? iobjText = iobjTokens.Count > 0 ? string.Join(' ', iobjTokens) : null;
        if (iobjTokens.Count > 0)
        {
            NounResolution r = scope.Resolve(iobjTokens);
            switch (r.Status)
            {
                case NounResolution.Kind.NotFound:
                {
                    string prefix = preposition is null
                        ? phrase
                        : $"{phrase} {dobjText} {preposition}";
                    (string msg, string? suggested) = WordSuggest.AppendSuggestion(
                        $"You can't see any {iobjText} here.",
                        iobjText!,
                        Vocabulary.VisibleNouns(scope),
                        commandPrefix: prefix);
                    return ParsedCommand.NoObject(msg, suggested);
                }
                case NounResolution.Kind.Ambiguous:
                    return ParsedCommand.Ambiguous(Disambiguation(r.Candidates), r.Candidates);
                case NounResolution.Kind.Single:
                    iobj = r.Thing;
                    break;
            }
        }

        return new ParsedCommand
        {
            Status = ParseStatus.Ok,
            Verb = verb,
            DirectObject = dobj,
            IndirectObject = iobj,
            Preposition = preposition,
            DirectObjectText = dobjText,
            IndirectObjectText = iobjText
        };
    }

    /// <summary>
    /// When the typed word maps to several verbs, choose one by the argument: a direction picks the
    /// movement verb; otherwise an object-taking verb is preferred.
    /// </summary>
    private static Verb ChooseVerb(List<Verb> candidates, Verb fallback, List<string> rest)
    {
        if (candidates.Count <= 1) return fallback;

        bool argIsDirection = rest.Count >= 1 && DirectionExtensions.TryParse(rest[^1], out _);

        Verb? move = candidates.FirstOrDefault(v => v.Id == StandardVerbIds.Go);
        if (argIsDirection && move is not null) return move;

        // Later-registered verbs override standard ones (e.g. Zork's custom "move" after AddCoreVerbs).
        Verb? objectVerb = candidates.LastOrDefault(v =>
            v.Id != StandardVerbIds.Go && (v.Accepts(VerbForms.Transitive) || v.Accepts(VerbForms.Ditransitive)));
        if (rest.Count > 0 && objectVerb is not null) return objectVerb;

        if (rest.Count == 0)
            return candidates.FirstOrDefault(v => v.Accepts(VerbForms.Intransitive)) ?? fallback;

        return fallback;
    }

    private ParsedCommand UnknownVerbFailure(IReadOnlyList<string> tokens, Scope scope)
    {
        string verbToken = tokens[0];
        List<string> rest = tokens.Skip(1).Where(t => !Articles.Contains(t) && !Noise.Contains(t)).ToList();

        string? verbFix = WordSuggest.SingleOrNull(
            WordSuggest.FindCloseMatches(verbToken, Vocabulary.VerbLeadingWords(_verbs)));

        string? nounFix = null;
        if (rest.Count > 0)
        {
            string objText = string.Join(' ', rest);
            nounFix = WordSuggest.SingleOrNull(
                WordSuggest.FindCloseMatches(objText, Vocabulary.VisibleNouns(scope)));
        }

        string message = $"I don't know how to \"{verbToken}\".";
        string? suggestedInput = null;

        if (verbFix is not null && nounFix is not null)
        {
            suggestedInput = $"{verbFix} {nounFix}";
            message += $" Did you mean {suggestedInput}?";
        }
        else if (verbFix is not null)
        {
            suggestedInput = verbFix;
            message += $" Did you mean {verbFix}?";
        }
        else if (nounFix is not null)
        {
            message += $" Did you mean {nounFix}?";
        }

        return ParsedCommand.Unknown(message, suggestedInput);
    }

    private (string message, string? suggestedInput) UnknownDirectionMessage(Verb verb, List<string> rest)
    {
        string bad = string.Join(' ', rest);
        string token = rest[^1];
        IReadOnlyList<string> matches = WordSuggest.FindCloseMatches(token, Vocabulary.DirectionWords());
        if (matches.Count == 0 || !DirectionExtensions.TryParse(matches[0], out Direction dir))
            return ($"\"{bad}\" isn't a direction.", null);

        string display = $"{verb.Words[0].ToUpperInvariant()} {dir.ToString().ToUpperInvariant()}";
        string suggestedInput = $"{verb.Words[0]} {dir.Words()[0]}";
        return ($"\"{bad}\" isn't a direction. Did you mean {display}?", suggestedInput);
    }

    private static string Disambiguation(IReadOnlyList<Thing> candidates)
    {
        string list = candidates.Count == 2
            ? $"the {candidates[0].Name} or the {candidates[1].Name}"
            : string.Join(", ", candidates.Take(candidates.Count - 1).Select(c => $"the {c.Name}")) +
              $", or the {candidates[^1].Name}";
        return $"Which do you mean: {list}?";
    }

    private static List<string> Tokenize(string input)
    {
        List<string> tokens = [];
        foreach (string raw in input.Trim().ToLowerInvariant().Split([' ', '\t', ',', '.', '!', '?'], StringSplitOptions.RemoveEmptyEntries))
            tokens.Add(raw);
        return tokens;
    }
}
