using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Peon.CLI.Services
{
    internal class ListfileService
    {
        private WebClient _webClient = new WebClient();
        private static string _listfile = "listfile.csv";
        private static Dictionary<uint, string> _filedataPair = new Dictionary<uint, string>();

        internal bool Initialized;

        internal void Initialize()
        {
            if (!File.Exists(_listfile))
            {
                _webClient.DownloadFile("https://wow.tools/casc/listfile/download/csv/unverified", _listfile);

                Initialized = true;
            }
            else
            {
                using (var reader = new StreamReader(_listfile))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var array = line.Split(';');

                        _filedataPair.Add(uint.Parse(array[0]), array[1]);
                    }

                    Initialized = true;
                }
            }
        }

        internal string GetFilenameById(uint id)
        {
            if (_filedataPair.TryGetValue(id, out string filename))
            {
                return filename;
            }

            return null;
        }
    }
}
