using FileCloner.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCloner.ViewModels
{
    partial class MainPageViewModel : ViewModelBase
    {
        /// <summary>
        /// Sends a request to initiate the file cloning process.
        /// </summary>
        private void SendRequest()
        {
            try
            {
                _client.SendRequest();
                IsSendRequestEnabled = false;
                IsSummarizeEnabled = true;
                IsStopCloningEnabled = true;
                MessageBox.Show("Request sent successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Generates and displays a summary of responses.
        /// </summary>
        private void SummarizeResponses()
        {
            SummaryGenerator.GenerateSummary();
            IsSummarizeEnabled = false;
            IsStartCloningEnabled = true;
            Dispatcher.Invoke(() => {
                MessageBox.Show("Summary is generated");
            });
            TreeGenerator(Constants.outputFilePath);
        }

        /// <summary>
        /// Starts the cloning process by creating files from selected items.
        /// </summary>
        private void StartCloning()
        {
            IsStartCloningEnabled = false;
            // Ensure that the directory for sender files exists
            Directory.CreateDirectory(Constants.senderFilesFolderPath);
            // clean the sender files folder before you start populating it with files
            _fileExplorerServiceProvider.CleanFolder(Constants.senderFilesFolderPath);

            foreach (var entry in _selectedFiles)
            {
                string key = entry.Key;
                List<string> value = entry.Value;
                string filePath = Path.Combine(Constants.senderFilesFolderPath, $"{key}.txt");

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    foreach (string filePathInValue in value)
                    {
                        writer.WriteLine(filePathInValue);
                    }
                }
            }
            _client.SendSummary();
        }

        /// <summary>
        /// Stops the cloning process with a warning prompt.
        /// </summary>
        private void StopCloning()
        {
            string message = "If you stop cloning, all incoming files related to the current session will be ignored.";
            string title = "WARNING";
            MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
            DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning);

            if (result == DialogResult.OK)
            {
                _client.StopCloning();
            }
            IsSendRequestEnabled = true;
            IsSummarizeEnabled = false;
            IsStartCloningEnabled = false;
            IsStopCloningEnabled = false;
        }

    }
}
