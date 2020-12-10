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
            if (args.Length != 1 && args.Length != 2)
            {
                Console.WriteLine("Usage: getpeerings <path> [project/network filter]");
                return 1;
            }

            string path = args[0];
            string filter = args.Length == 2 ? args[1] : string.Empty;

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
                            peering["source"] = GetShortName(peering["source_network"]?.Value<string>() ?? string.Empty);
                            peering["target"] = GetShortName(peering["network"]?.Value<string>() ?? string.Empty);
                            networkpeerings.Add(peering);
                        }
                    }
                }
            }

            Console.WriteLine($"Got {networkpeerings.Count} network peerings.");

            if (filter != string.Empty)
            {
                for (int i = 0; i < networkpeerings.Count;)
                {
                    if ((networkpeerings[i]["source"]?.Value<string>()?.Contains(filter) ?? true) ||
                        (networkpeerings[i]["target"]?.Value<string>()?.Contains(filter) ?? true))
                    {
                        i++;
                    }
                    else
                    {
                        networkpeerings.RemoveAt(i);
                    }
                }

                Console.WriteLine($"Filtered to {networkpeerings.Count} network peerings with: '{filter}'");
            }

            var table = new string[networkpeerings.Count + 1, 10];

            table[0, 0] = "source";
            table[0, 1] = "target";
            table[0, 2] = "autoCreateRoutes";
            table[0, 3] = "exchangeSubnetRoutes";
            table[0, 4] = "exportCustomRoutes";
            table[0, 5] = "exportIp";
            table[0, 6] = "importCustomRoutes";
            table[0, 7] = "importIp";
            table[0, 8] = "state";
            table[0, 9] = "stateDetails";
            var sortedNetworkpeerings = networkpeerings
                .OrderBy(p => p["source"])
                .ThenBy(p => p["target"])
                .ToList();
            for (int i = 0; i < sortedNetworkpeerings.Count; i++)
            {
                var networkpeering = sortedNetworkpeerings[i];
                table[i + 1, 0] = networkpeering["source"]?.Value<string>() ?? string.Empty;
                table[i + 1, 1] = networkpeering["target"]?.Value<string>() ?? string.Empty;
                table[i + 1, 2] = networkpeering["autoCreateRoutes"]?.Value<string>() ?? string.Empty;
                table[i + 1, 3] = networkpeering["exchangeSubnetRoutes"]?.Value<string>() ?? string.Empty;
                table[i + 1, 4] = networkpeering["exportCustomRoutes"]?.Value<string>() ?? string.Empty;
                table[i + 1, 5] = networkpeering["exportSubnetRoutesWithPublicIp"]?.Value<string>() ?? string.Empty;
                table[i + 1, 6] = networkpeering["importCustomRoutes"]?.Value<string>() ?? string.Empty;
                table[i + 1, 7] = networkpeering["importSubnetRoutesWithPublicIp"]?.Value<string>() ?? string.Empty;
                table[i + 1, 8] = networkpeering["state"]?.Value<string>() ?? string.Empty;
                table[i + 1, 9] = networkpeering["stateDetails"]?.Value<string>() ?? string.Empty;
            }

            ShowTable(table);

            return 0;
        }

        static string GetShortName(string name)
        {
            string project = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(name))) ?? string.Empty);
            string network = Path.GetFileName(name) ?? string.Empty;

            return $"{project}:{network}";
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
