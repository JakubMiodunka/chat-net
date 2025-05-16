using CommonUtilities.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;


namespace Server;

/// <summary>
/// SQL database client capable to read and update data stored in database.
/// </summary>
/// <remarks>
/// Intended to be used with Microsoft SQL Server.
/// </remarks>
public sealed class DatabaseClient
{
    #region Properties
    private readonly string _connectionString;
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates new instance of database client.
    /// </summary>
    /// <param name="connectionString">
    /// Connection string, which shall be used to connect to database.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    public DatabaseClient(string connectionString)
    {
        #region Arguments validation
        if (connectionString is null)
        {
            string argumentName = nameof(connectionString);
            const string ErrorMessage = "Provided connection string is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        _connectionString = connectionString;
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Opens new connection to database.
    /// </summary>
    /// <remarks>
    /// Keep in mind, that returned connection shall be explicitly disposed.
    /// </remarks>
    /// <returns>
    /// Opened connection to database.
    /// </returns>
    private IDbConnection OpenConnection()
    {
        var sqliteCOnnection = new SqlConnection(_connectionString);
        sqliteCOnnection.Open();

        return sqliteCOnnection;
    }

    /// <summary>
    /// Reads password hash of specified user from database.
    /// </summary>
    /// <param name="userId">
    /// Identifier of user, whose password hash shall be read.
    /// </param>
    /// <param name="timeout">
    /// Operation timeout expressed in seconds.
    /// Null value disables timeout restrictions.
    /// </param>
    /// <returns>
    /// Password hash of specified user.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown, when execution of requested operation fails.
    /// </exception>
    public string GetPasswordHashFor(int userId, int? timeout = 1)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@user_id", userId);

        var command = new CommandDefinition(
            commandType: CommandType.StoredProcedure,
            commandText: "sp_get_password_hash",
            commandTimeout: timeout,
            parameters: parameters);

        string? passwordHash;

        using (IDbConnection connection = OpenConnection())
        {
            passwordHash = connection.ExecuteScalar<string>(command);
        }

        if (passwordHash is null)
        {
            string errorMessage = $"Unable to get password hash for specified user: {userId}";
            throw new InvalidOperationException(errorMessage);
        }

        return passwordHash;
    }

    /// <summary>
    /// Reads details about specified user from database.
    /// </summary>
    /// <param name="userId">
    /// Identifier of user, whose details shall be read.
    /// </param>
    /// <param name="timeout">
    /// Operation timeout expressed in seconds.
    /// Null value disables timeout restrictions.
    /// </param>
    /// <returns>
    /// Details about specified user.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown, when execution of requested operation fails.
    /// </exception>
    public User GetUser(int userId, int? timeout = 1)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@user_id", userId);

        var command = new CommandDefinition(
            commandType: CommandType.StoredProcedure,
            commandText: "sp_get_user",
            commandTimeout: timeout,
            parameters: parameters);

        using (IDbConnection connection = OpenConnection())
        {
            try
            {
                return connection.QuerySingle<User>(command);
            }
            catch (InvalidOperationException exception)
            {
                string errorMessage = $"Unable to get details about user with specified identifier: {userId}";
                throw new InvalidOperationException(errorMessage, exception);
            }
        }
    }

    /// <summary>
    /// Reads details about messages sent in specified time frame to specified user.
    /// </summary>
    /// <param name="receiverId">
    /// Identifier of user, which shall be a receiver of searched messages.
    /// </param>
    /// <param name="startTimestamp">
    /// Time frame start timestamp.
    /// </param>
    /// <param name="endTimestamp">
    /// Time frame end timestamp.
    /// </param>
    /// <param name="timeout">
    /// Operation timeout expressed in seconds.
    /// Null value disables timeout restrictions.
    /// </param>
    /// <returns>
    /// Details about messages sent in specified time frame to specified user.
    /// </returns>
    public Message[] GetMessagesSentTo(int receiverId, DateTime startTimestamp, DateTime endTimestamp, int? timeout = 1)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@receiver_id", receiverId);
        parameters.Add("@start_timestamp", startTimestamp);
        parameters.Add("@end_timestamp", endTimestamp);

        var command = new CommandDefinition(
            commandType: CommandType.StoredProcedure,
            commandText: "sp_get_messages_sent_to",
            commandTimeout: timeout,
            parameters: parameters);

        using (IDbConnection connection = OpenConnection())
        {
            return connection.Query<Message>(command).ToArray();
        }
    }

    /// <summary>
    /// Reads details about messages sent in specified time frame by specified user.
    /// </summary>
    /// <param name="senderId">
    /// Identifier of user, which shall be a sender of searched messages.
    /// </param>
    /// <param name="startTimestamp">
    /// Time frame start timestamp.
    /// </param>
    /// <param name="endTimestamp">
    /// Time frame end timestamp.
    /// </param>
    /// <param name="timeout">
    /// Operation timeout expressed in seconds.
    /// Null value disables timeout restrictions.
    /// </param>
    /// <returns>
    /// Details about messages sent in specified time frame by specified user.
    /// </returns>
    public Message[] GetMessagesSentBy(int senderId, DateTime startTimestamp, DateTime endTimestamp, int? timeout = 1)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@sender_id", senderId);
        parameters.Add("@start_timestamp", startTimestamp);
        parameters.Add("@end_timestamp", endTimestamp);

        var command = new CommandDefinition(
            commandType: CommandType.StoredProcedure,
            commandText: "sp_get_messages_sent_by",
            commandTimeout: timeout,
            parameters: parameters);

        using (IDbConnection connection = OpenConnection())
        {
            return connection.Query<Message>(command).ToArray();
        }
    }

    /// <summary>
    /// Saves provided message in database.
    /// </summary>
    /// <param name="message">
    /// Message, which shall be saved to database.
    /// </param>
    /// <param name="timeout">
    /// Operation timeout expressed in seconds.
    /// Null value disables timeout restrictions.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown, when execution of requested operation fails.
    /// </exception>
    public void PutMessage(Message message, int? timeout = 1)
    {
        #region Arguments validation
        if (message is null)
        {
            string argumentName = nameof(message);
            const string ErrorMessage = "Provided message is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        var parameters = new DynamicParameters();
        parameters.Add("@timestamp", message.Timestamp);
        parameters.Add("@sender_id", message.SenderId);
        parameters.Add("@receiver_id", message.ReceiverId);
        parameters.Add("@content", message.Content);

        var command = new CommandDefinition(
            commandType: CommandType.StoredProcedure,
            commandText: "sp_add_message",
            commandTimeout: timeout,
            parameters: parameters);

        int affectedRows = 0;

        using (IDbConnection connection = OpenConnection())
        {
            affectedRows = connection.Execute(command);
        }

        if (affectedRows != 1)
        {
            string errorMessage = $"Failed to add provided message to database: {affectedRows}";
            throw new InvalidOperationException(errorMessage);
        }
    }
    #endregion
}
