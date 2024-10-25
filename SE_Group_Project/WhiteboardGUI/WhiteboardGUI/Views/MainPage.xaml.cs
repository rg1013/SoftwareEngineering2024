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
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace WhiteboardGUI
{
    public partial class MainPage : Page
    {
        private enum Tool { Pencil, Line, Circle, Text }
        private Tool currentTool = Tool.Pencil;
        private Point startPoint;
        private Line currentLine;
        private Ellipse currentEllipse;
        private Polyline currentPolyline;
        private TextBlock currentTextBlock;
        private TextBox currentTextBox;
        private List<UIElement> shapes = new List<UIElement>();
        private Brush selectedColor = Brushes.Black;
        private TcpClient client;

        private Shape selectedShape = null; // Keep track of the selected shape
        private Rectangle selectionBox; // To visually indicate selection
        public MainPage()
        {
            InitializeComponent();
            drawingCanvas.MouseDown += Canvas_MouseDown;
            drawingCanvas.MouseMove += Canvas_MouseMove;
            drawingCanvas.MouseUp += Canvas_MouseUp;
        }
        private void Text_Click(object sender, RoutedEventArgs e)
        {
            currentTool = Tool.Text;
        }
        private void Pencil_Click(object sender, RoutedEventArgs e) => currentTool = Tool.Pencil;
        private void Line_Click(object sender, RoutedEventArgs e) => currentTool = Tool.Line;
        private void Circle_Click(object sender, RoutedEventArgs e) => currentTool = Tool.Circle;


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
        private void TextInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && currentTextBox != null)
            {
                string inputText = currentTextBox.Text;
                if (!string.IsNullOrEmpty(inputText))
                {
                    // Create a new TextBlock to display the text
                    currentTextBlock = new TextBlock
                    {
                        Text = inputText,
                        Foreground = selectedColor,
                        FontSize = 16 // You can customize the font size
                    };

                    // Get the position of the TextBox and set the position of the TextBlock
                    double left = Canvas.GetLeft(currentTextBox);
                    double top = Canvas.GetTop(currentTextBox);
                    if (double.IsNaN(left) || double.IsNaN(top)) // Fallback if NaN
                    {
                        left = (double)currentTextBox.Margin.Left;
                        top = (double)currentTextBox.Margin.Top;
                    }

                    // Position the TextBlock the same as the TextBox
                    Canvas.SetLeft(currentTextBlock, left);
                    Canvas.SetTop(currentTextBlock, top);

                    // Add the TextBlock to the canvas and remove the TextBox
                    drawingCanvas.Children.Add(currentTextBlock);
                    drawingCanvas.Children.Remove(currentTextBox);
                    currentTextBox = null; // Clear the reference to the TextBox
                }
                e.Handled = true; // Prevent further handling of the Enter key
            }
        }


        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {


            startPoint = e.GetPosition(drawingCanvas);
            switch (currentTool)
            {
                case Tool.Text:
                    // Create a new TextBox at the clicked position
                    currentTextBox = new TextBox
                    {
                        Width = 100,
                        Height = 30,
                        Margin = new Thickness(startPoint.X, startPoint.Y, 0, 0)
                    };

                    // Add it to the canvas
                    drawingCanvas.Children.Add(currentTextBox);
                    currentTextBox.Focus(); // Set focus to the TextBox
                    currentTextBox.KeyDown += TextInput_KeyDown; // Subscribe to KeyDown event
                    break;
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
                        Points = polyline.Points.Select(p => new System.Drawing.Point((int)p.X, (int)p.Y)).ToList(),

                    };
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
            int port = 5000;
            Thread serverThread = new Thread(async () => await StartServer(port));
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        private void ClientCheckBox_Checked(object sender, RoutedEventArgs e)
        {
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
                _ = Task.Run(() => HandleClient(client, clients));
            }
        }

        private async Task HandleClient(TcpClient client, List<TcpClient> clients)
        {
            using NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);

            while (true)
            {
                string receivedData = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(receivedData)) continue;

                foreach (var otherClient in clients)
                {
                    if (otherClient != client)
                    {
                        StreamWriter clientWriter = new StreamWriter(otherClient.GetStream()) { AutoFlush = true };
                        await clientWriter.WriteLineAsync(receivedData);
                    }
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IShape shape = DeserializeShape(receivedData);
                    DrawReceivedShape(shape);
                });
            }
        }

        private async Task StartClient(int port)
        {
            client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, port);

            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);

            while (true)
            {
                string receivedData = await reader.ReadLineAsync();
                if (!string.IsNullOrEmpty(receivedData))
                {
                    IShape shape = DeserializeShape(receivedData);
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
                        polyline.Points.Add(new System.Windows.Point(point.X, point.Y));
                    }

                    drawingCanvas.Children.Add(polyline);
                    break;
            }
        }

        private string SerializeShape(IShape shape) => JsonConvert.SerializeObject(shape);
        private IShape DeserializeShape(string data) => JsonConvert.DeserializeObject<IShape>(data);
    }
}