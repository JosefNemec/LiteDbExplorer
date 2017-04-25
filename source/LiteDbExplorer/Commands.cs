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
            "Drop Collection",
            "DropCollection",
            typeof(Commands)
        );

        public static readonly RoutedUICommand AddCollection = new RoutedUICommand
        (
            "Add Collection...",
            "AddCollection",
            typeof(Commands)
        );

        public static readonly RoutedUICommand RenameCollection = new RoutedUICommand
        (
            "Rename Collection...",
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

        public static readonly RoutedUICommand AddFile = new RoutedUICommand
        (
            "Add File...",
            "AddFile",
            typeof(Commands)
        );

        public static readonly RoutedUICommand Find = new RoutedUICommand
        (
            "Find...",
            "Find",
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand FindNext = new RoutedUICommand
        (
            "Find Next",
            "FindNext",
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F3)
            }
        );

        public static readonly RoutedUICommand FindPrevious = new RoutedUICommand
        (
            "Find Previous",
            "FindPrevious",
            typeof(Commands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F3, ModifierKeys.Shift)
            }
        );
    }
}
