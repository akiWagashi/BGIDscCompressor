
namespace BGIDscCompressor.Utils.BitHnadler;

internal sealed class BitWriter
{
    private readonly List<Byte> Buffer = new List<Byte>();

    private int AvailableBitCount { get; set; } = 8;

    private int CurrentValue { get; set; } = 0;

    public void WriteBits(int writeValue, int needBitCount)
    {
        int writeBitCount = 0;

        if (needBitCount > 32) needBitCount = 32;

        while (needBitCount > 0)
        {
            if (this.AvailableBitCount == 0)
            {
                this.Buffer.Add((byte)this.CurrentValue);
                this.AvailableBitCount = 8;
                this.CurrentValue = 0;
            }

            writeBitCount = Math.Min(this.AvailableBitCount, needBitCount);

            int mask = (1 << writeBitCount) - 1;

            int writeBits = (writeValue >> (needBitCount - writeBitCount)) & mask;

            this.CurrentValue |= writeBits << (this.AvailableBitCount - writeBitCount);

            needBitCount -= writeBitCount;
            this.AvailableBitCount -= writeBitCount;
        }
    }

    public byte[] ToArray()
    {
        if (this.AvailableBitCount != 8) this.Buffer.Add((byte)this.CurrentValue);

        return this.Buffer.ToArray();
    }

};
