namespace Faerie.Samples.Softporn.Interpreter;

public enum Directions
{
    North,
    South,
    East,
    West,
    Up,
    Down,
    NoDirection
}

public enum Objects
{
    Desk,
    Washbasin,
    Graffiti,
    Mirror,
    Toilet,
    Businessman,
    Button,
    Bartender,
    Pimp,
    Hooker,
    Billboard,
    Preacher,
    TV,
    SlotMachines,
    Cards,
    Ashtray,
    Blonde,
    Bed,
    Bum,
    Peephole,
    DoorWest,
    Waitress,
    Table,
    Telephone,
    Closet,
    Sink,
    Elevator,
    Dealer,
    Cabinet,
    Bushes,
    Tree,
    Window,
    Sign,
    Girl,
    Newspaper,
    Ring,
    Whiskey,
    Beer,
    Hammer,
    Garbage,
    Flowers,
    AppleCore,
    Seeds,
    Candy,
    Pills,
    Plant,
    Passcard,
    Radio,
    Knife,
    Magazine,
    Rubber,
    Wine,
    Wallet,
    Doll,
    Apple,
    Pitcher,
    Stool,
    Rope,
    Rack,
    Mushroom,
    ControlUnit,
    Curtain,
    Water,
    Taxi,
    Inventory,
    All,
    On,
    Off,
    You,
    NoObject
}

public enum Places
{
    BHallwy,
    BBathrm,
    BBar,
    BStreet,
    BBackrm,
    BGDump,
    BInroom,
    BWledge,
    BBedrm,
    BBalcny,
    CStreet,
    CMarryc,
    CCasino,
    C21Room,
    CLobby,
    CHmoons,
    CHallwy,
    CBalcny,
    CHtdesk,
    DTelbth,
    DDisco,
    DStreet,
    DEntrnc,
    DPhrmcy,
    PPntfoy,
    PJacuzi,
    PKitchn,
    PGarden,
    PLivrom,
    PPntpch,
    YouHaveIt,
    Nowhere
}

public enum Verbs
{
    Go,
    Hail,
    Take,
    Drop,
    Look,
    Flush,
    Open,
    Inflate,
    Play,
    Press,
    Enter,
    Eat,
    Drink,
    Buy,
    Climb,
    Water,
    Fill,
    Pour,
    Listen,
    Close,
    Jump,
    Marry,
    Fuck,
    Wear,
    Answer,
    Call,
    Break,
    Cut,
    Dance,
    Kill,
    Pay,
    Smoke,
    Show,
    Smell,
    Help,
    Kiss,
    Stab,
    Say,
    Quit,
    ShowScore,
    Save,
    Restore,
    NoVerb
}

public static class SoftpornConstants
{
    public const int WordNameLength = 4;
    public const int MaxCarried = 7;
    public const int RecSize = 450;

    public static readonly Directions FirstDirection = Directions.North;
    public static readonly Directions LastDirection = Directions.Down;
    public static readonly Objects FirstObject = Objects.Desk;
    public static readonly Objects LastObject = Objects.NoObject;
    public static readonly Places FirstPlace = Places.BHallwy;
    public static readonly Places LastPlace = Places.Nowhere;
    public static readonly Verbs FirstVerb = Verbs.Go;
    public static readonly Verbs LastVerb = Verbs.NoVerb;

    public static readonly HashSet<Places> BarArea =
    [
        Places.BHallwy, Places.BBathrm, Places.BBar, Places.BStreet, Places.BBackrm,
        Places.BGDump, Places.BInroom, Places.BWledge, Places.BBedrm, Places.BBalcny
    ];

    public static readonly HashSet<Places> CasinoArea =
    [
        Places.CStreet, Places.CMarryc, Places.CCasino, Places.C21Room, Places.CLobby,
        Places.CHmoons, Places.CHallwy, Places.CBalcny, Places.CHtdesk
    ];

    public static readonly HashSet<Places> DiscoArea =
    [
        Places.DTelbth, Places.DDisco, Places.DStreet, Places.DEntrnc, Places.DPhrmcy
    ];

    public static readonly HashSet<Places> PenthouseArea =
    [
        Places.PPntfoy, Places.PJacuzi, Places.PKitchn, Places.PGarden, Places.PLivrom, Places.PPntpch
    ];

    public static readonly HashSet<Places> PublicPlaces =
    [
        Places.CStreet, Places.CCasino, Places.C21Room, Places.CLobby, Places.CHtdesk,
        Places.DStreet, Places.DEntrnc
    ];

    public static readonly HashSet<Objects> TakeableObjects =
    [
        Objects.Newspaper, Objects.Ring, Objects.Whiskey, Objects.Beer, Objects.Hammer, Objects.Garbage,
        Objects.Flowers, Objects.AppleCore, Objects.Seeds, Objects.Candy, Objects.Pills, Objects.Plant,
        Objects.Passcard, Objects.Radio, Objects.Knife, Objects.Magazine, Objects.Rubber, Objects.Wine,
        Objects.Wallet, Objects.Doll, Objects.Apple, Objects.Pitcher, Objects.Stool, Objects.Rope,
        Objects.Rack, Objects.Mushroom, Objects.ControlUnit, Objects.Water
    ];

    public static readonly HashSet<Verbs> StandAloneVerbs =
    [
        Verbs.Look, Verbs.Jump, Verbs.Dance, Verbs.Help, Verbs.Quit, Verbs.ShowScore, Verbs.Save, Verbs.Restore
    ];

    public static readonly HashSet<Verbs> SpecialVerbs =
    [
        Verbs.Take, Verbs.Hail, Verbs.Call, Verbs.Play, Verbs.Buy, Verbs.Save, Verbs.Restore
    ];

    public static readonly HashSet<char> Vowels = ['A', 'O', 'U', 'E', 'I', 'Y', 'a', 'o', 'u', 'e', 'i', 'y'];

    public static readonly string[] DirectionName =
    [
        "North", "South", "East", "West", "Up", "Down", "No direction"
    ];

    public static readonly string[] ObjectName =
    [
        "A Desk",
        "A Washbasin",
        "Graffiti",
        "A Mirror",
        "A Toilet",
        "A Businessman",
        "A Button",
        "The Bartender",
        "A Big Dude!",
        "A Funky Hooker",
        "A Billboard",
        "A Preacher",
        "A TV",
        "Slot Machines",
        "Cards",
        "An Ashtray",
        "A Voluptous Blonde",
        "A Bed",
        "A Bum",
        "A Peep Hole",
        "A Door to the West",
        "A Waitress",
        "A Table",
        "A Telephone",
        "A Closet",
        "A Sink",
        "An Elevator",
        "A Dealer",
        "A Cabinet",
        "Bushes",
        "A Tree",
        "A Window",
        "A Sign",
        "A Girl",
        "A Newspaper",
        "A Wedding Ring",
        "A Shot of Whiskey",
        "A Beer",
        "A Hammer",
        "Garbage",
        "Flowers",
        "The Core of an Apple",
        "Seeds",
        "Candy",
        "Pills",
        "A Plant",
        "A Passcard",
        "A Radio",
        "A Pocket Knife",
        "AdventureBoy Magazine",
        "A Rubber",
        "A Bottle of Wine",
        "A Wallet",
        "An Inflatable Doll",
        "An Apple",
        "A Pitcher",
        "A Stool",
        "A Rope",
        "A Display Rack",
        "A Mushroom",
        "A Remote Control Unit",
        "A Curtain",
        "Water",
        "A Taxi",
        "Inventory",
        "All",
        "On",
        "Off",
        "Yourself",
        "No object"
    ];

    public static readonly Places[] OrigObjectPlace =
    [
        Places.BHallwy,       // Desk
        Places.BBathrm,       // Washbasin
        Places.BBathrm,       // Graffiti
        Places.BBathrm,       // Mirror
        Places.BBathrm,       // Toilet
        Places.BHallwy,       // Businessman
        Places.CHtdesk,       // Button
        Places.BBar,            // Bartender
        Places.BBackrm,       // Pimp
        Places.BBedrm,        // Hooker
        Places.BBalcny,       // Billboard
        Places.CMarryc,       // Preacher
        Places.BBackrm,       // TV
        Places.CCasino,       // SlotMachines
        Places.C21Room,       // Cards
        Places.CHallwy,       // Ashtray
        Places.CHtdesk,       // Blonde
        Places.CHmoons,       // Bed
        Places.DStreet,       // Bum
        Places.CBalcny,       // Peephole
        Places.DEntrnc,       // DoorWest
        Places.DDisco,        // Waitress
        Places.DDisco,        // Table
        Places.DTelbth,       // Telephone
        Places.PLivrom,       // Closet
        Places.PKitchn,       // Sink
        Places.CHtdesk,       // Elevator
        Places.C21Room,       // Dealer
        Places.PKitchn,       // Cabinet
        Places.Nowhere,       // Bushes
        Places.Nowhere,       // Tree
        Places.BWledge,       // Window
        Places.BStreet,       // Sign
        Places.DDisco,        // Girl
        Places.Nowhere,       // Newspaper
        Places.Nowhere,       // Ring
        Places.Nowhere,       // Whiskey
        Places.Nowhere,       // Beer
        Places.PGarden,       // Hammer
        Places.BGDump,        // Garbage
        Places.BHallwy,       // Flowers
        Places.Nowhere,       // AppleCore
        Places.Nowhere,       // Seeds
        Places.BBedrm,        // Candy
        Places.BInroom,       // Pills
        Places.CLobby,        // Plant
        Places.Nowhere,       // Passcard
        Places.CBalcny,       // Radio
        Places.Nowhere,       // Knife
        Places.Nowhere,       // Magazine
        Places.DPhrmcy,       // Rubber
        Places.Nowhere,       // Wine
        Places.YouHaveIt,     // Wallet
        Places.Nowhere,       // Doll
        Places.Nowhere,       // Apple
        Places.Nowhere,       // Pitcher
        Places.PGarden,       // Stool
        Places.Nowhere,       // Rope
        Places.DPhrmcy,       // Rack
        Places.PGarden,       // Mushroom
        Places.Nowhere,       // ControlUnit
        Places.BBar,            // Curtain
        Places.Nowhere,       // Water
        Places.Nowhere,       // Taxi
        Places.Nowhere,       // Inventory
        Places.Nowhere,       // All
        Places.Nowhere,       // On
        Places.Nowhere,       // Off
        Places.Nowhere,       // You
        Places.Nowhere        // NoObject
    ];

    public static readonly string[] VerbName =
    [
        "GO ", "HAIL", "TAKE", "DROP", "LOOK",
        "FLUS", "OPEN", "INFL", "PLAY", "PRES",
        "ENTE", "EAT ", "DRIN", "BUY ", "CLIM",
        "WATE", "FILL", "POUR", "LIST", "CLOS",
        "JUMP", "MARR", "FUCK", "WEAR", "ANSW",
        "CALL", "BREA", "CUT ", "DANC", "KILL",
        "PAY ", "SMOK", "SHOW", "SMEL", "HELP",
        "KISS", "STAB", "SAY ", "QUIT",
        "SCOR", "SAVE", "REST", ""
    ];

    public static readonly string[] ObjName =
    [
        "DESK", "WASH", "GRAF", "MIRR", "TOIL",
        "BUSI", "BUTT", "BART", "DUDE", "HOOK",
        "BILL", "PREA", "TV ", "SLOT", "CARD",
        "ASHT", "VOLU", "BED ", "BUM ", "HOLE",
        "DOOR", "WAIT", "TABL", "TELE", "CLOS",
        "SINK", "ELEV", "DEAL", "CABI", "BUSH",
        "TREE", "WIND", "SIGN", "GIRL", "NEWS",
        "RING", "WHIS", "BEER", "HAMM", "GARB",
        "FLOW", "CORE", "SEED", "CAND", "PILL",
        "PLAN", "PASS", "RADI", "KNIF", "MAGA",
        "RUBB", "WINE", "WALL", "DOLL", "APPL",
        "PITC", "STOO", "ROPE", "RACK", "MUSH",
        "CONT", "CURT", "WATE",
        "TAXI", "INVE", "ALL ", "ON ", "OFF ",
        "YOU ", ""
    ];

    public static readonly (string Orig, string Repl)[] SynVerb =
    [
        ("GET ", "TAKE"), ("GRAB", "TAKE"), ("LEAV", "DROP"), ("PLAN", "DROP"), ("GIVE", "DROP"),
        ("SEAR", "LOOK"), ("EXAM", "LOOK"), ("READ", "LOOK"), ("WATC", "LOOK"), ("PULL", "OPEN"),
        ("PUSH", "PRES"), ("ORDE", "BUY "), ("SEDU", "FUCK"), ("RAPE", "FUCK"), ("SCRE", "FUCK"),
        ("USE ", "WEAR"), ("DIAL", "CALL"), ("SMAS", "BREA"), ("STOP", "QUIT"), ("BYE ", "QUIT")
    ];

    public static readonly (string Orig, string Repl)[] SynNoun =
    [
        ("CAB ", "TAXI"), ("PEEP", "HOLE"), ("DRAW", "DESK"), ("BASI", "WASH"), ("PIMP", "DUDE"),
        ("MACH", "SLOT"), ("21 ", "CARD"), ("BLAC", "CARD"), ("DISP", "RACK"), ("PHON", "TELE"),
        ("WEDD", "RING"), ("BLON", "VOLU"), ("EVE ", "GIRL"), ("PAPE", "NEWS"), ("SHOT", "WHIS"),
        ("TRAS", "GARB"), ("UNIT", "CONT")
    ];

    public static readonly string[] GlueWords =
    [
        "A ", "AN ", "THE ", "THIS", "THAT",
        "AT ", "TO ", "FROM", "WITH"
    ];

    public static readonly string[] DirName =
    [
        "NORT", "SOUT", "EAST", "WEST", "UP ", "DOWN", ""
    ];

    public static readonly string[] PlaceName =
    [
        "I'm in a Hallway",
        "I'm in a Bathroom",
        "I'm in a Sleazy Bar",
        "I'm on a Street outside the Bar",
        "I'm in the Backroom",
        "I'm in a Filthy Dumpster",
        "I'm inside the room I broke into!",
        "I'm on a Window Ledge",
        "I'm in a Hooker's Bedroom",
        "I'm on a Hooker's Balcony",
        "I'm on a Downtown Street",
        "I'm in a Quickie Marriage Center",
        "I'm in the Main Casino Room",
        "I'm in the '21' Room",
        "I'm in the Lobby of the Hotel",
        "I'm in the Honeymoon Suite",
        "I'm in the Hotel Hallway",
        "I'm on the Honeymooner's Balcony",
        "I'm at the Hotel Desk",
        "I'm in a Telephone Booth",
        "I'm in the Disco",
        "I'm on a Residential Street",
        "I'm in the Disco's Entrance",
        "I'm in the Pharmacy",
        "I'm in the Penthouse Foyer",
        "I'm in the Jacuzzi",
        "I'm in the Kitchen",
        "I'm in the Garden",
        "I'm in the Living Room",
        "I'm on the Penthouse Porch",
        "Nowhere",
        "Carried by You"
    ];

    /// <summary>orig_path[place][direction] — same order as Pascal softporn-1.inc.pas.</summary>
    public static readonly Places[,] OrigPath = BuildOrigPath();

    private static Places[,] BuildOrigPath()
    {
        var p = new Places[(int)Places.Nowhere + 1, 6];
        void Set(Places place, Places n, Places s, Places e, Places w, Places u, Places d)
        {
            p[(int)place, 0] = n;
            p[(int)place, 1] = s;
            p[(int)place, 2] = e;
            p[(int)place, 3] = w;
            p[(int)place, 4] = u;
            p[(int)place, 5] = d;
        }

        Set(Places.BHallwy, Places.BBathrm, Places.Nowhere, Places.BBar, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.BBathrm, Places.Nowhere, Places.BHallwy, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.BBar, Places.BStreet, Places.Nowhere, Places.Nowhere, Places.BHallwy, Places.Nowhere, Places.Nowhere);
        Set(Places.BStreet, Places.Nowhere, Places.BBar, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.BBackrm, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.BBar, Places.BBedrm, Places.Nowhere);
        Set(Places.BGDump, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.BStreet, Places.Nowhere, Places.Nowhere);
        Set(Places.BInroom, Places.BWledge, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.BWledge, Places.Nowhere, Places.Nowhere, Places.BBalcny, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.BBedrm, Places.BBalcny, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.BBackrm);
        Set(Places.BBalcny, Places.Nowhere, Places.BBedrm, Places.Nowhere, Places.BWledge, Places.Nowhere, Places.BGDump);

        Set(Places.CStreet, Places.CMarryc, Places.Nowhere, Places.CCasino, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.CMarryc, Places.Nowhere, Places.CStreet, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.CCasino, Places.C21Room, Places.Nowhere, Places.CLobby, Places.CStreet, Places.Nowhere, Places.Nowhere);
        Set(Places.C21Room, Places.Nowhere, Places.CCasino, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.CLobby, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.CCasino, Places.CHtdesk, Places.Nowhere);
        Set(Places.CHmoons, Places.CHallwy, Places.Nowhere, Places.CBalcny, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.CHallwy, Places.Nowhere, Places.Nowhere, Places.CHtdesk, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.CBalcny, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.CHmoons, Places.Nowhere, Places.Nowhere);
        Set(Places.CHtdesk, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.CHallwy, Places.Nowhere, Places.CLobby);

        Set(Places.DTelbth, Places.DDisco, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.DDisco, Places.Nowhere, Places.DTelbth, Places.DEntrnc, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.DStreet, Places.DEntrnc, Places.Nowhere, Places.DPhrmcy, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.DEntrnc, Places.Nowhere, Places.DStreet, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.DPhrmcy, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.DStreet, Places.Nowhere, Places.Nowhere);

        Set(Places.PPntfoy, Places.Nowhere, Places.Nowhere, Places.PKitchn, Places.Nowhere, Places.PLivrom, Places.Nowhere);
        Set(Places.PJacuzi, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.PPntpch, Places.Nowhere);
        Set(Places.PKitchn, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.PPntfoy, Places.Nowhere, Places.Nowhere);
        Set(Places.PGarden, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.PLivrom, Places.PPntpch, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.PPntfoy);
        Set(Places.PPntpch, Places.Nowhere, Places.PLivrom, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.PJacuzi);

        Set(Places.YouHaveIt, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere);
        Set(Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere, Places.Nowhere);

        return p;
    }

    public static Places PathAt(Places[,] path, Places place, Directions dir) =>
        path[(int)place, (int)dir];

    public static void SetPath(Places[,] path, Places place, Directions dir, Places dest) =>
        path[(int)place, (int)dir] = dest;
}
