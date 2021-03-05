# Acumatica xml config file sample

> The xml file bellow can be used (after parametrization) in order to install acumatica instance site. You can also got the xml file from acumatica configuration wizard.

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
  <upass Value="PUT-YOUR-SITE-PASSWORD" Description="Instance Name" />
  <iname Value="PUT-YOUR-SITE-NAME" Description="Instance Name" />
  <ipath Value="PUT-YOUR-PATH-ACUMATICA-FILES" Description="Instance Physical Files Path" />
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