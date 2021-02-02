<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Adapters.aspx.cs" Inherits="Api_Adapters" ValidateRequest="False" EnableViewStateMac="False" EnableViewState="False" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
	<h1>Export/Import Adapters</h1>
    <form id="form1" runat="server">
    <div>
		<asp:TextBox ID="EditDocument" runat="server" Height="314px" Width="701px" 
			Font-Names="Courier New" Font-Size="10pt" TextMode="MultiLine"></asp:TextBox>    
		<br /><asp:Button ID="SaveButton" runat="server" Text="Save" Width="100px" 
			onclick="SaveButton_Click" />
		<asp:Button ID="ImportButton" runat="server" Text="Import Trace" Width="100px" 
			onclick="ImportButton_Click" />
		<asp:Button ID="ButtonTestExport" runat="server" Text="Test export" 
			Width="100px" onclick="ButtonTestExport_Click" 
			 />

		<asp:Button ID="ButtonTestImport" runat="server" Text="Test import" 
			Width="100px" onclick="ButtonTestImport_Click" 
			 />


		</div>
    </form>
</body>
</html>
