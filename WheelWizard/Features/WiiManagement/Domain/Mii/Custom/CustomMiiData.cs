using System.Reflection;
using System.Runtime.CompilerServices;

namespace WheelWizard.WiiManagement.Domain.Mii.Custom;

/// <summary>
/// This custom attribute is used to mark properties within the CustomMiiData class.
/// It specifies how many bits that property occupies within the packed 28-bit custom data payload.
/// </summary>
[AttributeUsage(AttributeTargets.Property)] // Specifies that this attribute can only be applied to properties.
internal sealed class BitFieldAttribute : Attribute
{
    /// <summary>
    /// Gets the number of bits allocated to the property decorated with this attribute.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BitFieldAttribute"/> class.
    /// </summary>
    /// <param name="width">The number of bits the associated property will occupy.</param>
    public BitFieldAttribute(int width) => Width = width;
}

/// <summary>
/// Provides a structured way to access and modify the 28 "unknown" or unused bits
/// found within the standard 74-byte Mii data format. This allows storing custom data
/// without breaking compatibility with standard Mii readers.
///
/// The 28 bits are laid out as follows by this class:
/// • Bits 0–2  : Schema version. This helps manage different layouts of the custom data over time.
/// • Bits 3–27 : Available for custom fields defined as properties below. These are automatically
///               packed based on their declaration order and specified [BitField] width.
/// </summary>
public sealed class CustomMiiData
{
    // Defines the current version of the custom data layout.
    // Increment this (1-7, then loop) if the layout of properties below changes,
    // allowing older versions to recognize or ignore newer data formats.
    private const int SchemaVersion = 1;

    // The total number of bits available for custom data within the Mii format's unused sections.
    // This is a hard limit, Never change this!
    private const int TotalBits = 28;

    // Stores the packed custom data as a 32-bit unsigned integer.
    // Only the lower 28 bits are used. Bit 0 is the least significant bit (LSB).
    private uint _payload;

    #region Static reflection map – offset/width for every [BitField] property

    // A helper record to store metadata about each property marked with [BitField].
    // Prop: The reflection PropertyInfo object itself.
    // Offset: The starting bit position of this field within the 28-bit payload (0-27).
    // Width: The number of bits allocated to this field.
    // Mask: A pre-calculated bitmask to easily isolate/find or clear this field's bits within the payload.
    private sealed record FieldMeta(PropertyInfo Prop, int Offset, int Width, uint Mask);

    // A dictionary mapping property names (e.g., "Version", "IsCopyable") to their calculated metadata.
    // This is built once using reflection in the static constructor.
    private static readonly IReadOnlyDictionary<string, FieldMeta> _meta;

    // Static constructor: This runs once when the CustomMiiData class is first used.
    // Its purpose is to automatically determine the layout of the custom bit fields.
    static CustomMiiData()
    {
        // Get all public and non-public instance properties of this class.
        var properties = typeof(CustomMiiData).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Filter to find only properties that have the [BitField] attribute.
        var bitFieldProperties = properties.Where(p => p.GetCustomAttribute<BitFieldAttribute>() is not null);

        // Order these properties based on their declaration order in the source code.
        // MetadataToken provides a stable ordering. This ensures bits are packed consistently.
        var orderedProperties = bitFieldProperties.OrderBy(p => p.MetadataToken).ToArray();

        // Prepare a list to hold the metadata for each field.
        var fieldMetadataList = new List<FieldMeta>();
        // Initialize a cursor to keep track of the next available bit position.
        var currentBitOffset = 0;

        // Iterate through the ordered properties to calculate their position and mask.
        foreach (var prop in orderedProperties)
        {
            // Get the width specified in the [BitField] attribute for this property.
            var width = prop.GetCustomAttribute<BitFieldAttribute>()!.Width;

            // Validate the specified width.
            if (width is <= 0 or > TotalBits)
                throw new InvalidOperationException($"Bit width for '{prop.Name}' must be between 1 and {TotalBits}.");

            // Check if adding this property exceeds the total available bits.
            if (currentBitOffset + width > TotalBits)
                throw new InvalidOperationException(
                    $"Layout overflow – Adding '{prop.Name}' ({width} bits) exceeds the {TotalBits}-bit budget. Current offset: {currentBitOffset}."
                );

            // Calculate the bitmask for this field.
            // 1. Create a value with 'width' number of 1s (e.g., width 3 -> 0b111).
            //    This is done by shifting 1 left by 'width' (1 << width gives 0b1000 for width 3)
            //    and subtracting 1 ((1u << width) - 1u gives 0b0111).
            // 2. Shift this mask left by 'currentBitOffset' to position it correctly within the payload.
            //    e.g., if offset is 5 and width is 3, mask is 0b0111 << 5 = 0b11100000.
            var mask = ((1u << width) - 1u) << currentBitOffset;

            // Create the metadata record for this property and add it to the list.
            fieldMetadataList.Add(new FieldMeta(prop, currentBitOffset, width, mask));

            // Advance the cursor by the width of the current field.
            currentBitOffset += width;
        }

        // if not all 28 bits were used by the defined properties.
        if (currentBitOffset < TotalBits)
        {
            //for now do nothing but we could add stuff here
        }

        // Store the generated metadata list as a dictionary, keyed by property name for easy lookup.
        _meta = fieldMetadataList.ToDictionary(m => m.Prop.Name, m => m);
    }
    #endregion

    // ──────────────────────────────────────────  PUBLIC API PROPERTIES ──────────────────────────────────────────
    // These properties provide access to the custom data fields.
    // They use the [BitField] attribute and rely on GetField/SetField helpers.

    /// <summary>
    /// Gets or sets the 3-bit schema version (bits 0-2).
    /// This should always be the first field defined.
    /// The setter is private to ensure it's only set internally (e.g., in CreateEmpty).
    /// </summary>
    [BitField(3)]
    public byte Version
    {
        get => (byte)GetField();
        private set => SetField(value);
    }

    /// <summary>
    /// Gets or sets whether the Mii is allowed to be copied from other consoles (1 bit).
    /// Uses a boolean for easy access, mapping to 1 (true) or 0 (false).
    /// </summary>
    [BitField(1)]
    public bool IsCopyable
    {
        get => GetField() != 0; // Read the 1-bit value; non-zero means true.
        set => SetField(value ? 1u : 0u); // Write 1 if true, 0 if false.
    }

    /// <summary>
    /// Gets or sets a sample 4-bit colour value (0-15).
    /// The width could be changed if needed.
    /// </summary>
    [BitField(4)]
    public byte AccentColor
    {
        get => (byte)GetField();
        set => SetField(value);
    }

    /// <summary>
    /// Gets or sets eight individual feature flags packed into 3 bits
    /// </summary>
    [BitField(3)]
    public byte FacialExpression
    {
        get => (byte)GetField();
        set => SetField(value);
    }

    /// <summary>
    /// </summary>
    [BitField(17)]
    public ushort Spare
    {
        get => (ushort)GetField();
        set => SetField(value);
    }

    // Add new properties here.
    // Simply declare them with a [BitField(width)] attribute.
    // They will be automatically allocated space in the payload after the 'Spare' field,
    // provided the total width does not exceed 28 bits. The static constructor handles layout.

    // ──────────────────────────────────────────  CONSTRUCTION METHODS ──────────────────────────────────────────

    /// <summary>
    /// Private constructor used internally to create an instance with a given payload.
    /// </summary>
    /// <param name="payload">The packed 28-bit data.</param>
    private CustomMiiData(uint payload) => _payload = payload;

    /// <summary>
    /// Creates a <see cref="CustomMiiData"/> instance by extracting the 28 custom bits
    /// from a raw 74-byte Mii data block.
    /// </summary>
    public static CustomMiiData FromBytes(byte[] rawMiiBytes)
    {
        var result = CustomBitsCodec.Extract(rawMiiBytes);
        return new(result);
    } 

    /// <summary>
    /// Creates a <see cref="CustomMiiData"/> instance by extracting the 28 custom bits
    /// from a <see cref="Mii"/> object.
    /// This involves serializing the Mii object to bytes first.
    /// </summary>
    public static CustomMiiData FromMii(Mii mii)
    {
        var serializeResult = MiiSerializer.Serialize(mii);
        if (!serializeResult.IsSuccess)
            throw new InvalidOperationException("Failed to serialize Mii object to extract custom data.");
        return new(CustomBitsCodec.Extract(serializeResult.Value));
    }

    /// <summary>
    /// Creates a new <see cref="CustomMiiData"/> instance with all custom bits initially set to zero,
    /// but with the <see cref="Version"/> field automatically set to the current <see cref="SchemaVersion"/>.
    /// </summary>
    /// <returns>A new, default-initialized <see cref="CustomMiiData"/> instance.</returns>
    public static CustomMiiData CreateEmpty() => new(0) { Version = SchemaVersion };
    

    // ──────────────────────────────────────────  APPLY / SERIALIZE METHODS ──────────────────────────────────────────

    /// <summary>
    /// Applies the current custom data payload (_payload) to a given <see cref="Mii"/> object.
    /// This method returns a *new* Mii object instance with the changes applied, leaving the original untouched.
    /// </summary>
    /// <param name="mii">The original Mii object.</param>
    public Mii ApplyTo(Mii mii)
    {
        // Serialize the input Mii to get its byte representation.
        // The serializer should handle cloning or creating a safe copy.
        var serializeResult = MiiSerializer.Serialize(mii);
        if (!serializeResult.IsSuccess)
            throw new InvalidOperationException("Failed to serialize Mii object to apply custom data."); // Or handle error
        var bytes = serializeResult.Value;
        CustomBitsCodec.Inject(bytes, _payload);
        var deserializeResult = MiiSerializer.Deserialize(bytes);
        if (!deserializeResult.IsSuccess || deserializeResult.Value is null)
            throw new InvalidOperationException("Failed to deserialize Mii data after injecting custom payload."); // Or handle error

        return deserializeResult.Value;
    }

    /// <summary>
    /// Injects the current custom data payload (_payload) directly into a raw 74-byte Mii data block.
    /// This method modifies the provided byte array in place.
    /// </summary>
    public void ApplyTo(byte[] rawMiiBlock) => CustomBitsCodec.Inject(rawMiiBlock, _payload);

    // ──────────────────────────────────────────  PRIVATE HELPER METHODS ──────────────────────────────────────────

    /// <summary>
    /// Gets the value of the property that called this method.
    /// It uses reflection metadata (_meta) to find the correct bits in the payload.
    /// </summary>
    /// <param name="propName">The name of the calling property, automatically supplied by the compiler.</param>
    /// <returns>The value of the requested field, extracted from the _payload.</returns>
    private uint GetField([CallerMemberName] string? propName = null)
    {
        // Look up the metadata (offset, width, mask) for the property.
        var meta = _meta[propName!]; // Assumes propName is always valid due to CallerMemberName.

        // Apply the mask to the payload to isolate the bits for this field.
        // (e.g., payload & 0b11100000 isolates bits 5-7 if that's the mask).
        var isolatedBits = _payload & meta.Mask;

        // Right-shift the isolated bits by the field's offset to align them to the LSB.
        // (e.g., if isolatedBits is 0b10100000 and offset is 5, result is 0b101).
        return isolatedBits >> meta.Offset;
    }

    /// <summary>
    /// Sets the value of the property that called this method.
    /// It uses reflection metadata (_meta) to place the value into the correct bits in the payload.
    /// </summary>
    /// <param name="value">The value to set for the field.</param>
    /// <param name="propName">The name of the calling property, automatically supplied by the compiler.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is too large for the field's bit width.</exception>
    private void SetField(uint value, [CallerMemberName] string? propName = null)
    {
        // Look up the metadata for the property.
        var meta = _meta[propName!];

        // Validate that the provided value fits within the allocated number of bits.
        // Calculate the maximum possible value for the given width (2^width - 1).
        var maxValue = (1u << meta.Width) - 1;
        if (value > maxValue)
            throw new ArgumentOutOfRangeException(propName, $"Value {value} exceeds the {meta.Width}-bit limit (max {maxValue}).");

        // Update the payload:
        // 1. Clear the bits for this field in the current payload using the inverted mask.
        //    (e.g., _payload & ~0b11100000 clears bits 5-7).
        var clearedPayload = _payload & ~meta.Mask;

        // 2. Left-shift the new value by the field's offset to position it correctly.
        //    (e.g., if value is 0b101 and offset is 5, shifted value is 0b10100000).
        var shiftedValue = value << meta.Offset;

        // 3. Combine the cleared payload with the shifted new value using bitwise OR.
        //    (e.g., clearedPayload | 0b10100000 inserts the new value).
        _payload = clearedPayload | shiftedValue;
    }
}

// ──────────────────────────────────────────  LOW-LEVEL BIT MANIPULATION CODEC ──────────────────────────────────────────
// This static class is responsible for the direct manipulation of bytes in the Mii data array
// to extract or inject the 28 custom bits from/into their specific "unknown" locations.

/// <summary>
/// Handles the low-level reading and writing of the 28 custom bits spread across
/// different locations within the 74-byte raw Mii data format.
/// It ensures that only the designated "unknown" bits are touched, preserving
/// the official Mii data structure.
///
/// Extraction (`Extract`): Reads the relevant sections of the Mii data, isolates the specific
///                         unused bits, and packs them together into a single 28-bit `uint`.
/// Injection (`Inject`): Takes a 28-bit `uint` payload, unpacks it into fragments, reads the
///                       relevant Mii data sections, clears only the unused bits, and writes
///                       the fragments back into their original locations.
/// </summary>
internal static class CustomBitsCodec
{
    // Define constants for the byte offsets within the 74-byte Mii array where
    // the known unused bits reside. Refer to Mii.cs comments for the structure.

    // Location: Face data (0x20-0x21, 2 bytes)
    // Contains: unknown_0 (3 bits at original bits 10-12) & unknown_1 (1 bit at original bit 14)
    // Note: Bit numbering in Mii spec is often from MSB (bit 15) to LSB (bit 0). Here, we map
    // to standard 0-indexed LSB representation for masks.
    // E.g., Mii spec bits 10-12 correspond to bits 3-5 in a standard ushort (0-15).
    // E.g., Mii spec bit 14 corresponds to bit 1 in a standard ushort.
    private const int OFS_FACE = 0x20;

    // Mask for unknown_0 (bits 3, 4, 5 of the ushort at OFS_FACE): 0b0000 0000 0011 1000 = 0x0038
    // Mask for unknown_1 (bit 1 of the ushort at OFS_FACE):       0b0000 0000 0000 0010 = 0x0002


    // I wont explain this for EVERY field but this is the general idea:
    // so we have the address 0x20 and in the documentation you can see that 16 bits are used

    // u16 faceShape:3; // 0 - 7
    // u16 skinColor:3; // 0 - 5
    // u16 facialFeature:4; // 0 - 11
    // u16 unknown:3; // Mii appears unaffected by changes to this data
    // u16 mingleOff:1; // 0 = Mingle, 1 = Don't Mingle
    // u16 unknown:1; // Mii appears unaffected by changes to this data
    // u16 downloaded:1; // If the Mii has been downloaded from the Check Mii Out Channel

    // If we map this out we have these 16 bits 0000 0000 0000 0000
    // Those numbers are basically the position of the bits in that 16 bit number
    // so the Last bit is the "downloaded" bit, 0000 0000 0000 0001 (This is downloaded set to 1)
    // the bitmask is basically a mask saying, "hey, the bits set to 1 is the bit that is the relevent bit for this information"
    // so the bitmask for the "Unknown:1" is 0000 0000 0000 0010 because it is the second last bit


    // Location: Hair data (0x22-0x23, 2 bytes)
    // Contains: unknown_2 (5 bits at original bits 11-15)
    // Mapped to bits 0-4 of the ushort at OFS_HAIR.
    private const int OFS_HAIR = 0x22;

    // Mask for unknown_2 (bits 0, 1, 2, 3, 4): 0b0000 0000 0001 1111 = 0x001F
    private const ushort MASK_U2 = 0x001F; // Direct mask for bits 0-4

    // Location: Eyebrow data (0x24-0x27, 4 bytes)
    // Contains: unknown_3 (1 bit at original bit 5) & unknown_4 (6 bits at original bits 10-15)
    // Mapped to bit 26 (unknown_3) and bits 16-21 (unknown_4) of the uint at OFS_BROW.
    private const int OFS_BROW = 0x24;

    // Mask for unknown_3 (bit 26): 1 shifted left 26 times.
    private const uint MASK_U3 = 1u << 26; // 0b0000 0100 0000 0000 0000 0000 0000 0000 = 0x04000000

    // Mask for unknown_4 (bits 16-21): 6 ones shifted left 16 times. (0b111111 << 16)
    private const uint MASK_U4 = 0x003F0000; // 0b0000 0000 0011 1111 0000 0000 0000 0000

    // Location: Eye data (0x28-0x2B, 4 bytes)
    // Contains: unknown_5 (2 bits at original bits 6-7), unknown_6 (1 bit at original bit 19), unknown_7 (5 bits at original bits 27-31)
    // Mapped to bits 24-25 (unknown_5), bit 12 (unknown_6), and bits 0-4 (unknown_7) of the uint at OFS_EYE.
    private const int OFS_EYE = 0x28;

    // Mask for unknown_5 (bits 24-25): 0b11 shifted left 24 times.
    private const uint MASK_U5 = 0x03000000; // 0b0000 0011 0000 0000 0000 0000 0000 0000

    // Mask for unknown_6 (bit 12): 1 shifted left 12 times.
    // too lazy to explain but since its big endian we have to shift it 12 times
    private const uint MASK_U6 = 1u << 12; // 0b0000 0000 0000 0001 0000 0000 0000 0000 = 0x00001000

    // Mask for unknown_7 (bits 0-4): 5 ones.
    private const uint MASK_U7 = 0x0000001F; // 0b0000 0000 0000 0000 0000 0000 0001 1111

    // Location: Nose data (0x2C-0x2D, 2 bytes)
    // Contains: unknown_8 (3 bits at original bits 13-15)
    // Mapped to bits 0-2 of the ushort at OFS_NOSE.
    private const int OFS_NOSE = 0x2C;

    // Mask for unknown_8 (bits 0-2): 3 ones.
    private const ushort MASK_U8 = 0x0007; // 0b0000 0000 0000 0111

    // Location: Mole data (0x34-0x35, 2 bytes)
    // Contains: unknown_10 (1 bit at original bit 15)
    // Mapped to bit 0 of the ushort at OFS_MOLE.
    private const int OFS_MOLE = 0x34;

    // Mask for unknown_10 (bit 0): 1.
    private const ushort MASK_U10 = 0x0001; // 0b0000 0000 0000 0001

    // Defines the width (in bits) of each unknown fragment, in the order they are packed.
    // This MUST match the order of operations in Extract() and Inject().
    // Total width must sum to TOTAL_BITS (28).
    // Order: U0, U1, U2, U3, U4, U5, U6, U7, U8, U10
    private static readonly int[] _widths = { 3, 1, 5, 1, 6, 2, 1, 5, 3, 1 }; // Sum = 28

    /// <summary>
    /// Extracts the 28 designated unused bits from the raw Mii data and packs them
    /// into a single right-aligned 28-bit unsigned integer.
    /// </summary>
    /// <param name="rawMiiBytes">The 74-byte array containing Mii data.</param>
    internal static uint Extract(byte[] rawMiiBytes)
    {
        // Read the relevant multibyte chunks from the raw data array.
        var faceData = ReadU16(rawMiiBytes, OFS_FACE); // Read bytes 0x20, 0x21
        var hairData = ReadU16(rawMiiBytes, OFS_HAIR); // Read bytes 0x22, 0x23
        var browData = ReadU32(rawMiiBytes, OFS_BROW); // Read bytes 0x24-0x27
        var eyeData = ReadU32(rawMiiBytes, OFS_EYE); // Read bytes 0x28-0x2B
        var noseData = ReadU16(rawMiiBytes, OFS_NOSE); // Read bytes 0x2C, 0x2D
        var moleData = ReadU16(rawMiiBytes, OFS_MOLE); // Read bytes 0x34, 0x35

        // Initialize the payload to store the extracted bits and a cursor for packing.
        uint payload = 0;
        var cursor = 0; // Tracks the next bit position to write into the payload (starts at LSB).

        // Extract and pack each fragment in the predefined order (_widths array).
        // Fragment 1: unknown_0 (3 bits from faceData, bits 3-5)
        var frag_u0 = (uint)((faceData & 0x0038) >> 3); // Isolate bits 3-5 (0x38 mask) and shift right by 3.
        WriteFragment(ref payload, ref cursor, frag_u0, 3); // Write 3 bits to payload.

        // Fragment 2: unknown_1 (1 bit from faceData, bit 1)
        var frag_u1 = (uint)((faceData & 0x0002) >> 1); // Isolate bit 1 (0x02 mask) and shift right by 1.
        WriteFragment(ref payload, ref cursor, frag_u1, 1); // Write 1 bit.

        // Fragment 3: unknown_2 (5 bits from hairData, bits 0-4)
        var frag_u2 = (uint)(hairData & MASK_U2); // Isolate bits 0-4 (MASK_U2). No shift needed.
        WriteFragment(ref payload, ref cursor, frag_u2, 5); // Write 5 bits.

        // Fragment 4: unknown_3 (1 bit from browData, bit 26)
        var frag_u3 = (browData & MASK_U3) >> 26; // Isolate bit 26 (MASK_U3) and shift right by 26.
        WriteFragment(ref payload, ref cursor, frag_u3, 1); // Write 1 bit.

        // Fragment 5: unknown_4 (6 bits from browData, bits 16-21)
        var frag_u4 = (browData & MASK_U4) >> 16; // Isolate bits 16-21 (MASK_U4) and shift right by 16.
        WriteFragment(ref payload, ref cursor, frag_u4, 6); // Write 6 bits.

        // Fragment 6: unknown_5 (2 bits from eyeData, bits 24-25)
        var frag_u5 = (eyeData & MASK_U5) >> 24; // Isolate bits 24-25 (MASK_U5) and shift right by 24.
        WriteFragment(ref payload, ref cursor, frag_u5, 2); // Write 2 bits.

        // Fragment 7: unknown_6 (1 bit from eyeData, bit 12)
        var frag_u6 = (eyeData & MASK_U6) >> 12; // Isolate bit 12 (MASK_U6) and shift right by 12.
        WriteFragment(ref payload, ref cursor, frag_u6, 1); // Write 1 bit.

        // Fragment 8: unknown_7 (5 bits from eyeData, bits 0-4)
        var frag_u7 = eyeData & MASK_U7; // Isolate bits 0-4 (MASK_U7). No shift needed.
        WriteFragment(ref payload, ref cursor, frag_u7, 5); // Write 5 bits.

        // Fragment 9: unknown_8 (3 bits from noseData, bits 0-2)
        var frag_u8 = (uint)(noseData & MASK_U8); // Isolate bits 0-2 (MASK_U8). No shift needed.
        WriteFragment(ref payload, ref cursor, frag_u8, 3); // Write 3 bits.

        // Fragment 10: unknown_10 (1 bit from moleData, bit 0)
        var frag_u10 = (uint)(moleData & MASK_U10); // Isolate bit 0 (MASK_U10). No shift needed.
        WriteFragment(ref payload, ref cursor, frag_u10, 1); // Write 1 bit.

        // The payload now contains all 28 custom bits packed sequentially.
        return payload;
    }

    /// <summary>
    /// Injects a 28-bit custom data payload back into the designated unused bit locations
    /// within a raw 74-byte Mii data array. Modifies the array in place.
    /// </summary>
    /// <param name="raw">The 74-byte array representing Mii data. This array will be modified.</param>
    /// <param name="payload">The 28-bit custom data payload (stored in the lower bits of the uint).</param>
    /// <summary>Injects <paramref name="payload"/> back into <paramref name="raw"/>, preserving all official bits.</summary>
    internal static void Inject(byte[] raw, uint payload)
    {
        // Decompose the payload in the same order we packed it
        var fragments = Decompose(payload).ToArray();
        var i = 0;

        var face = ReadU16(raw, OFS_FACE);
        face = (ushort)(
            (uint)(face & ~0x003A) // wipe U0+U1 (0x38 | 0x02)
            | (fragments[i++] << 3) // U0 (3b) → bits 3‑5
            | (fragments[i++] << 1)
        ); // U1 (1b) → bit 1
        WriteU16(raw, OFS_FACE, face);

        var hair = ReadU16(raw, OFS_HAIR);
        hair = (ushort)((hair & ~MASK_U2) | fragments[i++]); // U2 (5b)
        WriteU16(raw, OFS_HAIR, hair);

        var brow = ReadU32(raw, OFS_BROW);
        brow =
            (brow & ~(MASK_U3 | MASK_U4))
            | (fragments[i++] << 26) // U3 (1b)
            | (fragments[i++] << 16); // U4 (6b)
        WriteU32(raw, OFS_BROW, brow);

        var eye = ReadU32(raw, OFS_EYE);
        eye =
            (eye & ~(MASK_U5 | MASK_U6 | MASK_U7))
            | (fragments[i++] << 24) // U5 (2b)
            | (fragments[i++] << 12) // U6 (1b)
            | fragments[i++]; // U7 (5b)
        WriteU32(raw, OFS_EYE, eye);

        var nose = ReadU16(raw, OFS_NOSE);
        nose = (ushort)((nose & ~MASK_U8) | fragments[i++]); // U8 (3b)
        WriteU16(raw, OFS_NOSE, nose);

        var mole = ReadU16(raw, OFS_MOLE);
        mole = (ushort)((mole & ~MASK_U10) | fragments[i]); // U10 (1b)
        WriteU16(raw, OFS_MOLE, mole);
    }

    // ───────────────────────────────  Private Helper Methods  ───────────────────────────────

    /// <summary>
    /// Reads a 16-bit unsigned integer (ushort) from a byte array at a specific offset,
    /// assuming big-endian byte order (most significant byte first).
    /// </summary>
    /// <param name="buffer">The byte array to read from.</param>
    /// <param name="offset">The starting index in the buffer.</param>
    /// <returns>The ushort value.</returns>
    private static ushort ReadU16(byte[] buffer, int offset) => (ushort)((buffer[offset] << 8) | buffer[offset + 1]);

    /// <summary>
    /// Writes a 16-bit unsigned integer (ushort) to a byte array at a specific offset,
    /// using big-endian byte order (most significant byte first).
    /// </summary>
    /// <param name="buffer">The byte array to write to.</param>
    /// <param name="offset">The starting index in the buffer.</param>
    /// <param name="value">The ushort value to write.</param>
    private static void WriteU16(byte[] buffer, int offset, ushort value)
    {
        buffer[offset] = (byte)(value >> 8); // Most significant byte
        buffer[offset + 1] = (byte)value; // Least significant byte
    }

    /// <summary>
    /// Reads a 32-bit unsigned integer (uint) from a byte array at a specific offset,
    /// assuming big-endian byte order.
    /// </summary>
    /// <param name="buffer">The byte array to read from.</param>
    /// <param name="offset">The starting index in the buffer.</param>
    /// <returns>The uint value.</returns>
    private static uint ReadU32(byte[] buffer, int offset) =>
        (uint)(buffer[offset] << 24) | (uint)(buffer[offset + 1] << 16) | (uint)(buffer[offset + 2] << 8) | buffer[offset + 3];

    /// <summary>
    /// Writes a 32-bit unsigned integer (uint) to a byte array at a specific offset,
    /// using big-endian byte order.
    /// </summary>
    /// <param name="buffer">The byte array to write to.</param>
    /// <param name="offset">The starting index in the buffer.</param>
    /// <param name="value">The uint value to write.</param>
    private static void WriteU32(byte[] buffer, int offset, uint value)
    {
        buffer[offset] = (byte)(value >> 24); // MSB
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value; // LSB
    }

    /// <summary>
    /// Writes a fragment of data (value) of a specific width into a destination uint (dst)
    /// at the current bit position (cursor). Used during the packing process in Extract().
    /// </summary>
    /// <param name="dst">The destination uint payload (modified by reference).</param>
    /// <param name="cursor">The current bit position to start writing at (modified by reference).</param>
    /// <param name="value">The data fragment to write (should fit within 'width' bits).</param>
    /// <param name="width">The number of bits this fragment occupies.</param>
    private static void WriteFragment(ref uint dst, ref int cursor, uint value, int width)
    {
        // Shift the value left by the cursor amount to position it correctly.
        var shiftedValue = value << cursor;
        // Use bitwise OR to merge the shifted value into the destination payload.
        dst |= shiftedValue;
        // Advance the cursor by the width of the fragment just written.
        cursor += width;
    }

    /// <summary>
    /// Decomposes a packed 28-bit payload (src) into its constituent fragments
    /// based on the predefined widths in the `_widths` array. Used during the injection process.
    /// </summary>
    /// <param name="src">The packed 28-bit payload.</param>
    /// <returns>An enumerable sequence of uint fragments in the order they were packed.</returns>
    private static IEnumerable<uint> Decompose(uint src)
    {
        var cursor = 0; // Tracks the current bit position being read from the source payload.
        // Iterate through the known widths of the fragments in their packing order.
        foreach (var width in _widths)
        {
            // Create a mask for the current fragment width (e.g., width 3 -> mask 0b111).
            var mask = (1u << width) - 1u;
            // Extract the fragment:
            // 1. Right-shift the source payload by the cursor position to bring the fragment to the LSB.
            // 2. Apply the mask to isolate only the bits of the current fragment.
            yield return (src >> cursor) & mask;
            // Advance the cursor for the next fragment.
            cursor += width;
        }
    }
}
