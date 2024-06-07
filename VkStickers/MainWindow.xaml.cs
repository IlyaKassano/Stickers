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

namespace VkStickers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static IntPtr _lastActiveWindow;
        GlobalKeyboardHook _hooker;
        bool _show = false;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        const int SC_MINIMIZE = 0xF020;
        const int SC_RESTORE = 0xF120;
        const int WM_SYSCOMMAND = 0x0112;

        public MainWindow()
        {
            InitializeComponent();

            //_hooker = new GlobalKeyboardHook();
            //_hooker.KeyboardPressed += Hook_KeyboardPressed;

            Thread thread = new(() =>
            {
                while (true)
                {
                    var caretLocation = CaretLocator.Locate();
                    if (caretLocation == null)
                        continue;

                    if (caretLocation.Width == 1 && !_show)
                    {
                        Debug.WriteLine("Show");
                        _lastActiveWindow = GetForegroundWindow();
                        var handle = Process.GetCurrentProcess().MainWindowHandle;
                        PostMessage(handle, WM_SYSCOMMAND, SC_RESTORE, 0);
                        SetForegroundWindow(handle);
                        SetActiveWindow(_lastActiveWindow);

                        _show = caretLocation.Width != 0;
                    }
                    if (caretLocation.Width == 0 && _show)
                    {
                        var handle = Process.GetCurrentProcess().MainWindowHandle;
                        var fore = GetForegroundWindow();
                        if (handle != fore) {
                            Debug.WriteLine("PostMessage");
                            PostMessage(handle, WM_SYSCOMMAND, SC_MINIMIZE, 0);

                            _show = caretLocation.Width != 0;
                        }
                    }

                    Thread.Sleep(1000);
                }
            });
            thread.Start();
        }
         
        private void Hook_KeyboardPressed(object? sender, GlobalKeyboardHookEventArgs e)
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
                _lastActiveWindow = GetForegroundWindow();
                var handle = Process.GetCurrentProcess().MainWindowHandle;
                SetForegroundWindow(handle);
                e.Handled = true;
            }
        }

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

        /*Bitmap Transparent2Color(Bitmap bmp1, Color target)
        {
            Bitmap bmp2 = new Bitmap(bmp1.Width, bmp1.Height);
            Rectangle rect = new Rectangle(Point.Empty, bmp1.Size);
            using (Graphics G = Graphics.FromImage(bmp2))
            {
                G.Clear(target);
                G.DrawImageUnscaledAndClipped(bmp1, rect);
            }
            return bmp2;
        }*/

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var stackPnl = (StackPanel)btn.Content;
            var img = (Image)stackPnl.Children[0];

            var path = ((BitmapImage)img.Source).UriSource;
            var bitmapImg = new BitmapImage(path);
            var paths = new StringCollection
            {
                path.AbsolutePath
            };
            Clipboard.SetFileDropList(paths);

            SetForegroundWindow(_lastActiveWindow);
            SetActiveWindow(_lastActiveWindow);

            InputSimulator inputSimulator = new InputSimulator();
            try
            {
                inputSimulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_V);
                inputSimulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                Thread.Sleep(100);
                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            } catch (Exception ex)
            {
                Console.WriteLine("Алярм!: "+ex);
            }
            //Clipboard.SetImage(bitmapImg);
        }
    }
}