using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileCloner.Models.DiffGenerator
{
    public class DiffGenerator
    {
        private string _diffFilePath;
        private readonly object _syncLock = new();

        public DiffGenerator(string diffFilePath)
        {
            _diffFilePath = diffFilePath;
        }

        public void GenerateSummary(List<string> jsonFiles)
        {
            Dictionary<string, FileMetadata> allFiles = new();

            for (int i = 0; i < jsonFiles.Count; i++)
            {
                try
                {
                    string file = jsonFiles[i];



                    string ipAddress = Path.GetFileNameWithoutExtension(file);

                    // Console.WriteLine(ipAddress);
                    string text = File.ReadAllText(file);
                    Root jsonRoot = JsonSerializer.Deserialize<Root>(text);

                    foreach (var rootKey in jsonRoot.Files.Keys)
                    {
                        var jsonElement = jsonRoot.Files[rootKey]; // This is a JsonElement for root key (e.g., "A", "B")

                        // Deserialize JsonElement into FileMetadata
                        var rootFile = JsonSerializer.Deserialize<FileMetadata>(jsonElement.GetRawText());
                        rootFile.Address = ipAddress;


                        // Process the children of this root
                        if (rootFile?.Children != null)
                        {
                            if (i == 0)
                            {
                                ProcessChildren(rootFile.Children, allFiles, rootFile.Address, "White");
                            }
                            else
                            {
                                ProcessChildren(rootFile.Children, allFiles, rootFile.Address, "#90ee90");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading or deserializing file {jsonFiles[i]}: {ex.Message}");
                }
            }

            // Write all files to the output file
            WriteAllFilesToFile(allFiles, _diffFilePath);

        }

        // Recursive method to process children
        public void ProcessChildren(Dictionary<string, FileMetadata> children, Dictionary<string, FileMetadata> allFiles, string IPaddress, string color)
        {
            //  Console.WriteLine("here guys?");
            foreach (var (fileName, fileData) in children)
            {


                if (fileData.Children.Count > 0)
                {
                    // If this is a folder, recursively process its children
                    fileData.Address = IPaddress;
                    ProcessChildren(fileData.Children, allFiles, fileData.Address, color);
                }
                else
                {
                    // If this is a file, add or update it in the allFiles dictionary
                    if (allFiles.TryGetValue(fileName, out var existingFile))
                    {
                        if (fileData.LastModified > existingFile.LastModified)
                        {
                            if (fileData.Color != "#90ee90")
                            {

                                allFiles[fileName] = fileData;
                                allFiles[fileName].Color = "#ffff00";
                            }
                            else
                            {
                                allFiles[fileName] = fileData;
                                fileData.Color = "Green";
                            }

                        }
                    }
                    else
                    {
                        allFiles[fileName] = fileData;
                        allFiles[fileName].Color = color;
                    }
                    fileData.Address = IPaddress;
                }
            }
        }




        public void Add_to_Tree(Node node, List<string> fullPath, int index, FileMetadata fileMetaData, string pathSoFar)
        {
            if (index == fullPath.Count)
            {
                node.size = fileMetaData.Size;

            }
            else
            {
                node.size = 0;
            }

            if (index >= fullPath.Count)
            {
                return;
            }

            if (fileMetaData.Color == "#ffff00")
            {
                node.color = "#ffff00";
            }
            else if (fileMetaData.Color == "#90ee90" && node.color != "#ffff00")
            {
                node.color = "#90ee90";
            }




            if (node.children.ContainsKey(fullPath[index]))
            {

                node = node.children[fullPath[index]];
                Add_to_Tree(node, fullPath, index + 1, fileMetaData, node.fullPath);


            }
            else
            {

                node.children[fullPath[index]] = new Node(fullPath[index], fileMetaData);



                node.LastModified = node.LastModified > fileMetaData.LastModified
                    ? node.LastModified
                    : fileMetaData.LastModified;

                node = node.children[fullPath[index]];
                node.fullPath = pathSoFar + "\\" + fullPath[index];
                //   node.IpAddress = fileMetaData.Address;

                Add_to_Tree(node, fullPath, index + 1, fileMetaData, node.fullPath);

            }
        }



        public void WriteAllFilesToFile(Dictionary<string, FileMetadata> files, string outputFilePath)
        {
            // Creating a dictionary to store the final tree structure
            Dictionary<string, Node> Tree_address = new();

            lock (_syncLock)
            {
                using (StreamWriter writer = new StreamWriter(outputFilePath))
                {
                    foreach (var (fileName, fileData) in files)
                    {

                        List<string> result = fileData.FullPath.Split('/').ToList();
                        // Console.WriteLine(fileData.Address);


                        if (Tree_address.ContainsKey(result[0]))
                        {

                            Add_to_Tree(Tree_address[result[0]], result, 1, fileData, Tree_address[result[0]].fullPath);
                        }
                        else
                        {
                            // If not, create a new node and add it to the tree
                            Tree_address[result[0]] = new Node(result[0], fileData);
                            Tree_address[result[0]].fullPath = result[0];
                            Add_to_Tree(Tree_address[result[0]], result, 1, fileData, Tree_address[result[0]].fullPath);
                        }
                    }

                    // Start writing the tree structure to the file
                    writer.WriteLine("{");

                    bool firstNode = true;
                    foreach (KeyValuePair<string, Node> entry in Tree_address)
                    {
                        if (!firstNode)
                        {
                            writer.WriteLine(",");
                        }
                        firstNode = false;
                        WriteNode(writer, entry.Value, 0);
                    }


                    writer.WriteLine("}");
                }
            }

        }

        public void WriteNode(StreamWriter writer, Node node, int indentLevel)
        {
            // Create indentation for better readability
            string indent = new string(' ', indentLevel * 2);

            // Write the node as a JSON-like structure
            writer.WriteLine($"{indent}\"{node.node_name}\": {{");
            writer.WriteLine($"{indent}  \"LAST_MODIFIED\": \"{node.LastModified:MM-dd-yyyy}\",");
            writer.WriteLine($"{indent}  \"FULL_PATH\": \"{node.fullPath}/\",");
            writer.WriteLine($"{indent}  \"COLOR\": \"{node.color}\",");
            writer.WriteLine($"{indent}  \"ADDRESS\": \"{node.IpAddress}\",");
            writer.WriteLine($"{indent}  \"NODE_SIZE\": \"{node.size}\"");


            if (node.children.Count > 0)
            {
                // If there are children, recursively write them
                writer.WriteLine($"{indent}  \"CHILDREN\": {{");

                bool firstChild = true;
                foreach (var child in node.children)
                {
                    if (!firstChild)
                    {
                        writer.WriteLine(",");
                    }
                    firstChild = false;

                    WriteNode(writer, child.Value, indentLevel + 1);
                }

                writer.WriteLine($"{indent}  }}");
            }


            writer.WriteLine($"{indent}}}");
        }

    }
}
