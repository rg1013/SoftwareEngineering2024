using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloner.Models.DiffGenerator
{
    public class Node
    {
        public string node_name;
        public Dictionary<string, Node> children;
        public DateTime LastModified { get; set; }
        public string IpAddress { get; set; }
        public string color { get; set; }

        public string fullPath { get; set; }

        public int ?size { get; set; }

        public Node(string node_name, FileMetadata fileContents)
        {
            this.node_name = node_name;
            this.children = new Dictionary<string, Node>();
            this.LastModified = fileContents.LastModified;
            this.IpAddress = fileContents.Address;
            this.color = "White";


        }

    }
}
