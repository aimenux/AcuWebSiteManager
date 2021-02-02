<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA507000.aspx.cs"
	Inherits="Page_FA507000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FA.TransferProcess" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" DefaultControlID="edPeriodID"
		NoteField="" Caption="Options" TabIndex="3100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edOrganizationID" runat="server" DataField="OrganizationID" MarkRequired="dynamic"/>
			<px:PXDateTimeEdit CommitChanges="True" ID="edTransferDate" runat="server" DataField="TransferDate" />
			<px:PXSelector CommitChanges="True" ID="edPeriodID" runat="server" DataField="PeriodID" Size="S" AutoRefresh="true"/>
			<px:PXTextEdit ID="edReason" runat="server" DataField="Reason" />
			<px:PXPanel ID="PXPanel1" runat="server" RenderSimple="True" RenderStyle="Simple">
				<px:PXLayoutRule runat="server" GroupCaption="Asset Transfer From" StartGroup="True" LabelsWidth="S" ControlSize="XM" />
				<px:PXSegmentMask ID="edBranchFrom" runat="server" CommitChanges="True" DataField="BranchFrom" DataSourceID="ds" AutoRefresh="true"/>
				<px:PXSelector ID="edDepartmentFrom" runat="server" CommitChanges="True" DataField="DepartmentFrom" DataSourceID="ds" />
				<px:PXSelector ID="edClassFrom" runat="server" CommitChanges="True" DataField="ClassFrom" DataSourceID="ds" />
				<px:PXLayoutRule runat="server" GroupCaption="To" StartColumn="True" StartGroup="True" LabelsWidth="S" ControlSize="XM" />
				<px:PXSegmentMask ID="edBranchTo" runat="server" CommitChanges="True" DataField="BranchTo" DataSourceID="ds" AutoRefresh="true"/>
				<px:PXSelector ID="edDepartmentTo" runat="server" CommitChanges="True" DataField="DepartmentTo" DataSourceID="ds" />
				<px:PXSelector ID="edClassTo" runat="server" CommitChanges="True" DataField="ClassTo" DataSourceID="ds" />
			</px:PXPanel>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" AdjustPageSize="Auto"
		AllowPaging="True" Caption="Fixed Assets" AllowSearch="True" FastFilterFields="AssetCD, Description" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataKeyNames="AssetCD" DataMember="Assets">
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="BranchID" />
					<px:PXGridColumn DataField="ClassID" />
					<px:PXGridColumn DataField="AssetCD" />
					<px:PXGridColumn DataField="Description" />
					<px:PXGridColumn DataField="ParentAssetID" />
					<px:PXGridColumn AllowNull="False" DataField="FADetailsTransfer__CurrentCost" TextAlign="Right" />
					<px:PXGridColumn DataField="FADetailsTransfer__ReceiptDate" />
					<px:PXGridColumn DataField="UsefulLife" TextAlign="Right" />
					<px:PXGridColumn DataField="FADetailsTransfer__TransferPeriodID" />
					<px:PXGridColumn DataField="FAAccountID" />
					<px:PXGridColumn DataField="FASubID" />
					<px:PXGridColumn DataField="FADetailsTransfer__TagNbr" />
					<px:PXGridColumn DataField="Account__AccountClassID" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
		<ActionBar PagerVisible="False" />
	</px:PXGrid>
</asp:Content>
