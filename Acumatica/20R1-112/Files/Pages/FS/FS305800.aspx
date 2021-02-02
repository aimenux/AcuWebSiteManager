<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="FS305800.aspx.cs" Inherits="Page_FS305800" Title="Untitled Page" %>
    <%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

        <asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
            <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="BatchRecords" TypeName="PX.Objects.FS.PostBatchMaint"
            PageLoadBehavior="GoFirstRecord" SuspendUnloading="False">
                <CallbackCommands>
                    <px:PXDSCallbackCommand Name="CopyPaste" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="Save" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="Cancel" Visible="True"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="False" />
                    <px:PXDSCallbackCommand Name="Delete" PostData="Self" Visible="False" />
                    <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
                    <px:PXDSCallbackCommand Name="Last" PostData="Self" />
                    <px:PXDSCallbackCommand Name="OpenDocument" Visible="False">
                    </px:PXDSCallbackCommand>
                </CallbackCommands>
            </px:PXDataSource>
        </asp:Content>
        <asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
            <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Service Contract" DataMember="BatchRecords" TabIndex="1300" DefaultControlID="edBatchNbr" AllowCollapse="True">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM">
                    </px:PXLayoutRule>
                    <px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" DataSourceID="ds">
                    </px:PXSelector>
                    <px:PXSelector ID="edBillingCycleID" runat="server" DataField="BillingCycleID" DataSourceID="ds">
                    </px:PXSelector>
                    <px:PXDateTimeEdit ID="UpToDate" runat="server" DataField="UpToDate">
                    </px:PXDateTimeEdit>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM">
                    </px:PXLayoutRule>
                    <px:PXDateTimeEdit ID="edInvoiceDate" runat="server" DataField="InvoiceDate">
                    </px:PXDateTimeEdit>
                    <px:PXMaskEdit ID="edFinPeriodID" runat="server" DataField="FinPeriodID" DataSourceID="ds" Width="100px">
                    </px:PXMaskEdit>
                    <px:PXNumberEdit ID="edQtyDoc" runat="server" DataField="QtyDoc" TextAlign="Left">
                    </px:PXNumberEdit>
                </Template>
            </px:PXFormView>
        </asp:Content>
        <asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
            <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" SkinID="Inquire" AdjustPageSize="Auto"
            TabIndex="2300" SyncPosition="True" KeepPosition="True">
                <Levels>
                    <px:PXGridLevel DataMember="BatchDetailsInfo">
                        <RowTemplate>
                            <px:PXTextEdit ID="edPostTo" runat="server" DataField="PostTo">
                            </px:PXTextEdit>
                            <px:PXTextEdit ID="edCreatedDocType" runat="server" DataField="CreatedDocType">
                            </px:PXTextEdit>
                            <px:PXTextEdit ID="edCreatedRefNbr" runat="server" DataField="CreatedRefNbr">
                            </px:PXTextEdit>
                            <px:PXSelector ID="edInvoiceRefNbr" runat="server" DataField="PostingBatchDetail__InvoiceRefNbr" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXSelector ID="edSrvOrdType" runat="server" DataField="PostingBatchDetail__SrvOrdType" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXSelector ID="edAppointmentID" runat="server" DataField="PostingBatchDetail__AppointmentID" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXSegmentMask ID="edBillCustomerID" runat="server" AllowEdit="True" DataField="PostingBatchDetail__BillCustomerID" Width="120px">
                            </px:PXSegmentMask>
                            <px:PXTextEdit ID="edAcctName" runat="server" DataField="PostingBatchDetail__Customer__AcctName">
                            </px:PXTextEdit>
                            <px:PXSelector ID="edSOID" runat="server" DataField="PostingBatchDetail__SOID" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXDateTimeEdit ID="edActualDateTimeBegin_Date" runat="server" DataField="PostingBatchDetail__ActualDateTimeBegin_Date">
                            </px:PXDateTimeEdit>
                            <px:PXDateTimeEdit ID="edActualDateTimeBegin_Time" runat="server" DataField="PostingBatchDetail__ActualDateTimeBegin_Time">
                            </px:PXDateTimeEdit>
                            <px:PXDateTimeEdit ID="edActualDateTimeEnd_Time" runat="server" DataField="PostingBatchDetail__ActualDateTimeEnd_Time">
                            </px:PXDateTimeEdit>
                            <px:PXSelector ID="edBranchLocationID" runat="server" DataField="PostingBatchDetail__BranchLocationID" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXSelector ID="edGeoZoneCD" runat="server" DataField="PostingBatchDetail__FSGeoZone__GeoZoneCD" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXTextEdit ID="edDocDesc" runat="server" DataField="PostingBatchDetail__DocDesc">
                            </px:PXTextEdit>
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="PostTo">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="CreatedDocType">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="CreatedRefNbr" LinkCommand="OpenDocument">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostingBatchDetail__InvoiceRefNbr">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostingBatchDetail__SrvOrdType">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostingBatchDetail__AppointmentID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostingBatchDetail__BillCustomerID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostingBatchDetail__AcctName">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostingBatchDetail__SOID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostingBatchDetail__ActualDateTimeBegin_Date">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostingBatchDetail__ActualDateTimeBegin_Time">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostingBatchDetail__ActualDateTimeEnd_Time">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostingBatchDetail__BranchLocationID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostingBatchDetail__GeoZoneCD">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostingBatchDetail__DocDesc">
                            </px:PXGridColumn>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                <ActionBar ActionsText="False">
                </ActionBar>
            </px:PXGrid>
        </asp:Content>