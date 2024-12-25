namespace BGIDscCompressor.Huffman;
internal struct HuffmanNodeDepthInfo : IComparable<HuffmanNodeDepthInfo>
{
    public uint Code { get; set; }

    public uint Depth { get; set; }

    public int CompareTo(HuffmanNodeDepthInfo deothInfo) 
    {
        int result = (int)this.Depth - (int)deothInfo.Depth;

        if (result == 0) result = (int)this.Code - (int)deothInfo.Code; 

        return result;

    }

}

