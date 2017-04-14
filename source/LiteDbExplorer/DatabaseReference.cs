using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDbExplorer
{
    public class DatabaseReference : INotifyPropertyChanged, IDisposable
    {
        public LiteDatabase LiteDatabase
        {
            get;
        }

        private ObservableCollection<CollectionReference> collections;
        public ObservableCollection<CollectionReference> Collections
        {
            get
            {
                return collections;
            }

            set
            {
                collections = value;
                OnPropertyChanged("Collections");
            }
        }

        private string name;
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        private string location;
        public string Location
        {
            get
            {
                return location;
            }

            set
            {
                location = value;
                OnPropertyChanged("Location");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DatabaseReference(string path, string password)
        {            
            Location = path;
            Name = Path.GetFileName(path);

            if (string.IsNullOrEmpty(password))
            {
                LiteDatabase = new LiteDatabase(path);
            }
            else
            {
                LiteDatabase = new LiteDatabase(string.Format("Filename={0};Password={1}", path, password));
            }

            UpdateCollections();
        }

        public void Dispose()
        {
            LiteDatabase.Dispose();
        }

        private void UpdateCollections()
        {
            Collections = new ObservableCollection<CollectionReference>(LiteDatabase.GetCollectionNames()
                .Where(a => a != "_chunks").OrderBy(a => a).Select(a =>
                {
                    if (a == "_files")
                    {
                        return new FileCollectionReference(a, this);
                    }
                    else
                    {
                        return new CollectionReference(a, this);
                    }
                }));
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddCollection(string name)
        {
            if (LiteDatabase.GetCollectionNames().Contains(name))
            {
                throw new Exception(string.Format("Cannot add collection \"{0}\", collection with that name already exists.", name));
            }

            var coll = LiteDatabase.GetCollection(name);
            var newDoc = new BsonDocument
            {
                ["_id"] = ObjectId.NewObjectId()
            };

            coll.Insert(newDoc);
            coll.Delete(newDoc["_id"]);
            UpdateCollections();
        }

        public void RenameCollection(string oldName, string newName)
        {
            LiteDatabase.RenameCollection(oldName, newName);
            UpdateCollections();
        }

        public void DropCollection(string name)
        {
            LiteDatabase.DropCollection(name);
            UpdateCollections();
        }

        public static bool IsDbPasswordProtected(string path)
        {
            using (var db = new LiteDatabase(path))
            {
                try
                {
                    db.GetCollectionNames();
                    return false;
                }
                catch (LiteException e)
                {
                    if (e.Message.Contains("password"))
                    {
                        return true;
                    }

                    throw;
                }
            }
        }
    }
}
