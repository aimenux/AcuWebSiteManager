<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX501000.aspx.cs" Inherits="Page_TX501000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.TX.ReportTax" PrimaryView="Period_Header" PageLoadBehavior="PopulateSavedValues" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="Period_Header" Caption="Selection" TabIndex="3500">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
		    <px:PXSegmentMask ID="edOrganizationID" runat="server" DataField="OrganizationID" AllowEdit="true" CommitChanges="true" />
            <px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" AllowEdit="true" CommitChanges="true"  AutoRefresh="True"/>
			<px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" AllowEdit="true" CommitChanges="true"/>
			<px:PXSelector Size="S" ID="edTaxPeriodID" runat="server" DataField="TaxPeriodID" CommitChanges="true" AutoRefresh="True"/>
			<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" />
			<px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="150px" Width="100%" ActionsPosition="Top" Caption="Details" SkinID="PrimaryInquire">
		<Levels>
			<px:PXGridLevel DataMember="Period_Details">
				<Columns>
					<px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
					<px:PXGridColumn DataField="SortOrder" TextAlign="Right" />
					<px:PXGridColumn DataField="Descr" />
					<px:PXGridColumn AllowNull="False" DataField="TaxHistory__ReportUnfiledAmt" TextAlign="Right" MatrixMode="true" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
	</px:PXGrid>
</asp:Content>
