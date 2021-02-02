<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM304500.aspx.cs" Inherits="Page_PM304500"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource EnableAttributes="True" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.PMQuoteMaint" PrimaryView="Quote" PageLoadBehavior="InsertRecord">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Delete" PopupVisible="true" ClosePopup="True" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" PopupVisible="True" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="ActionsFolder" StartNewGroup="true" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="PrimaryQuote" CommitChanges="True" PopupVisible="True" />
               <px:PXDSCallbackCommand Name="PasteLine" Visible="False" CommitChanges="true" DependOnGrid="ProductsGrid" />
               <px:PXDSCallbackCommand Name="ResetOrder" Visible="False" CommitChanges="true" DependOnGrid="ProductsGrid" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ViewActivity" DependOnGrid="gridActivities" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="OpenActivityOwner" Visible="False" CommitChanges="True" DependOnGrid="gridActivities" />
			<px:PXDSCallbackCommand Name="CurrencyView" Visible="False" />
			<px:PXDSCallbackCommand Name="Approve" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="Reject" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewShippingOnMap" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ValidateAddresses" Visible="False" CommitChanges="True" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Quote Summary" DataMember="Quote" FilesIndicator="True"
		NoteIndicator="True" LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edQuoteNbr" MarkRequired="Dynamic">
		<CallbackCommands>
			<Save PostData="Self" />
		</CallbackCommands>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
			<px:PXSelector ID="edQuoteNbr" runat="server" DataField="QuoteNbr" FilterByAllFields="True" AutoRefresh="True" />
			<px:PXSelector ID="edOpportunityID" runat="server" DataField="OpportunityID" CommitChanges="True" Enabled="true" AutoRefresh="true" AllowEdit="true" FilterByAllFields="True"/>
			<px:PXCheckBox ID="edIsPrimary" runat="server" DataField="IsPrimary" CommitChanges="true"/>
			<px:PXDropDown CommitChanges="True" ID="edStatus" runat="server" AllowNull="False" Enabled="false" DataField="Status" />
			<px:PXDateTimeEdit ID="edDocumentDate" runat="server" DataField="DocumentDate" />
			<px:PXDateTimeEdit ID="edExpirationDate" runat="server" DataField="ExpirationDate" />
			<px:PXTextEdit ID="edExternalRef" runat="server" DataField="ExternalRef" />
                                  	
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edSubject" runat="server" AllowNull="False" DataField="Subject" CommitChanges="True" Width="596" />
			
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector ID="edTemplateID" runat="server" DataField="TemplateID" CommitChanges="True" AllowEdit="true"/>
			<px:PXSelector ID="edProjectManager" runat="server" DataField="ProjectManager" />
			<px:PXSelector CommitChanges="True" ID="edBAccountID" runat="server" DataField="BAccountID" AllowEdit="True" FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
			<px:PXSelector CommitChanges="True" ID="edContactID" runat="server" DataField="ContactID" TextField="displayName" AllowEdit="True" AutoRefresh="True" TextMode="Search" DisplayMode="Text" FilterByAllFields="True" />
			<px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" TextMode="Search" DisplayMode="Text" />
		    
		    <pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" RateTypeView="currencyinfo" DataMember="_Currency_" DataSourceID="ds"></pxa:PXCurrencyRate>
            <px:PXLayoutRule runat="server"  Merge="true" />
			<px:PXSelector ID="edProjectID" runat="server" DataField="QuoteProjectID" Enabled="False" AllowEdit="True"/>
			<px:PXSegmentMask ID="edProjectCD" runat="server" DataField="QuoteProjectCD" CommitChanges="True" />			
			<px:PXLayoutRule runat="server"  Merge="false" />

			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S" />
		   
			<px:PXNumberEdit CommitChanges="True" ID="edCuryAmount" runat="server" DataField="CuryAmount" Enabled="False"/>
			<px:PXNumberEdit CommitChanges="True" ID="edCuryCostTotal" runat="server" DataField="CuryCostTotal" Enabled="False"/>
			<px:PXNumberEdit CommitChanges="True" ID="edCuryGrossMarginAmount" runat="server" DataField="CuryGrossMarginAmount" Enabled="False"/>
			<px:PXNumberEdit CommitChanges="True" ID="edGrossMarginPct" runat="server" DataField="GrossMarginPct" Enabled="False"/>
			<px:PXNumberEdit CommitChanges="True" ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False"/>
			<px:PXNumberEdit CommitChanges="True" ID="edCuryQuoteTotal" runat="server" DataField="CuryQuoteTotal" Enabled="False"/>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="280px" DataSourceID="ds" DataMember="QuoteCurrent">
		<Items>
			<px:PXTabItem Text="Estimation">
				<Template>
				    <px:PXGrid ID="ProductsGrid" SkinID="Details" runat="server" Width="100%" Height="500px" DataSourceID="ds" ActionsPosition="Top" BorderWidth="0px" SyncPosition="true">
					   <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
					   <Levels>
						  <px:PXGridLevel DataMember="Products">
							 <RowTemplate>
								<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
								<px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
								<px:PXSelector ID="edTaskCD2" runat="server" DataField="TaskCD" />
							 </RowTemplate>
							 <Columns>
								<px:PXGridColumn DataField="InventoryID" DisplayFormat="CCCCCCCCCCCCCCCCCCCC" AutoCallBack="True" AllowDragDrop="true"/>
								<px:PXGridColumn DataField="Descr" AllowDragDrop="true"/>
								<px:PXGridColumn DataField="Quantity" TextAlign="Right" AutoCallBack="True" AllowDragDrop="true"/>
								<px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" AutoCallBack="True" AllowDragDrop="true"/>
								<px:PXGridColumn AllowNull="False" DataField="CuryUnitCost" TextAlign="Right" AutoCallBack="True" />
								<px:PXGridColumn AllowNull="False" DataField="CuryExtCost" TextAlign="Right" AutoCallBack="True" />
								<px:PXGridColumn AllowNull="False" DataField="CuryUnitPrice" TextAlign="Right" AutoCallBack="True" />
								<px:PXGridColumn DataField="ManualPrice" TextAlign="Center" AllowNull="False" CommitChanges="True" Type="CheckBox" />
								<px:PXGridColumn AllowNull="False" DataField="CuryExtPrice" TextAlign="Right" AutoCallBack="True" />
								<px:PXGridColumn AllowNull="False" DataField="DiscPct" TextAlign="Right" AutoCallBack="True" />
								<px:PXGridColumn DataField="CuryDiscAmt" TextAlign="Right" AllowNull="False" AutoCallBack="True" />
								<px:PXGridColumn DataField="CuryAmount" TextAlign="Right" AllowNull="False" />
								<px:PXGridColumn DataField="DiscountID" TextAlign="Left" />
								<px:PXGridColumn DataField="DiscountSequenceID" TextAlign="Left" />	

								<px:PXGridColumn DataField="ManualDisc" TextAlign="Center" AllowNull="False" CommitChanges="True" Type="CheckBox" />
								<px:PXGridColumn DataField="TaskCD" AutoCallBack="True" />
								<px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" AllowDragDrop="true"/>
								<px:PXGridColumn DataField="ExpenseAccountGroupID" AutoCallBack="True" AllowDragDrop="true"/>
								<px:PXGridColumn DataField="RevenueAccountGroupID" AutoCallBack="True" />
								<px:PXGridColumn DataField="TaxCategoryID" DisplayFormat="&gt;aaaaaaaaaa" />
								<px:PXGridColumn DataField="EmployeeID" AutoCallBack="True" />
							 </Columns>
						  </px:PXGridLevel>
					   </Levels>
					   <ActionBar>
						  <CustomItems>
							 <px:PXToolBarButton Text="Insert Row" SyncText="false" ImageSet="main" ImageKey="AddNew">
								<AutoCallBack Target="ProductsGrid" Command="AddNew" Argument="1"></AutoCallBack>
								<ActionBar ToolBarVisible="External" MenuVisible="true" />
							 </px:PXToolBarButton>
							 <px:PXToolBarButton Text="Cut Row" SyncText="false" ImageSet="main" ImageKey="Copy">
								<AutoCallBack Target="ProductsGrid" Command="Copy"></AutoCallBack>
								<ActionBar ToolBarVisible="External" MenuVisible="true" />
							 </px:PXToolBarButton>
							 <px:PXToolBarButton Text="Insert Cut Row" SyncText="false" ImageSet="main" ImageKey="Paste">
								<AutoCallBack Target="ProductsGrid" Command="Paste"></AutoCallBack>
								<ActionBar ToolBarVisible="External" MenuVisible="true" />
							 </px:PXToolBarButton>
						  </CustomItems>
					   </ActionBar>
					   <CallbackCommands PasteCommand="PasteLine">
						  <Save PostData="Container" />
					   </CallbackCommands>
					   <Mode AllowUpload="True" AllowDragRows="true" InitNewRow="true" />
				    </px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Project Tasks">
				<Template>
					<px:PXGrid ID="TasksGrid" SkinID="Details" runat="server" Width="100%" Height="500px" DataSourceID="ds" ActionsPosition="Top" BorderWidth="0px" SyncPosition="true">
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Mode AllowUpload="True" />
						<Levels>
							<px:PXGridLevel DataMember="Tasks">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                                            <px:PXSegmentMask CommitChanges="True" ID="edTaskCD" runat="server" DataField="TaskCD"/>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="TaskCD" AutoCallBack="True" />
									<px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="Type" />
									<px:PXGridColumn DataField="PlannedStartDate" />
                                             <px:PXGridColumn DataField="PlannedEndDate" />                                            
                                            <px:PXGridColumn DataField="TaxCategoryID" />
								    <px:PXGridColumn DataField="isDefault" Type="CheckBox" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Billing info">
				<Template>
				    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
					<px:PXFormView ID="PXFormView1" runat="server" DataMember="QuoteCurrent" DataSourceID="ds" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
							<px:PXLayoutRule runat="server" GroupCaption="Financial Details" StartGroup="True" />
                            <px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
							<px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AllowEdit="True" AutoRefresh="True" FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" CommitChanges="True" />
					         <px:PXSelector ID="edTaxZoneID" runat="server" DataField="TaxZoneID" CommitChanges="true" />
							<px:PXSelector ID="edTermsID" runat="server" DataField="TermsID" CommitChanges="true" />
							<px:PXTextEdit ID="edCreatedByID_Creator_Username" runat="server" DataField="CreatedByID_Creator_Username" />
							<px:PXCheckBox ID="edAllowOverrideContactAddress" runat="server" DataField="AllowOverrideContactAddress" CommitChanges="true" />
						</Template>
					</px:PXFormView>

					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
					<px:PXFormView ID="edQuote_Contact" runat="server" DataMember="Quote_Contact" DataSourceID="ds" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" GroupCaption="Contact Information" StartGroup="True" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" CommitChanges="true" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXLabel ID="LFirstName" runat="server" Size="SM" />
							<px:PXDropDown ID="edTitle" runat="server" DataField="Title" Size="XS" SuppressLabel="True" CommitChanges="True" />
							<px:PXTextEdit ID="edFirstName" runat="server" DataField="FirstName" Width="164px" LabelID="LFirstName" CommitChanges="true" />
							<px:PXLayoutRule runat="server" />
							<px:PXTextEdit ID="edLastName" runat="server" DataField="LastName" SuppressLabel="False" CommitChanges="true" />
							<px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" SuppressLabel="False" CommitChanges="true" />
							<px:PXMailEdit ID="edEMail" runat="server" CommandSourceID="ds" DataField="EMail" CommitChanges="True" />
							<px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True" />

							<px:PXLayoutRule ID="PXLayoutRule8" runat="server" Merge="True" />
							<px:PXDropDown ID="edPhone1Type" runat="server" DataField="Phone1Type" Width="104px" SuppressLabel="True" CommitChanges="True" />
							<px:PXLabel ID="lblPhone1" runat="server" SuppressLabel="true" />
							<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" SuppressLabel="True" LabelWidth="0px" Size="XM" />

							<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
							<px:PXDropDown ID="edPhone2Type" runat="server" DataField="Phone2Type" Width="104px" SuppressLabel="True" CommitChanges="True" />
							<px:PXLabel ID="lblPhone2" runat="server" SuppressLabel="true" />
							<px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" SuppressLabel="True" LabelWidth="0px" Size="XM" />

							<px:PXLayoutRule ID="PXLayoutRule7" runat="server" Merge="True" />
							<px:PXDropDown ID="edPhone3Type" runat="server" DataField="Phone3Type" Width="104px" SuppressLabel="True" CommitChanges="True" />
							<px:PXLabel ID="LPhone3" runat="server" SuppressLabel="true" />
							<px:PXMaskEdit ID="edPhone3" runat="server" DataField="Phone3" SuppressLabel="True" LabelWidth="0px" Size="XM" LabelID="LPhone3" />


							<px:PXLayoutRule ID="PXLayoutRule9" runat="server" Merge="True" />
							<px:PXDropDown ID="edFaxType" runat="server" DataField="FaxType" Width="104px" SuppressLabel="True" CommitChanges="True" />
							<px:PXLabel ID="LFax" runat="server" SuppressLabel="true" />
							<px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" SuppressLabel="True" LabelWidth="0px" Size="XM" LabelID="LFax" CommitChanges="true" />
							<px:PXLayoutRule runat="server" />
						</Template>
						<ContentStyle BackColor="Transparent" />
					</px:PXFormView>

					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
					<px:PXFormView ID="edQuote_Address" runat="server" DataMember="Quote_Address" DataSourceID="ds" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" GroupCaption="Address" StartGroup="True" />
							<px:PXCheckBox ID="chkIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" CommitChanges="true" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" CommitChanges="true" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" CommitChanges="true" />
							<px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" CommitChanges="true"
								FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
							<px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" Size="S" CommitChanges="True" />
							<px:PXSelector ID="edCountryID" runat="server" AllowEdit="True" DataField="CountryID"
								FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" CommitChanges="True" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXButton ID="btnViewMainOnMap" runat="server" CommandName="ViewMainOnMap" CommandSourceID="ds"
								Size="xs" Text="View On Map" />
							<px:PXLayoutRule runat="server" />
						</Template>
					</px:PXFormView>

					
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Shipping Info">
				<Template>
                    <px:PXCheckBox ID="edAllowOverrideShippingContactAddress" runat="server" Size="SM" DataField="AllowOverrideShippingContactAddress" CommitChanges="true" AlignLeft="True" />
                    <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" />
                    <px:PXFormView ID="PXFormView1" runat="server" RenderStyle="Simple">
                        <Template>
                        </Template>
                        <ContentStyle BackColor="Transparent"/>
                    </px:PXFormView>   
                    <px:PXLayoutRule runat="server" EndGroup="True" />
                    <px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartRow="True" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
                    <px:PXFormView ID="formD" runat="server" DataMember="Shipping_Contact" DataSourceID="ds" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Ship-To Contact" />
                            <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" CommitChanges="True" />
                            <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" CommitChanges="True" />
                            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" ControlSize="SM" />
                            <px:PXDropDown ID="edPhone1Type" runat="server" DataField="Phone1Type" SuppressLabel="True" CommitChanges="True" Width="104px"/>
                            <px:PXLabel ID="LPhone1" runat="server" SuppressLabel="true" />
                            <px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" LabelWidth="0px" Size="XM" LabelID="LPhone1" SuppressLabel="True" CommitChanges="true"/>
                            <px:PXLayoutRule runat="server" />
                            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" ControlSize="SM" SuppressLabel="True" />
                            <px:PXDropDown ID="edPhone2Type" runat="server" DataField="Phone2Type" SuppressLabel="True" CommitChanges="True" Width="104px"/>
                            <px:PXLabel ID="LPhone2" runat="server" SuppressLabel="true" />
                            <px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" LabelWidth="0px" Size="XM" LabelID="LPhone2" SuppressLabel="True" CommitChanges="true"/>
                            <px:PXLayoutRule runat="server" />
                            <px:PXMailEdit ID="edEmail" runat="server" DataField="Email" CommitChanges="True" />
                            <px:PXLayoutRule runat="server" EndGroup="True" />
                        </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM"  />
                    <px:PXFormView ID="formB" DataMember="Shipping_Address" runat="server" DataSourceID="ds"  RenderStyle="Simple" SyncPosition="True">
                        <Template>
                            <px:PXLayoutRule runat="server" StartGroup="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Ship-To Address" />
                            <px:PXCheckBox ID="chkIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" CommitChanges="true" />
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" CommitChanges="true" />
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" CommitChanges="true" />
                            <px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" DataSourceID="ds" CommitChanges="True">
                                <CallBackMode PostData="Container" />
                                <Parameters>
                                    <px:PXControlParam ControlID="formB" Name="CRShippingAddress.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value"
                                        Type="String" />
                                </Parameters>
                            </px:PXSelector>
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXMaskEdit CommitChanges="True" ID="edPostalCode" runat="server" DataField="PostalCode" Size="s" />
                            <px:PXButton Size="xs" ID="btnViewMainOnMap" runat="server" CommandName="ViewShippingOnMap" CommandSourceID="ds" Text="View on Map" />
                            <px:PXLayoutRule runat="server" />
                            <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" DataSourceID="ds" CommitChanges="true" />
                            <px:PXLayoutRule runat="server" EndGroup="True" />
                        </Template>
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server"  />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Activities" LoadOnDemand="true">
				<Template>
					<pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%" AllowSearch="True" DataMember="Activities" AllowPaging="true" NoteField="NoteText"
						FilesField="NoteFiles" BorderWidth="0px" GridSkinID="Inquire" SplitterStyle="z-index: 100; border-top: solid 1px Gray;  border-bottom: solid 1px Gray" PreviewPanelStyle="z-index: 100; background-color: Window"
						PreviewPanelSkinID="Preview" BlankFilterHeader="All Activities" MatrixMode="true" PrimaryViewControlID="form">
						<ActionBar DefaultAction="cmdViewActivity" PagerVisible="False">
							<Actions>
								<AddNew Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Key="cmdAddTask">
									<AutoCallBack Command="NewTask" Target="ds"></AutoCallBack>
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdAddEvent">
									<AutoCallBack Command="NewEvent" Target="ds"></AutoCallBack>
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdAddEmail">
									<AutoCallBack Command="NewMailActivity" Target="ds"></AutoCallBack>
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdAddActivity">
									<AutoCallBack Command="NewActivity" Target="ds"></AutoCallBack>
									<ActionBar />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Activities">
								<RowTemplate>
									<px:PXTimeSpan TimeMode="True" ID="edTimeSpent" runat="server" DataField="TimeSpent" InputMask="hh:mm" MaxHours="99" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" ForceExport="True" />
									<px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
									<px:PXGridColumn DataField="CRReminder__ReminderIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
									<px:PXGridColumn DataField="ClassIcon" Width="31px" AllowShowHide="False" ForceExport="True" />
									<px:PXGridColumn DataField="ClassInfo" />
									<px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" />
									<px:PXGridColumn DataField="UIStatus" />
									<px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="StartDate" DisplayFormat="g" />
									<px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Visible="False" />
									<px:PXGridColumn DataField="TimeSpent" RenderEditorText="True" />
									<px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username" Visible="false" SyncVisible="False" SyncVisibility="False" />
									<px:PXGridColumn DataField="WorkgroupID" />
									<px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" DisplayMode="Text" />
									<px:PXGridColumn DataField="ProjectID" AllowShowHide="true" Visible="false" SyncVisible="false" />
									<px:PXGridColumn DataField="ProjectTaskID" AllowShowHide="true" Visible="false" SyncVisible="false" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<GridMode AllowAddNew="False" AllowUpdate="False" />
						<PreviewPanelTemplate>
							<px:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine" MaxLength="50" Width="100%" Height="100px" SkinID="Label">
								<AutoSize Container="Parent" Enabled="true" />
							</px:PXHtmlView>
						</PreviewPanelTemplate>
						<AutoSize Enabled="true" />
						<GridMode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpdate="False" AllowUpload="False" />
					</pxa:PXGridWithPreview>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Tax Details" LoadOnDemand="true">
				<Template>
					<px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" SkinID="Details" ActionsPosition="Top" BorderWidth="0px">
						<AutoSize Enabled="True" MinHeight="150" />
						<Levels>
							<px:PXGridLevel DataMember="Taxes">
								<Columns>
									<px:PXGridColumn DataField="TaxID" AllowUpdate="False" />
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TaxRate" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" DataField="CuryTaxableAmt" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" DataField="CuryTaxAmt" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" DataField="Tax__TaxType" Label="Tax Type" RenderEditorText="True" />
									<px:PXGridColumn AllowNull="False" DataField="Tax__PendingTax" Label="Pending VAT" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn AllowNull="False" DataField="Tax__ReverseTax" Label="Reverse VAT" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn AllowNull="False" DataField="Tax__ExemptTax" Label="Exempt From VAT" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn AllowNull="False" DataField="Tax__StatisticalTax" Label="Statistical VAT" TextAlign="Center" Type="CheckBox" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		    <px:PXTabItem Text="Attributes">
			   <Template>
				  <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" Width="100%" Height="100%" SkinID="DetailsInTab" MatrixMode="True">
					 <Levels>
						<px:PXGridLevel DataMember="Answers">
						    <Columns>
							   <px:PXGridColumn DataField="AttributeID" TextAlign="Left" AllowShowHide="False" TextField="AttributeID_description" />
							   <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" />
							   <px:PXGridColumn DataField="Value" RenderEditorText="True" />
						    </Columns>
						</px:PXGridLevel>
					 </Levels>
					 <AutoSize Enabled="true" />
					 <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
				  </px:PXGrid>
			   </Template>
		    </px:PXTabItem>
			<px:PXTabItem Text="Approval Details">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True">
                        <AutoSize Enabled="true" />
                        <Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />
                        <Levels>
                            <px:PXGridLevel DataMember="Approval">
                                <Columns>
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctCD" />
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctName" />
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctCD" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctName" />
                                    <px:PXGridColumn DataField="ApproveDate" />
                                    <px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Reason" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="AssignmentMapID"  Visible="false" SyncVisible="false"/>
                                    <px:PXGridColumn DataField="RuleID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="StepID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="CreatedDateTime" Visible="false" SyncVisible="false" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
		</Items>
	    <AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>

<asp:Content ID="Dialogs" ContentPlaceHolderID="phDialogs" runat="Server">
	<px:PXSmartPanel ID="PanelCopyQuote" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="Copy Quote"
		CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="CopyQuoteInfo" AutoCallBack-Enabled="true" AutoCallBack-Target="formCopyQuote" AutoCallBack-Command="Refresh"
		CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel">
		<px:PXFormView ID="formCopyQuote" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Services Settings" CaptionVisible="False" SkinID="Transparent"
			DataMember="CopyQuoteInfo">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
				<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" CommitChanges="True" />
				<px:PXCheckBox ID="edRecalculatePrices" runat="server" DataField="RecalculatePrices" CommitChanges="True" />
				<px:PXCheckBox ID="edOverrideManualPrices" runat="server" DataField="OverrideManualPrices" CommitChanges="True" />
				<px:PXCheckBox ID="edRecalculateDiscounts" runat="server" DataField="RecalculateDiscounts" CommitChanges="True" />
				<px:PXCheckBox ID="edOverrideManualDiscounts" runat="server" DataField="OverrideManualDiscounts" CommitChanges="True" />
			</Template>
		</px:PXFormView>
		<div style="padding: 5px; text-align: right;">
			<px:PXButton ID="PXButtonOK" runat="server" Text="OK" DialogResult="Yes" Width="63px" Height="20px"></px:PXButton>
			<px:PXButton ID="PXButtonCancel" runat="server" DialogResult="No" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
		</div>
	</px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelConvertQuote" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="Convert to Project"
		CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="ConvertQuoteInfo" AutoCallBack-Enabled="true" AutoCallBack-Target="formConvertQuote" AutoCallBack-Command="Refresh"
		CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel">
		<px:PXFormView ID="formConvertQuote" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Conversion Settings" CaptionVisible="False" SkinID="Transparent"
			DataMember="ConvertQuoteInfo">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="SS" ControlSize="XM" />
				<px:PXCheckBox ID="edCreateLaborRates" runat="server" DataField="CreateLaborRates" CommitChanges="True" AlignLeft="true"  />
                   <px:PXCheckBox ID="edActivateProject" runat="server" DataField="ActivateProject" CommitChanges="True" AlignLeft="true"  />
                   <px:PXCheckBox ID="edActivateTasks" runat="server" DataField="ActivateTasks" CommitChanges="True" AlignLeft="true"   />
                   <px:PXCheckBox ID="edCopyNotes" runat="server" DataField="CopyNotes" CommitChanges="True" AlignLeft="true"  />
				<px:PXCheckBox ID="edCopyFiles" runat="server" DataField="CopyFiles" CommitChanges="True" AlignLeft="true"  />
                   <px:PXLayoutRule ID="PXLayoutRule10" runat="server" LabelsWidth="S" ControlSize="S" Merge="true" />
				<px:PXCheckBox ID="edMoveActivities" runat="server" DataField="MoveActivities" CommitChanges="True" AlignLeft="true"  />
                   <px:PXSelector ID="edTaskCD" runat="server" DataField="TaskCD" CommitChanges="true" />							
			</Template>
		</px:PXFormView>
		<div style="padding: 5px; text-align: right;">
			<px:PXButton ID="PXButton3" runat="server" Text="OK" DialogResult="Yes" Width="63px" Height="20px"></px:PXButton>
			<px:PXButton ID="PXButton4" runat="server" DialogResult="No" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
		</div>
	</px:PXSmartPanel>

	<px:PXSmartPanel ID="PanelRecalculate" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="Recalculate Prices"
		CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="recalcdiscountsfilter" AutoCallBack-Enabled="true" AutoCallBack-Target="formRecalculate" AutoCallBack-Command="Refresh"
		CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel">
		<px:PXFormView ID="formRecalculate" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Services Settings" CaptionVisible="False" SkinID="Transparent"
			DataMember="recalcdiscountsfilter">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
				<px:PXDropDown ID="edRecalcTerget" runat="server" DataField="RecalcTarget" CommitChanges="true" />
				<px:PXCheckBox ID="edRecalculatePrices" runat="server" DataField="RecalcUnitPrices" CommitChanges="True" />
				<px:PXCheckBox ID="edOverrideManualPrices" runat="server" DataField="OverrideManualPrices" CommitChanges="True" />
				<px:PXCheckBox ID="edRecalculateDiscounts" runat="server" DataField="RecalcDiscounts" CommitChanges="True" />
				<px:PXCheckBox ID="edOverrideManualDiscounts" runat="server" DataField="OverrideManualDiscounts" CommitChanges="True" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<div style="padding: 5px; text-align: right;">
				<px:PXButton ID="PXButton1" runat="server" Text="OK" DialogResult="OK" Width="63px" Height="20px"></px:PXButton>
				<px:PXButton ID="PXButton2" runat="server" DialogResult="No" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
			</div>
		</px:PXPanel>
	</px:PXSmartPanel>
	<!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
</asp:Content>
