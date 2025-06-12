// Ignore Spelling: Json Serializer Deserialized Deserialize

using CommonUtilities.Requests.Models;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CommonUtilities.Requests;

// TODO: Add doc-strings.
// TODO: Add unit tests.
public static class RequestSerializer
{
    #region Constants
    // TODO: Add requests patters here.
    private const string GetAuthenticationRequestPattern = @"";
    private const string PutAuthenticationRequestPattern = @"";
    private const string GetMessagesRequestPattern = @"";
    private const string PutMessagesRequestPattern = @"";
    private const string GetUsersRequestPattern = @"";
    private const string PutUsersRequestPattern = @"";
    #endregion

    #region Static properties
    private static readonly Dictionary<Type, Regex> _requestPatternMapping  = new Dictionary<Type, Regex>
        {
            { typeof(GetAuthenticationRequest), new Regex(GetAuthenticationRequestPattern) },
            { typeof(PutAuthenticationRequest), new Regex(PutAuthenticationRequestPattern) },
            { typeof(GetMessagesRequest), new Regex(GetMessagesRequestPattern) },
            { typeof(PutMessagesRequest), new Regex(PutMessagesRequestPattern) },
            { typeof(GetUsersRequest), new Regex(GetUsersRequestPattern) },
            { typeof(PutUsersRequest), new Regex(PutUsersRequestPattern) }
        };
    #endregion

    #region Interactions
    public static byte[] Serialize<TRequest>(TRequest request) where TRequest : Request
    {
        string requestAsJsonString = JsonSerializer.Serialize(request);
        byte[] requestAsBytes = Encoding.UTF8.GetBytes(requestAsJsonString);

        return requestAsBytes;
    }
    public static Request Deserialize(IEnumerable<byte> receivedData)
    {
        string requestAsJsonString = Encoding.UTF8.GetString(receivedData.ToArray());

        Request? request = null;

        lock (_requestPatternMapping)
        {
            foreach (KeyValuePair<Type, Regex> mapping in _requestPatternMapping)
            {
                Type requestType = mapping.Key;
                Regex requestPattern = mapping.Value;

                if (requestPattern.IsMatch(requestAsJsonString))
                {
                    request = JsonSerializer.Deserialize(requestAsJsonString, requestType) as Request;

                    if (request is not null)
                    {
                        return request;
                    }
                }
            }
        }

        string errorMessage = $"Unable to deserialize provided JSON string: {requestAsJsonString}";
        throw new NotSupportedException(errorMessage);
    }
    #endregion
}

