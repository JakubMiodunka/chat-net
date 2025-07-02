// Ignore Spelling: Deauthentication

using CommonUtilities.Models;
using Moq;
using NUnit.Framework.Internal;
using Server.Repositories;
using Server.Security;


namespace UnitTests.Server.Security;

[Category("UnitTest")]
[TestOf(typeof(ConnectionAuthenticator))]
[Author("Jakub Miodunka")]
public class ConnectionAuthenticatorTests
{
    #region Default values
    private User _defaultUser;
    private string _defaultPasswordHash;
    private int _defaultConnetionIdentifier;
    #endregion

    #region Auxiliary methods
    private Mock<IUserRepository> CreateDefaultUserRepositoryFake()
    {
        var userRepositoryFake = new Mock<IUserRepository>();

        userRepositoryFake
            .Setup(userRepository => userRepository.GetAccountPasswordHash(_defaultUser.Identifier))
            .Returns(_defaultPasswordHash);

        userRepositoryFake
            .Setup(userRepository => userRepository.GetUserDetails(It.Is<IEnumerable<int>>((collection) => collection.Contains(_defaultUser.Identifier))))
            .Returns([_defaultUser]);

        return userRepositoryFake;
    }
    #endregion

    #region Test setup
    [SetUp]
    public void SetUp()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        _defaultUser = new User(
            Identifier: randomizer.Next(),
            Name: randomizer.GetString());

        _defaultPasswordHash = randomizer.GetString();
        _defaultConnetionIdentifier = randomizer.Next();
    }
    #endregion

    #region Test cases
    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsUserRepository()
    {
        TestDelegate actionUnderTest = () => new ConnectionAuthenticator(null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void AccessIsGrantedIfPasswordHashIsMatching()
    {
        Mock<IUserRepository> userRepositoryStub = CreateDefaultUserRepositoryFake();

        var connectionAuthenticatorUnderTest = new ConnectionAuthenticator(userRepositoryStub.Object);

        bool isAccessGranted = connectionAuthenticatorUnderTest.AuthenticateConnection(_defaultConnetionIdentifier, _defaultUser.Identifier, _defaultPasswordHash);

        Assert.That(isAccessGranted, Is.True);
    }

    [Test]
    public void AccessDeniedIfPasswordHashIsNotMatching()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        string nonMatchingPasswordHash = randomizer.GetString();
        while (nonMatchingPasswordHash == _defaultPasswordHash)
        {
            nonMatchingPasswordHash = randomizer.GetString();
        }

        Mock<IUserRepository> userRepositoryStub = CreateDefaultUserRepositoryFake();

        var connectionAuthenticatorUnderTest = new ConnectionAuthenticator(userRepositoryStub.Object);

        bool isAccessGranted = connectionAuthenticatorUnderTest.AuthenticateConnection(_defaultConnetionIdentifier, _defaultUser.Identifier, nonMatchingPasswordHash);

        Assert.That(isAccessGranted, Is.False);
    }

    [Test]
    public void DetailsAboutUserAssociatedWithConnectionAvailableIfConnectionIsAuthenticated()
    {
        Mock<IUserRepository> userRepositoryStub = CreateDefaultUserRepositoryFake();

        var connectionAuthenticatorUnderTest = new ConnectionAuthenticator(userRepositoryStub.Object);
        connectionAuthenticatorUnderTest.AuthenticateConnection(_defaultConnetionIdentifier, _defaultUser.Identifier, _defaultPasswordHash);

        User? obtainedUserDetails = connectionAuthenticatorUnderTest.GetUserAssociatedWithConnection(_defaultConnetionIdentifier);

        Assert.That(_defaultUser, Is.EqualTo(obtainedUserDetails));
    }

    [Test]
    public void DetailsAboutUserAssociatedWithConnectionNotAvailableIfConnectionIsNotAuthenticated()
    {
        Mock<IUserRepository> userRepositoryStub = CreateDefaultUserRepositoryFake();

        var connectionAuthenticatorUnderTest = new ConnectionAuthenticator(userRepositoryStub.Object);
        // Authenticator is operational but no connection is authenticated.

        User? uauthenticatedUserDetails = connectionAuthenticatorUnderTest.GetUserAssociatedWithConnection(_defaultConnetionIdentifier);

        Assert.That(uauthenticatedUserDetails, Is.Null);
    }

    [Test]
    public void DetailsAboutConnectionAssociatedWithUserAvailableIfConnectionIsAuthenticated()
    {
        Mock<IUserRepository> userRepositoryStub = CreateDefaultUserRepositoryFake();

        var connectionAuthenticatorUnderTest = new ConnectionAuthenticator(userRepositoryStub.Object);
        connectionAuthenticatorUnderTest.AuthenticateConnection(_defaultConnetionIdentifier, _defaultUser.Identifier, _defaultPasswordHash);

        int? obtainedConnectionIdentifier = connectionAuthenticatorUnderTest.GetConnectionAssociatedWithUser(_defaultUser.Identifier);

        Assert.That(_defaultConnetionIdentifier, Is.EqualTo(obtainedConnectionIdentifier));
    }

    [Test]
    public void DetailsAboutConnectionAssociatedWithUserNotAvailableIfConnectionIsNotAuthenticated()
    {
        Mock<IUserRepository> userRepositoryStub = CreateDefaultUserRepositoryFake();

        var connectionAuthenticatorUnderTest = new ConnectionAuthenticator(userRepositoryStub.Object);
        // Authenticator is operational but no connection is authenticated.

        int? obtainedConnectionIdentifier = connectionAuthenticatorUnderTest.GetConnectionAssociatedWithUser(_defaultUser.Identifier);

        Assert.That(obtainedConnectionIdentifier, Is.Null);
    }

    [Test]
    public void DeauthenticationOfConnectionPossible()
    {
        Mock<IUserRepository> userRepositoryStub = CreateDefaultUserRepositoryFake();

        var connectionAuthenticatorUnderTest = new ConnectionAuthenticator(userRepositoryStub.Object);
        connectionAuthenticatorUnderTest.AuthenticateConnection(_defaultConnetionIdentifier, _defaultUser.Identifier, _defaultPasswordHash);

        connectionAuthenticatorUnderTest.DeauthenticateConnection(_defaultConnetionIdentifier);
        User? unauthenticatedUserDetails = connectionAuthenticatorUnderTest.GetUserAssociatedWithConnection(_defaultConnetionIdentifier);

        Assert.That(unauthenticatedUserDetails, Is.Null);
    }
    #endregion
}
