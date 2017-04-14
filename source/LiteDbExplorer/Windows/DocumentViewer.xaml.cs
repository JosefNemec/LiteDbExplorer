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
using LiteDbExplorer.Converters;
using LiteDbExplorer.Controls;
using System.Collections.ObjectModel;

namespace LiteDbExplorer.Windows
{
    /// <summary>
    /// Interaction logic for DocumentViewer.xaml
    /// </summary>
    public partial class DocumentViewer : Window
    {
        public class CustomControl
        {
            public string Name
            {
                get; set;
            }

            public FrameworkElement EditControl
            {
                get; set;
            }

            public CustomControl(string name, FrameworkElement editControl)
            {
                Name = name;
                EditControl = editControl;
            }
        }
        
        private ObservableCollection<CustomControl> customControls;

        private BsonDocument document;

        public DocumentViewer(BsonDocument document, LiteFileInfo fileInfo)
        {
            InitializeComponent();

            this.document = document;
            customControls = new ObservableCollection<CustomControl>();

            for (int i = 0; i < document.Keys.Count; i++)
            {
                var key = document.Keys.ElementAt(i);
                customControls.Add(NewField(key));
            }

            ListItems.ItemsSource = customControls;

            if (fileInfo != null)
            {
                GroupFile.Visibility = Visibility.Visible;
                FileView.LoadFile(fileInfo);
            }
        }

        private CustomControl NewField(string key)
        {
            var valueEdit = BsonValueEditor.GetBsonValueEditor(string.Format("[{0}]", key), document[key], document);
            return new CustomControl(key, valueEdit);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var key = (sender as Button).Tag as string;
            var item = customControls.First(a => a.Name == key);
            customControls.Remove(item);
            document.Remove(key);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            //TODO make array and document types use this as well
            foreach (var ctrl in customControls)
            {
                var control = ctrl.EditControl;
                var values = control.GetLocalValueEnumerator();
                while (values.MoveNext())
                {
                    var current = values.Current;
                    if (BindingOperations.IsDataBound(control, current.Property))
                    {
                        var binding = control.GetBindingExpression(current.Property);
                        if (binding.IsDirty)
                        {
                            binding.UpdateSource();
                        }
                    }
                }
            }

            DialogResult = true;
            Close();
        }

        private void NewFieldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (InputBoxWindow.ShowDialog("Enter name of new field.", "New field name:", "", out string fieldName) != true)
            {
                return;
            }

            if (document.Keys.Contains(fieldName))
            {
                MessageBox.Show(string.Format("Field \"{0}\" already exists!", fieldName), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var menuItem = sender as MenuItem;
            BsonValue newValue;

            switch (menuItem.Header as string)
            {
                case "String":
                    newValue = new BsonValue(string.Empty);
                    break;
                case "Boolean":
                    newValue = new BsonValue(false);
                    break;
                case "Double":
                    newValue = new BsonValue((double)0);
                    break;
                case "Int32":
                    newValue = new BsonValue((int)0);
                    break;
                case "Int64":
                    newValue = new BsonValue((long)0);
                    break;
                case "DateTime":
                    newValue = new BsonValue(DateTime.MinValue);
                    break;
                case "Array":
                    newValue = new BsonArray();
                    break;
                case "Document":
                    newValue = new BsonDocument();
                    break;
                default:
                    throw new Exception("Uknown value type.");

            }

            document.Add(fieldName, newValue);
            var newField = NewField(fieldName);
            customControls.Add(newField);
            newField.EditControl.Focus();
            ItemsField_SizeChanged(ListItems, null);
            ListItems.ScrollIntoView(newField);
        }

        private void ItemsField_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView grid = listView.View as GridView;            
            grid.Columns[1].Width = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 10 - grid.Columns[0].ActualWidth - grid.Columns[2].ActualWidth;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            ItemsField_SizeChanged(ListItems, null);
        }
    }
}
