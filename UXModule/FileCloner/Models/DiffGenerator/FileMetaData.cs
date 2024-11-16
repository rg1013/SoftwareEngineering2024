using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FileCloner.Models.DiffGenerator
{
    public class FileMetadata
    {
        [JsonPropertyName("Size")]
        public int? Size { get; set; }  // Nullable to allow folders (no size)

        [JsonPropertyName("LastModified")]
        public DateTime LastModified { get; set; }

        [JsonPropertyName("FullPath")]
        public string FullPath { get; set; }

        [JsonPropertyName("Children")]
        public Dictionary<string, FileMetadata> Children { get; set; } = new(); // Recursive children

        [JsonPropertyName("ADDRESS")]
        public string Address { get; set; }

        public string Color {  get; set; }


        // public string IP_Address {  get; set; }
    }
}
