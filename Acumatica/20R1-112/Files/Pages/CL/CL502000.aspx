<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CL502000.aspx.cs"
Inherits="Page_CL502000" Title="Print/Email Lien Waivers" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
                     TypeName="PX.Objects.CN.Compliance.CL.Graphs.PrintEmailLienWaiversProcess"
                     PrimaryView="Filter"
                     PageLoadBehavior="PopulateSavedValues">
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection"
                   DefaultControlID="edAction">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXDropDown CommitChanges="True" ID="edAction" runat="server" DataField="Action" />
            <px:PXSelector CommitChanges="True" ID="edProjectId" runat="server" DataField="ProjectId" />
            <px:PXSelector CommitChanges="True" ID="edVendorId" runat="server" DataField="VendorId" />
            <px:PXDropDown CommitChanges="True" ID="edLienWaiverType" runat="server" DataField="LienWaiverType" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" AlreadyLocalized="False" DataField="StartDate" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" AlreadyLocalized="False" DataField="EndDate" />
            <px:PXCheckBox CommitChanges="True" ID="chkShouldShowProcessed" runat="server" AlignLeft="True" AlreadyLocalized="False" DataField="ShouldShowProcessed" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
            <px:PXCheckBox CommitChanges="True" ID="chkPrintWithDeviceHub" runat="server" AlignLeft="True" AlreadyLocalized="False" DataField="PrintWithDeviceHub" />
            <px:PXCheckBox CommitChanges="True" ID="chkDefinePrinterManually" runat="server" AlignLeft="True" AlreadyLocalized="False" DataField="DefinePrinterManually" />
            <px:PXSelector CommitChanges="True" ID="edPrinterID" runat="server" DataField="PrinterID" />
            <px:PXTextEdit CommitChanges="true" ID="edNumberOfCopies" runat="server" DataField="NumberOfCopies" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%"
               SkinID="Details" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="LienWaivers">
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" />
                    <px:PXGridColumn DataField="CreationDate" TextAlign="Center"/>
                    <px:PXGridColumn DataField="DocumentTypeValue" TextAlign="Center"/>
                    <px:PXGridColumn DataField="Status" TextAlign="Center"/>
                    <px:PXGridColumn DataField="Required" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="Received" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="IsReceivedFromJointVendor" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="IsProcessed" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="IsVoided" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="IsCreatedAutomatically" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="ProjectID" TextAlign="Center"/>
                    <px:PXGridColumn DataField="CustomerID" TextAlign="Center"/>
                    <px:PXGridColumn DataField="CustomerName" TextAlign="Center"/>
                    <px:PXGridColumn DataField="VendorID" TextAlign="Center"/>
                    <px:PXGridColumn DataField="VendorName" TextAlign="Center"/>
                    <px:PXGridColumn DataField="Subcontract" TextAlign="Center"/>
                    <px:PXGridColumn DataField="BillID" TextAlign="Center"/>
                    <px:PXGridColumn DataField="BillAmount" TextAlign="Center"/>
                    <px:PXGridColumn DataField="LienWaiverAmount" TextAlign="Center"/>
                    <px:PXGridColumn DataField="LienNoticeAmount" TextAlign="Center"/>
                    <px:PXGridColumn DataField="ApCheckID" TextAlign="Center"/>
                    <px:PXGridColumn DataField="PaymentDate" TextAlign="Center"/>
                    <px:PXGridColumn DataField="ThroughDate" TextAlign="Center"/>
                    <px:PXGridColumn DataField="JointVendorInternalId" TextAlign="Center"/>
                    <px:PXGridColumn DataField="JointVendorExternalName" TextAlign="Center"/>
                    <px:PXGridColumn DataField="JointAmount" TextAlign="Center"/>
                    <px:PXGridColumn DataField="JointLienWaiverAmount" TextAlign="Center"/>
                    <px:PXGridColumn DataField="JointLienNoticeAmount" TextAlign="Center"/>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar>
            <Actions>
                <Delete ToolBarVisible="False" />
                <AddNew ToolBarVisible="False" />
            </Actions>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
