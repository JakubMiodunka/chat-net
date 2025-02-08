// Ignore Spelling: ip

using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Server;

/// <summary>
/// Socket wrapper, which serves as a server in TCP client-server architecture.
/// </summary>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.bind?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.listen?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.acceptasync?view=net-9.0"/>
public sealed class ServerTcpSocket : IDisposable
{
    #region Properties
    private readonly Socket _listeningSocket;
    private readonly int _receivingBufferSize;
    private readonly IProtocol _protocol;
    private readonly ICipher _cipher;
    private readonly List<TcpConnectionHandler> _connectionHandlers;
    
    public bool ShallAcceptConnections;
    public event Action<int>? ConnectionAcceptedEvent; // Shall be assigned externally to process the event further.
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
        
        ShallAcceptConnections = false;

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
        connectionHandler.ReceivedDataEvent += (sender, receivedData) => Console.WriteLine($"CLIENT {sender.ConnectionIdentifier}, MESSAGE: {Encoding.UTF8.GetString(receivedData.ToArray())}"); // Only for demo.
        connectionHandler.ConnectionClosedEvent += (sender) => Console.WriteLine($"CLIENT {sender.ConnectionIdentifier}, CLENT CLOSED CONNECTION");    // Only for demo.
        connectionHandler.ConnectionClosedEvent += (sender) => _connectionHandlers.Remove(sender);

        _connectionHandlers.Add(connectionHandler);

        Task.Run(() => connectionHandler.StartListeningForData());

        ConnectionAcceptedEvent?.Invoke(connectionHandler.ConnectionIdentifier);
    }

    /// <summary>
    /// Triggers continues process of listening and accepting new connections on listening socket.
    /// </summary>
    /// <returns>
    /// Task related to listening and accepting new connections on listening socket.
    /// </returns>
    public async Task StartAcceptingConnections()
    {
        ShallAcceptConnections = true;
        
        _listeningSocket.Listen();

        while (ShallAcceptConnections)
        {
            Socket connectionSocket = await _listeningSocket.AcceptAsync();
            ProcessAcceptedConnection(connectionSocket);
        }
    }

    /// <summary>
    /// Suppresses accepting new connections, disposes all connection handlers along with listening socket.
    /// </summary>
    /// <remarks>
    /// Calling Socket.Shutdown method is not necessary here (and will cause throwing an exception)
    /// as listening socket on server site is not connected
    /// (Socket.Connected property value is set to 'false') - it only listens for new connections and accepts them.
    /// For each connection new separate (and connected) socket is created to handle it individually.
    /// </remarks>
    public void Dispose()
    {
        ShallAcceptConnections = false;

        _connectionHandlers.ForEach(connectionHandler => connectionHandler.Dispose());

        _listeningSocket.Close();                         // Calls Socket.Dispose() internally.
    }
    #endregion
}
