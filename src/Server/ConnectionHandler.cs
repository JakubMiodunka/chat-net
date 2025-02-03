using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using System.Net.Sockets;
using System.Text;


namespace Server;

public sealed class ConnectionHandler
{
    #region Properties
    private readonly Socket _connectionSocket;
    private readonly byte[] _receivingBuffer;
    private readonly IProtocol _protocol;
    private readonly ICipher _cipher;
    #endregion

    #region Instantiation
    public ConnectionHandler(Socket connectionSocket, int receivingBufferSize, IProtocol sessionLayerProtocol, ICipher cipher)
    {
        #region Arguments validation
        if (connectionSocket is null)
        {
            string argumentName = nameof(connectionSocket);
            const string ErrorMessage = "Provided connection socket is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (receivingBufferSize < 1)
        {
            string argumentName = nameof(receivingBufferSize);
            string errorMessage = $"Specified size of receiving buffer too small: {receivingBufferSize}";
            throw new ArgumentOutOfRangeException(argumentName, receivingBufferSize, errorMessage);
        }

        if (sessionLayerProtocol is null)
        {
            string argumentName = nameof(sessionLayerProtocol);
            const string ErrorMessage = "Provided session layer protocol is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (cipher is null)
        {
            string argumentName = nameof(cipher);
            const string ErrorMessage = "Provided cipher is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        _connectionSocket = connectionSocket;
        _receivingBuffer = new byte[receivingBufferSize];
        _protocol = sessionLayerProtocol;
        _cipher = cipher;
    }
    #endregion

    #region Interactions
    private void ProcessRecivedData(IEnumerable<byte> receivedData)
    {
        #region Arguments validation
        if (receivedData is null)
        {
            string argumentName = nameof(receivedData);
            const string ErrorMessage = "Provided data is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        // Temporary code:
        string message = Encoding.UTF8.GetString(receivedData.ToArray());
        Console.WriteLine($"Received message: {message}");
    }

    public async Task ReceiveData()
    {
        int numberOfReceivedBytes = 0;

        while (true)
        {
            numberOfReceivedBytes += await _connectionSocket.ReceiveAsync(_receivingBuffer, SocketFlags.None);
            
            byte[] receivedBytes = _receivingBuffer.Take(numberOfReceivedBytes).ToArray();
            byte[] encryptedPayload;
            
            try
            {
                encryptedPayload = _protocol.ExtractPayload(receivedBytes);
            }
            catch (ArgumentException)
            {
                continue;
            }

            numberOfReceivedBytes = 0;

            byte[] decryptedPayload = _cipher.Decrypt(encryptedPayload);
            
            ProcessRecivedData(decryptedPayload);
        }
    }

    public async Task SentData(IEnumerable<byte> data)
    {
        #region Arguments validation
        if (data is null)
        {
            string argumentName = nameof(data);
            const string ErrorMessage = "Provided data is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        byte[] encryptedData = _cipher.Encrypt(data);
        byte[] packet = _protocol.PreparePacket(payload: encryptedData);

        await _connectionSocket.SendAsync(packet, SocketFlags.None);
    }
    #endregion
}
