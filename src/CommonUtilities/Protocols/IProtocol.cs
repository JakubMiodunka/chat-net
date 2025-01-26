namespace CommonUtilities.Protocols;

// TODO: Add doc-strings.
public interface IProtocol
{
    byte[] ExtractPayload(IEnumerable<byte> packet);
    byte[] PreparePacket(IEnumerable<byte> payload);
}
