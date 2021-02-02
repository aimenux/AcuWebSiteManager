<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX303000.aspx.cs" Inherits="Page_TX303000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.TX.TXInvoiceEntry" PrimaryView="Document">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="Release" CommitChanges="true" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="Action" CommitChanges="true" Visible="False" />
            <px:PXDSCallbackCommand Name="Inquiry" CommitChanges="true" Visible="False" />
            <px:PXDSCallbackCommand Name="Report" CommitChanges="true" Visible="False" />
            <px:PXDSCallbackCommand Visible="false" Name="ReverseInvoice" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="VendorRefund" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="VoidDocument" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="PayInvoice" />
            <px:PXDSCallbackCommand Visible="False" Name="ViewSchedule" CommitChanges="true" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Visible="false" Name="CreateSchedule" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewBatch" />
            <px:PXDSCallbackCommand Visible="false" Name="NewVendor" />
            <px:PXDSCallbackCommand Visible="false" Name="EditVendor" />
            <px:PXDSCallbackCommand Visible="false" Name="ReclassifyBatch" />
            <px:PXDSCallbackCommand Visible="false" Name="VendorDocuments" />
            <px:PXDSCallbackCommand Visible="false" Name="AddPOReceipt2" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="AddReceiptLine2" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="AddPOOrder2" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="AddPOReceipt" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="AddReceiptLine" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="AddPOOrder" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPOOrderLine" />
			<px:PXDSCallbackCommand Visible="false" Name="AddLandedCost" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddLandedCost2" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="LinkLine" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" Name="ViewPODocument" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Visible="false" Name="AutoApply" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="ViewPayment" DependOnGrid="detgrid" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
            <px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LsLCSplits" DependOnGrid="gridLCTran" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="AddInvoices" />
            <px:PXDSCallbackCommand Visible="false" CommitChanges="true" Name="AddInvoicesOK" />
            <px:PXDSCallbackCommand Visible="false" Name="RecalculateDiscountsAction" />
            <px:PXDSCallbackCommand Visible="false" Name="RecalcOk" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Document" Caption="Document Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity"
        LinkIndicator="true" NotifyIndicator="true" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" DefaultControlID="edDocType">
        <CallbackCommands>
            <Save PostData="Self" />
        </CallbackCommands>
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="s" ControlSize="s" />
            <px:PXDropDown ID="edDocType" runat="server" DataField="DocType" />
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
            <px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edDocDate" runat="server" DataField="DocDate" />
            <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
            <px:PXTextEdit CommitChanges="True" ID="edInvoiceNbr" runat="server" DataField="InvoiceNbr" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="s" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AllowAddNew="True" AllowEdit="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID" />
            <pxa:PXCurrencyRate ID="edCury" DataField="CuryID" runat="server" DataSourceID="ds" RateTypeView="_APInvoice_CurrencyInfo_" DataMember="_Currency_" />
            <px:PXSelector CommitChanges="True" ID="edTermsID" runat="server" DataField="TermsID" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edDueDate" runat="server" DataField="DueDate" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edDiscDate" runat="server" DataField="DiscDate" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" Width="100%" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="s" ControlSize="s" />
            <px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
            <px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" />
            <px:PXNumberEdit ID="edCuryTaxAmt" runat="server" CommitChanges="True" DataField="CuryTaxAmt" />
            <px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDiscAmt" runat="server" DataField="CuryOrigDiscAmt" />
        </Template>
    </px:PXFormView>


    <style type="text/css">
        .leftDocTemplateCol {
            width: 50%;
            float: left;
            min-width: 90px;
        }

        .rightDocTemplateCol {
            min-width: 90px;
        }
    </style>
    <px:PXGrid ID="docsTemplate" runat="server" Visible="false">
        <Levels>
            <px:PXGridLevel>
                <Columns>
                    <px:PXGridColumn Key="Template">
                        <CellTemplate>
                            <div class="leftDocTemplateCol">
                                <div class="Field0"><%# ((PXGridCellContainer)Container).Text("refNbr") %></div>
                                <div class="Field1"><%# ((PXGridCellContainer)Container).Text("docDate") %></div>
                            </div>
                            <div class="rightDocTemplateCol">
                                <span class="Field1"><%# ((PXGridCellContainer)Container).Text("curyOrigDocAmt") %></span>
                                <span class="Field1"><%# ((PXGridCellContainer)Container).Text("curyID") %></span>
                                <div class="Field1"><%# ((PXGridCellContainer)Container).Text("status") %></div>

                            </div>
                            <div class="Field1"><%# ((PXGridCellContainer)Container).Text("vendorID_Vendor_acctName") %></div>
                        </CellTemplate>
                    </px:PXGridColumn>
                </Columns>
            </px:PXGridLevel>
        </Levels>
    </px:PXGrid>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="300px" Width="100%">
        <Items>
            <px:PXTabItem Text="Apply Tax To">
                <Template>
                    <px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="true" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Add Documents">
                                    <AutoCallBack Command="AddInvoices" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Taxes">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXDropDown ID="edOrigTranType" runat="server" DataField="OrigTranType" CommitChanges="true" />
                                    <px:PXSelector ID="edOrigRefNbr" runat="server" DataField="OrigRefNbr" AutoRefresh="True" AllowEdit="true" />
                                    <px:PXSelector CommitChanges="True" ID="edTaxID" runat="server" DataField="TaxID" AutoRefresh="true" />
                                    <px:PXNumberEdit ID="edTaxRate" runat="server" DataField="TaxRate" Enabled="False" />
                                    <px:PXNumberEdit ID="edTaxableAmt" runat="server" DataField="TaxableAmt" />
                                    <px:PXNumberEdit ID="edTaxAmt" runat="server" DataField="TaxAmt" />
                                    <px:PXSelector CommitChanges="True" ID="edTaxZoneID" runat="server" DataField="TaxZoneID" />
                                    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" CommitChanges="true" />
                                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" AutoRefresh="true" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="OrigTranType" />
                                    <px:PXGridColumn DataField="OrigRefNbr" />
                                    <px:PXGridColumn DataField="TaxID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="TaxRate" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryTaxableAmt" AutoCallBack="true" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="TaxZoneID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="AccountID" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="SubID" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowFormEdit="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Financial Details">
                <Template>
                    <px:PXFormView ID="form2" runat="server" Style="z-index: 100" Width="100%" DataMember="CurrentDocument" CaptionVisible="False" SkinID="Transparent" DataSourceID="ds" TabIndex="18500">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Link to GL" StartGroup="True" />
                            <px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" DataSourceID="ds" />
                            <px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" DataSourceID="ds" />
                            <px:PXSegmentMask ID="edAPAccountID" runat="server" DataField="APAccountID" CommitChanges="True" DataSourceID="ds" />
                            <px:PXSegmentMask ID="edAPSubID" runat="server" DataField="APSubID" AutoRefresh="True" DataSourceID="ds" />
                            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" GroupCaption="Default Payment Info" StartGroup="True" />
                            <px:PXCheckBox ID="chkSeparateCheck" runat="server" DataField="SeparateCheck" />
                            <px:PXCheckBox CommitChanges="True" ID="chkPaySel" runat="server" DataField="PaySel" />
                            <px:PXDateTimeEdit ID="edPayDate" runat="server" DataField="PayDate" />
                            <px:PXSegmentMask CommitChanges="True" ID="edPayLocationID" runat="server" AutoRefresh="True" DataField="PayLocationID" DataSourceID="ds" />
                            <px:PXSelector CommitChanges="True" ID="edPayTypeID" runat="server" DataField="PayTypeID" DataSourceID="ds" />
                            <px:PXSegmentMask CommitChanges="True" ID="edPayAccountID" runat="server" DataField="PayAccountID" DataSourceID="ds" />
                        </Template>
                        <AutoSize Enabled="True" />
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Applications">
                <Template>
                    <px:PXGrid ID="detgrid" runat="server" Style="z-index: 100;" Width="100%" Height="300px" SkinID="DetailsInTab">
                        <ActionBar DefaultAction="ViewPayment">
                            <CustomItems>
                                <px:PXToolBarButton Text="Auto Apply">
                                    <AutoCallBack Command="AutoApply" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Adjustments">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXNumberEdit CommitChanges="True" ID="edCuryAdjdAmt" runat="server" DataField="CuryAdjdAmt" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="AdjgDocType" Type="DropDownList" />
                                    <px:PXGridColumn DataField="AdjgRefNbr" AutoCallBack="True" LinkCommand="ViewPayment" />
                                    <px:PXGridColumn DataField="CuryAdjdAmt" AutoCallBack="True" TextAlign="Right" />
                                    <px:PXGridColumn DataField="APPayment__DocDate" />
                                    <px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" />
                                    <px:PXGridColumn DataField="APPayment__DocDesc" />
                                    <px:PXGridColumn DataField="APPayment__CuryID" />
                                    <px:PXGridColumn DataField="APPayment__FinPeriodID" />
                                    <px:PXGridColumn DataField="APPayment__ExtRefNbr" />
                                    <px:PXGridColumn DataField="AdjdDocType" />
                                    <px:PXGridColumn DataField="AdjdRefNbr" />
                                    <px:PXGridColumn DataField="APPayment__Status" Label="Status" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <CallbackCommands>
            <Search CommitChanges="True" PostData="Page" />
            <Refresh CommitChanges="True" PostData="Page" />
        </CallbackCommands>
        <AutoSize Container="Window" Enabled="True" MinHeight="180" />
    </px:PXTab>
    <px:PXSmartPanel ID="pnlAddInvoice" runat="server" Caption="Add Documents" DesignView="Content" LoadOnDemand="True" ShowAfterLoad="True" CaptionVisible="True" Key="DocumentList" Height="500px" Width="800px">
        <px:PXFormView ID="form1" runat="server" DataSourceID="ds" Width="100%" DataMember="BillFilter" DefaultControlID="edTaxID" SkinID="Transparent" CaptionVisible="False" TabIndex="19500">
            <Template>
                <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
                <px:PXSelector ID="edTaxID" runat="server" CommitChanges="True" DataField="TaxID"/>
                <px:PXSegmentMask ID="edVendorID" runat="server" AllowAddNew="True" AllowEdit="True" CommitChanges="True" DataField="VendorID"/>
                <px:PXTextEdit ID="edInvoiceNbr" runat="server" CommitChanges="True" DataField="InvoiceNbr" Size="S" />
                <px:PXLayoutRule runat="server" ControlSize="S" LabelsWidth="S" StartColumn="True" />
                <px:PXDateTimeEdit ID="edStartDate" runat="server" CommitChanges="True" DataField="StartDate" />
                <px:PXDateTimeEdit ID="edEndDate" runat="server" CommitChanges="True" DataField="EndDate" />
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="grid4" runat="server" Width="100%" Height="200px" DataSourceID="ds" BatchUpdate="True" SkinID="Inquire" TabIndex="19900" AdjustPageSize="Auto" AllowPaging="True"
            FastFilterFields="RefNbr,VendorID,InvoiceNbr">
            <Mode AllowAddNew="False" AllowDelete="False" />
            <Levels>
                <px:PXGridLevel DataKeyNames="DocType,RefNbr" DataMember="DocumentList">
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="DocType" />
                        <px:PXGridColumn DataField="RefNbr" />
                        <px:PXGridColumn DataField="InvoiceNbr" />
                        <px:PXGridColumn DataField="DocDate" />
                        <px:PXGridColumn DataField="VendorID" />
                        <px:PXGridColumn DataField="VendorID_description" />
                        <px:PXGridColumn DataField="VendorLocationID" />
                        <px:PXGridColumn DataField="CuryID" />
                        <px:PXGridColumn DataField="CuryOrigDocAmt" />
                        <px:PXGridColumn DataField="CuryDocBal" />
                        <px:PXGridColumn DataField="DueDate" />
                        <px:PXGridColumn DataField="DocDesc" />
                    </Columns>
                    <Layout FormViewHeight="" />
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton4" runat="server" CommandName="AddInvoicesOK" CommandSourceID="ds" Text="Add" SyncVisible="false" />
            <px:PXButton ID="OK" runat="server" DialogResult="OK" Text="Add &amp; Close" />
            <px:PXButton ID="Cancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
