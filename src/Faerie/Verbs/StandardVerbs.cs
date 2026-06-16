using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;
using static Faerie.Verbs.VerbText;

namespace Faerie.Verbs;

/// <summary>
/// The built-in verb behaviours. The engine ships as "an empty box": none of these are active
/// until the game definition asks for them via <c>AddMovement</c>, <c>AddCoreVerbs</c>,
/// <c>AddMetaVerbs</c> or the omnibus <c>AddStandardVerbs</c>.
/// </summary>
public static class StandardVerbs
{
    public static void InstallMovement(GameBuilder b)
    {
        b.Verbs.Go = b.DefineVerb(StandardVerbIds.Go, ["go", "walk", "move", "run", "head"],
            VerbForms.Transitive | VerbForms.Intransitive, Move);
    }

    public static void InstallCore(GameBuilder b)
    {
        b.Verbs.Look = b.DefineVerb(StandardVerbIds.Look, ["look", "l"],
            VerbForms.Intransitive | VerbForms.Transitive, Look);

        b.Verbs.Examine = b.DefineVerb(StandardVerbIds.Examine, ["examine", "x", "inspect", "look at", "study"],
            VerbForms.Transitive, Examine);

        b.Verbs.Search = b.DefineVerb(StandardVerbIds.Search, ["search", "look in", "look inside", "look under", "look behind"],
            VerbForms.Transitive, Search);

        b.Verbs.Inventory = b.DefineVerb(StandardVerbIds.Inventory, ["inventory", "i", "inv"],
            VerbForms.Intransitive, Inventory);

        b.Verbs.Take = b.DefineVerb(StandardVerbIds.Take, ["take", "get", "grab", "pick up"],
            VerbForms.Transitive, Take);

        b.Verbs.Drop = b.DefineVerb(StandardVerbIds.Drop, ["drop", "put down", "discard"],
            VerbForms.Transitive, Drop);

        // "move"/"push" an object. Note "move" is also a movement word (see InstallMovement); the
        // parser routes "move <direction>" to Go and "move <object>" here.
        b.Verbs.Push = b.DefineVerb(StandardVerbIds.Push, ["push", "move", "shove", "pull", "drag", "slide"],
            VerbForms.Transitive, Push);

        b.Verbs.Open = b.DefineVerb(StandardVerbIds.Open, ["open"], VerbForms.Transitive, Open);
        b.Verbs.Close = b.DefineVerb(StandardVerbIds.Close, ["close", "shut"], VerbForms.Transitive, Close);

        b.Verbs.Unlock = b.DefineVerb(StandardVerbIds.Unlock, ["unlock"],
            VerbForms.Transitive | VerbForms.Ditransitive, Unlock);
        b.Verbs.Lock = b.DefineVerb(StandardVerbIds.Lock, ["lock"],
            VerbForms.Transitive | VerbForms.Ditransitive, Lock);

        b.Verbs.Put = b.DefineVerb(StandardVerbIds.Put, ["put", "place", "insert"],
            VerbForms.Ditransitive | VerbForms.Transitive, Put);

        b.Verbs.Read = b.DefineVerb(StandardVerbIds.Read, ["read"], VerbForms.Transitive, Read);

        b.Verbs.Wear = b.DefineVerb(StandardVerbIds.Wear, ["wear", "don", "put on"], VerbForms.Transitive, Wear);
        b.Verbs.TakeOff = b.DefineVerb(StandardVerbIds.Remove, ["take off", "doff", "remove"], VerbForms.Transitive, TakeOff);

        b.Verbs.Eat = b.DefineVerb(StandardVerbIds.Eat, ["eat", "consume"], VerbForms.Transitive, Eat);
        b.Verbs.Drink = b.DefineVerb(StandardVerbIds.Drink, ["drink", "sip", "swig"], VerbForms.Transitive, Drink);

        b.Verbs.SwitchOn = b.DefineVerb(StandardVerbIds.PushButton, ["switch on", "turn on", "light", "ignite"],
            VerbForms.Transitive, SwitchOn);
        b.Verbs.SwitchOff = b.DefineVerb(StandardVerbIds.SwitchOff, ["switch off", "turn off", "extinguish", "douse"],
            VerbForms.Transitive, SwitchOff);

        b.Verbs.Give = b.DefineVerb(StandardVerbIds.Give, ["give", "offer", "hand", "feed"],
            VerbForms.Ditransitive | VerbForms.Transitive, Give);

        b.Verbs.Use = b.DefineVerb(StandardVerbIds.Use, ["use", "apply", "operate"],
            VerbForms.Transitive | VerbForms.Ditransitive, Use);

        b.Verbs.Wait = b.DefineVerb(StandardVerbIds.Wait, ["wait", "z"], VerbForms.Intransitive, Wait);
    }

    public static void InstallMeta(GameBuilder b)
    {
        b.Verbs.Help = b.DefineVerb(StandardVerbIds.Help, ["help", "?", "commands", "verbs"],
            VerbForms.Intransitive, (ctx) => { ctx.Engine.ShowHelp(); return VerbResult.Done; });

        b.Verbs.Score = b.DefineVerb(StandardVerbIds.Score, ["score"],
            VerbForms.Intransitive, Score);

        b.Verbs.Quit = b.DefineVerb(StandardVerbIds.Quit, ["quit", "exit game", "q"],
            VerbForms.Intransitive, (ctx) => { ctx.Engine.RequestQuit(); return VerbResult.Done; });

        b.Verbs.Save = b.DefineVerb(StandardVerbIds.Save, ["save"],
            VerbForms.Intransitive | VerbForms.Transitive, Save);

        b.Verbs.Restore = b.DefineVerb(StandardVerbIds.Restore, ["restore", "load"],
            VerbForms.Intransitive | VerbForms.Transitive, Restore);
    }

    // ---- handlers -------------------------------------------------------------------------

    private static VerbResult Move(VerbContext ctx)
    {
        if (ctx.Direction is not { } dir)
        {
            ctx.Say("Go where?");
            return VerbResult.Done;
        }

        Exit? exit = ctx.CurrentRoom.ExitTo(dir);
        if (exit is null)
        {
            ctx.Say("You can't go that way.");
            return VerbResult.Done;
        }

        if (!exit.CanPass(ctx, out string? reason))
        {
            ctx.Say(reason ?? "You can't go that way.");
            return VerbResult.Done;
        }

        ctx.MovePlayerTo(exit.Destination);
        return VerbResult.Done;
    }

    private static VerbResult Look(VerbContext ctx)
    {
        if (ctx.DirectObject is not null) return Examine(ctx);
        ctx.Engine.DescribeCurrentRoom(verbose: true);
        return VerbResult.Done;
    }

    private static VerbResult Examine(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing)
        {
            ctx.Say("Examine what?");
            return VerbResult.Done;
        }

        ctx.State.LastReferencedThing = thing;

        if (thing.OnFirstExamine is { } first)
        {
            thing.OnFirstExamine = null;
            first(ctx);
        }
        thing.OnExamine?.Invoke(ctx);

        string description = thing.ResolveDescription(ctx);
        ctx.Say(description);

        if (thing.Has(Attr.LightSource))
            ctx.Say(thing.Has(Attr.Lit) ? $"{Cap(The(thing))} {Is(thing)} lit." : $"{Cap(The(thing))} {Is(thing)} not lit.");

        if (thing.Has(Attr.Openable) && !thing.Has(Attr.Open))
        {
            if (!description.Contains("closed", StringComparison.OrdinalIgnoreCase))
                ctx.Say($"{Cap(The(thing))} {Is(thing)} closed.");
        }
        else if (thing.Has(Attr.Container) && thing.Has(Attr.Open))
            ListContents(ctx, thing);

        if (thing.Has(Attr.Supporter))
            ListOnTop(ctx, thing);

        return VerbResult.Done;
    }

    private static VerbResult Search(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing)
        {
            ctx.Say("Search what?");
            return VerbResult.Done;
        }

        ctx.State.LastReferencedThing = thing;

        // Reveal any concealed contents.
        bool revealed = false;
        foreach (Thing inner in ctx.State.ContentsOf(thing).Concat(ctx.State.ContentsOf(thing, onTop: true)))
        {
            if (inner.Has(Attr.Concealed))
            {
                inner.Set(Attr.Concealed, false);
                revealed = true;
            }
        }

        if (thing.Has(Attr.Container) && !thing.Has(Attr.Open) && thing.Has(Attr.Openable))
        {
            ctx.Say($"{Cap(The(thing))} {Is(thing)} closed.");
            return VerbResult.Done;
        }

        if (thing.Has(Attr.Container)) ListContents(ctx, thing);
        else if (thing.Has(Attr.Supporter)) ListOnTop(ctx, thing);
        else if (!revealed) ctx.Say($"You find nothing of interest.");

        return VerbResult.Done;
    }

    private static VerbResult Inventory(VerbContext ctx)
    {
        List<Thing> carried = ctx.State.Inventory.ToList();
        List<Thing> worn = ctx.State.Worn.ToList();

        if (carried.Count == 0 && worn.Count == 0)
        {
            ctx.Say("You are empty-handed.");
            return VerbResult.Done;
        }

        if (carried.Count > 0)
        {
            ctx.Say("You are carrying:");
            foreach (Thing t in carried) ctx.Say($"  {A(t)}");
        }

        if (worn.Count > 0)
        {
            ctx.Say("You are wearing:");
            foreach (Thing t in worn) ctx.Say($"  {A(t)}");
        }

        return VerbResult.Done;
    }

    private static VerbResult Take(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing)
        {
            ctx.Say("Take what?");
            return VerbResult.Done;
        }

        if (ctx.Carrying(thing))
        {
            ctx.Say($"You already have {It(thing)}.");
            return VerbResult.Done;
        }

        if (thing.Has(Attr.Animate))
        {
            ctx.Say("That wouldn't go down well.");
            return VerbResult.Done;
        }

        if (thing.OnTake is { } hook && hook(ctx)) return VerbResult.Done;

        if (!thing.Has(Attr.Takeable) || thing.Has(Attr.Fixed))
        {
            ctx.Say("That's not something you can carry.");
            return VerbResult.Done;
        }

        ctx.Take(thing);
        ctx.State.LastReferencedThing = thing;
        ctx.Say("Taken.");
        return VerbResult.Done;
    }

    private static VerbResult Drop(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing)
        {
            ctx.Say("Drop what?");
            return VerbResult.Done;
        }

        if (!ctx.Carrying(thing))
        {
            ctx.Say($"You aren't carrying {It(thing)}.");
            return VerbResult.Done;
        }

        if (ctx.Wearing(thing))
        {
            ctx.Say($"You'll have to take {It(thing)} off first.");
            return VerbResult.Done;
        }

        if (thing.OnDrop is { } onDrop && onDrop(ctx))
        {
            ctx.State.LastReferencedThing = thing;
            return VerbResult.Done;
        }

        Room room = ctx.CurrentRoom;
        ctx.PlaceHere(thing);
        ctx.State.LastReferencedThing = thing;
        ctx.Say("Dropped.");
        room.OnAfterDrop?.Invoke(ctx, thing);
        return VerbResult.Done;
    }

    private static VerbResult Open(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing) { ctx.Say("Open what?"); return VerbResult.Done; }

        if (!thing.Has(Attr.Openable)) { ctx.Say($"You can't open {The(thing)}."); return VerbResult.Done; }
        if (thing.Has(Attr.Locked)) { ctx.Say($"{Cap(The(thing))} {Is(thing)} locked."); return VerbResult.Done; }
        if (thing.Has(Attr.Open)) { ctx.Say($"{Cap(The(thing))} {Is(thing)} already open."); return VerbResult.Done; }

        thing.Set(Attr.Open);
        ctx.State.LastReferencedThing = thing;
        ctx.Say($"You open {The(thing)}.");
        if (thing.Has(Attr.Container)) ListContents(ctx, thing);
        return VerbResult.Done;
    }

    private static VerbResult Close(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing) { ctx.Say("Close what?"); return VerbResult.Done; }
        if (!thing.Has(Attr.Openable)) { ctx.Say($"You can't close {The(thing)}."); return VerbResult.Done; }
        if (!thing.Has(Attr.Open)) { ctx.Say($"{Cap(The(thing))} {Is(thing)} already closed."); return VerbResult.Done; }

        thing.Set(Attr.Open, false);
        ctx.State.LastReferencedThing = thing;
        ctx.Say($"You close {The(thing)}.");
        return VerbResult.Done;
    }

    private static VerbResult Unlock(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing) { ctx.Say("Unlock what?"); return VerbResult.Done; }
        if (!thing.Has(Attr.Lockable)) { ctx.Say($"You can't unlock {The(thing)}."); return VerbResult.Done; }
        if (!thing.Has(Attr.Locked)) { ctx.Say($"{Cap(The(thing))} {Is(thing)} already unlocked."); return VerbResult.Done; }

        Thing? key = ctx.IndirectObject ?? thing.Key;
        if (thing.Key is not null && key != thing.Key)
        {
            ctx.Say($"That doesn't fit {The(thing)}.");
            return VerbResult.Done;
        }
        if (key is null || !ctx.Carrying(key))
        {
            ctx.Say("You don't have the right key.");
            return VerbResult.Done;
        }

        thing.Set(Attr.Locked, false);
        ctx.State.LastReferencedThing = thing;
        ctx.Say($"You unlock {The(thing)} with {The(key)}.");
        return VerbResult.Done;
    }

    private static VerbResult Lock(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing) { ctx.Say("Lock what?"); return VerbResult.Done; }
        if (!thing.Has(Attr.Lockable)) { ctx.Say($"You can't lock {The(thing)}."); return VerbResult.Done; }
        if (thing.Has(Attr.Locked)) { ctx.Say($"{Cap(The(thing))} {Is(thing)} already locked."); return VerbResult.Done; }

        Thing? key = ctx.IndirectObject ?? thing.Key;
        if (key is null || !ctx.Carrying(key)) { ctx.Say("You don't have the right key."); return VerbResult.Done; }
        if (thing.Has(Attr.Open)) { ctx.Say($"You close {The(thing)} first."); thing.Set(Attr.Open, false); }

        thing.Set(Attr.Locked);
        ctx.Say($"You lock {The(thing)}.");
        return VerbResult.Done;
    }

    private static VerbResult Push(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing) { ctx.Say("Move what?"); return VerbResult.Done; }
        ctx.State.LastReferencedThing = thing;
        // Generic by design: a game gives this meaning via a reaction (e.g. move rug -> reveal trapdoor).
        ctx.Say($"You shove {The(thing)}, but nothing useful happens.");
        return VerbResult.Done;
    }

    private static VerbResult Put(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing) { ctx.Say("Put what?"); return VerbResult.Done; }

        if (ctx.IndirectObject is not { } target)
        {
            // "put down X" behaves like drop.
            return Drop(ctx);
        }

        if (!ctx.Carrying(thing)) { ctx.Say($"You need to be holding {The(thing)} first."); return VerbResult.Done; }
        if (thing == target) { ctx.Say("That's not going to work."); return VerbResult.Done; }

        bool onto = ctx.Preposition is "on" or "onto";

        if (onto)
        {
            if (!target.Has(Attr.Supporter)) { ctx.Say($"You can't put anything on {The(target)}."); return VerbResult.Done; }
            ctx.Move(thing, Placement.On(target));
            ctx.Say($"You put {The(thing)} on {The(target)}.");
            return VerbResult.Done;
        }

        if (!target.Has(Attr.Container)) { ctx.Say($"{Cap(The(target))} can't contain things."); return VerbResult.Done; }
        if (target.Has(Attr.Openable) && !target.Has(Attr.Open)) { ctx.Say($"{Cap(The(target))} {Is(target)} closed."); return VerbResult.Done; }

        ctx.Move(thing, Placement.Inside(target));
        ctx.Say($"You put {The(thing)} in {The(target)}.");
        return VerbResult.Done;
    }

    private static VerbResult Read(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing) { ctx.Say("Read what?"); return VerbResult.Done; }
        ctx.State.LastReferencedThing = thing;

        if (thing.Has(Attr.Readable) && thing.ReadableText is { } text) ctx.Say(text);
        else ctx.Say($"There's nothing to read on {The(thing)}.");
        return VerbResult.Done;
    }

    private static VerbResult Wear(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing) { ctx.Say("Wear what?"); return VerbResult.Done; }
        if (!thing.Has(Attr.Wearable)) { ctx.Say("You can't wear that."); return VerbResult.Done; }
        if (ctx.Wearing(thing)) { ctx.Say($"You're already wearing {It(thing)}."); return VerbResult.Done; }

        ctx.Move(thing, Placement.Worn);
        thing.Set(Attr.Worn);
        ctx.Say($"You put on {The(thing)}.");
        return VerbResult.Done;
    }

    private static VerbResult TakeOff(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing) { ctx.Say("Take off what?"); return VerbResult.Done; }
        if (!ctx.Wearing(thing)) { ctx.Say($"You aren't wearing {It(thing)}."); return VerbResult.Done; }

        thing.Set(Attr.Worn, false);
        ctx.Take(thing);
        ctx.Say($"You take off {The(thing)}.");
        return VerbResult.Done;
    }

    private static VerbResult Eat(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing) { ctx.Say("Eat what?"); return VerbResult.Done; }
        if (!thing.Has(Attr.Edible)) { ctx.Say("That's not edible."); return VerbResult.Done; }

        ctx.Remove(thing);
        ctx.Say($"You eat {The(thing)}. {(ctx.Random.Next(2) == 0 ? "Not bad." : "It does the job.")}");
        return VerbResult.Done;
    }

    private static VerbResult Drink(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing) { ctx.Say("Drink what?"); return VerbResult.Done; }
        if (!thing.Has(Attr.Drinkable)) { ctx.Say("You can't drink that."); return VerbResult.Done; }

        ctx.Say($"You drink {The(thing)}.");
        return VerbResult.Done;
    }

    private static VerbResult SwitchOn(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing) { ctx.Say("Switch on what?"); return VerbResult.Done; }

        if (thing.Has(Attr.LightSource))
        {
            if (thing.Has(Attr.Lit)) { ctx.Say($"{Cap(The(thing))} {Is(thing)} already lit."); return VerbResult.Done; }
            bool wasDark = !new Parsing.Scope(ctx.State, ctx).IsCurrentRoomLit;
            thing.Set(Attr.Lit);
            thing.Set(Attr.On);
            ctx.Say($"You light {The(thing)}. A warm glow pushes back the dark.");
            if (wasDark) ctx.Engine.DescribeCurrentRoom(verbose: true);
            return VerbResult.Done;
        }

        if (thing.Has(Attr.Switchable))
        {
            if (thing.Has(Attr.On)) { ctx.Say($"{Cap(The(thing))} {Is(thing)} already on."); return VerbResult.Done; }
            thing.Set(Attr.On);
            ctx.Say($"You switch on {The(thing)}.");
            return VerbResult.Done;
        }

        ctx.Say("That isn't something you can switch on.");
        return VerbResult.Done;
    }

    private static VerbResult SwitchOff(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } thing) { ctx.Say("Switch off what?"); return VerbResult.Done; }

        if (thing.Has(Attr.LightSource) && thing.Has(Attr.Lit))
        {
            thing.Set(Attr.Lit, false);
            thing.Set(Attr.On, false);
            ctx.Say($"You extinguish {The(thing)}.");
            if (!new Parsing.Scope(ctx.State, ctx).IsCurrentRoomLit)
                ctx.Say("Darkness closes in around you.");
            return VerbResult.Done;
        }

        if (thing.Has(Attr.Switchable) && thing.Has(Attr.On))
        {
            thing.Set(Attr.On, false);
            ctx.Say($"You switch off {The(thing)}.");
            return VerbResult.Done;
        }

        ctx.Say($"{Cap(The(thing))} {Is(thing)} already off.");
        return VerbResult.Done;
    }

    private static VerbResult Give(VerbContext ctx)
    {
        if (ctx.DirectObject is not { } gift) { ctx.Say("Give what?"); return VerbResult.Done; }
        if (!ctx.Carrying(gift)) { ctx.Say($"You aren't holding {The(gift)}."); return VerbResult.Done; }

        if (ctx.IndirectObject is not { } recipient)
        {
            ctx.Say("Give it to whom?");
            return VerbResult.Done;
        }

        if (!recipient.Has(Attr.Animate))
        {
            ctx.Say($"{Cap(The(recipient))} can't take {The(gift)}.");
            return VerbResult.Done;
        }

        // Default behaviour: the recipient is unmoved. Games override this with a reaction.
        ctx.Say($"{Cap(The(recipient))} takes no interest in {The(gift)}.");
        return VerbResult.Done;
    }

    private static VerbResult Use(VerbContext ctx)
    {
        // "use" is deliberately generic; games give it meaning through reactions. The default is a nudge.
        if (ctx.DirectObject is null) { ctx.Say("Use what?"); return VerbResult.Done; }
        ctx.Say("Nothing obvious happens. Try a more specific command.");
        return VerbResult.Done;
    }

    private static VerbResult Wait(VerbContext ctx)
    {
        ctx.Say("Time passes...");
        return VerbResult.Done;
    }

    private static VerbResult Score(VerbContext ctx)
    {
        ctx.Say(ctx.Engine.MaxScore > 0
            ? $"You have scored {ctx.State.Score} out of a possible {ctx.Engine.MaxScore}, in {ctx.State.TurnCount} turns."
            : $"You have taken {ctx.State.TurnCount} turns.");
        return VerbResult.Done;
    }

    private static VerbResult Save(VerbContext ctx)
    {
        ctx.Engine.RequestSave(ctx.DirectObjectText);
        return VerbResult.Done;
    }

    private static VerbResult Restore(VerbContext ctx)
    {
        ctx.Engine.RequestRestore(ctx.DirectObjectText);
        return VerbResult.Done;
    }

    // ---- listing helpers ------------------------------------------------------------------

    private static void ListContents(GameContext ctx, Thing container)
    {
        List<Thing> contents = ctx.State.ContentsOf(container).Where(t => !t.Has(Attr.Concealed)).ToList();
        if (contents.Count == 0) { ctx.Say($"{Cap(The(container))} {Is(container)} empty."); return; }

        ctx.Say($"{Cap(The(container))} contains:");
        foreach (Thing t in contents) ctx.Say($"  {A(t)}");
    }

    private static void ListOnTop(GameContext ctx, Thing supporter)
    {
        List<Thing> on = ctx.State.ContentsOf(supporter, onTop: true).Where(t => !t.Has(Attr.Concealed)).ToList();
        if (on.Count == 0) return;
        ctx.Say($"On {The(supporter)} you see:");
        foreach (Thing t in on) ctx.Say($"  {A(t)}");
    }
}
