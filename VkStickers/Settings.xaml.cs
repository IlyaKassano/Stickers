using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VkStickers.General;

namespace VkStickers
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Settings : Window
    {
        readonly Config _config;

        public Settings()
        {
            InitializeComponent();

            _config = ConfigLoader.GetConfig();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            /*int rowNum = 0;
            foreach (var prop in _config.GetType().GetProperties())
            {
                var rowDef = new RowDefinition()
                {
                    Height = GridLength.Auto
                };
                Grid1.RowDefinitions.Add(rowDef);

                var label = new Label()
                {
                    Content = prop.Name,
                };
                var textBox = new TextBox()
                {
                    Width = 100,
                };

                Grid.SetRow(label, rowNum);
                Grid.SetColumn(label, 0);
                Grid1.Children.Add(label);

                Grid.SetRow(textBox, rowNum);
                Grid.SetColumn(textBox, 1);
                Grid1.Children.Add(textBox);
                rowNum++;
            }*/
        }
    }
}
