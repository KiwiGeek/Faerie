using Faerie.Building;
using Faerie.Model;
using Faerie.Runtime;

namespace Faerie.Samples.Softporn;

internal sealed partial class SoftpornWorld
{
    private readonly GameBuilder _softporn;

    // ---- Rooms: first-class fields, assigned in DefineRooms (Build.Map.cs) ----
    internal Room Hallway = null!, Bathroom = null!, Bar = null!, Backroom = null!, HookerBedroom = null!, HookerBalcony = null!,
        BarStreet = null!, Dumpster = null!, BrokenRoom = null!, WindowLedge = null!,
        CasinoStreet = null!, MarriageCenter = null!, Casino = null!, TwentyOneRoom = null!, Lobby = null!,
        HoneymoonSuite = null!, HotelHallway = null!, HoneymoonBalcony = null!, HotelDesk = null!,
        PhoneBooth = null!, Disco = null!, DiscoStreet = null!, DiscoEntrance = null!, Pharmacy = null!,
        PenthouseFoyer = null!, Jacuzzi = null!, Kitchen = null!, Garden = null!, LivingRoom = null!,
        PenthousePorch = null!;

    internal StateKey<int> MoneyKey => _money;

    // ---- Things & scenery: first-class fields, assigned in DefineThings (Build.Things.cs) ----
    internal Thing Desk = null!, Washbasin = null!, Graffiti = null!, Mirror = null!, Toilet = null!,
        Businessman = null!, Button = null!, Bartender = null!, Pimp = null!, Hooker = null!, Billboard = null!,
        Preacher = null!, Tv = null!, SlotMachines = null!, Cards = null!, Ashtray = null!, Blonde = null!,
        Bed = null!, Bum = null!, Peephole = null!, DoorWest = null!, Waitress = null!, Table = null!,
        Telephone = null!, Closet = null!, Sink = null!, Elevator = null!, Dealer = null!, Cabinet = null!,
        Bushes = null!, Tree = null!, Window = null!, Sign = null!, Taxi = null!, Girl = null!, Curtain = null!,
        Newspaper = null!, Ring = null!, Whiskey = null!, Beer = null!, Hammer = null!, Garbage = null!,
        Flowers = null!, AppleCore = null!, Seeds = null!, Candy = null!, Pills = null!, Plant = null!,
        Passcard = null!, Radio = null!, Knife = null!, Magazine = null!, Rubber = null!, Wine = null!,
        Wallet = null!, Doll = null!, Apple = null!, Pitcher = null!, Stool = null!, Rope = null!, Rack = null!,
        Mushroom = null!, ControlUnit = null!, Water = null!;

    // Typed pass-throughs retained during the id->field migration so existing call sites still read
    // naturally (R(bar) / T(wallet)). These are trivial and can be inlined away.
    private static Thing T(Thing thing) => thing;

    private StateKey<int> _money = null!;
    private StateKey<bool> _ropeInUse = null!;
    private StateKey<bool> _windowBroken = null!;
    private StateKey<bool> _hookerFucked = null!;
    private StateKey<bool> _doorWestOpen = null!;
    private StateKey<bool> _radioListened = null!;
    private StateKey<bool> _wineOrdered = null!;
    private StateKey<bool> _telephoneRinging = null!;
    private StateKey<bool> _telephoneAnswered = null!;
    private StateKey<bool> _holePeeped = null!;
    private StateKey<bool> _girl2Fucked = null!;
    private StateKey<bool> _tiedToBed = null!;
    private StateKey<bool> _drawerOpen = null!;
    private StateKey<bool> _closetOpen = null!;
    private StateKey<bool> _cabinetOpen = null!;
    private StateKey<bool> _dollInflated = null!;
    private StateKey<bool> _stoolClimbed = null!;
    private StateKey<bool> _waterOn = null!;
    private StateKey<bool> _pitcherFull = null!;
    private StateKey<bool> _appleGiven = null!;
    private StateKey<bool> _candyGiven = null!;
    private StateKey<bool> _flowersGiven = null!;
    private StateKey<bool> _ringGiven = null!;
    private StateKey<bool> _marriedToGirl = null!;
    private StateKey<bool> _curtainOpen = null!;
    private StateKey<bool> _called5556969 = null!;
    private StateKey<bool> _called5550439 = null!;
    private StateKey<bool> _called5550987 = null!;
    private StateKey<bool> _rubberWorn = null!;
    private StateKey<int> _tvChannel = null!;
    private StateKey<string> _girlName = null!;
    private StateKey<string> _girlPart = null!;
    private StateKey<string> _girlDo = null!;
    private StateKey<string> _yourPart = null!;
    private StateKey<string> _yourObject = null!;
    private StateKey<string> _rubberColor = null!;
    private StateKey<string> _rubberFlavor = null!;
    private StateKey<string> _rubberLubricated = null!;
    private StateKey<string> _rubberRibbed = null!;
    private StateKey<int> _carryCount = null!;
    private StateKey<bool> _paidPimp = null;

    internal SoftpornWorld(GameBuilder b) => _softporn = b;

    private Room Register(Room room) => _softporn.Register(room);

    internal void BuildAll()
    {
        DefineState();
        DefineRooms();
        DefineThings();
        PlaceThings();
        DefineCustomVerbs();
        DefinePuzzles();
        DefineDaemons();
    }

    private void DefineThings()
    {
        // ---- scenery ----
        Desk = Scenery(_softporn.Scenery("desk").Called("desk", "drawer"));
        Washbasin = Scenery(_softporn.Scenery("washbasin").Called("washbasin", "basin", "wash"));
        Graffiti = Scenery(_softporn.Scenery("graffiti").Called("graffiti"));
        Mirror = Scenery(_softporn.Scenery("mirror").Called("mirror"));
        Toilet = Scenery(_softporn.Scenery("toilet").Called("toilet"));
        Button = Scenery(_softporn.Scenery("button").Called("button", "butt"));
        Billboard = Scenery(_softporn.Scenery("billboard").Called("billboard", "bill"));
        Tv = Scenery(_softporn.Scenery("tv").Called("tv", "television"));
        SlotMachines = Scenery(_softporn.Scenery("slot machines").Called("slots", "slot", "machines"));
        Cards = Scenery(_softporn.Scenery("cards").Called("cards", "21", "blackjack"));
        Ashtray = Scenery(_softporn.Scenery("ashtray").Called("ashtray", "asht"));
        Bed = Scenery(_softporn.Scenery("bed").Called("bed"));
        Peephole = Scenery(_softporn.Scenery("peephole").Called("peephole", "peep", "hole"));
        DoorWest = Scenery(_softporn.Scenery("door").Called("door", "west door"));
        Table = Scenery(_softporn.Scenery("table").Called("table", "tabl"));
        Telephone = Scenery(_softporn.Scenery("telephone").Called("telephone", "phone", "tele"));
        Closet = Scenery(_softporn.Scenery("closet").Called("closet", "clos").Openable());
        Sink = Scenery(_softporn.Scenery("sink").Called("sink").Switchable());
        Elevator = Scenery(_softporn.Scenery("elevator").Called("elevator", "elev"));
        Cabinet = Scenery(_softporn.Scenery("cabinet").Called("cabinet", "cabi").Openable());
        Bushes = Scenery(_softporn.Scenery("bushes").Called("bushes", "bush"));
        Tree = Scenery(_softporn.Scenery("tree").Called("tree"));
        Window = Scenery(_softporn.Scenery("window").Called("window", "wind"));
        Sign = Scenery(_softporn.Scenery("sign").Called("sign"));
        Taxi = Scenery(_softporn.Scenery("taxi").Called("taxi", "cab"));
        Curtain = Scenery(_softporn.Scenery("curtain").Called("curtain", "curt"));

        // ---- NPCs ----
        Businessman = Npc(_softporn.Creature("businessman").Called("businessman", "busi"));
        Bartender = Npc(_softporn.Creature("bartender").Called("bartender", "bart"));
        Pimp = Npc(_softporn.Creature("pimp").Called("pimp", "dude"));
        Hooker = Npc(_softporn.Creature("hooker").Called("hooker", "hook"));
        Preacher = Npc(_softporn.Creature("preacher").Called("preacher", "prea"));
        Blonde = Npc(_softporn.Creature("blonde").Called("blonde", "volu"));
        Bum = Npc(_softporn.Creature("bum").Called("bum"));
        Waitress = Npc(_softporn.Creature("waitress").Called("waitress", "wait"));
        Dealer = Npc(_softporn.Creature("dealer").Called("dealer", "deal"));
        Girl = Npc(_softporn.Creature("girl").Called("girl", "eve"));

        // ---- takeables ----
        Newspaper = _softporn.Item("newspaper").Called("newspaper", "news", "paper");
        Ring = _softporn.Item("wedding ring").Called("ring", "wedding ring");
        Whiskey = _softporn.Item("whiskey").Called("whiskey", "whis", "shot").Drinkable();
        Beer = _softporn.Item("beer").Called("beer").Drinkable();
        Hammer = _softporn.Item("hammer").Called("hammer", "hamm");
        Garbage = _softporn.Item("garbage").Called("garbage", "garb", "trash");
        Flowers = _softporn.Item("flowers").Called("flowers", "flow");
        AppleCore = _softporn.Item("apple core").Called("core", "apple core");
        Seeds = _softporn.Item("seeds").Called("seeds", "seed");
        Candy = _softporn.Item("candy").Called("candy", "cand");
        Pills = _softporn.Item("pills").Called("pills", "pill").Edible();
        Plant = _softporn.Item("plant").Called("plant", "plan");
        Passcard = _softporn.Item("passcard").Called("passcard", "pass");
        Radio = _softporn.Item("radio").Called("radio", "radi");
        Knife = _softporn.Item("knife").Called("knife", "knif").Wearable();
        Magazine = _softporn.Item("magazine").Called("magazine", "maga", "mag");
        Rubber = _softporn.Item("rubber").Called("rubber", "rubb", "condom").Wearable();
        Wine = _softporn.Item("wine").Called("wine").Drinkable();
        Wallet = _softporn.Item("wallet").Called("wallet", "wall");
        Doll = _softporn.Item("doll").Called("doll");
        Apple = _softporn.Item("apple").Called("apple", "appl").Edible();
        Pitcher = _softporn.Item("pitcher").Called("pitcher", "pitc");
        Stool = _softporn.Item("stool").Called("stool", "stoo");
        Rope = _softporn.Item("rope").Called("rope").Wearable();
        Rack = _softporn.Item("display rack").Called("rack", "display");
        Mushroom = _softporn.Item("mushroom").Called("mushroom", "mush").Edible();
        ControlUnit = _softporn.Item("remote control").Called("control unit", "remote", "cont", "unit");
        Water = _softporn.Item("water").Called("water", "wate").Drinkable();
    }

    private void PlaceThings()
    {
        Desk.StartsIn(Hallway);
        Washbasin.StartsIn(Bathroom);
        Graffiti.StartsIn(Bathroom);
        Mirror.StartsIn(Bathroom);
        Toilet.StartsIn(Bathroom);
        Businessman.StartsIn(Hallway);
        Bartender.StartsIn(Bar);
        Pimp.StartsIn(Backroom);
        Hooker.StartsIn(HookerBedroom);
        Billboard.StartsIn(HookerBalcony);
        Preacher.StartsIn(MarriageCenter);
        Tv.StartsIn(Backroom);
        SlotMachines.StartsIn(Casino);
        Cards.StartsIn(TwentyOneRoom);
        Ashtray.StartsIn(HotelHallway);
        Blonde.StartsIn(HotelDesk);
        Bed.StartsIn(HoneymoonSuite);
        Bum.StartsIn(DiscoStreet);
        Peephole.StartsIn(HoneymoonBalcony);
        DoorWest.StartsIn(DiscoEntrance);
        Waitress.StartsIn(Disco);
        Table.StartsIn(Disco);
        Telephone.StartsIn(PhoneBooth);
        Closet.StartsIn(LivingRoom);
        Sink.StartsIn(Kitchen);
        Elevator.StartsIn(HotelDesk);
        Dealer.StartsIn(TwentyOneRoom);
        Cabinet.StartsIn(Kitchen);
        Window.StartsIn(WindowLedge);
        Sign.StartsIn(BarStreet);
        Taxi.StartsIn(BarStreet);
        Girl.StartsIn(Disco);
        Curtain.StartsIn(Bar);
        Candy.StartsIn(HookerBedroom);
        Pills.StartsIn(BrokenRoom);
        Plant.StartsIn(Lobby);
        Radio.StartsIn(HoneymoonBalcony);
        Rubber.StartsIn(Pharmacy);
        Rack.StartsIn(Pharmacy);
        Hammer.StartsIn(Garden);
        Garbage.StartsIn(Dumpster);
        Flowers.StartsIn(Hallway);
        Stool.StartsIn(Garden);
        Mushroom.StartsIn(Garden);
        Button.StartsIn(Bar);

        Wallet.StartsCarried();

        Wine.OrderableFrom(Waitress);
        Beer.OrderableFrom(Bartender);
        Whiskey.OrderableFrom(Bartender);
    }

    // Scenery and NPCs are "proper-named" (the parser/lister treats their name as a fixed label).
    private static Thing Scenery(Thing thing) { thing.Proper(); return thing; }
    private static Thing Npc(Thing thing) { thing.Proper(); return thing; }

    private void DefineRooms()
    {
        // ---- Bar area ----
        Bar = Register(new Room("bar", "Bar")
            .ShortTitle("I'm in a Sleazy Bar")
            .Describe("I'm in a sleazy bar. Behind the bar sits a bartender. A sign hanging over him says: 'Beer $100 Whiskey $100'. The place isn't furnished too well. A curtain hangs on one wall. Next to the curtain is a utton.. A fan whirls slowly overhead - moving the stagnant air around."));

        Hallway = Register(new Room("hallway", "Hallway")
           .ShortTitle("I'm in a hallway")
           .Describe("I'm in a dimly lit hallway. The paint is peeling off the walls and the floor hasn't been cleaned in months. Cockroaches run across the floor - jumping as the loosely installed lightbulb crackles and flickers. An old desk sits pushed against the wall. A businessman sits on a broken chair next to the desk. Seems kind of drunk!")
           .East(Bar));

        Bathroom = Register(new Room("bathroom", "Bathroom")
            .ShortTitle("I'm in a Bathroom")
            .Describe("I'm in a bathroom. The stench is unbelievable!!!! Graffiti is all over the walls. Cockroaches don't seem to survive in this place - their dead bodies are strewn everywhere. The sink's faucets are broken - in fact the sink hangs from the wall by its rusted plumbing. A toilet sits in the corner. This baby looks dangerous!!")
            .South(Hallway));

        Dumpster = Register(new Room("dumpster", "Dumpster")
            .ShortTitle("I'm in a Filthy Dumpster")
            .Describe("There's a fire escape ladder above me which lowers automatically whenever weight is put on it. As a result I find myself in the garbage dumpster which some fool placed under it! The trash in this thing is foul!! I'm sitting in a sea of coffee grinds and egg shells. Various pieces of trash surround me................................ I don't like this - I think I may throw up!!!!!"));

        HookerBalcony = Register(new Room("hooker-balcony", "Hooker's Balcony")
            .ShortTitle("I'm on a Hooker's Balcony")
            .Describe("I'm on a balcony. Off in the distance I see a brightly lite billboard. A fire escape ladder is at one end of the balcony. A sign says 'Use only in extreme emergency!!!' Looking over towards the west end of the balcony I see a window ledge. It looks too dangerous to go there - I might fall!")
            .Down(Dumpster));

        HookerBedroom = Register(new Room("hooker-bedroom", "Hooker's Bedroom")
           .ShortTitle("I'm in a Hooker's Bedroom")
           .Describe("I'm in a seedy bedroom. There's a hooker in here also. The bed's a mess and the hooker's about the same! The room is painted bright pink and the ceiling is covered with mirrors! A fire exit is to the north. A sign says: 'Take precautions! The clap could be fatal!'")
           .North(HookerBalcony, gate: ctx => ctx.CurrentRoom == HookerBalcony ? null : ctx.Get(_hookerFucked) ? null : "The Hooker says: 'Don't go there ... do me first!!'"));

        Backroom = Register(new Room("backroom", "Backroom")
            .ShortTitle("I'm in the Backroom")
            .Describe("I'm in the backroom of the bar. There's this big dude in here with me. He's wearing a button. I can't see what it says ... maybe I should take a closer look at him ... Stairs lead up to the second floor. There's a TV in the corner also. I get the feeling loitering is not encouraged here!")
            .West(Bar, gate: ctx => ctx.CurrentRoom == Backroom ? null : ctx.Get(_curtainOpen) ? null : "I can't go that way!",
            onPass: ctx =>
            {
                // Close the curtain automatically when it's passed through. Doesn't affect exiting since that's explicitely allowed above.
                ctx.Set(_curtainOpen, false);
                return true;
            })
            .Up(HookerBedroom, gate: ctx => ctx.Get(_tvChannel) != 6
                                                ? "The Pimp blocks my way upstairs!"
                                                : (!HasMoney(ctx, 20) || !CarryingWallet(ctx)) && !ctx.Get(_paidPimp)
                                                    ? "The Pimp says I can't until I get $2000"
                                                    : ctx.Get(_hookerFucked) 
                                                        ? "The Pimp says 'No -- the hooker can't take it anymore!'" 
                                                        : null
            , onPass: ctx =>
            {
                ctx.Say("The Pimp takes $2000 and says OK");
                Spend(ctx, 20);
                ctx.Set(_paidPimp, true);
                return true;
            }));

        BarStreet = Register(new Room("bar-street", "Street outside the Bar")
            .ShortTitle("I'm on a Street outside the Bar")
            .Describe("I'm on the sidewalk outside the bar. A couple stray dogs wander around. A cat is crouched in the entrance watching the dogs. Old beat-up cars drive by. The sirens of police cars and ambulances wail in the distance. Some paper blows by - from the overfilled garbage dumpster next to me. I just miss stepping on a dogs \"calling card\". The dogs look at me - I hope they don't think I'm a fire hydrant!")
            .South(Bar)
            .East(Dumpster));

        BrokenRoom = Register(new Room("broken-room", "Broken Room")
            .ShortTitle("I'm in a Broken Room")
            .Describe("I'm in a broken room. The door to this room is locked shut. The plaster's falling off the wall ..... the usual decor for this building. Through the window leads the safety rope I'm using. The only other exit is the door which I broke through to get in here. I can't go back out that way - it's locked shut!"));

        WindowLedge = Register(new Room("window-ledge", "Window Ledge")
            .ShortTitle("I'm on a Window Ledge")
            .Describe("I'm on a window ledge. My safety rope leads back to the fire escape. While it helps me some I could still fall and kill myself ... so be careful!!!! The window looks into a room. But I can't see too much from here.")
            .East(HookerBalcony, onPass: ctx =>
            {
                if (ctx.CurrentRoom == WindowLedge) { return true; }
                if (ctx.Get(_ropeInUse)) { return true; }
                FallingDown(ctx, jumped: false);
                return false;
            })
            .Down(BrokenRoom, reciprocal: false, when: ctx => ctx.Get(_windowBroken), blocked: "I can't go that way!")
            .South(BrokenRoom));

        // ---- Casino area ----
        MarriageCenter = Register(new Room("marriage-center", "Quickie Marriage Center")
            .ShortTitle("I'm in a Quickie Marriage Center")
            .Describe("I'm in a quickie marriage center. A flashing neon sign says: 'Why wait? Marry the girl of your dream today!!! You provide the girl - we provide a legal marriage for only $1000!!'. A plaque hangs below the sign, procaliming - 'Over 1 million served!!!'"));

        TwentyOneRoom = Register(new Room("twenty-one-room", "21 Room")
            .ShortTitle("I'm in the '21' Room")
            .Describe("I'm in the 'Twenty-one room'. Tables for playing blackjack are everywhere. The noise of people winning and losing fortunes fills the room. A table stands in front of me - the dealer waits for me to join in. People gather ... they want to see me gamble my fortune away! A voice within me says: 'Go for it -- fool!'.!"));

        HoneymoonBalcony = Register(new Room("honeymoon-balcony", "Honeymooner's Balcony")
            .ShortTitle("I'm on the Honeymooner's Balcony")
            .Describe("I'm out on a porch. A high wooden fence surrounds the porch. There's a little hole in the fence. An arrow points at it - written next to it is a message - 'Look here!'. The sun is out - it's rays beat down on the porch making it very hot."));

        HotelHallway = Register(new Room("hotel-hallway", "Hotel Hallway")
            .ShortTitle("I'm in the Hotel Hallway")
            .Describe("I'm in a hallway. Doors line each side - most have 'Do not disturb' hung on the doorknobs. Waiters from room service pass by bringing food and drink to the hotel guests. An ashtray stands next to the wall. To the south is the entrance to the Honeymoon Suite."));

        HoneymoonSuite = Register(new Room("honeymoon-suite", "Honeymoon Suite")
            .ShortTitle("I'm in the Honeymoon Suite")
            .Describe("I'm in the honeymoon suite of the hotel. The decor is fabulous!!! A giant heart shaped bed sits in one corner of the room. The floor is covered with a deep shag rug. A tiffany lamp provides just the right amount of light to complete the atmosphere. A breeze flows through the curtains on the east wall.")
            .North(HotelHallway, gate: ctx => ctx.CurrentRoom == HoneymoonSuite ? null : ctx.Get(_marriedToGirl) ? null : "The door is locked shut!")
            .East(HoneymoonBalcony));

        Lobby = Register(new Room("lobby", "Lobby")
            .ShortTitle("I'm in the Lobby of the Hotel")
            .Describe("I'm in the main lobby. There's a stairway going up to the hotel desk. The only other exit leads back to the casino. Over in the corner is a flourishing plant which is sitting in a pot. Couchs and tables are also in the room - as are other items one would normally find in a lobby. The lobby is empty - everybody is out gambling."));

        HotelDesk = Register(new Room("hotel-desk", "Hotel Desk")
            .ShortTitle("I'm at the Hotel Desk")
            .Describe("I'm at the registration counter of the hotel. A sign says 'No vacancy'. There's an elevator next to the counter marked 'Penthouse - private'. A button is next to the elevator. The smell of perfume fills the air!")
            .West(HotelHallway)
            .Down(Lobby));

        Casino = Register(new Room("casino", "Main Casino Room")
            .ShortTitle("I'm in the Main Casino Room")
            .Describe("I'm in the main casino room. Row upon row of slot machines fill the room. The bells of the slot machines clamor as winnings are paid out to the winners. The police cart off one of the losers. Vagrancy is not tolerated here - vagrants are exterminated ... keeps the crime rate low ...! But everyone seems happy here .. it's a gambler's paradise!!!!!")
            .North(TwentyOneRoom)
            .East(Lobby));

        CasinoStreet = Register(new Room("casino-street", "Downtown Street")
            .ShortTitle("I'm on a Downtown Street")
            .Describe("I'm on a downtown street. People from all walks of life are milling about. Cadillacs, Limos and Mercedes sportcars drive up and down the street bringing gamblers to and from the casinos. To the north is an establishment - an advertiesemnt says: 'Use our services to enter into a blissful life!' To the east is 'The Adventurers Hotel'. Seems like quite the classy place!")
            .North(MarriageCenter)
            .East(Casino));

        // ---- Disco area ----
        PhoneBooth = Register(new Room("phone-booth", "Telephone Booth")
            .ShortTitle("I'm in a Telephone Booth")
            .Describe("I'm in a telephone booth. The directory is all ripped up and piled on the floor. There's some numbers scribbled on the side of the telephone. The only ones which are legible read '555-6969' and '555-0439'."));

        DiscoStreet = Register(new Room("disco-street", "Residential Street")
            .ShortTitle("I'm on a Residential Street")
            .Describe("I'm standing on a sidewalk. To the north is a fancy doorway - the entrance to the Disco Club. An old bum sits by the entrance - he's definitley seen better days. The guy really looks tormented! To the east is a pharmacy. Apartment houses and homes line the rest of the street."));

        DiscoEntrance = Register(new Room("disco-entrance", "Disco Entrance")
            .ShortTitle("I'm in the Disco's Entrance")
            .Describe("I'm in the entrance to the disco. Pictures line the entrance way - showing the happy singles who attend the club. Singles pass by me into the club. Couples wander out - kissing and making eyes at each other. A door is to the west. The door has a sign on it.")
            .South(DiscoStreet));

        Disco = Register(new Room("disco", "Disco")
            .ShortTitle("I'm in the Disco")
            .Describe("I'm in the 'Swinging Singles Disco'. There's a crazy DJ playing the newest hits. The dance floor is filled with guys and gals doin' the best steps in town. The crowd is really getting into it - everybody's having fun. There's a table to stand at over by the dance floor. A little cardboard sign at the table says: 'Wine - $100'.")
            .South(PhoneBooth)
            .East(DiscoEntrance, gate: ctx => ctx.CurrentRoom == Disco ? null : ctx.Get(_doorWestOpen) ? null : "The door is closed!"));

        Pharmacy = Register(new Room("pharmacy", "Pharmacy")
            .ShortTitle("I'm in the Pharmacy")
            .Describe("I'm in a pharmacy. A pharmacist sits behing the counter. On one wall sits a magazine rack. A sign reads 'This is not a library - no reading'. A mirror to protect against shoplifting is mounted in the corner. Kids stop and buy candy. Others buy newspapers/cigarettes etc.")
            .West(DiscoStreet));

        // ---- Penthouse area ----
        PenthouseFoyer = Register(new Room("penthouse-foyer", "Penthouse Foyer")
            .ShortTitle("I'm in the Penthouse Foyer")
            .Describe("I'm in the foyer of the penthouse. Over in the corner is a spiral staircase. Next to the elevator is a button. The place is nicely decorated - no expense spared. The kitchen is to the east."));

        Kitchen = Register(new Room("kitchen", "Kitchen")
            .ShortTitle("I'm in the Kitchen")
            .Describe("I'm in the kitchen. There's a sink to one side. High over the sink is mounted a cabinet. There's no dishes in sight - the place is kept quite tidy. Real nice. There's a little sign over the sink.")
            .West(PenthouseFoyer));

        Jacuzzi = Register(new Room("jacuzzi", "Jacuzzi")
            .ShortTitle("I'm in the Jacuzzi")
            .Describe("I'm in a jacuzzi!!! Oh boy - does this feel good!!! Water swirls around me - its warmth soaks into my body. The feeling of relaxation is almost numbing. Over on the other side of the jacuzzi is a most beautiful girl!!!! I hope she doesn't mind me being here."));

        PenthousePorch = Register(new Room("penthouse-porch", "Penthouse Porch")
            .ShortTitle("I'm on the Penthouse Porch")
            .Describe("I'm outside on an expansive rooftop. The sun shines in amongst the plants and trees - birds flutter about. There's a jacuzzi in the middle of the porch!! Well - there's the source of the gurgling noise!! A wooden fence surrounds the area.")
            .Down(Jacuzzi));

        LivingRoom = Register(new Room("living-room", "Living Room")
            .ShortTitle("I'm in the Living Room")
            .Describe("I'm in a living room. There's a closet on one wall. Nobody is here - but I think there is a gurgling noise coming from somewhere. A very nice place!")
            .North(PenthousePorch)
            .Down(PenthouseFoyer));

        Garden = Register(new Room("garden", "Garden")
            .ShortTitle("I'm in the Garden")
            .Describe("I'm in a lush garden!!! The air is filled with the aroma of all sorts of plants. Green ferns are everywhere. Roses and other flowers emit their wonderful fragrances. If ever there was a garden of Eden - this certainly has to be the place!! The entrance disappeared just as I walked in!!! Hmmm ... how do I get out of here!?!?!?!?!?!?")
            .South(Lobby, reciprocal: false));
    }

    private void DefineState()
    {
        _money = _softporn.State("money", 10);
        _ropeInUse = _softporn.State("rope-in-use", false);
        _windowBroken = _softporn.State("window-broken", false);
        _hookerFucked = _softporn.State("hooker-fucked", false);
        _doorWestOpen = _softporn.State("door-west-open", false);
        _radioListened = _softporn.State("radio-listened", false);
        _wineOrdered = _softporn.State("wine-ordered", false);
        _telephoneRinging = _softporn.State("telephone-ringing", false);
        _telephoneAnswered = _softporn.State("telephone-answered", false);
        _holePeeped = _softporn.State("hole-peeped", false);
        _girl2Fucked = _softporn.State("girl2-fucked", false);
        _tiedToBed = _softporn.State("tied-to-bed", false);
        _drawerOpen = _softporn.State("drawer-open", false);
        _closetOpen = _softporn.State("closet-open", false);
        _cabinetOpen = _softporn.State("cabinet-open", false);
        _dollInflated = _softporn.State("doll-inflated", false);
        _stoolClimbed = _softporn.State("stool-climbed", false);
        _waterOn = _softporn.State("water-on", false);
        _pitcherFull = _softporn.State("pitcher-full", false);
        _appleGiven = _softporn.State("apple-given", false);
        _candyGiven = _softporn.State("candy-given", false);
        _flowersGiven = _softporn.State("flowers-given", false);
        _ringGiven = _softporn.State("ring-given", false);
        _marriedToGirl = _softporn.State("married-to-girl", false);
        _curtainOpen = _softporn.State("curtain-open", false);
        _called5556969 = _softporn.State("called-5556969", false);
        _called5550439 = _softporn.State("called-5550439", false);
        _called5550987 = _softporn.State("called-5550987", false);
        _rubberWorn = _softporn.State("rubber-worn", false);
        _tvChannel = _softporn.State("tv-channel", 0);
        _girlName = _softporn.State("girl-name", "");
        _girlPart = _softporn.State("girl-part", "");
        _girlDo = _softporn.State("girl-do", "");
        _yourPart = _softporn.State("your-part", "");
        _yourObject = _softporn.State("your-object", "");
        _rubberColor = _softporn.State("rubber-color", "");
        _rubberFlavor = _softporn.State("rubber-flavor", "");
        _rubberLubricated = _softporn.State("rubber-lubricated", "non-lubricated");
        _rubberRibbed = _softporn.State("rubber-ribbed", "non-ribbed");
        _carryCount = _softporn.State("carry-count", 0);
        _paidPimp = _softporn.State("paid-pimp", false);
    }
}
