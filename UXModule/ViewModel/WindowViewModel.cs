using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UXModule.Model;

namespace UXModule.ViewModel
{
    public class WindowViewModel : BaseViewModel
    {
        #region Private Members

        private Window mwindow;

        //Margin around the window that allows to Drop Shadow
        private int mouterMarginSize = 10;

        private int mwindowCornerSize = 10;

        #endregion

        #region Public Members
        public MainPageViewModel MainPageViewModel { get; private set; }
        public double WindowMinimumWidth { get; set; } = 400;
        public double WindowMaximumHeight { get; set; } = 400;

        /// <summary>
        /// True if the window should be borderless because it is docked or maximized
        /// </summary>
        public bool Borderless { get { return (mwindow.WindowState == WindowState.Maximized); } }

        /// <summary>
        /// The size of the resize border around the window
        /// </summary>
        public int Resizebordersize { get { return Borderless ? 0 : 6; } }
        public Thickness ResizeBorderThickness
        {
            get
            {
                return new Thickness(Resizebordersize + OuterMarginSize);
            }

        }

        public Thickness InnerContentPadding
        {
            get
            {
                return new Thickness(Resizebordersize);
            }

        }
        public int OuterMarginSize
        {
            get
            {
                return mwindow.WindowState == WindowState.Maximized ? 0 : mouterMarginSize;
            }
            set
            {
                mouterMarginSize = value;
            }
        }
        public Thickness OuterMarginThickness
        {
            get
            {
                return new Thickness(OuterMarginSize);
            }
        }
        public int CornerRadius
        {
            get
            {
                return mwindow.WindowState == WindowState.Maximized ? 0 : mwindowCornerSize;
            }
            set
            {
                mwindowCornerSize = value;
            }
        }
        public CornerRadius WindowCornerRadius
        {
            get
            {
                return new CornerRadius(CornerRadius);
            }
        }
        public int CaptionHeightSize { get; set; } = 42;

        public GridLength CaptionHeightGridLength
        {
            get
            {
                return new GridLength(CaptionHeightSize);
            }
        }

        //currentpage of application
        //public ApplicationPage CurrentPage { get; set; } = ApplicationPage.Homepage;

        #endregion

        #region Commands
        //The command to minimize the  Window
        public ICommand MinimizeCommand { get; set; }

        //The command to maximize the  Window
        public ICommand MaximizeCommand { get; set; }

        //The command Close the  Window
        public ICommand CloseCommand { get; set; }

        //The command to show Menu of the window
        public ICommand MenuCommand { get; set; }


        #endregion

        #region Constructor
        private ApplicationPage _currentPage = ApplicationPage.Login;
        public ApplicationPage CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }
        public WindowViewModel(Window Window, MainPageViewModel viewModel)
        {
            this.mwindow = Window;
            this.MainPageViewModel = viewModel;

            mwindow.StateChanged += (Sender, e) =>
            {
                OnPropertyChanged(nameof(CaptionHeightGridLength));
                OnPropertyChanged(nameof(ResizeBorderThickness));
                OnPropertyChanged(nameof(CornerRadius));
                OnPropertyChanged(nameof(WindowCornerRadius));

            };
            MinimizeCommand = new RelayCommand(_ => mwindow.WindowState = WindowState.Minimized);
            MaximizeCommand = new RelayCommand(_ => mwindow.WindowState ^= WindowState.Maximized);
            CloseCommand = new RelayCommand(_ => mwindow.Close());
            MenuCommand = new RelayCommand(_ => SystemCommands.ShowSystemMenu(mwindow, GetMousePosition()));
        }
        #endregion
        #region Private Helpers
        /// <summary>
        /// Gets the current mouse position on the screen
        /// </summary>
        /// <returns></returns>
        private Point GetMousePosition()
        {
            // Position of the mouse relative to the window
            var position = Mouse.GetPosition(mwindow);

            // Add the window position so its a "ToScreen"
            return new Point(position.X + mwindow.Left, position.Y + mwindow.Top);
        }

        #endregion

    }
}
