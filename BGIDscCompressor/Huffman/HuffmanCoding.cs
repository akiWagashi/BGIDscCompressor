
using System.Collections;

namespace BGIDscCompressor.Huffman;
internal static class HuffmanCoding
{
    /// <summary>
    /// create huffman tree by leaf node depth info
    /// </summary>
    /// <param name="huffmanTree">create huffman tree buffer</param>
    /// <param name="leafNodeList">list of leaf node depth info</param>
    /// <param name="nodeCount">actual node number of childNodeList</param>
    public static void ReconstructHuffmanTreeByDepthInfo(HuffmanNode[] huffmanTree, HuffmanNodeDepthInfo[] leafNodeList, int nodeCount)
    {
        uint[,] nodePlane = new uint[2, 512];   //two planes,one is index in huffman tree of current depth node, and other is index in huffman tree of next depth node
        uint depth = 0; //depth of root node is 0
        uint currentDepthNodeNumber = 1;    //node number is 1 when depth is 0
        uint childPlaneIndex = 0;
        uint nextNodeIndex = 1;

        nodePlane[0, 0] = 0;    //set root node index

        for (uint i = 0; i < nodeCount;)    //leaf nodes at same depth, the smaller the code is, the more to the left it is.  
        {
            uint currentDepthChildNodeCount = 0;

            uint currentPlaneIndex = childPlaneIndex;

            childPlaneIndex ^= 1;

            while (leafNodeList[i].Depth == depth) //create a leaf ndoe when depth of childNodeList[i] equal current depth
            {
                HuffmanNode node = new HuffmanNode { isParent = false, Code = leafNodeList[i].Code };

                huffmanTree[nodePlane[currentPlaneIndex, currentDepthChildNodeCount]] = node;

                i++;
                currentDepthChildNodeCount++;

            }

            uint currentDepthInternalNodeNumber = currentDepthNodeNumber - currentDepthChildNodeCount;

            for (uint n = 0; n < currentDepthInternalNodeNumber; n++)   //create internal node of current depth
            {
                HuffmanNode node = new HuffmanNode();

                node.isParent = true;
                //prewrite the index for huffman tree of node in the next depth
                node.LeftChildNode = nodePlane[childPlaneIndex, n * 2] = nextNodeIndex++;
                node.RightChildNode = nodePlane[childPlaneIndex, n * 2 + 1] = nextNodeIndex++;

                huffmanTree[nodePlane[currentPlaneIndex, currentDepthChildNodeCount + n]] = node;

            }

            depth++;
            currentDepthNodeNumber = currentDepthInternalNodeNumber * 2; // is a ballpark

        }

    }

    public static HuffmanNodeDepthInfo[] BuildHuffmanTree(HuffmanNode[] huffmanTree, Dictionary<uint, uint> frequencyMap)
    {
        PriorityQueue<HuffmanNode, uint> nodeQueue = new PriorityQueue<HuffmanNode, uint>();

        foreach (var entry in frequencyMap) //init leaf node priorityQueue
        {
            var node = new HuffmanNode { isParent = false, Code = entry.Key, Weight = entry.Value };
            nodeQueue.Enqueue(node, entry.Value);
        }

        uint treeIndex = (uint)huffmanTree.Length - 1;

        while (nodeQueue.Count > 1) //builded huffman tree may be differ from original file , because code sort have some different
        {
            var leftNode = nodeQueue.Dequeue();
            var rightNode = nodeQueue.Dequeue();

            huffmanTree[treeIndex] = leftNode;
            huffmanTree[treeIndex - 1] = rightNode;

            var parentNode = new HuffmanNode { isParent = true, Weight = leftNode.Weight + rightNode.Weight, LeftChildNode = treeIndex, RightChildNode = treeIndex - 1 };

            nodeQueue.Enqueue(parentNode, parentNode.Weight);

            treeIndex -= 2;

        }

        if (nodeQueue.Count == 1) huffmanTree[0] = nodeQueue.Dequeue();

        var leafNodeDepthInfos = HuffmanCoding.SetNodeDepth(huffmanTree);

        Array.Sort(leafNodeDepthInfos, 0, frequencyMap.Count);

        HuffmanCoding.ReconstructHuffmanTreeByDepthInfo(huffmanTree, leafNodeDepthInfos, frequencyMap.Count);

        return leafNodeDepthInfos;

    }

    public static HuffmanNodeDepthInfo[] SetNodeDepth(HuffmanNode[] huffmanTree)
    {
        HuffmanNodeDepthInfo[] nodeDpethInfos = new HuffmanNodeDepthInfo[513];

        int infoIndex= 0;

        Stack<(uint, uint)> stack = new Stack<(uint, uint)>();

        stack.Push((0, 0));

        while (stack.Count > 0)
        {
            var (index, depth) = stack.Pop();

            var currentNode = huffmanTree[index];

            if (!currentNode.isParent)
            {
                currentNode.Depth = depth;

                nodeDpethInfos[infoIndex].Code = currentNode.Code;
                nodeDpethInfos[infoIndex].Depth = depth;

                infoIndex++;

            }
            else
            {
                if (currentNode.LeftChildNode != 0) stack.Push((currentNode.LeftChildNode, depth + 1));
                if (currentNode.RightChildNode != 0) stack.Push((currentNode.RightChildNode, depth + 1));
            }
        }

        return nodeDpethInfos.ToArray();

    }

    public static Dictionary<uint, BitArray> ConstructionPath(HuffmanNode[] huffmanTree)
    {
        Dictionary<uint, BitArray> pathMap = new Dictionary<uint, BitArray>();

        Stack<(uint, BitArray)> stack = new Stack<(uint, BitArray)>();

        stack.Push((0, new BitArray(0)));

        while (stack.Count > 0)
        {
            var (index, path) = stack.Pop();

            var currentNode = huffmanTree[index];

            if (!currentNode.isParent)
            {
                currentNode.Path = path;

                pathMap.Add(currentNode.Code, path);
            }
            else
            {
                if (currentNode.LeftChildNode != 0)
                {
                    var leftChildPath = new BitArray(path);

                    leftChildPath.Length = path.Length + 1;
                    leftChildPath[path.Length] = false;

                    stack.Push((currentNode.LeftChildNode, leftChildPath));
                }

                if (currentNode.RightChildNode != 0)
                {
                    var rightChildPath = new BitArray(path);

                    rightChildPath.Length = path.Length + 1;
                    rightChildPath[path.Length] = true;

                    stack.Push((currentNode.RightChildNode, rightChildPath));
                }
            }

        }

        return pathMap;
    }

}

