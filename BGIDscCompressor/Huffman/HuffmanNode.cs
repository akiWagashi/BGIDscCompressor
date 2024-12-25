
using System.Collections;

namespace BGIDscCompressor.Huffman;

internal class HuffmanNode 
{
    public bool isParent { get; set; }

    public uint LeftChildNode { get; set; }

    public uint RightChildNode { get; set; }

    public uint Depth {  get; set; }

    public uint Weight { get; set; }

    public uint Code { get; set; }

    public BitArray Path { get; set; }
}
