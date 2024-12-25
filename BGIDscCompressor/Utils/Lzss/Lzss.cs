namespace BGIDscCompressor.Utils.Lzss;

internal sealed class Lzss
{
    public static readonly int MinMatchLength = 2;
    public static readonly int MinMatchOffset = 2;
    public static readonly int OffsetBits = 12;
    public static readonly int SlidingWindowSize = (1 << OffsetBits) - 1 + MinMatchLength;
    public static readonly int LengthBits = 8;
    public static readonly int LookAheadSize = (1 << LengthBits) - 1 + MinMatchLength;

    public static List<LzssInfo> Parse(byte[] sourceBuffer)
    {
        var result = new List<LzssInfo>();

        int bufferIndex = 0;
        int windowIndex = 0;

        while (bufferIndex < sourceBuffer.Length)   //is slow
        {
            int matchLength = 0;
            int matchOffset = 0;
            int maxMatchLength = Math.Min(LookAheadSize, sourceBuffer.Length - bufferIndex);

            for (windowIndex = bufferIndex - MinMatchOffset; windowIndex >= 0 && windowIndex >= bufferIndex - SlidingWindowSize; windowIndex--)
            {
                int i = 0;

                while (i < maxMatchLength && sourceBuffer[windowIndex + i] == sourceBuffer[bufferIndex + i])
                {
                    i++;
                }

                if (i > matchLength)
                {
                    matchOffset = bufferIndex - windowIndex;
                    matchLength = i;
                }
            }

            if (matchLength >= MinMatchLength)
            {
                LzssInfo info = new LzssInfo() { Position = bufferIndex, Offset = matchOffset, Length = matchLength };
                result.Add(info);

            }
            else
            {
                matchLength = 1;
            }

            bufferIndex += matchLength;

        }

        return result;
    }
};
