using LiteDB;
using LiteDbExplorer.Converters;
using LiteDbExplorer.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace LiteDbExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public Paths PathDefinitions
        {
            get; set;
        } = new Paths();

        public ObservableCollection<DatabaseReference> Databases
        {
            get; set;
        } = new ObservableCollection<DatabaseReference>();

        private IEnumerable<DocumentReference> DbSelectedItems
        {
            get
            {
                if (ListCollectionData.Visibility == Visibility.Visible)
                {
                    return ListCollectionData.SelectedItems.Cast<DocumentReference>();
                }
                else
                {
                    return null;
                }
            }
        }

        private int DbItemsSelectedCount
        {
            get
            {
                if (ListCollectionData != null && ListCollectionData.Visibility == Visibility.Visible)
                {
                    return ListCollectionData.SelectedItems.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        private CollectionReference selectedCollection;
        public CollectionReference SelectedCollection
        {
            get
            {
                return selectedCollection;
            }

            set
            {
                selectedCollection = value;

                if (value == null)
                {
                    ListCollectionData.ItemsSource = null;
                    ListCollectionData.Visibility = Visibility.Hidden;
                }
                else
                {
                    List<string> keys = new List<string>();

                    foreach (var item in selectedCollection.Items)
                    {
                        keys = item.LiteDocument.Keys.Union(keys).ToList();
                    }

                    GridCollectionData.Columns.Clear();
                    foreach (var key in keys.OrderBy(a => a))
                    {
                        AddGridColumn(key);
                    }

                    ListCollectionData.ItemsSource = selectedCollection.Items;
                    ListCollectionData.Visibility = Visibility.Visible;
                }
            }
        }

        private DatabaseReference selectedDatabase;
        public DatabaseReference SelectedDatabase
        {
            get
            {
                return selectedDatabase;
            }

            set
            {
                selectedDatabase = value;
                if (selectedDatabase == null)
                {
                    Title = "LiteDB Explorer " + Versions.CurrentVersion;
                }
                else
                {
                    Title = string.Format("{0} - LiteDB Explorer {1}", selectedDatabase.Name, Versions.CurrentVersion);
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Config.ConfigureLogger();

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                var update = new Update();

                try
                {
                    if (update.IsUpdateAvailable)
                    {
                        update.DownloadUpdate();

                        if (MessageBox.Show("New update found, install now?", "Update found", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            update.InstallUpdate();
                        }
                    }
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Failed to process update.");
                }
            });

            DockSearch.Visibility = Visibility.Collapsed;
        }

        #region Exit Command
        private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (var database in Databases)
            {
                database.Dispose();
            }

            Application.Current.MainWindow.Close();
        }
        #endregion Exit Command

        #region Open Command
        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "All files|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                OpenDatabase(dialog.FileName);
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Failed to open database: ");
                MessageBox.Show("Failed to open database: " + exc.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion Open Command

        #region New Command
        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "All files|*.*",
                OverwritePrompt = true
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            using (var stream = new FileStream(dialog.FileName, System.IO.FileMode.Create))
            {
                LiteEngine.CreateDatabase(stream);
            }

            OpenDatabase(dialog.FileName);
        }
        #endregion New Command

        #region Close Command
        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedDatabase != null;
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectedCollection != null && SelectedCollection.Database == SelectedDatabase)
            {
                SelectedCollection = null;
            }

            SelectedDatabase.Dispose();
            Databases.Remove(SelectedDatabase);
            SelectedDatabase = null;
        }
        #endregion Close Command

        #region EditDbProperties Command
        private void EditDbPropertiesCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedDatabase != null;
        }

        private void EditDbPropertiesCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new DatabasePropertiesWindow(SelectedDatabase.LiteDatabase)
            {
                Owner = this
            };

            window.ShowDialog();
        }
        #endregion EditDbProperties Command

        #region AddFile Command
        private void AddFileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedDatabase != null;
        }

        private void AddFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddFileToDatabase(SelectedDatabase);
        }
        #endregion AddFile Command

        #region Add Command
        private void AddCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedCollection != null;
        }

        private void AddCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectedCollection is FileCollectionReference)
            {
                AddFileToDatabase(SelectedCollection.Database);
            }
            else
            {
                var newDoc = new BsonDocument
                {
                    ["_id"] = ObjectId.NewObjectId()
                };

                ListCollectionData.SelectedItem = SelectedCollection.AddItem(newDoc);
                ListCollectionData.ScrollIntoView(ListCollectionData.SelectedItem);
                UpdateGridColumns(newDoc);                
            }
        }
        #endregion Add Command

        #region Edit Command
        private void EditCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DbItemsSelectedCount == 1;
        }

        private void EditCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = DbSelectedItems.First();
            var dbItem = item.Collection.LiteCollection.FindById(item.LiteDocument["_id"]);
            LiteFileInfo fileInfo = null;

            if (item.Collection.Name == "_files")
            {
                fileInfo = (item.Collection as FileCollectionReference).GetFileObject(item);
            }

            var window = new Windows.DocumentViewer(dbItem, fileInfo)
            {
                Owner = this
            };

            using (var trans = item.Collection.Database.LiteDatabase.BeginTrans())
            {
                if (window.ShowDialog() == true)
                {
                    item.LiteDocument = dbItem;
                    item.Collection.UpdateItem(item);
                    trans.Commit();
                    UpdateGridColumns(dbItem);
                }
                else
                {
                    trans.Rollback();
                }
            }
        }
        #endregion Edit Command

        #region Remove Command
        private void RemoveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DbItemsSelectedCount > 0;
        }

        private void RemoveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to remove items?",
                "Are you sure?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            ) != MessageBoxResult.Yes)
            {
                return;
            }

            SelectedCollection.RemoveItems(DbSelectedItems.ToList());
        }
        #endregion Remove Command

        #region Export Command
        private void ExportCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DbItemsSelectedCount > 0;
        }

        private void ExportCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectedCollection is FileCollectionReference)
            {
                if (DbSelectedItems.Count() == 1)
                {
                    var file = DbSelectedItems.First();

                    var dialog = new SaveFileDialog()
                    {
                        Filter = "All files|*.*",
                        FileName = file.LiteDocument["filename"],
                        OverwritePrompt = true
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        (file.Collection as FileCollectionReference).SaveFile(file, dialog.FileName);
                    }
                }
                else
                {
                    var dialog = new CommonOpenFileDialog()
                    {
                        IsFolderPicker = true,
                        Title = "Select folder to export files to..."
                    };

                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        foreach (var file in DbSelectedItems)
                        {
                            var path = Path.Combine(dialog.FileName, file.LiteDocument["_id"].AsString + "-" + file.LiteDocument["filename"].AsString);
                            var dir = Path.GetDirectoryName(path);
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }

                            (file.Collection as FileCollectionReference).SaveFile(file, path);
                        }
                    }
                }
            }
            else
            {
                var dialog = new SaveFileDialog()
                {
                    Filter = "Json File|*.json",
                    FileName = "export.json",
                    OverwritePrompt = true
                };

                if (dialog.ShowDialog() == true)
                {
                    if (DbSelectedItems.Count() == 1)
                    {
                        File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(DbSelectedItems.First().LiteDocument, true, false));
                    }
                    else
                    {
                        var data = new BsonArray(DbSelectedItems.Select(a => a.LiteDocument));
                        File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(data, true, false));
                    }
                }
            }
        }
        #endregion Export Command

        #region AddCollection Command
        private void AddCollectionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedDatabase != null;
        }

        private void AddCollectionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (InputBoxWindow.ShowDialog("New collection name:", "Enter new colletion name", "", out string name) == true)
                {
                    SelectedDatabase.AddCollection(name);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    "Failed to add new collection:" + Environment.NewLine + exc.Message,
                    "Database error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        #endregion AddCollection Command

        #region RenameCollection Command
        private void RenameCollectionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedCollection != null && SelectedCollection.Name != "_files" && SelectedCollection.Name != "_chunks";
        }

        private void RenameCollectionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (InputBoxWindow.ShowDialog("New name:", "Enter new colletion name", SelectedCollection.Name, out string name) == true)
                {
                    SelectedDatabase.RenameCollection(SelectedCollection.Name, name);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    "Failed to rename collection:" + Environment.NewLine + exc.Message,
                    "Database error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        #endregion RenameCollection Command

        #region DropCollection Command
        private void DropCollectionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedCollection != null && SelectedCollection.Name != "_files" && SelectedCollection.Name != "_chunks";
        }

        private void DropCollectionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show(
                    string.Format("Are you sure you want to drop collection \"{0}\"?", SelectedCollection.Name),
                    "Are you sure?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                ) == MessageBoxResult.Yes)
                {
                    SelectedDatabase.DropCollection(SelectedCollection.Name);
                    SelectedCollection = null;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    "Failed to drop collection:" + Environment.NewLine + exc.Message,
                    "Database error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        #endregion DropCollection Command

        #region Find Command
        private void FindCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void FindCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DockSearch.Visibility = Visibility.Visible;
            TextSearch.Focus();
            TextSearch.SelectAll();
        }
        #endregion Find Command

        #region FindNext Command
        private void FindNextCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedCollection != null;
        }

        private void FindNextCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextSearch.Text))
            {
                return;
            }

            var skipIndex = -1;
            if (DbItemsSelectedCount > 0)
            {
                skipIndex = SelectedCollection.Items.IndexOf(DbSelectedItems.Last());
            }

            foreach (var item in SelectedCollection.Items.Skip(skipIndex + 1))
            {
                if (ItemMatchesSearch(TextSearch.Text, item, (bool)CheckSearchCase.IsChecked))
                {
                    SelectDocumentInView(item);
                    return;
                }
            }
        }
        #endregion FindNext Command

        #region FindPrevious Command
        private void FindPreviousCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedCollection != null;
        }

        private void FindPreviousCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextSearch.Text))
            {
                return;
            }

            var skipIndex = 0;
            if (DbItemsSelectedCount > 0)
            {
                skipIndex = SelectedCollection.Items.Count - SelectedCollection.Items.IndexOf(DbSelectedItems.Last()) - 1;
            }

            foreach (var item in SelectedCollection.Items.Reverse().Skip(skipIndex + 1))
            {
                if (ItemMatchesSearch(TextSearch.Text, item, (bool)CheckSearchCase.IsChecked))
                {
                    SelectDocumentInView(item);
                    return;
                }
            }
        }
        #endregion FindPrevious Command

        #region Copy Command
        private void CopyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DbItemsSelectedCount > 0 && SelectedCollection != null && SelectedCollection.Name != "_files";
        }

        private void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var data = new BsonArray(DbSelectedItems.Select(a => a.LiteDocument));
            Clipboard.SetData(DataFormats.Text, JsonSerializer.Serialize(data, true, false));
        }
        #endregion Copy Command

        #region Paste Command
        private void PasteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedCollection != null && SelectedCollection.Name != "_files" && Clipboard.ContainsText();            
        }

        private void PasteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var textData = Clipboard.GetText();
                var newValue = JsonSerializer.Deserialize(textData);

                if (newValue.IsArray)
                {
                    foreach (var value in newValue.AsArray)
                    {
                        var doc = value.AsDocument;
                        SelectedCollection.AddItem(doc);
                        UpdateGridColumns(doc);
                    }
                }
                else
                {
                    var doc = newValue.AsDocument;
                    SelectedCollection.AddItem(doc);
                    UpdateGridColumns(doc);
                }
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Cannot process clipboard data.");
                MessageBox.Show("Failed to paste document from clipboard: " + exc.Message, "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion Paste Command

        private bool ItemMatchesSearch(string matchTerm, DocumentReference document, bool matchCase)
        {
            var stringData = JsonSerializer.Serialize(document.LiteDocument);

            if (matchCase)
            {
                return stringData.IndexOf(matchTerm, 0, StringComparison.InvariantCulture) != -1;
            }
            else
            {
                return stringData.IndexOf(matchTerm, 0, StringComparison.InvariantCultureIgnoreCase) != -1;
            }
        }

        private void SelectDocumentInView(DocumentReference document)
        {
            ListCollectionData.SelectedItem = document;
            ListCollectionData.ScrollIntoView(document);
        }

        private void UpdateGridColumns(BsonDocument dbItem)
        {
            var headers = GridCollectionData.Columns.Select(a => (a.Header as TextBlock).Text);
            var missing = dbItem.Keys.Except(headers);

            foreach (var key in missing)
            {
                AddGridColumn(key);
            }
        }

        private void AddGridColumn(string key)
        {
            GridCollectionData.Columns.Add(new GridViewColumn()
            {
                Header = new TextBlock() { Text = key },
                DisplayMemberBinding = new Binding()
                {
                    Path = new PropertyPath(string.Format("LiteDocument[{0}]", key)),
                    Mode = BindingMode.OneWay,
                    Converter = new BsonValueToStringConverter()
                }
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (var db in Databases)
            {
                db.Dispose();
            }

            Application.Current.Shutdown(0);
        }

        private void TreeDatabasese_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null)
            {
                SelectedCollection = null;
                return;
            }

            if (e.NewValue is CollectionReference)
            {
                var collection = e.NewValue as CollectionReference;
                SelectedCollection = collection;
                SelectedDatabase = collection.Database;
            }
            else if (e.NewValue is DatabaseReference)
            {
                SelectedDatabase = e.NewValue as DatabaseReference;
            }
            else
            {
                SelectedCollection = null;
            }
        }

        private void ListCollectionData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CanExecuteRoutedEventArgs args = (CanExecuteRoutedEventArgs)Activator.CreateInstance(typeof(CanExecuteRoutedEventArgs),
                         BindingFlags.NonPublic | BindingFlags.Instance, null, new object[2] { Commands.Edit, null }, null);
            ExportCommand_CanExecute(this, args);

            if (args.CanExecute)
            {
                EditCommand_Executed(this, null);
            }
        }     

        private void RecentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var path = (sender as MenuItem).Tag as string;
            OpenDatabase(path);
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
                e.Handled = true;
            }
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            return source as TreeViewItem;
        }

        public void OpenDatabase(string path)
        {
            if (Databases.FirstOrDefault(a => a.Location == path) != null)
            {
                return;
            }                

            if (!File.Exists(path))
            {
                MessageBox.Show(
                    "Cannot open database, file not found.",
                    "File not found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            string password = null;
            if (DatabaseReference.IsDbPasswordProtected(path))
            {
                if (InputBoxWindow.ShowDialog("Database is password protected, enter password:", "Database password.", "", out password) != true)
                {
                    return;
                }
            }

            if (PathDefinitions.RecentFiles.Contains(path))
            {
                PathDefinitions.RecentFiles.Remove(path);
            }

            PathDefinitions.RecentFiles.Insert(0, path);

            try
            {
                Databases.Add(new DatabaseReference(path, password));
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to open database:" + Environment.NewLine + e.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                logger.Error(e, "Failed to process update: ");
                return;
            }
        }

        private void IssueMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Config.IssuesUrl);
        }

        private void HomepageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Config.HomepageUrl);
        }

        private void RecentItemMoreBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ButtonOpen.ContextMenu.IsEnabled = true;
            ButtonOpen.ContextMenu.PlacementTarget = ButtonOpen;
            ButtonOpen.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            ButtonOpen.ContextMenu.IsOpen = true;
        }

        private void ButtonCloseSearch_Click(object sender, RoutedEventArgs e)
        {
            DockSearch.Visibility = Visibility.Collapsed;
        }

        private void AddFileToDatabase(DatabaseReference database)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "All files|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                if (InputBoxWindow.ShowDialog("New file id:", "Enter new file id", Path.GetFileName(dialog.FileName), out string id) == true)
                {
                    var file = database.AddFile(id, dialog.FileName);
                    SelectedCollection = database.Collections.First(a => a.Name == "_files");
                    ListCollectionData.SelectedItem = file;
                    ListCollectionData.ScrollIntoView(ListCollectionData.SelectedItem);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    "Failed to upload file:" + Environment.NewLine + exc.Message,
                    "Database error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
