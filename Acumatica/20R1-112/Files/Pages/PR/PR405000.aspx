<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR405000.aspx.cs" Inherits="Page_PR405000" Title="Payroll Stubs" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PR.PRPayStubInq" PrimaryView="Filter">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataMember="Filter" DataSourceID="ds" Style="z-index: 100"
		Width="100%">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" />
			<px:PXSelector runat="server" ID="edEmployeeID" DataField="EmployeeID" CommitChanges="true"></px:PXSelector>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" Height="150px" SkinID="PrimaryInquire" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="PayChecks">
				<Columns>
					<px:PXGridColumn DataField="TransactionDate" />
					<px:PXGridColumn DataField="DocType" />
					<px:PXGridColumn DataField="RefNbr" LinkCommand="viewStubReport" />
					<px:PXGridColumn DataField="NetAmount" />
					<px:PXGridColumn DataField="GrossAmount" />
					<px:PXGridColumn DataField="StartDate" />
					<px:PXGridColumn DataField="EndDate" />
					<px:PXGridColumn DataField="TotalHours" />
					<px:PXGridColumn DataField="AverageRate" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
