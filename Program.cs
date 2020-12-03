using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace getpeerings
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: getpeerings <path>");
                return 1;
            }

            string path = args[0];

            string[] files = Directory.GetFiles(path, "*.json");

            var projects = new List<(string filename, JArray jarray)>();

            foreach (var filename in files)
            {
                var jarray = JArray.Parse(File.ReadAllText(filename));
                projects.Add((Path.GetFileNameWithoutExtension(filename), jarray));
            }

            Console.WriteLine($"Got {projects.Count} projects.");

            var networkpeerings = new List<JObject>();

            foreach (var project in projects)
            {
                foreach (var network in project.jarray)
                {
                    if (network is JObject jobject && jobject["peerings"] != null && jobject["peerings"] is JArray peerings)
                    {
                        foreach (JObject peering in peerings)
                        {
                            peering["project"] = project.filename;
                            peering["name"] = network["name"];
                            peering["network"] = Path.GetFileName(peering["network"]?.Value<string>());
                            peering["source_network"] = Path.GetFileName(peering["source_network"]?.Value<string>());
                            networkpeerings.Add(peering);
                        }
                    }
                }
            }

            Console.WriteLine($"Got {networkpeerings.Count} network peerings.");

            var table = new string[networkpeerings.Count + 1, 10];

            table[0, 0] = "project";
            table[0, 1] = "source";
            table[0, 2] = "target";
            table[0, 3] = "autoCreateRoutes";
            table[0, 4] = "exchangeSubnetRoutes";
            table[0, 5] = "exportCustomRoutes";
            table[0, 6] = "exportIp";
            table[0, 7] = "importCustomRoutes";
            table[0, 8] = "importIp";
            table[0, 9] = "state";
            //table[0, 10] = "stateDetails";
            var sortedNetworkpeerings = networkpeerings
                .OrderBy(p => p["project"])
                .ThenBy(p => p["name"])
                .ThenBy(p => p["network"])
                .ToList();
            for (int i = 0; i < sortedNetworkpeerings.Count; i++)
            {
                var networkpeering = sortedNetworkpeerings[i];
                table[i + 1, 0] = networkpeering["project"]?.Value<string>() ?? string.Empty;
                table[i + 1, 1] = networkpeering["name"]?.Value<string>() ?? string.Empty;
                table[i + 1, 2] = networkpeering["network"]?.Value<string>() ?? string.Empty;
                table[i + 1, 3] = networkpeering["autoCreateRoutes"]?.Value<string>() ?? string.Empty;
                table[i + 1, 4] = networkpeering["exchangeSubnetRoutes"]?.Value<string>() ?? string.Empty;
                table[i + 1, 5] = networkpeering["exportCustomRoutes"]?.Value<string>() ?? string.Empty;
                table[i + 1, 6] = networkpeering["exportSubnetRoutesWithPublicIp"]?.Value<string>() ?? string.Empty;
                table[i + 1, 7] = networkpeering["importCustomRoutes"]?.Value<string>() ?? string.Empty;
                table[i + 1, 8] = networkpeering["importSubnetRoutesWithPublicIp"]?.Value<string>() ?? string.Empty;
                table[i + 1, 9] = networkpeering["state"]?.Value<string>() ?? string.Empty;
                //table[i + 1, 10] = networkpeering["stateDetails"]?.Value<string>() ?? string.Empty;
            }

            ShowTable(table);

            return 0;
        }

        static void ShowTable(string[,] items)
        {
            if (items.Length == 0)
            {
                return;
            }

            int[] maxwidths = GetMaxWidths(items);

            int rowcount = items.GetLength(0);
            int colcount = items.GetLength(1);

            for (int row = 0; row < rowcount; row++)
            {
                var output = new StringBuilder();

                for (int col = 0; col < colcount; col++)
                {
                    if (col > 0)
                    {
                        output.Append(' ');
                    }
                    output.AppendFormat("{0,-" + maxwidths[col] + "}", items[row, col]);
                }

                Console.WriteLine(output.ToString().TrimEnd());
            }
        }

        static int[] GetMaxWidths(string[,] items)
        {
            int rowcount = items.GetLength(0);
            int colcount = items.GetLength(1);
            int[] maxwidths = new int[colcount];

            for (var row = 0; row < rowcount; row++)
            {
                for (var col = 0; col < colcount; col++)
                {
                    if (row == 0)
                    {
                        maxwidths[col] = items[row, col].Length;
                    }
                    else
                    {
                        if (items[row, col].Length > maxwidths[col])
                        {
                            maxwidths[col] = items[row, col].Length;
                        }
                    }
                }
            }

            return maxwidths;
        }
    }
}
