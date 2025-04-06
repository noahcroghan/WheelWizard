using WheelWizard.Shared;
using WheelWizard.WiiManagement;
using WheelWizard.WiiManagement.Domain;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Test.Features
{
    public class MiiDbServiceTests
    {
        private OperationResult<FullMii> CreateValidMii(uint id = 1, string name = "TestMii")
        {
            var miiname = MiiName.Create(name);
            var miiId = id;
            var height = MiiScale.Create(60);
            var weight = MiiScale.Create(50);
            var miiFacial = MiiFacialFeatures.Create(MiiFaceShape.Bread, MiiSkinColor.Brown, MiiFacialFeature.Beard, false, false);
            var miiHair = MiiHair.Create(1, HairColor.Black, false);
            var miiEyebrows = MiiEyebrow.Create(1, 1, EyebrowColor.Black, 1, 1, 1);
            var miiEyes = MiiEye.Create(1, 1, 1, EyeColor.Black, 1, 1);
            var miiNose = MiiNose.Create(NoseType.Default, 1, 1);
            var miiLips = MiiLip.Create(1, LipColor.Pink, 1, 1);
            var miiGlasses = MiiGlasses.Create(GlassesType.None, GlassesColor.Blue, 1, 1);
            var miiFacialHair = MiiFacialHair.Create(MustacheType.None, BeardType.None, MustacheColor.Black, 1, 1);
            var miiMole = MiiMole.Create(true, 1, 1, 1);
            var creatorName = MiiName.Create("Creator");
            var miiFavoriteColor = MiiFavoriteColor.Red;
            var EveryResult = new List<OperationResult> { miiname, height, weight, miiFacial, miiHair, miiEyebrows, miiEyes, miiNose, miiLips, miiGlasses, miiFacialHair, miiMole, creatorName };
            foreach (var result in EveryResult)
            {
                if (result.IsFailure)
                    return result.Error;
            }
            return new FullMii
            {
                Name = miiname.Value,
                MiiId = miiId,
                Height = height.Value,
                Weight = weight.Value,
                MiiFacial = miiFacial.Value,
                MiiHair = miiHair.Value,
                MiiEyebrows = miiEyebrows.Value,
                MiiEyes = miiEyes.Value,
                MiiNose = miiNose.Value,
                MiiLips = miiLips.Value,
                MiiGlasses = miiGlasses.Value,
                MiiFacialHair = miiFacialHair.Value,
                MiiMole = miiMole.Value,
                CreatorName = creatorName.Value,
                MiiFavoriteColor = miiFavoriteColor,
            };
        }
        
        private readonly IMiiRepository _repository;
        private readonly MiiDbService _service;

        public MiiDbServiceTests()
        {
            _repository = Substitute.For<IMiiRepository>();
            _service = new MiiDbService(_repository);
        }
        
        [Fact]
        public void CreateValidMii_ShouldSerializeAndDeserializeSuccessfully()
        {
            // Arrange
            var original = CreateValidMii(999, "RoundMii");
            if (original.IsFailure)
                Assert.True(false, "Failed to create valid Mii for serialization test. + " + original.Error.Message);

            // Act
            var serialized = MiiSerializer.Serialize(original.Value);

            // Assert serialization succeeded
            Assert.True(serialized.IsSuccess);

            var deserializedResult = MiiSerializer.Deserialize(serialized.Value);

            // Assert deserialization succeeded
            Assert.True(deserializedResult.IsSuccess);

            var deserialized = deserializedResult.Value;

            // Assert that key properties match
            Assert.Equal(original.Value.MiiId, deserialized.MiiId);
            Assert.Equal(original.Value.Name.ToString(), deserialized.Name.ToString());
            Assert.Equal(original.Value.Height.Value, deserialized.Height.Value);
            Assert.Equal(original.Value.MiiFacial.FaceShape, deserialized.MiiFacial.FaceShape);
            Assert.Equal(original.Value.MiiEyes.Type, deserialized.MiiEyes.Type);
            Assert.Equal(original.Value.MiiGlasses.Type, deserialized.MiiGlasses.Type);
            Assert.Equal(original.Value.MiiFacialHair.MustacheType, deserialized.MiiFacialHair.MustacheType);
            Assert.Equal(original.Value.MiiMole.Exists, deserialized.MiiMole.Exists);
            Assert.Equal(original.Value.CreatorName.ToString(), deserialized.CreatorName.ToString());
            Assert.Equal(original.Value.MiiFavoriteColor, deserialized.MiiFavoriteColor);
            Assert.Equal(original.Value.Weight.Value, deserialized.Weight.Value);
            Assert.Equal(original.Value.MiiHair.HairColor, deserialized.MiiHair.HairColor);
            Assert.Equal(original.Value.MiiEyebrows.Color, deserialized.MiiEyebrows.Color);
            Assert.Equal(original.Value.MiiNose.Type, deserialized.MiiNose.Type);
        }

        [Fact]
        public void GetAllMiis_ReturnsValidMiis_WhenRepositoryReturnsValidBlocks()
        {
            // Arrange: use the helper method to create a fully valid Mii.
            var fullMii = CreateValidMii();
            if (fullMii.IsFailure)
                Assert.True(false, "Failed to create valid Mii for GetAllMiis test.");
            var serialized = MiiSerializer.Serialize(fullMii.Value);
            Assert.True(serialized.IsSuccess, "Serialization failed for valid Mii.");

            _repository.LoadAllBlocks().Returns(new List<byte[]> { serialized.Value });

            // Act
            var result = _service.GetAllMiis();

            // Assert
            Assert.Single(result);
            Assert.Equal("TestMii", result[0].Name.ToString());
        }

        [Fact]
        public void GetByClientId_ReturnsMii_WhenRepositoryReturnsValidBlock()
        {
            // Arrange: create a valid Mii using the helper.
            var fullMii = CreateValidMii(123);
            if (fullMii.IsFailure)
                Assert.True(false, "Failed to create valid Mii for GetByClientId test.");
            var serialized = MiiSerializer.Serialize(fullMii.Value);
            Assert.True(serialized.IsSuccess);

            _repository.GetRawBlockByClientId(123).Returns(serialized.Value);

            // Act
            var result = _service.GetByClientId(123);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("TestMii", result.Value.Name.ToString());
        }

        [Fact]
        public void GetByClientId_ReturnsFailure_WhenRepositoryReturnsNullOrInvalidLength()
        {
            // Arrange: repository returns null.
            _repository.GetRawBlockByClientId(123).Returns((byte[])null);

            // Act
            var resultNull = _service.GetByClientId(123);

            // Assert
            Assert.True(resultNull.IsFailure);
            Assert.Equal("Mii block not found or invalid.", resultNull.Error.Message);

            // Arrange: repository returns an invalid block (wrong length)
            _repository.GetRawBlockByClientId(123).Returns(new byte[10]);

            // Act
            var resultInvalid = _service.GetByClientId(123);

            // Assert
            Assert.True(resultInvalid.IsFailure);
            Assert.Equal("Mii block not found or invalid.", resultInvalid.Error.Message);
        }

        [Fact]
        public void Update_ReturnsFailure_WhenRepositoryUpdateFails()
        {
            // Arrange: create a valid Mii using the helper.
            var fullMii = CreateValidMii(123);
            if (fullMii.IsFailure)
                Assert.True(false, "Failed to create valid Mii for Update test.");
            var serialized = MiiSerializer.Serialize(fullMii.Value);
            Assert.True(serialized.IsSuccess);

            // Simulate repository update failure.
            _repository.UpdateBlockByClientId(123, Arg.Any<byte[]>())
                .Returns(Fail("Update failed"));


            // Act
            var result = _service.Update(fullMii.Value);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Update failed", result.Error.Message);
        }

        [Fact]
        public void Update_ReturnsSuccess_WhenRepositoryUpdateSucceeds()
        {
            // Arrange: create a valid Mii using the helper.
            var fullMii = CreateValidMii(321);
            if (fullMii.IsFailure)
                Assert.True(false, "Failed to create valid Mii for Update test.");
            var serialized = MiiSerializer.Serialize(fullMii.Value);
            Assert.True(serialized.IsSuccess);

            _repository.UpdateBlockByClientId(321, Arg.Any<byte[]>())
                       .Returns(Ok());

            // Act
            var result = _service.Update(fullMii.Value);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void UpdateName_ReturnsFailure_WhenGetByClientIdFails()
        {
            // Arrange: repository returns null for the given clientId.
            _repository.GetRawBlockByClientId(111).Returns((byte[])null);

            // Act
            var result = _service.UpdateName(111, "NewName");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Mii block not found or invalid.", result.Error.Message);
        }

        [Fact]
        public void UpdateName_ReturnsFailure_WhenNewNameIsInvalid()
        {
            // Arrange: valid Mii block exists.
            var fullMii = CreateValidMii(222);
            if (fullMii.IsFailure)
                Assert.True(false, "Failed to create valid Mii for UpdateName test.");
            var serialized = MiiSerializer.Serialize(fullMii.Value);
            Assert.True(serialized.IsSuccess);
            _repository.GetRawBlockByClientId(222).Returns(serialized.Value);

            // Act: attempt to update the name with an invalid value (too long).
            var result = _service.UpdateName(222, "ThisNameIsWayTooLong");

            // Assert: expect failure from MiiName.Create.
            Assert.True(result.IsFailure);
            Assert.Equal("Mii name too long, maximum is 10 characters", result.Error.Message);
        }

        [Fact]
        public void UpdateName_ReturnsSuccess_WhenNameIsUpdated()
        {
            // Arrange: valid Mii block exists.
            var fullMii = CreateValidMii(333, "OldName");
            if (fullMii.IsFailure)
                Assert.True(false, "Failed to create valid Mii for UpdateName test.");
            var serialized = MiiSerializer.Serialize(fullMii.Value);
            Assert.True(serialized.IsSuccess);
            _repository.GetRawBlockByClientId(333).Returns(serialized.Value);

            // Simulate repository update success.
            _repository.UpdateBlockByClientId(333, Arg.Any<byte[]>())
                       .Returns(Ok());

            // Act: update the name.
            var result = _service.UpdateName(333, "NewName");

            // Assert
            Assert.True(result.IsSuccess);
        }
    }
}
