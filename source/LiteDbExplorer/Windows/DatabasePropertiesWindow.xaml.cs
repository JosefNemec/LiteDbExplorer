using LiteDB;
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
using Xceed.Wpf.Toolkit;

namespace LiteDbExplorer.Windows
{
    /// <summary>
    /// Interaction logic for DatabasePropertiesWindow.xaml
    /// </summary>
    public partial class DatabasePropertiesWindow : Window
    {
        public LiteDatabase Database
        {
            get; set;
        }

        public DatabasePropertiesWindow(LiteDatabase database)
        {
            InitializeComponent();
            Database = database;
            InputVersion.DataContext = Database.Engine;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            var binding = BindingOperations.GetBindingExpression(InputVersion, IntegerUpDown.ValueProperty);
            if (binding.IsDirty)
            {
                binding.UpdateSource();
            }

            DialogResult = true;
            Close();
        }

        private void ButtonShrink_Click(object sender, RoutedEventArgs e)
        {
            Database.Shrink();
        }

        private void ButtonPassword_Click(object sender, RoutedEventArgs e)
        {
            if (InputBoxWindow.ShowDialog("New password, enter empty string to remove password.", "", "", out string password) == true)
            {
                if (string.IsNullOrEmpty(password))
                {
                    Database.Shrink(null);
                }
                else
                {
                    Database.Shrink(password);
                }
            }
        }
    }
}
