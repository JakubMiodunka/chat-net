using CommonUtilities.Protocols;


namespace CommonUtilitiesTests.Protocols;

[TestOf(typeof(SimpleSessionLayerProtocol))]
[Category("Unit Test")]
[Author("Jakub Miodunka")]
public sealed class SimpleSessionLayerProtocolTests
{
    #region Default values
    private const int DefaultHeaderLength = 4;  // Value by default used by tested application.
    #endregion

    #region Test parameters
    // Values chosen using 3-value boundary analysis.
    private static readonly int[] s_validPayloadLength = [0, 1, 1019, 1020];
    private static readonly int[] s_invalidPayloadLength = [1021];
    #endregion

    #region Auxiliary properties
    private static readonly Random s_randomNumberGenerator = new Random();
    #endregion

    #region Test cases
    [Test]
    public void InstantiationImpossibleUsingInvalidHeaderLength(
        [Values(0)] int invalidHeaderLength)
    {
        TestDelegate actionUnderTest = () => new SimpleSessionLayerProtocol(invalidHeaderLength);

        Assert.Throws<ArgumentOutOfRangeException>(actionUnderTest);
    }

    [Test]
    public void InstantiationPossibleUsingValidHeaderLength(
        [Values(1, 2)] int validHeaderLength)
    {
        TestDelegate actionUnderTest = () => new SimpleSessionLayerProtocol(validHeaderLength);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void InstanceExposesUsedHeaderLengthProperly(
        [Values(1, 2)] int validHeaderLength)
    {
        var protocol = new SimpleSessionLayerProtocol(validHeaderLength);

        Assert.That(protocol.HeaderLength, Is.EqualTo(validHeaderLength));
    }

    [Test]
    public void PreparingPacketUsingNullReferenceAsPayloadImpossible()
    {
        var protocol = new SimpleSessionLayerProtocol(DefaultHeaderLength);

        TestDelegate actionUnderTest = () => protocol.PreparePacket(null);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void PreparingPacketUsingPayloadWithInvalidLengthImpossible(
        [ValueSource(nameof(s_invalidPayloadLength))] int invalidPayloadLength)
    {
        var payload = new byte[invalidPayloadLength];
        s_randomNumberGenerator.NextBytes(payload);

        var protocol = new SimpleSessionLayerProtocol(DefaultHeaderLength);
        TestDelegate actionUnderTest = () => protocol.PreparePacket(payload);

        Assert.Throws<ArgumentException>(actionUnderTest);
    }

    [Test]
    public void PreparingPacketUsingPayloadWithValidLengthPossible(
        [ValueSource(nameof(s_validPayloadLength))] int validPayloadLength)
    {
        var payload = new byte[validPayloadLength];
        s_randomNumberGenerator.NextBytes(payload);

        var protocol = new SimpleSessionLayerProtocol(DefaultHeaderLength);
        TestDelegate actionUnderTest = () => protocol.PreparePacket(payload);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void SumOfPacketHeaderIsEqualToPayloadLength(
        [ValueSource(nameof(s_validPayloadLength))] int validPayloadLength)
    {
        var payload = new byte[validPayloadLength];
        s_randomNumberGenerator.NextBytes(payload);

        var protocol = new SimpleSessionLayerProtocol(DefaultHeaderLength);
        byte[] packet = protocol.PreparePacket(payload);

        int headerSum = packet.Take(DefaultHeaderLength).Select(Convert.ToInt32).Sum();
        Assert.That(headerSum, Is.EqualTo(validPayloadLength));
    }

    [Test]
    public void TransferringPayloadToPacketIsTransparent(
        [ValueSource(nameof(s_validPayloadLength))] int validPayloadLength)
    {
        var expectedPayload = new byte[validPayloadLength];
        s_randomNumberGenerator.NextBytes(expectedPayload);

        var protocol = new SimpleSessionLayerProtocol(DefaultHeaderLength);
        byte[] packet = protocol.PreparePacket(expectedPayload);
        
        byte[] actualPayload = packet.Skip(DefaultHeaderLength).ToArray();
        Assert.That(expectedPayload.SequenceEqual(actualPayload));
    }

    [Test]
    public void ExtractingPayloadFromNullReferenceNotPossible()
    {
        var protocol = new SimpleSessionLayerProtocol(DefaultHeaderLength);

        TestDelegate actionUnderTest = () => protocol.ExtractPayload(null);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void ExtractingPayloadFromInvalidPacketNotPossible()
    {
        var packet = new List<byte>();
        
        byte[] header = Enumerable.Range(0, DefaultHeaderLength).Select(value => byte.MaxValue).ToArray();
        packet.AddRange(header);

        var payload = new byte[0];
        packet.AddRange(payload);

        var protocol = new SimpleSessionLayerProtocol(DefaultHeaderLength);
        TestDelegate actionUnderTest = () => protocol.ExtractPayload(packet);

        Assert.Throws<ArgumentException>(actionUnderTest);
    }

    [Test]
    public void TransferringPayloadIsTransparent(
        [Values(0, 1)] int lengthOfUnusedBufferSpace,
        [ValueSource(nameof(s_validPayloadLength))] int validPayloadLength)
    {
        var expectedPayload = new byte[validPayloadLength];
        s_randomNumberGenerator.NextBytes(expectedPayload);

        var recivingBuffer = new List<byte>();

        var protocol = new SimpleSessionLayerProtocol(DefaultHeaderLength);
        byte[] packet = protocol.PreparePacket(expectedPayload);
        recivingBuffer.AddRange(packet);

        var unusedBufferSpace = new byte[lengthOfUnusedBufferSpace];
        s_randomNumberGenerator.NextBytes(unusedBufferSpace);
        recivingBuffer.AddRange(unusedBufferSpace);

        byte[] actualPayload = protocol.ExtractPayload(packet);
        Assert.That(expectedPayload.SequenceEqual(actualPayload));
    }
    #endregion
}
