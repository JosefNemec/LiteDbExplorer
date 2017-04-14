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

namespace LiteDbExplorer.Windows
{
    /// <summary>
    /// Interaction logic for InputBoxWindow.xaml
    /// </summary>
    public partial class InputBoxWindow : Window
    {        
        public string Text
        {
            get
            {
                return TextText.Text;
            }
        }

        public InputBoxWindow()
        {
            InitializeComponent();
        }

        public static bool? ShowDialog(string message, string caption, string predefined, out string input)
        {
            var window = new InputBoxWindow();
            window.TextMessage.Text = message;
            window.Title = caption;
            window.TextText.Text = predefined;

            var result = window.ShowDialog();
            input = window.Text;
            return result;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextText.Focus();
            TextText.SelectAll();
        }
    }
}
