<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="FS306000.aspx.cs" Inherits="Page_FS306000" Title="Untitled Page" %>
    <%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

        <asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
            <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="BatchRecords" TypeName="PX.Objects.FS.PostDocBatchMaint"
            PageLoadBehavior="GoFirstRecord" SuspendUnloading="False">
                <CallbackCommands>
                    <px:PXDSCallbackCommand Name="CopyPaste" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="Save" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="Cancel" Visible="False"></px:PXDSCallbackCommand>
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
            <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="120px" Caption="Service Contract"
            DataMember="BatchRecords" TabIndex="1300" DefaultControlID="edBatchNbr">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM">
                    </px:PXLayoutRule>
                    <px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" DataSourceID="ds">
                    </px:PXSelector>
                    <px:PXSelector ID="edBillingCycleID" runat="server" DataField="BillingCycleID" DataSourceID="ds">
                    </px:PXSelector>
                    <px:PXDateTimeEdit ID="UpToDate" runat="server" DataField="UpToDate">
                    </px:PXDateTimeEdit>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM">
                    </px:PXLayoutRule>
                    <px:PXDateTimeEdit ID="edInvoiceDate" runat="server" DataField="InvoiceDate">
                    </px:PXDateTimeEdit>
                    <px:PXMaskEdit ID="edFinPeriodID" runat="server" DataField="FinPeriodID" DataSourceID="ds" Width="100px">
                    </px:PXMaskEdit>
                    <px:PXNumberEdit ID="edQtyDoc" runat="server" DataField="QtyDoc" TextAlign="Left">
                    </px:PXNumberEdit>
                </Template>
                <AutoSize Container="Window" Enabled="True" />
            </px:PXFormView>
        </asp:Content>
        <asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
            <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="130px" SkinID="Inquire" AdjustPageSize="Auto"
            TabIndex="2300" SyncPosition="True" KeepPosition="True">
                <Levels>
                    <px:PXGridLevel DataMember="BatchDetailsInfo">
                        <RowTemplate>
                            <px:PXTextEdit ID="edPostedTO" runat="server" DataField="PostedTO">
                            </px:PXTextEdit>
                            <px:PXTextEdit ID="edPostDocType" runat="server" DataField="PostDocType">
                            </px:PXTextEdit>
                            <px:PXTextEdit ID="edPostRefNbr" runat="server" DataField="PostRefNbr">
                            </px:PXTextEdit>
                            <px:PXSelector ID="edFSServiceOrder__SrvOrdType" runat="server" DataField="FSServiceOrder__SrvOrdType" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXSelector ID="edAppointmentID" runat="server" DataField="AppointmentID" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXSegmentMask ID="edFSServiceOrder__BillCustomerID" runat="server" AllowEdit="True" DataField="FSServiceOrder__BillCustomerID" Width="120px">
                            </px:PXSegmentMask>
                            <px:PXTextEdit ID="edCustomer__AcctName" runat="server" DataField="Customer__AcctName">
                            </px:PXTextEdit>
                            <px:PXSelector ID="edSOID" runat="server" DataField="SOID" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXDateTimeEdit ID="edFSAppointment__ActualDateTimeBegin_Date" runat="server" DataField="FSAppointment__ActualDateTimeBegin_Date">
                            </px:PXDateTimeEdit>
                            <px:PXDateTimeEdit ID="edFSAppointment__ActualDateTimeBegin_Time" runat="server" DataField="FSAppointment__ActualDateTimeBegin_Time">
                            </px:PXDateTimeEdit>
                            <px:PXDateTimeEdit ID="edFSAppointment__ActualDateTimeEnd_Time" runat="server" DataField="FSAppointment__ActualDateTimeEnd_Time">
                            </px:PXDateTimeEdit>
                            <px:PXSelector ID="edFSServiceOrder__BranchLocationID" runat="server" DataField="FSServiceOrder__BranchLocationID" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXSelector ID="edFSGeoZone__GeoZoneCD" runat="server" DataField="FSGeoZone__GeoZoneCD" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXTextEdit ID="edFSServiceOrder__DocDesc" runat="server" DataField="FSServiceOrder__DocDesc">
                            </px:PXTextEdit>
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="PostedTO">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostDocType">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostRefNbr" LinkCommand="OpenDocument">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="FSServiceOrder__SrvOrdType">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="AppointmentID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="FSServiceOrder__BillCustomerID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="Customer__AcctName">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="SOID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="FSAppointment__ActualDateTimeBegin_Date">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="FSAppointment__ActualDateTimeBegin_Time">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="FSAppointment__ActualDateTimeEnd_Time">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="FSServiceOrder__BranchLocationID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="FSGeoZone__GeoZoneCD">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="FSServiceOrder__DocDesc">
                            </px:PXGridColumn>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                <ActionBar ActionsText="False">
                </ActionBar>
            </px:PXGrid>
        </asp:Content>