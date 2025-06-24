using CommonUtilities.Models;


namespace Server.Repositories;

/// <summary>
/// Interface of repository containing details about application users.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Provides details about specified pool of application user.
    /// </summary>
    /// <param name="userIdentifiers">
    /// Unique identifiers of users, whose details are requested to be obtained.
    /// </param>
    /// <returns>
    /// Details about specified pool of users.
    /// </returns>
    public User[] GetUsers(IEnumerable<int> userIdentifiers);

    /// <summary>
    /// Provides hash of password related to account of specified user.
    /// </summary>
    /// <param name="userIdentifier">
    /// Unique identifier of user, whose password hash are requested to be obtained.
    /// </param>
    /// <returns>
    /// Hash of password related to account of specified user.
    /// If details about account related to specified user are not present in repository null reference will be returned.
    /// </returns>
    public string? GetAccountPasswordHash(int userIdentifier);
}
