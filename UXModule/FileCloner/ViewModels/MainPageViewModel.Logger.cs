using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloner.ViewModels
{
    partial class MainPageViewModel : ViewModelBase
    {
        /// <summary>
        /// Adds a message to the log with timestamp for UI display.
        /// </summary>
        private void UpdateLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogMessages.Insert(0, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]-  {message}");
                OnPropertyChanged(nameof(LogMessages));
            });
        }

    }
}
