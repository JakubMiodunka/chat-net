using CommonUtilities;
using CommonUtilities.Models;
using CommonUtilities.Requests.Models;
using Server.Repositories;

namespace Server;

// Below class is only a draft
public sealed class RequestHandler : TasksManager
{
    #region Delegates
    public delegate void SendRequest(int connectionIdentifier, Request request);
    public delegate void CloseConnection(int connectionIdentifier);
    #endregion

    #region Events
    public event SendRequest? SendRequestEvent;               // Invoked, when handler wants to sent some data to specified connection.
    public event CloseConnection? CloseConnectionEvent; // Invoked, when handler wants to close specified connection.
    #endregion

    #region Properties
    private readonly DatabaseClient _databaseClient;
    private readonly Dictionary<int, User> _authenticatedUsers;
    #endregion

    #region Instantiation
    public RequestHandler(DatabaseClient databaseClient) : base()
    {
        #region Arguments validation
        if (databaseClient is null)
        {
            string argumentName = nameof(databaseClient);
            const string ErrorMessage = "Provided database client is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        _databaseClient = databaseClient;
        _authenticatedUsers = new Dictionary<int, User>();
    }
    #endregion

    #region Client requests handling
    private PutAuthenticationRequest HandleGetAuthenticationRequest(int connectionId, GetAuthenticationRequest request)
    {
        throw new NotImplementedException();

        #region Arguments validation
        if (request is null)
        {
            string argumentName = nameof(request);
            const string ErrorMessage = "Provided request is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        // User user = _databaseClient.GetUser(request.UserId);
        // 
        // lock (_authenticatedUsers)
        // {
        //     _authenticatedUsers.Add(connectionId, user);
        // }
        // 
        // // TODO: Add password heck as it is not checked currently.
        // return new PutAuthenticationRequest(true);
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

        Message[] messages = _databaseClient.GetMessagesSentTo(requester.Identifier, request.StartTimestamp, request.EndTimestamp);

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

        throw new NotImplementedException();

        // User[] users = request.UserIdentifiers
        //     .Select(identifier => _databaseClient.GetUser(identifier))
        //     .ToArray();
        // 
        // return new PutUsersRequest(users);
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

        var message = new Message(DateTime.Now, requester.Identifier, request.ReceiverIdentifier, request.MessageContent);

        _databaseClient.PutMessage(message);
    }

    private void HandleRequest(int connectionIdentifier, Request request)
    {
        #region Arguments validation
        if (request is null)
        {
            string argumentName = nameof(request);
            const string ErrorMessage = "Provided request is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        if (request is GetAuthenticationRequest)
        {
            Request resposne = HandleGetAuthenticationRequest(connectionIdentifier, (GetAuthenticationRequest)request);
            SendRequestEvent?.Invoke(connectionIdentifier, resposne);
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
                CloseConnectionEvent?.Invoke(connectionIdentifier);
                return;
            }
        }

        if (request is GetMessagesRequest)
        {
            Request resposne = HandleGetMessagesRequest(requester, (GetMessagesRequest)request);
            SendRequestEvent?.Invoke(connectionIdentifier, resposne);
            return;
        }

        if (request is GetUsersRequest)
        {
            Request resposne = HandleGetUsersRequest(requester, (GetUsersRequest)request);
            SendRequestEvent?.Invoke(connectionIdentifier, resposne);
            return;
        }

        if (request is PutTextMessageRequest)
        {
            HandlePutTextMessageRequest(requester, (PutTextMessageRequest)request);
            return;
        }

        CloseConnectionEvent?.Invoke(connectionIdentifier);
    }
    #endregion

    // Methods form this region shall be attached to events raised by the server socket.
    #region Notifications from server site
    public void ScheduleRequestHandling(int connectionIdentifier, Request request)
    {
        Task task = Task.Run(() => HandleRequest(connectionIdentifier, request));
        AddTask(task);
    }
    
    // Beware, that if handler raised CloseConnectionEvent it expects confirmation,
    // that connection was truly closed on server site via this method.
    public void ConnectionClosed(int connectionIdentifier)
    {
        lock (_authenticatedUsers)
        {
            _authenticatedUsers.Remove(connectionIdentifier);
        }
    }
    #endregion
}
