<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false"
    CodeFile="CL401000.aspx.cs" Inherits="Page_CL401000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Documents"
        TypeName="PX.Objects.CN.Compliance.CL.Graphs.ComplianceDocumentEntry">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="false" Name="ComplianceDocument$PurchaseOrder$Link"
                DependOnGrid="grid" CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="false" Name="ComplianceDocument$Subcontract$Link"
                DependOnGrid="grid" CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="false" Name="ComplianceDocument$InvoiceID$Link" DependOnGrid="grid"
                CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="false" Name="ComplianceDocument$BillID$Link" DependOnGrid="grid"
                CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="false" Name="ComplianceDocument$ApCheckID$Link" DependOnGrid="grid"
                CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="false" Name="ComplianceDocument$ArPaymentID$Link" DependOnGrid="grid"
                CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="false" Name="ComplianceDocument$ProjectTransactionID$Link"
                DependOnGrid="grid" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server"
        Height="400px" AllowPaging="True" ActionsPosition="Top" AdjustPageSize="Auto"
        AllowSearch="True" SkinID="Primary" DataSourceID="ds" SyncPosition="True" KeepPosition="True"
        PageSize="50" FilesIndicator="True" BlankFilterHeader="All Records" Width="100%"
        AutoGenerateColumns="Append" TabIndex="8900" TemporaryFilterCaption="Filter Applied">
        <Levels>
            <px:PXGridLevel DataKeyNames="ComplianceDocumentID" DataMember="Documents">
                <RowTemplate>
                    <px:PXDateTimeEdit ID="edCreationDate" runat="server" AlreadyLocalized="False"
                        DataField="CreationDate" DefaultLocale=""/>
                    <px:PXSelector ID="edDocumentTypeValue" runat="server" DataField="DocumentTypeValue"
                        CommitChanges="True" AutoRefresh="True"/>
                    <px:PXSelector ID="edDocumentType" runat="server" DataField="DocumentType"
                        CommitChanges="True" AutoRefresh="True"/>
                    <px:PXSelector ID="edStatus" runat="server" DataField="Status"/>
                    <px:PXSelector ID="edBillID" runat="server" DataField="BillID" CommitChanges="True"
                        AutoRefresh="True" FilterByAllFields="True"/>
                    <px:PXNumberEdit ID="edBillAmount" runat="server" AlreadyLocalized="False" DataField="BillAmount"/>
                    <px:PXSelector ID="edInvoiceID" runat="server" DataField="InvoiceID" CommitChanges="True"
                        AutoRefresh="True" FilterByAllFields="True"/>
                    <px:PXNumberEdit ID="edInvoiceAmount" runat="server" AlreadyLocalized="False"
                        DataField="InvoiceAmount"/>
                    <px:PXCheckBox ID="edRequired" runat="server" DataField="Required" Text="Is this Required"
                        AlreadyLocalized="False"/>
                    <px:PXCheckBox ID="edReceived" runat="server" AlreadyLocalized="False" DataField="Received"
                        Text="Received"/>
                    <px:PXDateTimeEdit ID="edReceivedDate" runat="server" DataField="ReceivedDate"
                        AlreadyLocalized="False"/>
                    <px:PXCheckBox ID="ChkIsReceivedFromJointVendor" runat="server" AlreadyLocalized="False"
                        DataField="IsReceivedFromJointVendor" />
                    <px:PXDateTimeEdit ID="edJointReceivedDate" runat="server" AlreadyLocalized="False"
                        DataField="JointReceivedDate"/>
                    <px:PXCheckBox ID="CkhIsProcessed" runat="server" AlreadyLocalized="False"
                        DataField="IsProcessed" />
                    <px:PXCheckBox ID="ChkIsVoided" runat="server" AlreadyLocalized="False"
                        DataField="IsVoided" />
                    <px:PXCheckBox ID="ChkIsCreatedAutomatically" runat="server" AlreadyLocalized="False"
                        DataField="IsCreatedAutomatically" />
                    <px:PXDateTimeEdit ID="edSentDate" runat="server" AlreadyLocalized="False" DataField="SentDate"/>
                    <px:PXDateTimeEdit ID="edEffectiveDate" runat="server" AlreadyLocalized="False"
                        DataField="EffectiveDate"/>
                    <px:PXDateTimeEdit ID="edExpirationDate" runat="server" DataField="ExpirationDate"
                        AlreadyLocalized="False"/>
                    <px:PXNumberEdit ID="edLimit" runat="server" AlreadyLocalized="False" DataField="Limit"/>
                    <px:PXTextEdit ID="edMethodSent" runat="server" AlreadyLocalized="False" DataField="MethodSent"/>
                    <px:PXSelector runat="server" DataField="CostTaskID" FilterByAllFields="True"
                        AutoRefresh="True" ID="edCostTaskID" />
                    <px:PXSelector runat="server" DataField="RevenueTaskID" FilterByAllFields="True"
                        AutoRefresh="True" ID="edRevenueTaskID" />
                    <px:PXSegmentMask ID="edCostCode2" runat="server" DataField="CostCodeID" AutoRefresh="True"/>
                    <px:PXSelector ID="edCustomerID" runat="server" DataField="CustomerID"/>
                    <px:PXTextEdit ID="edCustomerName" runat="server" AlreadyLocalized="False"
                        DataField="CustomerName"/>
                    <px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID"/>
                    <px:PXTextEdit ID="edVendorName" runat="server" AlreadyLocalized="False" DataField="VendorName"/>
                    <px:PXSegmentMask ID="edSecondaryVendorID" runat="server" DataField="SecondaryVendorID"/>
                    <px:PXTextEdit ID="edSecondaryVendorName" runat="server" AlreadyLocalized="False"
                        DataField="SecondaryVendorName"/>
                    <px:PXSelector ID="edPurchaseOrder" runat="server" DataField="PurchaseOrder"
                        CommitChanges="True" AutoRefresh="True" FilterByAllFields="True"/>
                    <px:PXSelector ID="edPurchaseOrderLineItem" runat="server" AlreadyLocalized="False"
                        DataField="PurchaseOrderLineItem" AutoRefresh="True"/>
                    <px:PXSelector ID="edSubcontract" runat="server" DataField="Subcontract"
                        CommitChanges="True" AutoRefresh="True" FilterByAllFields="True"/>
                    <px:PXSelector ID="edSubcontractLineItem" runat="server" AlreadyLocalized="False"
                        DataField="SubcontractLineItem" AutoRefresh="True"/>
                    <px:PXSelector ID="edChangeOrderNumber" runat="server" DataField="ChangeOrderNumber"
                        CommitChanges="True" AutoRefresh="True" FilterByAllFields="True"/>
                    <px:PXNumberEdit ID="edLienWaiverAmount" runat="server" AlreadyLocalized="False"
                        DataField="LienWaiverAmount"/>
                    <px:PXNumberEdit ID="edLienNoticeAmount" runat="server" AlreadyLocalized="False"
                        DataField="LienNoticeAmount"/>
                    <px:PXTextEdit ID="edSponsorOrganization" runat="server" AlreadyLocalized="False"
                        DataField="SponsorOrganization"/>
                    <px:PXTextEdit ID="edCertificateNumber" runat="server" AlreadyLocalized="False"
                        DataField="CertificateNumber"/>
                    <px:PXTextEdit ID="edInsuranceCompany" runat="server" AlreadyLocalized="False"
                        DataField="InsuranceCompany"/>
                    <px:PXTextEdit ID="edPolicy" runat="server" AlreadyLocalized="False" DataField="Policy"/>
                    <px:PXSelector ID="edApCheckID" runat="server" DataField="ApCheckID" CommitChanges="True"
                        AutoRefresh="True" FilterByAllFields="True"/>
                    <px:PXTextEdit ID="edCheckNumber" runat="server" DataField="CheckNumber"/>
                    <px:PXSelector ID="edArPaymentID" runat="server" DataField="ArPaymentID" CommitChanges="True"
                        AutoRefresh="True" FilterByAllFields="True"/>
                    <px:PXSelector ID="edProjectTransactionID" runat="server" DataField="ProjectTransactionID"
                        CommitChanges="True" AutoRefresh="True" FilterByAllFields="True"/>
                    <px:PXDateTimeEdit ID="edReceiptDate" runat="server" DataField="ReceiptDate"
                        AlreadyLocalized="False"/>
                    <px:PXDateTimeEdit ID="edDateIssued" runat="server" AlreadyLocalized="False"
                        DataField="DateIssued"/>
                    <px:PXDateTimeEdit ID="edThroughDate" runat="server" AlreadyLocalized="False"
                        DataField="ThroughDate"/>
                    <px:PXDateTimeEdit ID="edReceiveDate" runat="server" AlreadyLocalized="False"
                        DataField="ReceiveDate"/>
                    <px:PXTextEdit ID="edReceivedBy" runat="server" DataField="ReceivedBy" AlreadyLocalized="False"/>
                    <px:PXDropDown ID="edSourceType" runat="server" DataField="SourceType"/>
                    <px:PXCheckBox ID="edIsRequiredJointCheck" runat="server" DataField="IsRequiredJointCheck"
                        Text="Is this Required" AlreadyLocalized="False"/>
                    <px:PXSegmentMask ID="edJointVendorInternalId" runat="server" DataField="JointVendorInternalId"/>
                    <px:PXTextEdit ID="edJointVendorExternalName" runat="server" DataField="JointVendorExternalName"/>
                    <px:PXNumberEdit ID="edJointAmount" runat="server" AlreadyLocalized="False"
                        DataField="JointAmount"/>
                    <px:PXNumberEdit ID="edJointLienWaiverAmount" runat="server" AlreadyLocalized="False"
                        DataField="JointLienWaiverAmount"/>
                    <px:PXNumberEdit ID="edJointLienNoticeAmount" runat="server" AlreadyLocalized="False"
                        DataField="JointLienNoticeAmount"/>
                    <px:PXTextEdit ID="edJointRelease" runat="server" AlreadyLocalized="False"
                        DataField="JointRelease"/>
                    <px:PXDateTimeEdit ID="edPaymentDate" runat="server" AlreadyLocalized="False"
                        DataField="PaymentDate"/>
                    <px:PXCheckBox ID="edJointReleaseReceived" runat="server" AlreadyLocalized="False"
                        DataField="JointReleaseReceived" Text="Joint Release Received"/>
                    <px:PXSelector ID="edAccountID" runat="server" DataField="AccountID"/>
                    <px:PXSelector ID="edProjectID" runat="server" DataField="ProjectID" AutoRefresh="True"
                        FilterByAllFields="True"/>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="DocumentType" CommitChanges="True" MatrixMode="True" />
                    <px:PXGridColumn DataField="CreationDate" TextAlign="Left" />
                    <px:PXGridColumn DataField="DocumentTypeValue" CommitChanges="True" />
                    <px:PXGridColumn DataField="Status" CommitChanges="True"/>
                    <px:PXGridColumn DataField="Required" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="Received" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="ReceivedDate" TextAlign="Left" />
                    <px:PXGridColumn DataField="IsReceivedFromJointVendor" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="JointReceivedDate" TextAlign="Left" />
                    <px:PXGridColumn DataField="IsProcessed" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="IsVoided" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="IsCreatedAutomatically" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="SentDate" TextAlign="Left" />
                    <px:PXGridColumn DataField="EffectiveDate" TextAlign="Left" />
                    <px:PXGridColumn DataField="IsExpired" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="ExpirationDate" TextAlign="Left" />
                    <px:PXGridColumn DataField="Limit" TextAlign="Right" />
                    <px:PXGridColumn DataField="MethodSent" TextAlign="Left" />
                    <px:PXGridColumn DataField="ProjectID" CommitChanges="True"
                        LinkCommand="ComplianceDocuments_Project_ViewDetails"/>
                    <px:PXGridColumn DataField="CostTaskID" TextAlign="Left" CommitChanges="True"
                        LinkCommand="ComplianceDocuments_Task_ViewDetails" />
                    <px:PXGridColumn DataField="RevenueTaskID" TextAlign="Left" CommitChanges="True"
                        LinkCommand="ComplianceDocuments_Task_ViewDetails" />
                    <px:PXGridColumn DataField="CostCodeID" CommitChanges="True"
                        LinkCommand="ComplianceDocuments_CostCode_ViewDetails"/>
                    <px:PXGridColumn DataField="CustomerID" CommitChanges="True"
                        LinkCommand="ComplianceDocuments_Customer_ViewDetails" />
                    <px:PXGridColumn DataField="CustomerName" TextAlign="Left" />
                    <px:PXGridColumn DataField="VendorID" CommitChanges="True"
                        LinkCommand="ComplianceDocuments_Vendor_ViewDetails" />
                    <px:PXGridColumn DataField="VendorName" TextAlign="Left" />
                    <px:PXGridColumn DataField="SecondaryVendorID" CommitChanges="True"
                        LinkCommand="ComplianceDocuments_Vendor_ViewDetails" />
                    <px:PXGridColumn DataField="SecondaryVendorName" TextAlign="Left" />
                    <px:PXGridColumn DataField="PurchaseOrder" DisplayMode="Text"
                        CommitChanges="True" LinkCommand="ComplianceDocument$PurchaseOrder$Link" />
                    <px:PXGridColumn DataField="PurchaseOrderLineItem" TextAlign="Left" />
                    <px:PXGridColumn DataField="Subcontract" DisplayMode="Text"
                        CommitChanges="True" LinkCommand="ComplianceDocument$Subcontract$Link" />
                    <px:PXGridColumn DataField="SubcontractLineItem" TextAlign="Left" CommitChanges="True"/>
                    <px:PXGridColumn DataField="ChangeOrderNumber" DisplayMode="Text"
                        CommitChanges="True" LinkCommand="ComplianceDocument$ChangeOrderNumber$Link" />
                    <px:PXGridColumn DataField="InvoiceID" DisplayMode="Text" CommitChanges="True"
                        LinkCommand="ComplianceDocument$InvoiceID$Link" />
                    <px:PXGridColumn DataField="InvoiceAmount" TextAlign="Right" />
                    <px:PXGridColumn DataField="BillID" DisplayMode="Text" CommitChanges="True"
                        LinkCommand="ComplianceDocument$BillID$Link" />
                    <px:PXGridColumn DataField="BillAmount" TextAlign="Right" />
                    <px:PXGridColumn DataField="LienWaiverAmount" TextAlign="Right" />
                    <px:PXGridColumn DataField="LienNoticeAmount" TextAlign="Right" />
                    <px:PXGridColumn DataField="SponsorOrganization" TextAlign="Left" />
                    <px:PXGridColumn DataField="CertificateNumber" TextAlign="Left" />
                    <px:PXGridColumn DataField="InsuranceCompany" TextAlign="Left" />
                    <px:PXGridColumn DataField="Policy" TextAlign="Left" />
                    <px:PXGridColumn DataField="AccountID" TextAlign="Left" CommitChanges="True"/>
                    <px:PXGridColumn DataField="ApCheckID" TextAlign="Left" DisplayMode="Text"
                        CommitChanges="True" LinkCommand="ComplianceDocument$ApCheckID$Link" />
                    <px:PXGridColumn DataField="CheckNumber" TextAlign="Left" />
                    <px:PXGridColumn DataField="ArPaymentID" TextAlign="Left" DisplayMode="Text"
                        CommitChanges="True" LinkCommand="ComplianceDocument$ArPaymentID$Link" />
                    <px:PXGridColumn DataField="ProjectTransactionID" TextAlign="Left" DisplayMode="Text"
                        CommitChanges="True" LinkCommand="ComplianceDocument$ProjectTransactionID$Link" />
                    <px:PXGridColumn DataField="PaymentDate" TextAlign="Left" />
                    <px:PXGridColumn DataField="ReceiptDate" TextAlign="Left" />
                    <px:PXGridColumn DataField="DateIssued" TextAlign="Left" />
                    <px:PXGridColumn DataField="ThroughDate" TextAlign="Left" />
                    <px:PXGridColumn DataField="ReceiveDate" TextAlign="Left" />
                    <px:PXGridColumn DataField="ReceivedBy" TextAlign="Left" />
                    <px:PXGridColumn DataField="SourceType" TextAlign="Left" />
                    <px:PXGridColumn DataField="IsRequiredJointCheck" TextAlign="Center"
                        Type="CheckBox" />
                    <px:PXGridColumn DataField="JointVendorInternalId" TextAlign="Left" CommitChanges="True"
                        LinkCommand="ComplianceDocuments_Vendor_ViewDetails" />
                    <px:PXGridColumn DataField="JointVendorExternalName" TextAlign="Left" />
                    <px:PXGridColumn DataField="JointAmount" TextAlign="Right" />
                    <px:PXGridColumn DataField="JointLienWaiverAmount" TextAlign="Right" />
                    <px:PXGridColumn DataField="JointLienNoticeAmount" TextAlign="Right" />
                    <px:PXGridColumn DataField="JointRelease" TextAlign="Left" />
                    <px:PXGridColumn DataField="JointReleaseReceived" TextAlign="Center"
                        Type="CheckBox" />
                    <px:PXGridColumn DataField="ComplianceDocumentID"/>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <Mode InplaceInsert="False" InitNewRow="True" />
        <CallbackCommands>
            <InitRow CommitChanges="True" />
        </CallbackCommands>
        <AutoCallBack>
            <Behavior CommitChanges="True" PostData="Page" />
        </AutoCallBack>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
