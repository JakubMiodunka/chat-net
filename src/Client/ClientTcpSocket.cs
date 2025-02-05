using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using CommonUtilities.Sockets;
using System.Net;
using System.Net.Sockets;


namespace Client;

/// <summary>
/// Socket wrapper, which serves as a client in TCP client-server architecture.
/// Capable to transfer encrypted data in full-duplex manner.
/// </summary>
public sealed class ClientTcpSocket : FullDuplexTcpSocket
{
    #region Properties
    private readonly IPEndPoint _serverEndPoint;

    protected override Socket Socket { get; }

    public Action<IEnumerable<byte>>? ReceivedDataCallback; // Shall be assigned externally to process revived data further. 
    #endregion

    #region Instantiation
    /// <summary>
    /// Initializes client TCP socket.
    /// </summary>
    /// <param name="serverEndPoint">
    /// End point of a server, to which client shall connect.
    /// </param>
    /// <param name="receivingBufferSize">
    /// Size of a buffer, used for buffering data incoming from the server site.
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
    public ClientTcpSocket(IPEndPoint serverEndPoint, int receivingBufferSize, IProtocol protocol, ICipher cipher)
        : base(receivingBufferSize, protocol, cipher)
    {
        #region Arguments validation
        if (serverEndPoint is null)
        {
            string argumentName = nameof(serverEndPoint);
            const string ErrorMessage = "Provided IP end point is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        _serverEndPoint = serverEndPoint;

        Socket = new Socket(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        ReceivedDataCallback = null;
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Calls received data callback with data received from server site to process it further.
    /// </summary>
    /// <param name="receivedData">
    /// Chunk of data received from server site.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    protected override void ProcessReceivedData(IEnumerable<byte> receivedData)
    {
        #region Arguments validation
        if (receivedData is null)
        {
            string argumentName = nameof(receivedData);
            const string ErrorMessage = "Provided data is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        if (ReceivedDataCallback is not null)
        {
            ReceivedDataCallback.Invoke(receivedData);
        }
    }

    /// <summary>
    /// Connects client TCP socket to server.
    /// </summary>
    /// <remarks>
    /// Server end point shall be specified during instance initialization.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown, when socket is already connected to server.
    /// </exception>
    public void ConnectToServer()
    {
        #region Arguments validation
        if (Socket.Connected)
        {
            const string ErrorMessage = "Socket already connected to server:";
            throw new InvalidOperationException(ErrorMessage);
        }      
        #endregion

        Socket.Connect(_serverEndPoint);
    }
    #endregion
}
