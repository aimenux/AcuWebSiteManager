<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Login.master" ClientIDMode="Static" AutoEventWireup="true" 
	CodeFile="LoginRemind.aspx.cs" Inherits="Frames_LoginRemind" EnableEventValidation="false" ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/Login.master" %>

<asp:Content ID="Content3" ContentPlaceHolderID="phUser" runat="Server">
	<asp:TextBox ID="edEmail" runat="server" CssClass="login_user border-box" placeholder="Enter your email address" />
	<asp:DropDownList runat="server" ID="cmbCompany" CssClass="login_company border-box" />
	<asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="btnSubmit_Click" CssClass="login_button" />
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="phStart" runat="Server">
	<script type='text/javascript'>
		window.onload = function ()
		{
			var editor = document.form1['edEmail'];
			if (editor && !editor.readOnly) editor.focus();
		}
	</script>
</asp:Content>
