namespace CommonUtilities.Padding;


/// <summary>
/// Shall be implemented by all providers of bit padding.
/// </summary>
public interface IBitPaddingProvider
{
    int DataBlockSize { get; }

    /// <summary>
    /// Adds padding to the given data set to make its length a multiple of the block size.
    /// </summary>
    /// <param name="data">
    /// Data set, which shall be padded.
    /// </param>
    /// <returns>
    /// Set of padded data, corresponding to provided data set.
    /// </returns>
    byte[] AddPadding(IEnumerable<byte> data);

    /// <summary>
    /// Removes padding from provided data set.
    /// </summary>
    /// <param name="data">
    /// Data set, from which padding shall be removed.
    /// </param>
    /// <returns>
    ///  Set of unpadded data, corresponding to provided data set.
    /// </returns>
    byte[] RemovePadding(IEnumerable<byte> data);
}
