<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="FS501300.aspx.cs" Title="Untitled Page" Inherits="Page_FS501300" %>
    <%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

        <asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
            <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FS.CreateInvoiceByContractPost">
            </px:PXDataSource>
        </asp:Content>
        <asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
            <px:PXFormView ID="form" runat="server" DataSourceID="ds" 
                Style="z-index: 100" Width="100%" DataMember="Filter" TabIndex="700" 
                DefaultControlID="">
                <Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM"/>
					<px:PXLayoutRule runat="server" GroupCaption="Filtering Options" StartGroup="True" />
					<px:PXSelector ID="edCustomerID" runat="server" CommitChanges="True" DataField="CustomerID"/>
					<px:PXSelector ID="edServiceContractID" runat="server" CommitChanges="True" DataField="ServiceContractID" AutoRefresh="True"/>
					<px:PXDateTimeEdit ID="edUpToDate" runat="server" DataField="UpToDate" CommitChanges="True"/>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXLayoutRule runat="server" GroupCaption="Billing Options" StartGroup="True" />
					<px:PXDateTimeEdit runat="server" DataField="InvoiceDate" ID="edInvoiceDate" CommitChanges="True"/>
                    <px:PXSelector ID="edInvoiceFinPeriodID" runat="server" CommitChanges="True" DataField="InvoiceFinPeriodID" Width="100px"/>
                </Template>
            </px:PXFormView>
        </asp:Content>
        <asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
            <px:PXGrid ID="gridContracts" runat="server" AllowPaging="True" DataSourceID="ds" 
                Style="z-index: 100" Width="100%"
                SkinID="PrimaryInquire" TabIndex="500" SyncPosition="True" BatchUpdate="True">
                <Levels>
                    <px:PXGridLevel DataMember="Contracts" DataKeyNames="ServiceContractID,ContractPeriodID">
					<RowTemplate>
                            <px:PXCheckBox ID="edSelected" runat="server" DataField="Selected" Text="Selected"/>
                            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AllowEdit="True"/>
                            <px:PXTextEdit ID="edCustomerContractNbr" runat="server" DataField="CustomerContractNbr"/>
                            <px:PXSegmentMask ID="edBillCustomerID" runat="server" DataField="BillCustomerID" AllowEdit="True"/>
                            <px:PXSegmentMask ID="edBillLocationID" runat="server" DataField="BillLocationID" AllowEdit="True"/>
							<px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" AllowEdit="True"/>
                            <px:PXSelector ID="edBranchLocationID" runat="server" DataField="BranchLocationID" AllowEdit="True"/>
                            <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc"/>
							<px:PXDropDown ID="edStatus" runat="server" DataField="Status"/>
							<px:PXSelector ID="edContractPostBatchID" runat="server" DataField="ContractPostBatchID" AllowEdit="True"/>
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox"/>
							<px:PXGridColumn DataField="RefNbr"/>
                            <px:PXGridColumn DataField="CustomerContractNbr"/>
							<px:PXGridColumn DataField="BillCustomerID" DisplayMode="Hint"/>
							<px:PXGridColumn DataField="BillLocationID" />
							<px:PXGridColumn DataField="BillingPeriod"/>
							<px:PXGridColumn DataField="BranchID"/>
							<px:PXGridColumn DataField="BranchLocationID"/>
							<px:PXGridColumn DataField="DocDesc"/>
							<px:PXGridColumn DataField="Status"/>
							<px:PXGridColumn DataField="ContractPostBatchID"/>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                <ActionBar DefaultAction="viewDocument"></ActionBar>
            </px:PXGrid>
        </asp:Content>