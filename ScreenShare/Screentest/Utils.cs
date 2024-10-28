using ScreenShare;
using ScreenShare.Client;
using ScreenShare.Server;
using ScreenShare.Client;
using ScreenShare.Server;
using ScreenShare;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Text.Json;

namespace ScreenTest
{
  
    public static partial class Utils
    {
      
        private static Random RandomGenerator { get; } = new(DateTime.Now.Second);

       
        public static SharedClientScreen GetMockClient(ITimerManager server, bool isDebugging = false, int id = -1)
        {
            // Generate a random client Id if not given.
            string clientId = (id == -1) ? Utils.RandomGenerator.Next().ToString() : id.ToString();
            return new(clientId, Utils.RandomGenerator.Next().ToString(), server, isDebugging);
        }

       
        public static List<SharedClientScreen> GetMockClients(ITimerManager server, int count, bool isDebugging = false)
        {
            List<SharedClientScreen> list = new();
            for (int i = 2; i < count + 2; ++i)
            {
                list.Add(Utils.GetMockClient(server, isDebugging, i));
            }
            return list;
        }

       
        public static string GetMockRegisterPacket(string clientId, string clientName)
        {
            // Create a REGISTER packet with empty data and serialize it.
            DataPacket packet = new(clientId, clientName, nameof(ClientDataHeader.Register), "");
            return JsonSerializer.Serialize<DataPacket>(packet);
        }

     
        public static string GetMockDeregisterPacket(string clientId, string clientName)
        {
            // Create a DEREGISTER packet with no data and serialize it.
            DataPacket packet = new(clientId, clientName, nameof(ClientDataHeader.Deregister), "");
            return JsonSerializer.Serialize<DataPacket>(packet);
        }

      
        public static string GetMockConfirmationPacket(string clientId, string clientName)
        {
            // Create a CONFIRMATION packet with no data and serialize it.
            DataPacket packet = new(clientId, clientName, nameof(ClientDataHeader.Confirmation), "");
            return JsonSerializer.Serialize<DataPacket>(packet);
        }

      
        public static (string mockPacket, string mockImage) GetMockImagePacket(string id, string name)
        {
            // Create a mock received image.
            string mockImage = Utils.GetMockImage();
            DataPacket packet = new(id, name, nameof(ClientDataHeader.Image), mockImage);
            return (JsonSerializer.Serialize<DataPacket>(packet), mockImage);
        }

        public static Bitmap GetMockBitmap()
        {
            // Create a WebClient to get the image from the URL.
            using WebClient client = new();
            // Image stream read from the URL.
            using Stream stream = client.OpenRead($"https://source.unsplash.com/random/400x400?sig={Utils.RandomGenerator.Next() + 1}");
            Bitmap image = new(stream);
            return image;
        }

        public static string GetMockImage()
        {
            // Create a mock bitmap image and convert it to base-64 string.
            Bitmap img = Utils.GetMockBitmap();
            MemoryStream ms = new();
            img.Save(ms, ImageFormat.Bmp);
            var data = ScreenProcessor.CompressByteArray(ms.ToArray());
            return Convert.ToBase64String(data) + "1";
        }

     
        public static string RandomString(int length)
        {
            // Pick a random alphanumeric character every time.
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Utils.RandomGenerator.Next(s.Length)]).ToArray());
        }
    }
}
