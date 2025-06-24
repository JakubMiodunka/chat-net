namespace CommonUtilities.Requests.Models;

/// <summary>
/// Request used by client to ask server about messages sent to or by him in specified time frame.
/// </summary>
/// <param name="StartTimestamp">
/// Time frame start timestamp.
/// </param>
/// <param name="EndTimestamp">
/// Time frame end timestamp.
/// </param>
public sealed record GetMessagesRequest(DateTime StartTimestamp, DateTime EndTimestamp) : Request;
