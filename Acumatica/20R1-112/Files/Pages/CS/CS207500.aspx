<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS207500.aspx.cs" Inherits="Page_CS207500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Width="100%" Visible="True" PrimaryView="Carrier" TypeName="PX.Objects.CS.CarrierMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Carrier" Caption="Ship Via Summary" FilesIndicator="True" NoteIndicator="True" 
		TemplateContainer="" TabIndex="7100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
			<px:PXSelector ID="edCarrierID" runat="server" DataField="CarrierID" DataSourceID="ds" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXCheckBox CommitChanges="True" ID="chkIsExternal" runat="server" DataField="IsExternal" />
		</Template>
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="300px" DataSourceID="ds" DataMember="CarrierCurrent">
		<Items>
			<px:PXTabItem Text="Details">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
					<px:PXSelector ID="edCalendarID" runat="server" DataField="CalendarID" AllowEdit="True" edit="1" />
					<px:PXSelector CommitChanges="True" ID="edCarrierPluginID" runat="server" DataField="CarrierPluginID" />
					<px:PXDropDown ID="edCalcMethod" runat="server" DataField="CalcMethod" CommitChanges="True" />
					<px:PXSelector ID="edPluginMethod" runat="server" DataField="PluginMethod" AutoRefresh="True" />
					<px:PXNumberEdit ID="edBaseRate" runat="server" DataField="BaseRate" />
					<px:PXCheckBox ID="chkIsCommonCarrier" runat="server" Checked="True" DataField="IsCommonCarrier"/>
					<px:PXCheckBox ID="chkCalcFreightOnReturn" runat="server" Checked="True" DataField="CalcFreightOnReturn" CommitChanges="true" />
					<px:PXCheckBox ID="chkConfirmationRequired" runat="server" Checked="True" DataField="ConfirmationRequired" />
					<px:PXCheckBox ID="chkPackageRequired" runat="server" Checked="True" DataField="PackageRequired" />
					<px:PXCheckBox ID="chkReturnLabel" runat="server" Checked="True" DataField="ReturnLabel" />
					<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AllowEdit="True" AutoRefresh="True" edit="1"/>
					<px:PXSegmentMask ID="edFreightSalesAcctID" runat="server" DataField="FreightSalesAcctID" CommitChanges="True" />
					<px:PXSegmentMask ID="edFreightSalesSubID" runat="server" DataField="FreightSalesSubID" />
					<px:PXSegmentMask ID="edFreightExpenseAcctID" runat="server" CommitChanges="True" DataField="FreightExpenseAcctID" />
					<px:PXSegmentMask ID="edFreightExpenseSubID" runat="server" DataField="FreightExpenseSubID" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Freight Rates" BindingContext="form" VisibleExp="DataControls[&quot;chkIsExternal&quot;].Value == false">
				<Template>
					<px:PXGrid ID="gridFreightRates" runat="server" Height="300px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="True" DataSourceID="ds" SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel DataMember="FreightRates" DataKeyNames="CarrierID,LineNbr">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
									<px:PXNumberEdit ID="edWeight" runat="server" DataField="Weight" />
									<px:PXNumberEdit ID="edVolume" runat="server" DataField="Volume" />
									<px:PXSelector ID="edZoneID" runat="server" DataField="ZoneID" AllowEdit="True" edit="1" />
									<px:PXNumberEdit ID="edRate" runat="server" DataField="Rate" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Weight" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="Volume" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="ZoneID" Width="100px" />
									<px:PXGridColumn DataField="Rate" TextAlign="Right" Width="100px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<Actions>
								<NoteShow Enabled="False" />
							</Actions>
						</ActionBar>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Packages">
				<Template>
					<px:PXGrid ID="gridCarrierPackages" runat="server" Height="300px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="True" DataSourceID="ds" SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel DataMember="CarrierPackages" DataKeyNames="CarrierID,BoxID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
									<px:PXSelector ID="edBoxID" runat="server" DataField="BoxID" />
									<px:PXTextEdit ID="edCSBox__Description" runat="server" DataField="CSBox__Description" />
								    <px:PXNumberEdit ID="edCSBox__BoxWeight" runat="server" DataField="CSBox__BoxWeight">
                                    </px:PXNumberEdit>
									<px:PXNumberEdit ID="edCSBox__MaxWeight" runat="server" DataField="CSBox__MaxWeight" />
									<px:PXNumberEdit ID="edCSBox__MaxVolume" runat="server" DataField="CSBox__MaxVolume">
                                    </px:PXNumberEdit>
									<px:PXNumberEdit ID="edCSBox__Length" runat="server" DataField="CSBox__Length" />
									<px:PXNumberEdit ID="edCSBox__Width" runat="server" DataField="CSBox__Width" />
									<px:PXNumberEdit ID="edCSBox__Height" runat="server" DataField="CSBox__Height" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="BoxID" Label="Box ID" Width="117px" />
									<px:PXGridColumn DataField="CSBox__Description" Label="Description" Width="500px" />
									<px:PXGridColumn DataField="CSBox__BoxWeight" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CSBox__MaxWeight" Label="Max. Weight" TextAlign="Right" Width="99px" />
									<px:PXGridColumn DataField="CommonSetup__WeightUOM" />
									<px:PXGridColumn DataField="CSBox__MaxVolume" Width="100px" TextAlign="Right" />
								    <px:PXGridColumn DataField="CommonSetup__VolumeUOM">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CSBox__Length" Label="Length" TextAlign="Right" Width="54px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CSBox__Width" Label="Width" TextAlign="Right" Width="54px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="CSBox__Height" Label="Height" TextAlign="Right" Width="54px">
                                    </px:PXGridColumn>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<Actions>
								<NoteShow Enabled="False" />
							</Actions>
						</ActionBar>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Advanced Fulfillment">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
					<px:PXCheckBox ID="chkValidatePackedQty" runat="server" Checked="True" DataField="ValidatePackedQty"/>
					<px:PXCheckBox ID="chkIsExternalShippingApplication" runat="server" Checked="True" DataField="IsExternalShippingApplication" CommitChanges="True"/>
					<px:PXDropDown ID="edShippingApplicationType" runat="server" DataField="ShippingApplicationType"/>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="180" />
	</px:PXTab>
</asp:Content>
