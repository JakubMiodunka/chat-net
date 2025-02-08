using CommonUtilities.BitPadding;
using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using Server.Sockets;
using System.Net;
using System.Text;

// Configuration:
var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8888);
int receivingBufferSize = 1024;
var protocol = new SimpleSessionLayerProtocol(4);
var encryptionKey = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
var bitPaddingProvider = new PkcsBitPaddingProvider(TeaCipher.DataBlockSize);
var cipher = new TeaCipher(encryptionKey, bitPaddingProvider);

// Event handlers:
void ConnectionAcceptedEventHandler(int connectionIdentifier)
{
    Console.WriteLine($"Connection: {connectionIdentifier}: Connection accepted.");
}

void DataReceivedEventHandler(ServerTcpSocket sender, int connectionIdentifier, byte[] receivedData)
{
    string receivedText = Encoding.UTF8.GetString(receivedData);
    Console.WriteLine($"Connection: {connectionIdentifier}: {receivedText}");

    // Echo-ing back to client.
    _ = sender.SentData(connectionIdentifier, receivedData);
}

void ConnectionClosedEventHandler(int connectionIdentifier)
{
    Console.WriteLine($"Connection: {connectionIdentifier}: Connection closed.");
}

// Main:
using (var tcpServer = new ServerTcpSocket(serverEndPoint, receivingBufferSize, protocol, cipher))
{
    tcpServer.ConnectionAcceptedEvent += ConnectionAcceptedEventHandler;
    tcpServer.DataReceivedEvent += (int connectionIdentifier, byte[] receivedData) => DataReceivedEventHandler(tcpServer, connectionIdentifier, receivedData);
    tcpServer.ConnectionClosedEvent += ConnectionClosedEventHandler;

    _ = tcpServer.StartAcceptingConnections();
    Console.WriteLine("Server: Listening for connections...");

    while (true)
    {
        string? input = Console.ReadLine();

        if (input is not null)
        {
            if (input == "end") // To perform graceful shutdown.
            {
                break;
            }
        }
    }
}

Console.WriteLine("Press ENTER to continue...");
Console.ReadLine();
