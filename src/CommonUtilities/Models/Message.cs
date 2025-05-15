namespace CommonUtilities.Models;

/// <summary>
/// Model of a message sent between two application users.
/// </summary>
/// <param name="Id">
/// Unique message identifier.
/// </param>
/// <param name="Timestamp">
/// Timestamp indicating when the message was sent.
/// </param>
/// <param name="SenderId">
/// Unique identifier of message sender.
/// </param>
/// <param name="ReceiverId">
/// Unique identifier of message receiver.
/// </param>
/// <param name="Content">
/// Message content.
/// </param>
public sealed record Message(int Id, DateTime Timestamp, int SenderId, int ReceiverId, string Content);