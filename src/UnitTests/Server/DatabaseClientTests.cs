using CommonUtilities.Models;
using Server.Database;

namespace UnitTests.Server;

//TODO: Add doc-strings
//TODO: Add more test cases to achieve full coverage.
public sealed class DatabaseClientTests
{
    #region Default values
    private const string DefaultConnectionString = @"Server=127.0.0.1;Database=chat-net-database;User Id=sa;Password=SqlServer2019;TrustServerCertificate=True";

    private DatabaseClient _defaultClient;
    #endregion

    #region Test setup
    [SetUp]
    public void SetUp()
    {
        _defaultClient = new DatabaseClient(DefaultConnectionString);
    }
    #endregion

    #region Test cases
    [TestCase(1)]
    public void GetExistingUserTest(int existingUserId)
    {
        User user = _defaultClient.GetUser(existingUserId);

        Assert.That(user, Is.Not.Null);
    }

    [TestCase(2)]
    public void GetNonExistingUserTest(int existingUserId)
    {
        TestDelegate actionUnderTest = () => _defaultClient.GetUser(existingUserId);

        Assert.Throws<ArgumentOutOfRangeException>(actionUnderTest);
    }
    #endregion
}
