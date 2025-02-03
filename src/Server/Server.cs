// Ignore Spelling: ip

using CommonUtilities.Ciphers;
using CommonUtilities.Protocols;
using System.Net;
using System.Net.Sockets;


namespace Server;

public sealed class Server
{
    #region Properties
    private readonly Socket _listeningSocket;
    private readonly IProtocol _protocol;
    private readonly ICipher _cipher;

    public readonly int ActiveConnectionsLimit;
    #endregion

    #region Instantiation
    public Server(IPEndPoint ipEndPoint, int activeConnectionsLimit, IProtocol protocol, ICipher cipher)
    {
        #region Arguments validation
        if (ipEndPoint is null)
        {
            string argumentName = nameof(ipEndPoint);
            const string ErrorMessage = "Provided IP end point is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (activeConnectionsLimit < 1)
        {
            string argumentName = nameof(activeConnectionsLimit);
            string errorMessage = $"Specified active connections limit is too small: {activeConnectionsLimit}";
            throw new ArgumentOutOfRangeException(argumentName, activeConnectionsLimit, errorMessage);
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
        _protocol = protocol;
        _cipher = cipher;

        ActiveConnectionsLimit = activeConnectionsLimit;

        _listeningSocket.Bind(ipEndPoint);
    }
    #endregion

    #region Interactions
    private void HandleConnection(Socket connetcionSocket)
    {
        #region Arguments validation
        if (connetcionSocket is null)
        {
            string argumentName = nameof(connetcionSocket);
            const string ErrorMessage = "Provided connection socket a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        // TODO: Implement.
        throw new NotImplementedException();
    }

    public async Task StartListeningForConnections()
    {
        _listeningSocket.Listen(ActiveConnectionsLimit);

        while (true)
        {
            Socket connectionSocket = await _listeningSocket.AcceptAsync();
            HandleConnection(connectionSocket);
        }
    }
    #endregion
}
