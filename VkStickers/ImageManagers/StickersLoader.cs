using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using WindowsInput.Native;
using WindowsInput;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using VkStickers.General;

namespace VkStickers.StickerManagers
{
    internal class StickersLoader
    {
        internal StickersLoader()
        {
        }

        internal void LoadStickers(string folder, TabControl tabControl, RoutedEventHandler stickerClickHandler)
        {
            int col = 0;
            int row = 0;


            var images = GetImages(folder);
            var folderName = folder.Split(new string[] { "\\", "/" }, StringSplitOptions.None).Last();
            var grid = CreateGridForStickers();
            var tabItem = new TabItem()
            {
                Header = folderName,
                Content = grid,
                //BorderBrush = Brushes.Transparent,
                //Background = new SolidColorBrush(MyColors.Background),
                Foreground = new SolidColorBrush(Colors.Black),
            };


            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
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
                    Background = new SolidColorBrush(MyColors.Background),
                    SnapsToDevicePixels = true,
                };

                Grid.SetRow(btn, row);
                Grid.SetColumn(btn, col);
                btn.Click += stickerClickHandler;

                grid.Children.Add(btn);

                if (col == grid.ColumnDefinitions.Count - 1)
                {
                    col = 0;
                    row++;
                    grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    continue;
                }
                col++;
            }

            tabControl.Items.Add(tabItem);
        }

        Grid CreateGridForStickers()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            return grid;
        }

        List<Image> GetImages(string folder)
        {
            var images = new List<Image>();

            foreach (var path in Directory.GetFiles(folder))
            {
                var image = new Image()
                {
                    Source = new BitmapImage(new Uri(Path.GetFullPath(path))),
                    MaxWidth = 100,
                    MaxHeight = 100,
                    SnapsToDevicePixels = true,
                };
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Fant);
                images.Add(image);
            }

            return images;
        }
    }
}
