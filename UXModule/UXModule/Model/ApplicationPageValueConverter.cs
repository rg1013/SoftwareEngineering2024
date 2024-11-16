using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UXModule.Model;
using UXModule.ViewModel;
using UXModule.Views;

namespace UXModule
{
    /// <summary>
    /// Converts the <see cref="ApplicationPage"/> to an actual view/page
    /// </summary>
    public class ApplicationPageValueConverter : BaseValueConverter<ApplicationPageValueConverter>
    {
        /*public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var viewModel = parameter as MainPageViewModel;
            // Find the appropriate page based on the ApplicationPage enum value
            switch ((ApplicationPage)value)
            {
                case ApplicationPage.Homepage:
                    // Homepage doesn't require a ViewModel
                    return viewModel == null ? new LoginPage() : new Homepage(viewModel);
                    break;
                case ApplicationPage.Login:
                    // LoginPage doesn't require a ViewModel
                    return new LoginPage();
                    break;
                
                case ApplicationPage.ServerHomePage:
                    // Only pass ViewModel to pages that require it
                    return viewModel == null ? new LoginPage() : new ServerHomePage(viewModel);
                    break;
                case ApplicationPage.ClientHomePage:
                    // Only pass ViewModel to pages that require it
                    return viewModel == null ? new LoginPage() : new ClientHomePage(viewModel);
                    break;
                default:
                    return null;  // Return null if no match is found
            }
        }
*/
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Access MainPageViewModel via WindowViewModel
            var windowViewModel = (WindowViewModel)Application.Current.MainWindow.DataContext;
            var mainPageViewModel = windowViewModel.MainPageViewModel;

            switch ((ApplicationPage)value)
            {
                case ApplicationPage.Homepage:
                    return mainPageViewModel == null ? new LoginPage(mainPageViewModel) : new HomePage(mainPageViewModel);
                case ApplicationPage.Login:
                    return new LoginPage(mainPageViewModel);
                case ApplicationPage.ServerHomePage:
                    return mainPageViewModel == null ? new LoginPage(mainPageViewModel) : new ServerHomePage(mainPageViewModel);
                case ApplicationPage.ClientHomePage:
                    return mainPageViewModel == null ? new LoginPage(mainPageViewModel) : new ClientHomePage(mainPageViewModel);
                default:
                    return null;
            }
        }



        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
