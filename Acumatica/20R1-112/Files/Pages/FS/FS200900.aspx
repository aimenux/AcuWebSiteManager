<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS200900.aspx.cs" Inherits="Page_FS200900" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="LicenseTypeRecords" 
        TypeName="PX.Objects.FS.LicenseTypeMaint">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Insert" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="CopyPaste" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="Delete" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="First" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="Previous" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="Next" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="Last" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
        AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" 
        SkinID="Primary" TabIndex="1900">
        <Levels>
            <px:PXGridLevel DataMember="LicenseTypeRecords" DataKeyNames="LicenseTypeCD">
                <RowTemplate>
                    <px:PXMaskEdit ID="edLicenseTypeCD" runat="server" DataField="LicenseTypeCD" CommitChanges="True">
                    </px:PXMaskEdit>
                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr">
                    </px:PXTextEdit>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="LicenseTypeCD" CommitChanges="True">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Descr">
                    </px:PXGridColumn>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar ActionsText="False">
        </ActionBar>
        <Mode AllowUpload="True" />
    </px:PXGrid>
</asp:Content>