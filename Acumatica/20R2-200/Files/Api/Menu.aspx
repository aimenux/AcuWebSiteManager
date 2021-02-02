<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Menu.aspx.cs" Inherits="Api_Menu" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
	a
	{
		text-decoration:none;
	}
    a:hover
    {
    	text-decoration:underline;
    }
    a:visited
    {
    	
    	color:Blue;
    }
    </style>
</head>
<body style="padding:10px;font-size:14pt;">
    	<asp:HyperLink runat="server" NavigateUrl="~/Api/Bind.aspx" Target="content">&raquo; Bind Profiler</asp:HyperLink>
    	<br />
    	<asp:HyperLink runat="server" NavigateUrl="~/Api/Trace.aspx"  Target="content">&raquo; View Trace</asp:HyperLink>
    	<br />
    	<asp:HyperLink runat="server" NavigateUrl="~/Api/Cache.aspx"  Target="cacheView">&raquo; View Cache</asp:HyperLink>
    	
    	<br />
    	<asp:HyperLink runat="server" NavigateUrl="~/Api/Logger.aspx"  Target="content">&raquo; View Code</asp:HyperLink>
    	
    	<br />
    	<asp:HyperLink runat="server" NavigateUrl="~/Api/Adapters.aspx"  Target="cacheView">&raquo; Adapters</asp:HyperLink>
    	

    <form id="form1" runat="server">
    <div>
		<br />
		<asp:Button ID="ButtonClearTrace" runat="server" Text="Clear Trace" 
			onclick="ButtonClearTrace_Click" />
		<br />
		<asp:Button ID="ButtonClearCaches" runat="server" Text="Clear Caches" 
			onclick="ButtonClearCaches_Click" />
		<br />
		<asp:CheckBox ID="CheckPaused" runat="server" 
			oncheckedchanged="CheckPaused_CheckedChanged" AutoPostBack="True" /> 
		<asp:Label AssociatedControlID="CheckPaused" ID="Label1" runat="server" Text="Paused"></asp:Label> 
    </div>
    </form>
</body>
</html>
