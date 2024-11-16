using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileCloner.Models.DiffGenerator
{
    public class Root
    {
        [JsonExtensionData]
        public Dictionary<string, JsonElement> Files { get; set; } = new Dictionary<string, JsonElement>();

    }
}
