<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CR304000.aspx.cs" Inherits="Page_CR304000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.OpportunityMaint" PrimaryView="Opportunity">
        <CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true"/>
			<px:PXDSCallbackCommand Name="Delete" PopupVisible="true" ClosePopup="True" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="True"/>
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="CreateInvoice" CommitChanges="True" Visible="True" />
			<px:PXDSCallbackCommand Name="CreateContact" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="CreateBAccount" CommitChanges="True" Visible="False" />
			<px:PXDSCallbackCommand Name="CreateBothContactAndAccount" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="SubmitQuote" CommitChanges="True" Visible="True" />
            <px:PXDSCallbackCommand Name="EditQuote" CommitChanges="True" Visible="True" />
            <px:PXDSCallbackCommand Name="CreateQuote" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds"/>
            <px:PXDSCallbackCommand Name="PrintQuote" CommitChanges="True" Visible="False"/>
            <px:PXDSCallbackCommand Name="SendQuote" CommitChanges="True" Visible="False"/>
            <px:PXDSCallbackCommand Name="CopyQuote" CommitChanges="True" Visible="False"/>
            <px:PXDSCallbackCommand Name="RecalculatePrices" CommitChanges="True" Visible="True"/>
            <px:PXDSCallbackCommand Name="CreateServiceOrder" CommitChanges="True" Visible="True"/>
            <px:PXDSCallbackCommand Name="OpenAppointmentBoard" CommitChanges="True" Visible="false"/>
	        <px:PXDSCallbackCommand Name="ViewQuote" Visible="false" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds"/>
            <px:PXDSCallbackCommand Name="ViewProject" Visible="false" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds"/>
            <px:PXDSCallbackCommand Name="PrimaryQuote" Visible="false" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
            <px:PXDSCallbackCommand Name="CreateSalesOrder" Visible="True" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ValidateAddresses" Visible="True" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewActivity" DependOnGrid="gridActivities" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="OpenActivityOwner" Visible="False" CommitChanges="True" DependOnGrid="gridActivities" />
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
            <px:PXDSCallbackCommand Name="Relations_TargetDetails" Visible="False" CommitChanges="True"	DependOnGrid="grdRelations" />
			<px:PXDSCallbackCommand Name="Relations_EntityDetails" Visible="False" CommitChanges="True"	DependOnGrid="grdRelations" />
			<px:PXDSCallbackCommand Name="Relations_ContactDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Name="ViewMainOnMap" CommitChanges="true" Visible="false" />
            <px:PXDSCallbackCommand Name="syncSalesforce" Visible="false" />
            <px:PXDSCallbackCommand Name="AddNewContact" Visible="false" />            
            <px:PXDSCallbackCommand Name="ViewShippingOnMap" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewOrder" Visible="True" />
			<px:PXDSCallbackCommand Name="ViewInvoice" Visible="True" />
            <px:PXDSCallbackCommand Name="PasteLine" Visible="False" CommitChanges="true" DependOnGrid="ProductsGrid" />
            <px:PXDSCallbackCommand Name="ResetOrder" Visible="False" CommitChanges="true" DependOnGrid="ProductsGrid" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Opportunity Summary" DataMember="Opportunity" FilesIndicator="True"
        NoteIndicator="True" LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edOpportunityID" TabIndex="100">
        <CallbackCommands>
            <Save PostData="Self" />
        </CallbackCommands>        
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edOpportunityID" runat="server" DataField="OpportunityID" FilterByAllFields="True" AutoRefresh="True" />
            <px:PXDropDown CommitChanges="True" ID="edStatus" runat="server" AllowNull="False" DataField="Status" />
            <px:PXSelector CommitChanges="True" ID="edClassID" runat="server" DataField="ClassID" AllowEdit="True" TextMode="Search" FilterByAllFields="True" edit="1" />
            <px:PXDropDown ID="edStageID" runat="server" AllowNull="False" DataField="StageID" CommitChanges="True" />
            <px:PXDateTimeEdit DataField="CloseDate" ID="edCloseDate" runat="server"  Size="M" CommitChanges="true" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edSubject" runat="server" AllowNull="False" DataField="Subject" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edBAccountID" runat="server" DataField="BAccountID" AllowEdit="True" FilterByAllFields="True" TextMode="Search" />
            <px:PXSelector ID="edLocationID" runat="server" DataField="LocationID"  AllowEdit="True" TextMode="Search" DisplayMode="Hint" FilterByAllFields="True" CommitChanges="True" />
            <px:PXSelector CommitChanges="True" ID="edContactID" runat="server" DataField="ContactID" TextField="displayName" AllowEdit="True" AutoRefresh="True" TextMode="Search" DisplayMode="Text" FilterByAllFields="True" edit="1" />
            <pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" RateTypeView="currencyinfo" DataMember="_Currency_" DataSourceID="ds"></pxa:PXCurrencyRate>
            <px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" TextMode="Search" DisplayMode="Text" FilterByAllFields="True" AutoRefresh="true" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXCheckBox ID="chkManualTotalEntry" runat="server" DataField="ManualTotalEntry" CommitChanges="true"/>
            <px:PXNumberEdit CommitChanges="True" ID="edCuryAmount" runat="server" DataField="CuryAmount" />
            <px:PXNumberEdit CommitChanges="True" ID="edCuryDiscTot" runat="server" DataField="CuryDiscTot" />
            <px:PXNumberEdit CommitChanges="True" ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal"  Enabled="False"/>
            <px:PXNumberEdit ID="edCuryProductsAmount" runat="server" DataField="CuryProductsAmount" Enabled="False"/>        
            <px:PXCheckBox ID="chkServiceManagement" runat="server" DataField="ChkServiceManagement" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="280px" DataSourceID="ds" DataMember="OpportunityCurrent" OnDataBound="tab_DataBound">
        <Items>
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
                                    <px:PXGridColumn DataField="TimeSpent" RenderEditorText="True"/>
                                    <px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username" Visible="false" SyncVisible="False" SyncVisibility="False" />
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" DisplayMode="Text"/>
                                    <px:PXGridColumn DataField="ProjectID" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="ProjectTaskID" AllowShowHide="true" Visible="false" SyncVisible="false"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <GridMode AllowAddNew="False" AllowUpdate="False" />
                        <PreviewPanelTemplate>
                            <px:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine" MaxLength="50" Width="100%" Height="100px" SkinID="Label" >
                                      <AutoSize Container="Parent" Enabled="true" />
                                </px:PXHtmlView>
                        </PreviewPanelTemplate>
                        <AutoSize Enabled="true" />
                        <GridMode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpdate="False" AllowUpload="False" />
                    </pxa:PXGridWithPreview>
                </Template>
            </px:PXTabItem>            
            <px:PXTabItem Text="Document Details" Key="ProductsTab">
                <Template>
                    <px:PXGrid ID="ProductsGrid" SkinID="Details" runat="server" Width="100%" Height="500px" DataSourceID="ds" ActionsPosition="Top" BorderWidth="0px" SyncPosition="true">
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                        <Mode AllowUpload="True" AllowDragRows="true"/>
                        <CallbackCommands PasteCommand="PasteLine">
                            <Save PostData="Container" />
                        </CallbackCommands>
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
                        <Levels>
                            <px:PXGridLevel DataMember="Products">
                                <Mode InitNewRow="true"/>
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="ProductsGrid" Name="CROpportunityProducts.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>                                                                    
									<px:PXSelector CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" AutoRefresh="True"/>
                                    <px:PXSelector CommitChanges="True" ID="edUOM" runat="server" DataField="UOM" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="ProductsGrid" Name="CROpportunityProducts.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" AllowEdit="true" CommitChanges="true">
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edVendorLocationID" runat="server" DataField="VendorLocationID" AllowEdit="true" AutoRefresh="true">
                                    </px:PXSegmentMask>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="CCCCCCCCCCCCCCCCCCCC" AutoCallBack="True" AllowDragDrop="true" />
                                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-&gt;AA" />
                                    <px:PXGridColumn DataField="Descr" />
                                    <px:PXGridColumn DataField="IsFree" AllowNull="False" TextAlign="Center" Type="CheckBox" AutoCallBack="True"/>
                                    <px:PXGridColumn DataField="BillingRule" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SiteID" DisplayFormat="&gt;AAAAAAAAAA" />
                                    <px:PXGridColumn DataField="Quantity" TextAlign="Right" AutoCallBack="True" AllowDragDrop="true" />
                                    <px:PXGridColumn DataField="EstimatedDuration" AutoCallBack="True"/>
                                    <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" AutoCallBack="True" AllowDragDrop="true" />
                                    <px:PXGridColumn DataField="CuryUnitPrice" AllowNull="False" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="CuryExtPrice" AllowNull="False" TextAlign="Right" AutoCallBack="True"  />
                                    <px:PXGridColumn DataField="DiscPct" AllowNull="False" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="CuryDiscAmt" TextAlign="Right" AllowNull="False" AutoCallBack="True" />                                    
                                    <px:PXGridColumn DataField="CuryAmount" TextAlign="Right" AllowNull="False" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="ManualDisc" TextAlign="Center" AllowNull="False" AutoCallBack="True" Type="CheckBox" />                                    
                                    <px:PXGridColumn DataField="DiscountID" TextAlign="Left" AllowShowHide="Server" AutoCallBack="True" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="DiscountSequenceID" TextAlign="Left" />
                                    <px:PXGridColumn DataField="TaxCategoryID" DisplayFormat="&gt;aaaaaaaaaa" />
                                    <px:PXGridColumn DataField="ManualPrice" TextAlign="Center" Type="CheckBox"/> 
                                    <px:PXGridColumn DataField="TaskID" DisplayFormat="&gt;#####" RenderEditorText="true" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="POCreate" TextAlign="Right" AutoCallBack="True" Type="CheckBox" />
                                    <px:PXGridColumn DataField="CuryUnitCost" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="VendorID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="VendorLocationID" AutoCallBack="True"/>
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" />
                                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Details">
                <Template>
                    <px:PXPanel ID="PXPanel1" runat="server" SkinID="transparent">
                        <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" GroupCaption="CRM" />
                        <px:PXDropDown ID="edResolution" runat="server" AllowNull="False" DataField="Resolution" CommitChanges="True" />
                        <px:PXSelector DataField="WorkgroupID" ID="WorkgroupID" runat="server"  CommitChanges="True" TextMode="Search" DisplayMode="Text" FilterByAllFields="True" />
                        <px:PXSegmentMask DataField="ParentBAccountID" ID="edParentBAccountID" runat="server" AllowEdit="True" FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
                        <px:PXTextEdit ID="edLastIncomingActivity" runat="server" DataField="ActivityStatistics.LastIncomingActivityDate" CommitChanges="true" Enabled="False" AllowEdit="True" />
                        <px:PXTextEdit ID="edLastOutgoingActivity" runat="server" DataField="ActivityStatistics.LastOutgoingActivityDate" CommitChanges="true" Enabled="False" />
                        <px:PXTextEdit ID="edClosingDate" runat="server" DataField="ClosingDate" CommitChanges="true" Enabled="False" />
                        <px:PXCheckBox ID="edIsActive" runat="server" DataField="IsActive" />
                        <px:PXLayoutRule runat="server" EndGroup="True" />

                        <px:PXLayoutRule ID="PXLayoutRule2" runat="server" LabelsWidth="SM" ControlSize="SM" GroupCaption="Forecasting" />
                        <px:PXFormView ID="panelfordatamember" runat="server" DataMember="ProbabilityCurrent" DataSourceID="ds" RenderStyle="Simple">
                            <Template>
                                <px:PXLayoutRule ID="PXLayoutRule14" runat="server" SuppressLabel="False" LabelsWidth="SM" />
                                <px:PXNumberEdit ID="edProbability" runat="server" DataField="Probability" Enabled="False" Size="M" />
                            </Template>
                            <ContentStyle BackColor="Transparent" />
                        </px:PXFormView>
                        <px:PXNumberEdit ID="edCuryWgtAmount" runat="server" DataField="CuryWgtAmount" Enabled="False" Size="M"/>
                        <px:PXLayoutRule runat="server" EndGroup="True" />  

                        <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" GroupCaption="Source" />
                        <px:PXDropDown ID="edSource" runat="server" DataField="Source" CommitChanges="True" />
                        <px:PXSelector ID="edCampaignSourceID" runat="server" DataField="CampaignSourceID" AllowEdit="True" TextMode="Search" DisplayMode="Hint" FilterByAllFields="True" CommitChanges="true" />
                        <px:PXTextEdit ID="edExternalRef" runat="server" DataField="ExternalRef"  CommitChanges="true" />
                        <px:PXSelector ID="edLeadID" runat="server" DataField="LeadID" TextField="displayName" DisplayMode="Text" Enabled="False" AllowEdit="True" />
                        <px:PXLayoutRule runat="server" EndGroup="True"> 
                        </px:PXLayoutRule> 

                        <px:PXLayoutRule ID="PXLayoutRule8" runat="server"  LabelsWidth="SM" ControlSize="M" GroupCaption="Additional Details" />
                        <px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="True" />
                        <px:PXSegmentMask ID="edProjectID" runat="server" DataField="ProjectID" CommitChanges="True" AutoRefresh="True" AllowAddNew="True" />
                        <px:PXSelector ID="edTaxZoneID" runat="server" DataField="TaxZoneID" CommitChanges="true"/>
                        <px:PXDropDown ID="edTaxCalcMode" runat="server" DataField="TaxCalcMode" CommitChanges="true"/>
                        <px:PXLayoutRule runat="server" EndGroup="True" />  
                    </px:PXPanel>
                    <px:PXRichTextEdit ID="edDescription" runat="server" DataField="Details" Style="border-top: 1px solid #BBBBBB; border-left: 0px; border-right: 0px; margin: 0px;
                                                                                                      padding: 0px; width: 100%;" AllowAttached="true" AllowSearch="true" AllowMacros="true"  AllowLoadTemplate="false" AllowSourceMode="true">
                        <LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M"/>
                        <AutoSize Enabled="True" MinHeight="216" />
                    </px:PXRichTextEdit>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Quotes" BindingContext="form" RepaintOnDemand="false">
            <Template>
                <px:PXGrid ID="formQuotes" runat="server" DataSourceID="ds" Width="100%" SkinID="Inquire" BorderStyle="None" SyncPosition="True">
                    <Levels>                        
                        <px:PXGridLevel DataMember="Quotes">  
                            <RowTemplate>
                                <px:PXCheckBox ID="chkIsPrimary" runat="server" DataField="IsPrimary">                                    
                                </px:PXCheckBox>                                     
                            </RowTemplate>                          
                            <Columns>
                                <px:PXGridColumn DataField="IsPrimary" TextAlign="Center" AllowNull="False" Type="CheckBox" LinkCommand="PrimaryQuote"/> 
                                <px:PXGridColumn DataField="QuoteNbr" LinkCommand="ViewQuote"  />
                                <px:PXGridColumn DataField="QuoteType" />
                                <px:PXGridColumn DataField="Subject" />                                
                                <px:PXGridColumn DataField="Status" RenderEditorText="True" />
                                <px:PXGridColumn DataField="DocumentDate" />
                                <px:PXGridColumn DataField="ExpirationDate" />
                                <px:PXGridColumn DataField="CuryID" />
								<px:PXGridColumn DataField="ManualTotalEntry" Type="CheckBox" TextAlign="Center" />
	                            <px:PXGridColumn DataField="CuryAmount" TextAlign="Right" />
                                <px:PXGridColumn DataField="CuryDiscTot" TextAlign="Right" />
                                <px:PXGridColumn DataField="CuryTaxTotal" TextAlign="Right" />
	                            <px:PXGridColumn DataField="CuryProductsAmount" TextAlign="Right" />
                                <px:PXGridColumn DataField="QuoteProjectID" LinkCommand="ViewProject" />
                            </Columns>                            
                        </px:PXGridLevel>
                    </Levels>
                    <ActionBar PagerVisible="False" DefaultAction="ViewQuote" >
                        <CustomItems>                            
	                        <px:PXToolBarButton ImageKey="AddNew" Tooltip="Create Quote" DisplayStyle="Image">
		                        <AutoCallBack Command="CreateQuote" Target="ds" />		                        
	                        </px:PXToolBarButton>
                            <px:PXToolBarButton Text="Copy Quote" Key="CopyQuote">
	                            <AutoCallBack Target="ds" Command="CopyQuote"/>	                            
                            </px:PXToolBarButton>
	                        <px:PXToolBarButton Text="Print" Key="PrintQuote">
		                        <AutoCallBack Target="ds" Command="PrintQuote"/>		                        
	                        </px:PXToolBarButton>
                            <px:PXToolBarButton Text="Send" Key="SendQuote">
                                <AutoCallBack Target="ds" Command="SendQuote"/>		                        
                            </px:PXToolBarButton>
	                        <px:PXToolBarButton Text="" Key="PrimaryQuote">
		                        <AutoCallBack Target="ds" Command="PrimaryQuote"/>		                        
	                        </px:PXToolBarButton>	                        
                        </CustomItems>
                    </ActionBar>
	                <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                    <AutoSize Enabled="True" />
                </px:PXGrid>
            </Template>
        </px:PXTabItem>
            <px:PXTabItem Text="Contact Info" RepaintOnDemand="False">
                <Template>
                    <px:PXFormView ID="edOpportunityCurrent" runat="server" DataMember="OpportunityCurrent" DataSourceID="ds" RenderStyle="Simple">
                        <Template>
                            <px:PXCheckBox ID="edAllowOverrideContactAddress" runat="server" DataField="AllowOverrideContactAddress" CommitChanges="true"/>
                        </Template>
                        <ContentStyle BackColor="Transparent"/>
                    </px:PXFormView>   

                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXFormView ID="edOpportunity_Contact" runat="server" DataMember="Opportunity_Contact" DataSourceID="ds" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" GroupCaption="Contact Information" StartGroup="True"/>
                            <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" CommitChanges="true"/>					        					        
                            <px:PXLayoutRule runat="server" Merge="True" />
        			        <px:PXLabel ID="LFirstName" runat="server" Size="SM" />
        			        <px:PXDropDown ID="edTitle" runat="server" DataField="Title" Size="XS" SuppressLabel="True" CommitChanges="True" />
        			        <px:PXTextEdit ID="edFirstName" runat="server" DataField="FirstName" Width="164px"  LabelID="LFirstName" CommitChanges="true"/>
        			        <px:PXLayoutRule runat="server" />
        			        <px:PXTextEdit ID="edLastName" runat="server" DataField="LastName" SuppressLabel="False" CommitChanges="true"/>
        			        <px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" SuppressLabel="False" CommitChanges="true"/>
        			        <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" CommitChanges="true" />
                            <px:PXMailEdit ID="edEMail" runat="server" CommandSourceID="ds" DataField="EMail" CommitChanges="True"/>
        			        <px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
                            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" ControlSize="SM" />
                            <px:PXDropDown ID="edPhone1Type" runat="server" DataField="Phone1Type" SuppressLabel="True" CommitChanges="True" Width="104px"/>
                            <px:PXLabel ID="LPhone1" runat="server" SuppressLabel="true" />
        			        <px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" LabelWidth="0px" Size="XM" LabelID="LPhone1" SuppressLabel="True" CommitChanges="true"/>
                            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" ControlSize="SM" SuppressLabel="True" />
                            <px:PXDropDown ID="edPhone2Type" runat="server" DataField="Phone2Type" SuppressLabel="True" CommitChanges="True" Width="104px"/>
                            <px:PXLabel ID="LPhone2" runat="server" SuppressLabel="true" />
        			        <px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" LabelWidth="0px" Size="XM" LabelID="LPhone2" SuppressLabel="True" CommitChanges="true"/>
                            <px:PXLayoutRule ID="PXLayoutRule12" runat="server" Merge="True" ControlSize="SM" SuppressLabel="True" />
                            <px:PXDropDown ID="edPhone3Type" runat="server" DataField="Phone3Type" SuppressLabel="True" CommitChanges="True" Width="104px"/>
                            <px:PXLabel ID="LPhone3" runat="server" SuppressLabel="true"  />
        			        <px:PXMaskEdit ID="edPhone3" runat="server" DataField="Phone3" LabelWidth="0px" Size="XM" SuppressLabel="True" LabelID="LPhone3"/>
                            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="True" ControlSize="SM" SuppressLabel="True" />
                            <px:PXDropDown ID="edFaxType" runat="server" DataField="FaxType" SuppressLabel="True" CommitChanges="True" Width="104px" />
                            <px:PXLabel ID="LFax" runat="server" SuppressLabel="true" />
        			        <px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" LabelWidth="0px" Size="XM" LabelID="LFax" SuppressLabel="True" CommitChanges="true"/>

        			        <px:PXLayoutRule runat="server" />					
                       </Template>
                        <ContentStyle BackColor="Transparent" />
                    </px:PXFormView>
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />   
                    <px:PXFormView ID="edOpportunity_Address" runat="server" DataMember="Opportunity_Address" DataSourceID="ds" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" GroupCaption="Address" StartGroup="True" />                            
                            <px:PXCheckBox ID="chkIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" CommitChanges="true"/>
        					<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" CommitChanges="true"/>
        					<px:PXTextEdit ID="edCity" runat="server" DataField="City" CommitChanges="true"/>
                            <px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" CommitChanges="true"
                                           FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
                            <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" Size="S" CommitChanges="True" />
        					<px:PXSelector ID="edCountryID" runat="server" AllowEdit="True" DataField="CountryID"
        						FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" CommitChanges="True" />
        					<px:PXLayoutRule runat="server" Merge="True" />
        					<px:PXButton ID="btnViewMainOnMap" runat="server" CommandName="ViewMainOnMap" CommandSourceID="ds" TabIndex="-1"
        						Size="xs" Text="View On Map" />
        					<px:PXLayoutRule runat="server" />
                       </Template>                       
                    </px:PXFormView>
                    <px:PXLayoutRule ID="PXLayoutRule16" runat="server" GroupCaption="Personal Data Privacy" StartGroup="True" />
                         <px:PXFormView ID="edOpportunity_Contact_GDPR" runat="server" DataMember="Opportunity_Contact" DataSourceID="ds" RenderStyle="Simple" SkinID="Transparent">
                            <Template>
                                <px:PXLayoutRule ID="PXLayoutRule16" runat="server" ControlSize="M" LabelsWidth="M"/>
                                <px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="ConsentAgreement" AlignLeft="True" CommitChanges="True"/>
                                <px:PXDateTimeEdit ID="edConsentDate" runat="server" DataField="ConsentDate" CommitChanges="True"/>
                                <px:PXDateTimeEdit ID="edConsentExpirationDate" runat="server" DataField="ConsentExpirationDate" CommitChanges="True"/>
                            </Template>
                            <ContentLayout OuterSpacing="None"/>
                            <ContentStyle BackColor="Transparent" BorderStyle="None"/>
                    </px:PXFormView>

                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Shipping Info">
                <Template>
                    <px:PXCheckBox ID="edAllowOverrideShippingContactAddress" runat="server" Size="SM" DataField="AllowOverrideShippingContactAddress" CommitChanges="true" AlignLeft="True" />
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
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" Height="200px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" AllowResize="True" />
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" AllowShowHide="False" TextField="AttributeID_description" />
                                    <px:PXGridColumn DataField="Value" AllowShowHide="False" AllowSort="False" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                        <ActionBar>
                            <Actions>
                                <Search Enabled="False" />
                            </Actions>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Relations" LoadOnDemand="True">
                <Template>
					  <px:PXGrid ID="grdRelations" runat="server" Height="400px" Width="100%" AllowPaging="True" SyncPosition="True" MatrixMode="True"
                        ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" SkinID="Details">
                        <Levels>
                          <px:PXGridLevel DataMember="Relations">
                            <Columns>
                              <px:PXGridColumn DataField="Role" CommitChanges="True"></px:PXGridColumn>
                              <px:PXGridColumn DataField="IsPrimary" Type="CheckBox" TextAlign="Center" CommitChanges="True"></px:PXGridColumn>
                              <px:PXGridColumn DataField="TargetType" CommitChanges="True"></px:PXGridColumn>
                              <px:PXGridColumn DataField="TargetNoteID" DisplayMode="Text"  LinkCommand="Relations_TargetDetails" CommitChanges="True"></px:PXGridColumn>
                              <px:PXGridColumn DataField="EntityID" AutoCallBack="true" LinkCommand="Relations_EntityDetails" CommitChanges="True"></px:PXGridColumn>
                              <px:PXGridColumn DataField="Name" ></px:PXGridColumn>
                              <px:PXGridColumn DataField="ContactID" AutoCallBack="true" TextAlign="Left" TextField="ContactName" DisplayMode="Text" LinkCommand="Relations_ContactDetails" ></px:PXGridColumn>
                              <px:PXGridColumn DataField="Email" ></px:PXGridColumn>
                              <px:PXGridColumn DataField="AddToCC" Type="CheckBox" TextAlign="Center" ></px:PXGridColumn>
                            </Columns>
                            <RowTemplate>
                              <px:PXSelector ID="edTargetNoteID" runat="server" DataField="TargetNoteID" FilterByAllFields="True" AutoRefresh="True" />
                              <px:PXSelector ID="edRelEntityID" runat="server" DataField="EntityID" FilterByAllFields="True" AutoRefresh="True" />
                              <px:PXSelector ID="edRelContactID" runat="server" DataField="ContactID" FilterByAllFields="True" AutoRefresh="True" />
                            </RowTemplate>
                          </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="True" ></Mode>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" ></AutoSize>
                      </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Tax Details" LoadOnDemand="true" Key="TaxDetailsTab">
                <Template>
                    <px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" SkinID="Details" ActionsPosition="Top" BorderWidth="0px">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Levels>
                            <px:PXGridLevel DataMember="Taxes">
                                <Columns>
                                    <px:PXGridColumn DataField="TaxID" AllowUpdate="False" CommitChanges="true" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TaxRate" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryTaxableAmt" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryExemptedAmt" TextAlign="Right" />
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
            <px:PXTabItem Text="Discount Details" BindingContext="form" RepaintOnDemand="false" Key="DiscountDetailsTab">
                <Template>
                    <px:PXGrid ID="formDiscountDetail" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" BorderStyle="None" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="DiscountDetails">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXCheckBox ID="chkSkipDiscount" runat="server" DataField="SkipDiscount" />
                                    <px:PXSelector ID="edDiscountID" runat="server" DataField="DiscountID"
                                        AllowEdit="True" edit="1" />
                                    <px:PXSelector ID="edDiscountSequenceID" runat="server" DataField="DiscountSequenceID" AllowEdit="True" AutoRefresh="True" edit="1" />
                                    <px:PXDropDown ID="edType" runat="server" DataField="Type" Enabled="False" />
                                    <px:PXCheckBox ID="chkIsManual" runat="server" DataField="IsManual" />
                                    <px:PXNumberEdit ID="edCuryDiscountableAmt" runat="server" DataField="CuryDiscountableAmt" />
                                    <px:PXNumberEdit ID="edDiscountableQty" runat="server" DataField="DiscountableQty" />
                                    <px:PXNumberEdit ID="edCuryDiscountAmt" runat="server" DataField="CuryDiscountAmt" CommitChanges="true" />
                                    <px:PXNumberEdit ID="edDiscountPct" runat="server" DataField="DiscountPct" CommitChanges="true" />
                                    <px:PXSegmentMask ID="edFreeItemID" runat="server" DataField="FreeItemID" AllowEdit="True" />
                                    <px:PXNumberEdit ID="edFreeItemQty" runat="server" DataField="FreeItemQty" />
                                    <px:PXTextEdit ID="edExtDiscCode" runat="server" DataField="ExtDiscCode" />
                                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SkipDiscount" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="DiscountID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="DiscountSequenceID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Type" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="IsManual" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="CuryDiscountableAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="DiscountableQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryDiscountAmt" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="DiscountPct" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="FreeItemID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" />
                                    <px:PXGridColumn DataField="FreeItemQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ExtDiscCode" />
                                    <px:PXGridColumn DataField="Description" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Orders" LoadOnDemand="True" Key="OrdersTab">
                <Template>
                      <px:PXGrid ID="grdSalesOrders" runat="server" Height="400px" Width="100%" AllowPaging="True" SyncPosition="True" MatrixMode="True"
                        ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" SkinID="Inquire">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Levels>
                          <px:PXGridLevel DataMember="SalesOrders">
                            <Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />
                            <Columns>
                              <px:PXGridColumn DataField="OrderType" />
                              <px:PXGridColumn DataField="OrderNbr" DisplayMode="Text" LinkCommand="SalesOrders_OrderDetails" CommitChanges="True" />
                              <px:PXGridColumn DataField="OrderDesc" />
                              <px:PXGridColumn DataField="Status" />
                              <px:PXGridColumn DataField="OrderDate" />
                              <px:PXGridColumn DataField="RequestDate" />
                              <px:PXGridColumn DataField="CuryID" />
                              <px:PXGridColumn DataField="CuryOrderTotal" />
                              <px:PXGridColumn DataField="ShipDate" AllowShowHide="true" Visible="False" SyncVisible="false"/>
                            </Columns>
                          </px:PXGridLevel>
                        </Levels>
                      </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Invoices" LoadOnDemand="True" Key="InvoicesTab">
                <Template>
                      <px:PXGrid ID="grdInvoices" runat="server" Height="400px" Width="100%" AllowPaging="True" SyncPosition="True" MatrixMode="True"
                        ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" SkinID="Inquire">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Levels>
                          <px:PXGridLevel DataMember="Invoices">
                            <Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />
                            <Columns>
                              <px:PXGridColumn DataField="DocType" />
                              <px:PXGridColumn DataField="RefNbr" DisplayMode="Text" LinkCommand="Invoices_InvoiceDetails" CommitChanges="True" />
                              <px:PXGridColumn DataField="DocDesc" />
                              <px:PXGridColumn DataField="Status" />
                              <px:PXGridColumn DataField="DocDate" />
                              <px:PXGridColumn DataField="CuryID" />
                              <px:PXGridColumn DataField="CuryOrigDocAmt" />
                              <px:PXGridColumn DataField="CuryDocBal" />
                            </Columns>
                          </px:PXGridLevel>
                        </Levels>
                      </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Sync Status">
				<Template>
					<px:PXGrid ID="syncGrid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" SkinID="Inquire" SyncPosition="true">
						<Levels>
							<px:PXGridLevel DataMember="SyncRecs" DataKeyNames="SyncRecordID">
								<Columns>
									<px:PXGridColumn DataField="SYProvider__Name" />
									<px:PXGridColumn DataField="RemoteID" CommitChanges="True" LinkCommand="GoToSalesforce" />
									<px:PXGridColumn DataField="Status" />
									<px:PXGridColumn DataField="Operation" />
									<px:PXGridColumn DataField="LastErrorMessage" />
									<px:PXGridColumn DataField="LastAttemptTS" DisplayFormat="g" />
									<px:PXGridColumn DataField="AttemptCount" />
									<px:PXGridColumn DataField="SFEntitySetup__ImportScenario" />
									<px:PXGridColumn DataField="SFEntitySetup__ExportScenario" />
								</Columns>                               
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<ActionBar>                        
							<CustomItems>
								<px:PXToolBarButton Key="SyncSalesforce">
									<AutoCallBack Command="SyncSalesforce" Target="ds"/>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Mode InitNewRow="true" />
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
    </px:PXTab>
</asp:Content>
<asp:Content ID="Dialogs" ContentPlaceHolderID="phDialogs" runat="Server">
    <px:PXSmartPanel ID="PanelCreateQuote" runat="server" Caption="Create New Quote"
                     CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="QuoteInfo" AutoCallBack-Enabled="true" AutoCallBack-Target="formCreateQuote" AutoCallBack-Command="Refresh"
                     CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" Width="400px" AcceptButtonID="PXButton2" CancelButtonID="PXButton3">
        <px:PXFormView ID="formCreateQuote" runat="server" DataSourceID="ds" Width="100%" Caption="Services Settings" CaptionVisible="False" SkinID="Transparent"
                       DataMember="QuoteInfo">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule6" runat="server" LabelsWidth="S" ControlSize="XM" />
                <px:PXDropDown ID="edQuoteType" runat="server" DataField="QuoteType" CommitChanges="true"/>
                <px:PXCheckBox ID="edAddProductsFromOpportunity" runat="server" DataField="AddProductsFromOpportunity" CommitChanges="true"/>
                <px:PXCheckBox ID="edMakeNewQuotePrimary" runat="server" DataField="MakeNewQuotePrimary" CommitChanges="true"/>
                <px:PXCheckBox ID="edRecalculatePrices" runat="server" DataField="RecalculatePrices" CommitChanges="true"/>
                <px:PXCheckBox ID="edOverrideManualPrices" runat="server" DataField="OverrideManualPrices" CommitChanges="true" Style="margin-left: 25px"/>
                <px:PXCheckBox ID="edRecalculateDiscounts" runat="server" DataField="RecalculateDiscounts" CommitChanges="true"/>
                <px:PXCheckBox ID="edOverrideManualDiscounts" runat="server" DataField="OverrideManualDiscounts" CommitChanges="true" Style="margin-left: 25px"/>
                <px:PXCheckBox ID="edOverrideManualDocGroupDiscounts" runat="server" DataField="OverrideManualDocGroupDiscounts" CommitChanges="true" Style="margin-left: 25px"/> 
            </Template>

        </px:PXFormView>
        <div style="padding: 5px; text-align: right;">
            <px:PXButton ID="PXButton1" runat="server" Text="Create and Review" CommitChanges="True" DialogResult="Yes" Width="160px" Height="20px"></px:PXButton>
            <px:PXButton ID="PXButton2" runat="server" Text="Create" CommitChanges="True" DialogResult="No" Width="63px" Height="20px" Style="margin-left: 5px" />
            <px:PXButton ID="PXButton3" runat="server" Text="Cancel" CommitChanges="True" DialogResult="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
        </div>

    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelCreateInvoice" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="Create New Invoice"
                     CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="InvoiceInfo" AutoCallBack-Enabled="true" AutoCallBack-Target="formCreateInvoice" AutoCallBack-Command="Refresh"
                     CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButton5" CancelButtonID="PXButton6">
        <px:PXFormView ID="formCreateInvoice" runat="server" DataSourceID="ds" Style="z-index: 100; text-align: left;" Width="100%" Caption="Services Settings" CaptionVisible="False" SkinID="Transparent"
                       DataMember="InvoiceInfo">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartRow="True" LabelsWidth="SM" ControlSize="XM" SuppressLabel="True" />
                <px:PXCheckBox ID="edRecalculatePrices" runat="server" DataField="RecalculatePrices" CommitChanges="true"/>
                <px:PXCheckBox ID="edOverrideManualPrices" runat="server" DataField="OverrideManualPrices" CommitChanges="true" Style="margin-left: 25px"/>
                <px:PXCheckBox ID="edRecalculateDiscounts" runat="server" DataField="RecalculateDiscounts" CommitChanges="true"/>
                <px:PXCheckBox ID="edOverrideManualDiscounts" runat="server" DataField="OverrideManualDiscounts" CommitChanges="true" Style="margin-left: 25px"/>
                <px:PXCheckBox ID="edOverrideManualDocGroupDiscounts" runat="server" DataField="OverrideManualDocGroupDiscounts" CommitChanges="true" Style="margin-left: 25px"/> 
                <px:PXCheckBox ID="edConfirmManualAmount" runat="server" DataField="ConfirmManualAmount" Width="480px" CommitChanges="true"/>
            </Template>
        </px:PXFormView>
        <div style="padding: 5px; text-align: right;">
            <px:PXButton ID="PXButton5" runat="server" Text="Create" CommitChanges="True" DialogResult="Yes" Width="63px" Height="20px" Style="margin-left: 5px" />
            <px:PXButton ID="PXButton6" runat="server" Text="Cancel" CommitChanges="True" DialogResult="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
        </div>

    </px:PXSmartPanel>

    <px:PXSmartPanel ID="panelCreateSalesOrder" runat="server" Style="z-index: 108; left: 27px; position: absolute; top: 99px;" Caption="Create Sales Order"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="CreateOrderParams" AutoCallBack-Enabled="true" AutoCallBack-Target="formCopyTo" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="btnSave" CancelButtonID="btnCancel">
        <px:PXFormView ID="formCopyTo" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False" DataMember="CreateOrderParams">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXSelector ID="edOrderType" runat="server" AllowNull="False" DataField="OrderType" DisplayMode="Text" Width="216px" CommitChanges="True"/>
                <px:PXCheckBox ID="edRecalculatePrices" runat="server" DataField="RecalculatePrices" CommitChanges="true"/>
                <px:PXCheckBox ID="edOverrideManualPrices" runat="server" DataField="OverrideManualPrices" CommitChanges="true" Style="margin-left: 25px"/>                
                <px:PXCheckBox ID="edRecalculateDiscounts" runat="server" DataField="RecalculateDiscounts" CommitChanges="true"/>
                <px:PXCheckBox ID="edOverrideManualDiscounts" runat="server" DataField="OverrideManualDiscounts" CommitChanges="true" Style="margin-left: 25px"/>
                <px:PXCheckBox ID="edOverrideManualDocGroupDiscounts" runat="server" DataField="OverrideManualDocGroupDiscounts" CommitChanges="true" Style="margin-left: 25px"/> 
                <px:PXCheckBox ID="edConfirmManualAmount" runat="server" DataField="ConfirmManualAmount" Width="480px" CommitChanges="true" UncheckImages=""/>
            </Template>
        </px:PXFormView>

        <div style="padding: 5px; text-align: right;">
                <px:PXButton ID="btnSave" runat="server" CommitChanges="True" DialogResult="OK" Text="OK" Height="20"/>
                <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" Height="20" />
        </div>
    </px:PXSmartPanel>

    <px:PXSmartPanel ID="panelCreateServiceOrder" runat="server" Style="z-index: 108; left: 27px; position: absolute; top: 99px;" Caption="Create Service Order" Width="380px" Height="130px" AutoRepaint="true"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="CreateServiceOrderFilter" AutoCallBack-Enabled="true" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">
        <px:PXFormView ID="formCreateServiceOrder" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False" DataMember="CreateServiceOrderFilter">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXSelector ID="edSrvOrdType" runat="server" AllowNull="False" DataField="SrvOrdType" DisplayMode="Text" CommitChanges="True" AutoRefresh="True" AllowEdit="True" />
                <px:PXSelector ID="edBranchLocationID" runat="server" AllowNull="False" DataField="BranchLocationID" DisplayMode="Text" CommitChanges="True" AutoRefresh="True" AllowEdit="True"/>
            </Template>
        </px:PXFormView>

        <div style="padding: 5px; text-align: right;">
                <px:PXButton ID="btnSave2" runat="server" CommitChanges="True" DialogResult="OK" Text="OK" Height="20"/>
                <px:PXButton ID="btnCancel2" runat="server" DialogResult="Cancel" Text="Cancel" Height="20" />
        </div>
    </px:PXSmartPanel>

     <px:PXSmartPanel ID="PanelCopyQuote" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="Copy Quote"
        CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="CopyQuoteInfo" AutoCallBack-Enabled="true" AutoCallBack-Target="formCopyQuote" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButton8" CancelButtonID="PXButton9">
        <px:PXFormView ID="formCopyQuote" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Services Settings" CaptionVisible="False" SkinID="Transparent"
            DataMember="CopyQuoteInfo">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSelector ID="edOpportunityId" runat="server" DataField="OpportunityId" CommitChanges="true"/>
                <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" CommitChanges="true"/>                
                <px:PXCheckBox ID="edRecalculatePrices" runat="server" DataField="RecalculatePrices" CommitChanges="true"/>
                <px:PXCheckBox ID="edOverrideManualPrices" runat="server" DataField="OverrideManualPrices" CommitChanges="true" Style="margin-left: 25px"/>                
                <px:PXCheckBox ID="edRecalculateDiscounts" runat="server" DataField="RecalculateDiscounts" CommitChanges="true"/>
                <px:PXCheckBox ID="edOverrideManualDiscounts" runat="server" DataField="OverrideManualDiscounts" CommitChanges="true" Style="margin-left: 25px"/>
                <px:PXCheckBox ID="edOverrideManualDocGroupDiscounts" runat="server" DataField="OverrideManualDocGroupDiscounts" CommitChanges="true" Style="margin-left: 25px"/>   
            </Template>
        </px:PXFormView>
        <div style="padding: 5px; text-align: right;">
            <px:PXButton ID="PXButton8" runat="server" Text="OK" DialogResult="Yes" Width="63px" Height="20px"></px:PXButton>
            <px:PXButton ID="PXButton9" runat="server" DialogResult="No" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
        </div>
    </px:PXSmartPanel> 

    <px:PXSmartPanel ID="PanelRecalculate" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="Recalculate Prices"
        CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="recalcdiscountsfilter" AutoCallBack-Enabled="true" AutoCallBack-Target="formRecalculate" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButton4" CancelButtonID="PXButton7">
        <px:PXFormView ID="formRecalculate" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Services Settings" CaptionVisible="False" SkinID="Transparent"
            DataMember="recalcdiscountsfilter">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                    <px:PXDropDown ID="edRecalcTerget" runat="server" DataField="RecalcTarget" CommitChanges="true" />
                    <px:PXCheckBox ID="edRecalculatePrices" runat="server" DataField="RecalcUnitPrices" CommitChanges="true"/>
                    <px:PXCheckBox ID="edOverrideManualPrices" runat="server" DataField="OverrideManualPrices" CommitChanges="true" Style="margin-left: 25px"/>                
                    <px:PXCheckBox ID="edRecalculateDiscounts" runat="server" DataField="RecalcDiscounts" CommitChanges="true"/>
                    <px:PXCheckBox ID="edOverrideManualDiscounts" runat="server" DataField="OverrideManualDiscounts" CommitChanges="true" Style="margin-left: 25px"/>
                    <px:PXCheckBox ID="edOverrideManualDocGroupDiscounts" runat="server" DataField="OverrideManualDocGroupDiscounts" CommitChanges="true" Style="margin-left: 25px"/> 
                </Template>
            </px:PXFormView>        
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
             <div style="padding: 5px; text-align: right;">
            <px:PXButton ID="PXButton4" runat="server" Text="OK" DialogResult="OK" Width="63px" Height="20px"></px:PXButton>
            <px:PXButton ID="PXButton7" runat="server" DialogResult="No" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
        </div>
        </px:PXPanel>
    </px:PXSmartPanel>

	<!--#include file="~\Pages\CR\Includes\CRCreateContactPanel.inc"-->

	<!--#include file="~\Pages\CR\Includes\CRCreateBothContactAndAccountPanel.inc"-->

</asp:Content>
