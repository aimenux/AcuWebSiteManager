<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="FS305900.aspx.cs" Inherits="Page_FS305900" Title="Untitled Page" %>
    <%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

        <asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
            <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="BatchRecords" TypeName="PX.Objects.FS.InventoryPostBatchMaint"
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
            <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Service Contract"
            DataMember="BatchRecords" TabIndex="1300" DefaultControlID="edBatchNbr" AllowCollapse="True">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM">
                    </px:PXLayoutRule>
                    <px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" DataSourceID="ds">
                    </px:PXSelector>
                    <px:PXDateTimeEdit ID="edCutOffDate" runat="server" DataField="CutOffDate">
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
            </px:PXFormView>
        </asp:Content>
        <asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
            <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="130px" SkinID="Inquire" AdjustPageSize="Auto"
            TabIndex="2300" SyncPosition="True" KeepPosition="True">
                <Levels>
                    <px:PXGridLevel DataMember="BatchDetailsInfo">
                        <RowTemplate>
                            <px:PXTextEdit ID="edMem_DocType" runat="server" DataField="Mem_DocType">
                            </px:PXTextEdit>
                            <px:PXTextEdit ID="edMem_DocNbr" runat="server" DataField="Mem_DocNbr">
                            </px:PXTextEdit>
                            <px:PXSelector ID="edSODetID" runat="server" DataField="SODetID">
                            </px:PXSelector>
                            <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True">
                            </px:PXSegmentMask>
                            <px:PXSelector ID="edSrvOrdType" runat="server" DataField="SrvOrdType" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXSelector ID="edAppointmentID" runat="server" DataField="AppointmentID" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXSegmentMask ID="edBillCustomerID" runat="server" AllowEdit="True" DataField="BillCustomerID" Width="120px">
                            </px:PXSegmentMask>
                            <px:PXTextEdit ID="edAcctName" runat="server" DataField="AcctName">
                            </px:PXTextEdit>
                            <px:PXSelector ID="edSOID" runat="server" DataField="SOID" AllowEdit="True">
                            </px:PXSelector>
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="Mem_DocType">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="Mem_DocNbr" LinkCommand="OpenDocument">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="SODetID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="InventoryID">
                            </px:PXGridColumn>                            
                            <px:PXGridColumn DataField="SrvOrdType">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="AppointmentID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="BillCustomerID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="AcctName">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="SOID">
                            </px:PXGridColumn>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                <ActionBar ActionsText="False">
                </ActionBar>
            </px:PXGrid>
        </asp:Content>