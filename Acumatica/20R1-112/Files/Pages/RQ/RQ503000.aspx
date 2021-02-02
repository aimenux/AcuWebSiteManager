<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="RQ503000.aspx.cs" Inherits="Page_RQ503000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.Objects.RQ.RQBiddingProcess"
		PrimaryView="Document" BorderStyle="NotSet" Width="100%">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="process" CommitChanges="True" PostData="Self" RepaintControls="All"
				PopupVisible="true" StartNewGroup="true"/>			
			<px:PXDSCallbackCommand Name="updateResult" CommitChanges="true" PostData="Self" RepaintControls="All" PopupVisible="true"/>
			<px:PXDSCallbackCommand Name="clearResult" CommitChanges="true" PostData="Self" RepaintControls="All" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="chooseVendor" DependOnGrid="gridVendor" Visible="false" />
			<px:PXDSCallbackCommand Name="vendorInfo" DependOnGrid="gridVendor" Visible="false" CommitChanges="true" CommitChangesIDs="formVC, formVA" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
		</CallbackCommands>
	</px:PXDataSource>
	<px:PXSmartPanel runat="server" ID="panelVendor" CaptionVisible="true" Caption="Vendor Address" Key="Vendor" AutoCallBack-Command="Refresh"
		AutoCallBack-Enabled="True" AutoCallBack-Target="formVC">
		<px:PXPanel ID="panelVC" runat="server" Caption="Vendor Address" Height="360px" Style="margin-top: 5px;
			z-index: 100; margin-left: 5px; left: 0px; position: relative; top: 0px; width: 450px;">
			<px:PXFormView ID="formVC" runat="server" CaptionVisible="False" 
				DataMember="Remit_Contact" DataSourceID="ds" Width="100%">
				<CallbackCommands>
					<Refresh RepaintControlsIDs="formVA" RepaintControls="None" />
					<Save RepaintControls="None" />
				</CallbackCommands>
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

					<px:PXCheckBox CommitChanges="True" ID="chkOverrideContact" runat="server" DataField="OverrideContact" />
					<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName"  />
					<px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation"  />
					<px:PXTextEdit ID="edPhone1" runat="server" DataField="Phone1"  />
					<px:PXMailEdit ID="edEmail" runat="server" DataField="Email" />
                 </Template>
				<ContentStyle BackColor="Transparent" BorderStyle="None">
				</ContentStyle>
			</px:PXFormView>
			<px:PXFormView ID="formVA" DataMember="Remit_Address"  runat="server"
				DataSourceID="ds" Width="100%" CaptionVisible="False" Style="left: 0px;
				position: absolute; top: 144px">
				<CallbackCommands>
					<Save RepaintControls="None" />
				</CallbackCommands>
				<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />				
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

					<px:PXCheckBox CommitChanges="True" ID="chkOverrideAddress" runat="server" DataField="OverrideAddress" />
					<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1"  />
					<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2"  />
					<px:PXTextEdit ID="edCity" runat="server" DataField="City"  />
					<px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" CommitChanges="true" />
					<px:PXSelector ID="edState" runat="server" DataField="State"   AutoRefresh="true">
						<CallBackMode PostData="Container" />
						<Parameters>
							<px:PXControlParam ControlID="formVA" Name="PORemitAddress.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value"
								Type="String" />
						</Parameters>
					</px:PXSelector>
					<px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="true" /></Template>
				<ContentStyle BackColor="Transparent" BorderStyle="None">
				</ContentStyle>
			</px:PXFormView>
		</px:PXPanel>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnSave0" runat="server" DialogResult="OK" Text="OK" />			
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Document"  Caption="Requisition Summary"
		 NoteIndicator="True"  
		FilesIndicator="True" DefaultControlID="edReqNbr">
		
	    <Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="SM" />

			<px:PXSelector ID="edReqNbr" runat="server" DataField="ReqNbr" />			
			<px:PXDropDown Size="s" ID="edStatus" runat="server" AllowNull="False" DataField="Status" Enabled="False"  />
			<px:PXCheckBox CommitChanges="True" ID="chkSplittable" runat="server" Checked="True" DataField="Splittable" Enabled="False" />
			
			<px:PXDateTimeEdit ID="edOrderDate" runat="server" DataField="OrderDate"  />
			<px:PXDropDown ID="edPriority" runat="server" AllowNull="False" DataField="Priority" SelectedIndex="1"  />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />

			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description"  />
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />

			<px:PXSegmentMask ID="edEmployeeID" runat="server" DataField="EmployeeID" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" DataField="VendorLocationID" AutoRefresh="true" />
			<px:PXTextEdit ID="edVendorRefNbr" runat="server" DataField="VendorRefNbr"  />
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="XM" />

			<pxa:PXCurrencyRate ID="edCury" runat="server" DataField="CuryID" DataMember="_Currency_"
				DataSourceID="ds" RateTypeView="_RQRequisition_CurrencyInfo_"
				 />
			<px:PXNumberEdit ID="edCuryEstExtCostTotal" runat="server" DataField="CuryEstExtCostTotal" Enabled="False"  /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="400px">
		<Items>
			<px:PXTabItem Text="Bidding Vendors">
				<Template>
					<px:PXGrid ID="gridVendor" runat="server" DataSourceID="ds" Width="100%" SkinID="Details"
						BorderWidth="0px">	
						<Mode InitNewRow="true" />
						<ActionBar>
							<CustomItems>								
								<px:PXToolBarButton Text="Vendor Info" Tooltip="View vendor information" Key="vendor">
								    <AutoCallBack Command="vendorInfo" Target="ds">
										<Behavior CommitChanges="True" />
									</AutoCallBack>
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Choose" Tooltip="Choose selected vendor for all requisition" Key="Content">
								    <AutoCallBack Command="chooseVendor" Target="ds">
										<Behavior CommitChanges="True" />
									</AutoCallBack>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Vendors" >
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />

									<px:PXLayoutRule runat="server" Merge="True" />

									<px:PXSegmentMask ID="edBidVendorID" runat="server" DataField="VendorID" />
									<px:PXSelector Size="xxs" ID="edCuryID" runat="server" DataField="CuryID" Enabled="False"  />
									<px:PXLayoutRule runat="server" Merge="False" />

									<px:PXLayoutRule runat="server" Merge="True" />

									<px:PXSegmentMask ID="edBidVendorLocationID" runat="server" DataField="VendorLocationID" AutoRefresh="true">
										<Parameters>
						                    <px:PXSyncGridParam ControlID="gridVendor" />
						                </Parameters>
									</px:PXSegmentMask>
									<px:PXDateTimeEdit Size="s" ID="edEntryDate" runat="server" DataField="EntryDate"  />
									<px:PXLayoutRule runat="server" Merge="False" />

									<px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate"  />
									<px:PXDateTimeEdit ID="edPromisedDate" runat="server" DataField="PromisedDate"  />
									<px:PXSelector ID="edFOBPoint" runat="server" DataField="FOBPoint"  />
									<px:PXLayoutRule runat="server" Merge="True" />

									<px:PXSelector Size="s" ID="edShipVia" runat="server" DataField="ShipVia"  />
									<pxa:CurrencyEditor SuppressLabel="True" Size="s" ID="CurrencyEditor1" Hidden="true" DataField="CuryInfoID" runat="server" DataSourceID="ds" DataMember="_RQBiddingVendor_CurrencyInfo_" />
									<px:PXLayoutRule runat="server" Merge="False" />

									<px:PXNumberEdit ID="edTotalQuoteQty" runat="server" DataField="TotalQuoteQty"  />
									<px:PXNumberEdit ID="edCuryTotalQuoteExtCost" runat="server" DataField="CuryTotalQuoteExtCost"  /></RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="VendorID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="true" />
									<px:PXGridColumn DataField="VendorID_Vendor_AcctName" />
									<px:PXGridColumn DataField="VendorLocationID" DisplayFormat="&gt;AAAAAA" AutoCallback="true" />
									<px:PXGridColumn DataField="VendorLocationID_Location_Descr" />
									<px:PXGridColumn DataField="Location__VShipTermsID" />
									<px:PXGridColumn DataField="FOBPoint" DisplayFormat="&gt;aaaaaaaaaaaaaaa"  />
									<px:PXGridColumn DataField="Location__VLeadTime" TextAlign="Right" />									
									<px:PXGridColumn DataField="ShipVia" DisplayFormat="&gt;aaaaaaaaaaaaaaa"  />
									<px:PXGridColumn DataField="EntryDate" />
									<px:PXGridColumn DataField="ExpireDate" />
									<px:PXGridColumn DataField="PromisedDate" />
									<px:PXGridColumn AllowNull="False" DataField="TotalQuoteQty" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryInfoID" TextField="CuryID" />
									<px:PXGridColumn AllowNull="False" DataField="CuryTotalQuoteExtCost" TextAlign="Right" />
									<px:PXGridColumn DataField="RemitContactID" Visible="false" AllowShowHide="false" />
									<px:PXGridColumn DataField="RemitAddressID" Visible="false" AllowShowHide="false" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="true" />						
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Bidding Results" Overflow="Hidden" LoadOnDemand="true">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="200px" Panel1MinSize="175" Panel2MinSize="175">
                        <AutoSize Enabled="true" />
                        <Template1>
                            <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" Caption="Requisition Details" SyncPositionWithGraph="true">
                                <Levels>
                                    <px:PXGridLevel DataMember="Lines">
                                        <Columns>
                                            <px:PXGridColumn DataField="LineNbr" TextAlign="Right" Visible="False" />
                                            <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAA" />
                                            <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;A" />
                                            <px:PXGridColumn DataField="Description" />
                                            <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" />
                                            <px:PXGridColumn AllowNull="False" DataField="OrderQty" TextAlign="Right" />
                                            <px:PXGridColumn AllowNull="False" DataField="BiddingQty" TextAlign="Right" />
                                            <px:PXGridColumn AllowNull="False" DataField="CuryEstUnitCost" TextAlign="Right" />
                                            <px:PXGridColumn AllowNull="False" DataField="CuryEstExtCost" TextAlign="Right" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <Layout FormViewHeight="600px" />
                                <Mode AllowAddNew="false" AllowUpdate="false" AllowDelete="false" />
                                <AutoCallBack Target="gridBidding" Command="Refresh">
                                </AutoCallBack>
                                <AutoSize Enabled="true" MinHeight="180"/>
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="gridBidding" runat="server" DataSourceID="ds" Style="z-index: 100; height: 167px;" Width="100%" SkinID="DetailsInTab"
                                Caption="Bidding Details">
                                <CallbackCommands>
                                    <Save CommitChanges="true" CommitChangesIDs="gridBidding" RepaintControls="None" />
                                    <FetchRow RepaintControls="None" />
                                </CallbackCommands>
                                <Levels>
                                    <px:PXGridLevel DataMember="Bidding">
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                            <px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr" />
                                            <px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" />
                                            <px:PXSegmentMask ID="edVendorLocationID" runat="server" DataField="VendorLocationID" />
                                            <px:PXTextEdit ID="edQuoteNumber" runat="server" DataField="QuoteNumber" />
                                            <px:PXNumberEdit ID="edQuoteQty" runat="server" DataField="QuoteQty" />
                                            <px:PXNumberEdit ID="edCuryQuoteUnitCost" runat="server" DataField="CuryQuoteUnitCost" />
                                            <px:PXNumberEdit ID="edOrderQty" runat="server" DataField="OrderQty" /></RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AutoCallBack="true" />
                                            <px:PXGridColumn AllowShowHide="False" DataField="LineNbr" TextAlign="Right" Visible="False" />
                                            <px:PXGridColumn DataField="VendorID" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="true" />
                                            <px:PXGridColumn DataField="VendorID_Vendor_AcctName" />
                                            <px:PXGridColumn DataField="VendorLocationID" DisplayFormat="&gt;AAAAAA" AutoCallBack="true" />
                                            <px:PXGridColumn DataField="QuoteNumber" />
                                            <px:PXGridColumn AllowNull="False" DataField="MinQty" TextAlign="Right" />
                                            <px:PXGridColumn AllowNull="False" DataField="QuoteQty" TextAlign="Right" />
                                            <px:PXGridColumn DataField="CuryID" />
                                            <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryQuoteUnitCost" TextAlign="Right" />
                                            <px:PXGridColumn AllowNull="False" DataField="CuryQuoteExtCost" TextAlign="Right" />
                                            <px:PXGridColumn AllowNull="False" DataField="OrderQty" TextAlign="Right" />
                                            <px:PXGridColumn DataField="Location__VShipTermsID" />
                                            <px:PXGridColumn DataField="RQBiddingVendor__FOBPoint" DisplayFormat="&gt;aaaaaaaaaaaaaaa" />
                                            <px:PXGridColumn DataField="Location__VLeadTime" TextAlign="Right" />
                                            <px:PXGridColumn DataField="RQBiddingVendor__ShipVia" DisplayFormat="&gt;aaaaaaaaaaaaaaa" />
                                            <px:PXGridColumn DataField="RQBiddingVendor__ExpireDate" />
                                            <px:PXGridColumn DataField="RQBiddingVendor__PromisedDate" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <Mode AllowAddNew="true" AllowUpdate="true" AllowDelete="true" />
                                <Parameters>
                                    <px:PXControlParam ControlID="form" Name="reqNbr" PropertyName="NewDataKey[&quot;ReqNbr&quot;]" Type="string" />
                                    <px:PXControlParam ControlID="grid" Name="lineNbr" PropertyName="DataValues[&quot;LineNbr&quot;]" Type="Int32" />
                                    <px:PXSyncGridParam ControlID="grid" />
                                </Parameters>
                                <AutoSize Enabled="True" MinHeight="180"/>
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Enabled="true" Container="Window" MinHeight="350" />
	</px:PXTab>
</asp:Content>
