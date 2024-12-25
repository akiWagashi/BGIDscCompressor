using BGIDscCompressor.DscHandler;
using System.Diagnostics;

namespace BGIDscCompressor;
internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage : ");
            Console.WriteLine("ToolName -u <dsc file> [uncompressed file path]");
            Console.WriteLine("ToolName -c <target file> [compressed file path] [key]");
            return;
        }

        string mode = args[0];

        Stopwatch watch = new Stopwatch();

        watch.Start();

        switch (mode)
        {
            case "-u":
                {

                    using (FileStream sourceFile = File.OpenRead(args[1]))
                    {
                        using (BinaryReader reader = new BinaryReader(sourceFile))
                        {

                            DscUncompressor dscUncompressor = DscUncompressor.Create(reader.ReadBytes((int)sourceFile.Length));

                            var uncompressedData = dscUncompressor.Uncompress();

                            string createFileName = string.Empty;

                            if (args.Length >= 3)
                                createFileName = args[2];
                            else
                                createFileName = Path.ChangeExtension(args[1], "uncompress");

                            using (FileStream createFile = File.Create(createFileName))
                            {
                                createFile.Write(uncompressedData);
                            }
                        }

                    }

                    break;
                }

            case "-c":
                {
                    uint? key = null;

                    if (args.Length >= 4) key = Convert.ToUInt32(args[3]);

                    using (FileStream sourceFile = File.OpenRead(args[1]))
                    {
                        using (BinaryReader reader = new BinaryReader(sourceFile))
                        {

                            DscCompressor dscCompressor = new DscCompressor(reader.ReadBytes((int)sourceFile.Length), key);

                            var compressedData = dscCompressor.Compress();

                            string createFileName = string.Empty;

                            if (args.Length >= 3)
                                createFileName = args[2];
                            else
                                createFileName = Path.ChangeExtension(args[1], "compress");


                            using (FileStream createFile = File.Create(createFileName))
                            {
                                createFile.Write(compressedData);
                            }

                        }

                    }

                    break;
                }
        }

        watch.Stop();

        Console.WriteLine($"SUCCESS : Execution completd in {watch.ElapsedMilliseconds} ms");
    }
}

