namespace CommonUtilities.Models;

/// <summary>
/// Model of a message sent between two application users.
/// </summary>
/// <param name="Timestamp">
/// Timestamp indicating when the message was sent.
/// </param>
/// <param name="Sender">
/// Message sender.
/// </param>
/// <param name="Receiver">
/// Message receiver.
/// </param>
/// <param name="Content">
/// Message content.
/// </param>
public sealed record Message(DateTime Timestamp, User Sender, User Receiver, string Content);