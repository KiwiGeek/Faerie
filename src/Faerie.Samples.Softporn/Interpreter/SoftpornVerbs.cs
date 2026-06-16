namespace Faerie.Samples.Softporn.Interpreter;

public static class SoftpornVerbs
{
    public static void Dispatch(SoftpornContext ctx)
    {
        switch (ctx.State.Verb)
        {
            case Verbs.Go: Go(ctx); break;
            case Verbs.Hail: Hail(ctx); break;
            case Verbs.Take: Take(ctx); break;
            case Verbs.Drop: Drop(ctx); break;
            case Verbs.Look: Look(ctx); break;
            case Verbs.Flush: Flush(ctx); break;
            case Verbs.Open: OpenVerb(ctx); break;
            case Verbs.Inflate: Inflate(ctx); break;
            case Verbs.Play: Play(ctx); break;
            case Verbs.Press: Press(ctx); break;
            case Verbs.Enter: Enter(ctx); break;
            case Verbs.Eat: Eat(ctx); break;
            case Verbs.Drink: Drink(ctx); break;
            case Verbs.Buy: Buy(ctx); break;
            case Verbs.Climb: Climb(ctx); break;
            case Verbs.Water: Water(ctx); break;
            case Verbs.Fill: Fill(ctx); break;
            case Verbs.Pour: Pour(ctx); break;
            case Verbs.Listen: Listen(ctx); break;
            case Verbs.Close: CloseVerb(ctx); break;
            case Verbs.Jump: Jump(ctx); break;
            case Verbs.Marry: Marry(ctx); break;
            case Verbs.Fuck: Fuck(ctx); break;
            case Verbs.Wear: Wear(ctx); break;
            case Verbs.Answer: Answer(ctx); break;
            case Verbs.Call: Call(ctx); break;
            case Verbs.Break: Break(ctx); break;
            case Verbs.Cut: Cut(ctx); break;
            case Verbs.Dance: Dance(ctx); break;
            case Verbs.Kill: Kill(ctx); break;
            case Verbs.Pay: Pay(ctx); break;
            case Verbs.Smoke: Smoke(ctx); break;
            case Verbs.Show: Show(ctx); break;
            case Verbs.Smell: Smell(ctx); break;
            case Verbs.Help: SoftpornHelpers.GiveHelp(ctx); break;
            case Verbs.Kiss: Kiss(ctx); break;
            case Verbs.Stab: SoftpornHelpers.StabSomeone(ctx); break;
            case Verbs.Quit: ctx.State.GameEnded = true; break;
            case Verbs.ShowScore: ShowScore(ctx); break;
            case Verbs.Save: Save(ctx); break;
            case Verbs.Restore: Restore(ctx); break;
            default: SoftpornHelpers.ICantDoThat(ctx); break;
        }
    }

    static void Go(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (s.TiedToBed)
            ctx.Messages.WriteMessage("But I'm tied to the bed!!!!!");
        else if (!ctx.NoDirection)
        {
            if (s.YourPlace == Places.BBedrm && s.Direction == Directions.North && !s.HookerFucked)
                ctx.Messages.WriteMessage("The Hooker says: 'Don't go there ... do me first!!'");
            else if (s.YourPlace == Places.CHallwy && s.Direction == Directions.South && !s.MarriedToGirl)
                ctx.Messages.WriteMessage("The door is locked shut!");
            else if (s.YourPlace == Places.DEntrnc && s.Direction == Directions.West && !s.DoorWOpen)
                ctx.Messages.WriteMessage("The door is closed!");
            else if (s.YourPlace == Places.BBackrm && s.Direction == Directions.Up && s.TvChannel != 6)
            {
                if (s.Money < 20 || !SoftpornHelpers.IsCarried(ctx, Objects.Wallet))
                    ctx.Messages.WriteMessage("The Pimp says I can't until I get $2000");
                else if (s.HookerFucked)
                    ctx.Messages.WriteMessage("The Pimp says 'No -- the hooker can't take it anymore!'");
                else
                {
                    ctx.Messages.WriteMessage("The Pimp takes $2000 and says OK");
                    s.Money -= 20;
                    s.YourPlace = Places.BBedrm;
                }
            }
            else if (s.YourPlace == Places.BBalcny && s.Direction == Directions.West && !s.RopeInUse)
                SoftpornHelpers.FallingDown(ctx);
            else
            {
                s.NewPlace = s.Path[(int)s.YourPlace, (int)s.Direction];
                if (s.NewPlace != Places.Nowhere)
                    s.YourPlace = s.NewPlace;
                else
                    SoftpornHelpers.ICantGoThatWay(ctx);
            }
        }
        else
            SoftpornHelpers.ICantDoThat(ctx);
    }

    static void Hail(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (s.Noun != Objects.Taxi)
            ctx.Messages.WriteMessage("Who are you kidding? You're pulling at straws, fool!!");
        else if (s.YourPlace is not (Places.BStreet or Places.CStreet or Places.DStreet))
            ctx.Messages.WriteMessage("I'm not in the street, fool!!");
        else
        {
            ctx.Messages.WriteLongMessage(36);
            ctx.TaxiDestination = ctx.Io.ReadLine("");
            ctx.TaxiDestination = ctx.TaxiDestination.TrimStart();
            ctx.TaxiDestination = ctx.TaxiDestination.ToUpperInvariant();
            ctx.TaxiDestination = (ctx.TaxiDestination + " ")[..4];
            Places newPlace;
            if (ctx.TaxiDestination == "DISC")
                newPlace = Places.DStreet;
            else if (ctx.TaxiDestination == "CASI")
                newPlace = Places.CStreet;
            else if (ctx.TaxiDestination == "BAR ")
                newPlace = Places.BStreet;
            else
                newPlace = Places.Nowhere;

            if (newPlace is Places.Nowhere || newPlace == s.YourPlace)
                ctx.Messages.WriteMessage("Huh? - Hail another!");
            else if (SoftpornHelpers.IsCarried(ctx, Objects.Wine))
            {
                SoftpornHelpers.WineInTaxi(ctx);
                s.YourPlace = newPlace;
                s.ObjectPlace[(int)Objects.Wine] = Places.Nowhere;
                s.ObjectsCarried--;
            }
            else
            {
                ctx.Messages.WriteMessage("We arrive and I get out.");
                s.YourPlace = newPlace;
            }
        }
    }

    static void Take(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (s.Noun == Objects.Inventory)
        {
            ctx.AnythingCarried = false;
            for (var noun = (int)SoftpornConstants.FirstObject; noun <= (int)SoftpornConstants.LastObject - 1; noun++)
            {
                if (!SoftpornHelpers.IsCarried(ctx, (Objects)noun))
                    continue;
                if (!ctx.AnythingCarried)
                    ctx.Messages.WriteMessage("I'm carrying the following:");
                ctx.AnythingCarried = true;
                ctx.Out.Write(SoftpornConstants.ObjectName[noun]);
                if ((Objects)noun == Objects.Wallet && s.Money > 0)
                    ctx.Out.WriteLine($" with ${s.Money}00");
                else
                    ctx.Out.WriteLine();
            }
            if (!ctx.AnythingCarried)
                ctx.Messages.WriteMessage("I'm not carrying anything!!");
        }
        else if (s.Noun == Objects.Off)
            ctx.Messages.WriteMessage("You're not a bird, fool!!");
        else if (s.Noun == Objects.All)
        {
            ctx.Messages.WriteMessage("You hog!!!");
            Thread.Sleep(300);
            ctx.Out.WriteLine();
            for (var noun = (int)SoftpornConstants.FirstObject; noun <= (int)SoftpornConstants.LastObject - 1; noun++)
            {
                if (!SoftpornHelpers.IsHere(ctx, (Objects)noun))
                    continue;
                ctx.Out.Write($"{SoftpornConstants.ObjectName[noun]}: ");
                if (s.ObjectsCarried >= SoftpornConstants.MaxCarried)
                    ctx.Out.WriteLine("I'm carrying too much!!!");
                else if (SoftpornConstants.TakeableObjects.Contains((Objects)noun))
                {
                    if (s.YourPlace == Places.DPhrmcy && (Objects)noun is Objects.Magazine or Objects.Rubber)
                    {
                        ctx.Out.WriteLine("The man says 'Shoplifter!!' and shoots me");
                        SoftpornHelpers.Purgatory(ctx);
                        return;
                    }
                    s.ObjectPlace[noun] = Places.YouHaveIt;
                    s.ObjectsCarried++;
                    if ((Objects)noun == Objects.Water)
                        s.PitcherFull = true;
                    if ((Objects)noun == Objects.Pitcher && s.PitcherFull)
                        s.ObjectPlace[(int)Objects.Water] = Places.YouHaveIt;
                    ctx.Out.WriteLine("Taken");
                }
                else
                    SoftpornHelpers.CantDoThat(ctx);
            }
        }
        else if (SoftpornHelpers.IsCarried(ctx, s.Noun))
            SoftpornHelpers.IAlreadyHaveIt(ctx);
        else if (!SoftpornHelpers.IsHere(ctx, s.Noun))
            SoftpornHelpers.FindMeOne(ctx);
        else if (s.ObjectsCarried >= SoftpornConstants.MaxCarried)
            ctx.Messages.WriteMessage("I'm carrying too much!!!");
        else if (!SoftpornConstants.TakeableObjects.Contains(s.Noun))
            SoftpornHelpers.ICantDoThat(ctx);
        else if (s.YourPlace == Places.DPhrmcy && s.Noun is Objects.Magazine or Objects.Rubber)
        {
            ctx.Messages.WriteMessage("The man says 'Shoplifter!!' and shoots me");
            SoftpornHelpers.Purgatory(ctx);
        }
        else if (s.Noun == Objects.Water && !SoftpornHelpers.IsCarried(ctx, Objects.Pitcher))
            ctx.Messages.WriteMessage("Get me the pitcher so I don't spill it!");
        else if (s.Noun == Objects.Candy && s.YourPlace == Places.BBedrm && !s.HookerFucked)
            ctx.Messages.WriteMessage("The Hooker says: 'Don't take it ... do me first!!'");
        else if (s.Noun == Objects.Rope && s.RopeInUse)
            ctx.Messages.WriteMessage("It is tied to the balcony");
        else
        {
            SoftpornHelpers.Ok(ctx);
            s.ObjectPlace[(int)s.Noun] = Places.YouHaveIt;
            s.ObjectsCarried++;
            if (s.Noun == Objects.Water)
                s.PitcherFull = true;
            if (s.Noun == Objects.Pitcher && s.PitcherFull)
                s.ObjectPlace[(int)Objects.Water] = Places.YouHaveIt;
        }
    }

    static void Drop(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (s.Noun is Objects.Inventory or Objects.Taxi or Objects.On or Objects.Off)
            SoftpornHelpers.Huh(ctx);
        else if (s.Noun == Objects.All)
        {
            ctx.AnythingCarried = false;
            ctx.Out.WriteLine();
            for (var noun = (int)SoftpornConstants.FirstObject; noun <= (int)SoftpornConstants.LastObject - 1; noun++)
            {
                if (!SoftpornHelpers.IsCarried(ctx, (Objects)noun))
                    continue;
                ctx.AnythingCarried = true;
                ctx.Out.WriteLine($"{SoftpornConstants.ObjectName[noun]}: Dropped");
                s.ObjectPlace[noun] = s.YourPlace;
                s.ObjectsCarried--;
            }
            if (!ctx.AnythingCarried)
                ctx.Out.WriteLine("I did't carry anything!!");
        }
        else if (!SoftpornHelpers.IsCarried(ctx, s.Noun))
            SoftpornHelpers.IDontHaveIt(ctx);
        else
        {
            s.ObjectPlace[(int)s.Noun] = s.YourPlace;
            s.ObjectsCarried--;
            if (s.Noun == Objects.Pitcher && s.PitcherFull)
                s.ObjectPlace[(int)Objects.Water] = s.YourPlace;
            else if (s.Noun == Objects.Rubber)
                s.RubberWorn = false;
            else if (s.YourPlace == Places.DDisco && SoftpornHelpers.IsHere(ctx, Objects.Girl) &&
                     s.Noun is Objects.Candy or Objects.Flowers or Objects.Ring)
            {
                switch (s.Noun)
                {
                    case Objects.Candy:
                        ctx.Messages.WriteMessage("She smiles and eats a couple!!");
                        s.CandyGiven = true;
                        break;
                    case Objects.Flowers:
                        ctx.Messages.WriteMessage("She blushes profusely and puts them in her hair!");
                        s.FlowersGiven = true;
                        s.ObjectPlace[(int)Objects.Flowers] = Places.Nowhere;
                        break;
                    case Objects.Ring:
                        ctx.Messages.WriteMessage("She blushes and puts it in her purse.");
                        s.RingGiven = true;
                        s.ObjectPlace[(int)Objects.Ring] = Places.Nowhere;
                        break;
                }
                if (s.CandyGiven && s.FlowersGiven && s.RingGiven)
                {
                    ctx.Messages.WriteMessage("She says: 'See you at the Marriage Center!!'");
                    s.ObjectPlace[(int)Objects.Girl] = Places.CMarryc;
                }
            }
            else if (SoftpornHelpers.IsHere(ctx, Objects.Bum) && s.Noun == Objects.Wine)
            {
                if (s.ObjectPlace[(int)Objects.Knife] == Places.Nowhere)
                {
                    SoftpornHelpers.BumTellsStory(ctx);
                    s.ObjectPlace[(int)Objects.Knife] = s.YourPlace;
                }
                else
                    ctx.Messages.WriteMessage("The bum mutters 'That stuff made me puke!! Get out of here!!!'");
            }
            else if (SoftpornHelpers.IsHere(ctx, Objects.Businessman) && s.Noun == Objects.Whiskey &&
                     s.ObjectPlace[(int)Objects.ControlUnit] == Places.Nowhere)
            {
                ctx.Messages.WriteMessage("The guy gives me a TV controller!!");
                s.ObjectPlace[(int)Objects.ControlUnit] = s.YourPlace;
            }
            else if (SoftpornHelpers.IsHere(ctx, Objects.Blonde) && s.Noun == Objects.Pills)
            {
                ctx.Messages.WriteLongMessage(57);
                s.ObjectPlace[(int)Objects.Blonde] = Places.Nowhere;
                s.ObjectPlace[(int)Objects.Pills] = Places.Nowhere;
            }
            else if (s.Noun == Objects.Apple && s.YourPlace == Places.PJacuzi && SoftpornHelpers.IsHere(ctx, Objects.Girl))
            {
                ctx.Messages.WriteLongMessage(50);
                s.AppleGiven = true;
            }
            else
                SoftpornHelpers.Ok(ctx);
        }
    }

    static void Look(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (ctx.NoObject)
            ctx.Messages.WriteLongMessage((int)s.YourPlace + 1);
        else if (s.Noun == Objects.All)
            ctx.Messages.WriteMessage("That's too much, one item at a time, please!!");
        else if (s.Noun is Objects.Inventory or Objects.On or Objects.Off)
            SoftpornHelpers.Huh(ctx);
        else if (!SoftpornHelpers.IsHere(ctx, s.Noun) && !SoftpornHelpers.IsCarried(ctx, s.Noun))
            SoftpornHelpers.FindMeOne(ctx);
        else
        {
            switch (s.Noun)
            {
                case Objects.Desk:
                    if (s.DrawerOpen)
                        SoftpornHelpers.ISeeSomething(ctx, Objects.Newspaper, "");
                    else
                        ctx.Messages.WriteMessage("It's drawer is shut");
                    break;
                case Objects.Washbasin:
                    SoftpornHelpers.ISeeSomething(ctx, Objects.Ring, "Dead cockroaches...");
                    break;
                case Objects.Graffiti:
                    SoftpornHelpers.LookGraffiti(ctx);
                    break;
                case Objects.Mirror:
                    ctx.Messages.WriteMessage("There's a pervert looking back at me!!");
                    break;
                case Objects.Toilet:
                    ctx.Messages.WriteMessage("Hasn't been flushed in ages! Stinks!!!");
                    break;
                case Objects.Businessman:
                    ctx.Messages.WriteMessage("He looks like a whiskey drinker to me!!");
                    break;
                case Objects.Button:
                    ctx.Messages.WriteMessage("Says Push.");
                    break;
                case Objects.Bartender:
                    ctx.Messages.WriteMessage("He's waiting for me to buy something!");
                    break;
                case Objects.Pimp:
                    ctx.Messages.WriteMessage("He's wearing a button proclaiming -- Support your local Pimp, gimme $2000!!!");
                    break;
                case Objects.Hooker:
                    ctx.Messages.WriteLongMessage(31);
                    break;
                case Objects.Billboard:
                    ctx.Messages.WriteLongMessage(63);
                    break;
                case Objects.TV:
                    if (!SoftpornHelpers.IsCarried(ctx, Objects.ControlUnit))
                        ctx.Messages.WriteMessage("To watch TV, I need the remote control unit!!");
                    else if (!s.HookerFucked)
                        ctx.Messages.WriteMessage("The Pimp says I can't watch TV");
                    else
                        SoftpornHelpers.WatchTv(ctx);
                    break;
                case Objects.SlotMachines:
                    ctx.Messages.WriteMessage("Playing them might be more fun....");
                    break;
                case Objects.Ashtray:
                    SoftpornHelpers.ISeeSomething(ctx, Objects.Passcard, "");
                    break;
                case Objects.Blonde:
                    ctx.Messages.WriteLongMessage(40);
                    break;
                case Objects.Bum:
                    ctx.Messages.WriteMessage("He grumbles -- I'll tell you a story for a bottle of wine.....");
                    break;
                case Objects.Peephole:
                    if (s.HolePeeped)
                        ctx.Messages.WriteMessage("All windows at the hotel across the road have their curtains shut.");
                    else
                    {
                        ctx.Messages.WriteLongMessage(55);
                        s.HolePeeped = true;
                    }
                    break;
                case Objects.DoorWest:
                    if (s.DoorWOpen)
                        ctx.Messages.WriteMessage("The door is open");
                    else
                    {
                        ctx.Messages.WriteMessage("The sign on the door says ");
                        ctx.Out.WriteLine("'Entry by showing passcard - Club members and their guests only!'");
                    }
                    break;
                case Objects.Waitress:
                    ctx.Messages.WriteMessage("She ignores you!");
                    break;
                case Objects.Telephone:
                    if (s.YourPlace == Places.DTelbth)
                        ctx.Messages.WriteMessage("A number is there - Call 555-6969 for a good time!");
                    else
                        SoftpornHelpers.ISeeNothingSpecial(ctx);
                    break;
                case Objects.Closet:
                    if (s.ClosetOpen)
                        SoftpornHelpers.ISeeSomething(ctx, Objects.Doll, "It's open");
                    else
                        ctx.Messages.WriteMessage("It's closed");
                    break;
                case Objects.Sink:
                    ctx.Messages.WriteMessage("The sign over the sink says 'Water on or off to operate'");
                    break;
                case Objects.Elevator:
                    ctx.Messages.WriteMessage("It's doors are closed");
                    break;
                case Objects.Dealer:
                    ctx.Messages.WriteMessage("He's waiting for me to play");
                    break;
                case Objects.Cabinet:
                    if (s.StoolClimbed)
                    {
                        if (s.CabinetOpen)
                            SoftpornHelpers.ISeeSomething(ctx, Objects.Pitcher, "It's open");
                        else
                            ctx.Messages.WriteMessage("It's closed");
                    }
                    else
                        SoftpornHelpers.ISeeNothingSpecial(ctx);
                    break;
                case Objects.Bushes:
                    ctx.Messages.WriteMessage("Entering them would be kinky!!!!");
                    break;
                case Objects.Tree:
                    SoftpornHelpers.ISeeSomething(ctx, Objects.Apple, "");
                    break;
                case Objects.Sign:
                    ctx.Messages.WriteMessage("It says 'Hail taxi here'");
                    break;
                case Objects.Girl:
                    if (s.YourPlace == Places.PJacuzi)
                        ctx.Messages.WriteLongMessage(35);
                    else if (s.YourPlace is Places.DDisco or Places.CMarryc)
                        ctx.Messages.WriteLongMessage(34);
                    else
                        ctx.Messages.WriteMessage("She slaps me and yells 'Pervert!!!!!'");
                    break;
                case Objects.Newspaper:
                    if (SoftpornHelpers.IsCarried(ctx, Objects.Newspaper))
                        ctx.Messages.WriteLongMessage(32);
                    else
                        SoftpornHelpers.IDontHaveIt(ctx);
                    break;
                case Objects.Garbage:
                    SoftpornHelpers.ISeeSomething(ctx, Objects.AppleCore, "");
                    break;
                case Objects.Flowers:
                    ctx.Messages.WriteMessage("They look beautiful!!!");
                    break;
                case Objects.AppleCore:
                    SoftpornHelpers.ISeeSomething(ctx, Objects.Seeds, "");
                    break;
                case Objects.Pills:
                    ctx.Messages.WriteMessage("The label on the bottle says");
                    ctx.Out.WriteLine("'Want to drive someone crazy with lust?? Try this!!!!'");
                    break;
                case Objects.Plant:
                    if (s.ObjectPlace[(int)Objects.Bushes] == Places.Nowhere)
                    {
                        ctx.Messages.WriteMessage("There's a group of bushes behind it!!");
                        s.ObjectPlace[(int)Objects.Bushes] = s.YourPlace;
                    }
                    else
                        SoftpornHelpers.ISeeNothingSpecial(ctx);
                    break;
                case Objects.Radio:
                    ctx.Messages.WriteMessage("Maybe I should listen...");
                    break;
                case Objects.Magazine:
                    if (SoftpornHelpers.IsCarried(ctx, Objects.Magazine))
                        ctx.Messages.WriteLongMessage(33);
                    else
                        SoftpornHelpers.IDontHaveIt(ctx);
                    break;
                case Objects.Rubber:
                    if (SoftpornHelpers.IsCarried(ctx, Objects.Rubber))
                        ctx.Messages.WriteMessage($"It's {s.RubberColor}, {s.RubberFlavor}-flavored, {s.RubberLubricated}, and {s.RubberRibbed}");
                    else
                        SoftpornHelpers.IDontHaveIt(ctx);
                    break;
                case Objects.Wallet:
                    if (s.Money > 0)
                    {
                        ctx.Out.WriteLine();
                        ctx.Out.WriteLine($"It contains ${s.Money}00.");
                    }
                    else
                        ctx.Messages.WriteMessage("It's empty");
                    break;
                case Objects.Doll:
                    if (s.DollInflated)
                        ctx.Messages.WriteMessage("It's inflated");
                    else
                        ctx.Messages.WriteMessage("It's rolled up in a little ball");
                    break;
                case Objects.Pitcher:
                    if (s.PitcherFull)
                        ctx.Messages.WriteMessage("It's full of water");
                    else
                        ctx.Messages.WriteMessage("It's empty");
                    break;
                case Objects.Rack:
                    SoftpornHelpers.ISeeSomething(ctx, Objects.Magazine, "");
                    break;
                case Objects.Curtain:
                    ctx.Messages.WriteMessage("It's on the east wall");
                    break;
                default:
                    SoftpornHelpers.ISeeNothingSpecial(ctx);
                    break;
            }
        }
    }

    static void Flush(SoftpornContext ctx)
    {
        if (!SoftpornHelpers.IsHere(ctx, ctx.State.Noun))
            SoftpornHelpers.FindMeOne(ctx);
        else if (ctx.State.Noun == Objects.Toilet)
        {
            ctx.Messages.WriteLongMessage(69);
            Thread.Sleep(300);
            ctx.Messages.WriteMessage("I'm dead from the germs!!");
            SoftpornHelpers.Purgatory(ctx);
        }
        else
            SoftpornHelpers.ICantDoThat(ctx);
    }

    static void OpenVerb(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (!SoftpornHelpers.IsHere(ctx, s.Noun))
            SoftpornHelpers.FindMeOne(ctx);
        else
        {
            switch (s.Noun)
            {
                case Objects.Window:
                    ctx.Messages.WriteMessage("Won't budge");
                    break;
                case Objects.Desk:
                    SoftpornHelpers.Open(ctx, () => s.DrawerOpen, v => s.DrawerOpen = v);
                    break;
                case Objects.DoorWest:
                    if (s.DoorWOpen)
                        ctx.Messages.WriteMessage("It's already open!!");
                    else
                    {
                        ctx.Messages.WriteMessage("A voice asks 'Passcard?' I search in my pockets and...");
                        if (SoftpornHelpers.IsCarried(ctx, Objects.Passcard))
                        {
                            ctx.Out.WriteLine("I have it! The door opens!");
                            s.DoorWOpen = true;
                            s.Path[(int)Places.DEntrnc, (int)Directions.West] = Places.DDisco;
                        }
                        else
                            ctx.Out.WriteLine("I don't have it!!");
                    }
                    break;
                case Objects.Curtain:
                    ctx.Messages.WriteMessage("It seems to be remotely controlled");
                    break;
                case Objects.Elevator:
                    ctx.Messages.WriteMessage("Push the button to open elevator");
                    break;
                case Objects.Closet:
                    SoftpornHelpers.Open(ctx, () => s.ClosetOpen, v => s.ClosetOpen = v);
                    break;
                case Objects.Cabinet:
                    if (s.StoolClimbed)
                        SoftpornHelpers.Open(ctx, () => s.CabinetOpen, v => s.CabinetOpen = v);
                    else
                        ctx.Messages.WriteMessage("I can't reach it!!");
                    break;
                default:
                    SoftpornHelpers.ICantDoThat(ctx);
                    break;
            }
        }
    }

    static void Inflate(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (s.Noun == Objects.Doll)
        {
            if (SoftpornHelpers.IsCarried(ctx, Objects.Doll))
            {
                if (s.DollInflated)
                    ctx.Messages.WriteMessage("You've already inflated it, stupid!!");
                else
                {
                    SoftpornHelpers.Ok(ctx);
                    s.DollInflated = true;
                }
            }
            else if (SoftpornHelpers.IsHere(ctx, Objects.Doll))
                ctx.Messages.WriteMessage("I can't unless I'm holding it close");
            else
                SoftpornHelpers.FindMeOne(ctx);
        }
        else
            ctx.Messages.WriteMessage("But the prime rate is already 257%!!");
    }

    static void Play(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (!SoftpornHelpers.IsHere(ctx, s.Noun))
            SoftpornHelpers.FindMeOne(ctx);
        else if (s.Noun == Objects.SlotMachines)
        {
            if (SoftpornHelpers.IsHere(ctx, Objects.SlotMachines))
            {
                if (s.Money > 0 && SoftpornHelpers.IsCarried(ctx, Objects.Wallet))
                    SoftpornHelpers.PlaySlot(ctx);
                else
                    SoftpornHelpers.SorryNoMoney(ctx);
            }
            else
                ctx.Messages.WriteMessage("OK, show me your slot....");
        }
        else if (s.Noun == Objects.Cards)
        {
            if (s.YourPlace == Places.C21Room)
            {
                if (s.Money > 0 && SoftpornHelpers.IsCarried(ctx, Objects.Wallet))
                    SoftpornHelpers.Play21(ctx);
                else
                    SoftpornHelpers.SorryNoMoney(ctx);
            }
            else
                SoftpornHelpers.NotYetButMaybeLater(ctx);
        }
        else
            ctx.Messages.WriteMessage("Playful bugger, eh??");
    }

    static void Press(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (s.Noun == Objects.Button)
        {
            if (s.YourPlace == Places.BBar)
            {
                ctx.Out.WriteLine();
                ctx.Out.Write("A voice says 'What's the password?' (one word) ");
                ctx.Password = ctx.Io.ReadLine("");
                ctx.Password = ctx.Password.ToUpperInvariant();
                if (ctx.Password.StartsWith("BELLYB", StringComparison.Ordinal))
                {
                    ctx.Messages.WriteMessage("The curtain pulls back!!");
                    s.Path[(int)Places.BBar, (int)Directions.East] = Places.BBackrm;
                }
                else
                    ctx.Messages.WriteMessage("Wrong!!");
            }
            else if (s.YourPlace is Places.CHtdesk or Places.PPntfoy)
            {
                if (SoftpornHelpers.IsHere(ctx, Objects.Blonde))
                    ctx.Messages.WriteMessage("The blonde says 'You can't go there!'");
                else
                {
                    ctx.Messages.WriteLongMessage(37);
                    s.YourPlace = s.YourPlace == Places.CHtdesk ? Places.PPntfoy : Places.CHtdesk;
                }
            }
            else
                SoftpornHelpers.NotYetButMaybeLater(ctx);
        }
        else
            ctx.Messages.WriteMessage("Pushy chump, eh???");
    }

    static void Enter(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (!SoftpornHelpers.IsHere(ctx, s.Noun))
            SoftpornHelpers.FindMeOne(ctx);
        else if (s.Noun == Objects.Bushes)
            s.YourPlace = Places.PGarden;
        else if (s.Noun == Objects.Window)
        {
            if (s.WindowBroken)
                s.YourPlace = Places.BInroom;
            else
                SoftpornHelpers.NotYetButMaybeLater(ctx);
        }
        else if (s.Noun == Objects.DoorWest)
        {
            if (s.DoorWOpen)
                s.YourPlace = Places.DDisco;
            else
                ctx.Messages.WriteMessage("The door is closed");
        }
        else if (s.Noun == Objects.Elevator)
            ctx.Messages.WriteMessage("Push the button to enter the elevator");
        else
            SoftpornHelpers.ICantDoThat(ctx);
    }

    static void Eat(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (!SoftpornHelpers.IsHere(ctx, s.Noun) && !SoftpornHelpers.IsCarried(ctx, s.Noun))
            SoftpornHelpers.FindMeOne(ctx);
        else if (s.Noun is Objects.Blonde or Objects.Waitress or Objects.Hooker or Objects.Girl)
        {
            ctx.Messages.WriteLongMessage(38);
            SoftpornHelpers.Purgatory(ctx);
        }
        else if (s.Noun == Objects.Mushroom)
        {
            ctx.Messages.WriteLongMessage(64);
            s.YourPlace = (Places)ctx.Rng.Next(3);
            Thread.Sleep(600);
            ctx.Messages.WriteLongMessage((int)s.YourPlace + 1);
        }
        else if (s.Noun is Objects.Garbage or Objects.AppleCore)
            ctx.Messages.WriteMessage("Too moldy!");
        else if (s.Noun == Objects.Apple)
            ctx.Messages.WriteMessage("Sorry ... not hungry!");
        else if (s.Noun == Objects.Pills)
        {
            ctx.Messages.WriteLongMessage(56);
            SoftpornHelpers.Purgatory(ctx);
        }
        else
            ctx.Messages.WriteMessage("Tastes awful!");
    }

    static void Drink(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (!SoftpornHelpers.IsCarried(ctx, s.Noun))
            SoftpornHelpers.IDontHaveIt(ctx);
        else
        {
            switch (s.Noun)
            {
                case Objects.Whiskey:
                    ctx.Messages.WriteMessage("This stuff is rot-gut! Give it to someone ... I don't want it.");
                    break;
                case Objects.Beer:
                    ctx.Messages.WriteMessage("Heh...heh...hey!!!! This stuff's OK!");
                    break;
                case Objects.Wine:
                    ctx.Messages.WriteMessage("Sour grapes....");
                    break;
                case Objects.Water:
                    ctx.Messages.WriteMessage("Thanks!");
                    break;
                default:
                    ctx.Messages.WriteMessage("Get your head examined!!!!");
                    break;
            }
            if (s.Noun is Objects.Beer or Objects.Water)
            {
                s.ObjectPlace[(int)s.Noun] = Places.Nowhere;
                s.ObjectsCarried--;
            }
        }
    }

    static void Buy(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (s.Money < 1 || !SoftpornHelpers.IsCarried(ctx, Objects.Wallet))
            SoftpornHelpers.SorryNoMoney(ctx);
        else
        {
            switch (s.Noun)
            {
                case Objects.Whiskey or Objects.Beer:
                    if (s.YourPlace != Places.BBar)
                        SoftpornHelpers.NotYetButMaybeLater(ctx);
                    else if (s.ObjectPlace[(int)s.Noun] != Places.Nowhere)
                        ctx.Messages.WriteMessage("Sorry ... all out!");
                    else
                    {
                        ctx.Messages.WriteMessage("I give the bartender $100 and he places it on the bar.");
                        s.Money--;
                        s.ObjectPlace[(int)s.Noun] = s.YourPlace;
                    }
                    break;
                case Objects.Wine:
                    if (s.YourPlace != Places.DDisco)
                        SoftpornHelpers.NotYetButMaybeLater(ctx);
                    else if (s.ObjectPlace[(int)Objects.Wine] != Places.Nowhere)
                        ctx.Messages.WriteMessage("All out!");
                    else
                    {
                        ctx.Messages.WriteMessage("The waitress takes $100 and says she'll return");
                        Thread.Sleep(3000);
                        ctx.Messages.WriteMessage("Poor service!!!");
                        Thread.Sleep(2000);
                        s.Money--;
                        s.ObjectPlace[(int)Objects.Wine] = s.YourPlace;
                    }
                    break;
                case Objects.Rubber or Objects.Magazine:
                    if (s.YourPlace != Places.DPhrmcy)
                        SoftpornHelpers.NotYetButMaybeLater(ctx);
                    else if (SoftpornHelpers.IsHere(ctx, s.Noun))
                    {
                        if (s.Noun == Objects.Rubber)
                            SoftpornHelpers.BuyRubber(ctx);
                        else
                            ctx.Messages.WriteMessage("He takes $100 and gives me the magazine");
                        s.Money--;
                        s.ObjectPlace[(int)s.Noun] = Places.YouHaveIt;
                        s.ObjectsCarried++;
                    }
                    else
                        ctx.Messages.WriteMessage("Sorry!!! --- sold out!");
                    break;
                case Objects.Hooker:
                    if (SoftpornHelpers.IsHere(ctx, Objects.Hooker))
                        ctx.Messages.WriteMessage("You've already paid the pimp, stupid!!!!");
                    else
                        SoftpornHelpers.FindMeOne(ctx);
                    break;
                default:
                    ctx.Messages.WriteMessage("Money can't buy everything!!!!");
                    break;
            }
        }
    }

    static void Climb(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (s.Noun == Objects.Stool)
        {
            if (SoftpornHelpers.IsHere(ctx, Objects.Stool))
            {
                SoftpornHelpers.Ok(ctx);
                s.StoolClimbed = true;
            }
            else
                ctx.Messages.WriteMessage("It's not on the floor here!");
        }
        else if (SoftpornHelpers.IsHere(ctx, s.Noun) || SoftpornHelpers.IsCarried(ctx, s.Noun))
            SoftpornHelpers.ICantDoThat(ctx);
        else
            SoftpornHelpers.FindMeOne(ctx);
    }

    static void Water(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (s.Noun is Objects.On or Objects.Off)
        {
            if (!SoftpornHelpers.IsHere(ctx, Objects.Sink))
                ctx.Messages.WriteMessage("Find a working sink!");
            else
            {
                s.WaterOn = s.Noun == Objects.On;
                if (s.WaterOn)
                {
                    ctx.Messages.WriteMessage("Water is running in the sink");
                    s.ObjectPlace[(int)Objects.Water] = s.YourPlace;
                }
                else if (!s.PitcherFull)
                {
                    SoftpornHelpers.Ok(ctx);
                    s.ObjectPlace[(int)Objects.Water] = Places.Nowhere;
                }
            }
        }
        else if (!SoftpornHelpers.IsCarried(ctx, Objects.Water))
            ctx.Messages.WriteMessage("I have no water!");
        else if (!SoftpornHelpers.IsHere(ctx, s.Noun))
            SoftpornHelpers.FindMeOne(ctx);
        else if (s.Noun == Objects.Seeds)
        {
            s.ObjectPlace[(int)Objects.Water] = Places.Nowhere;
            s.PitcherFull = false;
            if (s.YourPlace == Places.PGarden)
            {
                ctx.Messages.WriteMessage("A tree sprouts!!");
                s.ObjectPlace[(int)Objects.Tree] = s.YourPlace;
                s.ObjectPlace[(int)Objects.Seeds] = Places.Nowhere;
            }
            else
                ctx.Messages.WriteMessage("The seeds need better soil to grow.");
        }
        else
        {
            ctx.Messages.WriteMessage("It pours into the ground.");
            s.ObjectPlace[(int)Objects.Water] = Places.Nowhere;
            s.PitcherFull = false;
        }
    }

    static void Fill(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (s.Noun != Objects.Pitcher)
            SoftpornHelpers.ICantDoThat(ctx);
        else if (!SoftpornHelpers.IsCarried(ctx, Objects.Pitcher))
            ctx.Messages.WriteMessage("I don't have it!");
        else if (!SoftpornHelpers.IsHere(ctx, Objects.Sink))
            ctx.Messages.WriteMessage("Find a working sink!!");
        else if (!s.WaterOn)
            ctx.Messages.WriteMessage("No water!!");
        else if (s.PitcherFull)
            ctx.Messages.WriteMessage("The pithcer is already full!");
        else
        {
            SoftpornHelpers.Ok(ctx);
            s.PitcherFull = true;
        }
    }

    static void Pour(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (s.Noun != Objects.Water)
            SoftpornHelpers.ICantDoThat(ctx);
        else if (!SoftpornHelpers.IsCarried(ctx, Objects.Pitcher))
            ctx.Messages.WriteMessage("You have nothing to pour it with!");
        else if (!s.PitcherFull)
            ctx.Messages.WriteMessage("The pitcher is empty.");
        else if (s.YourPlace != Places.PGarden || !SoftpornHelpers.IsHere(ctx, Objects.Seeds))
            ctx.Messages.WriteMessage("It pours into the ground.");
        else
        {
            ctx.Messages.WriteMessage("A tree sprouts!!");
            s.ObjectPlace[(int)Objects.Tree] = s.YourPlace;
        }
    }

    static void Listen(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (!SoftpornHelpers.IsHere(ctx, s.Noun) && !SoftpornHelpers.IsCarried(ctx, s.Noun))
            SoftpornHelpers.FindMeOne(ctx);
        else if (s.Noun == Objects.Radio)
        {
            if (SoftpornHelpers.IsCarried(ctx, Objects.Radio))
            {
                if (s.RadioListened)
                    ctx.Messages.WriteMessage("Punk rock!!!!!");
                else
                {
                    ctx.Messages.WriteMessage("An advertisement says 'Call 555-0987 for all your liquor needs!!!!'");
                    s.RadioListened = true;
                }
            }
            else
                ctx.Messages.WriteMessage("Take it and put it next to my ear!");
        }
        else
            ctx.Messages.WriteMessage("Quiet as a mouse in heat!");
    }

    static void CloseVerb(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (!SoftpornHelpers.IsHere(ctx, s.Noun))
            SoftpornHelpers.FindMeOne(ctx);
        else
        {
            switch (s.Noun)
            {
                case Objects.Desk:
                    SoftpornHelpers.Close(ctx, () => s.DrawerOpen, v => s.DrawerOpen = v);
                    if (SoftpornHelpers.IsHere(ctx, Objects.Newspaper))
                        s.ObjectPlace[(int)Objects.Newspaper] = Places.Nowhere;
                    break;
                case Objects.Closet:
                    SoftpornHelpers.Close(ctx, () => s.ClosetOpen, v => s.ClosetOpen = v);
                    if (SoftpornHelpers.IsHere(ctx, Objects.Doll))
                        s.ObjectPlace[(int)Objects.Doll] = Places.Nowhere;
                    break;
                case Objects.Cabinet:
                    if (s.StoolClimbed)
                    {
                        SoftpornHelpers.Close(ctx, () => s.CabinetOpen, v => s.CabinetOpen = v);
                        if (SoftpornHelpers.IsHere(ctx, Objects.Pitcher))
                            s.ObjectPlace[(int)Objects.Pitcher] = Places.Nowhere;
                    }
                    else
                        ctx.Messages.WriteMessage("I can't reach it!");
                    break;
                case Objects.DoorWest:
                    SoftpornHelpers.Close(ctx, () => s.DoorWOpen, v => s.DoorWOpen = v);
                    s.Path[(int)Places.DEntrnc, (int)Directions.West] = Places.Nowhere;
                    break;
                default:
                    SoftpornHelpers.ICantDoThat(ctx);
                    break;
            }
        }
    }

    static void Jump(SoftpornContext ctx)
    {
        if (ctx.State.YourPlace is Places.BBalcny or Places.BWledge)
            SoftpornHelpers.FallingDown(ctx);
        else
            ctx.Messages.WriteMessage("Whoooopeeeee!!!");
    }

    static void Marry(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (s.Noun != Objects.Girl)
            ctx.Messages.WriteMessage("No way, weirdo!!");
        else if (!SoftpornHelpers.IsHere(ctx, Objects.Girl))
            ctx.Messages.WriteMessage("No girl!!");
        else if (s.YourPlace != Places.CMarryc)
            SoftpornHelpers.NotYetButMaybeLater(ctx);
        else if (s.Money < 30 || !SoftpornHelpers.IsCarried(ctx, Objects.Wallet))
        {
            if (s.Money < 20 || !SoftpornHelpers.IsCarried(ctx, Objects.Wallet))
                ctx.Messages.WriteMessage("The girl says: 'But you'll need $2000 for the honeymoon suite!'");
            ctx.Messages.WriteMessage("The preacher says 'I'll need $1000 too!!'");
        }
        else
        {
            ctx.Messages.WriteLongMessage(66);
            s.Money -= 30;
            s.ObjectPlace[(int)Objects.Girl] = Places.CHmoons;
            s.MarriedToGirl = true;
            s.Path[(int)Places.CHallwy, (int)Directions.South] = Places.CHmoons;
        }
    }

    static void Fuck(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (!SoftpornHelpers.IsHere(ctx, s.Noun) && !SoftpornHelpers.IsCarried(ctx, s.Noun) && s.Noun != Objects.You)
            SoftpornHelpers.FindMeOne(ctx);
        else
        {
            switch (s.Noun)
            {
                case Objects.Hooker:
                    if (s.HookerFucked)
                        ctx.Messages.WriteMessage("She can't take it any more!!!!");
                    else if (s.RubberWorn)
                    {
                        s.HookerFucked = true;
                        s.Score++;
                        ctx.Messages.WriteLongMessage(51);
                    }
                    else
                    {
                        ctx.Messages.WriteMessage("Oh no!!! I've got the dreaded atomic clap!!! I'm dead!!");
                        SoftpornHelpers.Purgatory(ctx);
                    }
                    break;
                case Objects.Doll:
                    if (SoftpornHelpers.IsCarried(ctx, Objects.Doll))
                    {
                        if (s.DollInflated)
                        {
                            ctx.Messages.WriteLongMessage(52);
                            s.ObjectPlace[(int)Objects.Doll] = Places.Nowhere;
                            s.ObjectsCarried--;
                        }
                        else
                            ctx.Messages.WriteMessage("Inflate it first -- stupid!!!");
                    }
                    else
                        ctx.Messages.WriteMessage("I can't unless I'm holding it close");
                    break;
                case Objects.Girl:
                    switch (s.YourPlace)
                    {
                        case Places.CHmoons:
                            if (s.WineOrdered)
                            {
                                ctx.Messages.WriteLongMessage(54);
                                s.Girl2Fucked = true;
                                s.Score++;
                                s.TiedToBed = true;
                                s.ObjectPlace[(int)Objects.Girl] = Places.PJacuzi;
                                s.ObjectPlace[(int)Objects.Rope] = s.YourPlace;
                            }
                            else
                                ctx.Messages.WriteMessage("She says  'Get me wine!!!  I'm nervous!!'");
                            break;
                        case Places.PJacuzi:
                            if (s.AppleGiven)
                            {
                                s.Score++;
                                ctx.Messages.WriteLongMessage(53);
                                s.GameEnded = true;
                            }
                            else
                                SoftpornHelpers.NotYetButMaybeLater(ctx);
                            break;
                        default:
                            SoftpornHelpers.NotYetButMaybeLater(ctx);
                            break;
                    }
                    break;
                case Objects.Bartender:
                    ctx.Messages.WriteMessage("He jumps over the bar and kills me!!");
                    SoftpornHelpers.Purgatory(ctx);
                    break;
                case Objects.You:
                    ctx.Messages.WriteMessage("Not tonight -- I have a headache!!");
                    break;
                case Objects.Waitress:
                    ctx.Messages.WriteMessage("She kicks me in the groin and says 'Wise up - Buster!!'");
                    break;
                case Objects.Blonde:
                    ctx.Messages.WriteMessage("She says 'I'm working! Leave me alone!!'");
                    break;
                case Objects.Pimp:
                    ctx.Messages.WriteMessage("He says 'You'll never have enough money for me - fool!'.  I guess he's gay!");
                    break;
                case Objects.Bum:
                    ctx.Messages.WriteMessage("To do that I need vaseline!!");
                    break;
                case Objects.Businessman:
                    ctx.Messages.WriteMessage("No way!!!  You're weird!!");
                    break;
                case Objects.Off:
                    ctx.Messages.WriteMessage("Fuck off yourself, asshole!!!");
                    break;
                default:
                    ctx.Messages.WriteMessage("Pervert!");
                    break;
            }
        }
    }

    static void Wear(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (!SoftpornHelpers.IsHere(ctx, s.Noun) && !SoftpornHelpers.IsCarried(ctx, s.Noun) && s.Noun != Objects.Knife)
            SoftpornHelpers.FindMeOne(ctx);
        else
        {
            switch (s.Noun)
            {
                case Objects.Rubber:
                    ctx.Messages.WriteMessage("It tickles!!");
                    s.RubberWorn = true;
                    s.ObjectPlace[(int)Objects.Rubber] = Places.YouHaveIt;
                    break;
                case Objects.Toilet:
                    ctx.Messages.WriteMessage(".....I got those constipation blues..........");
                    ctx.Out.WriteLine("Ahhh...relief! Thanks");
                    break;
                case Objects.Bed:
                    ctx.Messages.WriteMessage("Ahhhhh.......sleep!!!!");
                    Thread.Sleep(1000);
                    ctx.Out.WriteLine("No, I can't sleep!  Have to find me a girl!!!!");
                    break;
                case Objects.Rope:
                    if (SoftpornHelpers.IsCarried(ctx, Objects.Rope))
                    {
                        if (s.YourPlace == Places.BBalcny)
                        {
                            s.ObjectPlace[(int)Objects.Rope] = s.YourPlace;
                            s.RopeInUse = true;
                            ctx.Messages.WriteMessage("You tie the safety rope to the balcony");
                        }
                        else
                            SoftpornHelpers.NotYetButMaybeLater(ctx);
                    }
                    else
                        SoftpornHelpers.IDontHaveIt(ctx);
                    break;
                case Objects.Passcard:
                    if (SoftpornHelpers.IsCarried(ctx, Objects.Passcard))
                    {
                        if (s.YourPlace == Places.DEntrnc)
                        {
                            ctx.Messages.WriteMessage("I show my passcard and the door opens");
                            s.Path[(int)Places.DEntrnc, (int)Directions.West] = Places.DDisco;
                        }
                        else
                            SoftpornHelpers.NotYetButMaybeLater(ctx);
                    }
                    else
                        SoftpornHelpers.IDontHaveIt(ctx);
                    break;
                case Objects.Knife:
                    ctx.Messages.WriteMessage("Let me see if I still have the knife!");
                    Thread.Sleep(600);
                    if (SoftpornHelpers.IsCarried(ctx, Objects.Knife))
                    {
                        if (s.TiedToBed)
                        {
                            ctx.Messages.WriteMessage("I do and it worked! Thanks!");
                            s.TiedToBed = false;
                        }
                        else
                        {
                            ctx.Messages.WriteMessage("Samurai sex fiend!!!!!!!!!!!!!!!!!!!");
                            Thread.Sleep(600);
                            ctx.Out.WriteLine("I stab myself in extacy!");
                            SoftpornHelpers.Purgatory(ctx);
                        }
                    }
                    else
                        SoftpornHelpers.IDontHaveIt(ctx);
                    break;
                default:
                    SoftpornHelpers.ICantDoThat(ctx);
                    break;
            }
        }
    }

    static void Answer(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (!SoftpornHelpers.IsHere(ctx, s.Noun))
            SoftpornHelpers.FindMeOne(ctx);
        else if (s.Noun != Objects.Telephone)
            SoftpornHelpers.ICantDoThat(ctx);
        else if (s.TelephoneRinging)
        {
            ctx.Out.WriteLine();
            ctx.Out.WriteLine($"A girl says  'Hi honey!  This is {s.GirlName}. Dear, why");
            ctx.Out.WriteLine($"don't you forget this game and {s.GirlDo} with me????");
            ctx.Out.WriteLine($"After all, your {s.YourPart} has always turned me on!!!!");
            ctx.Out.WriteLine($"So bring a {s.YourObject} and come play with my {s.GirlPart}!'");
            ctx.Out.WriteLine("She hangs up!");
            s.TelephoneRinging = false;
            s.TelephoneAnswered = true;
        }
        else
            ctx.Messages.WriteMessage("It's not ringing!");
    }

    static void Call(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (s.YourPlace == Places.PPntpch)
            ctx.Messages.WriteMessage("This only takes incoming calls!!");
        else if (ctx.FullNoun == "6969" && !s.Called5556969)
        {
            ctx.Out.WriteLine();
            ctx.Out.WriteLine("A voice says 'Hello, please answer the questions with one word answers:");
            ctx.Out.Write("What's your favorite girls name?  ");
            s.GirlName = ctx.Io.ReadLine("");
            ctx.Out.Write("Name a nice part of her anatomy!  ");
            s.GirlPart = ctx.Io.ReadLine("");
            ctx.Out.Write("What do you like to do with her?  ");
            s.GirlDo = ctx.Io.ReadLine("");
            ctx.Out.Write("And the best part of your body?   ");
            s.YourPart = ctx.Io.ReadLine("");
            ctx.Out.Write("Finally, your favorite object?    ");
            s.YourObject = ctx.Io.ReadLine("");
            ctx.Out.WriteLine("He hangs up!");
            s.Called5556969 = true;
            s.GirlName = TitleCaseFirst(Lower(s.GirlName));
            s.GirlPart = Lower(s.GirlPart);
            s.GirlDo = Lower(s.GirlDo);
            s.YourPart = Lower(s.YourPart);
            s.YourObject = Lower(s.YourObject);
        }
        else if (ctx.FullNoun == "0439" && !s.Called5550439)
        {
            ctx.Messages.WriteLongMessage(67);
            s.Called5550439 = true;
        }
        else if (ctx.FullNoun == "0987" && s.MarriedToGirl && !s.Called5550987)
        {
            ctx.Messages.WriteLongMessage(68);
            s.WineOrdered = true;
            s.Called5550987 = true;
            s.ObjectPlace[(int)Objects.Wine] = Places.CHmoons;
        }
        else
            ctx.Messages.WriteMessage("Nobody answers");
    }

    static void Break(SoftpornContext ctx)
    {
        if (!SoftpornHelpers.IsHere(ctx, ctx.State.Noun))
            SoftpornHelpers.FindMeOne(ctx);
        else if (ctx.State.Noun == Objects.Window)
        {
            ctx.Messages.WriteMessage("Let me see if I have a hammer");
            Thread.Sleep(400);
            if (SoftpornHelpers.IsCarried(ctx, Objects.Hammer))
            {
                ctx.Messages.WriteMessage("The window smashes to pieces");
                ctx.State.Path[(int)Places.BWledge, (int)Directions.South] = Places.BInroom;
            }
            else
                SoftpornHelpers.IDontHaveIt(ctx);
        }
        else
            SoftpornHelpers.ICantDoThat(ctx);
    }

    static void Cut(SoftpornContext ctx)
    {
        var s = ctx.State;
        ctx.Messages.WriteMessage("Let me see if I still have the knife!");
        Thread.Sleep(600);
        if (SoftpornHelpers.IsCarried(ctx, Objects.Knife))
        {
            if (s.Noun == Objects.Rope && s.TiedToBed)
            {
                ctx.Messages.WriteMessage("I do and it worked! Thanks!");
                s.TiedToBed = false;
            }
            else
            {
                ctx.Messages.WriteMessage("Samurai sex fiend!!!!!!!!!!!!!!!!!!!");
                Thread.Sleep(600);
                ctx.Out.WriteLine("I stab myself in extacy!");
                SoftpornHelpers.Purgatory(ctx);
            }
        }
        else
            SoftpornHelpers.IDontHaveIt(ctx);
    }

    static void Dance(SoftpornContext ctx)
    {
        ctx.Out.WriteLine();
        for (var i = 0; i < 3; i++)
        {
            Thread.Sleep(500);
            ctx.Out.WriteLine("Boogie Woogie!!!");
            Thread.Sleep(500);
            ctx.Out.WriteLine("Yeh Yeh Yeh!!!");
        }
        ctx.Out.WriteLine("I got the steps, man!!");
    }

    static void Kill(SoftpornContext ctx) =>
        ctx.Messages.WriteMessage("Try using a knife!!!");

    static void Pay(SoftpornContext ctx)
    {
        if (!SoftpornHelpers.IsHere(ctx, ctx.State.Noun))
            SoftpornHelpers.FindMeOne(ctx);
        else
        {
            switch (ctx.State.Noun)
            {
                case Objects.Pimp:
                    if (ctx.State.HookerFucked)
                        ctx.Messages.WriteMessage("He says 'I don't want your money - stud!'");
                    else
                        ctx.Messages.WriteMessage("Try going up -- he'll take the money then");
                    break;
                case Objects.Hooker:
                    ctx.Messages.WriteMessage("You already paid the Pimp, stupid!!");
                    break;
                case Objects.Blonde or Objects.Waitress or Objects.Girl:
                    ctx.Messages.WriteMessage("She yells 'I'm not a whore!!!' and kills me!");
                    SoftpornHelpers.Purgatory(ctx);
                    break;
                case Objects.Preacher:
                    ctx.Messages.WriteMessage("Bring a girl here to marry -- he'll take the money then!");
                    break;
                case Objects.Businessman:
                    ctx.Messages.WriteMessage("He's too drunk to do business right now!");
                    break;
                case Objects.Bartender:
                    ctx.Messages.WriteMessage("Buy something -- he'll take the money then");
                    break;
                case Objects.Dealer:
                    ctx.Messages.WriteMessage("Why not play 21 instead?  You'll lose anyway, fool!");
                    break;
                default:
                    SoftpornHelpers.ICantDoThat(ctx);
                    break;
            }
        }
    }

    static void Smoke(SoftpornContext ctx)
    {
        if (ctx.State.Noun == Objects.Plant)
        {
            ctx.Messages.WriteMessage("A cop beats me over the head!!!!");
            SoftpornHelpers.Purgatory(ctx);
        }
        else
            SoftpornHelpers.ICantDoThat(ctx);
    }

    static void Show(SoftpornContext ctx)
    {
        var s = ctx.State;
        if (s.Noun == Objects.Passcard)
        {
            if (SoftpornHelpers.IsCarried(ctx, Objects.Passcard))
            {
                if (s.YourPlace == Places.DEntrnc)
                {
                    ctx.Messages.WriteMessage("I show my passcard and the door opens");
                    s.Path[(int)Places.DEntrnc, (int)Directions.West] = Places.DDisco;
                }
                else
                    SoftpornHelpers.NotYetButMaybeLater(ctx);
            }
            else
                SoftpornHelpers.IDontHaveIt(ctx);
        }
        else
            SoftpornHelpers.ICantDoThat(ctx);
    }

    static void Smell(SoftpornContext ctx)
    {
        if (!SoftpornHelpers.IsHere(ctx, ctx.State.Noun) && !SoftpornHelpers.IsCarried(ctx, ctx.State.Noun))
            SoftpornHelpers.FindMeOne(ctx);
        else
        {
            ctx.Messages.WriteMessage(ctx.State.Noun switch
            {
                Objects.Blonde => "Hmmm.....nice!!!!",
                Objects.Hooker => "OK, who's eating tuna fish?!?!?!",
                Objects.Toilet => "Arghhh...I'm going to puke!!!!!!",
                Objects.Plant => "Ahhh..chooo!!!!!!  I guess I'm allergic!",
                Objects.Garbage => "Yechhhhh!!!!!",
                Objects.Flowers => "Smells like perfume!!!",
                _ => "Smells OK"
            });
        }
    }

    static void Kiss(SoftpornContext ctx) =>
        ctx.Messages.WriteMessage("Don't do that!!!!  It gets me excited!!");

    static void ShowScore(SoftpornContext ctx)
    {
        ctx.Out.WriteLine();
        ctx.Out.WriteLine($"Your score is '{ctx.State.Score}' out of a possible '3'");
    }

    static void Save(SoftpornContext ctx)
    {
        var objnam = ctx.State.ObjNam.TrimEnd();
        var space = objnam.IndexOf(' ');
        if (space > 0)
            objnam = objnam[..space];
        var fileName = GameState.SaveFileName(objnam);
        ctx.State.SaveTo(fileName);
        ctx.Messages.WriteMessage($"{fileName} saved");
    }

    static void Restore(SoftpornContext ctx)
    {
        var objnam = ctx.State.ObjNam.TrimEnd();
        var space = objnam.IndexOf(' ');
        if (space > 0)
            objnam = objnam[..space];
        var fileName = GameState.SaveFileName(objnam);
        ctx.Messages.WriteMessage($"Restoring from {fileName}");
        ctx.Out.WriteLine();
        var loaded = GameState.LoadFrom(fileName);
        if (loaded == null)
            ctx.Messages.WriteMessage($"{fileName} never saved");
        else
        {
            CopyState(ctx.State, loaded);
            ctx.Messages.WriteLongMessage((int)ctx.State.YourPlace + 1);
        }
    }

    static void CopyState(GameState target, GameState source)
    {
        Array.Copy(source.ObjectPlace, target.ObjectPlace, target.ObjectPlace.Length);
        Array.Copy(source.PlaceVisited, target.PlaceVisited, target.PlaceVisited.Length);
        for (var p = 0; p <= (int)Places.Nowhere; p++)
            for (var d = 0; d < 6; d++)
                target.Path[p, d] = source.Path[p, d];

        target.YourPlace = source.YourPlace;
        target.NewPlace = source.NewPlace;
        target.Noun = source.Noun;
        target.Verb = source.Verb;
        target.Direction = source.Direction;
        target.GameEnded = source.GameEnded;
        target.ObjectsCarried = source.ObjectsCarried;
        target.Money = source.Money;
        target.Score = source.Score;
        target.TvChannel = source.TvChannel;
        target.RopeInUse = source.RopeInUse;
        target.WindowBroken = source.WindowBroken;
        target.ToiletFlushed = source.ToiletFlushed;
        target.Called5550987 = source.Called5550987;
        target.Called5556969 = source.Called5556969;
        target.Called5550439 = source.Called5550439;
        target.RubberWorn = source.RubberWorn;
        target.HookerFucked = source.HookerFucked;
        target.DoorWOpen = source.DoorWOpen;
        target.RadioListened = source.RadioListened;
        target.WineOrdered = source.WineOrdered;
        target.TelephoneRinging = source.TelephoneRinging;
        target.TelephoneAnswered = source.TelephoneAnswered;
        target.HolePeeped = source.HolePeeped;
        target.Girl2Fucked = source.Girl2Fucked;
        target.TiedToBed = source.TiedToBed;
        target.DrawerOpen = source.DrawerOpen;
        target.ClosetOpen = source.ClosetOpen;
        target.CabinetOpen = source.CabinetOpen;
        target.DollInflated = source.DollInflated;
        target.StoolClimbed = source.StoolClimbed;
        target.WaterOn = source.WaterOn;
        target.PitcherFull = source.PitcherFull;
        target.SeedsPlanted = source.SeedsPlanted;
        target.SeedsWatered = source.SeedsWatered;
        target.AppleGiven = source.AppleGiven;
        target.CandyGiven = source.CandyGiven;
        target.FlowersGiven = source.FlowersGiven;
        target.RingGiven = source.RingGiven;
        target.MarriedToGirl = source.MarriedToGirl;
        target.GirlName = source.GirlName;
        target.GirlPart = source.GirlPart;
        target.GirlDo = source.GirlDo;
        target.YourPart = source.YourPart;
        target.YourObject = source.YourObject;
        target.RubberColor = source.RubberColor;
        target.RubberFlavor = source.RubberFlavor;
        target.RubberLubricated = source.RubberLubricated;
        target.RubberRibbed = source.RubberRibbed;
    }

    static string Lower(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;
        return string.Concat(s.Select(SoftpornHelpers.Locase));
    }

    static string TitleCaseFirst(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;
        return char.ToUpperInvariant(s[0]) + (s.Length > 1 ? Lower(s[1..]) : "");
    }
}
