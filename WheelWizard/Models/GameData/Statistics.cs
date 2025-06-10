namespace WheelWizard.Models.GameData;

public class Statistics
{
    public RaceTotals RaceTotals { get; set; } = new();
    public PreferredControls Controls { get; set; } = new();
    public Performance Performance { get; init; } = new();
    public RaceCompletions RaceCompletions { get; init; } = new();
    public PreferredControls PreferredControls { get; init; } = new();
    public BattleCompletions BattleCompletions { get; init; } = new();
    public ushort TotalCompetitions { get; init; }
    public TrophyCabinet? Trophies { get; init; }
}

public class TrophyCabinet
{
    /// <summary>
    /// Key = “Mushroom Cup 50cc”, Value = TrophyInfo
    /// </summary>
    public Dictionary<string, TrophyInfo> PerCup { get; init; } = new();
}

/// <summary>
/// Single cup’s result: which trophy and what star rank.
/// </summary>
public class TrophyInfo
{
    public CupTrophyType CupType { get; set; } // Gold, Silver, Bronze, None
    public CupRank Rank { get; set; } // ThreeStars, TwoStars, OneStar, A, B, C, D, E, F
    public bool Completed { get; set; } // bit 0x52: 1 = completed
}

public enum CupTrophyType
{
    Gold = 0,
    Silver = 1,
    Bronze = 2,
    None = 3,
}

public enum CupRank
{
    ThreeStars = 0,
    TwoStars = 1,
    OneStar = 2,
    A = 3,
    B = 4,
    C = 5,
    D = 6,
    E = 7,
    F = 8,
}

public class BattleCompletions
{
    /// <summary>
    /// Key = Battle Stage, Value = Amount of times the stage was played
    /// </summary>
    public Dictionary<Stage, int> Stage { get; set; } = new();
}

// The 10 battle stages in the order they appear in RKPD (0x1A6 onward).
public enum Stage : byte
{
    DelfinoPier = 0,
    BlockPlaza,
    ChainChompWheel,
    FunkyStadium,
    ThwompDesert,
    GCNCookieLand,
    DSTwilightHouse,
    SNESBattleCourse4,
    GBABattleCourse3,
    N64Skyscraper,
}

public class RaceCompletions
{
    public Dictionary<Character, int> Character { get; set; } = new();

    /// <summary>
    /// Key = Vehicle enum, Value = races completed count.
    /// Uses 36 entries starting at 0x11E.
    /// </summary>
    public Dictionary<Vehicle, int> Vehicle { get; init; } = new();

    /// <summary>
    /// Key = Course enum, Value = races completed count.
    /// Uses 32 entries starting at 0x166.
    /// </summary>
    public Dictionary<Course, int> Course { get; init; } = new();
}

public enum Course : byte
{
    MarioCircuit = 0,
    MooMooMeadows,
    MushroomGorge,
    GrumbleVolcano,
    ToadsFactory,
    CoconutMall,
    DKSummit,
    WariosGoldMine,
    LuigiCircuit,
    DaisyCircuit,
    MoonviewHighway,
    MapleTreeway,
    BowserCastle,
    RainbowRoad,
    DryDryRuins,
    KoopaCape,
    GCNPeachBeach,
    GCNMarioCircuit,
    GCNWaluigiStadium,
    GCNDKMountain,
    DSYoshiFalls,
    DSDesertHills,
    DSPeachGardens,
    DSDelfinoSquare,
    SNESMarioCircuit3,
    SNESGhostValley2,
    N64MarioRaceway,
    N64SherbetLand,
    N64BowsersCastle,
    N64DKsJungleParkway,
    GBABowserCastle3,
    GBAShyGuyBeach,
}

public enum Vehicle : byte
{
    StandardKartS = 0,
    StandardKartM,
    StandardKartL,
    BoosterSeat,
    ClassicDragster,
    Offroader,
    MiniBeast,
    WildWing,
    FlameFlyer,
    CheepCharger,
    SuperBlooper,
    PiranhaProwler,
    TinyTitan,
    Daytripper,
    Jetsetter,
    BlueFalcon,
    Sprinter,
    Honeycoupe,
    StandardBikeS,
    StandardBikeM,
    StandardBikeL,
    BulletBike,
    MachBike,
    FlameRunner,
    BitBike,
    Sugarscoot,
    WarioBike,
    Quacker,
    ZipZip,
    ShootingStar,
    Magikruiser,
    Sneakster,
    Spear,
    JetBubble,
    DolphinDasher,
    Phantom,
}

// The 24 playable characters in the order they appear in RKPD (0xEC onward).
public enum Character : byte
{
    Mario = 0,
    BabyPeach,
    Waluigi,
    Bowser,
    BabyDaisy,
    DryBones,
    BabyMario,
    Luigi,
    Toad,
    DonkeyKong,
    Yoshi,
    Wario,
    BabyLuigi,
    Toadette,
    KoopaTroopa,
    Daisy,
    Peach,
    Birdo,
    DiddyKong,
    KingBoo,
    BowserJr,
    DryBowser,
    FunkyKong,
    Rosalina,
    Mii = 23,
}

public class Performance
{
    public int TricksPerformed { get; set; } // total amount of tricks performed
    public int ItemHitsDealt { get; set; } // total amount of item hits dealt
    public int ItemHitsReceived { get; set; } // total amount of item hits received
    public int FirstPlaces { get; set; } // total amount of first places achieved
    public float DistanceTotal { get; set; } // Total Distance driven
    public float DistanceInFirstPlace { get; set; } // Total Distance driven in first place
    public float DistanceVsRaces { get; set; }

    // percentage of time in 1st = floor(DistanceInFirstPlace / DistanceVsRaces * 100).

    public int PercentTimeInFirstPlace
    {
        get
        {
            if (DistanceVsRaces == 0)
                return 0;

            return (int)Math.Floor(DistanceInFirstPlace / DistanceVsRaces * 100);
        }
    }
}

public class PreferredControls
{
    public int WiiWheelRaces { get; set; } // amount of matched played with the Wii Wheel
    public int WiiWheelBattles { get; set; } // amount of matched played with the Wii Wheel in battles
    public float WheelWheelUsageRatio
    {
        get
        {
            if (WiiWheelRaces + WiiWheelBattles == 0)
                return 0f;

            return (float)WiiWheelRaces / (WiiWheelRaces + WiiWheelBattles);
        }
    }
    public DriftType PreferredDriftType { get; set; } = DriftType.Standard; // preferred drift type, default is standard
}

public enum DriftType
{
    Standard = 0,
    Manual = 1,
    Automatic = 2,
}

public class RaceTotals
{
    public uint AllRacesCount { get; set; } // total amount of races completed

    public uint OnlineRacesCount
    {
        get
        {
            return WinsVsLosses.OnlineVs.Wins
                + WinsVsLosses.OnlineVs.Losses
                + WinsVsLosses.OnlineBattle.Wins
                + WinsVsLosses.OnlineBattle.Losses;
        }
    }
    public uint BattleMatches { get; set; } // total amount of battle matches completed
    public WinsVsLosses WinsVsLosses { get; set; } = new();
    public int GhostChallengesSent { get; set; } // total amount of ghost challenges sent
    public int GhostChallengesReceived { get; set; } // total amount of ghost challenges received
}

public class WinsVsLosses
{
    public WinLoss OfflineVs { get; set; } = new();
    public WinLoss OfflineBattle { get; set; } = new();
    public WinLoss OnlineVs { get; set; } = new();
    public WinLoss OnlineBattle { get; set; } = new();
}

public class WinLoss
{
    public uint Wins { get; set; }
    public uint Losses { get; set; }
}
