<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" 
ValidateRequest="false" CodeFile="NewTfsItem.aspx.cs" Inherits="Page_NewTfsItem" 
Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.ObjectsExt.IP.IPTfsItem" PrimaryView="Case">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="SaveCloseBug" CommitChanges="True" Visible="True" ClosePopup="True"/>  
            <px:PXDSCallbackCommand Name="SaveCloseReq" CommitChanges="True" Visible="True" ClosePopup="True"/>  
			<px:PXDSCallbackCommand Name="Close" Visible="True" ClosePopup="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataMember="Case"  FilesIndicator="False" NoteIndicator="False" >
		<Template>
                <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" SuppressLabel="True" ControlSize="XXL"/>
				<px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" Width="650"/>
				<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" TextMode="MultiLine" Width="650">
				    <AutoSize Enabled="True" Container="Window" />
				</px:PXTextEdit>                
        </Template>
        <AutoSize Enabled="True" Container="Window" />
	</px:PXFormView>
</asp:Content>
