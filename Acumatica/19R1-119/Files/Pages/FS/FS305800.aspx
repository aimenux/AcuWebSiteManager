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
                            <px:PXTextEdit ID="edMem_PostedIn" runat="server" DataField="Mem_PostedIn">
                            </px:PXTextEdit>
                            <px:PXTextEdit ID="edMem_DocType" runat="server" DataField="Mem_DocType">
                            </px:PXTextEdit>
                            <px:PXTextEdit ID="edMem_DocNbr" runat="server" DataField="Mem_DocNbr">
                            </px:PXTextEdit>
                            <px:PXSelector ID="edInvoiceRefNbr" runat="server" DataField="InvoiceRefNbr" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXSelector ID="edSrvOrdType" runat="server" DataField="SrvOrdType" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXSelector ID="edAppointmentID" runat="server" DataField="AppointmentID" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXSegmentMask ID="edBillCustomerID" runat="server" AllowEdit="True" DataField="BillCustomerID" Width="120px">
                            </px:PXSegmentMask>
                            <px:PXTextEdit ID="edAcctName" runat="server" DataField="Customer__AcctName">
                            </px:PXTextEdit>
                            <px:PXSelector ID="edSOID" runat="server" DataField="SOID" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXDateTimeEdit ID="edActualDateTimeBegin_Date" runat="server" DataField="ActualDateTimeBegin_Date">
                            </px:PXDateTimeEdit>
                            <px:PXDateTimeEdit ID="edActualDateTimeBegin_Time" runat="server" DataField="ActualDateTimeBegin_Time">
                            </px:PXDateTimeEdit>
                            <px:PXDateTimeEdit ID="edActualDateTimeEnd_Time" runat="server" DataField="ActualDateTimeEnd_Time">
                            </px:PXDateTimeEdit>
                            <px:PXSelector ID="edBranchLocationID" runat="server" DataField="BranchLocationID" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXSelector ID="edGeoZoneCD" runat="server" DataField="FSGeoZone__GeoZoneCD" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc">
                            </px:PXTextEdit>
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="Mem_PostedIn" Width="50px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="Mem_DocType" Width="50px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="Mem_DocNbr" Width="100px" LinkCommand="OpenDocument">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="InvoiceRefNbr" Width="100px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="SrvOrdType" Width="100px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="AppointmentID" Width="100px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="BillCustomerID" Width="100px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="AcctName" Width="200px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="SOID" Width="100px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="ActualDateTimeBegin_Date" Width="90px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="ActualDateTimeBegin_Time" Width="90px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="ActualDateTimeEnd_Time" Width="90px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="BranchLocationID" Width="120px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="GeoZoneCD" Width="120px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="DocDesc" Width="200px">
                            </px:PXGridColumn>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                <ActionBar ActionsText="False">
                </ActionBar>
            </px:PXGrid>
        </asp:Content>