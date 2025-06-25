// Ignore Spelling: Deauthentication

using CommonUtilities.Models;
using Moq;
using NUnit.Framework.Internal;
using Server.Repositories;
using Server.Security;


namespace UnitTests.Server.Security;

// TODO: Maybe some more test cases are needed here?
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
    private Mock<IUserRepository> CreateUserRepositoryFakeContainingDefaultUser()
    {
        var userRepositoryFake = new Mock<IUserRepository>();

        userRepositoryFake
            .Setup(userRepository => userRepository.GetAccountPasswordHash(_defaultUser.Identifier))
            .Returns(_defaultPasswordHash);

        userRepositoryFake
            .Setup(userRepository => userRepository.GetUsers(It.Is<IEnumerable<int>>((collection) => collection.Contains(_defaultUser.Identifier))))
            .Returns([_defaultUser]);

        return userRepositoryFake;
    }
    #endregion

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

    
    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsUserRepository()
    {
        TestDelegate actionUnderTest = () => new ConnectionAuthenticator(null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void AccessIsGrantedIfPasswordHashIsMatching()
    {
        Mock<IUserRepository> userRepositoryStub = CreateUserRepositoryFakeContainingDefaultUser();

        var connectionAuthenticatorUnderTest = new ConnectionAuthenticator(userRepositoryStub.Object);

        bool isAccessGranted = connectionAuthenticatorUnderTest.AuthenticateConnection(_defaultConnetionIdentifier, _defaultUser.Identifier, _defaultPasswordHash);

        Assert.That(isAccessGranted, Is.True);
    }

    [Test]
    public void AccessNotGrantedIfPasswordHashIsNotMatching()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        Mock<IUserRepository> userRepositoryStub = CreateUserRepositoryFakeContainingDefaultUser();
        
        string nonMatchingPasswordHash = randomizer.GetString();
        while (nonMatchingPasswordHash == _defaultPasswordHash)
        {
            nonMatchingPasswordHash = randomizer.GetString();
        }

        var connectionAuthenticatorUnderTest = new ConnectionAuthenticator(userRepositoryStub.Object);

        bool isAccessGranted = connectionAuthenticatorUnderTest.AuthenticateConnection(_defaultConnetionIdentifier, _defaultUser.Identifier, nonMatchingPasswordHash);

        Assert.That(isAccessGranted, Is.False);
    }

    [Test]
    public void DetailsAboutUserAssociatedWithConnectionAvailableIfConnectionIsAuthenticated()
    {
        Mock<IUserRepository> userRepositoryStub = CreateUserRepositoryFakeContainingDefaultUser();

        var connectionAuthenticatorUnderTest = new ConnectionAuthenticator(userRepositoryStub.Object);
        connectionAuthenticatorUnderTest.AuthenticateConnection(_defaultConnetionIdentifier, _defaultUser.Identifier, _defaultPasswordHash);

        User? obtainedUserDetails = connectionAuthenticatorUnderTest.GetUserAssociatedWithConnection(_defaultConnetionIdentifier);

        Assert.That(_defaultUser, Is.EqualTo(obtainedUserDetails));
    }

    [Test]
    public void DetailsAboutUserAssociatedWithConnectionNotAvailableIfConnectionIsNotAuthenticated()
    {
        Mock<IUserRepository> userRepositoryStub = CreateUserRepositoryFakeContainingDefaultUser();

        var connectionAuthenticatorUnderTest = new ConnectionAuthenticator(userRepositoryStub.Object);
        User? uauthenticatedUserDetails = connectionAuthenticatorUnderTest.GetUserAssociatedWithConnection(_defaultConnetionIdentifier);

        Assert.That(uauthenticatedUserDetails, Is.Null);
    }

    [Test]
    public void DeauthenticationOfConnectionPossible()
    {
        Mock<IUserRepository> userRepositoryStub = CreateUserRepositoryFakeContainingDefaultUser();

        var connectionAuthenticatorUnderTest = new ConnectionAuthenticator(userRepositoryStub.Object);
        connectionAuthenticatorUnderTest.AuthenticateConnection(_defaultConnetionIdentifier, _defaultUser.Identifier, _defaultPasswordHash);

        connectionAuthenticatorUnderTest.DeauthenticateConnection(_defaultConnetionIdentifier);
        User? uauthenticatedUserDetails = connectionAuthenticatorUnderTest.GetUserAssociatedWithConnection(_defaultConnetionIdentifier);

        Assert.That(uauthenticatedUserDetails, Is.Null);
    }
}
