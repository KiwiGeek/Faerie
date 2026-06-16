using System.Text.Json;
using System.Text.Json.Serialization;

namespace Faerie.Samples.Softporn.Interpreter;

public sealed class GameState
{
    public Places[] ObjectPlace { get; set; } = new Places[(int)Objects.NoObject + 1];
    public bool[] PlaceVisited { get; set; } = new bool[(int)Places.Nowhere + 1];
    public Places[,] Path { get; set; } = new Places[(int)Places.Nowhere + 1, 6];

    public Places YourPlace { get; set; }
    public Places NewPlace { get; set; }

    public Objects[] Inven { get; set; } = new Objects[SoftpornConstants.MaxCarried + 1];
    public int Carrying { get; set; } = 1;

    public Objects Noun { get; set; }
    public Verbs Verb { get; set; }
    public Directions Direction { get; set; }

    public string Command { get; set; } = "";
    public string VerbNam { get; set; } = "    ";
    public string ObjNam { get; set; } = "    ";

    public bool GameEnded { get; set; }
    public char YesNo { get; set; }

    public int ObjectsCarried { get; set; }
    public int Money { get; set; }
    public int Score { get; set; }
    public int TvChannel { get; set; }

    public bool RopeInUse { get; set; }
    public bool WindowBroken { get; set; }
    public bool ToiletFlushed { get; set; }
    public bool Called5550987 { get; set; }
    public bool Called5556969 { get; set; }
    public bool Called5550439 { get; set; }
    public bool RubberWorn { get; set; }
    public bool HookerFucked { get; set; }
    public bool DoorWOpen { get; set; }
    public bool RadioListened { get; set; }
    public bool WineOrdered { get; set; }
    public bool TelephoneRinging { get; set; }
    public bool TelephoneAnswered { get; set; }
    public bool HolePeeped { get; set; }
    public bool Girl2Fucked { get; set; }
    public bool TiedToBed { get; set; }
    public bool DrawerOpen { get; set; }
    public bool ClosetOpen { get; set; }
    public bool CabinetOpen { get; set; }
    public bool DollInflated { get; set; }
    public bool StoolClimbed { get; set; }
    public bool WaterOn { get; set; }
    public bool PitcherFull { get; set; }
    public bool SeedsPlanted { get; set; }
    public bool SeedsWatered { get; set; }
    public bool AppleGiven { get; set; }
    public bool CandyGiven { get; set; }
    public bool FlowersGiven { get; set; }
    public bool RingGiven { get; set; }
    public bool MarriedToGirl { get; set; }

    public string GirlName { get; set; } = "";
    public string GirlPart { get; set; } = "";
    public string GirlDo { get; set; } = "";
    public string YourPart { get; set; } = "";
    public string YourObject { get; set; } = "";
    public string RubberColor { get; set; } = "";
    public string RubberFlavor { get; set; } = "";
    public string RubberLubricated { get; set; } = "non-lubricated";
    public string RubberRibbed { get; set; } = "non-ribbed";

    public void ResetFromOrig(bool cheat = false)
    {
        Array.Copy(SoftpornConstants.OrigObjectPlace, ObjectPlace, ObjectPlace.Length);
        for (var place = 0; place <= (int)Places.Nowhere; place++)
        {
            for (var dir = 0; dir < 6; dir++)
                Path[place, dir] = SoftpornConstants.OrigPath[place, dir];
        }

        Array.Fill(PlaceVisited, false);

        YourPlace = Places.BBar;
        ObjectsCarried = 0;
        TvChannel = 0;
        Money = cheat ? 100 : 10;
        Score = 0;

        RopeInUse = false;
        WindowBroken = false;
        ToiletFlushed = false;
        Called5550987 = false;
        Called5556969 = false;
        Called5550439 = false;
        RubberWorn = false;
        HookerFucked = false;
        DoorWOpen = false;
        RadioListened = false;
        WineOrdered = false;
        TelephoneRinging = false;
        TelephoneAnswered = false;
        HolePeeped = false;
        Girl2Fucked = false;
        TiedToBed = false;
        DrawerOpen = false;
        ClosetOpen = false;
        CabinetOpen = false;
        DollInflated = false;
        StoolClimbed = false;
        WaterOn = false;
        PitcherFull = false;
        SeedsPlanted = false;
        SeedsWatered = false;
        AppleGiven = false;
        CandyGiven = false;
        FlowersGiven = false;
        RingGiven = false;
        MarriedToGirl = false;

        GirlName = "";
        GirlPart = "";
        GirlDo = "";
        YourPart = "";
        YourObject = "";
        RubberColor = "";
        RubberFlavor = "";
        RubberLubricated = "non-lubricated";
        RubberRibbed = "non-ribbed";
    }

    public static string SaveFileName(string objnam)
    {
        var trimmed = objnam.TrimEnd();
        var fileName = "SOFTP" + (trimmed.Length >= 3 ? trimmed[..3] : trimmed);
        fileName = fileName.Replace(" ", "");
        if (fileName == "SOFTP")
            fileName = "SOFTPORN";
        return fileName + ".SAV";
    }

    public void SaveTo(string path)
    {
        var json = JsonSerializer.Serialize(this, SaveJsonOptions);
        File.WriteAllText(path, json);
    }

    public static GameState? LoadFrom(string path)
    {
        if (!File.Exists(path))
            return null;
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<GameState>(json, SaveJsonOptions);
    }

    private static readonly JsonSerializerOptions SaveJsonOptions = new()
    {
        WriteIndented = false,
        IncludeFields = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };
}
