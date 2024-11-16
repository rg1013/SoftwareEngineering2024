
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Threading.Channels;
using System.Threading.Tasks;
using UXModule.ViewModel;


namespace UXModule.Model
{
    
    /// Class contains implementation of the screen stitching using threads (tasks)
    
    public class ScreenStitcher
    {
        
        /// SharedClientScreen object.
        
        private readonly SharedClientScreen _sharedClientScreen;

        
        /// Thread to run stitcher.
        
        private Task? _stitchTask;

        
        /// A private variable to store old image.
        
        private Bitmap? _oldImage;

        
        /// Old resolution of the image.
        
        private Resolution? _resolution;

        
        /// A count to maintain the number of image stitched. Used in
        /// trace logs.
        
        private int _cnt = 0;

        
        /// Constructor for ScreenSticher.
        
        public ScreenStitcher(SharedClientScreen scs)
        {
            _oldImage = null;
            _stitchTask = null;
            _resolution = null;
            _sharedClientScreen = scs;
        }

        
        /// Uses the 'diff' image curr and the previous image to find the
        /// current image. This method is used when the client sends a diff
        /// instead of entire image to server.
        

        public static unsafe Bitmap Process(List<PixelDifference> changes, Bitmap prev)
        {
            //BitmapData currData = curr.LockBits(new Rectangle(0, 0, curr.Width, curr.Height), ImageLockMode.ReadWrite, curr.PixelFormat);

            foreach (var change in changes)
            {
                // Ensure the change is within the bounds of the image
                if (change.X >= 0 && change.X < prev.Width && change.Y >= 0 && change.Y < prev.Height)
                {
                    // Create a new color from the change data
                    Color newColor = Color.FromArgb(change.Alpha, change.Red, change.Green, change.Blue);

                    // Set the new color at the specified pixel location
                    prev.SetPixel(change.X, change.Y, newColor);
                }
            }
            return prev;
        }

        
        /// Method to decompress a byte array compressed by processor.
        

        public static byte[] DecompressByteArray(byte[] data)
        {
            MemoryStream input = new(data);
            MemoryStream output = new();
            using (DeflateStream dstream = new(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

        
        /// Creates(if not exist) and start the task `_stitchTask`
        /// Will read the image using `_sharedClientScreen.GetFrame`
        /// and puts the final image using `_sharedClientScreen.PutFinalImage`.
        
        /// <param name="taskId">
        /// Id of the task in which this function is called.
        /// </param>
        public void StartStitching(int taskId)
        {
            if (_stitchTask != null) return;

            _stitchTask = new Task(() =>
            {
                while (taskId == _sharedClientScreen.TaskId)
                {
                    (string, List<PixelDifference>)? newFrame = _sharedClientScreen.GetImage(taskId);

                    if (taskId != _sharedClientScreen.TaskId) break;

                    if (newFrame == null)
                    {
                        Trace.WriteLine(Utils.GetDebugMessage("New frame returned by _sharedClientScreen is null.", withTimeStamp: true));
                        continue;
                    }

                    Bitmap stichedImage = Stitch(_oldImage, ((string, List<PixelDifference>))newFrame);
                    Trace.WriteLine(Utils.GetDebugMessage($"STITCHED image from client {_cnt++}", withTimeStamp: true));
                    _oldImage = stichedImage;
                    _sharedClientScreen.PutFinalImage(stichedImage, taskId);

                }
            });

            _stitchTask?.Start();

            Trace.WriteLine(Utils.GetDebugMessage($"Successfully created the stitching task with id {taskId} for the client with id {_sharedClientScreen.Id}", withTimeStamp: true));
        }

        
        /// Method to stop the stitcher task.
        
        public void StopStitching()
        {
            if (_stitchTask == null) return;

            Task previousStitchTask = _stitchTask;
            _stitchTask = null;

            try
            {
                previousStitchTask?.Wait();
            }
            catch (Exception e)
            {
                Trace.WriteLine(Utils.GetDebugMessage($"Failed to start the stitching: {e.Message}", withTimeStamp: true));
            }

            Trace.WriteLine(Utils.GetDebugMessage($"Successfully stopped the processing task for the client with id {_sharedClientScreen.Id}", withTimeStamp: true));
        }

        
        /// Function to stitch new frame over old image. If the data sent from client
        /// has '1' in front then it is a complete image and hence the Process function
        /// is not used. Otherwise, the data will have a '0' in front of it and we will
        /// have to compute the XOR (using process function) in order to find the current
        /// image.
        
        private Bitmap Stitch(Bitmap? oldImage, (string, List<PixelDifference>) newFrame)
        {


            List<PixelDifference> isCompleteFrame = newFrame.Item2;
            // newFrame = newFrame.Remove(newFrame.Length - 1);

            if (isCompleteFrame != null)
            {
                Trace.WriteLine(Utils.GetDebugMessage($"Height: {oldImage.Height}, width: {oldImage.Width} ", withTimeStamp: false));
                oldImage = new Bitmap(oldImage, 1080, 720);
                Trace.WriteLine(Utils.GetDebugMessage($"Count: {newFrame.Item2.Count} ", withTimeStamp: false));
                Trace.WriteLine(Utils.GetDebugMessage($"eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee", withTimeStamp: false));
                oldImage = Process(newFrame.Item2, oldImage);
                return oldImage;
            }
            byte[]? deser;
            Trace.WriteLine(Utils.GetDebugMessage($"oooooooooooooooooooooooooooooooooooooooooooooo", withTimeStamp: false));
            deser = Convert.FromBase64String(newFrame.Item1);
            deser = DecompressByteArray(deser);

            MemoryStream ms = new(deser);
            var xor_bitmap = new Bitmap(ms);
            var newResolution = new Resolution() { Height = xor_bitmap.Height, Width = xor_bitmap.Width };


            if (oldImage == null || newResolution != _resolution)
            {
                oldImage = new Bitmap(newResolution.Width, newResolution.Height);
            }



            oldImage = xor_bitmap;

            _resolution = newResolution;
            return oldImage;
        }
    }
}
