using Client.Sockets;
using CommonUtilities.BitPadding;
using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using Server.Sockets;
using System.Net;


namespace IntegrationTests;

[Category("IntegrationTest")]
[NonParallelizable]
[Author("Jakub Miodunka")]
public class SocketsIntegrationTests
{
    #region Configuration
    private const string ServerIpAddress = "127.0.0.1";
    private const int ServerPort = 8888;
    #endregion

    #region Auxiliary properties
    private static readonly Random s_randomNumberGenerator = new Random();

    private ServerTcpSocket _server;
    private List<ClientTcpSocket> _clients;
    #endregion

    #region Auxiliary methods
    private IProtocol CreateProtocol()
    {
        const int HeaderLength = 4;

        return new SimpleSessionLayerProtocol(HeaderLength);
    }

    private ICipher CreateCipher()
    {
        var bitPaddingprovider = new PkcsBitPaddingProvider(TeaCipher.DataBlockSize);
        var encryptionKey = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

        return new TeaCipher(encryptionKey, bitPaddingprovider);
    }

    private ServerTcpSocket CreateServer()
    {
        const int ServerReceivingBufferSize = 1024;
        var serverIpAddress = IPAddress.Parse(ServerIpAddress);
        var serverEndPoint = new IPEndPoint(serverIpAddress, ServerPort);
        IProtocol protocol = CreateProtocol();
        ICipher cipher = CreateCipher();

        return new ServerTcpSocket(serverEndPoint, ServerReceivingBufferSize, protocol, cipher);
    }

    private ClientTcpSocket CreateClient()
    {
        const int ClientReceivingBufferSize = 1024;
        var serverIpAddress = IPAddress.Parse(ServerIpAddress);
        var serverEndPoint = new IPEndPoint(serverIpAddress, ServerPort);
        IProtocol protocol = CreateProtocol();
        ICipher cipher = CreateCipher();

        var client =  new ClientTcpSocket(serverEndPoint, ClientReceivingBufferSize, protocol, cipher);
        _clients.Add(client);

        return client;
    }
    #endregion

    #region Test arrangement
    [SetUp]
    public void Setup()
    {
        _server = CreateServer();
        _clients = new List<ClientTcpSocket>();
    }

    [TearDown]
    public void TearDown()
    {
        _clients.ForEach(clientSocket => clientSocket.Dispose());
        _server.Dispose();
    }
    #endregion

    #region Test cases
    [Test]
    public void ServerAcceptsClients(
        [Values(1, 2, 40)] int numberOfClients,
        [Values(250)] int eventsTimeout)
    {
        int acceptedConnections = 0;
        _server.ConnectionAcceptedEvent += (_) => acceptedConnections++;

        _server.StartAcceptingConnections();

        for (int i = 0; i < numberOfClients; i++)
        {
            ClientTcpSocket client = CreateClient();
            client.ConnectToServer();

            Task.Delay(eventsTimeout).Wait();
        }

        Assert.That(() => acceptedConnections, Is.EqualTo(numberOfClients).After(eventsTimeout * numberOfClients));
    }

    [Test]
    public void ServerTransfersDataToClient(
        [Values(10)] int dataLength,
        [Values(250)] int eventsTimeout)
    {
        int connectionIdentifier = 0;
        _server.ConnectionAcceptedEvent += (identifier) => connectionIdentifier = identifier;

        _server.StartAcceptingConnections();

        ClientTcpSocket clientSocket = CreateClient();

        var receivedData = new byte[0];
        clientSocket.DataReceivedEvent += (data) => receivedData = data;

        clientSocket.ConnectToServer();
        Task.Delay(eventsTimeout).Wait();    // Time required to process ConnectionAcceptedEvent by the server.

        var sentData = new byte[dataLength];
        s_randomNumberGenerator.NextBytes(sentData);

        _server.SentData(connectionIdentifier, sentData).Wait();

        Assert.That(() => sentData.SequenceEqual(receivedData), Is.True.After(eventsTimeout));
    }

    [Test]
    public void ServerReactsWhenClientsDisconnect(
        [Values(1, 2, 40)] int numberOfClients,
        [Values(250)] int eventsTimeout)
    {
        int closedConnections = 0;
        _server.ConnectionClosedEvent += (_) => closedConnections++;

        _server.StartAcceptingConnections();

        for (int i = 0; i < numberOfClients; i++)
        {
            ClientTcpSocket client = CreateClient();

            client.ConnectToServer();
            Task.Delay(eventsTimeout).Wait();

            client.Dispose();
            Task.Delay(eventsTimeout).Wait();
        }

        Assert.That(() => closedConnections, Is.EqualTo(numberOfClients).After(eventsTimeout * numberOfClients));
    }

    [Test]
    public void ClientTransfersDataToServer(
        [Values(10)] int dataLength,
        [Values(250)] int eventsTimeout)
    {
        var receivedData = new byte[0];
        _server.DataReceivedEvent += (_, data) => receivedData = data;

        _server.StartAcceptingConnections();

        ClientTcpSocket client = CreateClient();
        client.ConnectToServer();
        Task.Delay(eventsTimeout).Wait();    // Time required to process ConnectionAcceptedEvent by the server.

        var sentData = new byte[dataLength];
        s_randomNumberGenerator.NextBytes(sentData);
        client.SentData(sentData).Wait();

        Assert.That(() => sentData.SequenceEqual(receivedData), Is.True.After(eventsTimeout));
    }

    [Test]
    public void ClientReactsWhenServerDisconnects(
        [Values(250)] int eventsTimeout)
    {
        _server.StartAcceptingConnections();

        ClientTcpSocket client = CreateClient();
        
        bool eventRaised = false;
        client.ConnectionClosedEvent += () => eventRaised = true;

        client.ConnectToServer();
        Task.Delay(eventsTimeout).Wait();    // Time required to process ConnectionAcceptedEvent by the server.

        _server.Dispose();
        
        Assert.That(() => eventRaised, Is.True.After(eventsTimeout));
    }
    #endregion
}
