using System.IO.Abstractions;
using System.Text;
using System.Text.RegularExpressions;
using WheelWizard.Models.Enums;
using WheelWizard.Models.GameData;
using WheelWizard.Services;
using WheelWizard.Services.LiveData;
using WheelWizard.Services.Other;
using WheelWizard.Services.Settings;
using WheelWizard.Services.WiiManagement.SaveData;
using WheelWizard.Utilities.Generators;
using WheelWizard.Utilities.RepeatedTasks;
using WheelWizard.WheelWizardData;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.WiiManagement;

// big big thanks to https://kazuki-4ys.github.io/web_apps/FaceThief/ for the JS implementation
public interface IGameDataSingletonService
{
    /// <summary>
    /// Gets the currently loaded <see cref="Models.GameData.LicenseCollection"/>.
    /// </summary>
    LicenseCollection LicenseCollection { get; }

    /// <summary>
    /// Loads the game data from the rksys.dat file.
    /// </summary>
    OperationResult LoadGameData();

    /// <summary>
    /// Retrieves the user data for a specific index.
    /// </summary>
    /// <param name="index">Index of user (1-3)</param>
    LicenseProfile GetUserData(int index);

    /// <summary>
    /// Gets the currently selected user.
    /// </summary>
    LicenseProfile CurrentUser { get; }

    /// <summary>
    /// Gets the list of friends for the currently selected user.
    /// </summary>
    List<FriendProfile> CurrentFriends { get; }

    /// <summary>
    /// Checks if any user is valid (i.e., has a non-empty friend code).
    /// </summary>
    bool HasAnyValidUsers { get; }

    /// <summary>
    /// Refreshes the online status of the users based on the current live rooms.
    /// </summary>
    void RefreshOnlineStatus();

    /// <summary>
    /// Changes the name of a Mii for a specific user index.
    /// </summary>
    OperationResult ChangeMiiName(int userIndex, string newName);

    /// <summary>
    /// Subscribes a listener to the repeated task manager.
    /// </summary>
    void Subscribe(IRepeatedTaskListener subscriber);
}

public class GameDataSingletonService : RepeatedTaskManager, IGameDataSingletonService
{
    private readonly IMiiDbService _miiService;
    private readonly IFileSystem _fileSystem;
    private readonly IWhWzDataSingletonService _whWzDataSingletonService;
    private LicenseCollection UserList { get; }
    private byte[]? _saveData;

    public GameDataSingletonService(IMiiDbService miiService, IFileSystem fileSystem, IWhWzDataSingletonService whWzDataSingletonService)
        : base(40)
    {
        _miiService = miiService;
        _fileSystem = fileSystem;
        _whWzDataSingletonService = whWzDataSingletonService;
        UserList = new();
    }

    private const int RksysSize = 0x2BC000;
    private const string RksysMagic = "RKSD0006";
    private const int MaxPlayerNum = 4;
    private const int RkpdSize = 0x8CC0;
    private const string RkpdMagic = "RKPD";
    private const int MaxFriendNum = 30;
    private const int FriendDataOffset = 0x56D0;
    private const int FriendDataSize = 0x1C0;
    private const int MiiSize = 0x4A;

    /// <summary>
    /// Returns the "focused" or currently active license/user as determined by the Settings.
    /// </summary>
    public LicenseProfile CurrentUser => UserList.Users[(int)SettingsManager.FOCUSSED_USER.Get()];

    public List<FriendProfile> CurrentFriends => UserList.Users[(int)SettingsManager.FOCUSSED_USER.Get()].Friends;

    public LicenseCollection LicenseCollection => UserList;

    public LicenseProfile GetUserData(int index) => UserList.Users[index];

    public bool HasAnyValidUsers => UserList.Users.Any(user => user.FriendCode != "0000-0000-0000");

    public void RefreshOnlineStatus()
    {
        var currentRooms = RRLiveRooms.Instance.CurrentRooms;
        var onlinePlayers = currentRooms.SelectMany(room => room.Players.Values).ToList();
        foreach (var user in UserList.Users)
        {
            user.IsOnline = onlinePlayers.Any(player => player.Fc == user.FriendCode);
        }
    }

    public OperationResult LoadGameData()
    {
        var loadSaveDataResult = LoadSaveDataFile();
        if (loadSaveDataResult.IsFailure)
            _saveData = null;
        else
            _saveData = loadSaveDataResult.Value;
        if (_saveData != null && ValidateMagicNumber())
        {
            var result = ParseUsers();
            if (result.IsFailure)
                return result;
            return Ok();
        }

        // If the file was invalid or not found, create 4 dummy licenses
        UserList.Users.Clear();
        for (var i = 0; i < MaxPlayerNum; i++)
            UserList.Users.Add(CreateDummyUser());
        return Ok();
    }

    private LicenseProfile CreateDummyUser()
    {
        var noLicenseName = new MiiName("no license");
        var dummyUser = new LicenseProfile
        {
            FriendCode = "0000-0000-0000",
            MiiData = new()
            {
                Mii = new() { Name = noLicenseName },
                AvatarId = 0,
                ClientId = 0,
            },
            Vr = 5000,
            Br = 5000,
            TotalRaceCount = 0,
            TotalWinCount = 0,
            Friends = [],
            RegionId = 10, // 10 => “unknown”
            IsOnline = false,
        };
        return dummyUser;
    }

    private OperationResult ParseUsers()
    {
        UserList.Users.Clear();
        if (_saveData == null)
            return new ArgumentNullException(nameof(_saveData));

        for (var i = 0; i < MaxPlayerNum; i++)
        {
            var rkpdOffset = RksysMagic.Length + i * RkpdSize;
            var rkpdCheck = Encoding.ASCII.GetString(_saveData, rkpdOffset, RkpdMagic.Length) == RkpdMagic;
            if (!rkpdCheck)
            {
                UserList.Users.Add(CreateDummyUser());
                continue;
            }

            var user = ParseUser(rkpdOffset);
            if (user.IsFailure)
            {
                UserList.Users.Add(CreateDummyUser());
                continue;
            }
            UserList.Users.Add(user.Value);
        }

        // Keep this here so we always have 4 users if the code above were to be changed
        while (UserList.Users.Count < 4)
        {
            UserList.Users.Add(CreateDummyUser());
        }

        return Ok();
    }

    private OperationResult<LicenseProfile> ParseUser(int offset)
    {
        if (_saveData == null)
            return new ArgumentNullException(nameof(_saveData));

        var friendCode = FriendCodeGenerator.GetFriendCode(_saveData, offset + 0x5C);
        var miiDataResult = ParseMiiData(offset + 0x14);
        var miiToUse = miiDataResult.IsFailure ? new() { Mii = new() { Name = new("no name") } } : miiDataResult.Value;
        var user = new LicenseProfile
        {
            MiiData = miiToUse,
            FriendCode = friendCode,
            Vr = BigEndianBinaryReader.BufferToUint16(_saveData, offset + 0xB0),
            Br = BigEndianBinaryReader.BufferToUint16(_saveData, offset + 0xB2),
            TotalRaceCount = BigEndianBinaryReader.BufferToUint32(_saveData, offset + 0xB4),
            TotalWinCount = BigEndianBinaryReader.BufferToUint32(_saveData, offset + 0xDC),
            BadgeVariants = _whWzDataSingletonService.GetBadges(friendCode),
            // Region is often found near offset 0x23308 + 0x3802 in RKGD. This code is a partial guess.
            // In practice, region might be read differently depending on your rksys layout.
            RegionId = BigEndianBinaryReader.BufferToUint16(_saveData, 0x23308 + 0x3802) / 4096,
        };

        ParseFriends(user, offset);
        return user;
    }

    private OperationResult<MiiData> ParseMiiData(int offset)
    {
        if (_saveData == null)
            return new ArgumentNullException(nameof(_saveData));

        // In Mario Kart Wii's rksys, offset +0x10 => AvatarId, offset +0x14 => ClientId
        // The name is big-endian UTF-16 at offset itself (length 10 chars => 20 bytes).
        var name = BigEndianBinaryReader.GetUtf16String(_saveData, offset, 10);
        var avatarId = BitConverter.ToUInt32(_saveData, offset + 0x10);
        var clientId = BitConverter.ToUInt32(_saveData, offset + 0x14);

        var rawMiiResult = _miiService.GetByClientId(clientId);
        if (rawMiiResult.IsFailure)
            return new FormatException("Failed to parse mii data: " + rawMiiResult.Error.Message);

        var miiData = new MiiData
        {
            Mii = rawMiiResult.Value,
            AvatarId = avatarId,
            ClientId = clientId,
        };
        return miiData;
    }

    private void ParseFriends(LicenseProfile licenseProfile, int userOffset)
    {
        if (_saveData == null)
            return;

        var friendOffset = userOffset + FriendDataOffset;
        for (var i = 0; i < MaxFriendNum; i++)
        {
            var currentOffset = friendOffset + i * FriendDataSize;
            if (!CheckForMiiData(currentOffset + 0x1A))
                continue;

            byte[] rawMiiBytes = _saveData.AsSpan(currentOffset + 0x1A, MiiSize).ToArray();
            var friendCode = FriendCodeGenerator.GetFriendCode(_saveData, currentOffset + 4);
            var miiResult = MiiSerializer.Deserialize(rawMiiBytes);
            if (miiResult.IsFailure)
                continue;

            var friend = new FriendProfile
            {
                Vr = BigEndianBinaryReader.BufferToUint16(_saveData, currentOffset + 0x16),
                Br = BigEndianBinaryReader.BufferToUint16(_saveData, currentOffset + 0x18),
                FriendCode = friendCode,
                Wins = BigEndianBinaryReader.BufferToUint16(_saveData, currentOffset + 0x14),
                Losses = BigEndianBinaryReader.BufferToUint16(_saveData, currentOffset + 0x12),
                CountryCode = _saveData[currentOffset + 0x68],
                RegionId = _saveData[currentOffset + 0x69],
                BadgeVariants = _whWzDataSingletonService.GetBadges(friendCode),
                MiiData = new()
                {
                    Mii = miiResult.Value,
                    AvatarId = 0,
                    ClientId = 0,
                },
            };
            licenseProfile.Friends.Add(friend);
        }
    }

    private bool CheckForMiiData(int offset)
    {
        // If the entire 0x4A bytes are zero, we treat it as empty / no Mii data
        for (var i = 0; i < MiiSize; i++)
        {
            if (_saveData != null && _saveData[offset + i] != 0)
                return true;
        }

        return false;
    }

    private bool ValidateMagicNumber()
    {
        if (_saveData == null)
            return false;
        return Encoding.ASCII.GetString(_saveData, 0, RksysMagic.Length) == RksysMagic;
    }

    private OperationResult<byte[]> LoadSaveDataFile()
    {
        try
        {
            if (!Directory.Exists(PathManager.SaveFolderPath))
                return "Save folder not found";

            var currentRegion = (MarioKartWiiEnums.Regions)SettingsManager.RR_REGION.Get();
            if (currentRegion == MarioKartWiiEnums.Regions.None)
            {
                // Double check if there's at least one valid region
                var validRegions = RRRegionManager.GetValidRegions();
                if (validRegions.First() != MarioKartWiiEnums.Regions.None)
                {
                    currentRegion = validRegions.First();
                    SettingsManager.RR_REGION.Set(currentRegion);
                }
                else
                {
                    return "No valid regions found";
                }
            }

            var saveFileFolder = _fileSystem.Path.Combine(PathManager.SaveFolderPath, RRRegionManager.ConvertRegionToGameId(currentRegion));
            var saveFile = _fileSystem.Directory.GetFiles(saveFileFolder, "rksys.dat", SearchOption.TopDirectoryOnly);
            if (saveFile.Length == 0)
                return "rksys.dat not found";
            return _fileSystem.File.ReadAllBytes(saveFile[0]);
        }
        catch
        {
            return "Failed to load rksys.dat";
        }
    }

    /// <summary>
    /// Calculates the CRC32 of the specified slice of bytes using the
    /// standard polynomial (0xEDB88320) in the same way MKWii does.
    /// </summary>
    public static uint ComputeCrc32(byte[] data, int offset, int length)
    {
        const uint POLY = 0xEDB88320;
        var crc = 0xFFFFFFFF;

        for (var i = offset; i < offset + length; i++)
        {
            var b = data[i];
            crc ^= b;
            for (var j = 0; j < 8; j++)
            {
                if ((crc & 1) != 0)
                    crc = (crc >> 1) ^ POLY;
                else
                    crc >>= 1;
            }
        }

        return ~crc;
    }

    /// <summary>
    /// Fixes the MKWii save file by recalculating and inserting the CRC32 at 0x27FFC.
    /// </summary>
    public static void FixRksysCrc(byte[] rksysData)
    {
        if (rksysData == null || rksysData.Length < RksysSize)
            throw new ArgumentException("Invalid rksys.dat data");

        var lengthToCrc = 0x27FFC;
        var newCrc = ComputeCrc32(rksysData, 0, lengthToCrc);

        // 2) Write CRC at offset 0x27FFC in big-endian.
        BigEndianBinaryReader.WriteUInt32BigEndian(rksysData, 0x27FFC, newCrc);
    }

    public OperationResult ChangeMiiName(int userIndex, string? newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return "Cannot set name to an empty name.";
        if (userIndex is < 0 or >= MaxPlayerNum)
            return "Invalid license index. Please select a valid license.";

        var user = UserList.Users[userIndex];
        var miiIsEmptyOrNoName = IsNoNameOrEmptyMii(user);

        if (miiIsEmptyOrNoName)
            return "This license has no Mii data or is incomplete.\n" + "Please use the Mii Channel to create a Mii first.";

        if (user.MiiData?.Mii == null)
            return "This license has no Mii data or is incomplete.\n" + "Please use the Mii Channel to create a Mii first.";

        newName = Regex.Replace(newName, @"\s+", " ");

        // Basic checks
        if (newName.Length is > 10 or < 3)
            return "Names must be between 3 and 10 characters long.";

        if (newName.Length > 10)
            newName = newName.Substring(0, 10);
        var nameResult = MiiName.Create(newName);
        if (nameResult.IsFailure)
            return nameResult.Error.Message;

        user.Mii.Name = nameResult.Value;
        var nameWrite = WriteLicenseNameToSaveData(userIndex, newName);
        if (nameWrite.IsFailure)
            return nameWrite.Error.Message;
        var updated = _miiService.UpdateName(user.MiiData.ClientId, newName);
        if (updated.IsFailure)
            return updated.Error.Message;
        var rksysSaveResult = SaveRksysToFile();
        if (rksysSaveResult.IsFailure)
            return rksysSaveResult.Error.Message;

        return Ok();
    }

    private bool IsNoNameOrEmptyMii(LicenseProfile user)
    {
        if (user?.MiiData?.Mii == null)
            return true;

        var name = user.MiiData.Mii.Name;
        if (name.ToString() == "no name")
            return true;
        var raw = MiiSerializer.Serialize(user.MiiData.Mii).Value;
        if (raw.Length != 74)
            return true; // Not valid
        if (raw.All(b => b == 0))
            return true;

        // Otherwise, it’s presumably valid
        return false;
    }

    private OperationResult WriteLicenseNameToSaveData(int userIndex, string newName)
    {
        if (_saveData == null || _saveData.Length < RksysSize)
            return "Invalid save data";
        var rkpdOffset = 0x8 + userIndex * RkpdSize;
        var nameOffset = rkpdOffset + 0x14;
        var nameBytes = Encoding.BigEndianUnicode.GetBytes(newName);
        for (var i = 0; i < 20; i++)
            _saveData[nameOffset + i] = 0;
        Array.Copy(nameBytes, 0, _saveData, nameOffset, Math.Min(nameBytes.Length, 20));
        return Ok();
    }

    private OperationResult SaveRksysToFile()
    {
        if (_saveData == null || !SettingsHelper.PathsSetupCorrectly())
            return Fail("Invalid save data or config is not setup properly.");
        FixRksysCrc(_saveData);
        var currentRegion = (MarioKartWiiEnums.Regions)SettingsManager.RR_REGION.Get();
        var saveFolder = _fileSystem.Path.Combine(PathManager.SaveFolderPath, RRRegionManager.ConvertRegionToGameId(currentRegion));
        var trySaveRksys = TryCatch(() =>
        {
            _fileSystem.Directory.CreateDirectory(saveFolder);
            var path = _fileSystem.Path.Combine(saveFolder, "rksys.dat");
            _fileSystem.File.WriteAllBytes(path, _saveData);
        });
        if (trySaveRksys.IsFailure)
            return trySaveRksys.Error.Message;
        return Ok();
    }

    protected override Task ExecuteTaskAsync()
    {
        var result = LoadGameData();
        if (result.IsFailure)
        {
            throw new(result.Error.Message);
        }

        return Task.CompletedTask;
    }
}
