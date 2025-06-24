namespace CommonUtilities.Requests.Models;

/// <summary>
/// Request sent to server by a client to indicate, that new text message was sent by him
/// and needs to be delivered to other user.
/// </summary>
/// <param name="ReceiverIdentifier">
/// Identifier of a user, who shall be receiver of the message.
/// </param>
/// <param name="MessageContent">
/// Content of sent message.
/// </param>
public sealed record PutTextMessageRequest(int ReceiverIdentifier, string MessageContent) : Request;
