using BGIDscCompressor.Huffman;
using BGIDscCompressor.Utils;
using BGIDscCompressor.Utils.BitHnadler;
using BGIDscCompressor.Utils.Lzss;

namespace BGIDscCompressor.DscHandler;
public sealed class DscUncompressor
{
    private readonly uint UncompressBufferSize;

    private readonly uint CompressCount;

    private readonly byte[] UncompressBuffer;

    private readonly byte[] compressBuffer;

    private readonly KeyGenerator KeyGenerator;

    private DscUncompressor(byte[] input)
    {
        uint magic = (uint)BitConverter.ToUInt16(input, 0) << 16;
        uint key = BitConverter.ToUInt32(input, 0x10);

        this.KeyGenerator = new KeyGenerator(magic, key);

        this.UncompressBufferSize = BitConverter.ToUInt32(input, 0x14);
        this.UncompressBuffer = new byte[this.UncompressBufferSize];

        this.CompressCount = BitConverter.ToUInt32(input, 0x18);

        this.compressBuffer = input[DscScript.HeaderSize..];

    }

    public static DscUncompressor Create(byte[] input)
    {

        if (input.Length < DscScript.HeaderSize + DscScript.NodeBlockSize) throw new ArgumentException("the input buffer is too small");

        if (!DscScript.CheckCompress(input)) throw new ArgumentException("the input buffer maybe not match the compressed _bp file format");

        return new DscUncompressor(input);
    }

    private void UncompressDataByHuffman(HuffmanNode[] huffmanTree)
    {

        int uncompressByteCount = 0;

        BitReader bitReader = new BitReader(this.compressBuffer[DscScript.NodeBlockSize..]);

        int posCount = 0;

        for (int i = 0; i < this.CompressCount; i++)
        {
            uint nodeIndex = 0;

            do
            {
                int bit = bitReader.ReadNextBit();

                nodeIndex = bit == 0 ? huffmanTree[nodeIndex].LeftChildNode : huffmanTree[nodeIndex].RightChildNode;

            } while (huffmanTree[nodeIndex].isParent);

            uint code = huffmanTree[nodeIndex].Code;

            if (code >= 256)
            {
                int offset = bitReader.ReadBits(12) + Lzss.MinMatchOffset;
                int length = ((int)code & 0xFF) + Lzss.MinMatchLength;

                posCount++;

                for (int n = 0; n < length; n++)
                {
                    this.UncompressBuffer[uncompressByteCount] = this.UncompressBuffer[uncompressByteCount - offset];
                    uncompressByteCount++;
                }

            }
            else
            {
                this.UncompressBuffer[uncompressByteCount] = (byte)code;
                uncompressByteCount++;
            }

        }

    }

    public byte[] Uncompress()
    {
        HuffmanNodeDepthInfo[] nodeInfos = new HuffmanNodeDepthInfo[513];
        HuffmanNode[] nodes = new HuffmanNode[1023];    //the tree all nodes number has 2n-1 when leaf nodes number is n

        byte depth = 0;

        int nodeCount = 0;

        for (uint i = 0; i < DscScript.NodeBlockSize; i++)   //traverse depth info block
        {
            depth = (byte)(this.compressBuffer[i] - KeyGenerator.UpdateKey());

            if (depth > 0)
            {
                nodeInfos[nodeCount].Depth = depth;
                nodeInfos[nodeCount].Code = i;
                nodeCount++;
            }

        }

        Array.Sort(nodeInfos, 0, nodeCount);

        HuffmanCoding.ReconstructHuffmanTreeByDepthInfo(nodes, nodeInfos, nodeCount);

        this.UncompressDataByHuffman(nodes);

        return this.UncompressBuffer;
    }


}

