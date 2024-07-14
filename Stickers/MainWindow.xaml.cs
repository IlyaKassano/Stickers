using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
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
using Stickers.General;
using Stickers.ExternalProcesses;
using Stickers.StickerManagers;
using WindowsInput;
using WindowsInput.Native;
using static Stickers.General.WinApi;
using Rectangle = System.Drawing.Rectangle;
using System.Windows.Controls.Primitives;

namespace Stickers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Config _config;

        public static Color? StickerBackground = Colors.Transparent;

        string StickersDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Stickers");
        object _locker = new object();
        StickersLoader _stickerLoader;

        public MainWindow()
        {
            InitializeComponent();

            _config = ConfigLoader.GetConfig();
            _stickerLoader = new StickersLoader();

            //_hooker = new GlobalKeyboardHook();
            //_hooker.KeyboardPressed += Hook_KeyboardPressed;

            var thread = new Task(StartMonitoringCaret);
            thread.Start();
        }

        void StartMonitoringCaret()
        {
            while (true)
            {
                lock (_locker)
                {
                    CaretLocation? caretLocation = null;
                    try
                    {
                        caretLocation = CaretLocator.Locate();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }

                    if (caretLocation == null)
                        continue;

                    if (caretLocation.Width == 1)
                    {
                        if (!WindowManager.ShowWindow(caretLocation, _config.TargetProcesses) && WindowManager.Showing)
                        {
                            WindowManager.HideWindow(caretLocation);
                        }
                    }
                    if (caretLocation.Width == 0)
                    {
                        WindowManager.HideWindow(caretLocation);
                    }

                    Thread.Sleep(_config.CaretLocationInterval);
                }
            }
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
            this.Resources["SelectedItemBackgroundBrush"] = Brushes.Gray;

            Grid1.SnapsToDevicePixels = true;
            foreach (string dir in Directory.EnumerateDirectories(StickersDir))
            {
                _stickerLoader.LoadStickers(dir, TabControl1, SendSticker);
            }
        }

        private void SendSticker(object sender, RoutedEventArgs e)
        {
            lock (_locker)
            {
                var btn = (Button)sender;
                var stackPnl = (StackPanel)btn.Content;
                var img = (Image)stackPnl.Children[0];

                var path = ((BitmapImage)img.Source).UriSource.AbsolutePath;
                if (StickerBackground == null || System.IO.Path.GetExtension(path).Equals(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    var paths = new StringCollection
                    {
                        ((BitmapImage)img.Source).UriSource.AbsolutePath
                    };

                    Clipboard.SetFileDropList(paths);
                }
                else
                {
                    var src = ImageModifier.ReplaceTransparency((BitmapSource)img.Source, (Color)StickerBackground);
                    Clipboard.SetImage(src);
                }

                SetForegroundWindow(WindowManager.LastActiveWindow);
                SetActiveWindow(WindowManager.LastActiveWindow);

                InputSimulator inputSimulator = new InputSimulator();
                try
                {
                    inputSimulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                    inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_V);
                    inputSimulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                    Thread.Sleep(_config.SendEnterWaitTime);
                    inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error during Ctrl + V, Enter: " + ex);
                }
                //Clipboard.SetImage(bitmapImg);
            }
        }
    }
}