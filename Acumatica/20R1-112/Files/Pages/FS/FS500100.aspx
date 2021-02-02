<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS500100.aspx.cs" Inherits="Page_FS500100" Title="Untitled Page" %>
    <%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

        <asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
            <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FS.CreateInvoiceByAppointmentPost"
            SuspendUnloading="False" PageLoadBehavior="InsertRecord">
                <CallbackCommands>
                    <px:PXDSCallbackCommand Name="FixServiceOrdersWithoutBillingSettings" CommitChanges="True"/>
                    <px:PXDSCallbackCommand DependOnGrid="gridAppointments" Name="PostLines_ViewDetails" Visible="False" RepaintControls="All"
                    />
                    <px:PXDSCallbackCommand Name="ViewPostBatch" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False">
                    </px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="OpenReviewTemporaryBatch" VisibleOnProcessingResults="True"/>
                </CallbackCommands>
            </px:PXDataSource>
        </asp:Content>
        <asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
            <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter"
            TabIndex="700" DefaultControlID="edPostTo" AllowCollapse="True">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM">
                    </px:PXLayoutRule>
                    <px:PXLayoutRule runat="server" GroupCaption="Filtering Options" StartGroup="True">
                    </px:PXLayoutRule>
                    <px:PXDropDown ID="edPostTo" runat="server" DataField="PostTo" Size="XM" CommitChanges="True">
                    </px:PXDropDown>
                    <px:PXSelector ID="edBillingCycleID" runat="server" CommitChanges="True" DataField="BillingCycleID" Size="XM">
                    </px:PXSelector>
                    <px:PXSegmentMask runat="server" DataField="CustomerID" DataSourceID="ds" AutoRefresh="True" ID="edCustomerID" CommitChanges="True"
                    Size="XM">
                    </px:PXSegmentMask>
                    <px:PXDateTimeEdit runat="server" DataField="UpToDate" ID="edUpToDate" CommitChanges="True">
                    </px:PXDateTimeEdit>
                    <px:PXCheckBox ID="edIgnoreBillingCycles" runat="server" DataField="IgnoreBillingCycles" CommitChanges="True">
                    </px:PXCheckBox>
                    <px:PXLayoutRule runat="server" StartColumn="True">
                    </px:PXLayoutRule>
                    <px:PXLayoutRule runat="server" GroupCaption="Billing Options" StartGroup="True" LabelsWidth="SM" ControlSize="XM">
                    </px:PXLayoutRule>
                    <px:PXDateTimeEdit runat="server" DataField="InvoiceDate" ID="edInvoiceDate" CommitChanges="True">
                    </px:PXDateTimeEdit>
                    <px:PXSelector ID="edInvoiceFinPeriodID" runat="server" CommitChanges="True" DataField="InvoiceFinPeriodID" Width="100px">
                    </px:PXSelector>
                    <px:PXLayoutRule runat="server" GroupCaption="Billing Actions" StartGroup="True">
                    </px:PXLayoutRule>
                    <px:PXCheckBox ID="edPrepareInvoice" runat="server" DataField="PrepareInvoice" CommitChanges="True" AlignLeft="True">
                    </px:PXCheckBox>
                    <px:PXCheckBox ID="edReleaseInvoice" runat="server" DataField="ReleaseInvoice" CommitChanges="True" AlignLeft="True">
                    </px:PXCheckBox>
                    <px:PXCheckBox ID="edEmailInvoice" runat="server" DataField="EmailInvoice" CommitChanges="True" AlignLeft="True">
                    </px:PXCheckBox>
                    <px:PXCheckBox ID="edEmailSalesOrder" runat="server" DataField="EmailSalesOrder" CommitChanges="True" AlignLeft="True">
                    </px:PXCheckBox>
                    <px:PXCheckBox ID="edSOQuickProcess" runat="server" DataField="SOQuickProcess" CommitChanges="True" AlignLeft="True">
                    </px:PXCheckBox>
                    <px:PXLayoutRule runat="server" GroupCaption="Bill Actions" StartGroup="True">
                    </px:PXLayoutRule>
                    <px:PXCheckBox ID="edReleaseBill" runat="server" DataField="ReleaseBill" CommitChanges="True" AlignLeft="True">
                    </px:PXCheckBox>
                    <px:PXCheckBox ID="edPayBill" runat="server" DataField="PayBill" CommitChanges="True" AlignLeft="True">
                    </px:PXCheckBox>
                </Template>
            </px:PXFormView>
        </asp:Content>
        <asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
            <px:PXGrid ID="gridAppointments" runat="server"
                Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="true" 
                    AdjustPageSize="Auto" DataSourceID="ds" SkinID="PrimaryInquire" SyncPosition="True"
                        AutoSize="True" FastFilterFields="AppointmentID, BillCustomerID, BillLocationID,SOID,DocDesc"
                        BatchUpdate="True">
                <Levels>
                    <px:PXGridLevel DataMember="PostLines">
                        <RowTemplate>
                            <px:PXCheckBox ID="edSelected" runat="server" DataField="Selected" Text="Selected">
                            </px:PXCheckBox>
                            <px:PXSelector ID="edSrvOrdType" runat="server" DataField="SrvOrdType" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXSegmentMask ID="edBillCustomerID" runat="server" DataField="BillCustomerID" AllowEdit="True" DisplayMode="Hint">
                            </px:PXSegmentMask>
                            <px:PXSegmentMask ID="edBillLocationID" runat="server" DataField="BillLocationID" AllowEdit="True">
                            </px:PXSegmentMask>
                            <px:PXSelector ID="edBillingCycleCD" runat="server" DataField="BillingCycleCD" Size="XM" AllowEdit="True">
                    		</px:PXSelector>
                            <px:PXDateTimeEdit ID="edCutOffDate_Date" runat="server" DataField="CutOffDate_Date">
                            </px:PXDateTimeEdit>
                            <px:PXSelector ID="edSORefNbr" runat="server" DataField="SORefNbr" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXDateTimeEdit ID="edActualDateTimeBegin_Date" runat="server" DataField="ActualDateTimeBegin_Date">
                            </px:PXDateTimeEdit>
                            <px:PXDateTimeEdit ID="edActualDateTimeBegin_Time" runat="server" DataField="ActualDateTimeBegin_Time">
                            </px:PXDateTimeEdit>
                            <px:PXDateTimeEdit ID="edActualDateTimeEnd_Time" runat="server" DataField="ActualDateTimeEnd_Time">
                            </px:PXDateTimeEdit>
                            <px:PXSelector ID="edBranchLocationID" runat="server" DataField="BranchLocationID" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXDropDown ID="edStatus" runat="server" DataField="Status">
                            </px:PXDropDown>
                            <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc">
                            </px:PXTextEdit>
                            <px:PXTextEdit ID="edBatchID" runat="server" DataField="BatchID" AllowEdit="True">
                            </px:PXTextEdit>
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="SrvOrdType">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="RefNbr">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="BillCustomerID" DisplayMode="Hint">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="BillLocationID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="BillingCycleCD">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="CutOffDate">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="SORefNbr">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="ActualDateTimeBegin_Date">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="ActualDateTimeBegin_Time">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="ActualDateTimeEnd_Time">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="BranchLocationID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="Status">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="DocDesc">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="BatchID" LinkCommand="ViewPostBatch">
                            </px:PXGridColumn>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <Mode AllowAddNew="False" AllowDelete="False" />
                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
            </px:PXGrid>
        </asp:Content>

