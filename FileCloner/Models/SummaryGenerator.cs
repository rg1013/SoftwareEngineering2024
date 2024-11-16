using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace FileCloner.Models
{
    public class SummaryGenerator
    {
        // Dictionary to hold the summary data with relative paths as keys and metadata as values
        private static Dictionary<string, Dictionary<string, object>> _summary = new();

        // Generates the summary file at Constants.outputFilePath
        public static void GenerateSummary()
        {
            try
            {
                // Clear the dictionary for a fresh start
                _summary.Clear();

                // Parse the input file to add entries into the summary
                ParseInputFile(Constants.inputFilePath);

                // Parse files received from other systems and add entries into the summary
                var receivedFiles = Directory.GetFiles(Constants.receivedFilesFolderPath, "*.json");
                foreach (var file in receivedFiles)
                {
                    ParseReceivedFile(file);
                }

                // Write the resulting dictionary to the output file in JSON format
                WriteSummaryToFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate summary: {ex.Message}");
            }
        }

        private static void ParseInputFile(string inputFilePath)
        {
            // Deserialize the input file and add its entries into the _summary dictionary
            using (var stream = File.OpenRead(inputFilePath))
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(stream);
                if (data != null)
                {
                    ProcessDirectoryData(data, "WHITE");
                }
            }
        }

        private static void ParseReceivedFile(string filePath)
        {
            // Deserialize the received file and add its entries into the _summary dictionary
            using (var stream = File.OpenRead(filePath))
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(stream);
                if (data != null)
                {
                    ProcessDirectoryData(data, "GREEN");
                }
            }
        }

        private static void ProcessDirectoryData(Dictionary<string, object> data, string defaultColor)
        {
            foreach (var item in data)
            {
                if (item.Value is JsonElement element)
                {
                    TraverseAndAddEntries(element, defaultColor, item.Key);
                }
            }
        }

        private static void TraverseAndAddEntries(JsonElement element, string defaultColor, string Key, string relativePath = "")
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                // Check if this is a file (has "SIZE") and retrieve relative path
                if (element.TryGetProperty("SIZE", out var sizeProperty))
                {
                    if (element.TryGetProperty("RELATIVE_PATH", out var relPathProp))
                    {
                        relativePath = relPathProp.GetString() ?? string.Empty;
                    }

                    // Parse file metadata and add it to the dictionary
                    if (!_summary.ContainsKey(relativePath))
                    {
                        var fileData = ParseFileMetadata(element, defaultColor, Key);
                        _summary[relativePath] = fileData;
                    }
                    else
                    {
                        UpdateEntryWithNewData(element, relativePath);
                    }
                }

                // If there are children, process them recursively
                if (element.TryGetProperty("CHILDREN", out var children))
                {
                    foreach (var child in children.EnumerateObject())
                    {
                        TraverseAndAddEntries(child.Value, defaultColor, child.Name, relativePath + "\\" + child.Name);
                    }
                }
            }
        }

        private static Dictionary<string, object> ParseFileMetadata(JsonElement element, string color, string name)
        {
            var metadata = new Dictionary<string, object>();

            foreach (var property in element.EnumerateObject())
            {
                metadata[property.Name] = property.Value.ValueKind switch
                {
                    JsonValueKind.String => property.Value.GetString(),
                    JsonValueKind.Number => property.Value.GetInt64(),
                    _ => null
                };
            }

            metadata["NAME"] = name;
            metadata["COLOR"] = color;

            return metadata;
        }

        private static void UpdateEntryWithNewData(JsonElement element, string relativePath)
        {
            if (_summary.TryGetValue(relativePath, out var existingMetadata))
            {
                if (element.TryGetProperty("LAST_MODIFIED", out var newLastModifiedProp) && existingMetadata.ContainsKey("LAST_MODIFIED"))
                {
                    DateTime newTimestamp = DateTime.Parse(newLastModifiedProp.GetString()!, null, DateTimeStyles.AdjustToUniversal);
                    DateTime existingTimestamp = DateTime.Parse(existingMetadata["LAST_MODIFIED"].ToString(), null, DateTimeStyles.AdjustToUniversal);

                    if (newTimestamp > existingTimestamp)
                    {
                        string previousColor = existingMetadata["COLOR"].ToString();
                        string newColor = previousColor == "GREEN" ? "GREEN" : "RED";

                        var newMetadata = ParseFileMetadata(element, newColor, relativePath);
                        _summary[relativePath] = newMetadata;
                    }
                }
            }
        }

        private static void WriteSummaryToFile()
        {
            // Create a dictionary to group files by their ADDRESS, with a "CHILDREN" dictionary for each address
            var groupedByAddress = new Dictionary<string, Dictionary<string, object>>();

            foreach (var entry in _summary)
            {
                var metadata = entry.Value;

                if (metadata.TryGetValue("ADDRESS", out var address) && address is string addressStr &&
                    metadata.TryGetValue("NAME", out var name) && name is string nameStr)
                {
                    // Initialize the address entry with a "CHILDREN" dictionary if it doesn't exist
                    if (!groupedByAddress.ContainsKey(addressStr))
                    {
                        groupedByAddress[addressStr] = new Dictionary<string, object>
                        {
                            ["CHILDREN"] = new Dictionary<string, Dictionary<string, object>>()
                        };
                    }

                    // Create a copy of metadata without the NAME field
                    var metadataCopy = new Dictionary<string, object>(metadata);
                    metadataCopy.Remove("NAME");

                    // Add this entry under the "CHILDREN" section for the current address
                    var children = (Dictionary<string, Dictionary<string, object>>)groupedByAddress[addressStr]["CHILDREN"];
                    children[nameStr] = metadataCopy;
                }
            }

            // Write the grouped dictionary to the output file in JSON format
            using (var stream = File.Create(Constants.outputFilePath))
            {
                JsonSerializer.Serialize(stream, groupedByAddress, new JsonSerializerOptions { WriteIndented = true });
            }
        }


    }
}
