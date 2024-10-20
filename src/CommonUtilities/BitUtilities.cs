using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtilities;

public static class BitUtilities
{
    public static uint[] AsUintArray(IEnumerable<byte> data)
    {
        #region Arguments validation
        if (data is null)
        {
            string argumentName = nameof(data);
            const string ErrorMessage = "Provided data set is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if ((data.Count() % 4) != 0)
        {
            string argumentName = nameof(data);
            string errorMessage = $"Invalid length of provided data set: {data.Count()}";
            throw new ArgumentException(errorMessage, argumentName);
        }
        #endregion

        return data
            .Chunk(4)
            .Select(dataChunk => BitConverter.ToUInt32(dataChunk, 0))
            .ToArray();
    }
}
