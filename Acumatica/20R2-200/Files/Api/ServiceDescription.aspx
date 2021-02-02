<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ServiceDescription.aspx.cs" Inherits="Api_ServiceDescription" EnableViewState="False" EnableViewStateMac="False" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Api service description</title>
	<style>
	TH
	{
		padding:5px;
		background-color: #ddd;
		}
	</style>
</head>
<body>
<h1 style="border-bottom:2px solid black;">Acumatica Webservice API</h1>
    <form id="form1" runat="server">
    <div>
    <a href="ServiceDescription.aspx?WSDL">WSDL</a>
    <br />
    <a href="ServiceDescription.aspx?WSDL&M=10">Short</a>
    <br />
    <a href="ServiceDescription.aspx?content=reference">Api reference</a>
    <br />
    <a href="Frameset.htm">Trace</a>
    </div>
    </form>
    
    <div ID="DivGraphs" runat="server" Visible="false">
    <h2>Site map</h2>
	<asp:Table ID="TableGraphs" runat="server">
	<asp:TableHeaderRow  HorizontalAlign="Left">
		<asp:TableHeaderCell>Screen ID</asp:TableHeaderCell>
		<asp:TableHeaderCell>Graph</asp:TableHeaderCell>
		<asp:TableHeaderCell>Api alias</asp:TableHeaderCell>
		<asp:TableHeaderCell>Title</asp:TableHeaderCell>
	</asp:TableHeaderRow>
	</asp:Table> 
	</div>

	<div ID="DivGraph" runat="server" Visible="false">

	<h2 ID="HeaderGraph" runat="server" style="background-color:#ddd"></h2>
	<h3>Views</h3>
	<asp:Table ID="TableGraphViews" runat="server"  >
	<asp:TableHeaderRow HorizontalAlign="Left">
		<asp:TableHeaderCell>Name</asp:TableHeaderCell>
		<asp:TableHeaderCell>Row Type</asp:TableHeaderCell>
		<asp:TableHeaderCell>Parameters</asp:TableHeaderCell>
	</asp:TableHeaderRow>
	</asp:Table>  
	
	<h3>Actions</h3>
	<asp:Table ID="TableGraphActions" runat="server" >
	<asp:TableHeaderRow HorizontalAlign="Left">
		<asp:TableHeaderCell>Name</asp:TableHeaderCell>
		<asp:TableHeaderCell>Applied to</asp:TableHeaderCell>
	</asp:TableHeaderRow>
	</asp:Table>  	
	</div> 	
	<div ID="DivMembers" runat="server" Visible="false">

	<h2 ID="HeaderMembers" runat="server" style="background-color:#ddd;"></h2>
	<asp:Table ID="TableMembers" runat="server" style="white-space:nowrap;">
	<asp:TableHeaderRow HorizontalAlign="Left">
		<asp:TableHeaderCell>Field Name</asp:TableHeaderCell>
		<asp:TableHeaderCell>Type</asp:TableHeaderCell>
		<asp:TableHeaderCell>Props</asp:TableHeaderCell>
		<asp:TableHeaderCell>Description</asp:TableHeaderCell>
		<asp:TableHeaderCell>Attributes</asp:TableHeaderCell>
	</asp:TableHeaderRow>
	</asp:Table>  
	
	</div> 
</body>
</html>
