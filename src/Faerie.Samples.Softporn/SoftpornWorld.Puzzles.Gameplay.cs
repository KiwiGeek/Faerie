using Faerie.Model;
using Faerie.Runtime;
using Faerie.Verbs;

namespace Faerie.Samples.Softporn;

internal sealed partial class SoftpornWorld
{
    private bool InBarArea(GameContext ctx) =>
        ctx.CurrentRoom == Hallway || ctx.CurrentRoom == Bathroom ||
        ctx.CurrentRoom == Bar || ctx.CurrentRoom == BarStreet ||
        ctx.CurrentRoom == Backroom || ctx.CurrentRoom == Dumpster ||
        ctx.CurrentRoom == BrokenRoom || ctx.CurrentRoom == WindowLedge ||
        ctx.CurrentRoom == HookerBedroom || ctx.CurrentRoom == HookerBalcony;

    private bool InCasinoArea(GameContext ctx) =>
        ctx.CurrentRoom == CasinoStreet || ctx.CurrentRoom == MarriageCenter ||
        ctx.CurrentRoom == Casino || ctx.CurrentRoom == TwentyOneRoom ||
        ctx.CurrentRoom == Lobby || ctx.CurrentRoom == HoneymoonSuite ||
        ctx.CurrentRoom == HotelHallway || ctx.CurrentRoom == HoneymoonBalcony ||
        ctx.CurrentRoom == HotelDesk;

    private bool InDiscoArea(GameContext ctx) =>
        ctx.CurrentRoom == PhoneBooth || ctx.CurrentRoom == Disco ||
        ctx.CurrentRoom == DiscoStreet || ctx.CurrentRoom == DiscoEntrance ||
        ctx.CurrentRoom == Pharmacy;

    private bool InPenthouseArea(GameContext ctx) =>
        ctx.CurrentRoom == PenthouseFoyer || ctx.CurrentRoom == Jacuzzi ||
        ctx.CurrentRoom == Kitchen || ctx.CurrentRoom == Garden ||
        ctx.CurrentRoom == LivingRoom || ctx.CurrentRoom == PenthousePorch;

    private bool InPublic(GameContext ctx) =>
        ctx.CurrentRoom == CasinoStreet || ctx.CurrentRoom == Casino ||
        ctx.CurrentRoom == TwentyOneRoom || ctx.CurrentRoom == Lobby ||
        ctx.CurrentRoom == HotelDesk || ctx.CurrentRoom == DiscoStreet ||
        ctx.CurrentRoom == DiscoEntrance;

    private void ApplyAreaUpdates(GameContext ctx)
    {
        if (InBarArea(ctx))
        {
            MoveProp(ctx, Sign, BarStreet);
            MoveProp(ctx, Taxi, BarStreet);
            MoveProp(ctx, Button, Bar);
        }
        else if (InCasinoArea(ctx))
        {
            MoveProp(ctx, Sign, CasinoStreet);
            MoveProp(ctx, Taxi, CasinoStreet);
            MoveProp(ctx, Button, HotelDesk);
            MoveProp(ctx, Elevator, HotelDesk);
        }
        else if (InDiscoArea(ctx))
        {
            MoveProp(ctx, Sign, DiscoStreet);
            MoveProp(ctx, Taxi, DiscoStreet);
            MoveProp(ctx, Telephone, PhoneBooth);
        }
        else if (InPenthouseArea(ctx))
        {
            MoveProp(ctx, Button, PenthouseFoyer);
            MoveProp(ctx, Elevator, PenthouseFoyer);
            MoveProp(ctx, Telephone, PenthousePorch);
        }

        if (!ctx.InRoom(Bar))
            ctx.Set(_curtainOpen, false);

        if (!ctx.InRoom(DiscoEntrance))
            ctx.Set(_doorWestOpen, false);

        if (!ctx.Carrying(T(Stool)) && !ctx.LocatedIn(T(Stool), Garden))
            ctx.Set(_stoolClimbed, false);

        if (ctx.Get(_rubberWorn) && InPublic(ctx) && ctx.Random.Next(8) == 5)
        {
            ctx.Say("A passerby kills me for wearing my kinky rubber in public!");
            Purgatory(ctx);
        }
    }

    private void MoveProp(GameContext ctx, Thing thing, Room room)
    {
        if (!ctx.Carrying(thing) && ctx.RoomOf(thing) != room)
            ctx.Move(thing, Placement.InRoom(room));
    }

    private void DefineExamines()
    {
        Examine(Hooker, ctx => SayLong(ctx,
"""
Oh no!!! I paid for this?!?!?! This beast is really ugly!!!! Jeezzzz....
I hope I don't get the clap from this hooker................. Well ... she
seems to be annoyed that I haven't jumped on her yet ... go to it, stud!!!!
"""));
        Examine(Girl, ctx =>
        {
            if (ctx.InRoom(Jacuzzi)) SayLong(ctx,
"""
What a beautiful face!!! She's leaning back in the jacuzzi with her eyes
closed and seems extremely relaxed. The water is bubbling up around her...
She's so beautiful ........ a guy really could fall in love with a girl
like this. I presume her name is 'Eve' ... at least that's what the towel
next to her has embrodiered on it.
""");
            else if (ctx.InRoom(Disco) || ctx.InRoom(MarriageCenter)) SayLong(ctx,
"""
Cute and innocent!! Just the way I like my women.
Oh - this girl is great! She has a beautiful California tan ... and pert
little breasts ... a trim waist ..... and well rounded hips!!
I dream about getting this nice a girl. I hope you play this game well
enough so I can have my jolly with her!
You could make your puppet a very happy man...................
""");
            else ctx.Say("She slaps me and yells 'Pervert!!!!!'");
        });
        Examine(Blonde, ctx => SayLong(ctx,
"""
She's wearing the tightest jeans! Wow ... what a body!!!!! 36-24-35!! This
girls derriere is sensational!! And the shirt? See through - and what I see
I like!
As my eyes reluctantly roam from her body, I see bright blue eyes - and a
smile that dazzles me. I think she likes me!
"""));
        Examine(Billboard, ctx => SayLong(ctx,
"""
+---------------------------------------------------+
|        For those who desire the best:             |
|   Announcing, the most exclusive, the exciting,   |
|             the hottest spot in town,             |
|          ***************************              |
|          * SWINGING SINGLE'S DISCO *              |
|          ***************************              |
+---------------------------------------------------+
"""));
        Examine(Graffiti, ctx => SayLong(ctx,
"""
+-------------------------------------------------------------------+
|                                                                   |
|                  At my PC is where I sit                          |
|              when I feel like fondling it's bits!       I    h    |
|                                                         '    e    |
|                  C  The password is:                    d    r    |
|                  o     A                            a             |
|                  m  S  n   Bellybutton              l       f     |
|                  P  p  C                d           i       l     |
|                  e  u  I                r           k       o     |
|                  e  t  I        y       e       e       p         |
|                  t  k        e          e       c       p         |
|                  h  r        e          t       i                 |
|                  e  b        s    i     o       e                 |
|                  y  e        f    h     e       s                 |
|                     f        r    a     v       n                 |
|                  P  o        e    l     e       i                 |
|                  o  r        a    l         b                     |
|                  k  e        k          b                         |
|                  e  s                   l                         |
|                  e                                                |
+-------------------------------------------------------------------+
"""));
        Examine(Peephole, ctx =>
        {
            if (ctx.Get(_holePeeped)) ctx.Say("All windows at the hotel across the road have their curtains shut.");
            else
            {
                SayLong(ctx,
"""
Hmmmm ..... this is a Peeping Tom's paradise!!!!!
Across the road is another hotel. Aha! The curtains are open at one window!
The bathroom door opens and a girl walks out. Holy cow! Her boobs are huge -
and look at the sway as she strides across the room!
Now she's taking a large sausage-shaped object and looks at it longinly!
Damn! She shuts the curtain!
""");
                ctx.Set(_holePeeped, true);
            }
        });
        Examine(Desk, ctx =>
        {
            if (ctx.Get(_drawerOpen)) RevealHere(ctx, Newspaper);
            else ctx.Say("It's drawer is shut");
        });
        Examine(Washbasin, ctx => RevealHere(ctx, Ring));
        Examine(Garbage, ctx => RevealHere(ctx, AppleCore));
        Examine(AppleCore, ctx => RevealHere(ctx, Seeds));
        Examine(Ashtray, ctx => RevealHere(ctx, Passcard));
        Examine(Rack, ctx => RevealHere(ctx, Magazine));
        Examine(Tree, ctx => RevealHere(ctx, Apple));
        Examine(Plant, ctx =>
        {
            if (IsOffstage(ctx, T(Bushes)))
            {
                ctx.Say("There's a group of bushes behind it!!");
                ctx.PlaceHere(T(Bushes));
            }
            else CantDoThat(ctx);
        });
        Examine(Newspaper, ctx =>
        {
            if (ctx.Carrying(T(Newspaper))) SayLong(ctx,
"""
It's the gambler's gazette!!
There's an article here which tells how to activate the games at the
Adventurer's Hotel. It says that Blackjack can be played by entering
'Play 21'. The slot machines can be started with 'Play Slot'.
""");
            else ctx.Say("I don't have it!!");
        });
        Examine(Magazine, ctx =>
        {
            if (ctx.Carrying(T(Magazine))) SayLong(ctx,
"""
Hmmmm ..... an interesting magazine with a nice centerfold!
The feature article is about how to pick up an innocent girl at a disco.
It says - 'Shower her with presents. Dancing won't hurt either. And wine
is always good to get thing moving!'
""");
            else ctx.Say("I don't have it!!");
        });
        Examine(Mirror, ctx => ctx.Say("There's a pervert looking back at me!!"));
        Examine(Toilet, ctx => ctx.Say("Hasn't been flushed in ages! Stinks!!!"));
        Examine(Businessman, ctx => ctx.Say("He looks like a whiskey drinker to me!!"));
        Examine(Button, ctx => ctx.Say("Says Push."));
        Examine(Bartender, ctx => ctx.Say("He's waiting for me to buy something!"));
        Examine(Pimp, ctx => ctx.Say("He's wearing a button proclaiming -- Support your local Pimp, gimme $2000!!!"));
        _softporn.On(T(Tv)).Before(_softporn.Verbs.Examine!, ctx =>
        {
            if (!ctx.Carrying(T(ControlUnit)))
            {
                ctx.Say("To watch TV, I need the remote control unit!!");
                return VerbResult.Done;
            }
            WatchTv(ctx);
            return VerbResult.Done;
        });
        Examine(SlotMachines, ctx => ctx.Say("Playing them might be more fun...."));
        Examine(Bum, ctx => ctx.Say("He grumbles -- I'll tell you a story for a bottle of wine....."));
        Examine(DoorWest, ctx =>
        {
            if (ctx.Get(_doorWestOpen)) ctx.Say("The door is open");
            else
            {
                ctx.Say("The sign on the door says ");
                ctx.Say("'Entry by showing passcard - Club members and their guests only!'");
            }
        });
        Examine(Telephone, ctx =>
        {
            if (ctx.InRoom(PhoneBooth))
                ctx.Say("A number is there - Call 555-6969 for a good time!");
            else
                ctx.Say("I see nothing special");
        });
        Examine(Wallet, ctx =>
        {
            if (Money(ctx) > 0) ctx.Say($"I've got ${Money(ctx)}00 in it.");
            else ctx.Say("It's empty!");
        });
        Examine(Rubber, ctx =>
        {
            if (!ctx.Carrying(T(Rubber))) { ctx.Say("I don't have it!!"); return; }
            ctx.Say($"It's {ctx.Get(_rubberColor)}, {ctx.Get(_rubberFlavor)}-flavored, " +
                    $"{ctx.Get(_rubberLubricated)}, and {ctx.Get(_rubberRibbed)}");
        });
        Examine(Pills, ctx =>
        {
            ctx.Say("The label on the bottle says");
            ctx.Say("'Want to drive someone crazy with lust?? Try this!!!!'");
        });
        Examine(Closet, ctx =>
        {
            if (ctx.Get(_closetOpen))
            {
                if (ctx.LocatedIn(T(Doll), ctx.CurrentRoom))
                    ctx.Say("It's open");
                RevealHere(ctx, Doll);
            }
            else
                ctx.Say("It's closed");
        });
        Examine(Sink, ctx => ctx.Say("The sign over the sink says 'Water on or off to operate'"));
        Examine(Elevator, ctx => ctx.Say("It's doors are closed"));
        Examine(Dealer, ctx => ctx.Say("He's waiting for me to play"));
        Examine(Cabinet, ctx =>
        {
            if (!ctx.Get(_stoolClimbed))
            {
                ctx.Say("I see nothing special");
                return;
            }
            if (ctx.Get(_cabinetOpen))
            {
                if (ctx.LocatedIn(T(Pitcher), ctx.CurrentRoom))
                    ctx.Say("It's open");
                RevealHere(ctx, Pitcher);
            }
            else
                ctx.Say("It's closed");
        });
        Examine(Bushes, ctx => ctx.Say("Entering them would be kinky!!!!"));
        Examine(Sign, ctx => ctx.Say("It says 'Hail taxi here'"));
        Examine(Flowers, ctx => ctx.Say("They look beautiful!!!"));
        Examine(Doll, ctx =>
        {
            if (ctx.Get(_dollInflated))
                ctx.Say("It's inflated");
            else
                ctx.Say("It's rolled up in a little ball");
        });
        Examine(Pitcher, ctx =>
        {
            if (ctx.Get(_pitcherFull))
                ctx.Say("It's full of water");
            else
                ctx.Say("It's empty");
        });
        Examine(Curtain, ctx => ctx.Say("It's on the east wall"));
        Examine(Waitress, ctx => ctx.Say("She ignores you!"));
        Examine(Radio, ctx => ctx.Say("Maybe I should listen..."));
        Examine(Window, ctx => ctx.Say("The window looks into a room. But I can't see too much from here."));
    }

    private void Examine(Thing thing, Action<GameContext> handler)
    {
        thing.OnExamine = handler;
    }

    private void DefineContainers()
    {
        _softporn.On(T(Desk)).Before(_softporn.Verbs.Open!, ctx =>
        {
            if (ctx.Get(_drawerOpen))
            {
                ctx.Say("It's already open!!");
                return VerbResult.Done;
            }
            ctx.Set(_drawerOpen, true);
            return VerbResult.Done;
        });
        _softporn.On(T(Desk)).Before(_softporn.Verbs.Close!, ctx =>
        {
            if (!ctx.Get(_drawerOpen))
            {
                ctx.Say("It's already closed!!");
                return VerbResult.Done;
            }
            ctx.Set(_drawerOpen, false);
            ctx.Remove(T(Newspaper));
            return VerbResult.Done;
        });

        _softporn.On(T(Closet)).Before(_softporn.Verbs.Open!, ctx =>
        {
            if (ctx.Get(_closetOpen))
            {
                ctx.Say("It's already open!!");
                return VerbResult.Done;
            }
            ctx.Set(_closetOpen, true);
            RevealHere(ctx, Doll);
            return VerbResult.Done;
        });

        _softporn.On(T(Cabinet)).Before(_softporn.Verbs.Open!, ctx =>
        {
            if (!ctx.Get(_stoolClimbed) && !ctx.Carrying(T(Stool)))
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
            RevealHere(ctx, Pitcher);
            return VerbResult.Done;
        });

        _softporn.On(T(Closet)).Before(_softporn.Verbs.Close!, ctx =>
        {
            if (!ctx.Get(_closetOpen))
            {
                ctx.Say("It's already closed!!");
                return VerbResult.Done;
            }
            ctx.Set(_closetOpen, false);
            ctx.Remove(T(Doll));
            return VerbResult.Done;
        });

        _softporn.On(T(Cabinet)).Before(_softporn.Verbs.Close!, ctx =>
        {
            if (!ctx.Get(_stoolClimbed)) { ctx.Say("I can't reach it!"); return VerbResult.Done; }
            if (!ctx.Get(_cabinetOpen))
            {
                ctx.Say("It's already closed!!");
                return VerbResult.Done;
            }
            ctx.Set(_cabinetOpen, false);
            ctx.Remove(T(Pitcher));
            return VerbResult.Done;
        });

        _softporn.On(T(DoorWest)).Before(_softporn.Verbs.Close!, ctx =>
        {
            if (!ctx.Get(_doorWestOpen))
            {
                ctx.Say("It's already closed!!");
                return VerbResult.Done;
            }
            ctx.Set(_doorWestOpen, false);
            return VerbResult.Done;
        });

        _softporn.On(T(DoorWest)).Before(_softporn.Verbs.Open!, ctx =>
        {
            if (ctx.Get(_doorWestOpen))
            {
                ctx.Say("It's already open!!");
                return VerbResult.Done;
            }
            ctx.Say("A voice asks 'Passcard?' I search in my pockets and...");
            if (ctx.Carrying(T(Passcard)))
            {
                ctx.Say("I have it! The door opens!");
                ctx.Set(_doorWestOpen, true);
            }
            else
                ctx.Say("I don't have it!!");
            return VerbResult.Done;
        });

        _softporn.On(T(Window)).Before(_break, ctx =>
        {
            ctx.Say("Let me see if I have a hammer");
            Thread.Sleep(400);
            if (!ctx.Carrying(T(Hammer)))
            {
                ctx.Say("I don't have it!!");
                return VerbResult.Done;
            }
            ctx.Say("The window smashes to pieces");
            ctx.Set(_windowBroken, true);
            return VerbResult.Done;
        });

        _softporn.On(T(Window)).Before(_softporn.Verbs.Open!, ctx =>
        {
            ctx.Say("Won't budge");
            return VerbResult.Done;
        });

        _softporn.On(T(Curtain)).Before(_softporn.Verbs.Open!, ctx =>
        {
            ctx.Say("It seems to be remotely controlled");
            return VerbResult.Done;
        });

        _softporn.On(T(Elevator)).Before(_softporn.Verbs.Open!, ctx =>
        {
            ctx.Say("Push the button to open elevator");
            return VerbResult.Done;
        });

        _softporn.On(T(Toilet)).Before(_flush, ctx =>
        {
            SayLong(ctx,
"""
Ok, here goes .......
Oh no!!!! It's overflowing!!!!!!!!!!!!!!!!!
It's filling the room with gross sewage!!!!!
""");
            ctx.Say("I'm dead from the germs!!");
            Purgatory(ctx);
            return VerbResult.Done;
        });
    }

    private void DefineCommerce()
    {
        _softporn.On(T(Rubber)).Before(_buy, BuyAtPharmacy);
        _softporn.On(T(Magazine)).Before(_buy, BuyAtPharmacy);
    }

    private VerbResult BuyHandler(VerbContext ctx)
    {
        return ctx.DirectObject == T(Wine)
            ? BuyWineFromWaitress(ctx)
            : ctx.DirectObject is { } item &&
            (item == T(Beer) || item == T(Whiskey))
            ? BuyFromBartender(ctx)
            : VerbResult.Pass;
    }

    private VerbResult BuyFromBartender(VerbContext ctx)
    {
        if (!CarryingWallet(ctx) || !HasMoney(ctx, 1))
        {
            ctx.Say("Sorry -- no money!!");
            return VerbResult.Done;
        }

        if (ctx.DirectObject is not { } item ||
            (item != T(Beer) && item != T(Whiskey)))
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

        if (!ctx.InRoom(Disco))
        {
            ctx.Say("Not yet but maybe later!");
            return VerbResult.Done;
        }

        if (!IsOffstage(ctx, T(Wine)))
        {
            ctx.Say("All out!");
            return VerbResult.Done;
        }

        ctx.Say("The waitress takes $100 and says she'll return");
        Thread.Sleep(3000);
        ctx.Say("Poor service!!!");
        Thread.Sleep(2000);
        Spend(ctx, 1);
        ctx.PlaceHere(T(Wine));
        return VerbResult.Done;
    }

    private VerbResult BuyAtPharmacy(VerbContext ctx)
    {
        if (!CarryingWallet(ctx) || !HasMoney(ctx, 1))
        {
            ctx.Say("Sorry -- no money!!");
            return VerbResult.Done;
        }

        if (!ctx.InRoom(Pharmacy) || ctx.DirectObject is not { } item)
        {
            ctx.Say("Not yet but maybe later!");
            return VerbResult.Done;
        }

        if (!ctx.LocatedIn(item, ctx.CurrentRoom))
        {
            ctx.Say("Sorry!!! --- sold out!");
            return VerbResult.Done;
        }

        if (item == T(Rubber))
            BuyRubber(ctx);
        else
            ctx.Say("He takes $100 and gives me the magazine");

        Spend(ctx, 1);
        ctx.Take(item);
        return VerbResult.Done;
    }

    private void DefineTakeRules()
    {
        T(Candy).OnTake = ctx =>
        {
            if (ctx.InRoom(HookerBedroom) && !ctx.Get(_hookerFucked))
            {
                ctx.Say("The Hooker says: 'Don't take it ... do me first!!'");
                return true;
            }

            return false;
        };

        T(Rubber).OnTake = ctx =>
        {
            if (ctx.InRoom(Pharmacy))
            {
                ctx.Say("The man says 'Shoplifter!!' and shoots me");
                Purgatory(ctx);
                return true;
            }

            return false;
        };

        T(Magazine).OnTake = ctx =>
        {
            if (ctx.InRoom(Pharmacy))
            {
                ctx.Say("The man says 'Shoplifter!!' and shoots me");
                Purgatory(ctx);
                return true;
            }

            return false;
        };

        T(Water).OnTake = ctx =>
        {
            if (!ctx.Carrying(T(Pitcher)))
            {
                ctx.Say("Get me the pitcher so I don't spill it!");
                return true;
            }

            ctx.Set(_pitcherFull, true);
            return false;
        };

        T(Rope).OnTake = ctx =>
        {
            if (ctx.Get(_ropeInUse))
            {
                ctx.Say("It is tied to the balcony");
                return true;
            }

            return false;
        };

        T(Pitcher).OnTake = ctx =>
        {
            if (ctx.Get(_pitcherFull) && ctx.LocatedIn(T(Water), ctx.CurrentRoom))
                ctx.Take(T(Water));
            return false;
        };
    }

    private void DefineDropGifts()
    {
        T(Candy).OnDrop = ctx =>
        {
            if (!ctx.InRoom(Disco) || !ctx.LocatedIn(T(Girl), ctx.CurrentRoom))
                return false;
            ctx.Say("She smiles and eats a couple!!");
            ctx.Set(_candyGiven, true);
            ctx.Remove(T(Candy));
            CheckGirlCourtship(ctx);
            return true;
        };

        T(Flowers).OnDrop = ctx =>
        {
            if (!ctx.InRoom(Disco) || !ctx.LocatedIn(T(Girl), ctx.CurrentRoom))
                return false;
            ctx.Say("She blushes profusely and puts them in her hair!");
            ctx.Set(_flowersGiven, true);
            ctx.Remove(T(Flowers));
            CheckGirlCourtship(ctx);
            return true;
        };

        T(Ring).OnDrop = ctx =>
        {
            if (!ctx.InRoom(Disco) || !ctx.LocatedIn(T(Girl), ctx.CurrentRoom))
                return false;
            ctx.Say("She blushes and puts it in her purse.");
            ctx.Set(_ringGiven, true);
            ctx.Remove(T(Ring));
            CheckGirlCourtship(ctx);
            return true;
        };

        T(Wine).OnDrop = ctx => TryGiveWineToBum(ctx);

        _softporn.On(T(Wine)).Before(_softporn.Verbs.Give!, ctx =>
        {
            if (ctx.IndirectObject != T(Bum))
                return VerbResult.Pass;
            return TryGiveWineToBum(ctx) ? VerbResult.Done : VerbResult.Pass;
        });

        T(Whiskey).OnDrop = ctx =>
        {
            if (ctx.LocatedIn(T(Businessman), ctx.CurrentRoom) &&
                IsOffstage(ctx, T(ControlUnit)))
            {
                ctx.Say("The guy gives me a TV controller!!");
                ctx.PlaceHere(T(ControlUnit));
                return false;
            }

            return false;
        };

        T(Pills).OnDrop = ctx =>
        {
            if (ctx.LocatedIn(T(Blonde), ctx.CurrentRoom))
            {
                SayLong(ctx,
"""
The blonde looks at the pills and says 'Thanks!!! I love this stuff!'
She takes a pill .... her nipples start to stand up! Wow!!!!
She's breathing heavily .... I hope she rapes me!!!!!
She says 'So long!!! I'm going to see my boyfriend!'
She disappears down the stairs.....
""");
                ctx.Remove(T(Blonde));
                ctx.Remove(T(Pills));
                return true;
            }

            return false;
        };

        T(Apple).OnDrop = ctx =>
        {
            if (ctx.InRoom(Jacuzzi) && ctx.LocatedIn(T(Girl), ctx.CurrentRoom))
            {
                SayLong(ctx,
"""
She takes the apple and raises it to her mouth. With an outrageously innocent
look she takes a small bite out of it. A smile comes across her face! She's
really starting to look quite sexy!!!! She winks and lays back in the jacuzzi
""");
                ctx.Set(_appleGiven, true);
                ctx.Remove(T(Apple));
                return true;
            }

            return false;
        };

        T(Rubber).OnDrop = ctx => { ctx.Set(_rubberWorn, false); return false; };
    }

    private void CheckGirlCourtship(GameContext ctx)
    {
        if (!ctx.Get(_candyGiven) || !ctx.Get(_flowersGiven) || !ctx.Get(_ringGiven)) return;
        ctx.Say("She says: 'See you at the Marriage Center!!'");
        ctx.Move(T(Girl), Placement.InRoom(MarriageCenter));
    }

    private void DefineRomance()
    {
        _softporn.On(T(Hooker)).Before(_fuck, ctx =>
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
            SayLong(ctx,
"""
It's a good thing I was wearing that rubber!!!!!
She was OK - but really ... can't you do better that this??
The score is now '1' out of a possible of '3' ... so congratulations!!!!!
Well ... go to it - you stud!!! Find me another girl!
""");
            return VerbResult.Done;
        });

        _softporn.On(T(Doll)).Before(_fuck, ctx =>
        {
            if (!ctx.Carrying(T(Doll)))
            {
                ctx.Say("I can't unless I'm holding it close");
                return VerbResult.Done;
            }
            if (!ctx.Get(_dollInflated))
            {
                ctx.Say("Inflate it first -- stupid!!!");
                return VerbResult.Done;
            }
            SayLong(ctx,
"""
Oh boy!!!! - it's got 3 spots to try!!! I thrust into the doll - kinky .. eh?
I start to increase my tempo ... faster and faster I go!!!!
Suddenly there's a flatulent noise and the doll becomes a popped balloon
soaring around the room! It flies out of the room and disappears!
""");
            ctx.Remove(T(Doll));
            DecrementCarry(ctx);
            return VerbResult.Done;
        });

        _softporn.On(T(Girl)).Before(_fuck, ctx =>
        {
            if (ctx.InRoom(HoneymoonSuite) && ctx.Get(_wineOrdered))
            {
                SayLong(ctx,
"""
She says 'Lay down honey - let me give you a special surprise!!!'
I lay down and she says 'OK - now close your eyes'.
I close my eyes and she starts to go to work on me.......
I'm really enjoying myself when suddenly she ties me to the bed!!!!
Then she says 'So long - Turkey!' and runs out of the room!!!
Well - the score is now '2' out of a possible '3'.............
.......but I'm also tied to the bed and can't move!!!!!!!
""");
                ctx.State.Score++;
                ctx.Set(_girl2Fucked, true);
                ctx.Set(_tiedToBed, true);
                ctx.Move(T(Girl), Placement.InRoom(Jacuzzi));
                ctx.PlaceHere(T(Rope));
                return VerbResult.Done;
            }
            if (ctx.InRoom(Jacuzzi) && ctx.Get(_appleGiven))
            {
                ctx.State.Score++;
                string ending =
"""
She hops out of the tub - the steam rising from her skin ... her body is
the best looking I've ever seen!!!
Then she comes up to me and gives the best time of my life!!!
Well ... I guess that's it! As your puppet in this game I thank you for
the pleasure you have brought me ... so long ... I've got to get back to
my new girl here! Keep it up!
""";
                SayLong(ctx, ending);
                ctx.EndGame(true);
                return VerbResult.Done;
            }
            if (ctx.InRoom(HoneymoonSuite))
            {
                ctx.Say("She says  'Get me wine!!!  I'm nervous!!'");
                return VerbResult.Done;
            }
            return VerbResult.Pass;
        });

        _softporn.On(T(Rubber)).Before(_softporn.Verbs.TakeOff!, ctx =>
        {
            ctx.Set(_rubberWorn, false);
            return VerbResult.Pass;
        });

        _softporn.On(T(Rubber)).Before(_softporn.Verbs.Wear!, ctx =>
        {
            ctx.Say("It tickles!!");
            ctx.Set(_rubberWorn, true);
            ctx.Move(T(Rubber), Placement.Worn);
            return VerbResult.Done;
        });

        _softporn.On(T(Knife)).Before(_softporn.Verbs.Wear!, WearKnifeHandler);
        _softporn.On(T(Knife)).Before(_cut, WearKnifeHandler);

        _softporn.On(T(Passcard)).Before(_softporn.Verbs.Wear!, ctx =>
        {
            if (!ctx.Carrying(T(Passcard)))
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

        _softporn.On(T(Rope)).Before(_softporn.Verbs.Wear!, ctx =>
        {
            if (!ctx.Carrying(T(Rope)))
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
            ctx.PlaceHere(T(Rope));
            ctx.Say("You tie the safety rope to the balcony");
            return VerbResult.Done;
        });
    }

    private VerbResult WearKnifeHandler(VerbContext ctx)
    {
        ctx.Say("Let me see if I still have the knife!");
        Thread.Sleep(600);
        if (!ctx.Carrying(T(Knife)))
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
        _softporn.On(T(Sink)).Before(_softporn.Verbs.SwitchOn!, ctx =>
        {
            ctx.Set(_waterOn, true);
            ctx.Say("Water is running in the sink");
            ctx.PlaceHere(T(Water));
            return VerbResult.Done;
        });

        _softporn.On(T(Sink)).Before(_softporn.Verbs.SwitchOff!, ctx =>
        {
            ctx.Set(_waterOn, false);
            if (!ctx.Get(_pitcherFull))
            {
                Ok(ctx);
                if (ctx.LocatedIn(T(Water), ctx.CurrentRoom))
                    ctx.Remove(T(Water));
            }
            return VerbResult.Done;
        });

        _softporn.On(T(Stool)).Before(_climb, ctx =>
        {
            ctx.Set(_stoolClimbed, true);
            ctx.Say("OK");
            return VerbResult.Done;
        });
    }

    private void GrowTree(GameContext ctx)
    {
        ctx.Say("A tree sprouts!!");
        ctx.PlaceHere(T(Tree));
        RevealHere(ctx, Apple);
        if (ctx.Carrying(T(Seeds)) || ctx.LocatedIn(T(Seeds), ctx.CurrentRoom))
            ctx.Remove(T(Seeds));
        ctx.Remove(T(Water));
        ctx.Set(_pitcherFull, false);
    }

    private void DefineDeathTraps()
    {
        foreach (Thing npc in new[] { Blonde, Waitress, Hooker, Girl })
        {
            _softporn.On(npc).Before(_softporn.Verbs.Eat!, ctx =>
            {
                SayLong(ctx,
"""
She says 'Me first!!!!'
She takes my throbbing tool into her mouth!!!
She starts going to work ... feels so good!!!!!!
Then she smiles and bites it off! She says 'No oral sex in this game!!!!!!'
Suffer!!!!!
""");
                Purgatory(ctx);
                return VerbResult.Done;
            });
        }

        _softporn.On(T(Pills)).Before(_softporn.Verbs.Eat!, ctx =>
        {
            SayLong(ctx,
"""
This stuff is good! I'm breathing heavily - I've never been this horny!!!!!
I've just got to do something..............
Ah ... there goes a female german shepard!! That gives me an idea!.......
Kinky dog!!!! Chewed me to death!!!!
""");
            Purgatory(ctx);
            return VerbResult.Done;
        });

        _softporn.On(T(Mushroom)).Before(_softporn.Verbs.Eat!, ctx =>
        {
            SayLong(ctx,
"""
Holy Cow! Psychedelic!!!!
Pretty colors appear and I'm elsewhere!
""");
            bool carried = ctx.Carrying(T(Mushroom));
            ctx.Remove(T(Mushroom));
            if (carried) DecrementCarry(ctx);
            Thread.Sleep(600);
            ctx.MovePlayerTo(Hallway);
            ctx.Say(
"""
I'm in a dimly lit hallway.
The paint is peeling off the walls and the floor hasn't been cleaned in months.
Cockroaches run across the floor - jumping as the loosely installed lightbulb
crackles and flickers.
An old desk sits pushed against the wall. A businessman sits on a broken chair
next to the desk. Seems kind of drunk!
""");
            return VerbResult.Done;
        });

        _softporn.On(T(Garbage)).Before(_softporn.Verbs.Eat!, ctx =>
        {
            ctx.Say("Too moldy!");
            return VerbResult.Done;
        });

        _softporn.On(T(AppleCore)).Before(_softporn.Verbs.Eat!, ctx =>
        {
            ctx.Say("Too moldy!");
            return VerbResult.Done;
        });

        _softporn.On(T(Apple)).Before(_softporn.Verbs.Eat!, ctx =>
        {
            ctx.Say("Sorry ... not hungry!");
            return VerbResult.Done;
        });

        _softporn.On(T(Whiskey)).Before(_softporn.Verbs.Drink!, ctx =>
        {
            ctx.Say("This stuff is rot-gut! Give it to someone ... I don't want it.");
            return VerbResult.Done;
        });

        _softporn.On(T(Beer)).Before(_softporn.Verbs.Drink!, ctx =>
        {
            ctx.Say("Heh...heh...hey!!!! This stuff's OK!");
            ctx.Remove(T(Beer));
            DecrementCarry(ctx);
            return VerbResult.Done;
        });

        _softporn.On(T(Wine)).Before(_softporn.Verbs.Drink!, ctx =>
        {
            ctx.Say("Sour grapes....");
            return VerbResult.Done;
        });

        _softporn.On(T(Water)).Before(_softporn.Verbs.Drink!, ctx =>
        {
            ctx.Say("Thanks!");
            ctx.Remove(T(Water));
            ctx.Set(_pitcherFull, false);
            DecrementCarry(ctx);
            return VerbResult.Done;
        });
    }

    private void DefineNpcDrops()
    {
        _softporn.On(T(Radio)).Before(_listen, ctx =>
        {
            if (!ctx.Carrying(T(Radio)))
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

        _softporn.On(T(Bartender)).Before(_fuck, ctx =>
        {
            ctx.Say("He jumps over the bar and kills me!!");
            Purgatory(ctx);
            return VerbResult.Done;
        });

        _softporn.On(T(Waitress)).Before(_fuck, ctx =>
        {
            ctx.Say("She kicks me in the groin and says 'Wise up - Buster!!'");
            return VerbResult.Done;
        });

        _softporn.On(T(Blonde)).Before(_fuck, ctx =>
        {
            ctx.Say("She says 'I'm working! Leave me alone!!'");
            return VerbResult.Done;
        });

        _softporn.On(T(Pimp)).Before(_fuck, ctx =>
        {
            ctx.Say("He says 'You'll never have enough money for me - fool!'.  I guess he's gay!");
            return VerbResult.Done;
        });

        _softporn.On(T(Bum)).Before(_fuck, ctx =>
        {
            ctx.Say("To do that I need vaseline!!");
            return VerbResult.Done;
        });

        _softporn.On(T(Businessman)).Before(_fuck, ctx =>
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
        if (ctx.DirectObject != T(Taxi))
        {
            ctx.Say("Who are you kidding? You're pulling at straws, fool!!");
            return VerbResult.Done;
        }

        if (ctx.CurrentRoom != BarStreet &&
            ctx.CurrentRoom != CasinoStreet &&
            ctx.CurrentRoom != DiscoStreet)
        {
            ctx.Say("I'm not in the street, fool!!");
            return VerbResult.Done;
        }

        SayLong(ctx,
"""
A taxi pulls up and screeches to a halt! I get in the back and sit down.
A sign says 'We service 3 destinations. When asked, please specify -
Disco ... Casino ... or Bar'.
The driver turns and asks: 'Where to Mac???'
""");
        string dest = ctx.PromptLine("").Trim().ToUpperInvariant();
        dest = (dest + "    ")[..4];
        Room? target = dest switch
        {
            "DISC" => DiscoStreet,
            "CASI" => CasinoStreet,
            "BAR " => BarStreet,
            _ => null
        };

        if (target is null || target == ctx.CurrentRoom)
        {
            ctx.Say("Huh? - Hail another!");
            return VerbResult.Done;
        }

        if (ctx.Carrying(T(Wine)))
        {
            SayLong(ctx,
"""
The driver looks at me and says 'Hey!! What's that you got ... wine????'
He grabs the bottle and guzzles the wine down!!!!!!!!!!!!!!
Oh no!!!! He's swerving towards a huge truck!!!!!
I grab the wheel...............................
We struggle....................................
The truck just misses us!!!!!!!
""");
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
        _softporn.On(T(Passcard)).Before(_show, ShowPasscardHandler);
    }

    private VerbResult ShowPasscardHandler(VerbContext ctx)
    {
        if (!ctx.Carrying(T(Passcard)))
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
        if (ctx.DirectObject == T(Passcard)) return VerbResult.Pass;
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

        Thing obj = ctx.DirectObject;
        string msg =
            obj == Blonde ? "Hmmm.....nice!!!!" :
            obj == Hooker ? "OK, who's eating tuna fish?!?!?!" :
            obj == Toilet ? "Arghhh...I'm going to puke!!!!!!" :
            obj == Plant ? "Ahhh..chooo!!!!!!  I guess I'm allergic!" :
            obj == Garbage ? "Yechhhhh!!!!!" :
            obj == Flowers ? "Smells like perfume!!!" :
            "Smells OK";
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
        if (ctx.DirectObject != T(Girl))
        {
            ctx.Say("No way, weirdo!!");
            return VerbResult.Done;
        }
        if (!ctx.LocatedIn(T(Girl), ctx.CurrentRoom))
        {
            ctx.Say("No girl!!");
            return VerbResult.Done;
        }
        if (!ctx.InRoom(MarriageCenter))
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
        SayLong(ctx,
"""
OK.
Why am I doing this?
The Preacher takes $1000 and winks!
The girl grabs $2000 and says:
'Meet me at the Honeymoon Suite!! I've got connections to get a room there!!'
""");
        Spend(ctx, 30);
        ctx.Move(T(Girl), Placement.InRoom(HoneymoonSuite));
        ctx.Set(_marriedToGirl, true);
        return VerbResult.Done;
    }

    private VerbResult PlayHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null) { FindMeOne(ctx); return VerbResult.Done; }

        if (ctx.DirectObject == T(SlotMachines))
        {
            if (ctx.InRoom(Casino))
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

        if (ctx.DirectObject == T(Cards))
        {
            if (ctx.InRoom(TwentyOneRoom))
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
        if (ctx.DirectObject != T(Button)) return VerbResult.Pass;
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
        if (ctx.InRoom(HotelDesk) || ctx.InRoom(PenthouseFoyer))
        {
            if (ctx.LocatedIn(T(Blonde), ctx.CurrentRoom))
            {
                ctx.Say("The blonde says 'You can't go there!'");
                return VerbResult.Done;
            }
            SayLong(ctx,
"""
The elevator door open ... I get in.
As the doors close the music starts playing - it's the usual elevator stuff
... boring! We start to move ... after a few seconds the elevator stops.
The doors open and I get out.
""");
            if (ctx.InRoom(HotelDesk))
                ctx.MovePlayerTo(PenthouseFoyer);
            else
                ctx.MovePlayerTo(HotelDesk);
            return VerbResult.Done;
        }
        ctx.Say("Not yet but maybe later!");
        return VerbResult.Done;
    }

    private VerbResult FlushHandler(VerbContext ctx) => VerbResult.Pass;

    private VerbResult InflateHandler(VerbContext ctx)
    {
        if (ctx.DirectObject != T(Doll)) { CantDoThat(ctx); return VerbResult.Done; }
        if (!ctx.Carrying(T(Doll)))
        {
            if (ctx.LocatedIn(T(Doll), ctx.CurrentRoom))
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
        if (number.Length < 4 && ctx.LocatedIn(T(Telephone), ctx.CurrentRoom))
            number = NormalizePhone(ExtractPhoneDigits(ctx.PromptLine("Number? ")));
        if (number.Length < 4 && ctx.DirectObject == T(Telephone))
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
            SayLong(ctx,
"""
Hi there!!! This is Chuck (the author of this absurd game). If you're a
voluptous blonde who's looking for a good time, then call me immedeatley!!
""");
            ctx.Set(_called5550439, true);
            return VerbResult.Done;
        }
        if (number == "0987" && ctx.Get(_marriedToGirl) && !ctx.Get(_called5550987))
        {
            SayLong(ctx,
"""
A voice answers and says 'Wine for the nervous newlyweds! Coming right up!!'
""");
            ctx.Set(_wineOrdered, true);
            ctx.Set(_called5550987, true);
            ctx.Move(T(Wine), Placement.InRoom(HoneymoonSuite));
            return VerbResult.Done;
        }

        ctx.Say("Nobody answers");
        return VerbResult.Done;
    }

    private VerbResult AnswerHandler(VerbContext ctx)
    {
        if (ctx.DirectObject != T(Telephone))
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
        if (ctx.DirectObject == T(Stool)) { ctx.Set(_stoolClimbed, true); ctx.Say("OK"); return VerbResult.Done; }
        return VerbResult.Pass;
    }

    private VerbResult EnterHandler(VerbContext ctx)
    {
        if (ctx.DirectObject is null) { CantDoThat(ctx); return VerbResult.Done; }

        if (ctx.DirectObject == T(Bushes))
        {
            ctx.MovePlayerTo(Garden);
            return VerbResult.Done;
        }
        if (ctx.DirectObject == T(Window))
        {
            if (ctx.Get(_windowBroken))
                ctx.MovePlayerTo(BrokenRoom);
            else
                ctx.Say("Not yet but maybe later!");
            return VerbResult.Done;
        }
        if (ctx.DirectObject == T(DoorWest))
        {
            if (ctx.Get(_doorWestOpen))
                ctx.MovePlayerTo(Disco);
            else
                ctx.Say("The door is closed");
            return VerbResult.Done;
        }
        if (ctx.DirectObject == T(Elevator))
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
        if (ctx.DirectObject == T(Radio)) return VerbResult.Pass;
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
        if (ctx.CurrentRoom == HookerBalcony || ctx.CurrentRoom == WindowLedge)
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
        if (ctx.DirectObject == T(Plant))
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

        Thing obj = ctx.DirectObject;
        if (obj == Pimp)
            ctx.Say(ctx.Get(_hookerFucked)
                ? "He says 'I don't want your money - stud!'"
                : "Try going up -- he'll take the money then");
        else if (obj == Hooker)
            ctx.Say("You already paid the Pimp, stupid!!");
        else if (obj == Blonde || obj == Waitress || obj == Girl)
        {
            ctx.Say("She yells 'I'm not a whore!!!' and kills me!");
            Purgatory(ctx);
        }
        else if (obj == Preacher)
            ctx.Say("Bring a girl here to marry -- he'll take the money then!");
        else if (obj == Businessman)
            ctx.Say("He's too drunk to do business right now!");
        else if (obj == Bartender)
            ctx.Say("Buy something -- he'll take the money then");
        else if (obj == Dealer)
            ctx.Say("Why not play 21 instead?  You'll lose anyway, fool!");
        else
            CantDoThat(ctx);
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

        if (!ctx.Carrying(T(Water)) && !ctx.Get(_pitcherFull))
        {
            ctx.Say("I have no water!");
            return VerbResult.Done;
        }

        if (ctx.DirectObject == T(Seeds) ||
            (ctx.DirectObject == T(Water) && ctx.IndirectObject == T(Seeds)) ||
            (ctx.InRoom(Garden) && ctx.Carrying(T(Seeds)) && ctx.DirectObject is null))
        {
            if (ctx.InRoom(Garden))
                GrowTree(ctx);
            else
            {
                ctx.Say("The seeds need better soil to grow.");
                ctx.Remove(T(Water));
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
        if (!ctx.LocatedIn(T(Sink), ctx.CurrentRoom))
        {
            ctx.Say("Find a working sink!");
            return VerbResult.Done;
        }
        ctx.Set(_waterOn, on);
        if (on)
        {
            ctx.Say("Water is running in the sink");
            ctx.PlaceHere(T(Water));
        }
        else if (!ctx.Get(_pitcherFull))
        {
            Ok(ctx);
            if (ctx.LocatedIn(T(Water), ctx.CurrentRoom))
                ctx.Remove(T(Water));
        }
        return VerbResult.Done;
    }

    private VerbResult FillHandler(VerbContext ctx)
    {
        if (ctx.DirectObject != T(Pitcher))
        {
            CantDoThat(ctx);
            return VerbResult.Done;
        }
        if (!ctx.Carrying(T(Pitcher)))
        {
            ctx.Say("I don't have it!");
            return VerbResult.Done;
        }
        if (!ctx.LocatedIn(T(Sink), ctx.CurrentRoom))
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
        Thing? seeds = ctx.DirectObject == T(Seeds) ? T(Seeds)
            : ctx.IndirectObject == T(Seeds) ? T(Seeds) : null;
        bool pouringWater = ctx.DirectObject == T(Water) || ctx.IndirectObject == T(Water);
        if (!pouringWater && seeds is null)
        {
            CantDoThat(ctx);
            return VerbResult.Done;
        }
        if (!ctx.Carrying(T(Pitcher)))
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

        if (seeds is null && ctx.Carrying(T(Seeds)))
            seeds = T(Seeds);

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
