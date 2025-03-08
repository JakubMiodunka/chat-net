using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using System.Net.Sockets;


namespace CommonUtilities.Sockets;

/// <summary>
/// Implementation of TCP socket, capable to transfer encrypted data in full-duplex manner
/// (to send and receive encrypted data simultaneously).
/// </summary>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.connected?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.receiveasync?view=net-9.0"/
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.sendasync?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.shutdown?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.close?view=net-9.0"/>
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
    /// Reacts to situation, when remote resource will close the connection..
    /// </summary>
    /// <remarks>
    /// Method shall be implemented by derivative class according to specific needs.
    /// </remarks>
    protected abstract void ReactOnConnectionClose();

    /// <summary>
    /// Triggers continues process of listening for new patches of data on socket.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token, which shall be bound to launched task.
    /// </param>
    /// <returns>
    /// Task related to pending data transfer.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown, when socket is not connected to remote resource.
    /// </exception>
    protected async Task StartListeningForData(CancellationToken cancellationToken)
    {
        #region Arguments validation
        if (!Socket.Connected)
        {
            const string ErrorMessage = "Socket not connected to remote resource:";
            throw new InvalidOperationException(ErrorMessage);
        }
        #endregion

        Task<int>? receivingTask = null;
        var receivedData = new List<byte>();
        var receivingBuffer = new byte[_receivingBufferSize];

        while (Socket.Connected && !cancellationToken.IsCancellationRequested)
        {
            receivingTask ??= Socket.ReceiveAsync(receivingBuffer);

            if (!receivingTask.IsCompleted)
            {
                await Task.Delay(200);  // TODO: Maybe this time period shall be included in configuration rather than hard-coded here.
                continue;
            }

            int sizeOfReceivedDataChunk = receivingTask.Result;
            receivingTask = null;

            // Receiving 0 bytes is an indicator, that remote resource closed its socket.
            if (sizeOfReceivedDataChunk == 0)
            {
                ReactOnConnectionClose();
                return;
            }

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
    public virtual void Dispose()
    {
        // Value of Socket.Connected property is not depend on state of connected remote resource.
        // Is set to 'true' during runtime of Socket.Connect method and to 'false', when socket is being disposed.
        // Is not updated when remote resource will close/dispose its socket.
        if (Socket.Connected)
        {
            // For connection-oriented protocols (such as TCP),
            // it is recommended to call Socket.Shutdown on connected socket before disposing it (calling socket.Close()).
            Socket.Shutdown(SocketShutdown.Both);
        }

        Socket.Close(); // Calls Socket.Dispose() internally.
    }
    #endregion
}
