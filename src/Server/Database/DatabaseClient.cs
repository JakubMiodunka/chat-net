using CommonUtilities.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;


namespace Server.Database;

// TODO: Add doc-strings.
public sealed class DatabaseClient
{
    #region Properties
    private readonly string _connectionString;
    #endregion

    #region Instantiation
    public DatabaseClient(string connectionString, int? commandTimeout = null)
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
    private IDbConnection OpenConnection()
    {
        var sqliteCOnnection = new SqlConnection(_connectionString);
        sqliteCOnnection.Open();

        return sqliteCOnnection;
    }

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
            string argumentname = nameof(userId);
            string errorMessage = $"Unable to get password hash for specified user: {userId}";
            throw new ArgumentOutOfRangeException(argumentname, userId, errorMessage);
        }

        return passwordHash;
    }

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
                string errorMessage = $"Unable to get user with specified identifier: {userId}";
                throw new ArgumentOutOfRangeException(errorMessage, exception);
            }
        }
    }
    
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
    
    public void AddMessage(Message message, int? timeout = 1)
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
