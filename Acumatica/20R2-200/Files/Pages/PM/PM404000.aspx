<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM404000.aspx.cs"
    Inherits="Page_PM404000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.BudgetOverrunInquiry" PrimaryView="Filter">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="EditDocument" Visible="False" DependOnGrid="grid">
			</px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edProjectID" NoteField="">
        <Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXDropDown ID="edDocType" runat="server" AllowMultiSelect="True" CommitChanges="True" DataField="DocType" />
			<px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edDateFrom" runat="server" DataField="DateFrom" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edDateTo" runat="server" DataField="DateTo" />
            <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" GroupCaption="Additional Filters" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edAccountGroupID" runat="server" DataField="AccountGroupID" />
            <px:PXSegmentMask CommitChanges="True" ID="edProjectTaskID" runat="server" DataField="TaskID" />
            <px:PXSelector CommitChanges="True" ID="edCostCode" runat="server" AutoRefresh="True" DataField="CostCodeID" />
            <px:PXSelector CommitChanges="True" ID="edInventory" runat="server" AutoRefresh="True" DataField="InventoryID" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" AdjustPageSize="Auto"
        AllowPaging="True" Caption="Budget Control Lines" FastFilterFields="RefNbr,Description" SyncPosition="True" RestrictFields="True">
        <Levels>
            <px:PXGridLevel DataMember="BudgetControlLines">
                <RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="DocType" />
					<px:PXGridColumn DataField="RefNoteID" LinkCommand="EditDocument"/>
					<px:PXGridColumn DataField="ProjectID" />
					<px:PXGridColumn DataField="TaskID" />
					<px:PXGridColumn DataField="CostCodeID" />
					<px:PXGridColumn DataField="InventoryID" />
					<px:PXGridColumn DataField="AccountGroupID" />
					<px:PXGridColumn DataField="BudgetedAmount" TextAlign="Right" />
					<px:PXGridColumn DataField="ConsumedAmount" TextAlign="Right" />
					<px:PXGridColumn DataField="AvailableAmount" TextAlign="Right" />
					<px:PXGridColumn DataField="DocumentAmount" TextAlign="Right" />
					<px:PXGridColumn DataField="RemainingAmount" TextAlign="Right" />
					<px:PXGridColumn DataField="CuryID" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
