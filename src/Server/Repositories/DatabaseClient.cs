// Ignore Spelling: Timestamp

using CommonUtilities.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;


namespace Server.Repositories;

/// <summary>
/// SQL database client capable to read and update data stored in database.
/// </summary>
/// <remarks>
/// Intended to be used with Microsoft SQL Server.
/// </remarks>
public sealed class DatabaseClient : IUserRepository, IMessagesRepository
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
    private SqlConnection OpenConnection()
    {
        var sqliteCOnnection = new SqlConnection(_connectionString);
        sqliteCOnnection.Open();

        return sqliteCOnnection;
    }

    /// <summary>
    /// Reads password hash of specified user from database.
    /// </summary>
    /// <param name="userIdentifier">
    /// Identifier of user, whose password hash shall be read.
    /// </param>
    /// <param name="timeout">
    /// Operation timeout expressed in seconds.
    /// Null value disables timeout restrictions.
    /// </param>
    /// <returns>
    /// Password hash of specified user.
    /// </returns>
    public string? GetAccountPasswordHash(int userIdentifier)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@user_id", userIdentifier);

        var command = new CommandDefinition(
            commandType: CommandType.StoredProcedure,
            commandText: "sp_get_password_hash",
            parameters: parameters);

        using (IDbConnection connection = OpenConnection())
        {
            return connection.ExecuteScalar<string>(command);
        }
    }

    /// <summary>
    /// Provides details about specified pool of application user.
    /// </summary>
    /// <param name="userIdentifiers">
    /// Unique identifiers of users, whose details are requested to be obtained.
    /// </param>
    /// <returns>
    /// Details about specified pool of users.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    public User[] GetUsers(IEnumerable<int> userIdentifiers)
    {
        #region Arguments validation
        if (userIdentifiers is null)
        {
            string argumentName = nameof(userIdentifiers);
            const string ErrorMessage = "Provided collection of user identifiers is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        string userIdentifiersAsCsvList = string.Join(',', userIdentifiers);

        var parameters = new DynamicParameters();
        parameters.Add("@user_identifier_csv_list", userIdentifiersAsCsvList);

        var command = new CommandDefinition(
            commandType: CommandType.StoredProcedure,
            commandText: "sp_get_users",
            parameters: parameters);

        using (IDbConnection connection = OpenConnection())
        {
            return connection.Query<User>(command).ToArray();
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
    /// <returns>
    /// Details about messages sent in specified time frame to specified user.
    /// </returns>
    public Message[] GetMessagesSentTo(int receiverIdentifier, DateTime startTimestamp, DateTime endTimestamp)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@receiver_id", receiverIdentifier);
        parameters.Add("@start_timestamp", startTimestamp);
        parameters.Add("@end_timestamp", endTimestamp);

        var command = new CommandDefinition(
            commandType: CommandType.StoredProcedure,
            commandText: "sp_get_messages_sent_to",
            parameters: parameters);

        using (IDbConnection connection = OpenConnection())
        {
            return connection.Query<Message>(command).ToArray();
        }
    }

    /// <summary>
    /// Reads details about messages sent in specified time frame by specified user.
    /// </summary>
    /// <param name="senderIdentifier">
    /// Identifier of user, which shall be a sender of searched messages.
    /// </param>
    /// <param name="startTimestamp">
    /// Time frame start timestamp.
    /// </param>
    /// <param name="endTimestamp">
    /// Time frame end timestamp.
    /// </param>
    /// <returns>
    /// Details about messages sent in specified time frame by specified user.
    /// </returns>
    public Message[] GetMessagesSentBy(int senderIdentifier, DateTime startTimestamp, DateTime endTimestamp)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@sender_id", senderIdentifier);
        parameters.Add("@start_timestamp", startTimestamp);
        parameters.Add("@end_timestamp", endTimestamp);

        var command = new CommandDefinition(
            commandType: CommandType.StoredProcedure,
            commandText: "sp_get_messages_sent_by",
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
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown, when execution of requested operation fails.
    /// </exception>
    public void PutMessage(Message message)
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
        parameters.Add("@sender_id", message.SenderIdentifier);
        parameters.Add("@receiver_id", message.ReceiverIdentifier);
        parameters.Add("@content", message.Content);

        var command = new CommandDefinition(
            commandType: CommandType.StoredProcedure,
            commandText: "sp_put_message",
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
