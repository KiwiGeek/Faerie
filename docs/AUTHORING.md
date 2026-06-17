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

### Re-entry descriptions

By default, the full `.Describe(...)` text is shown the first time the player enters a room and
whenever they `LOOK`. On later visits, the engine shows the optional `.Brief(...)` text instead (or
falls back to the full description if you never set a brief).

```csharp
var hall = b.Room("Entrance Hall")
    .Describe("A grand, ruined hall. Dust hangs in the air.")
    .Brief("You are back in the entrance hall.");
```

### Short titles (Sierra-style banners)

Hi-Res / Sierra adventures use a one-line place name in the per-turn room banner, separate from the
long prose. Set it with `.ShortTitle(...)`; when omitted, the room's display name is used.

```csharp
var bar = b.Room("The Bar")
    .ShortTitle("BAR")
    .Describe("You are in a dim, smoky bar...");
```

See [Sierra-style room banners](#sierra-style-room-banners) below.

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

### Orderable stock (offstage catalog items)

When a shopkeeper sells items that are not physically on the floor yet, mark the stock with
`OrderableFrom(vendor)`. The item stays offstage, but the parser can still resolve its name while
the vendor is in the current room (for example `buy wine`). It does **not** appear in Sierra room
banners until it has been spawned.

```csharp
var waitress = b.Creature("waitress").StartsIn(disco);
var wine = b.Item("wine").OrderableFrom(waitress);
```

Wire your `buy` verb to check that the item is still offstage before placing it in the room.

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
- `ctx.RoomOf(thing)` — the room a thing ultimately belongs to, or `null` if it is off-stage.
  Carried, worn, and nested items resolve to the player's current room (see "Querying the world" below).
- `ctx.ThingsHere()` / `ctx.ThingsIn(room)` — enumerate things in a room (see "Querying the world").
- `ctx.IsAdjacent(room)` / `ctx.Nearby(thing)` — proximity checks for daemons and reactions.
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

(`OnEnter`, `OnFirstEnter`, `OnTurn`, and `OnAfterDrop` exist on rooms the same way — they're properties you assign with
`=`, e.g. `room.OnEnter = ctx => { ... };`.)

Thing hooks `OnExamine`, `OnFirstExamine`, `OnTake`, and `OnDrop` work the same way. Return `true` from
`OnTake` or `OnDrop` to fully handle the action and skip the default verb behaviour:

```csharp
egg.OnDrop = ctx =>
{
    if (!ctx.InRoom(treeRoom)) return false;
    ctx.Say("The egg shatters on the ground.");
    ctx.Remove(egg);
    ctx.PlaceHere(brokenCanary);
    return true;   // skip the usual "Dropped." placement
};

cliff.OnAfterDrop = (ctx, thing) =>
    ctx.Say($"{thing.Name} falls out of sight.");
```

### Querying the world (where things are)

Reactions and daemons often need to know more than "is it here?". `GameContext` provides spatial
queries that work for *any* room or thing, not just the player's current location.

#### Single-thing checks

- `ctx.RoomOf(thing)` — the room a thing ultimately sits in, or `null` if it is off-stage.
  The engine walks the containment chain: a key inside a chest in the hall resolves to the hall; a
  coin inside a bag in your inventory resolves to your **current** room while you are carrying the bag.
- `ctx.InRoom(room)` — is the player in this room right now?
- `ctx.Here(thing)` — is the thing in the current room **or** being carried/worn? (Parser scope only;
  does not include adjacent rooms.)
- `ctx.Nearby(thing)` — is the thing in the current room **or** one exit away? Carried and worn
  things count as being in the current room. Useful for proximity daemons (sword glow, smell, sound).

#### Adjacency

- `ctx.IsAdjacent(room)` — is there an exit from the **current** room leading directly to
  `room`? Replaces hand-rolling `ctx.CurrentRoom.Exits.Values.Any(e => e.Destination == room)`.

#### Enumerating things in a room

There are three useful “what’s in this room?” queries:

| API | What it returns |
| --- | --- |
| `ctx.ThingsIn(room)` | Loose floor items only (same as `ctx.State.ContentsOf(room)`) |
| `ctx.ThingsLocatedIn(room)` | Everything physically in the room — floor, containers, creatures — **excluding** the player's inventory and worn items |
| `ctx.ThingsIn(room, includePresent: true)` | Everything whose `RoomOf` is this room, **including** what the player is carrying while standing here |

The single-thing predicate `ctx.LocatedIn(thing, room)` matches the middle row: true when the thing is
in the room but not in your pockets. Use it when listing visible room contents (Sierra banners, custom
`EveryTurn` hooks). Do **not** use `RoomOf(thing) == room` alone — `RoomOf` maps carried items to the
current room for parser scope, so inventory would be counted by mistake.

```csharp
ctx.ThingsHere();                             // loose floor items only
ctx.ThingsLocatedHere();                      // in-room contents, not your inventory
ctx.ThingsHere(includePresent: true);         // loose + containers + your inventory
ctx.ThingsIn(kitchen, includePresent: true);  // same for any room
```

With `includePresent: true`, the query follows `RoomOf` and therefore includes:

- loose items on the floor;
- items inside containers that are in the room (open or closed);
- items you are carrying or wearing **while you are in that room**;
- items nested inside carried containers (a coin inside a bag in your pack).

Carried items only count for the room you are **currently** in. Pick up a lamp in the hall and walk
south — the lamp is no longer returned by `ThingsIn(hall, includePresent: true)`.

#### Examples

Creature proximity (the basis of the Zork sample's sword-glow feature):

```csharp
int ThreatLevel(GameContext ctx, Thing villain, bool defeated)
{
    if (defeated) return 0;
    Room? room = ctx.RoomOf(villain);
    if (room is null) return 0;
    if (room == ctx.CurrentRoom) return 2;          // same room — bright glow
    return ctx.IsAdjacent(room) ? 1 : 0;            // one room away — faint glow
}
```

Or simply `ctx.Nearby(villain)` when you only need a yes/no "in this room or next door?" check.

Scanning a room for a hidden treasure (including inside containers, but not your pockets unless
you are standing there):

```csharp
bool PlatinumBarInRoom(GameContext ctx, Room room) =>
    ctx.ThingsIn(room, includePresent: true).Any(t => t == platinumBar);
```

Because these queries are available inside `EveryTurn` daemons too, "creature proximity," "wander
toward the player," and similar behaviours are ordinary game code — no special engine support required.

Lower-level access remains on `ctx.State` (`ContentsOf`, `Inventory`, `PlacementOf`, …) when you need
finer control than these helpers provide.

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

**Overlapping verb words:** `AddCoreVerbs()` registers standard verbs first. If you `DefineVerb` with
words that collide (e.g. Zork's custom `move` shares `move`/`push` with core `push`), register your
verb **after** the standard set — the parser prefers the last-registered transitive match for object
commands. Movement still disambiguates on direction (`move north` → Go when Go claims `move`).

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
of the whole game, not "20 turns from now". For a relative delay from game start or from an in-game event, use
`ScheduleIn`:

```csharp
b.AtTurn(20, ctx => ctx.Say("A distant bell tolls. Time is running out."));
```

**Relative one-shot timers** — fire after *N player turns*. At build time, `ScheduleIn(N)` counts from turn zero
before the first command. From running game code, `ctx.ScheduleIn` counts from the *next* turn boundary (the
scheduling command's turn does not count toward *N*):

```csharp
b.ScheduleIn(5, ctx => ctx.Say("The fuse explodes."));
b.ScheduleIn("fuse", 5, ctx => ctx.Say("Boom."), when: ctx => ctx.Get(fuseLit));
// ctx.CancelSchedule("fuse") drops a named timer before it fires
```

The optional `when:` predicate gates a timer so it only fires while a condition holds — same as `EveryTurn`.

A classic "the bomb goes off" timer (manual countdown still works; `ScheduleIn` is simpler once lit):

```csharp
b.On(match).Before(b.Verbs.Light!, ctx =>
{
    ctx.ScheduleIn(5, c => c.Lose("BOOM. The bomb explodes."));
    return VerbResult.Pass;
});
```

Or with a recurring fuse hiss each turn until detonation, keep `EveryTurn` + state; for a silent fuse,
`ScheduleIn` alone is enough.

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

### Room presentation hooks

By default, Faerie describes rooms Infocom-style: bold room name, prose, “You can see …”, and
“Exits: …” on enter and look. Hi-Res / Sierra games use a different layout and timing. Rather than
hard-coding styles in the engine, games install optional **room presentation hooks**:

| Hook | When the engine calls it |
| --- | --- |
| `DescribeRoom` | First enter, re-enter, `LOOK`, and when a light source makes a dark room visible |
| `RefreshRoomDisplay` | After game start and at the end of each turn |

```csharp
b.WithRoomPresentation(new RoomPresentation
{
    DescribeRoom = ctx =>
    {
        if (ctx.Moment == RoomDescribeMoment.ReEnter) return;
        ctx.Out.PrintLine(ctx.Room.ResolveDescription(ctx.Context));
    },
    RefreshRoomDisplay = ctx => ctx.Say($"--- {ctx.CurrentRoom.Name} ---"),
    InputPrompt = "> "
});
```

`RoomDescribeMoment` values: `FirstEnter`, `ReEnter`, `Look`, `LightingChanged`. The engine decides
**when**; your hook decides **what** to print.

When no presentation is installed, built-in Infocom behaviour is used. To redraw manually from game
code, call `ctx.RefreshRoomDisplay()` (alias: `ctx.PrintRoomBanner()`).

#### Sierra preset

`WithSierraRoomBanner()` is sugar for `RoomPresentations.Sierra(...)`:

```csharp
var b = GameBuilder.Create("My Hi-Res Game")
    .AddStandardVerbs()
    .WithRoomBannerSeparatorWidth(40)          // optional; 0 = terminal width
    .WithSierraRoomBanner();                  // default prompt: "What shall I do? "
```

Or explicitly:

```csharp
b.WithRoomPresentation(RoomPresentations.Sierra(new SierraRoomPresentationOptions
{
    Prompt = "What shall I do? ",
    SeparatorWidth = 40
}));
```

The Sierra preset:

1. **Long prose once** — full room description on first visit and on `LOOK` (or “It is pitch dark…”
   when the room is unlit and you have no light).
2. **Compact banner every turn** — short title, comma-separated items, comma-separated exit
   destinations, then a row of `=` characters.
3. **Prompt** — copied to the game for the Avalonia host.

Set a banner title per room with `.ShortTitle(...)` (see [Short titles](#short-titles-sierra-style-banners)).
Exit lines use each destination's short title. Blocked exits (closed doors, failed conditions) are
omitted from **Other areas**.

When the room is lit, the banner lists visible non-scenery things physically in the room (via
`ThingsLocatedIn` / scope — not your inventory). In the dark, only the short title and separator
print until a lit light source is present.

Re-entry to an already-visited room skips the long prose; the banner still redraws every turn.

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