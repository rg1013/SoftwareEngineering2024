/******************************************************************************
* Filename    = CloudViewModel.cs
*
* Author      = Karumudi Harika
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = CloudModel for handling functions between server and cloud
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Text.Json;
using Updater;
using SECloud;
using SECloud.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using static ViewModels.CloudViewModel;
using System.Reflection.Metadata;
using SECloud.Models;
using System.Diagnostics;
using Networking.Communication;
namespace ViewModels;

/// <summary>
/// 
/// </summary>
public class CloudViewModel
{
    private const string CloudFilesName = "CloudFiles";
    private readonly LogServiceViewModel _logServiceViewModel;
    private readonly ServerViewModel _serverViewModel;
    private readonly CloudService _cloudService;
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    public static string NameToSaveCloudFiles => CloudFilesName;

    public CloudViewModel(LogServiceViewModel logServiceViewModel, ServerViewModel serverViewModel)
    {
        ServiceProvider serviceProvider = new ServiceCollection()
        .AddLogging(builder => {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        })
        .AddHttpClient()
        .BuildServiceProvider();
        ILogger<CloudService> logger = Provider(serviceProvider);
        var httpClient = new HttpClient(); // Simplified for testing

        // Configuration values - replace with your actual values
        string baseUrl = BaseUrl();
        string team = Team();
        string sasToken = Token();

        // Create CloudService instance
        _cloudService = new CloudService(
            baseUrl,
            team,
            sasToken,
            httpClient,
            logger);

        _logServiceViewModel = logServiceViewModel;
        _serverViewModel = serverViewModel;
    }

    private static string Token()
    {
        return "sp=racwdli&st=2024-11-12T08:57:21Z&se=2024-11-12T18:57:21Z&spr=https&sv=2022-11-02&sr=c&sig=0EwLtsdRB%2FfYBYImEucoLROlQD6yBWLktzPHoKfm6ik%3D";
    }

    private static string Team()
    {
        return "updater";
    }

    private static string BaseUrl()
    {
        return "https://secloudapp-2024.azurewebsites.net/api";
    }

    private static ILogger<CloudService> Provider(ServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<ILogger<CloudService>>();
    }

    /// <summary>
    /// 
    /// </summary>
    public class FileData
    {
        public List<string>? Id { get; set; }
        public List<string>? Name { get; set; }
        public List<string>? Description { get; set; }
        public List<string>? FileVersion { get; set; }
        public List<string>? LastUpdate { get; set; }
        public List<string>? LastModified { get; set; }
        public List<string>? CreatorName { get; set; }
        public List<string>? CreatorMail { get; set; }
    }

    public async Task PerformCloudSync()
    {
        _logServiceViewModel.UpdateLogDetails("Cloud starting sync..");
        ServiceResponse<Stream> downloadResponse = await DownloadResponseMethod();
        string cloudData;
        if (downloadResponse == null || downloadResponse.Data == null)
        {
            cloudData = "[]";  // Represents an empty JSON array
        }
        else
        {
            using var reader = new StreamReader(downloadResponse.Data);
            cloudData = await CloudDataMethod(reader);
        }
        string? serverData = ServerDataMethod();


        //analogy to set difference between cloud and server
        List<FileData> onlyCloudFiles = CloudHasMoreData(cloudData, serverData);
        string jsonCloudFiles = JsonSerializer.Serialize(onlyCloudFiles, s_jsonOptions);


        //analogy to set differnce between server and cloud
        List<FileData> onlyServerFiles = OnlyServerFileMethod(cloudData, serverData);
        string jsonServerFiles = JsonSerializer.Serialize(onlyServerFiles, s_jsonOptions);


        //send these to server then server broadcast these files to all connected clients
        if (onlyCloudFiles.Count != 0)
        {
            _logServiceViewModel.UpdateLogDetails("Cloud has more Data than server. Sending JSON file to server....");
            UpdateServerWithCloudData(jsonCloudFiles);

            //BroadCastNewFiles(onlyCloudFiles);
        }

        //send the files to cloud using upload function.
        if (onlyServerFiles.Count != 0)
        {
            _logServiceViewModel.UpdateLogDetails("Server has more data than cloud. Sending JSON to cloud");
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonServerFiles));
            ServiceResponse<string> response = await ResponseMethod(stream);
            _logServiceViewModel.UpdateLogDetails(response.Data);
        }
    }

    private async Task<ServiceResponse<string>> ResponseMethod(MemoryStream stream)
    {
        return await _cloudService.UploadAsync("ServerFiles.json", stream, "file/json");
    }

    private static List<FileData> OnlyServerFileMethod(string cloudData, string? serverData)
    {
        return ServerHasMoreData(cloudData, serverData);
    }

    private string? ServerDataMethod()
    {
        return _serverViewModel.GetServerData() as string;

        // Assume this is a JSON string
    }

    private static async Task<string> CloudDataMethod(StreamReader reader)
    {
        return await reader.ReadToEndAsync();
    }

    private async Task<ServiceResponse<Stream>> DownloadResponseMethod()
    {
        return await _cloudService.DownloadAsync("ServerFiles.json");
    }


    public static List<FileData> RemoveNAEntries(List<FileData> files)
    {
        // Remove any entries where Name or Id or any other critical field contains "N/A" or is null.
        return files
            .Where(file => !file.Name.Contains("N/A") && file.Id != null && file.Id.All(id => id != "N/A"))
            .ToList();
    }

    public static List<FileData> ServerHasMoreData(string cloudData, string serverData)
    {
        // Deserialize the JSON strings into lists of FileData objects
        List<FileData>? cloudFiles = JsonSerializer.Deserialize<List<FileData>>(cloudData) ?? new List<FileData>();
        List<FileData>? serverFiles = JsonSerializer.Deserialize<List<FileData>>(serverData) ?? new List<FileData>();

        // Remove entries with "N/A" values from both cloud and server files
        cloudFiles = RemoveNAEntries(cloudFiles);
        serverFiles = RemoveNAEntries(serverFiles);

        // Initialize a new list to hold files that need to be added to the cloud
        List<FileData> newFilesToCloud = new List<FileData>();

        // Iterate over each file in the server list
        foreach (FileData serverFile in serverFiles)
        {
            // Check if the server file exists in the cloud
            foreach (var index in Enumerable.Range(0, serverFile.Name.Count))
            {
                bool fileExists = cloudFiles.Any(cloudFile =>
                    cloudFile.Name.Contains(serverFile.Name[index]) &&
                    cloudFile.FileVersion.Contains(serverFile.FileVersion[index]));

                // If the file does not exist in the cloud, add it to the list
                if (!fileExists)
                {
                    newFilesToCloud.Add(new FileData {
                        Id = new List<string> { serverFile.Id?[index] },
                        Name = new List<string> { serverFile.Name?[index] },
                        Description = new List<string> { serverFile.Description?[index] },
                        FileVersion = new List<string> { serverFile.FileVersion?[index] },
                        LastModified = new List<string> { serverFile.LastModified?[index] },
                        CreatorName = new List<string> { serverFile.CreatorName?[index] },
                        CreatorMail = new List<string> { serverFile.CreatorMail?[index] }
                    });
                }
            }
        }

        return newFilesToCloud;
    }


    public static List<FileData> CloudHasMoreData(string cloudData, string serverData)
    {
        // Deserialize cloud and server data into lists of FileData objects
        List<FileData>? cloudFiles = JsonSerializer.Deserialize<List<FileData>>(cloudData);
        List<FileData>? serverFiles = JsonSerializer.Deserialize<List<FileData>>(serverData);

        // Remove entries with "N/A" values from both cloud and server files
        cloudFiles = RemoveNAEntries(cloudFiles ?? new List<FileData>());
        serverFiles = RemoveNAEntries(serverFiles ?? new List<FileData>());

        // Ensure both lists are not null
        cloudFiles ??= [];
        serverFiles ??= [];

        foreach (FileData cloudFile in cloudFiles)
        {
            // Find indices in cloudFile.Name that should be removed because they exist in serverFiles
            var indicesToRemove = cloudFile.Name
                .Select((name, index) => new { name, index })
                .Where(item => serverFiles
                    .Any(serverFile => serverFile.Name.Contains(item.name) &&
                                       serverFile.FileVersion.Contains(cloudFile.FileVersion[item.index])))
                .Select(item => item.index)
                .ToList();

            // Remove the items at these indices across all fields in cloudFile
            foreach (int index in indicesToRemove.OrderByDescending(i => i))
            {
                // Remove data at the specified index in each field, if they are not null and have enough elements
                if (cloudFile.Name.Count > index)
                {
                    cloudFile.Name.RemoveAt(index);
                }

                if (cloudFile.Id?.Count > index)
                {
                    cloudFile.Id.RemoveAt(index);
                }

                if (cloudFile.Description?.Count > index)
                {
                    cloudFile.Description.RemoveAt(index);
                }

                if (cloudFile.FileVersion?.Count > index)
                {
                    cloudFile.FileVersion.RemoveAt(index);
                }

                if (cloudFile.LastModified?.Count > index)
                {
                    cloudFile.LastModified.RemoveAt(index);
                }

                if (cloudFile.CreatorName?.Count > index)
                {
                    cloudFile.CreatorName.RemoveAt(index);
                }
            }
        }

        // Return only cloud files with remaining unique entries
        return cloudFiles.Where(file => file.Name.Any()).ToList();
    }


    private void UpdateServerWithCloudData(string file)
    {
        SaveFileToServer(file);
        _logServiceViewModel.UpdateLogDetails("Server updated with each file new data from cloud");
    }

    private void SaveFileToServer(string file)
    {
        string destinationPath = SaveFileToServerMethod();
        File.WriteAllText(destinationPath, file);
        _logServiceViewModel.UpdateLogDetails("File successfully saved on server");

        string? content = Utils.ReadBinaryFile(destinationPath) ?? throw new Exception("Failed to read file");
        string? serializedContent = Utils.SerializeObject(content) ?? throw new Exception("Failed to serialize content");
        FileContent fileContentToSend = new FileContent("Cloud.json", serializedContent);

        List<FileContent> fileContentsToSend = new List<FileContent>();
        fileContentsToSend?.Add(fileContentToSend);

        DataPacket dataPacketToSend = new DataPacket(
                        DataPacket.PacketType.Broadcast,
                        new List<FileContent> { fileContentToSend }
                        );

        // Serialize packet
        string serializedPacket = Utils.SerializeObject(dataPacketToSend);
        _serverViewModel.GetServer().Broadcast(serializedPacket);
    }

    private static string SaveFileToServerMethod()
    {
        return Path.Combine(AppConstants.ToolsDirectory, $"{CloudFilesName}.json");
    }
}
