using IniParser;
using IniParser.Model;
using Microsoft.Extensions.Logging;
using WheelWizard.ControllerSettings;
using WheelWizard.Services;

namespace WheelWizard.Dolphin;

public class DolphinControllerService
{
    private readonly ILogger<DolphinControllerService> _logger;
    private readonly List<DolphinControllerProfile> _profiles;
    private readonly Dictionary<string, DolphinControllerMapping> _mappings;

    public DolphinControllerService(ILogger<DolphinControllerService> logger)
    {
        _logger = logger;
        _profiles = new();
        _mappings = new();

        InitializeDefaultMappings();
        LoadExistingProfiles();
    }

    public List<DolphinControllerProfile> GetProfiles() => _profiles.ToList();

    public DolphinControllerProfile? GetProfile(string name) => _profiles.FirstOrDefault(p => p.Name == name);

    public bool CreateProfile(string name, ControllerInfo controller, DolphinControllerMapping mapping)
    {
        try
        {
            if (_profiles.Any(p => p.Name == name))
            {
                _logger.LogWarning("Profile with name '{Name}' already exists", name);
                return false;
            }

            var profile = new DolphinControllerProfile
            {
                Name = name,
                ControllerType = controller.ControllerType,
                Mapping = mapping,
                CreatedAt = DateTime.Now,
                IsActive = false,
            };

            _profiles.Add(profile);
            SaveProfileToDolphin(profile);

            _logger.LogInformation("Created controller profile '{Name}' for {Type}", name, controller.ControllerType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create controller profile '{Name}'", name);
            return false;
        }
    }

    public bool DeleteProfile(string name)
    {
        try
        {
            var profile = _profiles.FirstOrDefault(p => p.Name == name);
            if (profile == null)
            {
                _logger.LogWarning("Profile '{Name}' not found for deletion", name);
                return false;
            }

            _profiles.Remove(profile);
            DeleteProfileFromDolphin(profile);

            _logger.LogInformation("Deleted controller profile '{Name}'", name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete controller profile '{Name}'", name);
            return false;
        }
    }

    public bool SetActiveProfile(string profileName, int playerIndex = 1)
    {
        try
        {
            var profile = _profiles.FirstOrDefault(p => p.Name == profileName);
            if (profile == null)
            {
                _logger.LogWarning("Profile '{Name}' not found", profileName);
                return false;
            }

            foreach (var p in _profiles)
                p.IsActive = false;

            profile.IsActive = true;

            ApplyProfileToDolphin(profile, playerIndex);

            _logger.LogInformation("Activated controller profile '{Name}' for player {Player}", profileName, playerIndex);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate controller profile '{Name}'", profileName);
            return false;
        }
    }

    public bool UpdateProfile(string originalName, DolphinControllerProfile updatedProfile)
    {
        try
        {
            var existingProfile = _profiles.FirstOrDefault(p => p.Name == originalName);
            if (existingProfile == null)
            {
                _logger.LogWarning("Profile '{Name}' not found for update", originalName);
                return false;
            }

            // Check for name conflicts if name changed
            if (originalName != updatedProfile.Name && _profiles.Any(p => p.Name == updatedProfile.Name))
            {
                _logger.LogWarning("Profile with name '{Name}' already exists", updatedProfile.Name);
                return false;
            }

            // Update the profile
            existingProfile.Name = updatedProfile.Name;
            existingProfile.ControllerType = updatedProfile.ControllerType;
            existingProfile.Mapping = updatedProfile.Mapping;
            existingProfile.IsActive = updatedProfile.IsActive;

            // Save to Dolphin
            SaveProfileToDolphin(existingProfile);

            // If this is now the active profile, apply it
            if (existingProfile.IsActive)
            {
                ApplyProfileToDolphin(existingProfile, 1);
            }

            _logger.LogInformation("Updated controller profile '{Name}'", existingProfile.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update controller profile '{Name}'", originalName);
            return false;
        }
    }

    public DolphinControllerMapping GetMappingForControllerType(ControllerType controllerType)
    {
        var key = controllerType.ToString();
        return _mappings.TryGetValue(key, out var mapping) ? mapping : _mappings["Default"];
    }

    private void InitializeDefaultMappings()
    {
        // GameCube controller mapping (standard layout)
        var gamecubeMapping = new DolphinControllerMapping
        {
            Name = "GameCube Standard",
            Description = "Standard GameCube controller layout optimized for Mario Kart",
            ButtonMappings = new Dictionary<string, string>
            {
                // Face buttons
                ["Buttons/A"] = "`Button A`",
                ["Buttons/B"] = "`Button B`",
                ["Buttons/X"] = "`Button X`",
                ["Buttons/Y"] = "`Button Y`",

                // Triggers
                ["Triggers/L"] = "`Trigger L`",
                ["Triggers/R"] = "`Trigger R`",

                // D-Pad
                ["D-Pad/Up"] = "`Pad N`",
                ["D-Pad/Down"] = "`Pad S`",
                ["D-Pad/Left"] = "`Pad W`",
                ["D-Pad/Right"] = "`Pad E`",

                // Control Stick
                ["Main Stick/Up"] = "`Left Y+`",
                ["Main Stick/Down"] = "`Left Y-`",
                ["Main Stick/Left"] = "`Left X-`",
                ["Main Stick/Right"] = "`Left X+`",

                // C-Stick
                ["C-Stick/Up"] = "`Right Y+`",
                ["C-Stick/Down"] = "`Right Y-`",
                ["C-Stick/Left"] = "`Right X-`",
                ["C-Stick/Right"] = "`Right X+`",

                // Shoulder buttons
                ["Buttons/Z"] = "`Shoulder R`",

                // Start button
                ["Buttons/Start"] = "Start",
            },
        };

        // Wii Remote + Nunchuk mapping for Mario Kart
        var wiimoteMapping = new DolphinControllerMapping
        {
            Name = "Wii Remote + Nunchuk",
            Description = "Wii Remote with Nunchuk for Mario Kart Wii",
            ButtonMappings = new Dictionary<string, string>
            {
                // Wii Remote buttons
                ["Buttons/A"] = "`Button A`",
                ["Buttons/B"] = "`Trigger R`",
                ["Buttons/1"] = "`Button X`",
                ["Buttons/2"] = "`Button Y`",
                ["Buttons/+"] = "Start",
                ["Buttons/-"] = "Back",

                // D-Pad
                ["D-Pad/Up"] = "`Pad N`",
                ["D-Pad/Down"] = "`Pad S`",
                ["D-Pad/Left"] = "`Pad W`",
                ["D-Pad/Right"] = "`Pad E`",

                // IR Pointer (for menu navigation)
                ["IR/Up"] = "`Right Y+`",
                ["IR/Down"] = "`Right Y-`",
                ["IR/Left"] = "`Right X-`",
                ["IR/Right"] = "`Right X+`",

                // Nunchuk
                ["Nunchuk/Buttons/C"] = "`Shoulder L`",
                ["Nunchuk/Buttons/Z"] = "`Trigger L`",
                ["Nunchuk/Stick/Up"] = "`Left Y+`",
                ["Nunchuk/Stick/Down"] = "`Left Y-`",
                ["Nunchuk/Stick/Left"] = "`Left X-`",
                ["Nunchuk/Stick/Right"] = "`Left X+`",

                // Motion controls
                ["Shake/X"] = "`Button B`",
                ["Shake/Y"] = "`Button B`",
                ["Shake/Z"] = "`Button B`",
            },
        };

        _mappings["Xbox"] = gamecubeMapping;
        _mappings["PlayStation"] = gamecubeMapping;
        _mappings["Generic"] = gamecubeMapping;
        _mappings["Default"] = gamecubeMapping;
        _mappings["Wiimote"] = wiimoteMapping;
    }

    private void LoadExistingProfiles()
    {
        try
        {
            var profilesPath = Path.Combine(PathManager.ConfigFolderPath, "Profiles", "GCPad");
            if (!Directory.Exists(profilesPath))
            {
                _logger.LogInformation("No existing controller profiles found");
                return;
            }

            var profileFiles = Directory.GetFiles(profilesPath, "*.ini");
            foreach (var file in profileFiles)
            {
                try
                {
                    var profileName = Path.GetFileNameWithoutExtension(file);
                    var profile = LoadProfileFromFile(file, profileName);
                    if (profile != null)
                    {
                        _profiles.Add(profile);
                        _logger.LogDebug("Loaded existing profile: {Name}", profileName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load profile from {File}", file);
                }
            }

            _logger.LogInformation("Loaded {Count} existing controller profiles", _profiles.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load existing controller profiles");
        }
    }

    private DolphinControllerProfile? LoadProfileFromFile(string filePath, string profileName)
    {
        try
        {
            var parser = new FileIniDataParser();
            IniData data;

            try
            {
                data = parser.ReadFile(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read INI file {File}", filePath);
                return null;
            }

            var profile = new DolphinControllerProfile
            {
                Name = profileName,
                ControllerType = ControllerType.Generic, // Default, could be enhanced to detect from file
                Mapping = new DolphinControllerMapping
                {
                    Name = profileName,
                    Description = $"Loaded from {Path.GetFileName(filePath)}",
                    ButtonMappings = new Dictionary<string, string>(),
                },
                CreatedAt = File.GetCreationTime(filePath),
                IsActive = false,
            };

            // Parse the INI data to extract button mappings
            foreach (var section in data.Sections)
            {
                foreach (var key in section.Keys)
                {
                    var mappingKey = $"{section.SectionName}/{key.KeyName}";
                    profile.Mapping.ButtonMappings[mappingKey] = key.Value;
                }
            }

            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse profile file {File}", filePath);
            return null;
        }
    }

    private void SaveProfileToDolphin(DolphinControllerProfile profile)
    {
        try
        {
            var profilesPath = Path.Combine(PathManager.ConfigFolderPath, "Profiles", "GCPad");
            Directory.CreateDirectory(profilesPath);

            var filePath = Path.Combine(profilesPath, $"{profile.Name}.ini");
            var parser = new FileIniDataParser();
            var data = new IniData();

            // Group mappings by section
            var sections = new Dictionary<string, Dictionary<string, string>>();

            foreach (var mapping in profile.Mapping.ButtonMappings)
            {
                var parts = mapping.Key.Split('/', 2);
                if (parts.Length == 2)
                {
                    var section = parts[0];
                    var key = parts[1];

                    if (!sections.ContainsKey(section))
                        sections[section] = new Dictionary<string, string>();

                    sections[section][key] = mapping.Value;
                }
            }

            // Add sections to INI data
            foreach (var section in sections)
            {
                foreach (var kvp in section.Value)
                {
                    data[section.Key][kvp.Key] = kvp.Value;
                }
            }

            parser.WriteFile(filePath, data);
            _logger.LogDebug("Saved profile '{Name}' to {Path}", profile.Name, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save profile '{Name}' to Dolphin", profile.Name);
        }
    }

    private void DeleteProfileFromDolphin(DolphinControllerProfile profile)
    {
        try
        {
            var profilesPath = Path.Combine(PathManager.ConfigFolderPath, "Profiles", "GCPad");
            var filePath = Path.Combine(profilesPath, $"{profile.Name}.ini");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogDebug("Deleted profile file: {Path}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete profile file for '{Name}'", profile.Name);
        }
    }

    private void ApplyProfileToDolphin(DolphinControllerProfile profile, int playerIndex)
    {
        try
        {
            var configPath = Path.Combine(PathManager.ConfigFolderPath, "GCPadNew.ini");
            var parser = new FileIniDataParser();
            IniData data;

            try
            {
                data = parser.ReadFile(configPath);
            }
            catch (FileNotFoundException)
            {
                data = new IniData();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read existing GCPadNew.ini, creating new file");
                data = new IniData();
            }

            var sectionName = $"GCPad{playerIndex}";
            data[sectionName]["Device"] = "XInput/0/Gamepad"; // Default to XInput, could be made configurable
            data[sectionName]["Profile"] = profile.Name;

            parser.WriteFile(configPath, data);

            UpdateGameSpecificControllerConfig(profile, playerIndex);

            _logger.LogInformation("Applied profile '{Name}' to Dolphin for player {Player}", profile.Name, playerIndex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply profile '{Name}' to Dolphin", profile.Name);
        }
    }

    private void UpdateGameSpecificControllerConfig(DolphinControllerProfile profile, int playerIndex)
    {
        try
        {
            // Mario Kart Wii game IDs
            var gameIds = new[] { "RMCE01", "RMCJ01", "RMCK01", "RMCP01" }; // NTSC-U, NTSC-J, PAL, etc.

            foreach (var gameId in gameIds)
            {
                var gameIniPath = Path.Combine(PathManager.ConfigFolderPath, "GameSettings", $"{gameId}.ini");
                if (!File.Exists(gameIniPath))
                    continue;

                var parser = new FileIniDataParser();
                IniData data;

                try
                {
                    data = parser.ReadFile(gameIniPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read game config file {GameId}, skipping", gameId);
                    continue;
                }

                data["Controls"][$"PadProfile{playerIndex}"] = profile.Name;
                data["Controls"][$"PadType{playerIndex - 1}"] = "6"; // Standard Controller

                parser.WriteFile(gameIniPath, data);
                _logger.LogDebug("Updated game-specific config for {GameId}", gameId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update game-specific controller config");
        }
    }
}
