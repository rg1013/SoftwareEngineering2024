
using ScreenShare.Client;
using ScreenShare.Server;
using System.Drawing;
using System.IO.Compression;

namespace ScreenTest
{
    [Collection("Sequential")]
    public class ScreenshareStitcherTests
    {
        private byte[] CompressByteArray(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        /// Test to check if the decompression algorithm works properly.
      
        [Fact]
        public void TestDecompressByteArray()
        {
            byte[] arr = { (byte)'c', (byte)'h', (byte)'a', (byte)'r' };
            var compressed_arr = CompressByteArray(arr);

            var decompressed_arr = ScreenStitcher.DecompressByteArray(compressed_arr);
            Assert.Equal(arr, decompressed_arr);
        }


       
        
    }
}
