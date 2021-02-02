<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS207700.aspx.cs" Inherits="Page_CS207700" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CS.CarrierPluginMaint" PrimaryView="Plugin">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="Certify" CommitChanges="true" PostData="Page" Visible="false" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="false" />
            <px:PXDSCallbackCommand Name="BuyStampsPostage" Visible="False" />
			<px:PXDSCallbackCommand Name="GetStampsAccountInfo" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Carrier Plug-in Summary" DataMember="Plugin" FilesIndicator="True" NoteIndicator="True" TemplateContainer=""
		TabIndex="5100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edCarrierPluginID" runat="server" DataField="CarrierPluginID" DataSourceID="ds" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXSelector CommitChanges="True" ID="edPluginTypeName" runat="server" DataField="PluginTypeName" DataSourceID="ds" />
            <px:PXSelector runat="server" ID="edStampsReturnLabelNotification" DataField="ReturnLabelNotification" DisplayMode="Hint" AllowEdit="True" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
			<px:PXDropDown ID="edUnitType" runat="server" AllowNull="False" DataField="UnitType" CommitChanges="true" />

			<px:PXSelector ID="edKG" runat="server" DataField="KilogramUOM" DataSourceID="ds" AutoRefresh="True" CommitChanges="true" />
			<px:PXSelector ID="edPound" runat="server" DataField="PoundUOM" DataSourceID="ds" AutoRefresh="True" CommitChanges="true" />
			<px:PXSelector ID="edCM" runat="server" DataField="CentimeterUOM" DataSourceID="ds" AutoRefresh="True" CommitChanges="true" />
			<px:PXSelector ID="edInch" runat="server" DataField="InchUOM" DataSourceID="ds" AutoRefresh="True" CommitChanges="true" />
			<px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="365px">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Plug-in Parameters">
                <Template>
                    <px:PXGrid ID="PXGridSettings" runat="server" DataSourceID="ds" AllowFilter="False" Width="100%" SkinID="DetailsInTab" Height="100%" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Details">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXMaskEdit ID="edDetailID" runat="server" DataField="DetailID" />
                                    <px:PXTextEdit ID="edDescr" runat="server" AllowNull="False" DataField="Descr" />
                                    <px:PXTextEdit ID="edValue" runat="server" DataField="Value" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="DetailID" Width="90px" />
                                    <px:PXGridColumn AllowNull="False" DataField="Descr" Width="300px" />
                                    <px:PXGridColumn DataField="Value" Width="300px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar DefaultAction="gridSchedules">
                            <CustomItems>
                                <px:PXToolBarButton Text="Prepare Certification Files" Key="cert">
                                    <AutoCallBack Command="Certify" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Customer Accounts">
                <Template>
                    <px:PXGrid ID="PXGridAccounts" runat="server" DataSourceID="ds" AllowFilter="False" Width="100%" SkinID="DetailsInTab" Height="100%" FastFilterFields="CustomerID,CustomerID_description,CarrierAccount"
                        TabIndex="6900">
                        <Levels>
                            <px:PXGridLevel DataMember="CustomerAccounts">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
                                    <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" />
                                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" />
                                    <px:PXSegmentMask ID="edCustomerLocationID" runat="server" DataField="CustomerLocationID" AutoRefresh="True" />
                                    <px:PXTextEdit ID="edCarrierAccount" runat="server" DataField="CarrierAccount" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsActive" Label="Active" TextAlign="Center" Type="CheckBox" Width="40px" />
                                    <px:PXGridColumn DataField="CustomerID" Label="Customer" Width="100px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CustomerID_description" Width="200px" />
                                    <px:PXGridColumn DataField="CustomerLocationID" Width="100px" Label="Location" />
                                    <px:PXGridColumn DataField="CarrierAccount" Width="100px" />
                                    <px:PXGridColumn DataField="PostalCode" Width="100px">
                                    </px:PXGridColumn>
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Stamps.com Account Info" LoadOnDemand="True" RepaintOnDemand="False">
                <Template>
                    <px:PXGrid runat="server" Height="100%" SkinID="Inquire" Width="100%" ID="gridStampsInfo" AutoAdjustColumns="True" DataSourceID="ds">
                        <AutoSize Enabled="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="StampsAccountInfoRecord">
                                <Columns>
                                    <px:PXGridColumn DataField="AccountInfo" Width="40px" />
                                    <px:PXGridColumn DataField="AccountInfoValue" Width="70px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Buy Stamps.com Postage" CommandName="">
                                    <AutoCallBack Target="ds" Command="BuyStampsPostage" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Get Account Info">
                                    <AutoCallBack Command="GetStampsAccountInfo" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Territories Mapping" LoadOnDemand="True" RepaintOnDemand="False">
		        <Template>
			        <px:PXGrid runat="server" ID="gridTerritoriesMapping" Height="100%" SkinID="DetailsInTab" Width="100%" AutoAdjustColumns="True" DataSourceID="ds" SyncPosition="True">
				        <AutoSize Enabled="True" />
				        <Levels>
					        <px:PXGridLevel DataMember="ShipEngineTerritoriesMappings">
						        <Columns>
							        <px:PXGridColumn DataField="CountryID" Width="40px" CommitChanges="True" />
							        <px:PXGridColumn DataField="StateID" Width="40px" CommitChanges="True" />
							        <px:PXGridColumn DataField="StateName" Width="40px" />
							        <px:PXGridColumn DataField="CountryName" Width="40px" />
						        </Columns>
					        </px:PXGridLevel>
				        </Levels>
			        </px:PXGrid>
		        </Template>
            </px:PXTabItem>
        </Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="180" />
	</px:PXTab>
    <px:PXSmartPanel runat="server" ID="spBuyPostage" LoadOnDemand="true" AutoReload="true" CaptionVisible="true" Caption="Specify Postage Amount" Key="BuyPostageParamsView">
        <px:PXFormView runat="server" SkinID="Transparent" ID="BuyPostageFormView1" DefaultControlID="edBuyPostageAmount" DataMember="BuyPostageParamsView">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
                <px:PXNumberEdit runat="server" DataField="BuyPostageAmount" ID="edBuyPostageAmount" />
            </Template>
        </px:PXFormView>
        <px:PXLayoutRule runat="server" StartRow="True" />
        <px:PXPanel runat="server" SkinID="Buttons" ID="pnlButtons">
            <px:PXButton runat="server" Text="Buy" DialogResult="OK" ID="btnBuyOK" TabIndex="2" />
            <px:PXButton runat="server" Text="Cancel" DialogResult="Cancel" ID="btnBuyCancel" TabIndex="3" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
