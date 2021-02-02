<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM403000.aspx.cs" Inherits="Page_PM403000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont0" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.PM.AllocationAudit" PrimaryView="source">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewBatch" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewAllocationRule" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Inquire" TabIndex="100" >
		<Levels>
			<px:PXGridLevel DataKeyNames="TranID" DataMember="source">
                <Columns>
                    <px:PXGridColumn DataField="PMAllocationSourceTran__AllocationID"  LinkCommand="ViewAllocationRule"/>
                    <px:PXGridColumn DataField="PMAllocationSourceTran__StepID"/>
                    <px:PXGridColumn DataField="BranchID"/>
                    <px:PXGridColumn DataField="RefNbr" AutoCallBack="true" LinkCommand="ViewBatch"/>
                    <px:PXGridColumn DataField="Date"/>
                    <px:PXGridColumn DataField="FinPeriodID"/>
                    <px:PXGridColumn DataField="ProjectID"/>
                    <px:PXGridColumn DataField="TaskID"/>
                    <px:PXGridColumn DataField="AccountGroupID"/>
                    <px:PXGridColumn DataField="ResourceID"/>
                    <px:PXGridColumn DataField="BAccountID"/>
                    <px:PXGridColumn DataField="LocationID"/>
                    <px:PXGridColumn DataField="InventoryID"/>
                    <px:PXGridColumn DataField="Description"/>
                    <px:PXGridColumn DataField="UOM"/>
                    <px:PXGridColumn DataField="Qty" TextAlign="Right"/>
                    <px:PXGridColumn DataField="Billable" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="UseBillableQty" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="BillableQty" TextAlign="Right"/>
                    <px:PXGridColumn DataField="UnitRate" TextAlign="Right"/>
                    <px:PXGridColumn DataField="Amount" TextAlign="Right"/>
                    <px:PXGridColumn DataField="AccountID"/>
                    <px:PXGridColumn DataField="SubID"/>
                    <px:PXGridColumn DataField="OffsetAccountID"/>
                    <px:PXGridColumn DataField="OffsetSubID"/>
                    <px:PXGridColumn DataField="Allocated" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="BatchNbr"/>
                    <px:PXGridColumn DataField="OrigModule"/>
                    <px:PXGridColumn DataField="OrigTranType"/>
                    <px:PXGridColumn DataField="OrigRefNbr" />
                    <px:PXGridColumn DataField="OrigLineNbr" TextAlign="Right"/>
                    <px:PXGridColumn DataField="Billed" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="Reversed" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="BilledDate"/>
                    <px:PXGridColumn DataField="StartDate"/>
                    <px:PXGridColumn DataField="EndDate"/>
                    <px:PXGridColumn DataField="Reverse" />
                    <px:PXGridColumn DataField="EarningType"/>
                    <px:PXGridColumn DataField="OvertimeMultiplier" TextAlign="Right"/>
                    <px:PXGridColumn DataField="ARRefNbr" />
                    <px:PXGridColumn DataField="Skip" TextAlign="Center" Type="CheckBox"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
