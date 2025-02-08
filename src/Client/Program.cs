using Client.Sockets;
using CommonUtilities.BitPadding;
using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
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
void DataReceivedEventHandler(byte[] receivedData)
{
    string receivedText = Encoding.UTF8.GetString(receivedData);
    Console.WriteLine($"\rServer: {receivedText}");
}

void ConnectionClosedEventHandler()
{
    Console.WriteLine($"\rServer: Connection closed.");
}

// Main:
using (var socketClient = new ClientTcpSocket(serverEndPoint, receivingBufferSize, protocol, cipher))
{
    socketClient.DataReceivedEvent += DataReceivedEventHandler;
    socketClient.ConnectionClosedEvent += ConnectionClosedEventHandler;

    socketClient.ConnectToServer();
    Console.WriteLine("Client: Connected to server.");
    _ = socketClient.StartListeningForData();

    while (true)
    {
        string? input = Console.ReadLine();

        if (input is not null)
        {
            byte[] inputAsBytes = Encoding.UTF8.GetBytes(input);
            _ = socketClient.SentData(inputAsBytes);

            if (input == "end") // To perform graceful shutdown.
            {
                break;
            }
        }
    }
}

Console.WriteLine("Press ENTER to continue...");
Console.ReadLine();
