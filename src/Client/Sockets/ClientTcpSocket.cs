﻿using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using CommonUtilities.Sockets;
using System.Net;
using System.Net.Sockets;


namespace Client.Sockets;

/// <summary>
/// Socket wrapper, which serves as a client in TCP client-server architecture.
/// Capable to transfer encrypted data in full-duplex manner.
/// </summary>
/// <remarks>
/// To get it to operational state properly, first instantiate the class member, then assign events handlers
/// and finally call ConnectToServer() method to start data transfer.
/// Do not forget to dispose created instance, when it will be no longer needed.
/// </remarks>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.connect?view=net-9.0"/>
public sealed class ClientTcpSocket : FullDuplexTcpSocket
{
    #region Delegates
    public delegate void DataReceivedDelegate(byte[] receivedData);
    #endregion

    #region Properties
    private readonly IPEndPoint _serverEndPoint;
    private Task? _listeningForDataTask;
    private readonly CancellationTokenSource _cancellationTokenSourceForDataListening;

    protected override Socket Socket { get; }

    public event DataReceivedDelegate? DataReceivedEvent;
    public event Action? ConnectionClosedEvent;
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
        _listeningForDataTask = null;
        _cancellationTokenSourceForDataListening = new CancellationTokenSource();

        Socket = new Socket(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Processes patches of data received from server site.
    /// </summary>
    /// <param name="receivedData">
    /// Chunk of data received from server site.
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

        DataReceivedEvent?.Invoke(receivedData.ToArray());
    }

    /// <summary>
    /// Reacts to situation, when server will close the connection.
    /// </summary>
    protected override void RaiseConnectionClosedEvent()
    {
        ConnectionClosedEvent?.Invoke();
    }

    /// <summary>
    /// Connects client TCP socket to server.
    /// </summary>
    /// <remarks>
    /// Server end point shall be specified during instance initialization.
    /// Client TCP socket will start listening for incoming data immediately, after connection will be established.
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

        Socket.Connect(_serverEndPoint);    // Throws SocketException when connection will fail.
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
    }
    #endregion
}
