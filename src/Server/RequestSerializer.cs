// Ignore Spelling: Deserialize Serializer

using CommonUtilities.Models.Requests;
using System.Text;
using System.Text.Json;

namespace Server;

// TODO: Finish implementation
public static class RequestSerializer
{
    public static byte[] Serialize<T>(T request) where T : Request
    {
        #region Arguments validation
        if (request is null)
        {
            string argumentName = nameof(request);
            const string ErrorMessage = "Provided request is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        string requestAsJsonString = JsonSerializer.Serialize(request);
        byte[] requestAsBytes = Encoding.UTF8.GetBytes(requestAsJsonString);

        return requestAsBytes;
    }

    public static T Deserialize<T>(IEnumerable<byte> serializedRequest)
    {
        #region Arguments validation
        if (serializedRequest is null)
        {
            string argumentName = nameof(serializedRequest);
            const string ErrorMessage = "Provided serialized request is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        // TODO: handle DecoderFalbackException
        string requestAsJsonString = Encoding.UTF8.GetString(serializedRequest.ToArray());
        T? request = JsonSerializer.Deserialize<T>(requestAsJsonString);

        if (request == null)
        {
            throw new Exception();
        }

        return request;
    }
}
