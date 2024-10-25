using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Newtonsoft.Json;
using WhiteboardGUI.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;
//using ViewModel;

namespace WhiteboardGUI
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private enum Tool { Pencil, Line, Circle }
        private Tool currentTool = Tool.Pencil;
        private Point startPoint;
        private Line currentLine;
        private Ellipse currentEllipse;
        private Polyline currentPolyline;
        private List<Shape> shapes = new List<Shape>();
        private Brush selectedColor = Brushes.Black;
        private TcpClient client;
        /// <summary>
        /// Creates an instance of the main page.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            drawingCanvas.MouseDown += Canvas_MouseDown;
            drawingCanvas.MouseMove += Canvas_MouseMove;
            drawingCanvas.MouseUp += Canvas_MouseUp;
        }
        private void Pencil_Click(object sender, RoutedEventArgs e)
        {
            currentTool = Tool.Pencil;
        }

        private void Line_Click(object sender, RoutedEventArgs e)
        {
            currentTool = Tool.Line;
        }

        private void Circle_Click(object sender, RoutedEventArgs e)
        {
            currentTool = Tool.Circle;
        }

        private void ColorPicker_SelectionChanged(object sender, RoutedEventArgs e)
        {
            string selectedColorName = (colorPicker.SelectedItem as ComboBoxItem)?.Content.ToString();
            selectedColor = selectedColorName switch
            {
                "Red" => Brushes.Red,
                "Blue" => Brushes.Blue,
                "Green" => Brushes.Green,
                _ => Brushes.Black
            };
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(drawingCanvas);

            switch (currentTool)
            {
                case Tool.Pencil:
                    currentPolyline = new Polyline
                    {
                        Stroke = selectedColor,
                        StrokeThickness = 2,
                    };
                    currentPolyline.Points.Add(startPoint);
                    drawingCanvas.Children.Add(currentPolyline);
                    shapes.Add(currentPolyline);
                    break;
                case Tool.Line:
                    currentLine = new Line
                    {
                        Stroke = selectedColor,
                        StrokeThickness = 2,
                        X1 = startPoint.X,
                        Y1 = startPoint.Y
                    };
                    drawingCanvas.Children.Add(currentLine);
                    shapes.Add(currentLine);
                    break;
                case Tool.Circle:
                    currentEllipse = new Ellipse
                    {
                        Stroke = selectedColor,
                        StrokeThickness = 2
                    };
                    Canvas.SetLeft(currentEllipse, startPoint.X);
                    Canvas.SetTop(currentEllipse, startPoint.Y);
                    drawingCanvas.Children.Add(currentEllipse);
                    shapes.Add(currentEllipse);
                    break;
            }
        }
        private void ServerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Re-enable mouse movement
            IgnoreMouseMovement(false);
        }

        private void IgnoreMouseMovement(bool ignore)
        {
            if (ignore)
            {
                // Unsubscribe from mouse move events
                this.MouseMove -= Canvas_MouseMove; // Assuming MainPage_MouseMove is your mouse move handler
            }
            else
            {
                // Subscribe to mouse move events
                this.MouseMove += Canvas_MouseMove; // Re-attach the handler
            }
        }
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point endPoint = e.GetPosition(drawingCanvas);

                switch (currentTool)
                {
                    case Tool.Pencil:
                        currentPolyline?.Points.Add(endPoint);
                        break;
                    case Tool.Line:
                        if (currentLine != null)
                        {
                            currentLine.X2 = endPoint.X;
                            currentLine.Y2 = endPoint.Y;
                        }
                        break;
                    case Tool.Circle:
                        if (currentEllipse != null)
                        {
                            double radiusX = Math.Abs(endPoint.X - startPoint.X);
                            double radiusY = Math.Abs(endPoint.Y - startPoint.Y);
                            currentEllipse.Width = 2 * radiusX;
                            currentEllipse.Height = 2 * radiusY;
                            Canvas.SetLeft(currentEllipse, Math.Min(startPoint.X, endPoint.X));
                            Canvas.SetTop(currentEllipse, Math.Min(startPoint.Y, endPoint.Y));
                        }
                        break;
                }
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            currentLine = null;
            currentEllipse = null;
            currentPolyline = null;
            // Serialize and send the shape to the server
            if (shapes.LastOrDefault() is Shape lastShape)
            {
                IShape shapeToSend = ConvertToShapeObject(lastShape);
                string serializedShape = SerializeShape(shapeToSend);
                SendShapeToServer(serializedShape);
            }
        }
        private IShape ConvertToShapeObject(Shape shape)
        {
            switch (shape)
            {
                case Polyline polyline:
                    var scribbleShape = new ScribbleShape
                    {
                        Color = selectedColor.ToString(),
                        StrokeThickness = polyline.StrokeThickness,
                    };
                    foreach (var point in polyline.Points)
                    {
                        scribbleShape.Points.Add(WindowToDrawing(point)); // No conversion needed, uses System.Windows.Point
                    }
                    return scribbleShape;

                case Line line:
                    return new LineShape
                    {
                        StartX = line.X1,
                        StartY = line.Y1,
                        EndX = line.X2,
                        EndY = line.Y2,
                        Color = selectedColor.ToString(),
                        StrokeThickness = line.StrokeThickness
                    };

                case Ellipse ellipse:
                    return new CircleShape
                    {
                        CenterX = Canvas.GetLeft(ellipse),
                        CenterY = Canvas.GetTop(ellipse),
                        RadiusX = ellipse.Width / 2,
                        RadiusY = ellipse.Height / 2,
                        Color = selectedColor.ToString(),
                        StrokeThickness = ellipse.StrokeThickness
                    };

                default:
                    throw new NotSupportedException("Shape type not supported");
            }
        }

        // Send the serialized shape to the server
        private async void SendShapeToServer(string serializedShape)
        {
            if (client != null && client.Connected)
            {
                using NetworkStream stream = client.GetStream();
                using StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };
                await writer.WriteLineAsync(serializedShape);
            }
        }
        private void ServerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Start the server on a specified port
            int port = 5000; // Choose an appropriate port
                             //Task.Run(() => StartServer(port));
            Thread serverThread = new Thread(async () => await StartServer(port));
            serverThread.IsBackground = true; // Set the thread as a background thread
            serverThread.Start(); // Start the thread
            IgnoreMouseMovement(true);
        }

        private void ClientCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Ask for the server's port and connect as a client
            PortTextBox.Visibility = Visibility.Visible;
        }

        private void PortTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(PortTextBox.Text, out int port))
            {
                Task.Run(() => StartClient(port));
            }
        }
        private async Task StartServer(int port)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Server started on port {port}");

            List<TcpClient> clients = new List<TcpClient>();

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                clients.Add(client);

                // Handle receiving and broadcasting shapes
                await Task.Run(() => HandleClient(client, clients));
            }
        }

        private async Task HandleClient(TcpClient client, List<TcpClient> clients)
        {
            using NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

            while (true)
            {
                string receivedData = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(receivedData)) continue;

                // Broadcast to all clients
                foreach (var otherClient in clients)
                {
                    if (otherClient != client)
                    {
                        StreamWriter clientWriter = new StreamWriter(otherClient.GetStream()) { AutoFlush = true };
                        await clientWriter.WriteLineAsync(receivedData);
                    }
                }
            }
        }

        private async Task StartClient(int port)
        {
            client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, port);

            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

            while (true)
            {
                string receivedData = await reader.ReadLineAsync();
                if (!string.IsNullOrEmpty(receivedData))
                {
                    IShape shape = DeserializeShape(receivedData);

                    // Now add the shape to the canvas
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DrawReceivedShape(shape);
                    });
                }
            }
        }
        private void DrawReceivedShape(IShape shape)
        {
            switch (shape)
            {
                case CircleShape circle:
                    Ellipse ellipse = new Ellipse
                    {
                        Width = 2 * circle.RadiusX,
                        Height = 2 * circle.RadiusY,
                        Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(circle.Color)),
                        StrokeThickness = circle.StrokeThickness
                    };
                    Canvas.SetLeft(ellipse, circle.CenterX - circle.RadiusX);
                    Canvas.SetTop(ellipse, circle.CenterY - circle.RadiusY);
                    drawingCanvas.Children.Add(ellipse);
                    break;

                case LineShape line:
                    Line lineShape = new Line
                    {
                        X1 = line.StartX,
                        Y1 = line.StartY,
                        X2 = line.EndX,
                        Y2 = line.EndY,
                        Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(line.Color)),
                        StrokeThickness = line.StrokeThickness
                    };
                    drawingCanvas.Children.Add(lineShape);
                    break;

                case ScribbleShape scribble:
                    Polyline polyline = new Polyline
                    {
                        Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(scribble.Color)),
                        StrokeThickness = scribble.StrokeThickness
                    };
                    foreach (var point in scribble.Points)
                    {
                        System.Drawing.Point dp = (System.Drawing.Point)point;
                        polyline.Points.Add(new System.Windows.Point(dp.X, dp.Y));
                    }
                    drawingCanvas.Children.Add(polyline);
                    break;
            }
        }
        public string SerializeShape(IShape shape)
        {
            return JsonConvert.SerializeObject(shape);
        }

        public IShape DeserializeShape(string json)
        {
            var baseShape = JsonConvert.DeserializeObject<IShape>(json);

            return baseShape.ShapeType switch
            {
                "Circle" => JsonConvert.DeserializeObject<CircleShape>(json),
                "Line" => JsonConvert.DeserializeObject<LineShape>(json),
                "Scribble" => JsonConvert.DeserializeObject<ScribbleShape>(json),
                _ => throw new NotSupportedException("Shape type not supported")
            };
        }

        public object Convert(object value, Type targetType,
        object parameter, CultureInfo culture)
        {
            System.Drawing.Point dp = (System.Drawing.Point)value;
            return new System.Windows.Point(dp.X, dp.Y);
        }
        public System.Windows.Point DrawingToWindow(object value)
        {
            System.Drawing.Point dp = (System.Drawing.Point)value;
            return new System.Windows.Point(dp.X, dp.Y);
        }
        public System.Drawing.Point WindowToDrawing(object value)
        {
            System.Windows.Point wp = (System.Windows.Point)value;
            return new System.Drawing.Point((int)wp.X, (int)wp.Y);
        }
    }
}
