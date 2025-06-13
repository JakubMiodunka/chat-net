// Ignore Spelling: Serializer Deserialization

using CommonUtilities.Models;
using CommonUtilities.Requests;
using CommonUtilities.Requests.Models;
using NUnit.Framework.Internal;


namespace UnitTests.CommonUtilities.Requests;

[Category("UnitTest")]
[TestOf(typeof(RequestSerializer))]
[Author("Jakub Miodunka")]
public sealed class RequestSerializerTests
{
    #region Auxiliary methods
    private static Message GenerateRandomMessage(int maxMessageContentLength = 100)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        int contentLength = randomizer.Next(maxMessageContentLength);

        return new Message(
            Id: randomizer.Next(),
            Timestamp: DateTime.FromBinary(randomizer.NextLong(DateTime.MinValue.ToBinary(), DateTime.MaxValue.ToBinary())),
            SenderId: randomizer.Next(),
            ReceiverId: randomizer.Next(),
            Content: randomizer.GetString(contentLength));
    }

    private static User GenerateRandomUser(int maxUserNameLength = 25)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        int userNameLength = randomizer.Next(maxUserNameLength);

        return new User(
            Id: randomizer.Next(),
            Name: randomizer.GetString(userNameLength));
    }
    #endregion

    #region Test cases
    [Test]
    public void SerializationOfNullReferenceNotPossible()
    {
        TestDelegate actionUnderTest = () => RequestSerializer.Serialize<Request>(null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void DeserializationOfNullReferenceNotPossible()
    {
        TestDelegate actionUnderTest = () => RequestSerializer.Deserialize(null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void GetAuthenticationRequestSerializationIsTransparent(
        [Values(60)] int passwordHashLength)    // Hashes produced by SHA-256 has length of 60 characters.
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var expectedRequest = new GetAuthenticationRequest(
            UserId: randomizer.Next(),
            PasswordHash: randomizer.GetString(passwordHashLength));

        byte[] serializedRequest = RequestSerializer.Serialize(expectedRequest);
        var actualRequest = RequestSerializer.Deserialize(serializedRequest) as GetAuthenticationRequest;

        Assert.That(actualRequest, Is.EqualTo(expectedRequest));
    }

    [Test]
    public void PutAuthenticationRequestSerializationIsTransparent()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var expectedRequest = new PutAuthenticationRequest(IsAccessGranted: randomizer.NextBool());

        byte[] serializedRequest = RequestSerializer.Serialize(expectedRequest);
        var actualRequest = RequestSerializer.Deserialize(serializedRequest) as PutAuthenticationRequest;

        Assert.That(actualRequest, Is.EqualTo(expectedRequest));
    }

    [Test]
    public void GetMessagesRequestSerializationIsTransparent()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var startTimestamp = DateTime.FromBinary(randomizer.NextLong(DateTime.MinValue.ToBinary(), DateTime.MaxValue.ToBinary() - 2));
        var endTimestamp = DateTime.FromBinary(randomizer.NextLong(startTimestamp.ToBinary() + 1, DateTime.MaxValue.ToBinary()));
        var expectedRequest = new GetMessagesRequest(startTimestamp, endTimestamp);

        byte[] serializedRequest = RequestSerializer.Serialize(expectedRequest);
        var actualRequest = RequestSerializer.Deserialize(serializedRequest) as GetMessagesRequest;

        Assert.That(actualRequest, Is.EqualTo(expectedRequest));
    }

    [Test]
    public void PutMessagesRequestSerializationIsTransparent(
        [Values(0, 25, 100)] int numberOfMessages)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var messages = Enumerable.Range(0, numberOfMessages)
            .Select(_ => GenerateRandomMessage())
            .ToArray();

        var expectedRequest = new PutMessagesRequest(messages);

        byte[] serializedRequest = RequestSerializer.Serialize(expectedRequest);
        var actualRequest = RequestSerializer.Deserialize(serializedRequest) as PutMessagesRequest;

        // Manual equality check as one of request properties is a collection (is reference-type property).
        Assert.That(actualRequest, Is.Not.Null);
        Assert.That(actualRequest.Type, Is.EqualTo(expectedRequest.Type));
        Assert.That(actualRequest.Messages, Is.EquivalentTo(expectedRequest.Messages));
    }

    [Test]
    public void GetUsersRequestSerializationIsTransparent(
        [Values(0, 25, 100)] int numberOfRequestedUsers)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        int[] requestedUsersIds = Enumerable.Range(0, numberOfRequestedUsers)
            .Select(_ => randomizer.Next())
            .ToArray();

        var expectedRequest = new GetUsersRequest(requestedUsersIds);

        byte[] serializedRequest = RequestSerializer.Serialize(expectedRequest);
        var actualRequest = RequestSerializer.Deserialize(serializedRequest) as GetUsersRequest;

        // Manual equality check as one of request properties is a collection (is reference-type property).
        Assert.That(actualRequest, Is.Not.Null);
        Assert.That(actualRequest.Type, Is.EqualTo(expectedRequest.Type));
        Assert.That(actualRequest.UserIds, Is.EquivalentTo(expectedRequest.UserIds));
    }

    [Test]
    public void PutUsersRequestSerializationIsTransparent(
        [Values(0, 25, 100)] int numberOfUsers)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var useres = Enumerable.Range(0, numberOfUsers)
            .Select(_ => GenerateRandomUser())
            .ToArray();

        var expectedRequest = new PutUsersRequest(useres);

        byte[] serializedRequest = RequestSerializer.Serialize(expectedRequest);
        var actualRequest = RequestSerializer.Deserialize(serializedRequest) as PutUsersRequest;

        // Manual equality check as one of request properties is a collection (is reference-type property).
        Assert.That(actualRequest, Is.Not.Null);
        Assert.That(actualRequest.Type, Is.EqualTo(expectedRequest.Type));
        Assert.That(actualRequest.Users, Is.EquivalentTo(expectedRequest.Users));
    }
    #endregion
}
