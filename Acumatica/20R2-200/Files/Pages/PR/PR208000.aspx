<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR208000.aspx.cs" Inherits="Page_PR208000" Title="Tax Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PR.PRTaxMaintenance" PrimaryView="FakeView">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="ViewTaxDetails" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Width="100%" DataMember="FakeView">
		<Items>
			<px:PXTabItem Text="Tax Codes">
				<Template>
					<px:PXSplitContainer ID="splitContainerTaxes" runat="server" PositionInPercent="true" SplitterPosition="65" Orientation="Vertical">
						<Template1>
							<px:PXGrid runat="server" ID="grdTaxes" SkinID="Inquire" SyncPosition="true" DataSourceID="ds" Width="100%"
								Caption="Tax Codes" CaptionVisible="true">
								<Levels>
									<px:PXGridLevel DataMember="Taxes">
										<Columns>
											<px:PXGridColumn DataField="TaxCD" />
											<px:PXGridColumn DataField="Description" Width="180px" />
											<px:PXGridColumn DataField="TaxState" Width="60px" />
											<px:PXGridColumn DataField="TaxCategory" Width="150px" />
											<px:PXGridColumn DataField="BAccountID" Width="120px" />
											<px:PXGridColumn DataField="ExpenseAcctID" Width="70px" CommitChanges="true" />
											<px:PXGridColumn DataField="ExpenseSubID" Width="70px" CommitChanges="true" />
											<px:PXGridColumn DataField="LiabilityAcctID" Width="70px" CommitChanges="true" />
											<px:PXGridColumn DataField="LiabilitySubID" Width="70px" CommitChanges="true" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<ActionBar>
									<CustomItems>
										<px:PXToolBarButton CommandSourceID="ds" CommandName="ViewTaxDetails" Text="View Tax Details" />
									</CustomItems>
								</ActionBar>
								<AutoSize Container="Parent" Enabled="True" MinHeight="150" />
								<AutoCallBack Target="grdTaxAttributes" Command="Refresh" />
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXGrid ID="grdTaxAttributes" runat="server" DataSourceID="ds" Width="100%" SkinID="Inquire" MatrixMode="true" Caption="Tax Settings" CaptionVisible="true">
								<Levels>
									<px:PXGridLevel DataMember="TaxAttributes">
										<Columns>
											<px:PXGridColumn DataField="Description" Width="200px" />
											<px:PXGridColumn DataField="AllowOverride" TextAlign="Center" Type="CheckBox" Width="80px" />
											<px:PXGridColumn DataField="Value" Width="100px" CommitChanges="true" />
											<px:PXGridColumn DataField="Required" TextAlign="Center" Type="CheckBox" Width="80px" CommitChanges="true" />
											<px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="80px" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<AutoSize Container="Parent" Enabled="True" MinHeight="150" />
							</px:PXGrid>
						</Template2>
						<AutoSize Enabled="true" Container="Parent" MinHeight="150" />
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Company Tax Settings">
				<Template>
					<px:PXFormView ID="filterForm" runat="server" DataMember="CompanyAttributeFilter">
						<Template>
							<px:PXCheckBox ID="chkFilterStates" runat="server" DataField="FilterStates" CommitChanges="True" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="grdCompanyAttributes" runat="server" DataSourceID="ds" Width="100%" SkinID="Inquire" MatrixMode="True" Style="margin-top: 10px">
						<Levels>
							<px:PXGridLevel DataMember="CompanyAttributes">
								<Columns>
									<px:PXGridColumn DataField="Description" Width="400px" />
									<px:PXGridColumn DataField="State" Width="120px" />
									<px:PXGridColumn DataField="AllowOverride" TextAlign="Center" Type="CheckBox" Width="150px" CommitChanges="true" />
									<px:PXGridColumn DataField="Value" Width="200px" CommitChanges="true" />
									<px:PXGridColumn DataField="Required" TextAlign="Center" Type="CheckBox" Width="80px" CommitChanges="true" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Employees" Visible="false">
				<Template>
					<px:PXGrid ID="grdEmployees" runat="server" DataSourceID="ds" Width="100%" SkinID="Inquire" AllowPaging="true" AdjustPageSize="Auto">
						<Levels>
							<px:PXGridLevel DataMember="Employees">
								<Columns>
									<px:PXGridColumn DataField="AcctCD" />
									<px:PXGridColumn DataField="AcctName" />
									<px:PXGridColumn DataField="EmployeeClassID" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXTab>
	<px:PXSmartPanel runat="server" ID="spTaxDetails" AutoCallBack-Command="Refresh" AutoCallBack-Target="frmTaxDetails" 
		Caption="Tax Details" CaptionVisible="true" Key="CurrentTax" Width="600px" >
		<px:PXFormView runat="server" ID="frmTaxDetails" DataSourceID="ds" DataMember="CurrentTax" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" ControlSize="L" LabelsWidth="L" />
					<px:PXDropDown runat="server" ID="edTaxInvDescrType" DataField="TaxInvDescrType" CommitChanges="true" />
					<px:PXTextEdit runat="server" ID="edVndInvDescr" DataField="VndInvDescr" />
					<px:PXSelector runat="server" ID="edGovtRefNbr" DataField="GovtRefNbr" />
				<px:PXLayoutRule runat="server" ControlSize="L" LabelsWidth="L" GroupCaption="Tax Engine Info" />
					<px:PXTextEdit runat="server" ID="edTypeName" DataField="TypeName_Description" />
					<px:PXTextEdit runat="server" ID="edTaxUniqueCode" DataField="TaxUniqueCode" />
					<px:PXDropDown runat="server" ID="edJurisdictionLevel" DataField="JurisdictionLevel" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="spTaxDetailsButtons" runat="server" SkinID="Buttons">
			<px:PXButton ID="taxDetailsOkButton" runat="server" DialogResult="OK" Text="OK" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
