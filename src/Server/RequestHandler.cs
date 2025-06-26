using CommonUtilities;
using CommonUtilities.Models;
using CommonUtilities.Requests.Models;
using Server.Repositories;
using Server.Security;


namespace Server;

// TODO: Add unit tests.
/// <summary>
/// Handler for user requests received on server site.
/// </summary>
public sealed class RequestHandler : TasksManager
{
    #region Delegates
    public delegate void SendRequest(int connectionIdentifier, Request request);
    public delegate void CloseConnection(int connectionIdentifier);
    #endregion

    #region Events
    public event SendRequest? SendRequestEvent;               // Invoked, when handler wants to sent a request through connection.
    public event CloseConnection? CloseConnectionEvent;       // Invoked, when handler wants to close specified connection.
    #endregion

    #region Properties
    private readonly IConnectionAuthenticator _connectionAuthenticator;
    private readonly IUserRepository _userRepository;
    private readonly IMessagesRepository _messagesRepository;
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates new handler instance.
    /// </summary>
    /// <param name="connectionAuthenticator">
    /// Authenticator, which shall be used to authenticate incoming connections
    /// and track relations between established connection and users associated with them.
    /// </param>
    /// <param name="userRepository">
    /// Repository, which shall be used to obtain details about application users.
    /// </param>
    /// <param name="messagesRepository">
    /// Repository, which shall be used to details about messages exchanged between application users.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    public RequestHandler(IConnectionAuthenticator connectionAuthenticator, IUserRepository userRepository, IMessagesRepository messagesRepository) : base()
    {
        #region Arguments validation
        if (connectionAuthenticator is null)
        {
            string argumentName = nameof(connectionAuthenticator);
            const string ErrorMessage = "Provided connection authenticator is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (userRepository is null)
        {
            string argumentName = nameof(userRepository);
            const string ErrorMessage = "Provided user repository is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (messagesRepository is null)
        {
            string argumentName = nameof(messagesRepository);
            const string ErrorMessage = "Provided messages repository is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        _connectionAuthenticator = connectionAuthenticator;
        _userRepository = userRepository;
        _messagesRepository = messagesRepository;
    }
    #endregion

    #region Client requests handling
    /// <summary>
    /// Processes provided request and generates response to it.
    /// </summary>
    /// <param name="connectionIdentifier">
    /// Unique identifier of connection, at which provided request was received.
    /// </param>
    /// <param name="request">
    /// Request, which shall be processed.
    /// </param>
    /// <returns>
    /// Response to provided request, which shall be sent to requester.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    private PutAuthenticationRequest HandleGetAuthenticationRequest(int connectionIdentifier, GetAuthenticationRequest request)
    {
        #region Arguments validation
        if (request is null)
        {
            string argumentName = nameof(request);
            const string ErrorMessage = "Provided request is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        bool isAccessGranted = false;
        lock (_connectionAuthenticator)
        {
            isAccessGranted = _connectionAuthenticator.AuthenticateConnection(
                connectionIdentifier,
                request.UserIdentifier,
                request.PasswordHash);
        }

        return new PutAuthenticationRequest(isAccessGranted);
    }

    /// <summary>
    /// Processes provided request and generates response to it.
    /// </summary>
    /// <param name="requester">
    /// Details about application user, who sent provided request.
    /// </param>
    /// <param name="request">
    /// Request, which shall be processed.
    /// </param>
    /// <returns>
    /// Response to provided request, which shall be sent to requester.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
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

        var messages = new List<Message>();

        lock (_messagesRepository)
        {
            messages.AddRange(_messagesRepository.GetMessagesSentBy(requester.Identifier, request.StartTimestamp, request.EndTimestamp));
            messages.AddRange(_messagesRepository.GetMessagesSentTo(requester.Identifier, request.StartTimestamp, request.EndTimestamp));
        }

        return new PutMessagesRequest(messages.ToArray());
    }

    /// <summary>
    /// Processes provided request and generates response to it.
    /// </summary>
    /// <param name="requester">
    /// Details about application user, who sent provided request.
    /// </param>
    /// <param name="request">
    /// Request, which shall be processed.
    /// </param>
    /// <returns>
    /// Response to provided request, which shall be sent to requester.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
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

        var userDetails = Array.Empty<User>();

        lock (_userRepository)
        {
            userDetails = _userRepository.GetUserDetails(request.UserIdentifiers);
        }

        return new PutUsersRequest(userDetails);
    }

    /// <summary>
    /// Processes provided request and generates response to it.
    /// </summary>
    /// <param name="requester">
    /// Details about application user, who sent provided request.
    /// </param>
    /// <param name="request">
    /// Request, which shall be processed.
    /// </param>
    /// <returns>
    /// Response to provided request, which shall be sent to requester.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
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

        lock (_messagesRepository)
        {
            _messagesRepository.PutMessage(message);
        }
    }

    /// <summary>
    /// Processes provided request and generates response to it.
    /// </summary>
    /// <param name="connectionIdentifier">
    /// Unique identifier of connection, at which provided request was received.
    /// </param>
    /// <param name="request">
    /// Request, which shall be processed.
    /// </param>
    /// <returns>
    /// Response to provided request, which shall be sent to requester.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
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

        User? requester;

        lock (_connectionAuthenticator)
        {
            requester = _connectionAuthenticator.GetUserAssociatedWithConnection(connectionIdentifier);
        }

        if (requester is null)   // Means that connection is not authenticated.
        {
            if (request is GetAuthenticationRequest)
            {
                PutAuthenticationRequest response = HandleGetAuthenticationRequest(connectionIdentifier, (GetAuthenticationRequest)request);
                SendRequestEvent?.Invoke(connectionIdentifier, response);
                return;
            }

            // If attempt of sending request other that GetAuthenticationRequest through
            // not authenticated connection will be detected, it would be treated as security violation.
            CloseConnectionEvent?.Invoke(connectionIdentifier);
            return;
        }

        if (request is GetMessagesRequest)
        {
            PutMessagesRequest resposne = HandleGetMessagesRequest(requester, (GetMessagesRequest)request);
            SendRequestEvent?.Invoke(connectionIdentifier, resposne);
            return;
        }

        if (request is GetUsersRequest)
        {
            PutUsersRequest resposne = HandleGetUsersRequest(requester, (GetUsersRequest)request);
            SendRequestEvent?.Invoke(connectionIdentifier, resposne);
            return;
        }

        if (request is PutTextMessageRequest)
        {
            HandlePutTextMessageRequest(requester, (PutTextMessageRequest)request);
            return;
        }

        // If requests sent by requester is not recognized, it would be treated as security violation.
        lock (_connectionAuthenticator)
        {
            _connectionAuthenticator.DeauthenticateConnection(connectionIdentifier);
        }

        CloseConnectionEvent?.Invoke(connectionIdentifier);
    }
    #endregion

    // Methods form this region shall be attached to events raised by the server socket.
    #region Notifications
    /// <summary>
    /// Notifies handler about receiving new request, and schedules request processing task.
    /// </summary>
    /// <param name="connectionIdentifier">
    /// Unique identifier of connection, at which new request was received.
    /// </param>
    /// <param name="request">
    /// Newly received request, which shall be processed.
    /// </param>
    public void ReceivedNewRequest(int connectionIdentifier, Request request)
    {
        Task requestHandlingTask = Task.Run(() => HandleRequest(connectionIdentifier, request));
        AddTask(requestHandlingTask);
    }

    /// <summary>
    /// Notifies handler about closing of specified connection.
    /// </summary>
    /// <remarks>
    /// Beware, that if handler raised CloseConnectionEvent, it expects the confirmation,
    /// that connection was truly closed on server site via this method.
    /// </remarks>
    /// <param name="connectionIdentifier">
    /// Unique identifier of connection, which was closed.
    /// </param>
    public void ConnectionClosed(int connectionIdentifier)
    {
        lock (_connectionAuthenticator)
        {
            _connectionAuthenticator.DeauthenticateConnection(connectionIdentifier);
        }
    }
    #endregion
}
