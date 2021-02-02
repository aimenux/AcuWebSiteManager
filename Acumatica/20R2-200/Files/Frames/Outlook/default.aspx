<%@ Page Language="C#" AutoEventWireup="true" CodeFile="default.aspx.cs" Inherits="Frames_Outlook_default"   %><!DOCTYPE html>
<html>
<head runat="server">
	<title></title>
	<script src="//appsforoffice.microsoft.com/lib/1.1/hosted/office.js" type="text/javascript"></script>


</head>

<body no-enhance aurelia-app="apps/outlook-plugin/main">
	<%= ClientSideAppsHelper.RenderScriptConfiguration() %>	
</body>
</html>
