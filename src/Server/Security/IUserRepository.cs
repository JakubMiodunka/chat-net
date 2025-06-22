using CommonUtilities.Models;


namespace Server.Security;

/// <summary>
/// Provides access to details about application users.
/// </summary>
public interface IUserRepository
{
    // TODO: What if user with specified identifier does not exists in repository?
    /// <summary>
    /// Provides details about specified application user.
    /// </summary>
    /// <param name="userIdentifier">
    /// Unique identifier of user, whose details are requested to be obtained.
    /// </param>
    /// <returns>
    /// Details about specified user.
    /// </returns>
    public User GetUser(int userIdentifier);

    // TODO: What if user with specified identifier does not exists in repository?
    /// <summary>
    /// Provides hash of password related to account of specified user.
    /// </summary>
    /// <param name="userIdentifier">
    /// Unique identifier of user, whose password hash are requested to be obtained.
    /// </param>
    /// <returns>
    /// Hash of password related to account of specified user.
    /// </returns>
    public string GetUserPasswordHash(int userIdentifier);
}
