// Ignore Spelling: Deauthentication

using CommonUtilities.Models;
using Moq;
using NUnit.Framework.Internal;
using Server;
using Server.Repositories;
using Server.Security;


namespace UnitTests.Server;

// TODO: Continue here
[Category("UnitTest")]
[TestOf(typeof(RequestHandler))]
[Author("Jakub Miodunka")]
public class RequestHandlerTests
{
    // #region Default values
    // private User _defaultUser1;
    // private string _defaultPasswordHash;
    // private int _defaultConnetionIdentifier;
    // #endregion
    // 
    // #region Auxiliary methods
    // private Mock<IUserRepository> CreateDefaultUserRepositoryFake()
    // {
    //     var userRepositoryFake = new Mock<IUserRepository>();
    // 
    //     userRepositoryFake
    //         .Setup(userRepository => userRepository.GetAccountPasswordHash(_defaultUser.Identifier))
    //         .Returns(_defaultPasswordHash);
    // 
    //     userRepositoryFake
    //         .Setup(userRepository => userRepository.GetUserDetails(It.Is<IEnumerable<int>>((collection) => collection.Contains(_defaultUser.Identifier))))
    //         .Returns([_defaultUser]);
    // 
    //     return userRepositoryFake;
    // }
    // #endregion

    #region Test cases
    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsConnectionAuthenticator()
    {
        var userRepositorySub = new Mock<IUserRepository>();
        var messagesRepositoryStub = new Mock<IMessagesRepository>();

        TestDelegate actionUnderTest = () => new RequestHandler(null!, userRepositorySub.Object, messagesRepositoryStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsUserRepository()
    {
        var connectionAuthenticatorStub = new Mock<IConnectionAuthenticator>();
        var messagesRepositoryStub = new Mock<IMessagesRepository>();

        TestDelegate actionUnderTest = () => new RequestHandler(connectionAuthenticatorStub.Object, null!, messagesRepositoryStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsMessageRepository()
    {
        var connectionAuthenticatorStub = new Mock<IConnectionAuthenticator>();
        var userRepositorySub = new Mock<IUserRepository>();

        TestDelegate actionUnderTest = () => new RequestHandler(connectionAuthenticatorStub.Object, userRepositorySub.Object, null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }
    #endregion
}
