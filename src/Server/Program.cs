using CommonUtilities.BitPadding;
using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using Server;
using System.Net;


// Configuration:
var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8888);
int receivingBufferSize = 1024;
var protocol = new SimpleSessionLayerProtocol(4);
var encryptionKey = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
var bitPaddingProvider = new PkcsBitPaddingProvider(TeaCipher.DataBlockSize);
var cipher = new TeaCipher(encryptionKey, bitPaddingProvider);

// Main:
using (var tcpServer = new ServerTcpSocket(serverEndPoint, receivingBufferSize, protocol, cipher))
{
    tcpServer.ConnectionAcceptedEvent += (connectionId) => Console.WriteLine($"CLIENT {connectionId}, CONNECTION ACCEPTED");
    _ = tcpServer.StartAcceptingConnections();

    while (true)
    {

    }
}
