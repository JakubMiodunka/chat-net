// Ignore Spelling: ip

using CommonUtilities;
using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using System.Net;
using System.Net.Sockets;


namespace Server.Sockets;

/// <summary>
/// Socket wrapper, which serves as a server in TCP client-server architecture.
/// </summary>
/// <remarks>
/// To get it to operational state properly, first instantiate the class member, then assign events handlers
/// and finally call StartAcceptingConnections() method to start accepting new clients.
/// Do not forget to dispose created instance, when it will be no longer needed.
/// </remarks>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.bind?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.listen?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.acceptasync?view=net-9.0"/>
public sealed class ServerTcpSocket : TasksManager
{
    #region Delegates
    public delegate void ConnectionAcceptedDelegate(int connectionIdentifier);
    public delegate void DataReceivedDelegate(int connectionIdentifier, byte[] receivedData);
    public delegate void ConnectionClosedDelegate(int connectionIdentifier);
    #endregion

    #region Properties
    private readonly Socket _listeningSocket;
    private readonly int _receivingBufferSize;
    private readonly IProtocol _protocol;
    private readonly ICipher _cipher;
    private readonly List<TcpConnectionHandler> _connectionHandlers;
    private Task? _acceptingConnectionsTask;
    private readonly CancellationTokenSource _cancellationTokenSourceForAcceptingConnections;

    public event ConnectionAcceptedDelegate? ConnectionAcceptedEvent;
    public event DataReceivedDelegate? DataReceivedEvent;
    public event ConnectionClosedDelegate? ConnectionClosedEvent;
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
        _acceptingConnectionsTask = null;
        _cancellationTokenSourceForAcceptingConnections = new CancellationTokenSource();

        _listeningSocket.Bind(ipEndPoint);
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Creates new handler for newly accepted connection and processes the event.
    /// </summary>
    /// <remarks>
    /// Additionally whenever this method is being invoked, inactive connection handlers are disposed.
    /// It is simple yet effective mechanism of lazy-management of the pool.
    /// </remarks>
    /// <param name="connectionSocket">
    /// Socket referring to newly accepted connection.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    private void RaiseConnectionAcceptedEvent(Socket connectionSocket)
    {
        #region Arguments validation
        if (connectionSocket is null)
        {
            string argumentName = nameof(connectionSocket);
            const string ErrorMessage = "Provided client socket a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        var handler = new TcpConnectionHandler(connectionSocket, _receivingBufferSize, _protocol, _cipher);
        handler.DataReceivedEvent = (connectionIdentifier, receivedData) => DataReceivedEvent?.Invoke(connectionIdentifier, receivedData);
        handler.ConnectionClosedEvent = (connectionIdentifier) => ConnectionClosedEvent?.Invoke(connectionIdentifier);
        
        lock(_connectionHandlers)
        {
            List<TcpConnectionHandler> inactiveHandlers = _connectionHandlers.Where(handler => handler.IsActive).ToList();
            inactiveHandlers.ForEach(handler => handler.Dispose());
            inactiveHandlers.ForEach(handler => _connectionHandlers.Remove(handler));
            
            _connectionHandlers.Add(handler);
        }

        handler.StartListeningForData();

        ConnectionAcceptedEvent?.Invoke(handler.ConnectionIdentifier);
    }

    /// <summary>
    /// Triggers continues process of listening and accepting new connections on listening socket.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token, which shall be bound to launched task.
    /// </param>
    private async Task StartAcceptingConnections(CancellationToken cancellationToken)
    {
        _listeningSocket.Listen();

        Task<Socket>? acceptNewConnectionTask = null;

        while (_listeningSocket.IsBound && !cancellationToken.IsCancellationRequested)
        {
            acceptNewConnectionTask ??= _listeningSocket.AcceptAsync();

            if (!acceptNewConnectionTask.IsCompleted)
            {
                await Task.Delay(200);  // TODO: Maybe this time period shall be included in configuration rather than hard-coded here.
                continue;
            }

            Socket connectionSocket = acceptNewConnectionTask.Result;
            acceptNewConnectionTask = null;

            Task connectionAcceptedEventTask = Task.Run(() => RaiseConnectionAcceptedEvent(connectionSocket));
            AddTask(connectionAcceptedEventTask);
        }
    }

    /// <summary>
    /// Triggers continues process of listening and accepting new connections on listening socket.
    /// </summary>
    public void StartAcceptingConnections()
    {
        _acceptingConnectionsTask = StartAcceptingConnections(_cancellationTokenSourceForAcceptingConnections.Token);
    }

    /// <summary>
    /// Sends provided data to specified connection.
    /// </summary>
    /// <param name="connectionIdentifier">
    /// Identifier connection, to which provided data shall be sent.
    /// </param>
    /// <param name="data">
    /// Data, which shall be sent to specified connection.
    /// </param>
    /// <returns>
    /// Task related to pending data transfer.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown, when value of at least one argument will be considered as invalid.
    /// </exception>
    public async Task SentData(int connectionIdentifier, IEnumerable<byte> data)
    {
        TcpConnectionHandler handler;

        try
        {
            lock (_connectionHandlers)
            {
                handler = _connectionHandlers.First(handler => handler.ConnectionIdentifier == connectionIdentifier);
            }
        }
        catch (InvalidOperationException)
        {
            string argumentName = nameof(connectionIdentifier);
            string errorMessage = $"Connection with specified identifier not found: {connectionIdentifier}";
            throw new ArgumentOutOfRangeException(argumentName, connectionIdentifier, errorMessage);
        }

        await handler.SentData(data);
    }

    /// <summary>
    /// Closes connection with specified identifier.
    /// </summary>
    /// <param name="connectionIdentifier">
    /// Identifier of connection, which shall be closed.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown, when value of at least one argument will be considered as invalid.
    /// </exception>
    public void CloseConnection(int connectionIdentifier)
    {
        TcpConnectionHandler handler;

        try
        {
            lock (_connectionHandlers)
            {
                handler = _connectionHandlers.First(handler => handler.ConnectionIdentifier == connectionIdentifier);
            }
        }
        catch (InvalidOperationException)
        {
            string argumentName = nameof(connectionIdentifier);
            string errorMessage = $"Connection with specified identifier not found: {connectionIdentifier}";
            throw new ArgumentOutOfRangeException(argumentName, connectionIdentifier, errorMessage);
        }

        handler.Dispose();

        lock (_connectionHandlers)
        {
            _connectionHandlers.Remove(handler);
        }
    }

    /// <summary>
    /// Suppresses accepting new connections, disposes all connection handlers along with listening socket.
    /// </summary>
    /// <remarks>
    /// Calling Socket.Shutdown method is not necessary here (and will cause throwing an exception)
    /// as listening socket on server site is not connected (Socket.Connected property value is set
    /// to 'false') - it only listens for new connections and accepts them.
    /// For each connection new separate (and connected) socket is created to handle it individually.
    /// </remarks>
    public override void Dispose()
    {
        if (_acceptingConnectionsTask is not null)
        {
            _cancellationTokenSourceForAcceptingConnections.Cancel();
            _acceptingConnectionsTask.Wait();

            _acceptingConnectionsTask = null;
        }

        _listeningSocket.Close();   // Calls Socket.Dispose() internally.

        lock (_connectionHandlers)
        {
            _connectionHandlers.ForEach(handler => handler.Dispose());
            _connectionHandlers.Clear();
        }

        base.Dispose();
    }
    #endregion
}
