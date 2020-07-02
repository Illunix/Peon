using Peon.CLI.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Peon.CLI.Services
{
    public class ModelReader : IModelReader
    {
        private BinaryReader _reader;
        private static string _model;
        private List<uint> _fileIds = new List<uint>();

        public void Read(string model)
        {
            _model = model;

            _reader = new BinaryReader(File.OpenRead(model));

            if (_model.EndsWith(".m2"))
            {
                ReadM2();
            }

            if (_model.EndsWith(".wmo"))
            {
                ReadWMO();
            }

            if (_model.EndsWith(".adt"))
            {
                ReadADT();
            }

            _fileIds.RemoveAll(fileId => fileId.Equals(0));
        }

        public IReadOnlyList<uint> GetModelModelsFilesIds(string file, IReadOnlyList<string> downloadedFilePaths)
        {
            var models = new string[downloadedFilePaths.Count];

            if (file.EndsWith(".wmo"))
            {
                models = downloadedFilePaths
                    .Where(model => model.EndsWith(".m2")).ToArray();
            }

            if (file.EndsWith(".adt"))
            {
                models = downloadedFilePaths
                    .Where(model => model.EndsWith(".wmo") || model.EndsWith(".m2")).ToArray();
            }

            foreach (var model in models)
            {
                Read(model);

                return GetFileIds();
            }

            return null;
        }

        public IReadOnlyList<uint> GetFileIds()
        {
            return _fileIds;
        }

        public void ClearFileIds()
        {
            _fileIds.Clear();
        }

        private void ReadM2()
        {
            while (_reader.BaseStream.Position < _reader.BaseStream.Length)
            {
                var chunkId = new string(_reader.ReadChars(4));
                var chunkSize = _reader.ReadInt32();

                switch (chunkId)
                {
                    case "AFID":
                        for (var i = 0; i < chunkSize / 8; ++i)
                        {
                            _reader.ReadUInt32();

                            _fileIds.Add(_reader.ReadUInt32());
                        }
                        break;

                    case "SFID":
                        for (var i = 0; i < chunkSize / 4; ++i)
                        {
                            _fileIds.Add(_reader.ReadUInt32());
                        }
                        break;

                    case "TXID":
                        for (var i = 0; i < chunkSize / 4; ++i)
                        {
                            _fileIds.Add(_reader.ReadUInt32());
                        }
                        break;

                    default:
                        _reader.BaseStream.Position += chunkSize;
                        break;
                }
            }

            _reader.Close();
        }

        private void ReadWMO()
        {
            while (_reader.BaseStream.Position < _reader.BaseStream.Length)
            {
                var chunkId = new string(_reader.ReadChars(4).Reverse().ToArray());
                var chunkSize = _reader.ReadInt32();

                switch (chunkId)
                {
                    case "MODI":    // Doodads
                        for (var i = 0; i < chunkSize / 4; ++i)
                        {
                            _fileIds.Add(_reader.ReadUInt32());
                        }
                        break;

                    case "MOMT":    // Textures
                        for (var i = 0; i < chunkSize / 64; ++i)
                        {
                            // Skip Shader + BlendMode
                            _reader.ReadUInt64();
                            _reader.ReadUInt32();

                            for (var j = 0; j < 2; ++j)
                            {
                                _fileIds.Add(_reader.ReadUInt32());

                                _reader.ReadUInt64();
                            }
                            _fileIds.Add(_reader.ReadUInt32());

                            _reader.BaseStream.Position += (16 + 4 + 4);
                        }
                        break;

                    default:
                        _reader.BaseStream.Position += chunkSize;
                        break;
                }
            }

            _reader.Close();
        }

        private void ReadADT()
        {
            while (_reader.BaseStream.Position < _reader.BaseStream.Length)
            {
                var chunkId = new string(_reader.ReadChars(4).Reverse().ToArray());
                var chunkSize = _reader.ReadUInt32();

                switch (chunkId)
                {
                    case "MDID":    // Diffuse Textures
                    case "MHID":    // Height Textures
                        for (var i = 0; i < chunkSize / 4; ++i)
                        {
                            _fileIds.Add(_reader.ReadUInt32());
                        }
                        break;

                    case "MDDF":    // M2 Models
                        for (var i = 0; i < chunkSize / 36; ++i)
                        {
                            _fileIds.Add(_reader.ReadUInt32());

                            _reader.BaseStream.Position += sizeof(uint) * 8;
                        }
                        break;

                    case "MODF":    // WMO Models
                        for (var i = 0; i < chunkSize / 64; ++i)
                        {
                            _fileIds.Add(_reader.ReadUInt32());

                            _reader.BaseStream.Position += sizeof(uint) * 15;
                        }
                        break;

                    default:
                        _reader.BaseStream.Position += chunkSize;
                        break;
                }
            }

            _reader.Close();
        }
    }
}