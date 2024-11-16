/******************************************************************************
* Filename    = ServerViewModel.cs
*
* Author      = Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = ViewModel for Server side logic
*****************************************************************************/

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Updater;

namespace ViewModels;
public class ServerViewModel : INotifyPropertyChanged
{
    private Server _server;
    private LogServiceViewModel _logServiceViewModel;
    private Mutex _mutex;

    public ServerViewModel(LogServiceViewModel logServiceViewModel)
    {
        _server = new Server();
        _logServiceViewModel = logServiceViewModel;
        Server.NotificationReceived += AddLogMessage;

        // Create a named mutex
        _mutex = new Mutex(false, "Global\\MyUniqueServerMutexName");
    }

    public bool CanStartServer()
    {
        return _mutex.WaitOne(0); // Check if the mutex can be acquired
    }

    public void StartServer(string ip, string port)
    {
        if (CanStartServer())
        {
            Task.Run(() => _server.Start(ip, port));
        }
        else
        {
            _logServiceViewModel.UpdateLogDetails("Server is already running on another instance.");
        }
    }

    public void StopServer()
    {
        _server.Stop();
        _mutex.ReleaseMutex();
    }

    private void AddLogMessage(string message)
    {
        _logServiceViewModel.UpdateLogDetails(message);
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
