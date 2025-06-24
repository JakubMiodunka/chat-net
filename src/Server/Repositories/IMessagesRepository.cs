using CommonUtilities.Models;


namespace Server.Repositories;

/// <summary>
/// Interface of repository containing details about messages exchanged between application users.
/// </summary>
public interface IMessagesRepository
{
    /// <summary>
    /// Searches for details about messages sent in specified time frame to specified user.
    /// </summary>
    /// <param name="receiverIdentifier">
    /// Identifier of user, which shall be a receiver of searched messages.
    /// </param>
    /// <param name="startTimestamp">
    /// Time frame start timestamp.
    /// </param>
    /// <param name="endTimestamp">
    /// Time frame end timestamp.
    /// </param>
    /// <returns>
    /// Details about messages sent in specified time frame to specified user.
    /// </returns>
    Message[] GetMessagesSentTo(int receiverIdentifier, DateTime startTimestamp, DateTime endTimestamp);

    /// <summary>
    /// Searches for details about messages sent in specified time frame by specified user.
    /// </summary>
    /// <param name="senderIdentifier">
    /// Identifier of user, which shall be a sender of searched messages.
    /// </param>
    /// <param name="startTimestamp">
    /// Time frame start timestamp.
    /// </param>
    /// <param name="endTimestamp">
    /// Time frame end timestamp.
    /// </param>
    /// <returns>
    /// Details about messages sent in specified time frame by specified user.
    /// </returns>
    Message[] GetMessagesSentBy(int senderIdentifier, DateTime startTimestamp, DateTime endTimestamp);

    /// <summary>
    /// Saves provided message in repository.
    /// </summary>
    /// <param name="message">
    /// Message, which shall be saved to repository.
    /// </param>
    void PutMessage(Message message);
}
