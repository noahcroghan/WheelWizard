using System.Text;
using Avalonia.Media.Imaging;
using WheelWizard.MiiImages;
using WheelWizard.Models.MiiImages;
using WheelWizard.Services.WiiManagement.SaveData;
using WheelWizard.Views;

namespace WheelWizard.Services.WiiManagement;

public static class MiiImageManager
{
    private const int WiiMiiFileSize = 0x4A;

    public static int ParsedMiiDataCount => Images.Count;

    private static readonly int[] MakeupMap = { 0, 1, 6, 9, 0, 0, 0, 0, 0, 10, 0, 0 };
    private static readonly int[] WrinklesMap = { 0, 0, 0, 0, 5, 2, 3, 7, 8, 0, 9, 11 };

    // Shared HttpClient instance (better performance than creating new ones)
    private static readonly HttpClient MiiImageHttpClient = new();

    #region Mii Studio Data Generation (Replaces MiiStudioUrl call)

    /// <summary>
    /// Generates the encoded data string required by the Nintendo Mii Image URL.
    /// </summary>
    /// <param name="wiiMiiData">Raw byte data of the Wii Mii (must be 74 bytes).</param>
    /// <returns>The encoded data string, or null if input data is invalid.</returns>
    private static string? GenerateEncodedStudioData(byte[] wiiMiiData)
    {
        if (wiiMiiData == null || wiiMiiData.Length != WiiMiiFileSize)
        {
            // Invalid data length for a Wii Mii
            Console.WriteLine($"Invalid Wii Mii data length: Expected {WiiMiiFileSize}, got {(wiiMiiData?.Length ?? 0)}");
            return null;
        }

        try
        {
            var studio = GenerateStudioDataArray(wiiMiiData);
            return EncodeStudioData(studio);
        }
        catch (Exception ex) // Catch potential errors during parsing (e.g., ArgumentOutOfRangeException)
        {
            // Log the error if needed
            Console.WriteLine($"Error generating studio data: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Parses the Wii Mii data and generates the 46-byte studio data array.
    /// Based on the miiFileRead logic for Wii data in the provided JS.
    /// Uses BigEndianBinaryReader for reading values.
    /// </summary>
    private static byte[] GenerateStudioDataArray(byte[] buf)
    {
        var studio = new byte[46]; // Size of studio data array

        // Parse Wii data fields and map them to studio array indices
        // Offsets and logic match the 'else' block (Wii part) of miiFileRead

        // --- Basic Info ---
        var tmpU16_0 = BigEndianBinaryReader.BufferToUint16(buf, 0);
        var isGirl = ((tmpU16_0 >> 14) & 1) == 1;
        var favColor = (int)((tmpU16_0 >> 1) & 0xF);
        int height = buf[0x16];
        int weight = buf[0x17];

        studio[0x16] = (byte)(isGirl ? 1 : 0); // Gender
        studio[0x15] = (byte)favColor; // Favorite Color
        studio[0x1E] = (byte)height; // Height
        studio[2] = (byte)weight; // Weight (mapped to index 2 in studio)

        // --- Face ---
        var tmpU16_20 = BigEndianBinaryReader.BufferToUint16(buf, 0x20);
        var faceShape = (int)(tmpU16_20 >> 13);
        var skinColor = (int)((tmpU16_20 >> 10) & 7);
        var facialFeature = (int)((tmpU16_20 >> 6) & 0xF); // Note: JS uses 0xF mask here, map to makeup/wrinkles
        var makeup = MakeupMap.Length > facialFeature ? MakeupMap[facialFeature] : 0;
        var wrinkles = WrinklesMap.Length > facialFeature ? WrinklesMap[facialFeature] : 0;

        studio[0x13] = (byte)faceShape;
        studio[0x11] = (byte)skinColor;
        studio[0x14] = (byte)wrinkles;
        studio[0x12] = (byte)makeup;

        // --- Hair ---
        var tmpU16_22 = BigEndianBinaryReader.BufferToUint16(buf, 0x22);
        var hairStyle = (int)(tmpU16_22 >> 9);
        var hairColor = (int)((tmpU16_22 >> 6) & 7);
        var flipHair = (int)((tmpU16_22 >> 5) & 1);

        studio[0x1D] = (byte)hairStyle;
        studio[0x1B] = (byte)(hairColor == 0 ? 8 : hairColor); // Map color 0 to 8
        studio[0x1C] = (byte)flipHair;

        // --- Eyebrows ---
        var tmpU32_24 = BigEndianBinaryReader.BufferToUint32(buf, 0x24);
        var eyebrowStyle = (int)(tmpU32_24 >> 27);
        var eyebrowRotation = (int)((tmpU32_24 >> 22) & 0xF); // Note: JS uses 0xF mask
        var eyebrowColor = (int)((tmpU32_24 >> 13) & 7);
        var eyebrowScale = (int)((tmpU32_24 >> 9) & 0xF);
        var eyebrowYscale = 3; // Hardcoded in JS
        var eyebrowYposition = (int)((tmpU32_24 >> 4) & 0x1F);
        var eyebrowXspacing = (int)(tmpU32_24 & 0xF);

        studio[0xE] = (byte)eyebrowStyle;
        studio[0xC] = (byte)eyebrowRotation;
        studio[0xB] = (byte)(eyebrowColor == 0 ? 8 : eyebrowColor); // Map color 0 to 8
        studio[0xD] = (byte)eyebrowScale;
        studio[0xA] = (byte)eyebrowYscale;
        studio[0x10] = (byte)eyebrowYposition;
        studio[0xF] = (byte)eyebrowXspacing;

        // --- Eyes ---
        var tmpU32_28 = BigEndianBinaryReader.BufferToUint32(buf, 0x28);
        var eyeStyle = (int)(tmpU32_28 >> 26);
        var eyeRotation = (int)((tmpU32_28 >> 21) & 7); // Note: JS uses 7 (0b111) mask
        var eyeYposition = (int)((tmpU32_28 >> 16) & 0x1F);
        var eyeColor = (int)((tmpU32_28 >> 13) & 7);
        var eyeScale = (int)((tmpU32_28 >> 9) & 7); // Note: JS uses 7 mask
        var eyeYscale = 3; // Hardcoded in JS
        var eyeXspacing = (int)((tmpU32_28 >> 5) & 0xF);
        // int unknownEyeBit = (int)(tmpU32_28 & 0x1F); // Lower 5 bits unused in JS mapping

        studio[7] = (byte)eyeStyle;
        studio[5] = (byte)eyeRotation;
        studio[9] = (byte)eyeYposition;
        studio[4] = (byte)(eyeColor + 8); // Map color 0-7 to 8-15
        studio[6] = (byte)eyeScale;
        studio[3] = (byte)eyeYscale;
        studio[8] = (byte)eyeXspacing;

        // --- Nose ---
        var tmpU16_2C = BigEndianBinaryReader.BufferToUint16(buf, 0x2C);
        var noseStyle = (int)(tmpU16_2C >> 12);
        var noseScale = (int)((tmpU16_2C >> 8) & 0xF);
        var noseYposition = (int)((tmpU16_2C >> 3) & 0x1F);
        // int unknownNoseBits = (int)(tmpU16_2C & 7); // Lower 3 bits unused

        studio[0x2C] = (byte)noseStyle;
        studio[0x2B] = (byte)noseScale;
        studio[0x2D] = (byte)noseYposition;

        // --- Mouth ---
        var tmpU16_2E = BigEndianBinaryReader.BufferToUint16(buf, 0x2E);
        var mouseStyle = (int)(tmpU16_2E >> 11);
        var mouseColor = (int)((tmpU16_2E >> 9) & 3); // Lip color (0-3)
        var mouseScale = (int)((tmpU16_2E >> 5) & 0xF);
        var mouseYscale = 3; // Hardcoded in JS
        var mouseYposition = (int)(tmpU16_2E & 0x1F);

        studio[0x26] = (byte)mouseStyle;
        studio[0x24] = (byte)(mouseColor < 4 ? mouseColor + 19 : 0); // Map 0-3 to 19-22, else 0
        studio[0x25] = (byte)mouseScale;
        studio[0x23] = (byte)mouseYscale;
        studio[0x27] = (byte)mouseYposition;

        // --- Beard / Mustache ---
        var tmpU16_32 = BigEndianBinaryReader.BufferToUint16(buf, 0x32);
        var mustacheStyle = (int)(tmpU16_32 >> 14);
        var beardStyle = (int)((tmpU16_32 >> 12) & 3);
        var facialHairColor = (int)((tmpU16_32 >> 9) & 7);
        var mustacheScale = (int)((tmpU16_32 >> 5) & 0xF);
        var mustacheYposition = (int)(tmpU16_32 & 0x1F);

        studio[0x29] = (byte)mustacheStyle;
        studio[1] = (byte)beardStyle; // Mapped to index 1
        studio[0] = (byte)(facialHairColor == 0 ? 8 : facialHairColor); // Map color 0 to 8, Mapped to index 0
        studio[0x28] = (byte)mustacheScale;
        studio[0x2A] = (byte)mustacheYposition;

        // --- Glasses ---
        var tmpU16_30 = BigEndianBinaryReader.BufferToUint16(buf, 0x30);
        var glassesStyle = (int)(tmpU16_30 >> 12);
        var glassesColor = (int)((tmpU16_30 >> 9) & 7);
        var glassesScale = (int)((tmpU16_30 >> 5) & 7); // Note: JS uses 7 mask
        var glassesYposition = (int)(tmpU16_30 & 0x1F);
        // int unknownGlassesBits = (int)((tmpU16_30 >> 12) & 7); // Middle bits unused

        studio[0x19] = (byte)glassesStyle;
        byte mappedGlassesColor;
        if (glassesColor == 0)
            mappedGlassesColor = 8; // black -> 8
        else if (glassesColor < 6)
            mappedGlassesColor = (byte)(glassesColor + 13); // 1-5 -> 14-18
        else
            mappedGlassesColor = 0; // 6, 7 -> 0 (no mapping?)
        studio[0x17] = mappedGlassesColor;
        studio[0x18] = (byte)glassesScale;
        studio[0x1A] = (byte)glassesYposition;

        // --- Mole ---
        var tmpU16_34 = BigEndianBinaryReader.BufferToUint16(buf, 0x34);
        var enableMole = (int)(tmpU16_34 >> 15);
        var moleScale = (int)((tmpU16_34 >> 11) & 0xF);
        var moleYposition = (int)((tmpU16_34 >> 6) & 0x1F);
        var moleXposition = (int)((tmpU16_34 >> 1) & 0x1F);
        // int unknownMoleBit = (int)(tmpU16_34 & 1); // Lowest bit unused

        studio[0x20] = (byte)enableMole;
        studio[0x1F] = (byte)moleScale;
        studio[0x22] = (byte)moleYposition;
        studio[0x21] = (byte)moleXposition;

        return studio;
    }

    /// <summary>
    /// Encodes the studio data array into the hex string format required by the API.
    /// Based on the encodeStudio function in the provided JS.
    /// </summary>
    private static string EncodeStudioData(byte[] studioData)
    {
        byte n = 0;
        var dest = new StringBuilder("00", (studioData.Length + 1) * 2); // Preallocate buffer ("00" + 2 chars per byte)

        foreach (var b in studioData)
        {
            var eo = (byte)((7 + (b ^ n)) & 0xFF);
            n = eo; // Update n *after* calculating eo, using the new eo
            dest.Append(eo.ToString("x2")); // Append hex representation
        }
        return dest.ToString();
    }

    #endregion

    #region Mii Images Cache

    private const int MaxCachedImages = 126;
    private static readonly Dictionary<string, (Bitmap? image, bool success)> Images = new();
    private static readonly Queue<string> ImageOrder = new();
    public static int ImageCount { get; private set; } = 0;

    public static (Bitmap? image, bool success)? GetCachedMiiImage(MiiImage miiConfig) =>
        Images.TryGetValue(miiConfig.CachingKey, out var image) ? image : null;

    private static void AddMiiImage(MiiImage miiConfig, (Bitmap? image, bool success) imageResult)
    {
        // Don't cache if the image is null and success is true (shouldn't happen, but safety check)
        // Do cache if success is false (means we failed and shouldn't retry immediately)
        if (imageResult.image == null && imageResult.success)
        {
            // Console.WriteLine($"Skipping cache add for successful null image: {miiConfig.CachingKey}"); // Debugging
            return;
        }

        if (!Images.ContainsKey(miiConfig.CachingKey))
        {
            ImageOrder.Enqueue(miiConfig.CachingKey);
        }
        // Overwrite existing entry if present (Dispose old image if it exists)
        if (Images.TryGetValue(miiConfig.CachingKey, out var oldImageResult))
        {
            if (!ReferenceEquals(oldImageResult.image, imageResult.image)) // Only dispose if it's a different bitmap instance
            {
                oldImageResult.image?.Dispose();
                // Console.WriteLine($"Disposed old image for key: {miiConfig.CachingKey}"); // Debugging
            }
        }

        Images[miiConfig.CachingKey] = imageResult;
        // Console.WriteLine($"Added/Updated cache entry for key: {miiConfig.CachingKey}"); // Debugging


        // Enforce cache limit
        while (Images.Count > MaxCachedImages && ImageOrder.Count > 0)
        {
            var oldestKey = ImageOrder.Dequeue();
            if (Images.TryGetValue(oldestKey, out var oldestImageResult))
            {
                // Dispose the old bitmap before removing it from the cache
                oldestImageResult.image?.Dispose();
                Images.Remove(oldestKey);
                // Console.WriteLine($"Removed oldest image from cache: {oldestKey}"); // Debugging
            }
            // If TryGetValue fails, the key might have been removed elsewhere or was already replaced.
        }
        ImageCount = Images.Count;
        // Console.WriteLine($"Cache count: {ImageCount}"); // Debugging
    }

    // Creates a new image of this Mii and adds it to the cache (if it was loaded successfully)
    // Note: Changed return type to Task to allow awaiting the image generation/fetch
    public static async Task ResetMiiImageAsync(MiiImage miiImage)
    {
        if (string.IsNullOrEmpty(miiImage.Data))
        {
            miiImage.SetImage(null, false); // Cannot reset if no data
            return;
        }

        byte[] rawMiiData;
        try
        {
            rawMiiData = Convert.FromBase64String(miiImage.Data);
        }
        catch (FormatException)
        {
            miiImage.SetImage(null, false); // Mark as failed
            return;
        }

        // Always request a fresh image
        var newImageResult = await RequestMiiImageAsync(miiImage.CachingKey, rawMiiData, miiImage.Variant);

        // Add *before* comparing/setting to update cache regardless
        AddMiiImage(miiImage, newImageResult);

        // Only update the MiiImage object if the new image is different or the success status changed
        if (!ReferenceEquals(miiImage.Image, newImageResult.image) || miiImage.LoadedImageSuccessfully != newImageResult.success)
        {
            miiImage.SetImage(newImageResult.image, newImageResult.success);
        }
    }

    // Return the image, and a bool indicating if the image was loaded successfully
    private static async Task<(Bitmap? image, bool success)> RequestMiiImageAsync(
        string cacheKey,
        byte[] rawMiiData,
        MiiImageVariants.Variant variant
    )
    {
        // 1. Generate the encoded studio data string locally
        var encodedStudioData = GenerateEncodedStudioData(rawMiiData);
        if (string.IsNullOrEmpty(encodedStudioData))
        {
            Console.WriteLine($"Failed to generate studio data for cache key: {cacheKey}");
            return (null, false); // Indicate failure due to data generation issue
        }

        // 2. Construct the final image URL query parameters
        var miiImageUrlParams = MiiImageVariants.Get(variant)(encodedStudioData);
        if (string.IsNullOrEmpty(miiImageUrlParams))
        {
            Console.WriteLine($"Failed to get image URL parameters for variant {variant}");
            return (null, false);
        }
        var fullImageUrl = $"{Endpoints.MiiImageUrl}?{miiImageUrlParams}";

        // 3. Fetch the image from the Nintendo server
        try
        {
            // Use MiiImageHttpClient
            using var imageResponse = await MiiImageHttpClient.GetAsync(fullImageUrl);

            if (!imageResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch Mii image from {fullImageUrl}. Status: {imageResponse.StatusCode}");
                return (null, false); // Indicate failure
            }

            // Read the image stream and create the Bitmap
            // Important: Copy stream to memory stream because Bitmap takes ownership and original stream might be disposed by HttpClient
            await using var imageStream = await imageResponse.Content.ReadAsStreamAsync();
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reset stream position for Bitmap constructor

            // Check if stream is empty (sometimes API returns success but empty content)
            if (memoryStream.Length == 0)
            {
                Console.WriteLine($"Received empty image stream from {fullImageUrl}.");
                return (null, false);
            }
            
            var bitmap = new Bitmap(memoryStream); // Bitmap constructor reads from the stream
            return (bitmap, true); // Success
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"HTTP request error fetching Mii image from {fullImageUrl}: {httpEx.Message}");
            return (null, false);
        }
        catch (Exception ex) // Catch other potential errors (network issues, Bitmap creation errors)
        {
            Console.WriteLine($"Error processing Mii image request for {fullImageUrl}: {ex.Message}");
            return (null, false);
        }
    }

    #endregion

    public static void ClearImageCache()
    {
        // Dispose all cached bitmaps before clearing
        foreach (var kvp in Images)
        {
            kvp.Value.image?.Dispose();
        }
        Images.Clear();
        ImageOrder.Clear();
        ImageCount = 0;
    }
}
