<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX504000.aspx.cs" Inherits="Page_TX504000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.TX.TaxByZipEnq" PrimaryView="Filter">
		<CallbackCommands>
			
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Filter" Caption="Selection" LinkPage="">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edTaxDate" runat="server" DataField="TaxDate" />
			<px:PXSelector CommitChanges="True" ID="edTaxZoneID" runat="server" DataField="TaxZoneID" DataKeyNames="TaxZoneID" DataSourceID="ds" InputMask="&gt;aaaaaaaaaa" MaxLength="10" />
			<px:PXSelector CommitChanges="True" ID="edTaxID" runat="server" DataField="TaxID" DataKeyNames="TaxID" DataSourceID="ds" InputMask="&gt;aaaaaaaaaa" MaxLength="10" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" Caption="Details" SkinID="Details">
		<Levels>
			<px:PXGridLevel DataKeyNames="TaxZoneID,ZipCode,TaxID" DataMember="Records">
				<Columns>
					<px:PXGridColumn DataField="TaxZoneID" Label="Tax Zone ID" MaxLength="10" />
					<px:PXGridColumn DataField="ZipCode" Label="Zip Code" MaxLength="9" />
					<px:PXGridColumn DataField="TaxID" DisplayFormat="&gt;aaaaaaaaaa" Label="Tax ID" MaxLength="10" />
					<px:PXGridColumn DataField="Descr" Label="Description" MaxLength="60" />
					<px:PXGridColumn AllowNull="False" DataField="TaxRate" Label="Tax Rate" TextAlign="Right" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
