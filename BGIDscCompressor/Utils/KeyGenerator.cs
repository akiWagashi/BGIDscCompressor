
namespace BGIDscCompressor.Utils;
internal sealed class KeyGenerator
{
    private uint Magic { get; init; }

    private uint Key { get; set; }

    public KeyGenerator(uint magic, uint key)
    {
        this.Magic = magic;
        this.Key = key;
    }

    public byte UpdateKey()
    {
        uint temp = 20021 * (this.Key & 0xFFFF);
        uint result = this.Magic | (this.Key >> 16);
        result = result * 20021 + this.Key * 346;
        result = (result + (temp >> 16)) & 0xFFFF;
        this.Key = (result << 16) + (temp & 0xFFFF) + 1;

        return (byte)result;

    }
}

