using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Networking;
using Networking.Communication;

namespace UXModule
{
    public enum Action
    {
        ClientUserConnected,
        ClientUserLeft,
        ServerSendUserID,
        ServerUserAdded,
        ServerUserLeft,
        ServerEnd,
        StartOfMeeting
    }

    public class Server_Dashboard : INotificationHandler
    {
        private ICommunicator _communicator;
        private string UserName { get; set; }
        private string UserEmail { get; set; }
        private string ProfilePictureUrl { get; set; } // Added this line

        public int total_user_count = 1;  // Start at 1 since server is user 1
        public int current_user_count = 1;

        public ObservableCollection<UserDetails> ServerUserList { get; private set; } = new ObservableCollection<UserDetails>();
        public ObservableCollection<UserDetails> TotalServerUserList { get; private set; } = new ObservableCollection<UserDetails>();


        public Server_Dashboard(ICommunicator communicator, string username, string useremail, string profilePictureUrl)
        {
            _communicator = communicator;
            _communicator.Subscribe("Dashboard", this, true);
            UserName = username;
            UserEmail = useremail;
            ProfilePictureUrl = profilePictureUrl;
            ServerUserList.CollectionChanged += (s, e) => OnPropertyChanged(nameof(ServerUserList));
        }

        public string Initialize()
        {
            // Create and add server user
            var server_user = new UserDetails
            {
                userName = UserName,
                userEmail = UserEmail,
                userId = "1",
                IsHost = true,
                ProfilePictureUrl = ProfilePictureUrl // Set profile picture URL
            };
            ServerUserList.Add(server_user);
            TotalServerUserList.Add(server_user);

            string server_credentials = "failure";
            while (server_credentials == "failure")
            {
                server_credentials = _communicator.Start();
            }
            if (server_credentials != "failure")
            {
                string[] parts = server_credentials.Split(':');
                string server_ip = parts[0];
                string server_port = parts[1];

                // Notify that server user is ready
                OnPropertyChanged(nameof(ServerUserList));
            }
            return server_credentials;
        }

        [JsonSerializable(typeof(DashboardDetails))]
        public class DashboardDetails
        {
            [JsonInclude]
            public UserDetails? User { get; set; }
            [JsonInclude]
            public bool IsConnected { get; set; }
            [JsonInclude]
            public Action Action { get; set; }
            [JsonInclude]
            public string? msg { get; set; }
        }


        public void BroadcastMessage(string message)
        {
            foreach (UserDetails user in ServerUserList)
            {
                SendMessage(user.userId, message);
            }
        }

        public void SendMessage(string clientIP, string message)
        {
            try
            {
                _communicator.Send(message, "Dashboard", clientIP);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        public void OnDataReceived(string message)
        {
            try
            {
                DashboardDetails? details = JsonSerializer.Deserialize<DashboardDetails>(message);
                if (details == null)
                {
                    Console.WriteLine("Error: Deserialized message is null");
                    return;
                }

                switch (details.Action)
                {
                    case Action.ClientUserConnected:
                        HandleUserConnected(details);
                        break;
                    case Action.ClientUserLeft:
                        HandleUserLeft(details);
                        break;
                    default:
                        Console.WriteLine($"Unknown action: {details.Action}");
                        break;
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing message: {ex.Message}");
            }
        }

        private void HandleUserConnected(DashboardDetails details)
        {
            Trace.WriteLine("[dashServer] received client info");
            if (details?.User != null)
            {
                var userToUpdate = ServerUserList.FirstOrDefault(u => u.userId == details.User.userId);
                var userInTotalList = TotalServerUserList.FirstOrDefault(u => u.userId == details.User.userId);

                if (userToUpdate == null)
                {
                    // Update user details
                    string newuserid = details.User.userId;

                    // Update the existing user's details
                    var newUserDetails = new UserDetails();
                    newUserDetails.userName = details.User.userName;
                    newUserDetails.userEmail = details.User.userEmail;
                    newUserDetails.IsHost = false;
                    newUserDetails.ProfilePictureUrl = details.User.ProfilePictureUrl; // Update profile picture URL
                    newUserDetails.userId = newuserid;

                    ServerUserList.Add(newUserDetails);

                    if (userInTotalList == null)
                    {
                        TotalServerUserList.Add(newUserDetails);
                    }

                    var listUsers = new List<UserDetails>(ServerUserList);
                    var jsonUserList = JsonSerializer.Serialize(listUsers);
                    SendMessage(newuserid, jsonUserList);

                    // Create message for new user joined
                    DashboardDetails dashboardMessage = new()
                    {
                        User = newUserDetails,
                        Action = Action.ServerUserAdded,
                        msg = "User " + newUserDetails.userName + " Joined"
                    };

                    // First send individual update
                    string joinMessage = JsonSerializer.Serialize(dashboardMessage);
                    BroadcastMessage(joinMessage);

                    // Trigger UI update
                    OnPropertyChanged(nameof(ServerUserList));
                }


            }
        }

        private void HandleUserLeft(DashboardDetails details)
        {
            if (details?.User != null)
            {
                var userToRemove = ServerUserList.FirstOrDefault(u => u.userId == details.User.userId);
                if (userToRemove != null)
                {
                    DashboardDetails dashboardMessage = new()
                    {
                        Action = Action.ServerUserLeft,
                        msg = "User with " + userToRemove.userName + " Left"
                    };
                    string json_message = JsonSerializer.Serialize(dashboardMessage);
                    ServerUserList.Remove(userToRemove);
                    OnPropertyChanged(nameof(ServerUserList));
                    BroadcastMessage(json_message);
                }
            }
        }

        private void HandleEndOfMeeting()
        {
            DashboardDetails dashboardMessage = new()
            {
                Action = Action.ServerEnd,
                msg = "Meeting Ended"
            };
            string json_message = JsonSerializer.Serialize(dashboardMessage);
            BroadcastMessage(json_message);
            ServerUserList.Clear();
        }

        private void HandleStartOfMeeting()
        {
            Trace.WriteLine("[Dash Server] Meeting started");
        }

        public bool ServerStop()
        {

            DashboardDetails dashboardMessage = new()
            {
                Action = Action.ServerEnd,
                msg = "Meeting Ended"
            };
            string json_message = JsonSerializer.Serialize(dashboardMessage);
            BroadcastMessage(json_message);
            ServerUserList.Clear();

            return true;
        }
            
            

        public void OnClientJoined(TcpClient socket)
        {
            total_user_count++;
            current_user_count++;

            string newUserId = total_user_count.ToString();

            // Create new user with temporary placeholder - don't set a name yet
            UserDetails details = new UserDetails
            {
                userId = newUserId,
                userEmail = "",
                IsHost = false
            };

            _communicator.AddClient(newUserId, socket);

            // Send only the userId to the new client
            DashboardDetails dashboardMessage = new DashboardDetails
            {
                User = new UserDetails { userId = newUserId },  // Only send userId
                Action = Action.ServerSendUserID,
                IsConnected = true
            };

            string json_message = JsonSerializer.Serialize(dashboardMessage);
            _communicator.Send(json_message, "Dashboard", newUserId);

            Trace.WriteLine($"Client joined with ID: {newUserId}");
        }

        public void OnClientLeft(string clientId)
        {
            var userLeaving = ServerUserList.FirstOrDefault(u => u.userId == clientId);

            Trace.WriteLine("[Dash server onclientleft ]" + clientId);
            if (userLeaving != null)
            {
                DashboardDetails dashboardMessage = new()
                {
                    Action = Action.ServerUserLeft,
                    msg = "User with " + userLeaving.userName + " left"
                };
                string json_message = JsonSerializer.Serialize(dashboardMessage);

                ServerUserList.Remove(userLeaving);
                _communicator.RemoveClient(clientId);

                OnPropertyChanged(nameof(ServerUserList));
                BroadcastMessage(json_message);
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
