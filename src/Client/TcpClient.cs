using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using System.Net;
using System.Net.Sockets;


namespace Client;

/// <summary>
/// TCP client, capable to send and receive data in full-duplex manner.
/// </summary>
public class TcpClient : IDisposable
{
    #region Delegates
    public delegate void ReceivedDataCallback(IEnumerable<byte> data);
    #endregion

    #region Properties
    private readonly IPEndPoint _serverEndPoint;
    private readonly Socket _clientSocket;
    private readonly IProtocol _protocol;
    private readonly ICipher _cipher;
    private readonly int _receivingBufferSize;
    private readonly HashSet<ReceivedDataCallback> _receivedDataCallbacks;
    private bool _shallListenFroData;
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates new instance of TCP client.
    /// </summary>
    /// <param name="serverEndPoint">
    /// Server end point.
    /// </param>
    /// <param name="receivingBufferSize">
    /// Size of receiving buffer. Expressed in bytes.
    /// </param>
    /// <param name="protocol">
    /// Session layer protocol, which shall be used during communication.
    /// </param>
    /// <param name="cipher">
    /// Cipher, which shall be used during communication to encrypt and decrypt data. 
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown, when value of at least one argument will be considered as invalid.
    /// </exception>
    public TcpClient(IPEndPoint serverEndPoint, int receivingBufferSize, IProtocol protocol, ICipher cipher)
    {
        #region Arguments validation
        if (serverEndPoint is null)
        {
            string argumentName = nameof(serverEndPoint);
            const string ErrorMessage = "Provided IP end point is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

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

        _serverEndPoint = serverEndPoint;
        _clientSocket = new Socket(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _protocol = protocol;
        _cipher = cipher;
        _receivingBufferSize = receivingBufferSize;
        _receivedDataCallbacks = new HashSet<ReceivedDataCallback>();
        _shallListenFroData = false;
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Adds provided delegate to a pool of callbacks, which are invoked every time,
    /// when TCP client receives new data from a server.
    /// </summary>
    /// <remarks>
    /// Callbacks pool can store only one reference to particular callback.
    /// </remarks>
    /// <param name="newCallback">
    /// Callback which shall be invoked every time, when TCP client received new data from server.
    /// </param>
    public void AddReceivedDataCallback(ReceivedDataCallback newCallback)
    {
        _receivedDataCallbacks.Add(newCallback);
    }

    /// <summary>
    /// Connects TCP client to server.
    /// </summary>
    /// <remarks>
    /// Server end point shall be specified during instance initialization.
    /// </remarks>
    public void ConnectToServer()
    {
        _clientSocket.Connect(_serverEndPoint);
    }

    /// <summary>
    /// Triggers an infinite process of listening for data incoming from server.
    /// </summary>
    /// <returns>
    /// Task related to pending data transfer.
    /// </returns>
    public async Task StartListeningForData()
    {
        var receivedData = new List<byte>();
        var receivingBuffer = new byte[_receivingBufferSize];

        _shallListenFroData = true;

        while (_shallListenFroData)
        {
            int sizeOfReceivedDataChunk = await _clientSocket.ReceiveAsync(receivingBuffer);
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

            _receivedDataCallbacks.ToList()
                .ForEach(callBack => callBack.Invoke(decryptedPayload));
        }
    }

    /// <summary>
    /// Sends provided data to server.
    /// </summary>
    /// <param name="data">
    /// Data, which shall be sent to the server.
    /// </param>
    /// <returns>
    /// Task related to pending data transfer.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
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
        #endregion

        byte[] encryptedData = _cipher.Encrypt(data);
        byte[] packet = _protocol.PreparePacket(payload: encryptedData);

        await _clientSocket.SendAsync(packet);
    }

    /// <summary>
    /// Shuts down both sending and receiving operations of TCP client.
    /// </summary>
    public void Dispose()
    {
        _shallListenFroData = false;
        _clientSocket.Shutdown(SocketShutdown.Both);
    }
    #endregion
}
