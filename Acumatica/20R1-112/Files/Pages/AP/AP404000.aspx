<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP404000.aspx.cs" Inherits="Page_AP404000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.APChecksToPrintEnq" PrimaryView="Filter">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="ProcessPayment" DependOnGrid="grid" CommitChanges="True"/>
			<px:PXDSCallbackCommand Name="ViewCashAccount" DependOnGrid="grid" CommitChanges="True"/>
		</CallbackCommands>

	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector CommitChanges="True" ID="edPayTypeID" runat="server" DataField="PayTypeID" />
			<px:PXSegmentMask CommitChanges="True" ID="edPayAccountID" runat="server" DataField="PayAccountID" AutoRefresh="True"/>
			<px:PXDateTimeEdit CommitChanges="True" ID="edPayDate" runat="server" DataField="PayDate" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXNumberEdit ID="edBalance" runat="server" DataField="Balance" Enabled="False" />
			<px:PXNumberEdit ID="edCuryBalance" runat="server" DataField="CuryBalance" Enabled="False" />
			<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Enabled="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" Caption="Checks To Print Summary" SkinID="PrimaryInquire" SyncPosition="True" FastFilterFields="PayAccountID,PayAccountID_CashAccount_Descr,PayTypeID" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="Documents">
				<Columns>
					<px:PXGridColumn DataField="PayTypeID" />
					<px:PXGridColumn DataField="PayAccountID" LinkCommand="ViewCashAccount" />
					<px:PXGridColumn DataField="PayAccountID_CashAccount_Descr" />
					<px:PXGridColumn DataField="CuryID" />
					<px:PXGridColumn DataField="DocCount" TextAlign="Right" />
					<px:PXGridColumn AllowNull="False" DataField="CuryDocBal" TextAlign="Right" />
					<px:PXGridColumn AllowNull="False" DataField="DocBal" TextAlign="Right" AllowShowHide="False" Visible="False" />
					<px:PXGridColumn DataField="OverdueDocCount" TextAlign="Right" />
					<px:PXGridColumn AllowNull="False" DataField="OverdueDocBal" TextAlign="Right" AllowShowHide="False" Visible="False" />
					<px:PXGridColumn AllowNull="False" DataField="OverdueCuryDocBal" TextAlign="Right" />
					<px:PXGridColumn DataField="MinPayDate" />
					<px:PXGridColumn DataField="MaxPayDate" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	    <ActionBar DefaultAction="ProcessPayment"/>
	</px:PXGrid>
</asp:Content>
