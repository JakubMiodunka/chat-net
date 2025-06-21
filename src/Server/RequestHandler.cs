using CommonUtilities;
using CommonUtilities.Models;
using CommonUtilities.Requests;
using CommonUtilities.Requests.Models;
using Server.Sockets;

namespace Server;

// Below class is only a draft
public sealed class RequestHandler : TasksManager
{
    #region Properties
    private readonly ServerTcpSocket _server;
    private readonly DatabaseClient _databaseClient;

    private readonly Dictionary<int, User> _authenticatedUsers;
    #endregion

    #region Instantiation
    public RequestHandler(ServerTcpSocket server, DatabaseClient databaseClient) : base()
    {
        #region Arguments validation
        if (server is null)
        {
            string argumentName = nameof(server);
            const string ErrorMessage = "Provided server is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (databaseClient is null)
        {
            string argumentName = nameof(databaseClient);
            const string ErrorMessage = "Provided database client is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        _server = server;
        _databaseClient = databaseClient;
        _authenticatedUsers = new Dictionary<int, User>();

        _server.DataReceivedEvent += HandleReceivedData;
    }
    #endregion

    // Currently password hash is not checked.
    private PutAuthenticationRequest HandleGetAuthenticationRequest(int connectionId, GetAuthenticationRequest request)
    {
        #region Arguments validation
        if (request is null)
        {
            string argumentName = nameof(request);
            const string ErrorMessage = "Provided request is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        User user = _databaseClient.GetUser(request.UserId);

        lock(_authenticatedUsers)
        {
            _authenticatedUsers.Add(connectionId, user);
        }

        return new PutAuthenticationRequest(true);
    }
    private PutMessagesRequest HandleGetMessagesRequest(User requester, GetMessagesRequest request)
    {
        #region Arguments validation
        if (requester is null)
        {
            string argumentName = nameof(requester);
            const string ErrorMessage = "Provided requester is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (request is null)
        {
            string argumentName = nameof(request);
            const string ErrorMessage = "Provided request is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        Message[] messages = _databaseClient.GetMessagesSentTo(requester.Id, request.StartTimestamp, request.EndTimestamp);

        return new PutMessagesRequest(messages);
    }
    private PutUsersRequest HandleGetUsersRequest(User requester, GetUsersRequest request)
    {
        #region Arguments validation
        if (requester is null)
        {
            string argumentName = nameof(requester);
            const string ErrorMessage = "Provided requester is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (request is null)
        {
            string argumentName = nameof(request);
            const string ErrorMessage = "Provided request is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        User[] users = request.UserIdentifiers
            .Select(identifier => _databaseClient.GetUser(identifier))
            .ToArray();

        return new PutUsersRequest(users);
    }
    private void HandlePutTextMessageRequest(User requester, PutTextMessageRequest request)
    {
        #region Arguments validation
        if (requester is null)
        {
            string argumentName = nameof(requester);
            const string ErrorMessage = "Provided requester is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (request is null)
        {
            string argumentName = nameof(request);
            const string ErrorMessage = "Provided request is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        //TODO: Do sortieing more elegant with message ID here.
        var message = new Message(default, DateTime.Now, requester.Id, request.ReceiverIdentifier, request.MessageContent);

        _databaseClient.PutMessage(message);
    }

    private async Task HandleReceicvedDataTask(int connectionIdentifier, byte[] receivedData)
    {
        Request request;

        try
        {
            request = RequestSerializer.Deserialize(receivedData);
        }
        catch(NotSupportedException)
        {
            CloseConnection(connectionIdentifier);
            return;
        }

        if (request is GetAuthenticationRequest)
        {
            Request resposne = HandleGetAuthenticationRequest(connectionIdentifier, (GetAuthenticationRequest)request);
            await SentRequest(connectionIdentifier, resposne);
            return;
        }

        User requester;

        lock (_authenticatedUsers)
        {
            try
            {
                requester = _authenticatedUsers[connectionIdentifier];
            }
            catch (KeyNotFoundException)
            {
                CloseConnection(connectionIdentifier);
                return;
            }
        }

        if (request is GetMessagesRequest)
        {
            Request resposne = HandleGetMessagesRequest(requester, (GetMessagesRequest)request);
            await SentRequest(connectionIdentifier, resposne);
            return;
        }

        if (request is GetUsersRequest)
        {
            Request resposne = HandleGetUsersRequest(requester, (GetUsersRequest)request);
            await SentRequest(connectionIdentifier, resposne);
            return;
        }

        if (request is PutTextMessageRequest)
        {
            HandlePutTextMessageRequest(requester, (PutTextMessageRequest)request);
            return;
        }

        CloseConnection(connectionIdentifier);
        return;
    }

    private void HandleReceivedData(int connectionIdentifier, byte[] receivedData)
    {
        Task task = Task.Run(() => HandleReceicvedDataTask(connectionIdentifier, receivedData));
        AddTask(task);
    }

    private async Task SentRequest(int connectionIdentifier, Request request)
    {
        byte[] serializedRequest = RequestSerializer.Serialize(request);
        await _server.SentData(connectionIdentifier, serializedRequest);
    }

    private void CloseConnection(int connectionIdentifier)
    {
        lock(_authenticatedUsers)
        {
            _authenticatedUsers.Remove(connectionIdentifier);
        }

        _server.CloseConnection(connectionIdentifier);
    }
}
