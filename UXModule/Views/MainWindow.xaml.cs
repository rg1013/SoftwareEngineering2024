﻿using System.ComponentModel;
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
using UXModule.ViewModel;
using UXModule.Views;


namespace UXModule.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public event PropertyChangingEventHandler? PropertyChang;  

        private readonly MainPageViewModel _mainPageViewModel;
        public MainWindow()
        {
            InitializeComponent();

     

            _mainPageViewModel = new MainPageViewModel();

            this.DataContext = new WindowViewModel(this, _mainPageViewModel);

            MainFrame.Content = new LoginPage(_mainPageViewModel);
            this.Show();

            //this.Close();

        }
}
}