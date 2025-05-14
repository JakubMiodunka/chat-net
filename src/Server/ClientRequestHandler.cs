using CommonUtilities.Models.Requests;
namespace Server;

public sealed class ClientRequestHandler
{
    #region Delegates
    public delegate void SentResponse(int connectionIdentifier, byte[] responseData);
    public delegate void CloseConnection(int connectionIdentifier);
    #endregion

    #region Properties
    // Value = user name, Key - identifier of connection, which is used by user, all users mentioned here are authenticated.
    private readonly Dictionary<int, string> _connectionMapping;
    private readonly List<int> _unauthenticatedConnections;

    public event SentResponse? SentResponseEvent;       // Request to sent provided data to specified connection.
    public event CloseConnection? CloseConnectionEvent; // Request to close specified connection.
    #endregion

    #region Instantiation
    public ClientRequestHandler()
    {
        _connectionMapping = new Dictionary<int, string>();
        _unauthenticatedConnections = new List<int>();
    }
    #endregion

    #region Interactions
    public void ProcessNewConnection(int connectionIdentifier)
    {
        _unauthenticatedConnections.Add(connectionIdentifier);
    }
    
    private AuthenticateRespond ProcessAuthenticateRequest(int connectionIdentifier, AuthenticateRequest request)
    {
        // TODO: Check password hash before authentication.
        
        _unauthenticatedConnections.Remove(connectionIdentifier);

        _connectionMapping.Add(connectionIdentifier, request.UserName);

        return new AuthenticateRespond(IsAccessGranted: true);
    }

    private PutMessagesRespond ProcessGetMessagesRequest(int connectionIdentifier, GetMessagesRequest request)
    {
        throw new NotImplementedException();
    }

    public void ProcessRequest(int connectionIdentifier, IEnumerable<byte> requestData)
    {
        throw new NotImplementedException();
    }

    public void ProcessClosedConnection(int connectionIdentifier)
    {
        _unauthenticatedConnections.Remove(connectionIdentifier);
        _connectionMapping.Remove(connectionIdentifier);
    }
    #endregion
}
