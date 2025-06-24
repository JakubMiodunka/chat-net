using CommonUtilities.Models;
using Server.Repositories;


namespace Server;

// TODO: Add unit tests.
/// <summary>
/// Instance of this class is responsible for authenticating of incoming connections
/// and tracking relations between established connection and users associated with them.
/// </summary>
public sealed class ConnectionAuthenticator
{
    #region Properties
    // Key: Unique connection identifier, Value: Details about user, who is associated with this connection.
    private readonly Dictionary<int, User> _connectionsToUsersMapping;
    private readonly IUserRepository _userRepository;
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates new authenticator instance.
    /// </summary>
    /// <param name="userRepository">
    /// Repository of users, form which authenticator shall obtain details about application users.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    public ConnectionAuthenticator(IUserRepository userRepository)
    {
        #region Arguments validation
        if (userRepository is null)
        {
            string argumentName = nameof(userRepository);
            const string ErrorMessage = "Provided user repository is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        _userRepository = userRepository;
        _connectionsToUsersMapping = new Dictionary<int, User>();
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Authenticates connection and associates it with application user.
    /// </summary>
    /// <param name="connectionIdentifier">
    /// Unique identifier of connection, which user requests to be authenticated.
    /// </param>
    /// <param name="userIdentifier">
    /// Unique identifier of user, who requests to be authenticated.
    /// </param>
    /// <param name="passwordHash">
    /// Hash of password related to account of user, who requests to be authenticated.
    /// </param>
    /// <returns>
    /// True if authentication was successful, false otherwise.
    /// </returns>
    /// /// <exception cref="ArgumentException">
    /// Thrown, when at least one argument will be considered as invalid.
    /// </exception>
    /// <exception cref="DataMisalignedException">
    /// Thrown, when informations provided by used user repository are incomplete or not consistent.
    /// </exception>
    public bool AuthenticateConnection(int connectionIdentifier, int userIdentifier, string passwordHash)
    {
        throw new NotImplementedException();
        //#region Arguments validation
        //if (_connectionsToUsersMapping.ContainsKey(connectionIdentifier))
        //{
        //    string argumentName = nameof(connectionIdentifier);
        //    string errorMessage = $"Connection with specified identifier is already authenticated: {connectionIdentifier}";
        //    throw new ArgumentException(argumentName, errorMessage);
        //}
        //#endregion
        //
        //string? expectedPasswordHash = _userRepository.GetAccountPasswordHash(userIdentifier);
        //
        //bool isAccessGranted = expectedPasswordHash is null ? false : passwordHash == expectedPasswordHash;
        //
        //if (isAccessGranted)
        //{
        //    User? userDetails = _userRepository.GetUser(userIdentifier);
        //
        //    if (userDetails is null)
        //    {
        //        string errorMessage = $"Incomplete data related to specified user detected in repository: {userIdentifier}";
        //        throw new DataMisalignedException(errorMessage);
        //    }
        //
        //    _connectionsToUsersMapping.Add(connectionIdentifier, userDetails);
        //}
        //
        //return isAccessGranted;
    }

    /// <summary>
    /// Provides details about already authenticated user, who is associated with specified connection.
    /// </summary>
    /// <param name="connectionIdentifier">
    /// Unique identifier of connection, which user details shall be returned.
    /// </param>
    /// <returns>
    /// Details about user associated with specified connection.
    /// If user of specified connection is not authenticated null reference will be returned.
    /// </returns>
    public User? GetUserAssociatedWithConnection(int connectionIdentifier)
    {
        try
        {
            return _connectionsToUsersMapping[connectionIdentifier];
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    /// <summary>
    /// Specifies connection, which details shall no longer be tracked by authenticator.
    /// </summary>
    /// <param name="connectionIdentifier">
    /// Unique identifier of connection, which details shall no longer be tracked.
    /// </param>
    public void DeleteConnectionDetails(int connectionIdentifier)
    {
        _connectionsToUsersMapping.Remove(connectionIdentifier);
    }
    #endregion
}
