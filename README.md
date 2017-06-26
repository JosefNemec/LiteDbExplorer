
# <img src="https://github.com/JosefNemec/LiteDbExplorer/blob/master/source/LiteDbExplorer/Images/icon.png" width="32">  LiteDb Explorer
Graphical editor for [LiteDB](https://github.com/mbdavid/LiteDB) databases. Writter in .NET and WPF.

### Features in current Alpha release:
* View and edit existing documents
* Add new items to database including files
* Preview files (images and text files)
* Export database documents (as JSON) and files
* Change password in protected databases
* Shrink database
* Open multiple databases at the same time
<p>
<img align="center" src="https://raw.githubusercontent.com/JosefNemec/LiteDbExplorer/master/web/screen1.png" width="400" >
<img align="center" src="https://raw.githubusercontent.com/JosefNemec/LiteDbExplorer/master/web/screen2.png" width="200" >
<img align="center" src="https://raw.githubusercontent.com/JosefNemec/LiteDbExplorer/master/web/screen3.png" width="200" >
</p>

Download
---------

Grab latest build from [releases](https://github.com/JosefNemec/LiteDbExplorer/releases) page.
Application will automatically notify about new version when released.

Requirements: Windows 7, 8 or 10 and [.Net 4.6](https://www.microsoft.com/en-us/download/details.aspx?id=53344)

Building
---------

To build from cmdline run **build.ps1** in PowerShell. Script builds Release configuration by default into the same directory. Script accepts *Configuration*, *OutputPath*, *Portable* (creates zip package) and *SkipBuild* parameters.

Contributions
---------

All contributions are welcome!

Regarding code styling, there are only a few major rules:
* private fields and properties should use camelCase (without underscore)
* all methods (private and public) should use PascalCase
* use spaces instead of tabs with 4 spaces width
* always encapsulate with brackets:
```
if (true)
{
    DoSomething()
}
```
instead of 
```
if (true)
    DoSomething()
```
