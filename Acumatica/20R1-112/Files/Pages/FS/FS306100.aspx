<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="FS306100.aspx.cs" Inherits="Page_FS306100" Title="Untitled Page" %>
    <%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

        <asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
            <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="ContractBatchRecords" TypeName="PX.Objects.FS.ContractPostBatchMaint"
            PageLoadBehavior="GoFirstRecord" SuspendUnloading="False">
                <CallbackCommands>
                    <px:PXDSCallbackCommand Name="CopyPaste" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="Save" Visible="False"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="Cancel" Visible="True"></px:PXDSCallbackCommand>
                    <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="False" />
                    <px:PXDSCallbackCommand Name="Delete" PostData="Self" Visible="False" />
                    <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
                    <px:PXDSCallbackCommand Name="Last" PostData="Self" />
                    <px:PXDSCallbackCommand Name="OpenDocument" Visible="False" />
					<px:PXDSCallbackCommand Name="OpenContract" Visible="False" />
                </CallbackCommands>
            </px:PXDataSource>
        </asp:Content>
        <asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
            <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="120px" Caption="Service Contract"
            DataMember="ContractBatchRecords" TabIndex="1300" DefaultControlID="edBatchNbr">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM">
                    </px:PXLayoutRule>
                    <px:PXSelector ID="edContractPostBatchNbr" runat="server" DataField="ContractPostBatchNbr" DataSourceID="ds">
                    </px:PXSelector>
                    <px:PXDateTimeEdit ID="UpToDate" runat="server" DataField="UpToDate">
                    </px:PXDateTimeEdit>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM">
                    </px:PXLayoutRule>
                    <px:PXDateTimeEdit ID="edInvoiceDate" runat="server" DataField="InvoiceDate">
                    </px:PXDateTimeEdit>
                    <px:PXMaskEdit ID="edFinPeriodID" runat="server" DataField="FinPeriodID" DataSourceID="ds" Width="100px">
                    </px:PXMaskEdit>
                </Template>
                <AutoSize Container="Window" Enabled="True" />
            </px:PXFormView>
        </asp:Content>
        <asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
            <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="130px" SkinID="Inquire" AdjustPageSize="Auto"
            TabIndex="2300" SyncPosition="True" KeepPosition="True">
                <Levels>
                    <px:PXGridLevel DataMember="ContractPostDocRecords">
                        <RowTemplate>
                            <px:PXTextEdit ID="edPostDocType" runat="server" DataField="PostDocType">
                            </px:PXTextEdit>
                            <px:PXTextEdit ID="edPostRefNbr" runat="server" DataField="PostRefNbr">
                            </px:PXTextEdit>
                            <px:PXSelector ID="edRefNbr" runat="server" DataField="ContractRefNbr" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXTextEdit ID="edCustomerContractNbr" runat="server" DataField="CustomerContractNbr" AllowEdit="True">
                            </px:PXTextEdit>
							<px:PXSelector ID="edBillCustomerID" runat="server" DataField="BillCustomerID" AllowEdit="True">
                            </px:PXSelector>
							<px:PXTextEdit ID="edAcctName" runat="server" DataField="AcctName">
                            </px:PXTextEdit>
							<px:PXSelector ID="edBillLocationID" runat="server" DataField="BillLocationID" AllowEdit="True">
                            </px:PXSelector>
							<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate">
							</px:PXDateTimeEdit>
							<px:PXDateTimeEdit ID="edNextBillingInvoiceDate" runat="server" DataField="NextBillingInvoiceDate">
							</px:PXDateTimeEdit>
							<px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" AllowEdit="True">
                            </px:PXSelector>
							<px:PXSelector ID="edBranchLocationID" runat="server" DataField="BranchLocationID" AllowEdit="True">
                            </px:PXSelector>
                            <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc">
                            </px:PXTextEdit>
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="PostDocType">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PostRefNbr" LinkCommand="OpenDocument">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="ContractRefNbr" LinkCommand="OpenContract">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="CustomerContractNbr">
                            </px:PXGridColumn>
							<px:PXGridColumn DataField="BillCustomerID">
                            </px:PXGridColumn>
							<px:PXGridColumn DataField="AcctName">
                            </px:PXGridColumn>
							<px:PXGridColumn DataField="BillLocationID">
                            </px:PXGridColumn>
							<px:PXGridColumn DataField="StartDate">
                            </px:PXGridColumn>
							<px:PXGridColumn DataField="NextBillingInvoiceDate">
                            </px:PXGridColumn>
							<px:PXGridColumn DataField="BranchID">
                            </px:PXGridColumn>
							<px:PXGridColumn DataField="BranchLocationID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="DocDesc">
                            </px:PXGridColumn>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                <ActionBar ActionsText="False">
                </ActionBar>
            </px:PXGrid>
        </asp:Content>