<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Trace.aspx.cs" Inherits="Api_Trace" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body style="padding-left:10px">
<h1>API Trace</h1>
	<asp:Table runat=server ID="TableContent" BackColor="#E8E8E8" CellPadding="3">
	<asp:TableHeaderRow BackColor="#9DC8C4">
	<asp:TableHeaderCell>Method</asp:TableHeaderCell>
	<asp:TableHeaderCell>View</asp:TableHeaderCell>
	<asp:TableHeaderCell>View Type</asp:TableHeaderCell>
	<asp:TableHeaderCell>New Rows</asp:TableHeaderCell>
	<asp:TableHeaderCell>Delta</asp:TableHeaderCell>
	<asp:TableHeaderCell>Status list</asp:TableHeaderCell>
	<asp:TableHeaderCell>Graph</asp:TableHeaderCell>
	<asp:TableHeaderCell>Graph Instance</asp:TableHeaderCell>
	</asp:TableHeaderRow>
	</asp:Table>
    <pre id="ContentView" runat="server"></pre>

</body>
</html>
