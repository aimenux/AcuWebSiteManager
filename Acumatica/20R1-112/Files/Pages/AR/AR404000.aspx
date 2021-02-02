<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR404000.aspx.cs" Inherits="Page_AR404000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARStatementHistory" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="PrintReport" Visible="true" DependOnGrid="grid" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edStatementCycleId">
			<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector CommitChanges="True" ID="edStatementCycleId" runat="server" DataField="StatementCycleId" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" />
            <px:PXLayoutRule runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="XM" />
            <px:PXCheckBox CommitChanges="true" ID="edIncludeOnDemandStatements" runat="server" DataField="IncludeOnDemandStatements" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Caption="Details" AllowSearch="True" AdjustPageSize="Auto" Height="150px" Style="z-index: 100" Width="100%" 
		SkinID="PrimaryInquire" AllowPaging="True" RestrictFields="True" SyncPosition="true" FastFilterFields ="StatementCycleId,Descr">
		<Levels>
			<px:PXGridLevel DataMember="History">
				<Columns>
					<px:PXGridColumn DataField="StatementCycleId" SortDirection="Ascending" LinkCommand ="ViewDetails" />
					<px:PXGridColumn DataField="StatementDate" SortDirection="Descending" />
					<px:PXGridColumn DataField="Descr" />
					<px:PXGridColumn DataField="NumberOfDocuments" TextAlign="Right" />
					<px:PXGridColumn DataField="ToPrintCount" TextAlign="Right" />
					<px:PXGridColumn DataField="PrintedCount" TextAlign="Right" />
					<px:PXGridColumn DataField="ToEmailCount" TextAlign="Right" />
					<px:PXGridColumn DataField="EmailedCount" TextAlign="Right" />
					<px:PXGridColumn DataField="NoActionCount" TextAlign="Right" />
					<px:PXGridColumn DataField="EmailCompletion" TextAlign="Right" />
					<px:PXGridColumn DataField="PrintCompletion" TextAlign="Right" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="cmdViewDetails" />
	</px:PXGrid>
</asp:Content>
