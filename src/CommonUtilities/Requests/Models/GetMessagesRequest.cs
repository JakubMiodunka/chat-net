namespace CommonUtilities.Requests.Models;

/// <summary>
/// Request for sending details about messages sent in specified time frame,
/// which receiver is a requester.
/// </summary>
/// <param name="StartTimestamp">
/// Time frame start timestamp.
/// </param>
/// <param name="EndTimestamp">
/// Time frame end timestamp.
/// </param>
public sealed record GetMessagesRequest(DateTime StartTimestamp, DateTime EndTimestamp) : Request;
