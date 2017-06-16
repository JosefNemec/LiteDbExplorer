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

    public class DocumentFieldData
    {
        public string Name
        {
            get; set;
        }

        public FrameworkElement EditControl
        {
            get; set;
        }

        public DocumentFieldData(string name, FrameworkElement editControl)
        {
            Name = name;
            EditControl = editControl;
        }
    }

    /// <summary>
    /// Interaction logic for DocumentViewer.xaml
    /// </summary>
    public partial class DocumentViewer : Window
    {
        public static readonly RoutedUICommand PreviousItem = new RoutedUICommand
        (
            "Previous Item",
            "PreviousItem",
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.PageUp)
            }
        );

        public static readonly RoutedUICommand NextItem = new RoutedUICommand
        (
            "Next Item",
            "NextItem",
            typeof(Commands),
            new InputGestureCollection()
            {
                    new KeyGesture(Key.PageDown)
            }
        );

        private ObservableCollection<DocumentFieldData> customControls;

        private BsonDocument currentDocument;
        private DocumentReference documentReference;
        private LiteTransaction dbTrans;

        private bool isReadOnly = false;
        public bool IsReadOnly
        {
            get
            {
                return isReadOnly;
            }
        }

        public DocumentViewer(BsonDocument document, bool readOnly)
        {
            InitializeComponent();
            isReadOnly = readOnly;

            currentDocument = document;
            customControls = new ObservableCollection<DocumentFieldData>();

            for (int i = 0; i < document.Keys.Count; i++)
            {
                var key = document.Keys.ElementAt(i);
                customControls.Add(NewField(key, readOnly));
            }

            ListItems.ItemsSource = customControls;

            ButtonNext.Visibility = Visibility.Collapsed;
            ButtonPrev.Visibility = Visibility.Collapsed;

            if (readOnly)
            {
                ButtonClose.Visibility = Visibility.Visible;
                ButtonOK.Visibility = Visibility.Collapsed;
                ButtonCancel.Visibility = Visibility.Collapsed;
                DropNewField.Visibility = Visibility.Collapsed;
            }
        }

        public DocumentViewer(DocumentReference document)
        {
            InitializeComponent();
            LoadDocument(document);
        }

        private void LoadDocument(DocumentReference document)
        {
            if (dbTrans != null)
            {
                dbTrans.Rollback();
                dbTrans.Dispose();
            }

            if (document.Collection is FileCollectionReference)
            {
                var fileInfo = (document.Collection as FileCollectionReference).GetFileObject(document);
                GroupFile.Visibility = Visibility.Visible;
                FileView.LoadFile(fileInfo);
            }

            currentDocument = document.Collection.LiteCollection.FindById(document.LiteDocument["_id"]);
            documentReference = document;
            dbTrans = documentReference.Collection.Database.LiteDatabase.BeginTrans();
            customControls = new ObservableCollection<DocumentFieldData>();

            for (int i = 0; i < document.LiteDocument.Keys.Count; i++)
            {
                var key = document.LiteDocument.Keys.ElementAt(i);
                customControls.Add(NewField(key, IsReadOnly));
            }

            ListItems.ItemsSource = customControls;
        }

        private DocumentFieldData NewField(string key, bool readOnly)
        {
            var valueEdit = BsonValueEditor.GetBsonValueEditor(string.Format("[{0}]", key), currentDocument[key], currentDocument, readOnly);
            return new DocumentFieldData(key, valueEdit);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var key = (sender as Button).Tag as string;
            var item = customControls.First(a => a.Name == key);
            customControls.Remove(item);
            currentDocument.Remove(key);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (dbTrans != null)
            {
                dbTrans.Rollback();
            }

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

            if (documentReference != null)
            {
                documentReference.LiteDocument = currentDocument;
                documentReference.Collection.UpdateItem(documentReference);
                dbTrans.Commit();
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

            if (currentDocument.Keys.Contains(fieldName))
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

            currentDocument.Add(fieldName, newValue);
            var newField = NewField(fieldName, false);
            customControls.Add(newField);
            newField.EditControl.Focus();
            ItemsField_SizeChanged(ListItems, null);
            ListItems.ScrollIntoView(newField);
        }

        private void ItemsField_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView grid = listView.View as GridView;
            var newWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 10 - grid.Columns[0].ActualWidth - grid.Columns[2].ActualWidth;
            if (newWidth > 0)
            {
                grid.Columns[1].Width = newWidth;
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            ItemsField_SizeChanged(ListItems, null);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (dbTrans != null)
            {
                dbTrans.Dispose();
            }
        }

        private void NextItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (documentReference == null)
            {
                e.CanExecute = false;
            }
            else
            {
                var index = documentReference.Collection.Items.IndexOf(documentReference);
                e.CanExecute = index + 1 < documentReference.Collection.Items.Count;
            }
        }

        private void NextItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var index = documentReference.Collection.Items.IndexOf(documentReference);

            if (index + 1 < documentReference.Collection.Items.Count)
            {
                var newDocument = documentReference.Collection.Items[index + 1];                
                LoadDocument(newDocument);
            }
        }

        private void PreviousItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (documentReference == null)
            {
                e.CanExecute = false;
            }
            else
            {
                var index = documentReference.Collection.Items.IndexOf(documentReference);
                e.CanExecute = index > 0;
            }
        }

        private void PreviousItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var index = documentReference.Collection.Items.IndexOf(documentReference);

            if (index > 0)
            {
                var newDocument = documentReference.Collection.Items[index - 1];
                LoadDocument(newDocument);
            }
        }
    }
}
