<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="RQ303000.aspx.cs" Inherits="Page_RQ301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" TypeName="PX.Objects.RQ.RQBiddingEntry"
		PrimaryView="Vendor" BorderStyle="NotSet" Width="100%">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="True" CommitChanges="True" />		
		</CallbackCommands>
	</px:PXDataSource>	
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Vendor" 
		 Caption="Bidding Response" 
        DefaultControlID="edReqNbr"   LinkIndicator="true" NotifyIndicator="true" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects">
<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

			<px:PXSelector runat="server" DataField="ReqNbr" ID="edReqNbr" />
			<px:PXSegmentMask runat="server" DataField="VendorID" ID="edVendorID"  />
			<px:PXSegmentMask runat="server" DataField="VendorLocationID" ID="edVendorLocationID" AutoRefresh="true" />
			<px:PXDateTimeEdit ID="edEntryDate" runat="server" DataField="EntryDate"  />
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="XM" />

            <pxa:PXCurrencyRate ID="edCury" runat="server" DataField="CuryID" 
                DataMember="_Currency_" DataSourceID="ds" 
                RateTypeView="_RQBiddingVendor_CurrencyInfo_"  />
			<px:PXNumberEdit ID="edTotalQuoteQty" runat="server" DataField="TotalQuoteQty" Enabled="False"  />
			<px:PXNumberEdit ID="edCuryTotalQuoteExtCost" runat="server" DataField="CuryTotalQuoteExtCost" Enabled="False"  /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="504px" Style="z-index: 100;" Width="100%" DataSourceID="ds" 
		 DataMember="CurrentDocument">
		<Items>
			<px:PXTabItem Text="Bidding Details">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; height: 423px;
						top: 0px; left: 0px; margin-bottom: 0px;" Width="100%"
						BorderWidth="0px" SkinID="Details" Height="400px">
						<Levels>
							<px:PXGridLevel DataMember="Lines" >
								<Columns>
									<px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" />
									<px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;A" />
									<px:PXGridColumn DataField="Description" />
									<px:PXGridColumn DataField="AlternateID" />									
									<px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa"  />
									<px:PXGridColumn DataField="OrderQty" AllowNull="False" TextAlign="Right" />						
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="MinQty" TextAlign="Right" />																					
									<px:PXGridColumn DataField="QuoteQty" AllowNull="False" TextAlign="Right" />									
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryQuoteUnitCost" TextAlign="Right" />
									<px:PXGridColumn DataField="QuoteNumber" />									
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryQuoteExtCost" TextAlign="Right" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<Layout FormViewHeight="400px" />
						<AutoSize Enabled="True" MinHeight="150" />												
					</px:PXGrid>
				</Template>
			</px:PXTabItem>			
			<px:PXTabItem Text="Vendor Info">
				<Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
                    <px:PXFormView ID="formVC" runat="server" Caption="Vendor Contact" AllowCollapse="false" DataMember="Remit_Contact" DataSourceID="ds" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                            <px:PXCheckBox CommitChanges="True" ID="chkOverrideContact" runat="server" DataField="OverrideContact" />
                            <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
                            <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
                            <px:PXTextEdit ID="edPhone1" runat="server" DataField="Phone1" />
                            <px:PXMailEdit ID="edEmail" runat="server" DataField="Email" />
                        </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                    </px:PXFormView>
                    <px:PXFormView ID="formVA" DataMember="Remit_Address" runat="server" AllowCollapse="false" DataSourceID="ds" Caption="Vendor Address" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                            <px:PXCheckBox CommitChanges="True" ID="chkOverrideAddress" runat="server" DataField="OverrideAddress" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                            <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" CommitChanges="true" />
                            <px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="true">
                                <CallBackMode PostData="Container" />
                                <Parameters>
                                    <px:PXControlParam ControlID="formVA" Name="PORemitAddress.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value"
                                        Type="String" />
                                </Parameters>
                            </px:PXSelector>
                            <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="true" />
                        </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />

					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Vendor Bidding" />

					<px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate"  />
					<px:PXDateTimeEdit ID="edPromisedDate" runat="server" DataField="PromisedDate"  />
					<px:PXSelector ID="edFOBPoint" runat="server" DataField="FOBPoint"  />
					<px:PXSelector ID="edShipVia" runat="server" DataField="ShipVia"  /></Template>
			</px:PXTabItem>			
		</Items>		
		<AutoSize Container="Window" Enabled="True" MinHeight="180" />
	</px:PXTab>
</asp:Content>
