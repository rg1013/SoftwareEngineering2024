using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Networking;
using Networking.Communication;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static UXModule.Server_Dashboard;


namespace UXModule
{
    [JsonSerializable(typeof(UserDetails))]
    public class UserDetails : INotifyPropertyChanged
    {
        private string? _userName;
        private string? _profilePictureUrl;

        [JsonInclude]
        public string? userName
        {
            get { return _userName; }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(userName));
                }
            }
        }

        [JsonInclude]
        public bool IsHost { get; set; }

        [JsonInclude]
        public string? userId { get; set; }

        [JsonInclude]
        public string? userEmail { get; set; }

        [JsonInclude]
        public string? ProfilePictureUrl
        {
            get { return _profilePictureUrl; }
            set
            {
                if (_profilePictureUrl != value)
                {
                    _profilePictureUrl = value;
                    OnPropertyChanged(nameof(ProfilePictureUrl));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Client_Dashboard : INotificationHandler, INotifyPropertyChanged
    {
        private ICommunicator _communicator;
        private string UserName { get; set; }
        private string UserEmail { get; set; }
        private string UserID { get; set; }

        private string UserProfileUrl { get; set; }



        public ObservableCollection<UserDetails> ClientUserList { get; set; } = new ObservableCollection<UserDetails>();

        public Client_Dashboard(ICommunicator communicator, string username, string useremail, string pictureURL)
        {
            _communicator = communicator;
            _communicator.Subscribe("Dashboard", this, isHighPriority: true);
            UserName = username;
            UserEmail = useremail;
            UserProfileUrl = pictureURL;
            UserID = string.Empty; // Initialize UserID
            ClientUserList.CollectionChanged += (s, e) => OnPropertyChanged(nameof(ClientUserList));
        }

        public string Initialize(string serverIP, string serverPort)
        {
            string server_response = _communicator.Start(serverIP, serverPort);
            return server_response;
        }

        public void SendMessage(string clientIP, string message)
        {
            string json_message = JsonSerializer.Serialize(message);
            try
            {
                _communicator.Send(json_message, "Dashboard", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        public void SendInfo(string username, string useremail)
        {
            DashboardDetails details = new DashboardDetails
            {
                User = new UserDetails
                {
                    userName = username,
                    userEmail = useremail,
                    ProfilePictureUrl = UserProfileUrl,
                    userId = UserID
                },
                Action = Action.ClientUserConnected
            };
            string json_message = JsonSerializer.Serialize(details);
            try
            {
                _communicator.Send(json_message, "Dashboard", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        public bool ClientLeft()
        {
            DashboardDetails details = new DashboardDetails
            {
                User = new UserDetails { userName = UserName },
                Action = Action.ClientUserLeft
            };
            string json_message = JsonSerializer.Serialize(details);
            try
            {
                _communicator.Send(json_message, "Dashboard", null);
                _communicator.Stop();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                return false;
            }
        }

        public void OnDataReceived(string message)
        {
            try
            {
                var details = JsonSerializer.Deserialize<DashboardDetails>(message);
                if (details == null)
                {
                    Console.WriteLine("Error: Deserialized message is null");
                    return;
                }

                switch (details.Action)
                {
                    case Action.ServerSendUserID:
                        HandleRecievedUserInfo(details);
                        break;
                    case Action.ServerUserAdded:
                        HandleUserConnected(details);
                        break;
                    case Action.ServerUserLeft:
                        HandleUserLeft(details);
                        break;
                    case Action.ServerEnd:
                        HandleEndOfMeeting();
                        break;
                    default:
                        Console.WriteLine($"Unknown action: {details.Action}");
                        break;
                }
            }
            catch (JsonException)
            {
                Trace.WriteLine("[DashClient] recieved list from server");
                var userList = JsonSerializer.Deserialize<List<UserDetails>>(message);
                if (userList != null)
                {
                    ClientUserList = new ObservableCollection<UserDetails>(userList);
                }
                else
                {
                    Console.WriteLine("Error: Deserialized user list is null");
                }
                OnPropertyChanged(nameof(ClientUserList));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing message: {ex.Message}");
            }
        }

        private void HandleRecievedUserInfo(DashboardDetails message)
        {
            if (message.User != null && message.User.userId != null)
            {
                UserID = message.User.userId;
                SendInfo(UserName, UserEmail);
            }
            else
            {
                Console.WriteLine("Error: Received user info is null");
            }
        }

        private void HandleUserConnected(DashboardDetails message)
        {
            if (message.User != null && message.User.userId != null)
            {
                UserDetails userData = message.User;
                string newuserid = userData.userId;

                Trace.WriteLine($"[Dash client] User Connected: {userData.userName}, {userData.ProfilePictureUrl}");

                if (ClientUserList.Count >= int.Parse(newuserid))
                {
                    ClientUserList[int.Parse(newuserid) - 1] = userData;
                }
                else
                {
                    ClientUserList.Add(userData);
                }
                OnPropertyChanged(nameof(ClientUserList));
            }
            else
            {
                Console.WriteLine("Error: Received user info is null");
            }
        }

        private void HandleUserLeft(DashboardDetails message)
        {
            if (message.User != null && message.User.userId != null)
            {

                string leftuserid = message.User.userId;

                foreach (var user in ClientUserList)
                {
                    if (user.userId == leftuserid)
                    {
                        ClientUserList.Remove(user);
                    }
                }
            }
            else
            {
                Console.WriteLine("Error: Received user info is null");
            }
        }

        private void HandleEndOfMeeting()
        {
            _communicator.Stop();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
