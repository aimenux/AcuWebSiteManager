<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR404600.aspx.cs" Inherits="Page_AR404600" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARStatementForCustomer" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edCustomerID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
			<px:PXSelector CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Caption="Details" AllowSearch="True" AllowPaging="True" AdjustPageSize="Auto" 
        Height="150px" Style="z-index: 100" Width="100%" SkinID="PrimaryInquire" RestrictFields="True" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="Details">
				<Columns>
					<px:PXGridColumn DataField="BranchID" Width="80px" />
					<px:PXGridColumn DataField="StatementCycleId" Width="90px"/>
					<px:PXGridColumn DataField="StatementDate" Width="100px" />
					<px:PXGridColumn DataField="StatementBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="OverdueBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryID" AllowShowHide="Server" Width="80px" />
					<px:PXGridColumn DataField="CuryStatementBalance" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryOverdueBalance" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="DontPrint" TextAlign="Center" Width="60px" Type="CheckBox" />
					<px:PXGridColumn DataField="Printed" TextAlign="Center" Width="65px" Type="CheckBox" />
					<px:PXGridColumn DataField="DontEmail" TextAlign="Center" Width="60px" Type="CheckBox" />
					<px:PXGridColumn DataField="Emailed" TextAlign="Center" Width="70px" Type="CheckBox" />
                    <px:PXGridColumn DataField="OnDemand" TextAlign="Center" Width="78px" Type="CheckBox" />
                    <px:PXGridColumn DataField="PreparedOn" Width="80px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
	</px:PXGrid>
</asp:Content>
