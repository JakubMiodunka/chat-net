using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.SqlTypes;


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

    public bool IsUserExist(string userName, int? timeout = 1)
    {
        #region Arguments validation
        if (userName is null)
        {
            string argumentName = nameof(userName);
            const string ErrorMessage = "Provided user name is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        var parameters = new DynamicParameters();
        parameters.Add("@UserName", userName);

        var command = new CommandDefinition(
            commandType: CommandType.Text,
            commandText: @"SELECT 1 FROM Users WHERE Name=@UserName LIMIT 1",
            commandTimeout: timeout,
            parameters: parameters);

        using (IDbConnection connection = OpenConnection())
        {
            return connection.ExecuteScalar<object>(command) != null;
        }
    }

    public string GetPasswordHashFor(string userName, int? timeout = 1)
    {
        #region Arguments validation
        if (userName is null)
        {
            string argumentName = nameof(userName);
            const string ErrorMessage = "Provided user name is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        var parameters = new DynamicParameters();
        parameters.Add("@UserName", userName);

        var command = new CommandDefinition(
            commandType: CommandType.Text,
            commandText: @"SELECT PasswordHash FROM Users WHERE Name=@UserName LIMIT 1",
            commandTimeout: timeout,
            parameters: parameters);

        string? passwordHash;

        using (IDbConnection connection = OpenConnection())
        {
            passwordHash = connection.ExecuteScalar<string>(command);
        }

        if (passwordHash is null)
        {
            string errorMessage = $"Unable to get password hash for specified user: {userName}";
            throw new SqlNullValueException(errorMessage);
        }

        return passwordHash;
    }
    #endregion
}
