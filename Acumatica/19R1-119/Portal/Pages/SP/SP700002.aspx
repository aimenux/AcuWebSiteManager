<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="SP700002.aspx.cs" Inherits="Pages_SP700002"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SO.SOOrderEntry" PrimaryView="Document">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="proceedtoCheckout" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="SubmitOrder" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="GoBack" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="ShippingDetail" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" Visible="false" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="Cancel" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="Delete" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="CopyPaste" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="Next" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="Previous" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" Visible="false" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="CurrencyView" Visible="False" />
            <px:PXDSCallbackCommand Visible="False" Name="LSSOLine_generateLotSerial" CommitChanges="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSSOLine_binLotSerial" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="POSupplyOK" Visible="False" CommitChanges="true" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="Hold" Visible="false" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="CreditHold" Visible="false" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Flow" Visible="false" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Action" Visible="False" CommitChanges="true" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Inquiry" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="Report" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AddInvoice" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AddInvoiceOK" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="CheckCopyParams" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
            <px:PXDSCallbackCommand Name="InventorySummary" Visible="false" CommitChanges="true" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Visible="false" Name="CalculateFreight" />
            <px:PXDSCallbackCommand Visible="false" Name="RecalculatePackages" />
            <px:PXDSCallbackCommand Visible="false" Name="ShopRates" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="RefreshRates" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="CreatePayment" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewPayment" DependOnGrid="detgrid" />
            <px:PXDSCallbackCommand Visible="false" Name="AuthorizeCCPayment" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="VoidCCPayment" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="CaptureCCPayment" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="CreditCCPayment" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="ValidateCCPayment" />
            <px:PXDSCallbackCommand Visible="false" Name="RecalcExternalTax" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="CreatePrepayment" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="AddInvBySite" Visible="False" CommitChanges="true" />

            <px:PXDSCallbackCommand Name="PasteLine" Visible="False"/>
			<px:PXDSCallbackCommand Name="openAppointmentBoard" Visible="False"/>
            <px:PXDSCallbackCommand Name="ResetOrder" Visible="False"/>

            <px:PXDSCallbackCommand Name="AddInvSelBySite" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="CreateCCPaymentMethodHF" PopupCommand="SyncCCPaymentMethods" PopupCommandTarget="ds" DependOnGrid="grdPMInstanceDetails" Visible="False" />
            <px:PXDSCallbackCommand Name="SyncCCPaymentMethods" CommitChanges="true" Visible="False" />
            <px:PXDSCallbackCommand Name="RecalculateDiscountsAction" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="RecalcOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
            <px:PXDSCallbackCommand Visible="false" Name="SOLineSplit$RefNoteID$Link" DependOnGrid="grid2" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="QuickProcessOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
            <px:PXDSCallbackCommand Name="QuickProcess" ShowProcessInfo="true" Visible="False"/>
            <px:PXDSCallbackCommand Name="recalculateDiscountsFromImport" Visible="false" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>

<asp:Content ID="con1" ContentPlaceHolderID="phF" runat="Server">
    <px:PXToolBar ID="toolbar1" runat="server" SkinID="Navigation" BackColor="Transparent" CommandSourceID="PXWizard">
        <Items>
            <px:PXToolBarButton Text="Submit Order" Tooltip="Submit Order" CommandName="SubmitOrder" CommandSourceID="ds" />
            <px:PXToolBarButton Text="Continue" Tooltip="Continue" CommandName="WizNext" />
            <px:PXToolBarButton CommandName="GoBack" CommandSourceID="ds" />
            <px:PXToolBarButton Text="Return to Shipping Detail" Tooltip="Return to Shipping Detail" CommandName="WizPrev" />
        </Items>
        <Layout ItemsAlign="Left" />
    </px:PXToolBar>
    <px:PXWizard ID="PXWizard" runat="server" Width="100%" DataMember="Document" SkinID="Flat" ButtonsVisible="False" style="background-color: #F5F5F5 !important" 
        NoteIndicator="False" FilesIndicator="False">
        <NextCommand Command="proceedtoCheckout" Target="ds"></NextCommand>
        <PrevCommand Command="shippingDetail" Target="ds"></PrevCommand>
        <CancelCommand Command="goBack" Target="ds"></CancelCommand>
        <SaveCommand Command="submitOrder" Target="ds"></SaveCommand>
        <AutoSize Enabled="True" Container="Window" MinHeight="300" MinWidth="400" />
        <Pages>
            <px:PXWizardPage>
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule29" runat="server" StartColumn="True" StartGroup="True" GroupCaption="Ship-To Info" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXSegmentMask CommitChanges="True" ID="edCustomerLocationID" runat="server" AutoRefresh="True"
                        DataField="CustomerLocationID" DataSourceID="ds" SelectOnly="True"/>
                    
                    <px:PXCheckBox CommitChanges="True" ID="chkOverrideShipment" runat="server" DataField="OverrideShipment" />
                                        
                    <px:PXTextEdit ID="edFullName" runat="server" DataField="Shipping_Contact.FullName" />
                    <px:PXTextEdit ID="edAttention" runat="server" DataField="Shipping_Contact.Attention" />
                    <px:PXMaskEdit ID="edPhone1" runat="server" DataField="Shipping_Contact.Phone1" />
                    <px:PXTextEdit ID="edEmail" runat="server" DataField="Shipping_Contact.Email" CommitChanges="True" />
                    
                    <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="Shipping_Address.AddressLine1" />
                    <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="Shipping_Address.AddressLine2" />
                    <px:PXTextEdit ID="edCity" runat="server" DataField="Shipping_Address.City" />
                    <px:PXSelector ID="edCountryID" runat="server" DataField="Shipping_Address.CountryID" AutoRefresh="True" DataSourceID="ds"/>
                    <px:PXSelector ID="edState" runat="server" DataField="Shipping_Address.State" AutoRefresh="True" DataSourceID="ds">
                        <CallBackMode PostData="Container" />
                        <Parameters>
                            <px:PXControlParam ControlID="PXWizard" Name="SOShippingAddress.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSelector>
                    <px:PXMaskEdit CommitChanges="True" ID="edPostalCode" runat="server" DataField="Shipping_Address.PostalCode" />
                    <px:PXLayoutRule ID="PXLayoutRule34" runat="server" StartColumn="True" StartGroup="True" GroupCaption="Shipping Information" LabelsWidth="SM" ControlSize="XM" />
                    
                    <px:PXNumberEdit ID="edCuryLineTotal" runat="server" Enabled="False" DataField="CurrentDocument.PortalLineTotal" />
                    <px:PXNumberEdit ID="edCuryFreightAmt" runat="server" Enabled="False" DataField="CurrentDocument.CuryFreightAmt" />
                    <px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" />
                    <px:PXNumberEdit ID="edCuryDiscTot" runat="server" DataField="CuryDiscTot" Enabled="False" />
                    
                    <px:PXLayoutRule ID="PXLayoutRule10" runat="server" Merge="True" />
                    <px:PXNumberEdit ID="edCuryOrderTotal" runat="server" DataField="CuryOrderTotal" Enabled="False" />
                    <px:PXTextEdit ID="edCurrencyStatus" runat="server" DataField="finalFilter.CurrencyStatus" Size="XS" Enabled="False" SuppressLabel="true" />
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="False" />
                    
                    <px:PXSelector CommitChanges="True" Size="s" ID="edShipVia" runat="server" DataField="CurrentDocument.ShipVia" DataSourceID="ds" />
                    <px:PXDateTimeEdit CommitChanges="True" ID="edRequestDate" runat="server" DataField="RequestDate" />
                    <px:PXCheckBox ID="chkResedential" runat="server" DataField="CurrentDocument.Resedential" CommitChanges="True"/>
                    <px:PXCheckBox CommitChanges="True" ID="chkUseCustomerAccount" runat="server" DataField="CurrentDocument.UseCustomerAccount" />
                    <px:PXDropDown ID="edShipComplete" runat="server" AllowNull="False" DataField="ShipComplete"/>
                    <px:PXTextEdit ID="edComment" runat="server" DataField="Comment" TextMode="MultiLine" Width="400" Height="123" SuppressLabel="True" Style="resize: none" />
                </Template>
            </px:PXWizardPage>
            <px:PXWizardPage>
                <Template>
                    <px:PXPanel runat="server" ID="pnlTBR">
                    <px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartColumn="True" StartGroup="true" GroupCaption="Shipping Information" ColumnWidth="L" />
                    <px:PXTextEdit ID="edShippingInformation" runat="server" DataField="finalFilter.ShippingInformation" Size="L" 
                        SuppressLabel="true" Height="210px" TextMode="MultiLine" Style="resize: none"  Enabled="False"/>
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" StartGroup="true" GroupCaption="Totals" />
                    <px:PXNumberEdit ID="edCuryLineTotalfinal" runat="server" Enabled="False" DataField="finalFilter.CuryLineTotal" Size="S"/>
                    <px:PXNumberEdit ID="edCuryFreightAmtfinal" runat="server" Enabled="False" DataField="finalFilter.CuryFreightAmt" Size="S" />
                    <px:PXNumberEdit ID="edCuryTaxTotalfinal" runat="server" Enabled="False" DataField="finalFilter.CuryTaxTotal" Size="S" />
                    <px:PXNumberEdit ID="edCuryDiscTotfinal" runat="server" Enabled="False" DataField="finalFilter.CuryDiscTot" Size="S" />
                    <px:PXLayoutRule ID="PXLayoutRule10" runat="server" Merge="True" />
                    <px:PXNumberEdit ID="edCuryOrderTotalfinal" runat="server" Enabled="False" DataField="finalFilter.CuryOrderTotal" Size="S" />
                    <px:PXTextEdit ID="PXTextEdit4" runat="server" DataField="finalFilter.CurrencyStatus" Size="XS" Enabled="False" SuppressLabel="true" />
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="False" />
                    </px:PXPanel>

                    <px:PXGrid ID="gridDocumentCardDetails" runat="server" DataSourceID="ds"
                        Width="100%" SkinID="Details" AdjustPageSize="Auto" AllowSearch="True"
                        FilesIndicator="False" NoteIndicator="False" SyncPosition="true" AllowPaging="true">
                        <Levels>
                            <px:PXGridLevel DataMember="DocumentCardDetails">
                                <RowTemplate>
                                    <px:PXTextEdit ID="edInventoryIDDescription" runat="server" DataField="InventoryIDDescription" />
                                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
                                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
                                    <px:PXNumberEdit ID="edCuryUnitPrice" runat="server" DataField="CuryUnitPrice" />
                                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" />
                                    <px:PXNumberEdit ID="edBaseDiscountAmt" runat="server" DataField="BaseDiscountAmt" />
                                    <px:PXNumberEdit ID="edTotalPrice" runat="server" DataField="TotalPrice" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="InventoryIDDescription" Width="100px" />
                                    <px:PXGridColumn DataField="Descr" Width="160px" />
                                    <px:PXGridColumn DataField="Qty" Width="60px" />
                                    <px:PXGridColumn DataField="CuryUnitPrice" Width="100px" />
                                    <px:PXGridColumn DataField="UOM" Width="100px" />
                                    <px:PXGridColumn DataField="BaseDiscountAmt" Width="100px" />
                                    <px:PXGridColumn DataField="TotalPrice" Width="100px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <CallbackCommands>
                            <Refresh CommitChanges="true" />
                        </CallbackCommands>
                        <Mode AllowAddNew="False" AutoInsert="False" />
                        <ActionBar>
                            <Actions>
                                <ExportExcel Enabled="False" />
                                <AddNew Enabled="False" />
                                <FilterShow Enabled="False" />
                                <FilterSet Enabled="False" />
                                <NoteShow Enabled="False" />
                                <Search Enabled="False" />
                                <Delete Enabled="False" />
                                <Refresh Enabled="False" />
                                <AdjustColumns Enabled="False" />
                            </Actions>
                        </ActionBar>
                        <AutoSize Enabled="True" Container="Parent" MinHeight="300" MinWidth="400" />
                    </px:PXGrid>
                </Template>
            </px:PXWizardPage>
        </Pages>
    </px:PXWizard>
</asp:Content>
