using Faerie.Building;
using Faerie.Model;
using Faerie.Parsing;
using Faerie.Runtime;
using Faerie.Verbs;
using Xunit;

namespace Faerie.Tests;

public sealed class TypoSuggestionTests
{
    // ---- Levenshtein ------------------------------------------------------------------------

    [Theory]
    [InlineData(0, "", "")]
    [InlineData(1, "a", "")]
    [InlineData(1, "", "a")]
    [InlineData(0, "north", "north")]
    [InlineData(2, "notrh", "north")]
    [InlineData(2, "take", "tkae")]
    [InlineData(1, "lamp", "lam")]
    [InlineData(3, "kitten", "sitting")]
    public void Levenshtein_Distance(int expected, string a, string b)
    {
        Assert.Equal(expected, Levenshtein.Distance(a, b));
        Assert.Equal(expected, Levenshtein.Distance(b, a));
    }

    // ---- WordSuggest ------------------------------------------------------------------------

    [Fact]
    public void FindCloseMatches_ExcludesExactMatch()
    {
        IReadOnlyList<string> hits = WordSuggest.FindCloseMatches("take", ["take", "tale"]);
        Assert.DoesNotContain("take", hits);
    }

    [Fact]
    public void FindCloseMatches_ReturnsClosestOnly()
    {
        IReadOnlyList<string> hits = WordSuggest.FindCloseMatches("tkae", ["make", "take", "wake"]);
        Assert.Equal(["take"], hits);
    }

    [Fact]
    public void FindCloseMatches_TiesReturnAllEquallyClose()
    {
        IReadOnlyList<string> hits = WordSuggest.FindCloseMatches("dat", ["bat", "cat", "hat"]);
        Assert.Equal(3, hits.Count);
        Assert.Contains("bat", hits);
        Assert.Contains("cat", hits);
        Assert.Contains("hat", hits);
    }

    [Fact]
    public void FindCloseMatches_RespectsMaxDistance()
    {
        Assert.Empty(WordSuggest.FindCloseMatches("xyz", ["take", "lamp"], maxDistance: 1));
    }

    [Fact]
    public void FindCloseMatches_ShortWordsAllowDistanceOne()
    {
        IReadOnlyList<string> hits = WordSuggest.FindCloseMatches("ky", ["key"], maxDistance: 1);
        Assert.Equal(["key"], hits);
    }

    [Theory]
    [InlineData(1, "Did you mean take?")]
    [InlineData(2, "Did you mean take or tale?")]
    public void DidYouMean_FormatsMatchCount(int count, string expected)
    {
        List<string> matches = count switch
        {
            1 => ["take"],
            2 => ["take", "tale"],
            _ => throw new ArgumentOutOfRangeException()
        };
        Assert.Equal(expected, WordSuggest.DidYouMean(matches));
    }

    [Fact]
    public void WithSuggestion_AppendsClauseWhenMatchExists()
    {
        string msg = WordSuggest.WithSuggestion("Nope.", ["take", "drop"], "tkae");
        Assert.Equal("Nope. Did you mean take?", msg);
    }

    [Fact]
    public void AppendSuggestion_WithPrefix_ReturnsFullCommand()
    {
        (string msg, string? input) = WordSuggest.AppendSuggestion(
            "Nope.", "lam", ["lamp"], commandPrefix: "take");
        Assert.Equal("Nope. Did you mean lamp?", msg);
        Assert.Equal("take lamp", input);
    }

    [Fact]
    public void AppendSuggestion_Ambiguous_NoSuggestedInput()
    {
        (string _, string? input) = WordSuggest.AppendSuggestion(
            "Nope.", "dat", ["bat", "cat", "hat"]);
        Assert.Null(input);
    }

    [Fact]
    public void SingleOrNull_ReturnsOnlyForOneMatch()
    {
        Assert.Equal("take", WordSuggest.SingleOrNull(["take"]));
        Assert.Null(WordSuggest.SingleOrNull([]));
        Assert.Null(WordSuggest.SingleOrNull(["take", "tale"]));
    }

    // ---- Vocabulary -------------------------------------------------------------------------

    [Fact]
    public void Vocabulary_VerbWords_IncludesPhrasesAndParts()
    {
        GameBuilder b = GameBuilder.Create("T");
        Room hall = b.Room("Hall").Describe("A hall.");
        b.StartIn(hall);
        b.DefineVerb("pick", ["pick up", "get"], VerbForms.Transitive, _ => VerbResult.Pass);
        VerbLibrary lib = b.Build().Verbs;

        List<string> words = Vocabulary.VerbWords(lib).ToList();
        Assert.Contains("pick up", words);
        Assert.Contains("pick", words);
        Assert.Contains("get", words);
    }

    [Fact]
    public void Vocabulary_VisibleNouns_IncludesNounsAndAdjectives()
    {
        GameBuilder b = GameBuilder.Create("T").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        Thing key = b.Item("brass key").Adjectives("brass").Describe("A key.");
        key.StartsIn(hall);
        b.StartIn(hall);
        Game game = b.Build();

        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();

        List<string> nouns = Vocabulary.VisibleNouns(new Scope(engine.State, new GameContext(engine, engine.State, engine.Out))).ToList();
        Assert.Contains("brass key", nouns);
        Assert.Contains("brass", nouns);
    }

    // ---- Parser integration -----------------------------------------------------------------

    private static (GameEngine engine, InMemoryTerminal term, Thing lamp) BuildHallWithLamp()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A plain hall.");
        Thing lamp = b.Item("lamp").Describe("A brass lamp.").LightSource(lit: false);
        lamp.StartsIn(hall);
        b.StartIn(hall);
        Game game = b.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();
        term.Reset();
        return (engine, term, lamp);
    }

    private static (GameEngine engine, InMemoryTerminal term) BuildHallWithMailbox()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A plain hall.");
        Thing mailbox = b.Scenery("small mailbox").Called("mailbox").Adjectives("small")
            .Describe("The small mailbox is closed.").Container(open: false);
        mailbox.StartsIn(hall);
        b.StartIn(hall);
        Game game = b.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();
        term.Reset();
        return (engine, term);
    }

    [Fact]
    public void FindCloseMatches_Mailbo_MatchesMailbox()
    {
        IReadOnlyList<string> hits = WordSuggest.FindCloseMatches(
            "mailbo", ["small mailbox", "mailbox", "small"]);
        Assert.Contains("mailbox", hits);
        Assert.Equal("mailbox", WordSuggest.SingleOrNull(hits));
    }

    [Fact]
    public void FindCloseMatches_Opne_MatchesOpenUniquely()
    {
        GameBuilder b = GameBuilder.Create("T").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A hall.");
        b.StartIn(hall);
        IReadOnlyList<string> hits = WordSuggest.FindCloseMatches(
            "opne", Vocabulary.VerbLeadingWords(b.Build().Verbs));
        Assert.Equal("open", WordSuggest.SingleOrNull(hits));
    }

    [Fact]
    public void Submit_UnknownVerbAndObject_SuggestsFullCommand()
    {
        var (engine, term) = BuildHallWithMailbox();
        engine.Submit("opne mailbo");
        Assert.Contains("Did you mean open mailbox?", term.Output);
        Assert.Equal("open mailbox", engine.SuggestedInput);
    }

    [Fact]
    public void Submit_UnknownVerb_SuggestsClosestVerb()
    {
        var (engine, term, _) = BuildHallWithLamp();
        engine.Submit("tkae lamp");
        Assert.Contains("I don't know how to \"tkae\"", term.Output);
        Assert.Contains("Did you mean take?", term.Output);
        Assert.Equal("take", engine.SuggestedInput);
    }

    [Fact]
    public void Submit_BadDirection_SuggestsGoNorth()
    {
        var (engine, term, _) = BuildHallWithLamp();
        engine.Submit("go notrh");
        Assert.Contains("isn't a direction", term.Output);
        Assert.Contains("Did you mean GO NORTH?", term.Output);
        Assert.Equal("go north", engine.SuggestedInput);
    }

    [Fact]
    public void Submit_UnknownObject_SuggestsVisibleNoun()
    {
        var (engine, term, _) = BuildHallWithLamp();
        engine.Submit("take lam");
        Assert.Contains("You can't see any lam here", term.Output);
        Assert.Contains("Did you mean lamp?", term.Output);
        Assert.Equal("take lamp", engine.SuggestedInput);
    }

    [Fact]
    public void Submit_UnknownMultiWordObject_SuggestsPhrase()
    {
        GameBuilder b = GameBuilder.Create("Test").AddStandardVerbs();
        Room hall = b.Room("Hall").Describe("A plain hall.");
        Thing key = b.Item("brass key").Adjectives("brass").Describe("A key.");
        key.StartsIn(hall);
        b.StartIn(hall);
        Game game = b.Build();
        InMemoryTerminal term = new();
        GameEngine engine = new(game, term, randomSeed: 1);
        engine.Start();
        term.Reset();

        engine.Submit("take brass ky");
        Assert.Contains("You can't see any brass ky here", term.Output);
        Assert.Contains("Did you mean brass key?", term.Output);
        Assert.Equal("take brass key", engine.SuggestedInput);
    }

    [Fact]
    public void Submit_TooDistantTypo_NoSuggestion()
    {
        var (engine, term, _) = BuildHallWithLamp();
        engine.Submit("qxzq lamp");
        Assert.Contains("I don't know how to \"qxzq\"", term.Output);
        Assert.DoesNotContain("Did you mean", term.Output);
        Assert.Null(engine.SuggestedInput);
    }

    [Fact]
    public void Submit_ValidCommand_ClearsSuggestedInput()
    {
        var (engine, term, lamp) = BuildHallWithLamp();
        engine.Submit("take lam");
        Assert.Equal("take lamp", engine.SuggestedInput);
        term.Reset();
        engine.Submit("take lamp");
        Assert.True(engine.State.IsCarried(lamp));
        Assert.Null(engine.SuggestedInput);
    }
}
