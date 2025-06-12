using CommonUtilities.Models;

namespace CommonUtilities.Requests.Models;

/// <summary>
/// Respond containing details about pool of messages.
/// </summary>
/// <param name="Messages">
/// Messages, which shall be included in request content.
/// </param>
public sealed record PutMessagesRequest(Message[] Messages) : Request;