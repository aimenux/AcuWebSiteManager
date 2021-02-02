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
                    <px:PXGridColumn DataField="PMAllocationSourceTran__AllocationID" Width="120px"  LinkCommand="ViewAllocationRule"/>
                    <px:PXGridColumn DataField="PMAllocationSourceTran__StepID" Width="120px"/>
                    <px:PXGridColumn DataField="BranchID" Width="120px"/>
                    <px:PXGridColumn DataField="RefNbr" Width="90px" AutoCallBack="true" LinkCommand="ViewBatch"/>
                    <px:PXGridColumn DataField="Date" Width="90px"/>
                    <px:PXGridColumn DataField="FinPeriodID"/>
                    <px:PXGridColumn DataField="ProjectID" Width="90px"/>
                    <px:PXGridColumn DataField="TaskID" Width="90px"/>
                    <px:PXGridColumn DataField="AccountGroupID"/>
                    <px:PXGridColumn DataField="ResourceID" Width="120px"/>
                    <px:PXGridColumn DataField="BAccountID" Width="150px"/>
                    <px:PXGridColumn DataField="LocationID" Width="100px"/>
                    <px:PXGridColumn DataField="InventoryID" Width="150px"/>
                    <px:PXGridColumn DataField="Description" Width="200px"/>
                    <px:PXGridColumn DataField="UOM"/>
                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="100px"/>
                    <px:PXGridColumn DataField="Billable" TextAlign="Center" Type="CheckBox" Width="70px"/>
                    <px:PXGridColumn DataField="UseBillableQty" TextAlign="Center" Type="CheckBox" Width="130px"/>
                    <px:PXGridColumn DataField="BillableQty" TextAlign="Right" Width="100px"/>
                    <px:PXGridColumn DataField="UnitRate" TextAlign="Right" Width="100px"/>
                    <px:PXGridColumn DataField="Amount" TextAlign="Right" Width="100px"/>
                    <px:PXGridColumn DataField="AccountID"/>
                    <px:PXGridColumn DataField="SubID" Width="120px"/>
                    <px:PXGridColumn DataField="OffsetAccountID"/>
                    <px:PXGridColumn DataField="OffsetSubID" Width="120px"/>
                    <px:PXGridColumn DataField="Allocated" TextAlign="Center" Type="CheckBox" Width="70px"/>
                    <px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox" Width="70px"/>
                    <px:PXGridColumn DataField="BatchNbr" Width="90px"/>
                    <px:PXGridColumn DataField="OrigModule"/>
                    <px:PXGridColumn DataField="OrigTranType"/>
                    <px:PXGridColumn DataField="OrigRefNbr"  Width="90px"/>
                    <px:PXGridColumn DataField="OrigLineNbr" TextAlign="Right"/>
                    <px:PXGridColumn DataField="Billed" TextAlign="Center" Type="CheckBox" Width="60px"/>
                    <px:PXGridColumn DataField="Reversed" TextAlign="Center" Type="CheckBox"  Width="110px"/>
                    <px:PXGridColumn DataField="BilledDate" Width="90px"/>
                    <px:PXGridColumn DataField="StartDate" Width="90px"/>
                    <px:PXGridColumn DataField="EndDate" Width="90px"/>
                    <px:PXGridColumn DataField="Reverse" Width="110px" />
                    <px:PXGridColumn DataField="EarningType"/>
                    <px:PXGridColumn DataField="OvertimeMultiplier" TextAlign="Right" Width="100px"/>
                    <px:PXGridColumn DataField="ARRefNbr"  Width="150px"/>
                    <px:PXGridColumn DataField="Skip" TextAlign="Center" Type="CheckBox" Width="60px"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
