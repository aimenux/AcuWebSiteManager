![.NET 5](https://github.com/aimenux/AcuWebSiteManager/workflows/.NET%205/badge.svg)

# AcuWebSiteManager
```
Providing net global tool in order to manage acumatica website creation and deletion
```

> In this repo, i m building a global tool that allows to create or delete [acumatica](https://www.acumatica.com/) websites.
>
> The tool is based on 3 sub commmands :
> - Use sub command `CreateSite` to create site
> - Use sub command `DeleteSite` to delete site
> - Use sub command `ListSites` to list iis sites
>
> To run code in debug or release mode, type the following commands in your favorite terminal : 
> - `.\App.exe CreateSite -x [XmlConfigFile]`
> - `.\App.exe DeleteSite -x [XmlConfigFile]`
> - `.\App.exe ListSites`
>
> To install, run, uninstall global tool from a local source path, type commands :
> - `dotnet tool install -g --add-source .\Nugets\ --configfile .\Nugets\nuget.config AcuWebSiteManager`
> - `AcuWebSiteManager -h`
> - `AcuWebSiteManager ListSites`
> - `AcuWebSiteManager CreateSite -h`
> - `AcuWebSiteManager DeleteSite -h`
> - `AcuWebSiteManager CreateSite -x [XmlConfigFile]`
> - `AcuWebSiteManager DeleteSite -x [XmlConfigFile]`
> - `dotnet tool uninstall -g AcuWebSiteManager`
>

**`Tools`** : vs19, net 5.0