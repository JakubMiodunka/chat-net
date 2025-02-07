using Client;
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

// Main:
using (var socketClient = new ClientTcpSocket(serverEndPoint, receivingBufferSize, protocol, cipher))
{
    socketClient.DataReceivedCallback = (data) => Console.WriteLine($"\rSERVER: {Encoding.UTF8.GetString(data.ToArray())}");
    socketClient.ConnectionClosedCallback = () => Console.WriteLine($"\rSERVER CLOSED CONNECTION");
    socketClient.ConnectToServer();

    _ = socketClient.StartListeningForData();   // Listen for incoming data in background.

    while (true)
    {
        Console.Write("\rCLIENT:");
        string? input = Console.ReadLine();

        if (input is not null)
        {
            byte[] inputAsBytes = Encoding.UTF8.GetBytes(input);
            _ = socketClient.SentData(inputAsBytes);    // Sent typed string in background.
        }
    }
}
