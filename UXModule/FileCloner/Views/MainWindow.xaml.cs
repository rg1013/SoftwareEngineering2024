/******************************************************************************
 * Filename    = MainWindow.xaml.cs
 *
 * Author      = Sai Hemanth Reddy
 *
 * Project     = FileCloner
 *
 * Description = Code behind for MainWindow. Initializes and navigates to MainPage.
 *****************************************************************************/
using System.Windows;
using System.Windows.Controls;

namespace FileCloner.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Creates an instance of the main window and navigates to the MainPage.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Page mainPage = new MainPage();
            MainFrame.Navigate(mainPage);
        }
    }
}
