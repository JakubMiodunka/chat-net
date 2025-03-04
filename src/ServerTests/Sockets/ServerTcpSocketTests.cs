using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using Moq;
using Server.Sockets;
using System.Net;


namespace ServerTests.Sockets;

[TestOf(typeof(ServerTcpSocket))]
[Category("Unit Test")]
[NonParallelizable]
[Author("Jakub Miodunka")]
public class ServerTcpSocketTests
{
    #region Default values
    private const int DefaultPort = 8888;
    private const int DefaultReceivingBufferSize = 1024;

    private IPEndPoint _defaultIpEndPoint;
    #endregion

    #region Test setup
    [SetUp]
    public void SetUp()
    {
        _defaultIpEndPoint = new IPEndPoint(IPAddress.Loopback, DefaultPort);
    }
    #endregion

    #region Test cases
    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsServerIpEndPoint()
    {
        var protocolStub = new Mock<IProtocol>();
        var cipherStub = new Mock<ICipher>();

        TestDelegate actionUnderTest =
            () => { using (var serverTcpSocket = new ServerTcpSocket(null, DefaultReceivingBufferSize, protocolStub.Object, cipherStub.Object)); };

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsProtocol()
    {
        var cipherStub = new Mock<ICipher>();

        TestDelegate actionUnderTest =
            () => { using (var serverTcpSocket = new ServerTcpSocket(_defaultIpEndPoint, DefaultReceivingBufferSize, null, cipherStub.Object)); };

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsCipher()
    {
        var protocolStub = new Mock<IProtocol>();

        TestDelegate actionUnderTest =
            () => { using (var serverTcpSocket = new ServerTcpSocket(_defaultIpEndPoint, DefaultReceivingBufferSize, protocolStub.Object, null)); };

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingInvalidReceivingBufferSize(
        [Values(0)] int invalidReceivingBufferSize)
    {
        var protocolStub = new Mock<IProtocol>();
        var cipherStub = new Mock<ICipher>();

        TestDelegate actionUnderTest =
            () => { using (var serverTcpSocket = new ServerTcpSocket(_defaultIpEndPoint, invalidReceivingBufferSize, protocolStub.Object, cipherStub.Object)); };

        Assert.Throws<ArgumentOutOfRangeException>(actionUnderTest);
    }

    [Test]
    public void InstantiationPossibleUsingValidReceivingBufferSize(
        [Values(1, 2)] int validReceivingBufferSize)
    {
        var protocolStub = new Mock<IProtocol>();
        var cipherStub = new Mock<ICipher>();

        TestDelegate actionUnderTest =
            () => { using (var serverTcpSocket = new ServerTcpSocket(_defaultIpEndPoint, validReceivingBufferSize, protocolStub.Object, cipherStub.Object)); };

        Assert.DoesNotThrow(actionUnderTest);
    }
    #endregion
}
