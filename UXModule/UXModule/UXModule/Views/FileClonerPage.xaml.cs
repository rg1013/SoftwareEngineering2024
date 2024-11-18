using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViewModel.FileClonerViewModel;

namespace UXModule.Views
{

    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class FileClonerPage : Page
    {
        /// <summary>
        /// Creates an instance of the main page.
        /// </summary>
        public FileClonerPage()
        {
            InitializeComponent();
            try
            {
                // Create the ViewModel and set as data context.
                ViewModel.FileClonerViewModel.MainPageViewModel viewModel = new();
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
                viewModel.SelectedNode = e.NewValue as ViewModel.FileClonerViewModel.Node;
            }
        }
    }
}
