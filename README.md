[![.NET](https://github.com/aimenux/AcuWebSiteManager/actions/workflows/ci.yml/badge.svg)](https://github.com/aimenux/AcuWebSiteManager/actions/workflows/ci.yml)

# AcuWebSiteManager
```
Providing net global tool in order to manage acumatica website creation and deletion
```

> In this repo, i m building a global tool that allows to create or delete [acumatica](https://www.acumatica.com/) websites.
>
> The tool is based on multiple sub commmands :
> - Use sub command `CreateSite` to create site
> - Use sub command `DeleteSite` to delete site
> - Use sub command `ListSites` to list iis sites
> - Use sub command `SwitchDb` to switch site to use another database
> - Use sub command `ImportDb` to import bacpac file into target database
> - Use sub command `ExportDb` to export bacpac file from source database
>
> To run code in debug or release mode, type the following commands in your favorite terminal : 
> - `.\App.exe ListSites`
> - `.\App.exe CreateSite -x [XmlConfigFile]`
> - `.\App.exe DeleteSite -x [XmlConfigFile]`
> - `.\App.exe SwitchDb -s [CurrentWebSite] -d [TargetDatabaseName]`
> - `.\App.exe ImportDb -f [SourceBacPacFilePath] -s [SourceServerName] -d [TargetDatabaseName]`
> - `.\App.exe ExportDb -f [TargetBacPacFilePath] -s [SourceServerName] -d [SourceDatabaseName]`
>
> To install, run, update, uninstall global tool from a local source path, type commands :
> - `dotnet tool install -g --configfile .\Nugets\local.config AcuWebSiteManager`
> - `AcuWebSiteManager -h`
> - `AcuWebSiteManager -c`
> - `AcuWebSiteManager ListSites`
> - `AcuWebSiteManager CreateSite -h`
> - `AcuWebSiteManager DeleteSite -h`
> - `AcuWebSiteManager CreateSite -x [XmlConfigFile]`
> - `AcuWebSiteManager DeleteSite -x [XmlConfigFile]`
> - `AcuWebSiteManager SwitchDb -s [CurrentWebSite] -d [TargetDatabaseName]`
> - `AcuWebSiteManager ImportDb -f [SourceBacPacFilePath] -s [SourceServerName] -d [TargetDatabaseName]`
> - `AcuWebSiteManager ExportDb -f [TargetBacPacFilePath] -s [SourceServerName] -d [SourceDatabaseName]`
> - `dotnet tool update -g AcuWebSiteManager --ignore-failed-sources`
> - `dotnet tool uninstall -g AcuWebSiteManager`
>
> To install global tool from [nuget source](https://www.nuget.org/packages/AcuWebSiteManager), type these command :
> - `dotnet tool install -g AcuWebSiteManager --ignore-failed-sources`
>
>
> ![ListSitesScreen](Screenshots/ListSitesScreen.png)
>

**`Tools`** : net 8.0

---

<div style="display: flex; justify-content: space-between">
  <a href="./INSTALLATION-GUIDE.md"> ➡ See installation guide </a>
</div>