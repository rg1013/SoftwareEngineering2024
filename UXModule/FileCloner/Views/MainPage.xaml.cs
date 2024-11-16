/******************************************************************************
 * Filename    = MainPage.xaml.cs
 *
 * Author      = Sai Hemanth Reddy
 * 
 * Project     = FileCloner
 *
 * Description = Code behind for MainPage
 *****************************************************************************/
using System.Windows;
using System.Windows.Controls;
using FileCloner.ViewModels;

namespace FileCloner.Views
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        /// <summary>
        /// Creates an instance of the main page.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            try
            {
                // Create the ViewModel and set as data context.
                MainPageViewModel viewModel = new();
                DataContext = viewModel;
            }
            catch (Exception exception)
            {
                _ = MessageBox.Show(exception.Message);
                Application.Current.Shutdown();
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainPageViewModel viewModel)
            {
                viewModel.SelectedNode = e.NewValue as Node;
            }
        }
    }
}