namespace CommonUtilities.Models;

/// <summary>
/// Model of a message sent between two application users.
/// </summary>
/// <param name="Timestamp">
/// Timestamp indicating when the message was sent.
/// </param>
/// <param name="SenderIdentifier">
/// Unique identifier of message sender.
/// </param>
/// <param name="ReceiverIdentifier">
/// Unique identifier of message receiver.
/// </param>
/// <param name="Content">
/// Message content.
/// </param>
public sealed record Message(DateTime Timestamp, int SenderIdentifier, int ReceiverIdentifier, string Content);