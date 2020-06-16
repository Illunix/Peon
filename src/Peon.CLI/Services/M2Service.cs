using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Peon.CLI.Services
{
    internal class M2Service
    {
        internal List<Texture> GetAllTextures(string file, bool verbose = false)
        {
            using (var reader = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                var textureList = new List<Texture>();

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var chunkId = new string(reader.ReadChars(4));
                    var chunkSize = reader.ReadInt32();

                    switch (chunkId)
                    {
                        case "MD21":
                            reader.BaseStream.Position += 80;

                            var textureSize = reader.ReadUInt32();
                            var textureOffset = reader.ReadUInt32();

                            reader.BaseStream.Position = textureOffset + 8;
                            for (var i = 0; i < textureSize; ++i)
                            {
                                textureList.Add(new Texture
                                {
                                    Type = reader.ReadUInt32(),
                                    Flags = reader.ReadUInt32()
                                });

                                reader.BaseStream.Position += 8;
                            }

                            reader.BaseStream.Position = 8 + chunkSize;
                            break;
                        case "TXID":
                            for (var i = 0; i < chunkSize / 4; ++i)
                            {
                                var fileDataId = reader.ReadUInt32();
                                textureList[i].FileDataId = fileDataId;
                            }
                            break;
                        default:
                            if (verbose)
                            {
                                Console.WriteLine($"Skipping {chunkId} with size {chunkSize}");
                            }
                            reader.BaseStream.Position += chunkSize;
                            break;
                    }
                }

                return textureList;
            }
        }
    }
}
