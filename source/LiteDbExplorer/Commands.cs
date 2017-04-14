using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LiteDbExplorer
{
    public static class Commands
    {
        public static readonly RoutedUICommand Exit = new RoutedUICommand
        (
            "Exit",
            "Exit",
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F4, ModifierKeys.Alt)
            }
        );

        public static readonly RoutedUICommand Add = new RoutedUICommand
        (
            "Add...",
            "Add",
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Insert)
            }
        );

        public static readonly RoutedUICommand Edit = new RoutedUICommand
        (
            "Edit...",
            "Edit",
            typeof(Commands)
        );

        public static readonly RoutedUICommand Remove = new RoutedUICommand
        (
            "Remove",
            "Remove",
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Delete)
            }
        );

        public static readonly RoutedUICommand DropCollection = new RoutedUICommand
        (
            "Drop",
            "DropCollection",
            typeof(Commands)
        );

        public static readonly RoutedUICommand AddCollection = new RoutedUICommand
        (
            "Add...",
            "AddCollection",
            typeof(Commands)
        );

        public static readonly RoutedUICommand RenameCollection = new RoutedUICommand
        (
            "Rename...",
            "RenameCollection",
            typeof(Commands)
        );

        public static readonly RoutedUICommand Export = new RoutedUICommand
        (
            "Export as...",
            "Export",
            typeof(Commands)
        );

        public static readonly RoutedUICommand EditDbProperties = new RoutedUICommand
        (
            "Database properties...",
            "EditDb",
            typeof(Commands)
        );
    }
}
