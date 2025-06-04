using CommonUtilities.Models.Requests;
using Server;


namespace UnitTests.Server;

// TODO: Finish implementation
[Category("UnitTest")]
[TestOf(typeof(RequestSerializer))]
[Author("Jakub Miodunka")]
public class RequestSerializerTests
{
    #region Test cases
    [Test]
    public void SerializationOfAuthenticateRequestIsTransparent()
    {
        var referenceRequest = new AuthenticateRequest(1, "asdadd");

        byte[] serializedRequest = RequestSerializer.Serialize(referenceRequest);
        AuthenticateRequest deserializedRequest = RequestSerializer.Deserialize<AuthenticateRequest>(serializedRequest);

        Assert.That(deserializedRequest, Is.EqualTo(referenceRequest));
    }
    #endregion
}
