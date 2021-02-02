<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM202500.aspx.cs" Inherits="Page_AM202500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.AM.OvhdMaint" BorderStyle="NotSet" PrimaryView="OvhdRecords" Visible="true">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Width="100%" AllowPaging="True"  AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary">
	    <Levels>
			<px:PXGridLevel DataMember="OvhdRecords" DataKeyNames="OvhdID">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                    <px:PXMaskEdit ID="edOvhdID" runat="server" DataField="OvhdID" CommitChanges="True" />
                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
                    <px:PXDropDown ID="edOvhdType" runat="server" DataField="OvhdType" />
                    <px:PXNumberEdit ID="edCostRate" runat="server" DataField="CostRate" />
                    <px:PXSegmentMask ID="edAcctID" runat="server" DataField="AcctID" />
                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID"/>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="OvhdID" Width="110px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="Descr" Width="275px" />
                    <px:PXGridColumn DataField="OvhdType" RenderEditorText="True" Width="200px" />
                    <px:PXGridColumn DataField="CostRate" TextAlign="Right" Width="115px" />
                    <px:PXGridColumn DataField="AcctID" Width="120px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="SubID" Width="120px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar ActionsText="False" >
        </ActionBar>
	</px:PXGrid>
</asp:Content>
