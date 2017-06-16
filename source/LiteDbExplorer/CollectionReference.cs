using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDbExplorer
{
    public class CollectionReference : INotifyPropertyChanged
    {
        private DatabaseReference database;
        public DatabaseReference Database
        {
            get
            {
                return database;
            }

            set
            {
                database = value;
                OnPropertyChanged("Database");
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

        private ObservableCollection<DocumentReference> items;
        public ObservableCollection<DocumentReference> Items
        {
            get
            {
                if (items == null)
                {
                    items = new ObservableCollection<DocumentReference>();
                    foreach (var item in LiteCollection.FindAll().Select(a => new DocumentReference(a, this)))
                    {
                        items.Add(item);
                    }
                }

                return items;
            }

            set
            {
                items = value;
                OnPropertyChanged("Items");
            }
        }

        public LiteCollection<BsonDocument> LiteCollection
        {
            get
            {
                return database.LiteDatabase.GetCollection(Name);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public CollectionReference(string name, DatabaseReference database)
        {
            Name = name;
            Database = database;
        }

        public virtual void UpdateItem(DocumentReference document)
        {
            LiteCollection.Update(document.LiteDocument);
        }

        public virtual void RemoveItem(DocumentReference document)
        {
            LiteCollection.Delete(document.LiteDocument["_id"]);
            Items.Remove(document);
        }

        public virtual void RemoveItems(IEnumerable<DocumentReference> documents)
        {
            foreach (var doc in documents)
            {
                RemoveItem(doc);
            }
        }

        public virtual DocumentReference AddItem(BsonDocument document)
        {
            LiteCollection.Insert(document);
            var newDoc = new DocumentReference(document, this);
            Items.Add(newDoc);
            return newDoc;
        }

        public virtual void Refresh()
        {
            if (items == null)
            {
                items = new ObservableCollection<DocumentReference>();
            }
            else
            {
                items.Clear();
            }

            foreach (var item in LiteCollection.FindAll().Select(a => new DocumentReference(a, this)))
            {
                items.Add(item);
            }

            OnPropertyChanged("Items");
        }
    }
}
