using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace injecenv
{
    internal class SaveData
    {
        static SaveData()
        {
            DataFileName = "rules.json";
            ExampleDataFileName = "example rules.json";
        }

        public SaveData()
        {
            Variables = new();
            Paths = new();
        }

        public static string DataFileName { get; }

        public static string ExampleDataFileName { get; }

        [JsonPropertyName("add-to-Path")]
        public List<string> Paths { get; init; }

        [JsonPropertyName("set-variables")]
        public Dictionary<string, string> Variables { get; init; }

        public static void CreateExample()
        {
            var file = new FileInfo(ExampleDataFileName);
            File.WriteAllText(file.FullName, JsonSerializer.Serialize(new SaveData
            {
                Paths = new()
                {
                    "../some/relative/path",
                    "/an/absolute/path/ignoring/disk/letter",
                    "C:/fully/qualified/absolute/path"
                },
                Variables = new()
                {
                    {
                        "JAVA_NOT_HOME",
                        "/custom/jdk/1.17"
                    },
                    {
                        "FOUR_value_(starts_with_':')",
                        ":4"
                    }
                }
            }, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }

        public static SaveData? Load()
        {
            var file = new FileInfo(DataFileName);
            if (file.Exists)
            {
                try
                {
                    return JsonSerializer.Deserialize<SaveData>(File.ReadAllText(file.FullName));
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}