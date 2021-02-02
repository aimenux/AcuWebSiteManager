<%@ Page Language="C#" AutoEventWireup="true" CodeFile="OpenItem.aspx.cs" Inherits="Api_OpenItem" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
	<style type="text/css">
		body {
			font-size: 9pt;
font-family: Arial,sans-serif, Tahoma, Verdana;
			padding: 2px;	
		}
		.PageInfoMenuItem {
			display: block;
			
    padding: 5px 1px 5px 5px;
			min-height: 16px;
			color: black;
			text-decoration: none;
			opacity: 0.7;
		}
		
		

.PageInfoMenuItem:hover, .PageInfoMenuItem.active
{
   background-color: #F5F5F5;
    border: 1px solid #BBBBBB;
    opacity: 1;
    padding: 4px 0 4px 4px;
}

	</style>
</head>
<body >
    <form id="form1" runat="server">
    <div ID="MenuContainer" runat="server">

    </div>
	<a href="OpenItem.aspx?action=assistant" class="PageInfoMenuItem">Install Assistant</a>
	
	
	

    </form>
	<iframe name="MenuAction" width="1px" height="1px" style="border: 0px;"></iframe>
</body>
</html>
