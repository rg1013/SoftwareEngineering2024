using UXModule.ViewModel;
using UXModule.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace UXModule.Model
{
    
    /// Class contains implementation of the screen processing using threads (tasks)
    
    public class ScreenProcessor
    {
        // The queue in which the image will be enqueued after
        // processing it
        private readonly Queue<(string, List<PixelDifference>)> _processedFrame;

        // Processing task
        private Task? _processorTask;

        // Limits the number of frames in the queue
        public const short MaxQueueLength = 40;

        // The screen capturer object
        private readonly ScreenCapturer _capturer;

        // Current and the new resolutions 
        private Resolution _currentRes;
        private Resolution _newRes;
        public readonly object ResolutionLock;

        // Height and Width of the images captured by the capturer
        private int _capturedImageHeight;
        private int _capturedImageWidth;

        // Tokens added to be able to stop the thread execution
        private bool _cancellationToken;

        // Storing the previous frame
        Bitmap? prevImage;

        // Stores whether diff image is being sent for the first time or not
        private int _first_xor = 0;

        
        /// Called by ScreenshareClient.
        /// Initializes queue, oldRes, newRes, cancellation token and the previous image.
        
        public ScreenProcessor(ScreenCapturer Capturer)
        {
            _capturer = Capturer;
            _processedFrame = new Queue<(string, List<PixelDifference>)>();
            ResolutionLock = new();

            Trace.WriteLine(Utils.GetDebugMessage("Successfully created an instance of ScreenProcessor", withTimeStamp: true));
        }

        
        /// Pops and return the image from the queue. If there is no image in the queue then it waits for 
        /// the queue to become not empty
        
        public (string, List<PixelDifference>) GetFrame(ref bool cancellationToken)
        {
            while (true)
            {
                lock (_processedFrame)
                {
                    if (_processedFrame.Count != 0)
                    {
                        break;
                    }
                }

                if (cancellationToken)
                    return ("", null);
                Thread.Sleep(100);
            }
            lock (_processedFrame)
            {
                Trace.WriteLine(Utils.GetDebugMessage("Successfully sent frame", withTimeStamp: true));
                return _processedFrame.Dequeue();
            }
        }

        
        /// Returns the length of the processed image queue 
        
        public int GetProcessedFrameLength()
        {
            lock (_processedFrame)
            {
                Trace.WriteLine(Utils.GetDebugMessage("Successfully sent frame length", withTimeStamp: true));
                return _processedFrame.Count;
            }
        }

        
        /// In this function we go through every pixel of both the images and
        /// returns a bitmap image which has xor of all the coorosponding pixels
        
        public static unsafe List<PixelDifference>? Process(Bitmap curr, Bitmap prev)
        {


            // Lock the images and extract bitmap data
            BitmapData currData = curr.LockBits(new Rectangle(0, 0, curr.Width, curr.Height), ImageLockMode.ReadOnly, curr.PixelFormat);
            BitmapData prevData = prev.LockBits(new Rectangle(0, 0, prev.Width, prev.Height), ImageLockMode.ReadOnly, prev.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(curr.PixelFormat) / 8;
            int heightInPixels = currData.Height;
            int widthInBytes = currData.Width * bytesPerPixel;

            // Take pointer to both the image bytes
            byte* currPtr = (byte*)currData.Scan0;
            byte* prevPtr = (byte*)prevData.Scan0;

            // Initialize the list of pixel differences
            List<PixelDifference> changes = new List<PixelDifference>();
            int diffCount = 0;

            // Iterate over both images
            for (int y = 0; y < heightInPixels; y++)
            {
                byte* currRow = currPtr + (y * currData.Stride);
                byte* prevRow = prevPtr + (y * prevData.Stride);

                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    int oldBlue = prevRow[x];
                    int oldGreen = prevRow[x + 1];
                    int oldRed = prevRow[x + 2];
                    int oldAlpha = bytesPerPixel == 4 ? prevRow[x + 3] : 255; // Check for alpha channel in 32-bit images

                    int newBlue = currRow[x];
                    int newGreen = currRow[x + 1];
                    int newRed = currRow[x + 2];
                    int newAlpha = bytesPerPixel == 4 ? currRow[x + 3] : 255;

                    // Compare pixel values
                    if (oldBlue != newBlue || oldGreen != newGreen || oldRed != newRed || oldAlpha != newAlpha)
                    {
                        diffCount++;
                        if (diffCount > 1000)
                        {
                            curr.UnlockBits(currData);
                            prev.UnlockBits(prevData);
                            return null;
                        }
                        changes.Add(new PixelDifference((ushort)(x / bytesPerPixel), (ushort)y, (byte)newAlpha, (byte)newRed, (byte)newGreen, (byte)newBlue));
                    }
                }
            }

            // Unlock the bitmaps
            curr.UnlockBits(currData);
            prev.UnlockBits(prevData);

            return changes;
        }

        
        /// Main function which will run in loop and capture the image
        /// calculate the image bits differences and append it in the array
        
        private void Processing()
        {
            while (!_cancellationToken)
            {
                Bitmap? img = _capturer.GetImage(ref _cancellationToken);
                if (_cancellationToken)
                    break;

                Debug.Assert(img != null, Utils.GetDebugMessage("img is null"));
                (string, List<PixelDifference>) serialized_buffer = Compress(img);

                lock (_processedFrame)
                {
                    if (_processedFrame.Count < MaxQueueLength)
                    {
                        _processedFrame.Enqueue(serialized_buffer);
                    }
                    else
                    {
                        // Sleep for some time, if queue is filled 
                        while (_processedFrame.Count > MaxQueueLength / 2)
                            _processedFrame.Dequeue();
                    }
                }
                prevImage = img;
            }
        }

        
        /// Called by ScreenshareClient when the client starts screen sharing.
        /// Creates a task for the Processing function.
        
        public void StartProcessing()
        {
            // dropping one frame to set the previous image value
            _cancellationToken = false;
            _first_xor = 0;
            Bitmap? img = null;
            try
            {
                img = _capturer.GetImage(ref _cancellationToken);
                Debug.Assert(!_cancellationToken);
            }
            catch (Exception e)
            {
                Trace.WriteLine(Utils.GetDebugMessage($"Failed to cancel processor task: {e.Message}", withTimeStamp: true));
            }

            Debug.Assert(img != null, Utils.GetDebugMessage("img is null"));
            _capturedImageHeight = img.Height;
            _capturedImageWidth = img.Width;

            _newRes = new() { Height = _capturedImageHeight, Width = _capturedImageWidth };
            _currentRes = _newRes;
            prevImage = new Bitmap(_newRes.Width, _newRes.Height);

            Trace.WriteLine(Utils.GetDebugMessage("Previous image set and" +
                "going to start image processing", withTimeStamp: true));

            try
            {
                _processorTask = new Task(Processing);
                _processorTask.Start();
            }
            catch (OperationCanceledException e)
            {
                Trace.WriteLine(Utils.GetDebugMessage($"Processor task cancelled: {e.Message}", withTimeStamp: true));
            }
            catch (Exception e)
            {
                Trace.WriteLine(Utils.GetDebugMessage($"Failed to cancel processor task: {e.Message}", withTimeStamp: true));
            }
        }

        
        /// Called by ScreenshareClient when the client stops screen sharing
        /// kill the processor task and make the processor task variable null
        /// Empty the Queue.
        
        public void StopProcessing()
        {
            Debug.Assert(_processorTask != null, Utils.GetDebugMessage("_processorTask was null, cannot call cancel."));

            try
            {
                _cancellationToken = true;
                _processorTask.Wait();
            }
            catch (Exception e)
            {
                Trace.WriteLine(Utils.GetDebugMessage($"Failed to cancel processor task: {e.Message}", withTimeStamp: true));
            }

            Debug.Assert(_processedFrame != null, Utils.GetDebugMessage("_processedTask is found null"));
            _processedFrame.Clear();

            Trace.WriteLine(Utils.GetDebugMessage("Successfully stopped image processing", withTimeStamp: true));
        }

        
        /// Setting new resolution for sending the image. 
        
        /// <param name="res"> New resolution values </param>
        public void SetNewResolution(int windowCount)
        {
            Debug.Assert(windowCount != 0, Utils.GetDebugMessage("windowCount is found 0"));
            Resolution res = new()
            {
                Height = _capturedImageHeight / windowCount,
                Width = _capturedImageWidth / windowCount
            };
            // taking lock since newres is shared variable as it is
            // used even in Compress function
            lock (ResolutionLock)
            {
                _newRes = res;
            }
            Trace.WriteLine(Utils.GetDebugMessage("Successfully changed the rew resolution" +
                " variable", withTimeStamp: true));
        }

        
        /// Compressing the image byte array data using Deflated stream. It provides
        /// a lossless compression.
        
        /// <param name="data">Image data to be compressed</param>
        /// <returns>Compressed data</returns>
        public static byte[] CompressByteArray(byte[] data)
        {
            MemoryStream output = new();
            using (DeflateStream dstream = new(output, CompressionLevel.Fastest))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        
        /// Called by StartProcessing, if the image resolution has changed then set
        /// the new image resolution
        
        public (string, List<PixelDifference>) Compress(Bitmap img)
        {
            List<PixelDifference>? new_img = null;

            lock (ResolutionLock)
            {
                // if the new resolution and the current resolution are the same and the previous image is not
                // not null then process the image using the previous image
                if (prevImage != null && _newRes == _currentRes)
                {
                    //new_img = Process(img, prevImage);
                    new_img = null;
                }
                // else we need to update the current res with the new res and change the resolution
                // of captured image to the new resolution
                else if (_newRes != _currentRes)
                {
                    _currentRes = _newRes;
                }
            }
            // compressing image to the current  resolution values
            img = new Bitmap(img, _currentRes.Width, _currentRes.Height);
            new_img = null;

            // if no processing happened then send the whole image
            if (new_img == null)
            {
                MemoryStream ms = new();
                img.Save(ms, ImageFormat.Jpeg);
                Trace.WriteLine(Utils.GetDebugMessage($"Height: {img.Height}, width: {img.Width} ", withTimeStamp: false));
                var data = CompressByteArray(ms.ToArray());
                _first_xor = 0;
                return (Convert.ToBase64String(data), new_img);
            }
            // else if processing was done then compress the processed image
            else
            {
                if (_first_xor == 0)
                {
                    MemoryStream ms = new();
                    img.Save(ms, ImageFormat.Bmp);
                    Trace.WriteLine(Utils.GetDebugMessage($"Height: {img.Height}, width: {img.Width} ", withTimeStamp: false));
                    var data = CompressByteArray(ms.ToArray());
                    _first_xor = 1;
                    return (Convert.ToBase64String(data), new_img);
                }

                else
                {

                    return ("", new_img);
                }
            }
        }
    }
}
