namespace CommonUtilities.Models.Requests;

/// <summary>
/// Respond containing details about pool of messages.
/// </summary>
/// <param name="Messages">
/// Messages, which shall be included in request content.
/// </param>
public sealed record PutMessagesRespond(Message[] Messages) : Request;