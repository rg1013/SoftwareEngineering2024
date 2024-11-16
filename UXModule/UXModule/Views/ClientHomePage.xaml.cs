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
using UXModule.ViewModel;

namespace UXModule.Views
{
    /// <summary>
    /// Interaction logic for ClientHomePage.xaml
    /// </summary>
    public partial class ClientHomePage : Page
    {
        private readonly MainPageViewModel _viewModel;

        public ClientHomePage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool response = _viewModel.ClientLeaveSession();
            if (response)
            {
                this.NavigationService.GoBack();
            }
        }
    }
}
