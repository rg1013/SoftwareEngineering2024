/******************************************************************************
 * Filename    = FileExplorerServiceProvider.cs
 *
 * Author      = Sai Hemanth Reddy
 *
 * Product     = PlexShare
 * 
 * Project     = FileCloner
 *
 * Description = Provides file system services such as loading directory path
 *               from config and generating input file by parsing directory structure.
 *****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace FileCloner.Models
{
    /// <summary>
    /// Service provider for file exploration, including config path loading and
    /// directory parsing for file structure generation.
    /// </summary>
    public class FileExplorerServiceProvider
    {
        public void CleanFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                foreach (var file in Directory.GetFiles(folderPath))
                {
                    File.Delete(file);
                }
            }
        }

        /// <summary>
        /// Generates an input file in JSON format representing the directory structure.
        /// </summary>
        /// <param name="sourceDirPath">The root directory to be parsed.</param>
        public void GenerateInputFile(string sourceDirPath)
        {
            try
            {
                string targetPath = Constants.inputFilePath;

                // Generate the directory structure dictionary
                var directoryStructure = ParseDirectory(sourceDirPath, sourceDirPath);

                // Wrap the structure in a root dictionary using the root directory name as the key
                var finalOutput = new Dictionary<string, object>
                {
                    [new DirectoryInfo(sourceDirPath).Name] = directoryStructure
                };

                // Serialize to JSON and write to the target path
                string jsonOutput = JsonSerializer.Serialize(finalOutput, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(targetPath, jsonOutput);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to generate input file", e);
            }
        }

        /// <summary>
        /// Recursively parses a directory and returns its structure as a dictionary.
        /// </summary>
        /// <param name="dirPath">Path of the directory to parse.</param>
        /// <param name="sourceDirPath">The root source directory path to calculate relative paths.</param>
        /// <returns>Dictionary representing the directory and its contents.</returns>
        private Dictionary<string, object> ParseDirectory(string dirPath, string sourceDirPath)
        {
            // Directory information object for accessing properties
            var dirInfo = new DirectoryInfo(dirPath);

            // Calculate the relative path of the directory from the sourceDirPath
            string relativePath = Path.GetRelativePath(sourceDirPath, dirPath);

            // Dictionary to store directory data, including full path, last modified date, and children
            var directoryData = new Dictionary<string, object>
            {
                ["LAST_MODIFIED"] = dirInfo.LastWriteTime,
                ["FULL_PATH"] = dirInfo.FullName,
                ["RELATIVE_PATH"] = relativePath,
                ["COLOR"] = "WHITE",
                ["ADDRESS"] = Constants.IPAddress,
                ["CHILDREN"] = new Dictionary<string, object>()
            };

            // Access children element in directoryData to store subdirectories and files
            var children = (Dictionary<string, object>)directoryData["CHILDREN"];

            // Recursively add subdirectories
            foreach (var directory in dirInfo.GetDirectories())
            {
                children[directory.Name] = ParseDirectory(directory.FullName, sourceDirPath);
            }

            // Add files to the dictionary, including size, full path, last modified date, and relative path
            foreach (var file in dirInfo.GetFiles())
            {
                children[file.Name] = new Dictionary<string, object>
                {
                    ["LAST_MODIFIED"] = file.LastWriteTime,
                    ["FULL_PATH"] = file.FullName,
                    ["RELATIVE_PATH"] = Path.GetRelativePath(sourceDirPath, file.FullName),
                    ["COLOR"] = "WHITE",
                    ["ADDRESS"] = Constants.IPAddress,
                    ["SIZE"] = file.Length,
                };
            }

            return directoryData;
        }
    }
}