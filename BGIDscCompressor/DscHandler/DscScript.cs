using System.Text;

namespace BGIDscCompressor.DscHandler;
public class DscScript
{
    public static readonly string CompressSignature = "DSC FORMAT 1.00\0";

    public static readonly int HeaderSize = 0x20;

    public static readonly int NodeBlockSize = 512;

    public static readonly uint DefualtMagic = 0x53440000;

    public static bool CheckCompress(ReadOnlySpan<byte> input)
    {
        if(input.Length < 0x10 ) return false;

        return DscScript.CompressSignature.Equals(Encoding.UTF8.GetString(input.Slice(0, 0x10)));
    }
}

