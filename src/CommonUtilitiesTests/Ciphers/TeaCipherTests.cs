// Ignore Spelling: Misconfigured

using CommonUtilities.Ciphers;
using CommonUtilities.Padding;
using Moq;


namespace CommonUtilitiesTests.Ciphers;

[TestOf(typeof(TeaCipher))]
[Category("Unit Test")]
[Author("Jakub Miodunka")]
public sealed class TeaCipherTests
{
    #region Constants
    private const int ValidSizeOfDataBlock = 8;         // TEA operates on 32-bit (8 bytes) data blocks.
    private const int ValidSizeOfEncryptionKey = 16;    // TEA is using 128-bit (16 bytes) encryption key.
    #endregion

    #region Auxiliary properties
    private static readonly Random s_randomNumberGenerator = new Random();
    #endregion

    #region Auxiliary methods
    private static Mock<IBitPaddingProvider> CreateTransparentBitPaddingProviderFake()
    {
        var bitPaddingProviderFake = new Mock<IBitPaddingProvider>();

        bitPaddingProviderFake
            .Setup(bitPaddingProvider => bitPaddingProvider.SizeOfDataBlock)
            .Returns(ValidSizeOfDataBlock);

        bitPaddingProviderFake
            .Setup(bitPaddingProvider => bitPaddingProvider.AddBitPadding(It.IsAny<byte[]>()))
            .Returns<byte[]>(inputDataSet => inputDataSet);

        bitPaddingProviderFake
            .Setup(bitPaddingProvider => bitPaddingProvider.RemoveBitPadding(It.IsAny<byte[]>()))
            .Returns<byte[]>(inputDataSet => inputDataSet);

        return bitPaddingProviderFake;
    }
    #endregion

    #region Test cases
    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsEncryptionKey()
    {
        Mock<IBitPaddingProvider> bitPaddingProviderStub = CreateTransparentBitPaddingProviderFake();

        TestDelegate actionUnderTest = () => new TeaCipher(null, bitPaddingProviderStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingInvalidEncryptionKey(
        [Values(15, 17)] int invalidSizeOfEncryptionKey)
    {
        var encryptionKey = new byte[invalidSizeOfEncryptionKey];
        s_randomNumberGenerator.NextBytes(encryptionKey);

        Mock<IBitPaddingProvider> bitPaddingProviderStub = CreateTransparentBitPaddingProviderFake();

        TestDelegate actionUnderTest = () => new TeaCipher(encryptionKey, bitPaddingProviderStub.Object);

        Assert.Throws<ArgumentException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsBitPaddingProvider()
    {
        var encryptionKey = new byte[ValidSizeOfEncryptionKey];
        s_randomNumberGenerator.NextBytes(encryptionKey);

        TestDelegate actionUnderTest = () => new TeaCipher(encryptionKey, null);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingMisconfiguredBitPaddingProvider(
        [Values(7, 9)] int invalidSizeOfDataBlock)
    {
        var encryptionKey = new byte[ValidSizeOfEncryptionKey];
        s_randomNumberGenerator.NextBytes(encryptionKey);

        Mock<IBitPaddingProvider> bitPaddingProviderStub = CreateTransparentBitPaddingProviderFake();
        bitPaddingProviderStub
            .Setup(bitPaddingProvider => bitPaddingProvider.SizeOfDataBlock)
            .Returns(invalidSizeOfDataBlock);

        TestDelegate actionUnderTest = () => new TeaCipher(encryptionKey, bitPaddingProviderStub.Object);

        Assert.Throws<ArgumentException>(actionUnderTest);
    }

    [Test]
    public void EncryptionOfNullReferenceNotPossible()
    {
        var encryptionKey = new byte[ValidSizeOfEncryptionKey];
        s_randomNumberGenerator.NextBytes(encryptionKey);

        Mock<IBitPaddingProvider> bitPaddingProviderStub = CreateTransparentBitPaddingProviderFake();

        var instanceUnderTest = new TeaCipher(encryptionKey, bitPaddingProviderStub.Object);

        TestDelegate actionUnderTest = () => instanceUnderTest.Encrypt(null);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void DecryptionOfNullReferenceNotPossible()
    {
        var encryptionKey = new byte[ValidSizeOfEncryptionKey];
        s_randomNumberGenerator.NextBytes(encryptionKey);

        Mock<IBitPaddingProvider> bitPaddingProviderStub = CreateTransparentBitPaddingProviderFake();

        var instanceUnderTest = new TeaCipher(encryptionKey, bitPaddingProviderStub.Object);

        TestDelegate actionUnderTest = () => instanceUnderTest.Decrypt(null);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void EncryptionIsTransparent(
        [Values(0, 2, 3, 4, 8, 9, 16, 27)] int numberOfDataBlocksToProcess)
    {
        var encryptionKey = new byte[ValidSizeOfEncryptionKey];
        s_randomNumberGenerator.NextBytes(encryptionKey);

        var inputDataSet = new byte[ValidSizeOfDataBlock * numberOfDataBlocksToProcess];
        s_randomNumberGenerator.NextBytes(inputDataSet);

        Mock<IBitPaddingProvider> bitPaddingProviderStub = CreateTransparentBitPaddingProviderFake();

        var instanceUnderTest = new TeaCipher(encryptionKey, bitPaddingProviderStub.Object);

        byte[] encryptedDataSet = instanceUnderTest.Encrypt(inputDataSet);
        byte[] decryptedDataSet = instanceUnderTest.Decrypt(encryptedDataSet);

        Assert.That(decryptedDataSet.SequenceEqual(inputDataSet));
    }
    #endregion
}
