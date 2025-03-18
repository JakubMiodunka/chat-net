using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using CommonUtilities.Sockets;
using System.Net.Sockets;


namespace Server.Sockets;

/// <summary>
/// Handles connections accepted by TCP-based server.
/// Capable to transfer encrypted data in full-duplex manner.
/// </summary>
/// <remarks>
/// To get it to operational state properly, first instantiate the class member, then assign events handlers
/// and finally call StartListeningForData() method to start data transfer.
/// Do not forget to dispose created instance, when it will be no longer needed.
/// </remarks>
internal sealed class TcpConnectionHandler : FullDuplexTcpSocket
{
    #region Delegates
    public delegate void DataReceivedDelegate(int connectionIdentifier, byte[] receivedData);
    public delegate void ConnectionClosedDelegate(int connectionIdentifier);
    #endregion

    #region Static properties
    private static int s_nextConnectionIdentifier = 1;
    #endregion

    #region Properties
    private Task? _listeningForDataTask;
    private readonly CancellationTokenSource _cancellationTokenSourceForDataListening;

    protected override Socket Socket { get; }

    public readonly int ConnectionIdentifier;
    public DataReceivedDelegate? DataReceivedEvent;
    public ConnectionClosedDelegate? ConnectionClosedEvent;

    public bool IsActive    // Is false, when handled connection was closed, true otherwise.
    {
        get;
        private set;
    }
    #endregion

    #region Instantiation
    /// <summary>
    /// Initializes TCP connection handler.
    /// </summary>
    /// <param name="connectionSocket">
    /// Socket used to handle particular client connection.
    /// </param>
    /// <param name="receivingBufferSize">
    /// Size of a buffer, used for buffering data incoming from the client site.
    /// Expressed in bytes.
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
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one argument will be considered as invalid.
    /// </exception>
    public TcpConnectionHandler(Socket connectionSocket, int receivingBufferSize, IProtocol protocol, ICipher cipher)
        : base(receivingBufferSize, protocol, cipher)
    {
        #region Arguments validation
        if (connectionSocket is null)
        {
            string argumentName = nameof(connectionSocket);
            const string ErrorMessage = "Provided client socket is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (!connectionSocket.Connected)
        {
            string argumentName = nameof(connectionSocket);
            const string ErrorMessage = "Provided client socket not connected:";
            throw new ArgumentException(ErrorMessage, argumentName);
        }
        #endregion

        _listeningForDataTask = null;
        _cancellationTokenSourceForDataListening = new CancellationTokenSource();

        Socket = connectionSocket;

        ConnectionIdentifier = s_nextConnectionIdentifier++;
        IsActive = connectionSocket.Connected;
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Processes patches of data received from client site.
    /// </summary>
    /// <param name="receivedData">
    /// Chunk of data received from client site.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    protected override void RaiseDataReceivedEvent(IEnumerable<byte> receivedData)
    {
        #region Arguments validation
        if (receivedData is null)
        {
            string argumentName = nameof(receivedData);
            const string ErrorMessage = "Provided data is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        DataReceivedEvent?.Invoke(ConnectionIdentifier, receivedData.ToArray());
    }

    /// <summary>
    /// Reacts to situation, when client will close the connection.
    /// </summary>
    protected override void RaiseConnectionClosedEvent()
    {
        IsActive = false;   // Setting property value manually as value of Socket.Connected is still true.
        ConnectionClosedEvent?.Invoke(ConnectionIdentifier);
    }

    /// <summary>
    /// Triggers continues process of listening for new patches of data on socket.
    /// </summary>
    /// <remarks>
    /// Makes base.StartListeningForData method public as freshly created handler instance
    /// is not fully configured. Process of listening for incoming data shall be triggered
    /// manually, right after configuration of handler instance will be finished.
    /// </remarks>
    public void StartListeningForData()
    {
        _listeningForDataTask = StartListeningForData(_cancellationTokenSourceForDataListening.Token);
    }

    /// <summary>
    /// Suppresses currently pending sending and receiving operations on socket
    /// and dispose the socket itself.
    /// </summary>
    public override void Dispose()
    {
        if (_listeningForDataTask is not null)
        {
            _cancellationTokenSourceForDataListening.Cancel();
            _listeningForDataTask.Wait();
        }

        base.Dispose();

        IsActive = Socket.Connected;
    }
    #endregion
}
