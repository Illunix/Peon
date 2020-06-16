using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Peon.CLI.Services
{
    internal class BattleNetService
    {
        internal string GetLastestWowBuildConfig()
        {
            var webClient = new WebClient();

            using (var stream = new MemoryStream(webClient.DownloadData("http://eu.patch.battle.net:1119/wow_beta/versions")))
            using (var reader = new StreamReader(stream))
            {
                // Read useless lines.
                reader.ReadLine();
                reader.ReadLine();

                var line = reader.ReadLine();
                var array = line.Split('|');

                // second element of the array
                // us|3f483ee25f283e9072d1a9dceb0160c2|230ddf963c980e2d5ec9882c2a8a00ce||32861|8.3.0.32861|a96756c514489774e38ef1edbc17dcc5
                var buildConfig = array[1];

                return buildConfig;
            }
        }
    }
}
