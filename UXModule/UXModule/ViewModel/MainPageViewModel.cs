using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Networking;
using Networking.Communication;
using UXModule;
using System.Windows;

namespace UXModule.ViewModel
{
    public class MainPageViewModel : BaseViewModel
    {
        private ICommunicator _communicator;
        private Server_Dashboard _serverSessionManager;
        private Client_Dashboard _clientSessionManager;

        // UserDetailsList is bound to the UI to display the participant list
        private ObservableCollection<UserDetails> _userDetailsList = new ObservableCollection<UserDetails>();
        public ObservableCollection<UserDetails> UserDetailsList
        {
            get { return _userDetailsList; }
            set
            {
                if (_userDetailsList != value)
                {
                    _userDetailsList = value;
                    OnPropertyChanged(nameof(UserDetailsList));
                }
            }
        }

        public MainPageViewModel()
        {
            _serverPort = string.Empty;
            _serverIP = string.Empty;
            _userName = string.Empty;
        }

        private string _serverPort;
        private string _serverIP;
        private string _userName;

        public string? UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public string? UserEmail { get; private set; }

        public string? ServerIP
        {
            get { return _serverIP; }
            set
            {
                _serverIP = value;
                OnPropertyChanged(nameof(ServerIP));
            }
        }

        public string ServerPort
        {
            get { return _serverPort; }
            set
            {
                if (_serverPort != value)
                {
                    _serverPort = value;
                    OnPropertyChanged(nameof(ServerPort));
                }
            }
        }

        public bool IsHost { get; private set; } = false;

        // Method to create a session as host
        public string CreateSession(string username, string useremail, string profilePictureUrl)
        {
            IsHost = true;
            _communicator = CommunicationFactory.GetCommunicator(isClientSide: false);
            _serverSessionManager = new Server_Dashboard(_communicator, username, useremail, profilePictureUrl);
            _serverSessionManager.PropertyChanged += UpdateUserListOnPropertyChanged; // Subscribe to PropertyChanged
            string serverCredentials = _serverSessionManager.Initialize();

            if (serverCredentials != "failure")
            {
                string[] parts = serverCredentials.Split(':');
                ServerIP = parts[0];
                ServerPort = parts[1];
                return "success";
            }
            return "failure";
        }

        // Method to join a session as client
        public string JoinSession(string username, string useremail, string serverip, string serverport, string profilePictureUrl)
        {
            IsHost = false;
            _communicator = CommunicationFactory.GetCommunicator();
            _clientSessionManager = new Client_Dashboard(_communicator, username, useremail, profilePictureUrl);
            _clientSessionManager.PropertyChanged += UpdateUserListOnPropertyChanged; // Subscribe to PropertyChanged
            string serverResponse = _clientSessionManager.Initialize(serverip, serverport);

            if (serverResponse == "success")
            {
                UserName = username;
                UserEmail = useremail;
                UpdateUserListOnPropertyChanged(this, new PropertyChangedEventArgs(nameof(Client_Dashboard.ClientUserList)));
            }
            return serverResponse;
        }

        public bool ServerStopSession()
        {
            return _serverSessionManager.ServerStop();
        }

        public bool ClientLeaveSession()
        {
            return _clientSessionManager.ClientLeft();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        // Method to update UserDetailsList when Users in Server_Dashboard or Client_Dashboard changes
        private void UpdateUserListOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Server_Dashboard.ServerUserList) || e.PropertyName == nameof(Client_Dashboard.ClientUserList))
            {
                var users = _serverSessionManager?.ServerUserList ?? _clientSessionManager?.ClientUserList;

                if (users != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UserDetailsList.Clear();
                        foreach (var user in users)
                        {
                            UserDetailsList.Add(user);
                        }
                    });
                }


            }
        }
    }
    }
