using Client.Sockets;
using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using Moq;
using System.Net;


namespace UnitTests.Client;

[Category("UnitTest")]
[TestOf(typeof(ClientTcpSocket))]
[NonParallelizable]
[Author("Jakub Miodunka")]
public class ClientTcpSocketTests
{
    #region Default values
    private const int DefaultReceivingBufferSize = 1024;
    
    private IPEndPoint _defaultServerIpEndPoint;
    #endregion

    #region Test setup
    [SetUp]
    public void SetUp()
    {
        _defaultServerIpEndPoint = new IPEndPoint(IPAddress.Loopback, 8888);
    }
    #endregion

    #region Test cases
    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsServerIpEndPoint()
    {
        var protocolStub = new Mock<IProtocol>();
        var cipherStub = new Mock<ICipher>();

        TestDelegate actionUnderTest =
            () => { using (var clientTcpSocket = new ClientTcpSocket(null, DefaultReceivingBufferSize, protocolStub.Object, cipherStub.Object)); };

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsProtocol()
    {
        var cipherStub = new Mock<ICipher>();

        TestDelegate actionUnderTest =
            () => { using (var clientTcpSocket = new ClientTcpSocket(_defaultServerIpEndPoint, DefaultReceivingBufferSize, null, cipherStub.Object)) ; };

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsCipher()
    {
        var protocolStub = new Mock<IProtocol>();

        TestDelegate actionUnderTest =
            () => { using (var clientTcpSocket = new ClientTcpSocket(_defaultServerIpEndPoint, DefaultReceivingBufferSize, protocolStub.Object, null)); };

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingInvalidReceivingBufferSize(
        [Values(0)] int invalidReceivingBufferSize)
    {
        var protocolStub = new Mock<IProtocol>();
        var cipherStub = new Mock<ICipher>();

        TestDelegate actionUnderTest =
            () => { using (var clientTcpSocket = new ClientTcpSocket(_defaultServerIpEndPoint, invalidReceivingBufferSize, protocolStub.Object, cipherStub.Object)); };

        Assert.Throws<ArgumentOutOfRangeException>(actionUnderTest);
    }

    [Test]
    public void InstantiationPossibleUsingValidReceivingBufferSize(
        [Values(1, 2)] int validReceivingBufferSize)
    {
        var protocolStub = new Mock<IProtocol>();
        var cipherStub = new Mock<ICipher>();

        TestDelegate actionUnderTest =
            () => { using (var clientTcpSocket = new ClientTcpSocket(_defaultServerIpEndPoint, validReceivingBufferSize, protocolStub.Object, cipherStub.Object)); };

        Assert.DoesNotThrow(actionUnderTest);
    }
    #endregion
}
