using FileCloner.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace FileCloner.ViewModels
{
    partial class MainPageViewModel : ViewModelBase
    {
        /// <summary>
        /// Generates the initial tree structure of the directory specified in RootDirectoryPath.
        /// </summary>
        private void TreeGenerator(string filePath)
        {
            try
            {
                // Clear any existing nodes in the tree and reset counters
                Tree.Clear();
                ResetCounts();
                if (filePath == Constants.outputFilePath)
                {
                    RootGenerator(filePath);
                    return;
                }

                // Generate input file representing the structure of the root directory
                _fileExplorerServiceProvider.GenerateInputFile(RootDirectoryPath);

                // Parse the input file and create tree nodes
                RootGenerator(Constants.inputFilePath);
            }
            catch (Exception e)
            {
                // Show error if tree generation fails
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Parses the JSON file representing directory structure and generates the root node.
        /// </summary>
        private void RootGenerator(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                var rootDictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);

                foreach (var root in rootDictionary)
                {
                    string color = root.Value.TryGetProperty("COLOR", out JsonElement colorProperty) ? colorProperty.GetString() : "";
                    // Create a root node for each directory
                    var rootNode = new Node
                    {
                        Name = root.Key,
                        IsFile = false,
                        IconPath = new Uri(Constants.folderIconPath, UriKind.Absolute),
                        Color = color,
                        RelativePath = root.Value.TryGetProperty("RELATIVE_PATH", out JsonElement relativePath) ? relativePath.GetString() : "",
                        IpAddress = root.Value.TryGetProperty("ADDRESS", out JsonElement address) ? address.GetString() : null,
                        FullFilePath = root.Value.TryGetProperty("FULL_PATH", out JsonElement fullFilePath) ? fullFilePath.GetString() : "PATH NOT GIVEN!",
                    };
                    // Add root node to the tree and increment folder count
                    Tree.Add(rootNode);
                    FolderCount++;
                    PopulateChildren(rootNode, (JsonElement)root.Value);  // Recursively populate child nodes
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Recursively populates child nodes for a given parent node.
        /// </summary>
        private void PopulateChildren(Node parentNode, JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("CHILDREN", out JsonElement childrenElement))
            {
                foreach (var child in childrenElement.EnumerateObject())
                {
                    bool isFile = child.Value.TryGetProperty("SIZE", out JsonElement sizeElement);
                    string color = child.Value.TryGetProperty("COLOR", out JsonElement colorProperty) ? colorProperty.GetString() : "";
                    var childNode = new Node
                    {
                        Name = child.Name,
                        Color = color,
                        IpAddress = child.Value.TryGetProperty("ADDRESS", out JsonElement address) ? address.GetString() : "localhost",
                        FullFilePath = child.Value.TryGetProperty("FULL_PATH", out JsonElement fullFilePath) ? fullFilePath.GetString() : "PATH NOT GIVEN!",
                        IsFile = isFile,
                        Size = isFile ? sizeElement.GetInt32() : 0,
                        Parent = parentNode,
                        RelativePath = child.Value.TryGetProperty("RELATIVE_PATH", out JsonElement relativePath) ? relativePath.GetString() : "",
                        IconPath = new Uri(isFile ? Constants.fileIconPath : Constants.folderIconPath, UriKind.Absolute)
                    };
                    if (color == "GREEN" || color == "RED")
                    {
                        childNode.IsChecked = true;
                        childNode.CheckBoxClick();
                    }
                    parentNode.Children.Add(childNode);

                    // Increment counters based on node type
                    if (isFile)
                    {
                        FileCount++;
                    }
                    else
                    {
                        FolderCount++;
                        PopulateChildren(childNode, child.Value); // Recurse if it's a folder
                    }
                }
            }
        }

    }
}
