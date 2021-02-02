<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM604000.aspx.cs" Inherits="Page_SM604000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="contDataSource" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds"
                     runat="server"
                     TypeName="PX.Data.Licensing.SM.SMLicenseManagment"
                     PrimaryView="MainInformationFilter">
	</px:PXDataSource>
</asp:Content>
