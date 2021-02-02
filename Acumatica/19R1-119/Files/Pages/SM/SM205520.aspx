<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM205520.aspx.cs" Inherits="Pages_SM205520" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Audit" TypeName="PX.SM.AUAuditExplore">
		<CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="AUAuditSetup_View" Visible="False"
                DependOnGrid="gridAudit" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="gridAudit" runat="server" Width="100%" Style="z-index: 100"
        AllowPaging="True" AdjustPageSize="Auto" DataSourceID="ds"
        SkinID="Inquire">
        <Levels>
            <px:PXGridLevel DataMember="Audit">
                <Columns>                    
                    <px:PXGridColumn DataField="ScreenID" DisplayFormat="CC.CC.CC.CC" Width="150" LinkCommand="AUAuditSetup_View" />
										<px:PXGridColumn DataField="ScreenName" Width="200" />
                    <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="Description" Width="200" />
                    <px:PXGridColumn DataField="CreatedByID" Width="150" />
                    <px:PXGridColumn DataField="CreatedDateTime" Width="150" />
                </Columns>
            </px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar DefaultAction="View">
            <Actions>
                <ExportExcel Enabled="False" />
            </Actions>
            <CustomItems>
                <px:PXToolBarButton Text="View" Key="View">
                    <AutoCallBack Command="AUAuditSetup_View" Target="ds" />
                    <PopupCommand Command="Refresh" Target="gridAudit" />
                    <ActionBar GroupIndex="0" Order="0" />
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" AllowRowSelect="False" />
	</px:PXGrid>
</asp:Content>
