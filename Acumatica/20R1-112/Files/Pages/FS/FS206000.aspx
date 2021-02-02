<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS206000.aspx.cs"
Inherits="Page_FS206000" Title="Untitled Page" %>
    <%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
        <asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
            <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="BillingCycleRecords" TypeName="PX.Objects.FS.BillingCycleMaint"
            PageLoadBehavior="InsertRecord"></px:PXDataSource>
        </asp:Content>
        <asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
            <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="BillingCycleRecords" 
                TabIndex="900" DefaultControlID="edBillingCycleCD" FilesIndicator="true">
                <Template>
                    <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True"></px:PXLayoutRule>
                    <px:PXSelector ID="edBillingCycleCD" runat="server" DataField="BillingCycleCD"></px:PXSelector>
                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr"></px:PXTextEdit>
                    <px:PXLabel ID="PXLabel1" runat="server" Height="10px"></px:PXLabel>
                    <px:PXLayoutRule runat="server"></px:PXLayoutRule>
                    <px:PXGroupBox ID="edBillingBy" runat="server" Caption="Run Billing For" CommitChanges="True" DataField="BillingBy">
                        <Template>
                            <px:PXRadioButton ID="edBillingBy_op0" runat="server" Text="Appointments" Value="AP" GroupName="edBillingBy"/>
                            <px:PXRadioButton ID="edBillingBy_op1" runat="server" Text="Service Orders" Value="SO" GroupName="edBillingBy"/>
                        </Template>
                    </px:PXGroupBox>
                    <px:PXLayoutRule runat="server"></px:PXLayoutRule>
                    <px:PXGroupBox ID="edBillingCycleType" runat="server" Caption="Group Billing Documents By" CommitChanges="True" DataField="BillingCycleType">
                        <Template>
                            <px:PXRadioButton ID="edBillingCycleType_op0" runat="server" Text="Appointments" Value="AP" GroupName="edBillingCycleType"/>
                            <px:PXRadioButton ID="edBillingCycleType_op1" runat="server" Text="Service Orders" Value="SO" GroupName="edBillingCycleType"/>
                            <px:PXRadioButton ID="edBillingCycleType_op2" runat="server" Text="Customer Order" Value="PO" GroupName="edBillingCycleType"/>
                            <px:PXRadioButton ID="edBillingCycleType_op3" runat="server" Text="External Reference" Value="WO" GroupName="edBillingCycleType"/>
                            <px:PXRadioButton ID="edBillingCycleType_op4" runat="server" Text="Time Cycle" Value="TC" GroupName="edBillingCycleType"/>
                        </Template>
                    </px:PXGroupBox>
                    <px:PXLayoutRule runat="server" GroupCaption="Time Frame Grouping Settings"></px:PXLayoutRule>
                    <px:PXGroupBox ID="edTimeCycleType" runat="server" Caption="Prepare On" CommitChanges="True" DataField="TimeCycleType">
                        <Template>
                            <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                            <px:PXRadioButton ID="edTimeCycleType_op1" runat="server" GroupName="edTimeCycleType" Text="Fixed Day of Month" Value="MT" Size="SM" />
                            <px:PXDropDown ID="edTimeCycleDayOfMonth" runat="server" DataField="TimeCycleDayOfMonth" Size="XS" CommitChanges="True" SuppressLabel="True"></px:PXDropDown>
                            <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                            <px:PXLayoutRule runat="server" Merge="True"></px:PXLayoutRule>
                            <px:PXRadioButton ID="edTimeCycleType_op2" runat="server" Text="Fixed Day of Week" Value="WK" GroupName="edTimeCycleType" Size="SM" />
                            <px:PXDropDown ID="edTimeCycleWeekDay" runat="server" DataField="TimeCycleWeekDay" CommitChanges="True" Size="S" SuppressLabel="True"></px:PXDropDown>
                            <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                        </Template>
                    </px:PXGroupBox>
                    <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                    <px:PXCheckBox ID="edGroupBillByLocations" runat="server" DataField="GroupBillByLocations" AlignLeft="True">
                    </px:PXCheckBox>
                    <px:PXCheckBox ID="edInvoiceOnlyCompletedServiceOrder" runat="server" AlignLeft="True" DataField="InvoiceOnlyCompletedServiceOrder" Width="350px">
                    </px:PXCheckBox>
                </Template>
                <AutoSize Container="Window" Enabled="True" />
            </px:PXFormView>
        </asp:Content>