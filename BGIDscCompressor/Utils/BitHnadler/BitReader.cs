
namespace BGIDscCompressor.Utils.BitHnadler;
internal sealed class BitReader
{

    private readonly byte[] Data;

    private uint ByteIndex { get; set; } = 0;

    private int AvailableBitCount { get; set; } = 0;

    private int CurrentValue { get; set; } = 0;

    public BitReader(byte[] data) => this.Data = data;

    public int ReadBits(int needBitCount)
    {
        int result = 0;
        int readBitCount = 0;

        if (needBitCount > 32) needBitCount = 32;

        while (needBitCount > 0)
        {
            if (this.AvailableBitCount == 0)
            {
                if (this.ByteIndex >= this.Data.Length) throw new IndexOutOfRangeException("BitReader index out range of Data");

                this.CurrentValue = this.Data[this.ByteIndex];
                this.ByteIndex++;
                this.AvailableBitCount = 8;
            }

            readBitCount = Math.Min(this.AvailableBitCount, needBitCount);

            result <<= readBitCount;

            result |= (this.CurrentValue >> (this.AvailableBitCount - readBitCount));
            this.CurrentValue &= (1 << (this.AvailableBitCount - readBitCount)) - 1;

            this.AvailableBitCount -= readBitCount;
            needBitCount -= readBitCount;

        }

        return result;
    }

    public int ReadNextBit() => this.ReadBits(1);

}

