using CommonUtilities.Models;
using Server;


namespace UnitTests.Server;

//TODO: Add more test cases to cover all functionalities of DatabaseClient class.
[Category("IntegrationTest")]
[TestOf(typeof(DatabaseClient))]
[Author("Jakub Miodunka")]
public sealed class DatabaseIntegrationTests
{
    #region Configuration
    private const string ConnectionString = @"Server=127.0.0.1;Database=chat-net-database;User Id=sa;Password=SqlServer2019;TrustServerCertificate=True";
    #endregion

    #region Axillary properties
    /*
     * Below properties shall
     */
    private static User[] _users;
    private static Message[] _messages;
    private static Dictionary<int, string> _passwordHashes; //key - user identifier, value - password hash of particular user

    private DatabaseClient _client;
    #endregion

    #region Test setup
    [SetUp]
    public void SetUp()
    {
        _client = new DatabaseClient(ConnectionString);

        _users ??=
            [
                new User(1, "Sylvester Stallone"),
                new User(2, "Arnold Schwarzenegger"),
                new User(3, "Bruce Willis"),
                new User(4, "Jean-Claude Van Damme"),
                new User(5, "Kurt Russell"),
                new User(6, "Chuck Norris"),
                new User(7, "Harrison Ford"),
                new User(8, "Dolph Lundgren"),
                new User(9, "Tom Cruise"),
                new User(10, "Val Kilmer")
            ];

        _messages ??=
            [
                new Message(1, DateTime.Parse("2025-01-01T12:00:00"), 1, 2, "I'm John, John Rambo"),
                new Message(2, DateTime.Parse("2025-01-01T12:15:00"), 2, 1, "I'll be back"),
                new Message(3, DateTime.Parse("2024-07-19T20:10:00"), 9, 10, "Good job Iceman"),
                new Message(4, DateTime.Parse("2024-07-20T01:00:00"), 10, 9, "I've got you Mav"),
                new Message(5, DateTime.Parse("2025-03-21T13:30:00"), 3, 7, "Zed is dead baby, Zed is dead"),
                new Message(7, DateTime.Parse("2025-03-21T21:15:00"), 7, 3, "Wrong number - Indy here"),
                new Message(6, DateTime.Parse("2023-10-10T14:30:00"), 5, 6, "We need to burn it!")
            ];

        _passwordHashes ??= new Dictionary<int, string>()
        {
            { 1, "1234567890" },
            { 2, "0987654321" },
            { 3, "24680" },
            { 4, "13579" },
            { 5, "2143658709" },
            { 6, "3254769801" },
            { 7, "321654987" },
            { 8, "432765098" },
            { 9, "0192837465" },
            { 10, "5647382910" }
        };
    }
    #endregion

    #region Test cases
    [TestCaseSource(nameof(_users))]
    public void ReadingPasswordHashIsPossible(User user)
    {
        string expectedPasswordHash = _passwordHashes[user.Id];
        string actualPasswordHash = _client.GetPasswordHashFor(user.Id);

        Assert.That(actualPasswordHash, Is.EqualTo(expectedPasswordHash));
    }

    [TestCaseSource(nameof(_users))]
    public void ReadingUserDetailsIsPossible(User expectedUser)
    {
        User actualUser = _client.GetUser(expectedUser.Id);

        Assert.That(actualUser, Is.Not.Null);
    }

    [TestCase(-1)]
    public void ReadingNonExistingUserCausesException(int nonExistingUserId)
    {
        TestDelegate actionUnderTest = () => _client.GetUser(nonExistingUserId);

        Assert.Throws<InvalidOperationException>(actionUnderTest);
    }

    [TestCaseSource(nameof(_users))]
    public void ReadingUserIdIsSuccessful(User expectedUser)
    {
        User actualUser = _client.GetUser(expectedUser.Id);

        Assert.That(actualUser.Id, Is.EqualTo(expectedUser.Id));
    }

    [TestCaseSource(nameof(_users))]
    public void ReadingUserNameIsSuccessful(User expectedUser)
    {
        User actualUser = _client.GetUser(expectedUser.Id);

        Assert.That(actualUser.Name, Is.EqualTo(expectedUser.Name));
    }
    #endregion
}
