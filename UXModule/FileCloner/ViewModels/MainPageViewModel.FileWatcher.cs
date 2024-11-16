using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloner.ViewModels
{
    partial class MainPageViewModel : ViewModelBase
    {
        public void WatchFile(string path)
        {
            Trace.WriteLine($"Started watching at {path}");
            using FileSystemWatcher watcher = new();
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.Attributes |
            NotifyFilters.DirectoryName |
            NotifyFilters.FileName |
            NotifyFilters.LastWrite |
            NotifyFilters.Size;
            watcher.Filter = "*.*"; //only text files to be monitored

            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            watcher.EnableRaisingEvents = true;
            while (true) ;
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TreeGenerator(_rootDirectoryPath);
            });
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TreeGenerator(_rootDirectoryPath);
            });
        }
    }
}
