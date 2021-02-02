<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS200000.aspx.cs" Inherits="Page_FS200000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="WrkProcessRecords" 
        TypeName="PX.Objects.FS.WrkProcess">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" Visible="False" />
<px:PXDSCallbackCommand Name="Cancel" PopupCommand="" PopupCommandTarget="" PopupPanel="" 
                Text="" Visible="False"></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="False" />
<px:PXDSCallbackCommand Name="CopyPaste" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="Delete" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" 
                Visible="False" />
			<px:PXDSCallbackCommand Name="Previous" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="Next" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="Last" PostData="Self" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Height="200px" 
        Style="z-index: 100" Width="100%" DataMember="WrkProcessRecords" TabIndex="3600">
		<Template>
            <px:PXLayoutRule runat="server" StartRow="True">
            </px:PXLayoutRule>
            <px:PXNumberEdit ID="edProcessID" runat="server" DataField="ProcessID">
            </px:PXNumberEdit>
        </Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXFormView>
</asp:Content>
