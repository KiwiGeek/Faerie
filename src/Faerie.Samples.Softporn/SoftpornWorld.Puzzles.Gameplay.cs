using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;
using static Faerie.Verbs.StandardVerbIds;

namespace Faerie.Samples.Softporn;

internal sealed partial class SoftpornWorld
{
    private bool InBarArea(GameContext ctx) =>
        ctx.CurrentRoom == R(SoftpornIds.Hallway) || ctx.CurrentRoom == R(SoftpornIds.Bathroom) ||
        ctx.CurrentRoom == R(SoftpornIds.Bar) || ctx.CurrentRoom == R(SoftpornIds.BarStreet) ||
        ctx.CurrentRoom == R(SoftpornIds.Backroom) || ctx.CurrentRoom == R(SoftpornIds.Dumpster) ||
        ctx.CurrentRoom == R(SoftpornIds.BrokenRoom) || ctx.CurrentRoom == R(SoftpornIds.WindowLedge) ||
        ctx.CurrentRoom == R(SoftpornIds.HookerBedroom) || ctx.CurrentRoom == R(SoftpornIds.HookerBalcony);

    private bool InCasinoArea(GameContext ctx) =>
        ctx.CurrentRoom == R(SoftpornIds.CasinoStreet) || ctx.CurrentRoom == R(SoftpornIds.MarriageCenter) ||
        ctx.CurrentRoom == R(SoftpornIds.Casino) || ctx.CurrentRoom == R(SoftpornIds.TwentyOneRoom) ||
        ctx.CurrentRoom == R(SoftpornIds.Lobby) || ctx.CurrentRoom == R(SoftpornIds.HoneymoonSuite) ||
        ctx.CurrentRoom == R(SoftpornIds.HotelHallway) || ctx.CurrentRoom == R(SoftpornIds.HoneymoonBalcony) ||
        ctx.CurrentRoom == R(SoftpornIds.HotelDesk);

    private bool InDiscoArea(GameContext ctx) =>
        ctx.CurrentRoom == R(SoftpornIds.PhoneBooth) || ctx.CurrentRoom == R(SoftpornIds.Disco) ||
        ctx.CurrentRoom == R(SoftpornIds.DiscoStreet) || ctx.CurrentRoom == R(SoftpornIds.DiscoEntrance) ||
        ctx.CurrentRoom == R(SoftpornIds.Pharmacy);

    private bool InPenthouseArea(GameContext ctx) =>
        ctx.CurrentRoom == R(SoftpornIds.PenthouseFoyer) || ctx.CurrentRoom == R(SoftpornIds.Jacuzzi) ||
        ctx.CurrentRoom == R(SoftpornIds.Kitchen) || ctx.CurrentRoom == R(SoftpornIds.Garden) ||
        ctx.CurrentRoom == R(SoftpornIds.LivingRoom) || ctx.CurrentRoom == R(SoftpornIds.PenthousePorch);

    private bool InPublic(GameContext ctx) =>
        ctx.CurrentRoom == R(SoftpornIds.CasinoStreet) || ctx.CurrentRoom == R(SoftpornIds.Casino) ||
        ctx.CurrentRoom == R(SoftpornIds.TwentyOneRoom) || ctx.CurrentRoom == R(SoftpornIds.Lobby) ||
        ctx.CurrentRoom == R(SoftpornIds.HotelDesk) || ctx.CurrentRoom == R(SoftpornIds.DiscoStreet) ||
        ctx.CurrentRoom == R(SoftpornIds.DiscoEntrance);

    private void ApplyAreaUpdates(GameContext ctx)
    {
        if (InBarArea(ctx))
        {
            MoveProp(ctx, SoftpornIds.Sign, SoftpornIds.BarStreet);
            MoveProp(ctx, SoftpornIds.Taxi, SoftpornIds.BarStreet);
            MoveProp(ctx, SoftpornIds.Button, SoftpornIds.Bar);
        }
        else if (InCasinoArea(ctx))
        {
            MoveProp(ctx, SoftpornIds.Sign, SoftpornIds.CasinoStreet);
            MoveProp(ctx, SoftpornIds.Taxi, SoftpornIds.CasinoStreet);
            MoveProp(ctx, SoftpornIds.Button, SoftpornIds.HotelDesk);
            MoveProp(ctx, SoftpornIds.Elevator, SoftpornIds.HotelDesk);
        }
        else if (InDiscoArea(ctx))
        {
            MoveProp(ctx, SoftpornIds.Sign, SoftpornIds.DiscoStreet);
            MoveProp(ctx, SoftpornIds.Taxi, SoftpornIds.DiscoStreet);
            MoveProp(ctx, SoftpornIds.Telephone, SoftpornIds.PhoneBooth);
        }
        else if (InPenthouseArea(ctx))
        {
            MoveProp(ctx, SoftpornIds.Button, SoftpornIds.PenthouseFoyer);
            MoveProp(ctx, SoftpornIds.Elevator, SoftpornIds.PenthouseFoyer);
            MoveProp(ctx, SoftpornIds.Telephone, SoftpornIds.PenthousePorch);
        }

        if (!ctx.InRoom(Bar))
            ctx.Set(_curtainOpen, false);

        if (!ctx.InRoom(DiscoEntrance))
            ctx.Set(_doorWestOpen, false);

        if (!ctx.Carrying(T(SoftpornIds.Stool)) && !ctx.LocatedIn(T(SoftpornIds.Stool), Garden))
            ctx.Set(_stoolClimbed, false);

        if (ctx.Get(_rubberWorn) && InPublic(ctx) && ctx.Random.Next(8) == 5)
        {
            ctx.Say("A passerby kills me for wearing my kinky rubber in public!");
            Purgatory(ctx);
        }
    }

    private void MoveProp(GameContext ctx, string thingId, string roomId)
    {
        Thing thing = T(thingId);
        Room room = R(roomId);
        if (!ctx.Carrying(thing) && ctx.RoomOf(thing) != room)
            ctx.Move(thing, Placement.InRoom(room));
    }

    private void DefineExamines()
    {
        Examine(SoftpornIds.Hooker, ctx => SayLong(ctx, 31));
        Examine(SoftpornIds.Girl, ctx =>
        {
            if (ctx.InRoom(Jacuzzi)) SayLong(ctx, 35);
            else if (ctx.InRoom(R(SoftpornIds.Disco)) || ctx.InRoom(R(SoftpornIds.MarriageCenter))) SayLong(ctx, 34);
            else ctx.Say("She slaps me and yells 'Pervert!!!!!'");
        });
        Examine(SoftpornIds.Blonde, ctx => SayLong(ctx, 40));
        Examine(SoftpornIds.Billboard, ctx => SayLong(ctx, 63));
        Examine(SoftpornIds.Graffiti, ctx => SoftpornMessages.SayBlock(ctx, 59, 62));
        Examine(SoftpornIds.Peephole, ctx =>
        {
            if (ctx.Get(_holePeeped)) ctx.Say("All windows at the hotel across the road have their curtains shut.");
            else { SayLong(ctx, 55); ctx.Set(_holePeeped, true); }
        });
        Examine(SoftpornIds.Desk, ctx =>
        {
            if (ctx.Get(_drawerOpen)) RevealHere(ctx, SoftpornIds.Newspaper);
            else ctx.Say("It's drawer is shut");
        });
        Examine(SoftpornIds.Washbasin, ctx => RevealHere(ctx, SoftpornIds.Ring));
        Examine(SoftpornIds.Garbage, ctx => RevealHere(ctx, SoftpornIds.AppleCore));
        Examine(SoftpornIds.AppleCore, ctx => RevealHere(ctx, SoftpornIds.Seeds));
        Examine(SoftpornIds.Ashtray, ctx => RevealHere(ctx, SoftpornIds.Passcard));
        Examine(SoftpornIds.Rack, ctx => RevealHere(ctx, SoftpornIds.Magazine));
        Examine(SoftpornIds.Tree, ctx => RevealHere(ctx, SoftpornIds.Apple));
        Examine(SoftpornIds.Plant, ctx =>
        {
            if (IsOffstage(ctx, T(SoftpornIds.Bushes)))
            {
                ctx.Say("There's a group of bushes behind it!!");
                ctx.PlaceHere(T(SoftpornIds.Bushes));
            }
            else CantDoThat(ctx);
        });
        Examine(SoftpornIds.Newspaper, ctx =>
        {
            if (ctx.Carrying(T(SoftpornIds.Newspaper))) SayLong(ctx, 32);
            else ctx.Say("I don't have it!!");
        });
        Examine(SoftpornIds.Magazine, ctx =>
        {
            if (ctx.Carrying(T(SoftpornIds.Magazine))) SayLong(ctx, 33);
            else ctx.Say("I don't have it!!");
        });
        Examine(SoftpornIds.Mirror, ctx => ctx.Say("There's a pervert looking back at me!!"));
        Examine(SoftpornIds.Toilet, ctx => ctx.Say("Hasn't been flushed in ages! Stinks!!!"));
        Examine(SoftpornIds.Businessman, ctx => ctx.Say("He looks like a whiskey drinker to me!!"));
        Examine(SoftpornIds.Button, ctx => ctx.Say("Says Push."));
        Examine(SoftpornIds.Bartender, ctx => ctx.Say("He's waiting for me to buy something!"));
        Examine(SoftpornIds.Pimp, ctx => ctx.Say("He's wearing a button proclaiming -- Support your local Pimp, gimme $2000!!!"));
        _b.On(T(SoftpornIds.Tv)).Before(_b.Verbs.Examine!, ctx =>
        {
            if (!ctx.Carrying(T(SoftpornIds.ControlUnit)))
            {
                ctx.Say("To watch TV, I need the remote control unit!!");
                return VerbResult.Done;
            }
            WatchTv(ctx);
            return VerbResult.Done;
        });
        Examine(SoftpornIds.SlotMachines, ctx => ctx.Say("Playing them might be more fun...."));
        Examine(SoftpornIds.Bum, ctx => ctx.Say("He grumbles -- I'll tell you a story for a bottle of wine....."));
        Examine(SoftpornIds.DoorWest, ctx =>
        {
            if (ctx.Get(_doorWestOpen)) ctx.Say("The door is open");
            else
            {
                ctx.Say("The sign on the door says ");
                ctx.Say("'Entry by showing passcard - Club members and their guests only!'");
            }
        });
        Examine(SoftpornIds.Telephone, ctx =>
        {
            if (ctx.InRoom(R(SoftpornIds.PhoneBooth)))
                ctx.Say("A number is there - Call 555-6969 for a good time!");
            else
                ctx.Say("I see nothing special");
        });
        Examine(SoftpornIds.Wallet, ctx =>
        {
            if (Money(ctx) > 0) ctx.Say($"I've got ${Money(ctx)}00 in it.");
            else ctx.Say("It's empty!");
        });
        Examine(SoftpornIds.Rubber, ctx =>
        {
            if (!ctx.Carrying(T(SoftpornIds.Rubber))) { ctx.Say("I don't have it!!"); return; }
            ctx.Say($"It's {ctx.Get(_rubberColor)}, {ctx.Get(_rubberFlavor)}-flavored, " +
                    $"{ctx.Get(_rubberLubricated)}, and {ctx.Get(_rubberRibbed)}");
        });
        Examine(SoftpornIds.Pills, ctx =>
        {
            ctx.Say("The label on the bottle says");
            ctx.Say("'Want to drive someone crazy with lust?? Try this!!!!'");
        });
        Examine(SoftpornIds.Closet, ctx =>
        {
            if (ctx.Get(_closetOpen))
            {
                if (ctx.LocatedIn(T(SoftpornIds.Doll), ctx.CurrentRoom))
                    ctx.Say("It's open");
                RevealHere(ctx, SoftpornIds.Doll);
            }
            else
                ctx.Say("It's closed");
        });
        Examine(SoftpornIds.Sink, ctx => ctx.Say("The sign over the sink says 'Water on or off to operate'"));
        Examine(SoftpornIds.Elevator, ctx => ctx.Say("It's doors are closed"));
        Examine(SoftpornIds.Dealer, ctx => ctx.Say("He's waiting for me to play"));
        Examine(SoftpornIds.Cabinet, ctx =>
        {
            if (!ctx.Get(_stoolClimbed))
            {
                ctx.Say("I see nothing special");
                return;
            }
            if (ctx.Get(_cabinetOpen))
            {
                if (ctx.LocatedIn(T(SoftpornIds.Pitcher), ctx.CurrentRoom))
                    ctx.Say("It's open");
                RevealHere(ctx, SoftpornIds.Pitcher);
            }
            else
                ctx.Say("It's closed");
        });
        Examine(SoftpornIds.Bushes, ctx => ctx.Say("Entering them would be kinky!!!!"));
        Examine(SoftpornIds.Sign, ctx => ctx.Say("It says 'Hail taxi here'"));
        Examine(SoftpornIds.Flowers, ctx => ctx.Say("They look beautiful!!!"));
        Examine(SoftpornIds.Doll, ctx =>
        {
            if (ctx.Get(_dollInflated))
                ctx.Say("It's inflated");
            else
                ctx.Say("It's rolled up in a little ball");
        });
        Examine(SoftpornIds.Pitcher, ctx =>
        {
            if (ctx.Get(_pitcherFull))
                ctx.Say("It's full of water");
            else
                ctx.Say("It's empty");
        });
        Examine(SoftpornIds.Curtain, ctx => ctx.Say("It's on the east wall"));
        Examine(SoftpornIds.Waitress, ctx => ctx.Say("She ignores you!"));
        Examine(SoftpornIds.Radio, ctx => ctx.Say("Maybe I should listen..."));
        Examine(SoftpornIds.Window, ctx => ctx.Say("The window looks into a room. But I can't see too much from here."));
    }

    private void Examine(string thingId, Action<GameContext> handler)
    {
        T(thingId).OnExamine = handler;
    }

    private void DefineContainers()
    {
        _b.On(T(SoftpornIds.Desk)).Before(_b.Verbs.Open!, ctx =>
        {
            if (ctx.Get(_drawerOpen))
            {
                ctx.Say("It's already open!!");
                return VerbResult.Done;
            }
            ctx.Set(_drawerOpen, true);
            return VerbResult.Done;
        });
        _b.On(T(SoftpornIds.Desk)).Before(_b.Verbs.Close!, ctx =>
        {
            if (!ctx.Get(_drawerOpen))
            {
                ctx.Say("It's already closed!!");
                return VerbResult.Done;
            }
            ctx.Set(_drawerOpen, false);
            ctx.Remove(T(SoftpornIds.Newspaper));
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Closet)).Before(_b.Verbs.Open!, ctx =>
        {
            if (ctx.Get(_closetOpen))
            {
                ctx.Say("It's already open!!");
                return VerbResult.Done;
            }
            ctx.Set(_closetOpen, true);
            RevealHere(ctx, SoftpornIds.Doll);
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Cabinet)).Before(_b.Verbs.Open!, ctx =>
        {
            if (!ctx.Get(_stoolClimbed) && !ctx.Carrying(T(SoftpornIds.Stool)))
            {
                ctx.Say("I can't reach it!!");
                return VerbResult.Done;
            }
            if (ctx.Get(_cabinetOpen))
            {
                ctx.Say("It's already open!!");
                return VerbResult.Done;
            }
            ctx.Set(_cabinetOpen, true);
            RevealHere(ctx, SoftpornIds.Pitcher);
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Closet)).Before(_b.Verbs.Close!, ctx =>
        {
            if (!ctx.Get(_closetOpen))
            {
                ctx.Say("It's already closed!!");
                return VerbResult.Done;
            }
            ctx.Set(_closetOpen, false);
            ctx.Remove(T(SoftpornIds.Doll));
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Cabinet)).Before(_b.Verbs.Close!, ctx =>
        {
            if (!ctx.Get(_stoolClimbed)) { ctx.Say("I can't reach it!"); return VerbResult.Done; }
            if (!ctx.Get(_cabinetOpen))
            {
                ctx.Say("It's already closed!!");
                return VerbResult.Done;
            }
            ctx.Set(_cabinetOpen, false);
            ctx.Remove(T(SoftpornIds.Pitcher));
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.DoorWest)).Before(_b.Verbs.Close!, ctx =>
        {
            if (!ctx.Get(_doorWestOpen))
            {
                ctx.Say("It's already closed!!");
                return VerbResult.Done;
            }
            ctx.Set(_doorWestOpen, false);
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.DoorWest)).Before(_b.Verbs.Open!, ctx =>
        {
            if (ctx.Get(_doorWestOpen))
            {
                ctx.Say("It's already open!!");
                return VerbResult.Done;
            }
            ctx.Say("A voice asks 'Passcard?' I search in my pockets and...");
            if (ctx.Carrying(T(SoftpornIds.Passcard)))
            {
                ctx.Say("I have it! The door opens!");
                ctx.Set(_doorWestOpen, true);
            }
            else
                ctx.Say("I don't have it!!");
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Window)).Before(_break, ctx =>
        {
            ctx.Say("Let me see if I have a hammer");
            Thread.Sleep(400);
            if (!ctx.Carrying(T(SoftpornIds.Hammer)))
            {
                ctx.Say("I don't have it!!");
                return VerbResult.Done;
            }
            ctx.Say("The window smashes to pieces");
            ctx.Set(_windowBroken, true);
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Window)).Before(_b.Verbs.Open!, ctx =>
        {
            ctx.Say("Won't budge");
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Curtain)).Before(_b.Verbs.Open!, ctx =>
        {
            ctx.Say("It seems to be remotely controlled");
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Elevator)).Before(_b.Verbs.Open!, ctx =>
        {
            ctx.Say("Push the button to open elevator");
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Toilet)).Before(_flush, ctx =>
        {
            SayLong(ctx, 69);
            ctx.Say("I'm dead from the germs!!");
            Purgatory(ctx);
            return VerbResult.Done;
        });
    }

    private void DefineCommerce()
    {
        _b.On(T(SoftpornIds.Rubber)).Before(_buy, BuyAtPharmacy);
        _b.On(T(SoftpornIds.Magazine)).Before(_buy, BuyAtPharmacy);
    }

    private VerbResult BuyHandler(VerbContext ctx)
    {
        if (ctx.DirectObject == T(SoftpornIds.Wine))
            return BuyWineFromWaitress(ctx);
        if (ctx.DirectObject is { } item &&
            (item == T(SoftpornIds.Beer) || item == T(SoftpornIds.Whiskey)))
            return BuyFromBartender(ctx);
        return VerbResult.Pass;
    }

    private VerbResult BuyFromBartender(VerbContext ctx)
    {
        if (!CarryingWallet(ctx) || !HasMoney(ctx, 1))
        {
            ctx.Say("Sorry -- no money!!");
            return VerbResult.Done;
        }

        if (ctx.DirectObject is not { } item ||
            (item != T(SoftpornIds.Beer) && item != T(SoftpornIds.Whiskey)))
        {
            CantDoThat(ctx);
            return VerbResult.Done;
        }

        if (!ctx.InRoom(Bar))
        {
            ctx.Say("Not yet but maybe later!");
            return VerbResult.Done;
        }

        if (!IsOffstage(ctx, item))
        {
            ctx.Say("Sorry ... all out!");
            return VerbResult.Done;
        }

        ctx.Say("I give the bartender $100 and he places it on the bar.");
        Spend(ctx, 1);
        ctx.PlaceHere(item);
        return VerbResult.Done;
    }

    private VerbResult BuyWineFromWaitress(VerbContext ctx)
    {
        if (!CarryingWallet(ctx) || !HasMoney(ctx, 1))
        {
            ctx.Say("Sorry -- no money!!");
            return VerbResult.Done;
        }

        if (!ctx.InRoom(R(SoftpornIds.Disco)))
        {
            ctx.Say("Not yet but maybe later!");
            return VerbResult.Done;
        }

        if (!IsOffstage(ctx, T(SoftpornIds.Wine)))
        {
            ctx.Say("All out!");
            return VerbResult.Done;
        }

        ctx.Say("The waitress takes $100 and says she'll return");
        Thread.Sleep(3000);
        ctx.Say("Poor service!!!");
        Thread.Sleep(2000);
        Spend(ctx, 1);
        ctx.PlaceHere(T(SoftpornIds.Wine));
        return VerbResult.Done;
    }

    private VerbResult BuyAtPharmacy(VerbContext ctx)
    {
        if (!CarryingWallet(ctx) || !HasMoney(ctx, 1))
        {
            ctx.Say("Sorry -- no money!!");
            return VerbResult.Done;
        }

        if (!ctx.InRoom(R(SoftpornIds.Pharmacy)) || ctx.DirectObject is not { } item)
        {
            ctx.Say("Not yet but maybe later!");
            return VerbResult.Done;
        }

        if (!ctx.LocatedIn(item, ctx.CurrentRoom))
        {
            ctx.Say("Sorry!!! --- sold out!");
            return VerbResult.Done;
        }

        if (item == T(SoftpornIds.Rubber))
            BuyRubber(ctx);
        else
            ctx.Say("He takes $100 and gives me the magazine");

        Spend(ctx, 1);
        ctx.Take(item);
        return VerbResult.Done;
    }

    private void DefineTakeRules()
    {
        T(SoftpornIds.Candy).OnTake = ctx =>
        {
            if (ctx.InRoom(HookerBedroom) && !ctx.Get(_hookerFucked))
            {
                ctx.Say("The Hooker says: 'Don't take it ... do me first!!'");
                return true;
            }

            return false;
        };

        T(SoftpornIds.Rubber).OnTake = ctx =>
        {
            if (ctx.InRoom(R(SoftpornIds.Pharmacy)))
            {
                ctx.Say("The man says 'Shoplifter!!' and shoots me");
                Purgatory(ctx);
                return true;
            }

            return false;
        };

        T(SoftpornIds.Magazine).OnTake = ctx =>
        {
            if (ctx.InRoom(R(SoftpornIds.Pharmacy)))
            {
                ctx.Say("The man says 'Shoplifter!!' and shoots me");
                Purgatory(ctx);
                return true;
            }

            return false;
        };

        T(SoftpornIds.Water).OnTake = ctx =>
        {
            if (!ctx.Carrying(T(SoftpornIds.Pitcher)))
            {
                ctx.Say("Get me the pitcher so I don't spill it!");
                return true;
            }

            ctx.Set(_pitcherFull, true);
            return false;
        };

        T(SoftpornIds.Rope).OnTake = ctx =>
        {
            if (ctx.Get(_ropeInUse))
            {
                ctx.Say("It is tied to the balcony");
                return true;
            }

            return false;
        };

        T(SoftpornIds.Pitcher).OnTake = ctx =>
        {
            if (ctx.Get(_pitcherFull) && ctx.LocatedIn(T(SoftpornIds.Water), ctx.CurrentRoom))
                ctx.Take(T(SoftpornIds.Water));
            return false;
        };
    }

    private void DefineDropGifts()
    {
        T(SoftpornIds.Candy).OnDrop = ctx =>
        {
            if (!ctx.InRoom(R(SoftpornIds.Disco)) || !ctx.LocatedIn(T(SoftpornIds.Girl), ctx.CurrentRoom))
                return false;
            ctx.Say("She smiles and eats a couple!!");
            ctx.Set(_candyGiven, true);
            ctx.Remove(T(SoftpornIds.Candy));
            CheckGirlCourtship(ctx);
            return true;
        };

        T(SoftpornIds.Flowers).OnDrop = ctx =>
        {
            if (!ctx.InRoom(R(SoftpornIds.Disco)) || !ctx.LocatedIn(T(SoftpornIds.Girl), ctx.CurrentRoom))
                return false;
            ctx.Say("She blushes profusely and puts them in her hair!");
            ctx.Set(_flowersGiven, true);
            ctx.Remove(T(SoftpornIds.Flowers));
            CheckGirlCourtship(ctx);
            return true;
        };

        T(SoftpornIds.Ring).OnDrop = ctx =>
        {
            if (!ctx.InRoom(R(SoftpornIds.Disco)) || !ctx.LocatedIn(T(SoftpornIds.Girl), ctx.CurrentRoom))
                return false;
            ctx.Say("She blushes and puts it in her purse.");
            ctx.Set(_ringGiven, true);
            ctx.Remove(T(SoftpornIds.Ring));
            CheckGirlCourtship(ctx);
            return true;
        };

        T(SoftpornIds.Wine).OnDrop = ctx =>
        {
            if (ctx.LocatedIn(T(SoftpornIds.Bum), ctx.CurrentRoom))
            {
                if (IsOffstage(ctx, T(SoftpornIds.Knife)))
                {
                    BumTellsStory(ctx);
                    ctx.PlaceHere(T(SoftpornIds.Knife));
                }
                else
                    ctx.Say("The bum mutters 'That stuff made me puke!! Get out of here!!!'");
                ctx.Remove(T(SoftpornIds.Wine));
                return true;
            }

            return false;
        };

        T(SoftpornIds.Whiskey).OnDrop = ctx =>
        {
            if (ctx.LocatedIn(T(SoftpornIds.Businessman), ctx.CurrentRoom) &&
                IsOffstage(ctx, T(SoftpornIds.ControlUnit)))
            {
                ctx.Say("The guy gives me a TV controller!!");
                ctx.PlaceHere(T(SoftpornIds.ControlUnit));
                return false;
            }

            return false;
        };

        T(SoftpornIds.Pills).OnDrop = ctx =>
        {
            if (ctx.LocatedIn(T(SoftpornIds.Blonde), ctx.CurrentRoom))
            {
                SayLong(ctx, 57);
                ctx.Remove(T(SoftpornIds.Blonde));
                ctx.Remove(T(SoftpornIds.Pills));
                return true;
            }

            return false;
        };

        T(SoftpornIds.Apple).OnDrop = ctx =>
        {
            if (ctx.InRoom(Jacuzzi) && ctx.LocatedIn(T(SoftpornIds.Girl), ctx.CurrentRoom))
            {
                SayLong(ctx, 50);
                ctx.Set(_appleGiven, true);
                ctx.Remove(T(SoftpornIds.Apple));
                return true;
            }

            return false;
        };

        T(SoftpornIds.Rubber).OnDrop = ctx => { ctx.Set(_rubberWorn, false); return false; };
    }

    private void CheckGirlCourtship(GameContext ctx)
    {
        if (!ctx.Get(_candyGiven) || !ctx.Get(_flowersGiven) || !ctx.Get(_ringGiven)) return;
        ctx.Say("She says: 'See you at the Marriage Center!!'");
        ctx.Move(T(SoftpornIds.Girl), Placement.InRoom(R(SoftpornIds.MarriageCenter)));
    }

    private void DefineRomance()
    {
        _b.On(T(SoftpornIds.Hooker)).Before(_fuck, ctx =>
        {
            if (ctx.Get(_hookerFucked)) { ctx.Say("She can't take it any more!!!!"); return VerbResult.Done; }
            if (!ctx.Get(_rubberWorn))
            {
                ctx.Say("Oh no!!! I've got the dreaded atomic clap!!! I'm dead!!");
                Purgatory(ctx);
                return VerbResult.Done;
            }
            ctx.Set(_hookerFucked, true);
            ctx.State.Score++;
            SayLong(ctx, 51);
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Doll)).Before(_fuck, ctx =>
        {
            if (!ctx.Carrying(T(SoftpornIds.Doll)))
            {
                ctx.Say("I can't unless I'm holding it close");
                return VerbResult.Done;
            }
            if (!ctx.Get(_dollInflated))
            {
                ctx.Say("Inflate it first -- stupid!!!");
                return VerbResult.Done;
            }
            SayLong(ctx, 52);
            ctx.Remove(T(SoftpornIds.Doll));
            DecrementCarry(ctx);
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Girl)).Before(_fuck, ctx =>
        {
            if (ctx.InRoom(HoneymoonSuite) && ctx.Get(_wineOrdered))
            {
                SayLong(ctx, 54);
                ctx.State.Score++;
                ctx.Set(_girl2Fucked, true);
                ctx.Set(_tiedToBed, true);
                ctx.Move(T(SoftpornIds.Girl), Placement.InRoom(Jacuzzi));
                ctx.PlaceHere(T(SoftpornIds.Rope));
                return VerbResult.Done;
            }
            if (ctx.InRoom(Jacuzzi) && ctx.Get(_appleGiven))
            {
                ctx.State.Score++;
                SayLong(ctx, 53);
                ctx.Win(SoftpornMessages.Text(53));
                return VerbResult.Done;
            }
            if (ctx.InRoom(HoneymoonSuite))
            {
                ctx.Say("She says  'Get me wine!!!  I'm nervous!!'");
                return VerbResult.Done;
            }
            return VerbResult.Pass;
        });

        _b.On(T(SoftpornIds.Rubber)).Before(_b.Verbs.TakeOff!, ctx =>
        {
            ctx.Set(_rubberWorn, false);
            return VerbResult.Pass;
        });

        _b.On(T(SoftpornIds.Rubber)).Before(_b.Verbs.Wear!, ctx =>
        {
            ctx.Say("It tickles!!");
            ctx.Set(_rubberWorn, true);
            ctx.Move(T(SoftpornIds.Rubber), Placement.Worn);
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Knife)).Before(_b.Verbs.Wear!, WearKnifeHandler);
        _b.On(T(SoftpornIds.Knife)).Before(_cut, WearKnifeHandler);

        _b.On(T(SoftpornIds.Passcard)).Before(_b.Verbs.Wear!, ctx =>
        {
            if (!ctx.Carrying(T(SoftpornIds.Passcard)))
            {
                ctx.Say("I don't have it!!");
                return VerbResult.Done;
            }
            if (!ctx.InRoom(DiscoEntrance))
            {
                ctx.Say("Not yet but maybe later!");
                return VerbResult.Done;
            }
            ctx.Say("I show my passcard and the door opens");
            ctx.Set(_doorWestOpen, true);
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Rope)).Before(_b.Verbs.Wear!, ctx =>
        {
            if (!ctx.Carrying(T(SoftpornIds.Rope)))
            {
                ctx.Say("I don't have it!!");
                return VerbResult.Done;
            }
            if (!ctx.InRoom(HookerBalcony))
            {
                ctx.Say("Not yet but maybe later!");
                return VerbResult.Done;
            }
            ctx.Set(_ropeInUse, true);
            ctx.PlaceHere(T(SoftpornIds.Rope));
            ctx.Say("You tie the safety rope to the balcony");
            return VerbResult.Done;
        });
    }

    private VerbResult WearKnifeHandler(VerbContext ctx)
    {
        ctx.Say("Let me see if I still have the knife!");
        Thread.Sleep(600);
        if (!ctx.Carrying(T(SoftpornIds.Knife)))
        {
            ctx.Say("I don't have it!!");
            return VerbResult.Done;
        }
        if (ctx.Get(_tiedToBed))
        {
            ctx.Say("I do and it worked! Thanks!");
            ctx.Set(_tiedToBed, false);
            return VerbResult.Done;
        }
        ctx.Say("Samurai sex fiend!!!!!!!!!!!!!!!!!!!");
        Thread.Sleep(600);
        ctx.Say("I stab myself in extacy!");
        Purgatory(ctx);
        return VerbResult.Done;
    }

    private void DefinePhone()
    {
        // Call/Answer handlers are registered on the verbs; phone logic lives in CallHandler/AnswerHandler.
    }

    private void DefineGarden()
    {
        _b.On(T(SoftpornIds.Sink)).Before(_b.Verbs.SwitchOn!, ctx =>
        {
            ctx.Set(_waterOn, true);
            ctx.Say("Water is running in the sink");
            ctx.PlaceHere(T(SoftpornIds.Water));
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Sink)).Before(_b.Verbs.SwitchOff!, ctx =>
        {
            ctx.Set(_waterOn, false);
            if (!ctx.Get(_pitcherFull))
            {
                Ok(ctx);
                if (ctx.LocatedIn(T(SoftpornIds.Water), ctx.CurrentRoom))
                    ctx.Remove(T(SoftpornIds.Water));
            }
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Stool)).Before(_climb, ctx =>
        {
            ctx.Set(_stoolClimbed, true);
            ctx.Say("OK");
            return VerbResult.Done;
        });
    }

    private void GrowTree(GameContext ctx)
    {
        ctx.Say("A tree sprouts!!");
        ctx.PlaceHere(T(SoftpornIds.Tree));
        RevealHere(ctx, SoftpornIds.Apple);
        if (ctx.Carrying(T(SoftpornIds.Seeds)) || ctx.LocatedIn(T(SoftpornIds.Seeds), ctx.CurrentRoom))
            ctx.Remove(T(SoftpornIds.Seeds));
        ctx.Remove(T(SoftpornIds.Water));
        ctx.Set(_pitcherFull, false);
    }

    private void DefineDeathTraps()
    {
        foreach (string npc in new[] { SoftpornIds.Blonde, SoftpornIds.Waitress, SoftpornIds.Hooker, SoftpornIds.Girl })
        {
            _b.On(T(npc)).Before(_b.Verbs.Eat!, ctx =>
            {
                SayLong(ctx, 38);
                Purgatory(ctx);
                return VerbResult.Done;
            });
        }

        _b.On(T(SoftpornIds.Pills)).Before(_b.Verbs.Eat!, ctx =>
        {
            SayLong(ctx, 56);
            Purgatory(ctx);
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Mushroom)).Before(_b.Verbs.Eat!, ctx =>
        {
            SayLong(ctx, 64);
            bool carried = ctx.Carrying(T(SoftpornIds.Mushroom));
            ctx.Remove(T(SoftpornIds.Mushroom));
            if (carried) DecrementCarry(ctx);
            Thread.Sleep(600);
            ctx.MovePlayerTo(R(SoftpornIds.Hallway));
            ctx.Say(SoftpornMessages.Text(1));
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Garbage)).Before(_b.Verbs.Eat!, ctx =>
        {
            ctx.Say("Too moldy!");
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.AppleCore)).Before(_b.Verbs.Eat!, ctx =>
        {
            ctx.Say("Too moldy!");
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Apple)).Before(_b.Verbs.Eat!, ctx =>
        {
            ctx.Say("Sorry ... not hungry!");
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Whiskey)).Before(_b.Verbs.Drink!, ctx =>
        {
            ctx.Say("This stuff is rot-gut! Give it to someone ... I don't want it.");
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Beer)).Before(_b.Verbs.Drink!, ctx =>
        {
            ctx.Say("Heh...heh...hey!!!! This stuff's OK!");
            ctx.Remove(T(SoftpornIds.Beer));
            DecrementCarry(ctx);
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Wine)).Before(_b.Verbs.Drink!, ctx =>
        {
            ctx.Say("Sour grapes....");
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Water)).Before(_b.Verbs.Drink!, ctx =>
        {
            ctx.Say("Thanks!");
            ctx.Remove(T(SoftpornIds.Water));
            ctx.Set(_pitcherFull, false);
            DecrementCarry(ctx);
            return VerbResult.Done;
        });
    }

    private void DefineNpcDrops()
    {
        _b.On(T(SoftpornIds.Radio)).Before(_listen, ctx =>
        {
            if (!ctx.Carrying(T(SoftpornIds.Radio)))
            {
                ctx.Say("Take it and put it next to my ear!");
                return VerbResult.Done;
            }
            if (ctx.Get(_radioListened))
                ctx.Say("Punk rock!!!!!");
            else
            {
                ctx.Say("An advertisement says 'Call 555-0987 for all your liquor needs!!!!'");
                ctx.Set(_radioListened, true);
            }
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Bartender)).Before(_fuck, ctx =>
        {
            ctx.Say("He jumps over the bar and kills me!!");
            Purgatory(ctx);
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Waitress)).Before(_fuck, ctx =>
        {
            ctx.Say("She kicks me in the groin and says 'Wise up - Buster!!'");
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Blonde)).Before(_fuck, ctx =>
        {
            ctx.Say("She says 'I'm working! Leave me alone!!'");
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Pimp)).Before(_fuck, ctx =>
        {
            ctx.Say("He says 'You'll never have enough money for me - fool!'.  I guess he's gay!");
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Bum)).Before(_fuck, ctx =>
        {
            ctx.Say("To do that I need vaseline!!");
            return VerbResult.Done;
        });

        _b.On(T(SoftpornIds.Businessman)).Before(_fuck, ctx =>
        {
            ctx.Say("No way!!!  You're weird!!");
            return VerbResult.Done;
        });
    }

    private static string ExtractPhoneDigits(string rawInput)
    {
        var digits = new System.Text.StringBuilder();
        foreach (char c in rawInput)
            if (char.IsDigit(c)) digits.Append(c);
        return digits.ToString();
    }

    private VerbResult HailHandler(VerbContext ctx)
    {
        if (ctx.DirectObject != T(SoftpornIds.Taxi))
        {
            ctx.Say("Who are you kidding? You're pulling at straws, fool!!");
            return VerbResult.Done;
        }

        if (ctx.CurrentRoom != R(SoftpornIds.BarStreet) &&
            ctx.CurrentRoom != R(SoftpornIds.CasinoStreet) &&
            ctx.CurrentRoom != R(SoftpornIds.DiscoStreet))
        {
            ctx.Say("I'm not in the street, fool!!");
            return VerbResult.Done;
        }

        SayLong(ctx, 36);
        string dest = ctx.PromptLine("").Trim().ToUpperInvariant();
        dest = (dest + "    ")[..4];
        Room? target = dest switch
        {
            "DISC" => R(SoftpornIds.DiscoStreet),
            "CASI" => R(SoftpornIds.CasinoStreet),
            "BAR " => R(SoftpornIds.BarStreet),
            _ => null
        };

        if (target is null || target == ctx.CurrentRoom)
        {
            ctx.Say("Huh? - Hail another!");
            return VerbResult.Done;
        }

        if (ctx.Carrying(T(SoftpornIds.Wine)))
        {
            SayLong(ctx, 58);
            ctx.Say("The idiot cab driver backs over me and kills me!!!!!!");
            Purgatory(ctx);
            return VerbResult.Done;
        }

        ctx.Say("We arrive and I get out.");
        ctx.MovePlayerTo(target);
        return VerbResult.Done;
    }

    private void DefineSmellAndShow()
    {
        _b.On(T(SoftpornIds.Passcard)).Before(_show, ShowPasscardHandler);
    }

    private VerbResult ShowPasscardHandler(VerbContext ctx)
    {
        if (!ctx.Carrying(T(SoftpornIds.Passcard)))
        {
            ctx.Say("I don't have it!!");
            return VerbResult.Done;
        }
        if (!ctx.InRoom(DiscoEntrance))
        {
            ctx.Say("Not yet but maybe later!");
            return VerbResult.Done;
        }
        ctx.Say("I show my passcard and the door opens");
        ctx.Set(_doorWestOpen, true);
        return VerbResult.Done;
    }

    private VerbResult ShowHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null) { CantDoThat(ctx); return VerbResult.Done; }
        if (ctx.DirectObject == T(SoftpornIds.Passcard)) return VerbResult.Pass;
        CantDoThat(ctx);
        return VerbResult.Done;
    }

    private VerbResult SmellHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null) { CantDoThat(ctx); return VerbResult.Done; }
        if (!ctx.LocatedIn(ctx.DirectObject, ctx.CurrentRoom) && !ctx.Carrying(ctx.DirectObject))
        {
            FindMeOne(ctx);
            return VerbResult.Done;
        }

        string msg = ctx.DirectObject.Id switch
        {
            SoftpornIds.Blonde => "Hmmm.....nice!!!!",
            SoftpornIds.Hooker => "OK, who's eating tuna fish?!?!?!",
            SoftpornIds.Toilet => "Arghhh...I'm going to puke!!!!!!",
            SoftpornIds.Plant => "Ahhh..chooo!!!!!!  I guess I'm allergic!",
            SoftpornIds.Garbage => "Yechhhhh!!!!!",
            SoftpornIds.Flowers => "Smells like perfume!!!",
            _ => "Smells OK"
        };
        ctx.Say(msg);
        return VerbResult.Done;
    }

    private VerbResult FuckHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null) { CantDoThat(ctx); return VerbResult.Done; }
        return VerbResult.Pass;
    }

    private VerbResult MarryHandler(VerbContext ctx)
    {
        if (ctx.DirectObject != T(SoftpornIds.Girl))
        {
            ctx.Say("No way, weirdo!!");
            return VerbResult.Done;
        }
        if (!ctx.LocatedIn(T(SoftpornIds.Girl), ctx.CurrentRoom))
        {
            ctx.Say("No girl!!");
            return VerbResult.Done;
        }
        if (!ctx.InRoom(R(SoftpornIds.MarriageCenter)))
        {
            ctx.Say("Not yet but maybe later!");
            return VerbResult.Done;
        }
        if (!HasMoney(ctx, 30) || !CarryingWallet(ctx))
        {
            if (!HasMoney(ctx, 20) || !CarryingWallet(ctx))
                ctx.Say("The girl says: 'But you'll need $2000 for the honeymoon suite!'");
            ctx.Say("The preacher says 'I'll need $1000 too!!'");
            return VerbResult.Done;
        }
        SayLong(ctx, 66);
        Spend(ctx, 30);
        ctx.Move(T(SoftpornIds.Girl), Placement.InRoom(HoneymoonSuite));
        ctx.Set(_marriedToGirl, true);
        return VerbResult.Done;
    }

    private VerbResult PlayHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null) { FindMeOne(ctx); return VerbResult.Done; }

        if (ctx.DirectObject == T(SoftpornIds.SlotMachines))
        {
            if (ctx.InRoom(R(SoftpornIds.Casino)))
            {
                if (HasMoney(ctx, 1) && CarryingWallet(ctx))
                    PlaySlots(ctx);
                else
                    ctx.Say("Sorry -- no money!!");
            }
            else
                ctx.Say("OK, show me your slot....");
            return VerbResult.Done;
        }

        if (ctx.DirectObject == T(SoftpornIds.Cards))
        {
            if (ctx.InRoom(R(SoftpornIds.TwentyOneRoom)))
            {
                if (HasMoney(ctx, 1) && CarryingWallet(ctx))
                    PlayBlackjack(ctx);
                else
                    ctx.Say("Sorry -- no money!!");
            }
            else
                ctx.Say("Not yet but maybe later!");
            return VerbResult.Done;
        }

        ctx.Say("Playful bugger, eh??");
        return VerbResult.Done;
    }

    private VerbResult PressHandler(VerbContext ctx)
    {
        if (ctx.DirectObject != T(SoftpornIds.Button)) return VerbResult.Pass;
        if (ctx.InRoom(Bar))
        {
            ctx.Blank();
            ctx.SayInline("A voice says 'What's the password?' (one word) ");
            string pw = ctx.PromptLine("").Trim().ToUpperInvariant();
            if (pw.StartsWith("BELLYB", StringComparison.Ordinal))
            {
                ctx.Set(_curtainOpen, true);
                ctx.Say("The curtain pulls back!!");
            }
            else
                ctx.Say("Wrong!!");
            return VerbResult.Done;
        }
        if (ctx.InRoom(R(SoftpornIds.HotelDesk)) || ctx.InRoom(R(SoftpornIds.PenthouseFoyer)))
        {
            if (ctx.LocatedIn(T(SoftpornIds.Blonde), ctx.CurrentRoom))
            {
                ctx.Say("The blonde says 'You can't go there!'");
                return VerbResult.Done;
            }
            SayLong(ctx, 37);
            if (ctx.InRoom(R(SoftpornIds.HotelDesk)))
                ctx.MovePlayerTo(R(SoftpornIds.PenthouseFoyer));
            else
                ctx.MovePlayerTo(R(SoftpornIds.HotelDesk));
            return VerbResult.Done;
        }
        ctx.Say("Not yet but maybe later!");
        return VerbResult.Done;
    }

    private VerbResult FlushHandler(VerbContext ctx) => VerbResult.Pass;

    private VerbResult InflateHandler(VerbContext ctx)
    {
        if (ctx.DirectObject != T(SoftpornIds.Doll)) { CantDoThat(ctx); return VerbResult.Done; }
        if (!ctx.Carrying(T(SoftpornIds.Doll)))
        {
            if (ctx.LocatedIn(T(SoftpornIds.Doll), ctx.CurrentRoom))
                ctx.Say("I can't unless I'm holding it close");
            else
                FindMeOne(ctx);
            return VerbResult.Done;
        }
        if (ctx.Get(_dollInflated))
        {
            ctx.Say("You've already inflated it, stupid!!");
            return VerbResult.Done;
        }
        Ok(ctx);
        ctx.Set(_dollInflated, true);
        return VerbResult.Done;
    }

    private VerbResult CallHandler(VerbContext ctx)
    {
        if (ctx.InRoom(PenthousePorch))
        {
            ctx.Say("This only takes incoming calls!!");
            return VerbResult.Done;
        }

        string number = NormalizePhone(ExtractPhoneDigits(ctx.RawInput));
        if (number.Length < 4 && ctx.LocatedIn(T(SoftpornIds.Telephone), ctx.CurrentRoom))
            number = NormalizePhone(ExtractPhoneDigits(ctx.PromptLine("Number? ")));
        if (number.Length < 4 && ctx.DirectObject == T(SoftpornIds.Telephone))
            number = NormalizePhone(ExtractPhoneDigits(ctx.PromptLine("Number? ")));
        if (number.Length < 4)
            number = NormalizePhone(ExtractPhoneDigits(ctx.PromptLine("Number? ")));

        if (number == "6969" && !ctx.Get(_called5556969))
        {
            ctx.Blank();
            ctx.Say("A voice says 'Hello, please answer the questions with one word answers:");
            ctx.Set(_girlName, TitleCaseFirst(ctx.PromptLine("What's your favorite girls name?  ").Trim()));
            ctx.Set(_girlPart, ctx.PromptLine("Name a nice part of her anatomy!  ").Trim().ToLowerInvariant());
            ctx.Set(_girlDo, ctx.PromptLine("What do you like to do with her?  ").Trim().ToLowerInvariant());
            ctx.Set(_yourPart, ctx.PromptLine("And the best part of your body?   ").Trim().ToLowerInvariant());
            ctx.Set(_yourObject, ctx.PromptLine("Finally, your favorite object?    ").Trim().ToLowerInvariant());
            ctx.Say("He hangs up!");
            ctx.Set(_called5556969, true);
            return VerbResult.Done;
        }
        if (number == "0439" && !ctx.Get(_called5550439))
        {
            SayLong(ctx, 67);
            ctx.Set(_called5550439, true);
            return VerbResult.Done;
        }
        if (number == "0987" && ctx.Get(_marriedToGirl) && !ctx.Get(_called5550987))
        {
            SayLong(ctx, 68);
            ctx.Set(_wineOrdered, true);
            ctx.Set(_called5550987, true);
            ctx.Move(T(SoftpornIds.Wine), Placement.InRoom(HoneymoonSuite));
            return VerbResult.Done;
        }

        ctx.Say("Nobody answers");
        return VerbResult.Done;
    }

    private VerbResult AnswerHandler(VerbContext ctx)
    {
        if (ctx.DirectObject != T(SoftpornIds.Telephone))
        {
            CantDoThat(ctx);
            return VerbResult.Done;
        }
        if (!ctx.Get(_telephoneRinging))
        {
            ctx.Say("It's not ringing!");
            return VerbResult.Done;
        }
        ctx.Blank();
        ctx.Say($"A girl says  'Hi honey!  This is {ctx.Get(_girlName)}. Dear, why");
        ctx.Say($"don't you forget this game and {ctx.Get(_girlDo)} with me????");
        ctx.Say($"After all, your {ctx.Get(_yourPart)} has always turned me on!!!!");
        ctx.Say($"So bring a {ctx.Get(_yourObject)} and come play with my {ctx.Get(_girlPart)}!'");
        ctx.Say("She hangs up!");
        ctx.Set(_telephoneRinging, false);
        ctx.Set(_telephoneAnswered, true);
        return VerbResult.Done;
    }

    private VerbResult ClimbHandler(VerbContext ctx)
    {
        if (ctx.DirectObject == T(SoftpornIds.Stool)) { ctx.Set(_stoolClimbed, true); ctx.Say("OK"); return VerbResult.Done; }
        return VerbResult.Pass;
    }

    private VerbResult EnterHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null) { CantDoThat(ctx); return VerbResult.Done; }

        if (ctx.DirectObject == T(SoftpornIds.Bushes))
        {
            ctx.MovePlayerTo(Garden);
            return VerbResult.Done;
        }
        if (ctx.DirectObject == T(SoftpornIds.Window))
        {
            if (ctx.Get(_windowBroken))
                ctx.MovePlayerTo(R(SoftpornIds.BrokenRoom));
            else
                ctx.Say("Not yet but maybe later!");
            return VerbResult.Done;
        }
        if (ctx.DirectObject == T(SoftpornIds.DoorWest))
        {
            if (ctx.Get(_doorWestOpen))
                ctx.MovePlayerTo(R(SoftpornIds.Disco));
            else
                ctx.Say("The door is closed");
            return VerbResult.Done;
        }
        if (ctx.DirectObject == T(SoftpornIds.Elevator))
        {
            ctx.Say("Push the button to enter the elevator");
            return VerbResult.Done;
        }

        CantDoThat(ctx);
        return VerbResult.Done;
    }

    private VerbResult ListenHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null) { CantDoThat(ctx); return VerbResult.Done; }
        if (ctx.DirectObject == T(SoftpornIds.Radio)) return VerbResult.Pass;
        if (!ctx.LocatedIn(ctx.DirectObject, ctx.CurrentRoom) && !ctx.Carrying(ctx.DirectObject))
        {
            FindMeOne(ctx);
            return VerbResult.Done;
        }
        ctx.Say("Quiet as a mouse in heat!");
        return VerbResult.Done;
    }

    private VerbResult JumpHandler(VerbContext ctx)
    {
        if (ctx.CurrentRoom == HookerBalcony || ctx.CurrentRoom == R(SoftpornIds.WindowLedge))
            FallingDown(ctx, jumped: true);
        else ctx.Say("Whoooopeeeee!!!");
        return VerbResult.Done;
    }

    private VerbResult DanceHandler(VerbContext ctx)
    {
        ctx.Blank();
        for (int i = 0; i < 3; i++)
        {
            Thread.Sleep(500);
            ctx.Say("Boogie Woogie!!!");
            Thread.Sleep(500);
            ctx.Say("Yeh Yeh Yeh!!!");
        }
        ctx.Say("I got the steps, man!!");
        return VerbResult.Done;
    }

    private VerbResult SmokeHandler(VerbContext ctx)
    {
        if (ctx.DirectObject == T(SoftpornIds.Plant))
        {
            ctx.Say("A cop beats me over the head!!!!");
            Purgatory(ctx);
            return VerbResult.Done;
        }
        CantDoThat(ctx);
        return VerbResult.Done;
    }

    private VerbResult PayHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null) { CantDoThat(ctx); return VerbResult.Done; }
        if (!ctx.LocatedIn(ctx.DirectObject, ctx.CurrentRoom))
        {
            FindMeOne(ctx);
            return VerbResult.Done;
        }

        switch (ctx.DirectObject.Id)
        {
            case SoftpornIds.Pimp:
                ctx.Say(ctx.Get(_hookerFucked)
                    ? "He says 'I don't want your money - stud!'"
                    : "Try going up -- he'll take the money then");
                break;
            case SoftpornIds.Hooker:
                ctx.Say("You already paid the Pimp, stupid!!");
                break;
            case SoftpornIds.Blonde:
            case SoftpornIds.Waitress:
            case SoftpornIds.Girl:
                ctx.Say("She yells 'I'm not a whore!!!' and kills me!");
                Purgatory(ctx);
                break;
            case SoftpornIds.Preacher:
                ctx.Say("Bring a girl here to marry -- he'll take the money then!");
                break;
            case SoftpornIds.Businessman:
                ctx.Say("He's too drunk to do business right now!");
                break;
            case SoftpornIds.Bartender:
                ctx.Say("Buy something -- he'll take the money then");
                break;
            case SoftpornIds.Dealer:
                ctx.Say("Why not play 21 instead?  You'll lose anyway, fool!");
                break;
            default:
                CantDoThat(ctx);
                break;
        }
        return VerbResult.Done;
    }

    private VerbResult CutHandler(VerbContext ctx) => WearKnifeHandler(ctx);

    private VerbResult WaterHandler(VerbContext ctx)
    {
        string upper = ctx.RawInput.ToUpperInvariant();
        if (upper.Contains(" ON", StringComparison.Ordinal) || upper.EndsWith(" ON", StringComparison.Ordinal))
            return WaterSink(ctx, on: true);
        if (upper.Contains(" OFF", StringComparison.Ordinal) || upper.EndsWith(" OFF", StringComparison.Ordinal))
            return WaterSink(ctx, on: false);

        if (!ctx.Carrying(T(SoftpornIds.Water)) && !ctx.Get(_pitcherFull))
        {
            ctx.Say("I have no water!");
            return VerbResult.Done;
        }

        if (ctx.DirectObject == T(SoftpornIds.Seeds) ||
            (ctx.DirectObject == T(SoftpornIds.Water) && ctx.IndirectObject == T(SoftpornIds.Seeds)) ||
            (ctx.InRoom(Garden) && ctx.Carrying(T(SoftpornIds.Seeds)) && ctx.DirectObject is null))
        {
            if (ctx.InRoom(Garden))
                GrowTree(ctx);
            else
            {
                ctx.Say("The seeds need better soil to grow.");
                ctx.Remove(T(SoftpornIds.Water));
                ctx.Set(_pitcherFull, false);
            }
            return VerbResult.Done;
        }

        if (ctx.DirectObject is not null)
        {
            FindMeOne(ctx);
            return VerbResult.Done;
        }

        CantDoThat(ctx);
        return VerbResult.Done;
    }

    private VerbResult WaterSink(VerbContext ctx, bool on)
    {
        if (!ctx.LocatedIn(T(SoftpornIds.Sink), ctx.CurrentRoom))
        {
            ctx.Say("Find a working sink!");
            return VerbResult.Done;
        }
        ctx.Set(_waterOn, on);
        if (on)
        {
            ctx.Say("Water is running in the sink");
            ctx.PlaceHere(T(SoftpornIds.Water));
        }
        else if (!ctx.Get(_pitcherFull))
        {
            Ok(ctx);
            if (ctx.LocatedIn(T(SoftpornIds.Water), ctx.CurrentRoom))
                ctx.Remove(T(SoftpornIds.Water));
        }
        return VerbResult.Done;
    }

    private VerbResult FillHandler(VerbContext ctx)
    {
        if (ctx.DirectObject != T(SoftpornIds.Pitcher))
        {
            CantDoThat(ctx);
            return VerbResult.Done;
        }
        if (!ctx.Carrying(T(SoftpornIds.Pitcher)))
        {
            ctx.Say("I don't have it!");
            return VerbResult.Done;
        }
        if (!ctx.LocatedIn(T(SoftpornIds.Sink), ctx.CurrentRoom))
        {
            ctx.Say("Find a working sink!!");
            return VerbResult.Done;
        }
        if (!ctx.Get(_waterOn))
        {
            ctx.Say("No water!!");
            return VerbResult.Done;
        }
        if (ctx.Get(_pitcherFull))
        {
            ctx.Say("The pithcer is already full!");
            return VerbResult.Done;
        }
        Ok(ctx);
        ctx.Set(_pitcherFull, true);
        return VerbResult.Done;
    }

    private VerbResult PourHandler(VerbContext ctx)
    {
        Thing? seeds = ctx.DirectObject == T(SoftpornIds.Seeds) ? T(SoftpornIds.Seeds)
            : ctx.IndirectObject == T(SoftpornIds.Seeds) ? T(SoftpornIds.Seeds) : null;
        bool pouringWater = ctx.DirectObject == T(SoftpornIds.Water) || ctx.IndirectObject == T(SoftpornIds.Water);
        if (!pouringWater && seeds is null)
        {
            CantDoThat(ctx);
            return VerbResult.Done;
        }
        if (!ctx.Carrying(T(SoftpornIds.Pitcher)))
        {
            ctx.Say("You have nothing to pour it with!");
            return VerbResult.Done;
        }
        if (!ctx.Get(_pitcherFull))
        {
            ctx.Say("The pitcher is empty.");
            return VerbResult.Done;
        }
        if (!ctx.InRoom(Garden))
        {
            ctx.Say("It pours into the ground.");
            ctx.Set(_pitcherFull, false);
            return VerbResult.Done;
        }

        if (seeds is null && ctx.Carrying(T(SoftpornIds.Seeds)))
            seeds = T(SoftpornIds.Seeds);

        if (seeds is null || (!ctx.Carrying(seeds) && !ctx.LocatedIn(seeds, ctx.CurrentRoom)))
        {
            ctx.Say("It pours into the ground.");
            ctx.Set(_pitcherFull, false);
            return VerbResult.Done;
        }
        GrowTree(ctx);
        return VerbResult.Done;
    }

    private VerbResult KissHandler(VerbContext ctx) { ctx.Say("Don't do that!!!!..."); return VerbResult.Done; }
    private VerbResult StabHandler(VerbContext ctx) { ctx.Say("OK - warmonger!"); Purgatory(ctx); return VerbResult.Done; }
}
