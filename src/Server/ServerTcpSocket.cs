// Ignore Spelling: ip

using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Server;

/// TODO: Add mechanism of limiting actively handled connections.
/// TODO: Add callback invoked every time when new connection will be accepted.
/// <summary>
/// Socket wrapper, which serves as a server in TCP client-server architecture.
/// </summary>
public sealed class ServerTcpSocket : IDisposable
{
    #region Properties
    private readonly Socket _listeningSocket;
    private readonly int _receivingBufferSize;
    private readonly IProtocol _protocol;
    private readonly ICipher _cipher;
    private readonly List<TcpConnectionHandler> _connectionHandlers;
    private bool _shallAcceptConnections;
    #endregion

    #region Instantiation
    /// <summary>
    /// Initializes server TCP socket.
    /// </summary>
    /// <param name="ipEndPoint">
    /// End point, to which listening socket shall bind to.
    /// </param>
    /// <param name="receivingBufferSize">
    /// Size of a buffer, used by every connection handle for buffering data incoming from particular client.
    /// Expressed in bytes.
    /// </param>
    /// <param name="protocol">
    /// Session layer protocol, which shall be used during communication with each client.
    /// </param>
    /// <param name="cipher">
    /// Cipher, which shall be used during communication with each client to encrypt and decrypt data. 
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown, when value of at least one argument will be considered as invalid.
    /// </exception>
    public ServerTcpSocket(IPEndPoint ipEndPoint, int receivingBufferSize, IProtocol protocol, ICipher cipher)
    {
        #region Arguments validation
        if (ipEndPoint is null)
        {
            string argumentName = nameof(ipEndPoint);
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

        _listeningSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _receivingBufferSize = receivingBufferSize;
        _protocol = protocol;
        _cipher = cipher;
        _connectionHandlers = new List<TcpConnectionHandler>();
        _shallAcceptConnections = false;

        _listeningSocket.Bind(ipEndPoint);
    }
    #endregion

    #region Interactions
    /// TODO: Remove demo code and re-factor when time will come.
    /// TODO: Figure out how to launch connection handler actions in entirely new thread (maybe using Thread class?).
    /// <summary>
    /// Processes newly accepted connection.
    /// </summary>
    /// <param name="connectionSocket">
    /// Socket referring to newly accepted connection.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    private void ProcessAcceptedConnection(Socket connectionSocket)
    {
        #region Arguments validation
        if (connectionSocket is null)
        {
            string argumentName = nameof(connectionSocket);
            const string ErrorMessage = "Provided client socket a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        var connectionHandler = new TcpConnectionHandler(connectionSocket, _receivingBufferSize, _protocol, _cipher);
        connectionHandler.ReceivedDataCallback = (connectionId, receivedData) => Console.WriteLine($"CLIENT {connectionId}, MESSAGE: {Encoding.UTF8.GetString(receivedData.ToArray())}"); // Only for demo.
        _connectionHandlers.Add(connectionHandler);

        Task.Run(() => connectionHandler.StartListeningForData());
    }

    /// <summary>
    /// Triggers continues process of listening and accepting new connections on listening socket.
    /// </summary>
    /// <returns>
    /// Task related to listening and accepting new connections on listening socket.
    /// </returns>
    public async Task StartAcceptingConnections()
    {
        _shallAcceptConnections = true;
        
        _listeningSocket.Listen();

        while (_shallAcceptConnections)
        {
            Socket connectionSocket = await _listeningSocket.AcceptAsync();
            ProcessAcceptedConnection(connectionSocket);
        }
    }

    /// <summary>
    /// Suppresses accepting new connections, disposes all connection handlers along with listening socket.
    /// </summary>
    public void Dispose()
    {
        _shallAcceptConnections = false;

        _connectionHandlers.ForEach(connectionHandler => connectionHandler.Dispose());

        // For connection-oriented protocols (such as TCP),
        // it is recommended to call Socket.Shutdown before disposing the socket (calling socket.Close()).
        _listeningSocket.Shutdown(SocketShutdown.Both);
        _listeningSocket.Close();                         // Calls Socket.Dispose() internally.
    }
    #endregion
}
