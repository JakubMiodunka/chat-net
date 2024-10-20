using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtilities.Padding;


namespace CommonUtilities.BitPadding;

// PKCS#7 algorythm is described in RFC 5652. 
public sealed class Pkcs7PaddingProvider : IBitPaddingProvider
{
    #region Properties
    public int DataBlockSize { get; init; }
    #endregion

    #region Instantiation
    public Pkcs7PaddingProvider(int dataBlockSize)
    {
        #region Arguments validation
        if (dataBlockSize < 2)
        {
            string argumentName = nameof(dataBlockSize);
            string errorMessage = $"Invalid data block size specified: {dataBlockSize}";
            throw new ArgumentOutOfRangeException(argumentName, dataBlockSize, errorMessage);
        }
        #endregion

        DataBlockSize = dataBlockSize;
    }
    #endregion

    #region Padding
    public byte[] AddPadding(IEnumerable<byte> data)
    {
        IEnumerable<byte[]> dataChunks = data.Chunk(DataBlockSize - 1);

        var paddedData = new List<byte>();

        foreach (byte[] dataChunk in dataChunks)
        {
            byte paddingLength = Convert.ToByte(DataBlockSize - dataChunk.Count());
            byte paddingByte = paddingLength;

            IEnumerable<byte> padding = Enumerable.Repeat(paddingByte, paddingLength);

            paddedData.AddRange(dataChunk);
            paddedData.AddRange(padding);
        }

        return paddedData.ToArray();
    }

    public byte[] RemovePadding(IEnumerable<byte> data)
    {
        IEnumerable<byte[]> dataChunks = data.Chunk(DataBlockSize);

        var unpaddedData = new List<byte>();

        foreach (byte[] dataChunk in dataChunks)
        {
            byte paddingLength = dataChunk.Last();

            IEnumerable<byte> unpaddedDataChunk = dataChunk.SkipLast(paddingLength);

            unpaddedData.AddRange(unpaddedDataChunk);
        }

        return unpaddedData.ToArray();
    }
    #endregion
}
