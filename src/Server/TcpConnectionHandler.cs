using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using CommonUtilities.Sockets;
using System.Net.Sockets;


namespace Server;

/// <summary>
/// Handles connections accepted by TCP-based server.
/// Capable to transfer encrypted data in full-duplex manner.
/// </summary>
public sealed class TcpConnectionHandler : FullDuplexTcpSocket
{
    #region Static properties
    private static int s_nextConnectionIdentifier = 1;
    #endregion

    #region Properties
    protected override Socket Socket { get; }

    public readonly int ConnectionIdentifier;                       // Unique for every connection during program runtime.
    public Action<int, IEnumerable<byte>>? ReceivedDataCallback;    // Shall be assigned externally to process revived data further.
    public Action<int>? ConnectionClosedCallback;                   // Shall be assigned externally to process the event further. 
    #endregion

    #region Instantiation
    /// <summary>
    /// Initializes TCP connection handler.
    /// </summary>
    /// <param name="clientSocket">
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
    public TcpConnectionHandler(Socket clientSocket, int receivingBufferSize, IProtocol protocol, ICipher cipher)
        : base(receivingBufferSize, protocol, cipher)
    {
        #region Arguments validation
        if (clientSocket is null)
        {
            string argumentName = nameof(clientSocket);
            const string ErrorMessage = "Provided client socket is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (!clientSocket.Connected)
        {
            string argumentName = nameof(clientSocket);
            const string ErrorMessage = "Provided client socket not connected:";
            throw new ArgumentException(ErrorMessage, argumentName);
        }
        #endregion

        Socket = clientSocket;

        ConnectionIdentifier = s_nextConnectionIdentifier++;
        ReceivedDataCallback = null;
        ConnectionClosedCallback = null;
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Calls received data callback with connection identifier and data received from client.
    /// </summary>
    /// <param name="receivedData">
    /// Chunk of data received from client site.
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
            ReceivedDataCallback.Invoke(ConnectionIdentifier, receivedData);
        }
    }

    /// <summary>
    /// Calls connection closed callback as a reaction to closing connection by the client.
    /// </summary>
    protected override void ReactOnConnectionClose()
    {
        if (ConnectionClosedCallback is not null)
        {
            ConnectionClosedCallback.Invoke(ConnectionIdentifier);
        }
    }
    #endregion
}
