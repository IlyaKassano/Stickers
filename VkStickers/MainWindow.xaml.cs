using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsInput;
using WindowsInput.Native;
using static VkStickers.WinApi;
using Rectangle = System.Drawing.Rectangle;

namespace VkStickers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //_hooker = new GlobalKeyboardHook();
            //_hooker.KeyboardPressed += Hook_KeyboardPressed;

            Thread thread = new(() =>
            {
                while (true)
                {
                    lock (_locker)
                    {
                        var caretLocation = CaretLocator.Locate();
                        if (caretLocation == null)
                            continue;

                        if (caretLocation.Width == 1)
                        {
                            WindowManager.ShowWindow(caretLocation);
                        }
                        if (caretLocation.Width == 0)
                        {
                            WindowManager.HideWindow(caretLocation);
                        }

                        Thread.Sleep(10);
                    }
                }
            });
            thread.Start();
        }

        /*private void Hook_KeyboardPressed(object? sender, GlobalKeyboardHookEventArgs e)
        {
            Debug.WriteLine(e.KeyboardData.VirtualCode);
            var point = new Point();
            Debug.WriteLine(GetCaretPos(out point));
            Debug.WriteLine("{0}, {1}", point.X, point.Y);


            if (e.KeyboardData.VirtualCode != 97)
                return;

            // seems, not needed in the life.
            //if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyDown &&
            //    e.KeyboardData.Flags == GlobalKeyboardHook.LlkhfAltdown)
            //{
            //    MessageBox.Show("Alt + Print Screen");j
            //    e.Handled = true;
            //}
            //else

            if (e.KeyboardState == KeyboardState.KeyDown)
            {
                WindowManager.LastActiveWindow = GetForegroundWindow();
                var handle = Process.GetCurrentProcess().MainWindowHandle;
                SetForegroundWindow(handle);
                e.Handled = true;
            }
        }*/

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Grid1.SnapsToDevicePixels = true;
            var images = new List<Image>();
            foreach (var path in Directory.GetFiles("Stickers"))
            {
                var image = new Image()
                {
                    Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(path))),
                    MaxWidth = 100,
                    MaxHeight = 100,
                    SnapsToDevicePixels = true,
                };
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Fant);
                images.Add(image);
            }

            int col = 0;
            int row = 0;
            Directory.CreateDirectory("test");
            foreach (var img in images)
            {
                StackPanel stackPnl = new StackPanel
                {
                    Background = Brushes.Transparent,
                    Orientation = Orientation.Horizontal,
                    SnapsToDevicePixels = true,
                };
                stackPnl.Children.Add(img);

                Button btn = new Button
                {
                    Content = stackPnl,
                    BorderThickness = new Thickness(0),
                    BorderBrush = Brushes.Transparent,
                    Background = new SolidColorBrush(new Color() {
                        B = 41,
                        R = 41,
                        G = 41,
                    }),
                    SnapsToDevicePixels = true,
                };

                Grid.SetRow(btn, row);
                Grid.SetColumn(btn, col);
                btn.Click += Btn_Click;

                Grid1.Children.Add(btn);

                if (col == Grid1.ColumnDefinitions.Count - 1)
                {
                    col = 0;
                    row++;
                    Grid1.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    continue;
                }
                col++;
            }
        }

        object _locker = new();
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            lock (_locker)
            {
                var btn = (Button)sender;
                var stackPnl = (StackPanel)btn.Content;
                var img = (Image)stackPnl.Children[0];

                var src = ((BitmapSource)img.Source).ReplaceTransparency(Color.FromRgb(0x1d, 0x1f, 0x24)); // TODO Pick color
                Clipboard.SetImage(src);

                SetForegroundWindow(WindowManager.LastActiveWindow);
                SetActiveWindow(WindowManager.LastActiveWindow);

                InputSimulator inputSimulator = new InputSimulator();
                try
                {
                    inputSimulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                    inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_V);
                    inputSimulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                    Thread.Sleep(100);
                    inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Алярм!: " + ex);
                }
                //Clipboard.SetImage(bitmapImg);
            }
        }
    }
}