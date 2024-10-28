using System.Configuration;
using System.Data;
using System.Windows;

namespace ScreenShare
{
     
    // Interaction logic for App.xaml
     
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            var app = new App();
            var mainWindowClient = new Window(); // Create a new window
            var screenShareClientPage = new ScreenShareServer(); // Create an instance of your ScreenShareClient page
            mainWindowClient.Content = screenShareClientPage; // Set the content of the window to be the ScreenShareClient page
            mainWindowClient.Title = "Screen Share Client"; // Set a title for the window (optional)

            app.Run(mainWindowClient); // Start the application with the mainWindow
        }
    }

}
