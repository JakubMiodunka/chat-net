using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using System.Net.Sockets;


namespace CommonUtilities.Sockets;

/// TODO: Figure out how to detect if connection was closed by remote resource and implement reaction to it.
/// <summary>
/// Implementation of TCP socket, capable to transfer encrypted data in full-duplex manner
/// (to send and receive encrypted data simultaneously).
/// </summary>
public abstract class FullDuplexTcpSocket : IDisposable
{
    #region Properties
    private readonly int _receivingBufferSize;
    private readonly IProtocol _protocol;
    private readonly ICipher _cipher;
    
    protected abstract Socket Socket { get; }   // Shall be connected to remote resource by derivative class before transferring data.
    #endregion

    #region Instantiation
    /// <summary>
    /// Initializes functionalities of full-duplex TCP socket.
    /// </summary>
    /// <param name="receivingBufferSize">
    /// Size of a buffer, used for buffering incoming data.
    /// </param>
    /// <param name="protocol">
    /// Session layer protocol, which shall be used during communication.
    /// </param>
    /// <param name="cipher">
    /// Cipher, which shall be used during communication to encrypt and decrypt data. 
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown, when value of at least one argument will be considered as invalid.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    protected FullDuplexTcpSocket(int receivingBufferSize, IProtocol protocol, ICipher cipher)
    {
        #region Arguments validation
        if (receivingBufferSize < 1)
        {
            string argumentName = nameof(receivingBufferSize);
            string errorMessage = $"Specified size of receiving buffer too small: {receivingBufferSize}";
            throw new ArgumentOutOfRangeException(argumentName, receivingBufferSize, errorMessage);
        }

        if (protocol is null)
        {
            string argumentName = nameof(protocol);
            const string ErrorMessage = "Provided protocol is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (cipher is null)
        {
            string argumentName = nameof(cipher);
            const string ErrorMessage = "Provided cipher is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        _receivingBufferSize = receivingBufferSize;
        _protocol = protocol;
        _cipher = cipher;
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Processes received patches of data.
    /// </summary>
    /// <remarks>
    /// Method shall be implemented by derivative class according to specific needs.
    /// </remarks>
    /// <param name="receivedData">
    /// New patch of data, required to be processed further.
    /// </param>
    protected abstract void ProcessReceivedData(IEnumerable<byte> receivedData);

    /// <summary>
    /// Triggers continues process of listening for new patches of data on socket.
    /// </summary>
    /// <returns>
    /// Task related to pending data transfer.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown, when socket is not connected to remote resource.
    /// </exception>
    public async Task StartListeningForData()
    {
        #region Arguments validation
        if (!Socket.Connected)
        {
            const string ErrorMessage = "Socket not connected to remote resource:";
            throw new InvalidOperationException(ErrorMessage);
        }
        #endregion

        var receivedData = new List<byte>();
        var receivingBuffer = new byte[_receivingBufferSize];

        while (Socket.Connected)
        {
            int sizeOfReceivedDataChunk = await Socket.ReceiveAsync(receivingBuffer);
            byte[] receivedDataChunk = receivingBuffer.Take(sizeOfReceivedDataChunk).ToArray();
            receivedData.AddRange(receivedDataChunk);

            byte[] encryptedPayload;

            try
            {
                encryptedPayload = _protocol.ExtractPayload(receivedData);
            }
            catch (ArgumentException)
            {
                continue;
            }

            receivedData.Clear();

            byte[] decryptedPayload = _cipher.Decrypt(encryptedPayload);

            ProcessReceivedData(decryptedPayload);
        }
    }

    /// <summary>
    /// Sends provided data through the socket.
    /// </summary>
    /// <param name="data">
    /// Patch of data, which shall be sent.
    /// </param>
    /// <returns>
    /// Task related to pending data transfer.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown, when socket is not connected to remote resource.
    /// </exception>
    public async Task SentData(IEnumerable<byte> data)
    {
        #region Arguments validation
        if (data is null)
        {
            string argumentName = nameof(data);
            const string ErrorMessage = "Provided data is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (!Socket.Connected)
        {
            const string ErrorMessage = "Socket not connected to remote resource:";
            throw new InvalidOperationException(ErrorMessage);
        }
        #endregion

        byte[] encryptedData = _cipher.Encrypt(data);
        byte[] packet = _protocol.PreparePacket(payload: encryptedData);

        await Socket.SendAsync(packet);
    }

    /// <summary>
    /// Suppresses currently pending sending and receiving operations on socket
    /// and dispose the socket itself.
    /// </summary>
    public void Dispose()
    {
        if (Socket.Connected)
        {
            // For connection-oriented protocols (such as TCP),
            // it is recommended to call Socket.Shutdown before disposing the socket (calling socket.Close()).
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();                         // Calls Socket.Dispose() internally.
        }
    }
    #endregion
}
