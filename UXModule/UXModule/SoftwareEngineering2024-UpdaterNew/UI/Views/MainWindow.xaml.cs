/******************************************************************************
 * Filename    = MainWindow.xaml.cs
 *
 * Author      = Santhoshi Kumari Balaga
 *
 * Product     = UI
 * 
 * Project     = Views
 *
 * Description = Initialize a page for MainWindow
 *****************************************************************************/
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UI.ViewModels;
using ViewModels;
using System.Windows.Navigation;

namespace UI.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool isChatPanelOpen = false;
    public MainWindow()
    {
        InitializeComponent();

    }

    private void ChatTab_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (isChatPanelOpen)
        {
            // Hide the chat panel and make the main content take full width
            ChatPanel.Visibility = Visibility.Collapsed;
            MainGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star); // Full width for TabControl
            MainGrid.ColumnDefinitions[1].Width = new GridLength(0); // Collapse chat panel
            isChatPanelOpen = false;
        }
        else
        {
            // Show the chat panel and make the TabControl take 70% of the width
            ChatPanel.Visibility = Visibility.Visible;
            MainGrid.ColumnDefinitions[0].Width = new GridLength(8, GridUnitType.Star); // 70% width for TabControl
            MainGrid.ColumnDefinitions[1].Width = new GridLength(2, GridUnitType.Star); // 30% width for ChatPanel
            isChatPanelOpen = true;
        }

    }
}
