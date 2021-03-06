# Installation guide

You can install Acumatica sites via their installer then their wizard. This way present some disadvantages :
>
> :one: If you are creating multiples sites with the same configuration always, wizard can be boring and taking a lot of time to put the same infos again and again.
>
> :two: When you delete acumatica site, multiple things are not properly deleted (database, files, app-pools, etc.)
>

AcuWebSiteManager is global tool tackling those issues :
>
> :one: Edit site infos once (through xml file) and install multiple times without editing infos everytime
>
> :two: Remove properly acumatica site whenever you want (all files, database, app-pool are removed)
>

First of all you need to install AcuWebSiteManager : `dotnet tool install -g AcuWebSiteManager --ignore-failed-sources`
>
Second you could use :
>
> :zap: either your acumatica installation path and acumatica wizard to build xml file adapted to your usage
>
> :zap: or follow the below steps :
>
>> :white_check_mark: Download [20R2-200.zip archive](https://dev.azure.com/aimenux/AcuDemos/_git/AcuAssets?path=%2FAssets%2F20R2-200.zip) (containing installation files for 20R2-200)
>>
>> :white_check_mark: Unzip archive in some folder on your disk (for example C:\Acumatica\20R2-200)
>>
>> :white_check_mark: Edit 20R2-200.xml and modify values for tags :
>

```xml
<Root>
  <dbsrvname Value="PUT-YOUR-SERVER-NAME" Description="Database Server Name" />
  <dbname Value="PUT-YOUR-DATABASE-NAME" Description="Database Name" />
  <upass Value="PUT-YOUR-SITE-PASSWORD" Description="Instance Password" />
  <iname Value="PUT-YOUR-SITE-NAME" Description="Instance Name" />
  <ipath Value="PUT-YOUR-ACUMATICA-INSTALL-PATH" Description="Instance Physical Files Path" />
  <svirtdir Value="PUT-YOUR-VIRTUAL-DIR-NAME" Description="Virtual Directory Name" />
  <spool Value="PUT-YOUR-APP-POOL-NAME" Description="Application Pool Name" />
</Root>
```

Now, that you have acumatica installation files for some version (20R2-200, etc.) and you have xml file prepared, you can run the following commands :
>
> :pushpin: To create instance site tape `AcuWebSiteManager CreateSite -x "C:\Acumatica\20R2-200\20R2-200.xml"`
>
> :pushpin: To delete instance site tape `AcuWebSiteManager DeleteSite -x "C:\Acumatica\20R2-200\20R2-200.xml"`
>
> :pushpin: To list instance sites tape `AcuWebSiteManager ListSites`
>

AcuWebSiteManager provides some others command in order to manipulate databases
>
> :pushpin: To export database to bacpac file tape `AcuWebSiteManager ExportDb -s "SERVER-NAME" -d "DATABASE-NAME" -f "BACPAC-FILE-PATH"`
>
> :pushpin: To import database from bacpac file tape `AcuWebSiteManager ImportDb -s "SERVER-NAME" -d "DATABASE-NAME" -f "BACPAC-FILE-PATH"`
>

You find bellow a complete example of xml file (the format is specific to acumatica)
>
> :pushpin: Only the tag `upass` does not exist in the default acumatica xml file (this tag is custom to AcuWebSiteManager tool)
>
> :pushpin: The tag `upass` is optional, if the tag `upass` is missed AcuWebSiteManager tool will use `Acumatica` as default password
>

```xml
<Root>
  <configmode Value="NewInstance" Description="Configuration Mode" />
  <saasmode Value="No" Description="SaaS mode" />
  <vstemplates Value="Yes" Description="Install Visual Studio Templates" />
  <dbsrvtype Value="MSSqlServer" Description="Database Server Type" />
  <dbsrvname Value="PUT-YOUR-SERVER-NAME" Description="Database Server Name" />
  <dbsrvwinauth Value="Yes" Description="Database Server Authentication Type" />
  <dbsrvtimeout Value="30" Description="Database Server Timeout" />
  <dbnew Value="Yes" Description="Database" />
  <dbname Value="PUT-YOUR-DATABASE-NAME" Description="Database Name" />
  <dbupdate Value="No" Description="Database Update" />
  <dbmode Value="NotSet" Description="Insert Template Data into Database" />
  <dbsize Value="1" Description="Size of the Database" />
  <dbskip Value="No" Description="Skip Database Setup" />
  <dbshrink Value="No" Description="Shrink Database" />
  <dboptimize Value="No" Description="Optimize tables on MySql" />
  <upass Value="PUT-YOUR-SITE-PASSWORD" Description="Instance Password" />
  <iname Value="PUT-YOUR-SITE-NAME" Description="Instance Name" />
  <ipath Value="PUT-YOUR-ACUMATICA-INSTALL-PATH" Description="Instance Physical Files Path" />
  <icount Value="1" Description="Instance SSL Certificate Trumbprint" />
  <vmsize Value="Small" Description="Size of Azure Virtual Machine" />
  <coursetemplate Value="T210" Description="Training Course" />
  <webserver Value="localhost" Description="Web Server Name" />
  <swebsite Value="Default Web Site" Description="Web Site Name" />
  <svirtdir Value="PUT-YOUR-VIRTUAL-DIR-NAME" Description="Virtual Directory Name" />
  <sactions Value="NotSet" Description="Account to Access ASP.NET Application" />
  <spool Value="PUT-YOUR-APP-POOL-NAME" Description="Application Pool Name" />
  <spoolmode Value="Integrated" Description="Application Pool Pipiline Mode" />
  <spoolauth Value="NotSet" Description="Application pool Authintication type" />
  <dbwinauth Value="Yes" Description="DB Connection Authentication Type" />
  <dbnewuser Value="Yes" Description="DB Connection" />
  <adminchange Value="No" Description="Administrator Must Change Password" />
  <portal Value="No" Description="Deploy Tenant Portal" />
  <securemode Value="No" Description="Secure Tenant on Login Form" />
  <company Description="Tenants Information">
    <Company CompanyID="1" Delete="No" ParentID="none" CompanyType="" Visible="No" LoginName="" />
    <Company CompanyID="2" Delete="No" ParentID="1" CompanyType="I100" Visible="Yes" LoginName="Company" />
  </company>
  <file Value="InstallConfig.xml" Description="Configuration File Name" />
  <instupgradebackup Value="Yes" Description="No backup instance before upgrade" />
  <output Value="Normal" Description="Output Mode" />
  <fulllog Value="No" Description="Full Logging Mode" />
</Root>
```

---

<div style="display: flex; justify-content: space-between">
  <a href="./README.md"> ⬅ Back To README </a>
</div>