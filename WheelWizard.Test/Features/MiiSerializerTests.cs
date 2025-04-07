using WheelWizard.WiiManagement;

namespace WheelWizard.Test.Serialization;

public class MiiSerializerTests
{
    // List of base64 strings that represent 100% valid Mii data blocks.
    private readonly string[] dataList = {
        "AAAAQgBlAGUAAAAAAAAAAAAAAAAAAEBAgeGIAcKv7BAABEJBMb0oogiMCEgUTbiNAIoAiiUFAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        "wBAASAOzA8EDtQByACADtQB4AAAAAAAAgAAAAAAAAAAgF4+gmVMm1SCSjpgAbWAvAIoAiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        "gBYDngBxAHUAaQAAAAAAAAAAAAAAAH9QgAAAAAAAAAAAFxAAItQQPBiODhgIZVEPcKBhDSUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        "wBbgFwBsAHUAbQBp4BcAAAAAAAAAAF89gAAAAAAAAAAAFTqAmY4IwSCQDngAbWAQZOAAiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        "gAwAUQBGAFMARgBZAFMATQBHAAAAAAAAgAAAAAAAAACgbERAAKQHIEhvCTglXZitAIoAiiUFAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        "gAAAbgBvACAAbgBhAG0AZQAAAAAAAEBAgAAAAuz/gtIEF0JAMZQoogiMCFgUTbiNAIoAiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        "gBAARABhAHgAdABlAHIAAAAAAAAAAG5VgAAAAAAAAAAgF3hAAVQosgiMCFgUTbiNAIoAiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        "gAgAbgBvACAAbgBhAG0AZQAAAAAAAEBAgAAAAOz/gtIQHogAMZcIogiMCFgUTbiNAIoAiiUFAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        "gAoARABlAGUAbgBlAAAAAAAAAAAAAAAmgAAAAAAAAACALE/AuWQoolRRBPgAjUjNJnAAiiUFAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        "gAomBgBNAGEAcgO6JmoAAAAAAAAAAEEmgAAAAAAAAAAAF2ZgMZQokgitCFgUVbJtgIoKiiTMAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        "gAzwYAAAAAAAAAAAAAAAAAAAAAAAAEBAgAAAAAAAAAAEDEIAMYUIogiMCFgTTbhtIGAAiiUFAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        "gAgAbgBvACAAbgBhAG0AZQAAAAAAAEBAgAAAAOz/gtIQF4gAMZQIogiMCFgUTbiNAIoAiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        "wBbgFwBMA7EAbgBjAGUAWCEiAAAAAEBBgAAAAAAAAAAgFzoAuVMIooxQDlgAfZgPZOMAiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        "gBYATQBpAG4AaQBuAGcAAAAAAAAAAEBOgAAAAAAAAAAg10KAuRQoopSMSFiiTZhtIIoAiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        "wBAAZwBhAG4AZwBuAGUAdwB3AHMDyAAAgAAAAAAAAAAEbDZAqaQosmBsCFgUTQCNAAoAgCIFAAAAAAAAAAAAAAAAAAAAAAAAAAA=",
        "wBIATABpAGMAbwByAGkAYwBlAAAAAAosgAAAAAAAAAAgTH5AuUUo8kiRCtgAbUALguAAiiUFAAAAAAAAAAAAAAAAAAAAAAAAAAA="
    };

    // Use MemberData to supply the valid Mii data to our tests.
    public static IEnumerable<object[]> ValidMiiData
    {
        get
        {
            yield return new object[] { "AAAAQgBlAGUAAAAAAAAAAAAAAAAAAEBAgeGIAcKv7BAABEJBMb0oogiMCEgUTbiNAIoAiiUFAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
            yield return new object[] { "wBAASAOzA8EDtQByACADtQB4AAAAAAAAgAAAAAAAAAAgF4+gmVMm1SCSjpgAbWAvAIoAiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
            yield return new object[] { "gBYDngBxAHUAaQAAAAAAAAAAAAAAAH9QgAAAAAAAAAAAFxAAItQQPBiODhgIZVEPcKBhDSUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
            yield return new object[] { "wBbgFwBsAHUAbQBp4BcAAAAAAAAAAF89gAAAAAAAAAAAFTqAmY4IwSCQDngAbWAQZOAAiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
            yield return new object[] { "gAwAUQBGAFMARgBZAFMATQBHAAAAAAAAgAAAAAAAAACgbERAAKQHIEhvCTglXZitAIoAiiUFAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
            yield return new object[] { "gAAAbgBvACAAbgBhAG0AZQAAAAAAAEBAgAAAAuz/gtIEF0JAMZQoogiMCFgUTbiNAIoAiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
            yield return new object[] { "gBAARABhAHgAdABlAHIAAAAAAAAAAG5VgAAAAAAAAAAgF3hAAVQosgiMCFgUTbiNAIoAiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
            yield return new object[] { "gAgAbgBvACAAbgBhAG0AZQAAAAAAAEBAgAAAAOz/gtIQHogAMZcIogiMCFgUTbiNAIoAiiUFAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
            yield return new object[] { "gAoARABlAGUAbgBlAAAAAAAAAAAAAAAmgAAAAAAAAACALE/AuWQoolRRBPgAjUjNJnAAiiUFAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
            yield return new object[] { "gAomBgBNAGEAcgO6JmoAAAAAAAAAAEEmgAAAAAAAAAAAF2ZgMZQokgitCFgUVbJtgIoKiiTMAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
            yield return new object[] { "gAzwYAAAAAAAAAAAAAAAAAAAAAAAAEBAgAAAAAAAAAAEDEIAMYUIogiMCFgTTbhtIGAAiiUFAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
            yield return new object[] { "gAgAbgBvACAAbgBhAG0AZQAAAAAAAEBAgAAAAOz/gtIQF4gAMZQIogiMCFgUTbiNAIoAiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
            yield return new object[] { "wBbgFwBMA7EAbgBjAGUAWCEiAAAAAEBBgAAAAAAAAAAgFzoAuVMIooxQDlgAfZgPZOMAiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
            yield return new object[] { "gBYATQBpAG4AaQBuAGcAAAAAAAAAAEBOgAAAAAAAAAAg10KAuRQoopSMSFiiTZhtIIoAiiUEAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
            yield return new object[] { "wBAAZwBhAG4AZwBuAGUAdwB3AHMDyAAAgAAAAAAAAAAEbDZAqaQosmBsCFgUTQCNAAoAgCIFAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
            yield return new object[] { "wBIATABpAGMAbwByAGkAYwBlAAAAAAosgAAAAAAAAAAgTH5AuUUo8kiRCtgAbUALguAAiiUFAAAAAAAAAAAAAAAAAAAAAAAAAAA=" };
        }
    }

    [Theory]
    [MemberData(nameof(ValidMiiData))]
    public void Deserialize_ValidMiiData_ShouldSucceed(string base64Data)
    {
        // Arrange: convert the base64 string into a byte array.
        var data = Convert.FromBase64String(base64Data);
        Assert.Equal(MiiSerializer.MiiBlockSize, data.Length);

        // Act: deserialize the byte array.
        var result = MiiSerializer.Deserialize(data);

        // Assert: deserialization should succeed and key properties should be set.
        Assert.True(result.IsSuccess, $"Deserialization failed for data: {base64Data}");
        var mii = result.Value;
        Assert.NotNull(mii);
        Assert.NotEqual(0u, mii.MiiId);
        Assert.NotNull(mii.Name);
    }

    [Theory]
    [MemberData(nameof(ValidMiiData))]
    public void RoundTrip_Serialization_ShouldBeConsistent(string base64Data)
    {
        // Arrange: decode and deserialize the original base64 data.
        var originalBytes = Convert.FromBase64String(base64Data);
        var deserializationResult = MiiSerializer.Deserialize(originalBytes);
        Assert.True(deserializationResult.IsSuccess, "Deserialization of original data failed.");
        var mii = deserializationResult.Value;

        // Act: serialize the Mii back into a byte array.
        var serializationResult = MiiSerializer.Serialize(mii);
        Assert.True(serializationResult.IsSuccess, "Serialization failed for the deserialized Mii.");
        var roundTripBytes = serializationResult.Value;
        Assert.Equal(MiiSerializer.MiiBlockSize, roundTripBytes.Length);

        // Re-deserialize the round-trip bytes.
        var roundTripDeserialization = MiiSerializer.Deserialize(roundTripBytes);
        Assert.True(roundTripDeserialization.IsSuccess, "Deserialization of round-trip data failed.");
        var miiRoundTrip = roundTripDeserialization.Value;

        // Assert: key properties should be equal between the original and the round-trip Mii.
        Assert.Equal(mii.MiiId, miiRoundTrip.MiiId);
        Assert.Equal(mii.Name.ToString(), miiRoundTrip.Name.ToString());
        Assert.Equal(mii.Height.Value, miiRoundTrip.Height.Value);
        Assert.Equal(mii.Weight.Value, miiRoundTrip.Weight.Value);
        Assert.Equal(mii.MiiFacial.FaceShape, miiRoundTrip.MiiFacial.FaceShape);
        Assert.Equal(mii.MiiEyes.Type, miiRoundTrip.MiiEyes.Type);
        Assert.Equal(mii.MiiGlasses.Type, miiRoundTrip.MiiGlasses.Type);
        Assert.Equal(mii.CreatorName.ToString(), miiRoundTrip.CreatorName.ToString());
    }

    [Fact]
    public void Serialize_NullMii_ShouldFail()
    {
        // Act: attempt to serialize a null FullMii.
        var result = MiiSerializer.Serialize(null);

        // Assert: the operation should fail with the proper error message.
        Assert.True(result.IsFailure);
        Assert.Equal("Mii cannot be null.", result.Error.Message);
    }

    [Fact]
    public void Deserialize_InvalidLengthData_ShouldFail()
    {
        // Arrange: create a byte array with an invalid length.
        var invalidData = new byte[10];

        // Act: attempt to deserialize the invalid data.
        var result = MiiSerializer.Deserialize(invalidData);

        // Assert: the operation should fail because the length is not 74 bytes.
        Assert.True(result.IsFailure);
        Assert.Equal("Invalid Mii data length.", result.Error.Message);
    }
}
