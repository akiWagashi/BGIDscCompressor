using BGIDscCompressor.Huffman;
using BGIDscCompressor.Utils;
using BGIDscCompressor.Utils.BitHnadler;
using BGIDscCompressor.Utils.Lzss;
using System.Text;

namespace BGIDscCompressor.DscHandler;
public class DscCompressor
{
    private readonly byte[] UncompressBuffer;

    private readonly uint InitKey;

    private readonly KeyGenerator keyGenerator;

    public DscCompressor(byte[] input, uint? key)
    {
        this.UncompressBuffer = input;

        if (key is null)
            this.InitKey = (uint)new Random().Next(int.MinValue, int.MaxValue);
        else
            this.InitKey = key.Value;

        this.keyGenerator = new KeyGenerator(DscScript.DefualtMagic, this.InitKey);
    }

    public byte[] Compress()
    {
        Dictionary<uint, uint> frequencyMap = new Dictionary<uint, uint>();

        var lzssPosInfos = Lzss.Parse(this.UncompressBuffer);

        int posInfoIndex = 0;

        for (int i = 0; i < this.UncompressBuffer.Length;) //record code frequency
        {
            if (posInfoIndex < lzssPosInfos.Count && i == lzssPosInfos[posInfoIndex].Position)
            {

                uint code = (1 << 8) + (uint)(lzssPosInfos[posInfoIndex].Length - Lzss.MinMatchLength);

                if (frequencyMap.ContainsKey(code))
                {
                    frequencyMap[code]++;
                }
                else
                {
                    frequencyMap[code] = 1;
                }

                i += lzssPosInfos[posInfoIndex].Length;
                posInfoIndex++;
            }
            else
            {

                if (frequencyMap.ContainsKey(this.UncompressBuffer[i]))
                {
                    frequencyMap[this.UncompressBuffer[i]]++;
                }
                else
                {
                    frequencyMap[this.UncompressBuffer[i]] = 1;
                }

                i++;
            }

        }

        HuffmanNode[] huffmanTree = new HuffmanNode[frequencyMap.Count * 2 - 1];

        var nodeDepthInfos = HuffmanCoding.BuildHuffmanTree(huffmanTree, frequencyMap);     //build huffman tree and get leaf node depth info

        byte[] nodeDepthInfoBlockBuffer = new byte[DscScript.NodeBlockSize];

        for (int i = 0; i < frequencyMap.Count; i++) nodeDepthInfoBlockBuffer[nodeDepthInfos[i].Code] = (byte)nodeDepthInfos[i].Depth;  //init node depth info block

        var pathMap = HuffmanCoding.ConstructionPath(huffmanTree);  //construction leaf node path and get mapping between code and path

        BitWriter bitWriter = new BitWriter();

        posInfoIndex = 0;

        int compreessCount = 0;

        for (int i = 0; i < this.UncompressBuffer.Length;)  //write compressed data based on source data
        {
            if (posInfoIndex < lzssPosInfos.Count && i == lzssPosInfos[posInfoIndex].Position)
            {
                uint code = (1 << 8) + (uint)(lzssPosInfos[posInfoIndex].Length - Lzss.MinMatchLength);

                foreach (var item in pathMap[code]) bitWriter.WriteBits(Convert.ToInt32(item), 1);

                bitWriter.WriteBits(lzssPosInfos[posInfoIndex].Offset - Lzss.MinMatchOffset, 12);

                i += lzssPosInfos[posInfoIndex].Length;
                posInfoIndex++;

            }
            else
            {
                foreach (var item in pathMap[(uint)this.UncompressBuffer[i]]) bitWriter.WriteBits(Convert.ToInt32(item), 1);
                i++;
            }

            compreessCount++;
        }

        byte[] compressData = bitWriter.ToArray();

        MemoryStream memoryStream = new MemoryStream(DscScript.HeaderSize + DscScript.NodeBlockSize + compressData.Length);

        using (BinaryWriter msWriter = new BinaryWriter(memoryStream))
        {
            msWriter.Write(Encoding.UTF8.GetBytes(DscScript.CompressSignature)); //write compressSignature

            msWriter.Write(this.InitKey);   //write nodeBlock init key

            msWriter.Write(this.UncompressBuffer.Length);   //write uncopress data size

            msWriter.Write(compreessCount); //write

            msWriter.Write(0);  //

            for (int i = 0; i < DscScript.NodeBlockSize; i++)    //write encrypted depth info 
            {
                msWriter.Write((byte)(nodeDepthInfoBlockBuffer[i] + keyGenerator.UpdateKey()));
            }

            msWriter.Write(compressData);   //write compressed data

        }

        return memoryStream.ToArray();
    }

}

