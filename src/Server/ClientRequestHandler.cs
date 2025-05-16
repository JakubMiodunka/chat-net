using CommonUtilities.Models;
using CommonUtilities.Models.Requests;
namespace Server;

// For now this class is only a raw, incomplete draft.
public sealed class ClientRequestHandler
{
    #region Delegates
    public delegate void SentResponse(int connectionIdentifier, byte[] responseData);
    public delegate void CloseConnection(int connectionIdentifier);
    #endregion

    #region Properties
    // Value = user identifier, Key - identifier of connection, which is used by user, all users mentioned here are authenticated.
    private readonly Dictionary<int, int> _connectionMapping;
    private readonly List<int> _unauthenticatedConnections;
    private readonly DatabaseClient _databaseClient;

    public event SentResponse? SentResponseEvent;       // Request to sent provided data to specified connection.
    public event CloseConnection? CloseConnectionEvent; // Request to close specified connection.
    #endregion

    #region Instantiation
    public ClientRequestHandler(DatabaseClient databaseClient)
    {
        #region Arguments validation
        if (databaseClient is null)
        {
            string argumentName = nameof(databaseClient);
            const string ErrorMessage = "Provided database client is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        _connectionMapping = new Dictionary<int, int>();
        _unauthenticatedConnections = new List<int>();
        _databaseClient = databaseClient;
    }
    #endregion

    #region Interactions
    public void ProcessNewConnection(int connectionIdentifier)
    {
        _unauthenticatedConnections.Add(connectionIdentifier);
    }
    
    private AuthenticateRespond ProcessAuthenticateRequest(int connectionIdentifier, AuthenticateRequest request)
    {
        #region Arguments validation
        if (request is null)
        {
            string argumentName = nameof(request);
            const string ErrorMessage = "Provided request is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        string expectedPasswordHash = _databaseClient.GetPasswordHashFor(request.UserId);
        string actualPasswordHash = request.PasswordHash;

        bool isAccessGranted = expectedPasswordHash == actualPasswordHash;

        if (isAccessGranted)
        {
            _unauthenticatedConnections.Remove(connectionIdentifier);
            _connectionMapping.Add(connectionIdentifier, request.UserId);
        }
        else
        {
            //TODO: Disconnect if access not granted.
        }

        return new AuthenticateRespond(isAccessGranted);
    }

    private PutMessagesRequest ProcessGetMessagesRequest(int connectionIdentifier, GetMessagesRequest request)
    {
        #region Arguments validation
        if (request is null)
        {
            string argumentName = nameof(request);
            const string ErrorMessage = "Provided request is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        if (!_connectionMapping.ContainsKey(connectionIdentifier))
        {
            //TODO: Disconnect if user unauthenticated.
        }

        int userId = _connectionMapping[connectionIdentifier];
        Message[] messages = _databaseClient.GetMessagesSentBy(userId, request.StartTimestamp, request.EndTimestamp);

        return new PutMessagesRequest(messages);
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
