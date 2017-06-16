using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LiteDbExplorer.Converters;
using Xceed.Wpf.Toolkit;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace LiteDbExplorer.Controls
{
    public class BsonValueEditor
    {
        public static FrameworkElement GetBsonValueEditor(string bindingPath, BsonValue bindingValue, object bindingSource)
        {
            var binding = new Binding()
            {
                Path = new PropertyPath(bindingPath),
                Source = bindingSource,
                Mode = BindingMode.TwoWay,
                Converter = new BsonValueToNetValueConverter(),
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };

            if (bindingValue.IsArray)
            {
                var button = new Button()
                {
                    Content = "Array"
                };

                button.Click += (s, a) =>
                {
                    var arrayValue = bindingValue as BsonArray;
                    var window = new Windows.ArrayViewer(arrayValue)
                    {
                        Owner = Application.Current.MainWindow
                    };

                    if (window.ShowDialog() == true)
                    {
                        arrayValue.Clear();
                        arrayValue.AddRange(window.EditedItems);
                    }
                };

                return button;
            }
            else if (bindingValue.IsBoolean)
            {
                var check = new CheckBox();
                check.SetBinding(ToggleButton.IsCheckedProperty, binding);
                return check;
            }
            else if (bindingValue.IsDateTime)
            {
                var datePicker = new DateTimePicker()
                {
                    TextAlignment = TextAlignment.Left
                };

                datePicker.SetBinding(DateTimePicker.ValueProperty, binding);
                return datePicker;
            }
            else if (bindingValue.IsDocument)
            {
                var button = new Button()
                {
                    Content = "Document"
                };

                button.Click += (s, a) =>
                {
                    var window = new Windows.DocumentViewer(bindingValue as BsonDocument)
                    {
                        Owner = Application.Current.MainWindow
                    };

                    window.ShowDialog();
                };

                return button;
            }
            else if (bindingValue.IsDouble)
            {
                var numberEditor = new DoubleUpDown()
                {
                    TextAlignment = TextAlignment.Left
                };

                numberEditor.SetBinding(DoubleUpDown.ValueProperty, binding);
                return numberEditor;
            }
            else if (bindingValue.IsInt32)
            {
                var numberEditor = new IntegerUpDown()
                {
                    TextAlignment = TextAlignment.Left
                };

                numberEditor.SetBinding(IntegerUpDown.ValueProperty, binding);
                return numberEditor;
            }
            else if (bindingValue.IsInt64)
            {
                var numberEditor = new LongUpDown()
                {
                    TextAlignment = TextAlignment.Left
                };

                numberEditor.SetBinding(LongUpDown.ValueProperty, binding);
                return numberEditor;
            }
            else if (bindingValue.IsString)
            {
                var stringEditor = new TextBox();
                stringEditor.SetBinding(TextBox.TextProperty, binding);
                return stringEditor;
            }
            else if (bindingValue.IsBinary)
            {
                var text = new TextBlock()
                {
                    Text = "[Binary Data]"
                };

                return text;
            }
            else if (bindingValue.IsObjectId)
            {
                var text = new TextBlock()
                {
                    Text = bindingValue.AsString
                };

                return text;
            }
            else
            {
                var stringEditor = new TextBox();
                stringEditor.SetBinding(TextBox.TextProperty, binding);
                return stringEditor;
            }
        }
    }
}
