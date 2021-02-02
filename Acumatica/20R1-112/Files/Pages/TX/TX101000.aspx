<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX101000.aspx.cs" Inherits="Page_TX101000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Settings" TypeName="PX.Objects.TX.TaxImportSettings" BorderStyle="NotSet" SuspendUnloading="False">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="PXTab1" runat="server" Height="323px" Width="100%" DataMember="Settings" DataSourceID="ds">
		<Items>
			<px:PXTabItem Text="Tax Category Mapping">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXSelector ID="edTaxableCategoryID" runat="server" DataField="TaxableCategoryID" />
					<px:PXSelector ID="edFreightCategoryID" runat="server" DataField="FreightCategoryID" />
					<px:PXSelector ID="edServiceCategoryID" runat="server" DataField="ServiceCategoryID" />
					<px:PXSelector ID="edLaborCategoryID" runat="server" DataField="LaborCategoryID" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="States to Import">
				<Template>
					<px:PXGrid ID="grid" runat="server" Width="100%" AllowSearch="true" DataSourceID="ds" SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel DataMember="Items">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
									<px:PXMaskEdit ID="edStateCode" runat="server" DataField="StateCode" />
									<px:PXTextEdit ID="edStateName" runat="server" DataField="StateName" />
									<px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" />
									<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="StateCode" />
									<px:PXGridColumn DataField="StateName" />
									<px:PXGridColumn DataField="AccountID" />
									<px:PXGridColumn DataField="SubID" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="200" />
						<ActionBar PagerVisible="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="210" />
	</px:PXTab>
</asp:Content>
