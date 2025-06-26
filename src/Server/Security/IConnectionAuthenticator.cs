using CommonUtilities.Models;


namespace Server.Security;

/// <summary>
/// Implementation of this interface is able to authenticate incoming connections
/// and track relations between established connection and users associated with them.
/// </summary>
public interface IConnectionAuthenticator
{
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
    bool AuthenticateConnection(int connectionIdentifier, int userIdentifier, string passwordHash);

    /// <summary>
    /// Provides details about already authenticated user, who is associated with specified connection.
    /// </summary>
    /// <param name="connectionIdentifier">
    /// Unique identifier of connection, which user details shall be returned.
    /// </param>
    /// <returns>
    /// Details about user associated with specified connection.
    /// If user of specified connection is not authenticated null reference shall be returned.
    /// </returns>
    User? GetUserAssociatedWithConnection(int connectionIdentifier);

    /// <summary>
    /// Makes specified connection no longer authenticated.
    /// </summary>
    /// <param name="connectionIdentifier">
    /// Unique identifier of connection, which authentication shall be discontinued.
    /// </param>
    void DeauthenticateConnection(int connectionIdentifier);
}
