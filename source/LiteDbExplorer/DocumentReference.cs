using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LiteDbExplorer
{
    public class DocumentReference : INotifyPropertyChanged
    {
        private BsonDocument document;
        public BsonDocument LiteDocument
        {
            get
            {
                return document;
            }

            set
            {
                document = value;
                OnPropertyChanged();
            }
        }

        private CollectionReference collection;
        public CollectionReference Collection
        {
            get
            {
                return collection;
            }

            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }

        public DocumentReference()
        {
        }

        public DocumentReference(BsonDocument document, CollectionReference collection)
        {
            LiteDocument = document;
            Collection = collection;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
