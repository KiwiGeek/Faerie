# Writing a Game from Scratch

This is a complete, beginner-friendly guide to building your own text adventure with the engine.
You do **not** need to understand how the engine works inside. You only need to know how to write a
little C#. Every example below is real code you can paste and adjust.

If you can copy a recipe and change the words, you can make a game.

---

## 1. The mental model (read this once)

A game is made of four kinds of things:

- **Rooms** — places the player can be (the Kitchen, the Forest, the Bridge of a starship).
- **Things** — objects in the world: items you can pick up, scenery you can look at, doors,
  and **creatures/NPCs**.
- **Verbs** — actions the player can type (`take`, `go north`, `give bone to dog`). The engine
  ships with a big set of standard verbs you switch on; you can add your own.
- **Rules** — what happens when the player does something: puzzles, win conditions, lose
  conditions, monsters, timers.

The golden rule of this engine: **you wire everything together with C# variables, never with text
names.** You make a room, keep it in a variable, and connect other things to *that variable*. There
are no "magic strings" to misspell.

```csharp
var kitchen = b.Room("Kitchen");
var hall    = b.Room("Hall");
hall.North(kitchen);   // you pass the actual room, not the word "kitchen"
```

---

## 2. Set up the project

You need the .NET 10 SDK. The simplest path is to copy the sample app and rename it, but here is the
from-scratch version.

Create a new desktop app that references the engine and the terminal:

```
dotnet new classlib -n MyGame        # or a console/winexe app; see below
```

Your game project needs references to two engine projects (add them to your `.csproj`):

```xml
<ItemGroup>
  <ProjectReference Include="..\Faerie\Faerie.csproj" />
  <ProjectReference Include="..\Faerie.Terminal.Avalonia\Faerie.Terminal.Avalonia.csproj" />
</ItemGroup>
<ItemGroup>
  <PackageReference Include="Avalonia" Version="11.3.0" />
  <PackageReference Include="Avalonia.Desktop" Version="11.3.0" />
  <PackageReference Include="Avalonia.Themes.Fluent" Version="11.3.0" />
</ItemGroup>
```

The easiest thing is to **copy the three files** from `Faerie.Samples.HauntedHouse`
(`Program.cs`, `App.cs`, and the `.csproj`) into your own project and just replace
`HauntedHouseGame.Build()` with your own `MyGame.Build()`. `Program.cs` and `App.cs` are pure
boilerplate that opens the window and starts the engine — you will almost never touch them.

The only file you actually write is your game definition. Everything below goes in one method:

```csharp
using Faerie.Building;
using Faerie.Model;
using Faerie.Presentation;
using Faerie.Runtime;
using Faerie.Verbs;

public static class MyGame
{
    public static Game Build()
    {
        var b = GameBuilder.Create("My First Adventure")
            .AddStandardVerbs();      // turn on all the built-in commands

        // ... everything in this guide goes here ...

        b.StartIn(/* a room */);
        return b.Build();
    }
}
```

`b` is your **builder**. You will call methods on it to make rooms and things, then wire them up.

---

## 3. Rooms

`b.Room("Name")` creates a room and gives you back a handle. Keep it in a variable. Use `.Describe`
for what the player reads when they look around.

```csharp
var clearing = b.Room("Forest Clearing")
    .Describe("Sunlight filters through tall pines. A path leads north into the gloom.");

var cabin = b.Room("Hunter's Cabin")
    .Describe("A one-room cabin. Dust covers a small table.");
```

### Connecting rooms (exits)

Use the direction helpers. **Exits are two-way by default** — connecting the clearing north to the
cabin automatically lets the player go south from the cabin back to the clearing.

```csharp
clearing.North(cabin);          // clearing -> N -> cabin, and cabin -> S -> clearing
clearing.East(river);
cabin.Up(loft);
```

Available directions: `North South East West Up Down In` (plus the diagonals if you use
`Connect(Direction.NorthEast, ...)`). To make a **one-way** exit, pass `reciprocal: false`:

```csharp
ledge.Down(pit, reciprocal: false);   // you can fall down, but not climb back up
```

### Where the player starts

```csharp
b.StartIn(clearing);
```

### Dark rooms

A dark room shows nothing until the player brings a light source (see Items) or ambient light applies.

```csharp
var cave = b.Room("Dark Cave").Describe("Black as pitch.").Dark();
```

For lighting that depends on puzzle state (an open grating, a window, etc.), use `LitWhen` with a
`GameContext` lambda. It composes with `.Dark()` and with carried light sources:

```csharp
var gratingRoom = b.Room("Grating Room").Describe("...").Dark()
    .LitWhen(ctx => ctx.Get(gratingOpen));
```

---

## 4. Things: items, scenery, and containers

### Items (things you can pick up)

```csharp
var key = b.Item("brass key")
    .Describe("A small, tarnished brass key.")
    .Adjectives("brass", "small");      // so "take small key" works too
cabin.Contains(key);                    // the key starts in the cabin
```

`Contains` sets the starting location. You can also write it from the item's side:
`key.StartsIn(cabin)`.

### Synonyms and adjectives (so the parser understands the player)

Every thing already answers to its name. Add more words with `.Called(...)` and `.Adjectives(...)`:

```csharp
var lantern = b.Item("brass lantern")
    .Called("lamp", "light")            // "take lamp" works
    .Adjectives("brass", "old");        // "examine old lantern" works
```

### Scenery (things you can examine but not take)

```csharp
var table = b.Scenery("table")
    .Describe("A rough wooden table, scarred by knives.");
cabin.Contains(table);
```

### Containers and supporters

```csharp
var chest = b.Scenery("chest").Describe("An iron-bound chest.").Container(open: false);
chest.Contains... // no: put things inside with StartsInside
var coin = b.Item("gold coin").StartsInside(chest);

var shelf = b.Scenery("shelf").Supporter();
var book  = b.Item("dusty book").StartsOn(shelf);
```

The player must `open chest` before they can see or take the coin. A `Container(open: true)` starts
open.

### Light sources

```csharp
var lantern = b.Item("lantern").Describe("An oil lantern.").LightSource(lit: false);
```

The player types `light lantern` (or `turn on lantern`) to light it. Carry a lit light source into a
dark room to see. (Want it to require matches? See "Reacting to actions".)

### Locked doors

A door is just a scenery thing placed in a room, attached to an exit. `LockedWith` takes the key
**object** (not its name):

```csharp
var key  = b.Item("iron key");
var door = b.Scenery("oak door").Called("door").LockedWith(key);   // locked, needs `key`
cabin.Contains(door);
```

To make that door actually block an exit, attach it to the exit (this is the one slightly advanced
bit; copy it as-is):

```csharp
var exit = cabin.Connect(Direction.North, forest);  // returns the exit
exit.Door = door;                                    // now you must open the door to go north
```

The player unlocks it with `unlock door with key` (while carrying the key), then `open door`.

### Readable things

```csharp
var note = b.Item("note").Readable("It says: 'The treasure lies beneath the third floorboard.'");
```

---

## 5. Creatures and NPCs

A creature is made with `b.Creature(...)`. It's a thing flagged as alive, so the engine words
messages correctly and won't let the player "take" it.

```csharp
var dog = b.Creature("guard dog")
    .Called("hound", "mutt")
    .Describe("A huge dog watches you, hackles raised.");
forest.Contains(dog);
```

To make the NPC *do* something — react to being given an item, talked to, attacked — use reactions,
covered next.

---

## 6. Reacting to actions (the heart of puzzles)

Most of your game's personality comes from intercepting verbs. The pattern is always:

```csharp
b.On(thing).Before(b.Verbs.SomeVerb!, ctx =>
{
    // do something
    return VerbResult.Done;   // Done = "I handled it, stop here"
                              // Pass = "let the normal behaviour happen"
});
```

`b.Verbs` holds a handle to every standard verb (`b.Verbs.Take`, `b.Verbs.Examine`, `b.Verbs.Give`,
`b.Verbs.Open`, ...). The `!` just tells C# "yes, this exists" (it does, once you've called
`AddStandardVerbs`).

`ctx` is your toolbox inside a reaction. The most useful things on it:

- `ctx.Say("text")` — print a line to the player (supports colour markup, see §11).
  `ctx.SayInline("text")` prints without a trailing newline.
- `ctx.Carrying(thing)` / `ctx.Wearing(thing)` / `ctx.Here(thing)` — true/false checks.
  (`Here` = the thing is in the current room **or** being carried.)
- `ctx.InRoom(room)` — is the player in this room right now?
- `ctx.RoomOf(thing)` — the room a thing is currently in, or `null` if it's carried, worn, or off-stage.
  This is how a reaction or daemon asks "where is X" for anything, not just the current room (see
  "Querying the world" below).
- `ctx.Take(thing)`, `ctx.PlaceHere(thing)`, `ctx.Remove(thing)` — move things around.
- `ctx.MovePlayerTo(room)` — teleport the player.
- `ctx.Win("text")` / `ctx.Lose("text")` — end the game (see §8).
- `ctx.Random` — a shared `System.Random` for chance-based outcomes (`ctx.Random.Next(6)`).
- `ctx.DirectObject`, `ctx.IndirectObject` — what the player referred to ("attack troll **with sword**").

### Example: feed the dog to get past it

```csharp
var steak = b.Item("juicy steak");
var dog   = b.Creature("guard dog");

// "give steak to dog"
b.On(dog).Before(b.Verbs.Give!, ctx =>
{
    if (ctx.DirectObject == steak)
    {
        ctx.Remove(steak);
        ctx.Say("The dog snatches the steak and trots off, tail wagging. The way north is clear.");
        // (you might also unlock an exit here)
        return VerbResult.Done;
    }
    ctx.Say("The dog isn't interested in that.");
    return VerbResult.Done;
});
```

### Example: the lantern needs matches to light

```csharp
var matches = b.Item("matches");
var lantern = b.Item("lantern").LightSource(lit: false);

b.On(lantern).Before(b.Verbs.SwitchOn!, ctx =>
{
    if (!ctx.Carrying(matches))
    {
        ctx.Say("You have nothing to light it with.");
        return VerbResult.Done;        // stop: it does NOT light
    }
    return VerbResult.Pass;            // let the normal "light it" behaviour run
});
```

### Example: a custom response to examining something

```csharp
var painting = b.Scenery("painting").Describe("A portrait of a stern old man.");
painting.OnExamine = ctx => ctx.Say("His eyes seem to follow you around the room...");
```

(`OnEnter`, `OnFirstEnter`, `OnTurn` exist on rooms the same way — they're properties you assign with
`=`, e.g. `room.OnEnter = ctx => { ... };`.)

### Querying the world (where things are)

Reactions and daemons often need to know more than "is it here?". The context gives you spatial queries
that work for *any* room or thing, not just the player's current location:

- `ctx.RoomOf(thing)` — the room a thing sits in, or `null` if it's carried/worn/off-stage.
- `ctx.InRoom(room)` — is the player here now?
- `ctx.CurrentRoom.Exits` — the exits leading out of a room; `.Values` are the `Exit`s, and each
  `exit.Destination` is the room it leads to. This is how you reason about which rooms are adjacent.
- `ctx.State.ContentsOf(room)` — every thing currently loose in a given room.

For example, "is a living troll in this room or an adjacent one?" (the basis of the sample's sword-glow
and combat features):

```csharp
bool TrollNear(GameContext ctx)
{
    if (trollDefeated.Get(ctx)) return false;
    Room here = ctx.CurrentRoom;
    if (ctx.RoomOf(troll) == here) return true;                       // same room
    return here.Exits.Values.Any(e => ctx.RoomOf(troll) == e.Destination);  // one room away
}
```

Because these queries are available inside `EveryTurn` daemons too, "creature proximity," "wander toward
the player," and similar behaviours are ordinary game code — no special engine support required.

---

## 7. Custom verbs and nouns

If the standard verbs aren't enough, define your own. A verb has: an id, the words that trigger it,
the grammatical forms it accepts, and a handler.

```csharp
// A "pray" verb that takes no object:
var pray = b.DefineVerb("pray", ["pray", "kneel"], VerbForms.Intransitive, ctx =>
{
    ctx.Say("You bow your head. A strange calm settles over you.");
    return VerbResult.Done;
});

// An "unlock X with Y" style verb that takes two objects:
var bribe = b.DefineVerb("bribe", ["bribe"], VerbForms.Ditransitive | VerbForms.Transitive, ctx =>
{
    if (ctx.DirectObject is { } who && ctx.IndirectObject is { } what)
        ctx.Say($"You try to bribe {who.Name} with {what.Name}...");
    else
        ctx.Say("Bribe whom, with what?");
    return VerbResult.Done;
});
```

`VerbForms` values (combine with `|`):

- `Intransitive` — no object: `pray`
- `Transitive` — one object: `bribe guard`
- `Ditransitive` — two objects joined by a preposition: `bribe guard with coin`

The parser already understands `to with in into on onto at from using under behind about` as the
preposition between two objects. "Nouns" aren't declared separately — any room/thing's `.Called` and
`.Adjectives` words are the nouns.

---

## 8. Win and lose conditions

Ending the game is one method call from inside any reaction, room hook, or daemon:

```csharp
ctx.Win("You step into the sunlight, free at last. You win!");
ctx.Lose("The floor gives way and you tumble into darkness. You have died.");
```

### Win when the player reaches a place with the treasure

```csharp
var treasure = b.Item("golden idol");
var exit     = b.Room("Cave Mouth");

exit.OnEnter = ctx =>
{
    if (ctx.Carrying(treasure))
        ctx.Win("You escape the cave clutching the idol. Fortune is yours!");
    else
        ctx.Say("You could leave now... but you came here for treasure.");
};
```

### Win when the player performs an action

```csharp
var lever = b.Scenery("lever").Called("lever");
b.On(lever).Before(b.Verbs.Use!, ctx =>
{
    ctx.Win("With a grinding roar, the great door opens and you walk free.");
    return VerbResult.Done;
});
```

### Lose by stepping somewhere deadly

```csharp
var pit = b.Room("Bottomless Pit");
pit.OnEnter = ctx => ctx.Lose("You fall, and fall, and fall...");
```

After the game ends the player can still type `RESTORE` or `QUIT` (those keep working); everything
else gets a gentle "the game is over" nudge.

---

## 9. Tracking state: flags, counters, timers

### Flags and counters (survive saving the game)

Make a typed "state key" with a name and a default value, then read/write it through `ctx`:

```csharp
var spokenToWizard = b.State("spoke-to-wizard", false);
var coinsCollected = b.State("coins", 0);

// later, inside a reaction:
ctx.Set(spokenToWizard, true);
if (ctx.Get(spokenToWizard)) { /* ... */ }

ctx.Set(coinsCollected, ctx.Get(coinsCollected) + 1);
```

Keep these as `b.State(...)` (global) so they're written into save files automatically.

### Score

```csharp
b.WithMaxScore(100);             // enables "Score: x/100" in the default title bar
// inside a reaction:
ctx.State.Score += 25;
```

### Timers and recurring events (daemons)

Run something every turn:

```csharp
b.EveryTurn(ctx =>
{
    if (ctx.CurrentRoom == swamp && ctx.Random.Next(4) == 0)
        ctx.Say("Something splashes in the murky water nearby.");
});
```

Run something once, at a specific turn. **Note `AtTurn` takes an absolute turn number, not a delay** — turn 20
of the whole game, not "20 turns from now". For a relative delay, capture the current turn or count down with a
state key (as in the bomb example below):

```csharp
b.AtTurn(20, ctx => ctx.Say("A distant bell tolls. Time is running out."));
```

The optional `when:` predicate on `EveryTurn` gates a daemon so it only runs while a condition holds — handy for
starting a timer or behaviour at a certain point and skipping the per-turn work otherwise:

A classic "the bomb goes off" timer:

```csharp
var turnsLeft = b.State("fuse", 5);
b.EveryTurn(ctx =>
{
    int left = ctx.Get(turnsLeft);
    if (left <= 0) { ctx.Lose("BOOM. The bomb explodes."); return; }
    ctx.Say($"The fuse hisses. ({left} turns left)");
    ctx.Set(turnsLeft, left - 1);
}, when: ctx => /* only after they light it */ ctx.Get(fuseLit));
```

### Rewriting or hiding output (`FilterOutput`)

Sometimes you want to change *how the game's text appears* rather than what happens — an echoing room, a
poisoned/drunk haze, a censored word, mirror writing. Register an output filter; it runs on every line of game
text just before it's shown, and returns a rewritten string (or `null` to hide the line completely):

```csharp
// While the player is in the loud room, throw the last word of every line back at them.
b.FilterOutput((ctx, text) =>
    ctx.InRoom(loudRoom) ? $"{text}\n(...{text.Split(' ').Last()}...)" : text);

// Hide anything mentioning the wizard until the player has met him.
b.FilterOutput((ctx, text) =>
    !ctx.Get(metWizard) && text.Contains("wizard") ? null : text);
```

Notes:

- Filters run in the order you register them; each sees the previous one's result, and the chain stops as soon
  as one returns `null` (suppressed).
- Use the context (`ctx.InRoom`, state keys, etc.) to scope the effect — there's no separate "per-room" API; the
  predicate *is* the scope.
- The title bar and status bar are **not** filtered, and a filter is bypassed for any text it prints itself, so
  it can't loop. Keep filters to pure string work (don't call `ctx.Say` from inside one).

---

## 10. The window: title bar, status bar, native title, icon

### In-game title bar and status bar (the strips at top and bottom)

```csharp
b.WithDefaultTitleBar();    // "My Game" on the left, score/turns on the right

b.WithStatusBar(ctx => new BarContent
{
    Left  = $" {ctx.CurrentRoom.Name}",
    Right = "F11 fullscreen ",
    Style = new TextStyle(TerminalColor.White, TerminalColor.Blue)   // any colours you like
});
```

You can fully style your own title bar with `WithTitleBar(...)` the same way. Leave the status bar
unset and there simply won't be one.

### Native OS window title and icon

```csharp
b.WithWindowTitle("My First Adventure");
b.WithWindowIcon("avares://MyGame/Assets/icon.ico");   // or a file path
```

For the icon to load from `avares://`, put the file in your game project and mark it as an
`<AvaloniaResource>` in the `.csproj` (the sample already does this for its `Assets/Fonts` folder —
copy that pattern for an `Assets` folder). If the icon can't be found, the app just uses the default.

---

## 11. Coloured and styled text

Anywhere you write text for the player (`Describe`, `Say`, intro, bars), you can use markup:

```
ctx.Say("You found the {fg:gold}{bold}idol{/}{/}! {blink}Run!{/}");
```

- `{fg:red}` / `{bg:blue}` set colours. Names include black, blue, green, cyan, red, magenta, brown,
  gray, yellow, white, amber, gold, and the "light" variants — or any `#RRGGBB` value.
- `{bold}`, `{underline}`, `{blink}`, `{inverse}` turn on attributes.
- `{/}` closes the most recent one. `{{` and `}}` are literal braces.

---

## 12. The intro and starting

```csharp
b.WithIntro("{fg:white}{bold}MY FIRST ADVENTURE{/}{/}\n\nA tale of mild peril.\n\nType HELP for commands.");
b.OnStart(ctx => { /* optional: runs once after the first room is shown */ });
b.StartIn(clearing);
return b.Build();
```

---

## 13. What the 