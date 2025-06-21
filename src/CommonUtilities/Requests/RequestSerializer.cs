// Ignore Spelling: Json Serializer Deserialized Deserialize

using CommonUtilities.Requests.Models;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace CommonUtilities.Requests;

/// <summary>
/// Utility, which is able to serialize requests instances to sequences of bytes
/// and deserialize those back to their original form.
/// </summary>
public static class RequestSerializer
{
    #region Static properties
    private static Dictionary<Type, Regex>? s_requestTypeToRegexMappingCache;
    private static Dictionary<Type, Regex> s_requestTypeToRegexMapping
    {
        get
        {
            return s_requestTypeToRegexMappingCache ??= GenerateRequestTypeToRegexMapping();
        }
    }
    #endregion

    #region Static methods
    private static Dictionary<Type, Regex> GenerateRequestTypeToRegexMapping()
    {
        Type[] requestTypes = {
            typeof(GetAuthenticationRequest),
            typeof(PutAuthenticationRequest),
            typeof(GetMessagesRequest),
            typeof(PutMessagesRequest),
            typeof(GetUsersRequest),
            typeof(PutUsersRequest),
            typeof(PutTextMessageRequest)};

        var requestTypeToRegexMapping = new Dictionary<Type, Regex>();

        foreach (Type type in requestTypes)
        {
            var regex = new Regex($"^.*\"Type\":\"{type.Name}\".*$");
            requestTypeToRegexMapping.Add(type, regex);
        }

        return requestTypeToRegexMapping;
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Serializes provided request to sequence of bytes.
    /// </summary>
    /// <typeparam name="TRequest">
    /// Type of request, which is requested to be serialized.
    /// </typeparam>
    /// <param name="request">
    /// Request, which shall be serialized.
    /// </param>
    /// <returns>
    /// Sequence of bytes corresponding to provided request.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    public static byte[] Serialize<TRequest>(TRequest request) where TRequest : Request
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

    /// <summary>
    /// Deserializes sequence of bytes to an request instance.
    /// </summary>
    /// <param name="bytes">
    /// Sequence of bytes, which shall be deserialized into request instance.
    /// </param>
    /// <returns>
    /// Request corresponding to provided sequence of bytes.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown, when provided byte sequence cannot be deserialized to request instance.
    /// </exception>
    public static Request Deserialize(IEnumerable<byte> bytes)
    {
        #region Arguments validation
        if (bytes is null)
        {
            string argumentName = nameof(bytes);
            const string ErrorMessage = "Provided byte sequence is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        string requestAsJsonString;

        try
        {
            requestAsJsonString = Encoding.UTF8.GetString(bytes.ToArray());
        }
        catch (DecoderFallbackException exception)
        {
            string decodingErrorMessage = $"Unable to decode provided byte sequence: {bytes}";
            throw new NotSupportedException(decodingErrorMessage, exception);
        }

        lock (s_requestTypeToRegexMapping)
        {
            foreach (KeyValuePair<Type, Regex> mapping in s_requestTypeToRegexMapping)
            {
                Type requestType = mapping.Key;
                Regex requestPattern = mapping.Value;

                if (requestPattern.IsMatch(requestAsJsonString))
                {
                    Request? request = JsonSerializer.Deserialize(requestAsJsonString, requestType) as Request;

                    if (request is not null)
                    {
                        return request;
                    }
                }
            }
        }

        string jsonDeserializationErrorMessage = $"Unable to deserialize decoded JSON string: {requestAsJsonString}";
        throw new NotSupportedException(jsonDeserializationErrorMessage);
    }
    #endregion
}

