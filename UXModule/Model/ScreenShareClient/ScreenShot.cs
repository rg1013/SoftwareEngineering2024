﻿using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Resource = SharpDX.DXGI.Resource;

using System.DirectoryServices.ActiveDirectory;
using System.Windows.Media;
namespace UXModule.Model
{
     
    // Class contains the necessary functions for taking the screenshot of the current screen.
     
    public class Screenshot
    {
        private static readonly object _lock = new();
        private static Screenshot? instance;
        public Boolean CaptureActive { get; private set; }
        private Factory1? _factory1;
        private Adapter1? _adapter1;
        private Device? _device;
        private Output? _output;
        private Output1? _output1;
        private Int32 _width;
        private Int32 _height;
        private Rectangle _bounds;
        private Texture2DDescription _texture2DDescription;
        private Texture2D? _texture2D;
        private OutputDuplication? _outputDuplication;
        private Bitmap? _bitmap;

        private Int32 MakeScreenshot_LastDisplayIndexValue;
        private Int32 MakeScreenshot_LastAdapterIndexValue;

        protected Screenshot()
        {
            CaptureActive = false;
            InitializeVariables(0, 0, true);
        }

        public static Screenshot Instance()
        {
            lock (_lock)
            {
                if (instance == null)
                {
                    instance = new Screenshot();
                    Trace.WriteLine(Utils.GetDebugMessage("[Screenshare] Successfully created an instance of Screenshot.", withTimeStamp: true));
                }
                return instance;
            }
        }

         
        // Core function of class for taking the screenshot. Uses SharpDX for faster image capture.
       
        public Bitmap MakeScreenshot(Int32 displayIndex = 0, Int32 adapterIndex = 0, Int32 maxTimeout = 5000)
        {
            InitializeVariables(displayIndex, adapterIndex);
            Resource screenResource;
            // acquire the next frame, place it in a screenResource then convert it into a bitmap image
            _outputDuplication.TryAcquireNextFrame(maxTimeout, out _, out screenResource);
            if (screenResource == null)
                return null;
            Texture2D screenTexture2D = screenResource.QueryInterface<Texture2D>();
            _device.ImmediateContext.CopyResource(screenTexture2D, _texture2D);
            DataBox dataBox = _device.ImmediateContext.MapSubresource(_texture2D, 0, MapMode.Read, MapFlags.None);
            BitmapData bitmapData = _bitmap.LockBits(_bounds, ImageLockMode.WriteOnly, _bitmap.PixelFormat);
            IntPtr dataBoxPointer = dataBox.DataPointer;
            IntPtr bitmapDataPointer = bitmapData.Scan0;
            for (Int32 y = 0; y < _height; y++)
            {
                Utilities.CopyMemory(bitmapDataPointer, dataBoxPointer, _width * 4);
                dataBoxPointer = IntPtr.Add(dataBoxPointer, dataBox.RowPitch);
                bitmapDataPointer = IntPtr.Add(bitmapDataPointer, bitmapData.Stride);
            }
            // free up memory and resources after task is done
            _bitmap.UnlockBits(bitmapData);
            _device.ImmediateContext.UnmapSubresource(_texture2D, 0);
            _outputDuplication.ReleaseFrame();
            screenTexture2D.Dispose();
            screenResource.Dispose();
            // update the captured frame to a lower resolution (1080p -> 720p) and then return.
            Bitmap SmallBitmap = new Bitmap(_bitmap, _bitmap.Width * 2 / 3, _bitmap.Height * 2 / 3);
            return SmallBitmap;
        }

         
        // Initializes the members of the class.
         
        private void InitializeVariables(Int32 displayIndex, Int32 adapterIndex, Boolean forcedInitialization = false)
        {
            Boolean displayIndexChanged = MakeScreenshot_LastDisplayIndexValue != displayIndex;
            Boolean adapterIndexChanged = MakeScreenshot_LastAdapterIndexValue != adapterIndex;

            // reset all values in case of change in display, adapter or forced init.
            if (displayIndexChanged || adapterIndexChanged || forcedInitialization)
            {
                DisposeVariables();
                _factory1 = new Factory1();
                _adapter1 = _factory1.GetAdapter1(adapterIndex);
                _device = new Device(_adapter1);
                _output = _adapter1.GetOutput(displayIndex);
                _output1 = _output.QueryInterface<Output1>();
                _width = _output1.Description.DesktopBounds.Right - _output1.Description.DesktopBounds.Left;
                _height = _output1.Description.DesktopBounds.Bottom - _output1.Description.DesktopBounds.Top;
                _bounds = new Rectangle(Point.Empty, new Size(_width, _height));
                _texture2DDescription = new Texture2DDescription
                {
                    CpuAccessFlags = CpuAccessFlags.Read,
                    BindFlags = BindFlags.None,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = _width,
                    Height = _height,
                    OptionFlags = ResourceOptionFlags.None,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Staging
                };
                _texture2D = new Texture2D(_device, _texture2DDescription);
                _outputDuplication = _output1.DuplicateOutput(_device);
                _outputDuplication.TryAcquireNextFrame(1000, out _, out _);
                _outputDuplication.ReleaseFrame();
                _bitmap = new Bitmap(_width, _height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                MakeScreenshot_LastAdapterIndexValue = adapterIndex;
                MakeScreenshot_LastDisplayIndexValue = displayIndex;
            }
        }

         
        // Disposes the class memebers to avoid memory hogging.
         
        public void DisposeVariables()
        {
            _bitmap?.Dispose();
            _outputDuplication?.Dispose();
            _texture2D?.Dispose();
            _output1?.Dispose();
            _output?.Dispose();
            _device?.Dispose();
            _adapter1?.Dispose();
            _factory1?.Dispose();
            MakeScreenshot_LastAdapterIndexValue = -1;
            MakeScreenshot_LastDisplayIndexValue = -1;
        }
    }


}