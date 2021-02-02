<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR404300.aspx.cs" Inherits="Page_AR404300" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARStatementDetails" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edStatementCycleId">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector CommitChanges="True" ID="edStatementCycleId" runat="server" DataField="StatementCycleId" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edStatementDate" runat="server" DataField="StatementDate" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Caption="Details" AllowSearch="True" AllowPaging="True" AdjustPageSize="Auto" Height="150px" 
		 Width="100%" SkinID="PrimaryInquire" RestrictFields="True" SyncPosition ="true" FastFilterFields ="CustomerID, CustomerID_BAccountR_acctName">
		<Levels>
			<px:PXGridLevel DataMember="Details">
				<Columns>
					<px:PXGridColumn DataField="CustomerID" AllowUpdate="False" SortDirection="Ascending" LinkCommand ="ViewDetails" />
					<px:PXGridColumn DataField="CustomerID_BAccountR_acctName" />
					<px:PXGridColumn DataField="StatementBalance"  TextAlign="Right" />
					<px:PXGridColumn DataField="OverdueBalance"  TextAlign="Right" />
					<px:PXGridColumn DataField="CuryID" />
					<px:PXGridColumn DataField="CuryStatementBalance"  TextAlign="Right" />
					<px:PXGridColumn DataField="CuryOverdueBalance"  TextAlign="Right" />
					<px:PXGridColumn DataField="UseCurrency" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="DontPrint" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="Printed" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="DontEmail" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="Emailed" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="OnDemand" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="PreparedOn" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="ViewDetails" />
	</px:PXGrid>
</asp:Content>
