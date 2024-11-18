/******************************************************************************
 * Filename    = DashboardPage.xaml.cs
 *
 * Author      = Santhoshi Kumari Balaga
 *
 * Product     = UI
 * 
 * Project     = Views
 *
 * Description = Initialize a page for Dashboard
 *****************************************************************************/
using System.Windows;
using System.Windows.Controls;
using UI.ViewModels;

namespace UI.Views;

public partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();

        this.DataContext = new DashboardViewModel();
    }

    
}
