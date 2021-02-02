<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP505010.aspx.cs"
    Inherits="Page_EP505010" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.TimeCardRelease" PrimaryView="FilteredItems"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top"
        Caption="Time Cards" SkinID="PrimaryInquire" SyncPosition="True" FastFilterFields="TimeCardCD,EmployeeID,EmployeeID_description" AdjustPageSize="Auto" AllowPaging="True">
        <Levels>
            <px:PXGridLevel DataMember="FilteredItems">
                <RowTemplate>
                    <px:PXSelector ID="edRefNbr" runat="server" DataField="TimecardCD" Enabled="False" AllowEdit="True" />
                    <px:PXSelector CommitChanges="True" ID="edEmployee" runat="server" DataField="EmployeeID" />
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" GroupCaption="Time" StartColumn="True" StartGroup="True" />
                    <px:PXTimeSpan runat="server" DataField="TimeSpent" ID="RegularTime" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" />
                    <px:PXTimeSpan ID="BillableTime" runat="server" DataField="TimeBillable" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" />
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" GroupCaption="Overtime" StartColumn="True" StartGroup="True" />
                    <px:PXTimeSpan runat="server" DataField="OvertimeSpent" ID="OvertimeSpent" Enabled="False" SuppressLabel="True" Size="XS" InputMask="hh:mm" MaxHours="99" />
                    <px:PXTimeSpan ID="BillableOvertime" runat="server" DataField="OvertimeBillable" Enabled="False" SuppressLabel="True" Size="XS" InputMask="hh:mm" MaxHours="99" />
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" GroupCaption="Total" StartColumn="True" StartGroup="True" />
                    <px:PXTimeSpan ID="edTimeSpent" runat="server" DataField="TotalTimeSpent" Enabled="false" Size="XS" SuppressLabel="True" InputMask="hh:mm" MaxHours="99" />
                    <px:PXTimeSpan ID="PXMaskEdit1" runat="server" DataField="TotalTimeBillable" Enabled="false" SuppressLabel="True" Size="XS" InputMask="hh:mm" MaxHours="99" />

                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="TimeCardCD" />
                    <px:PXGridColumn DataField="EmployeeID" Label="Employee ID" />
                    <px:PXGridColumn DataField="EmployeeID_description" />
                    <px:PXGridColumn DataField="WeekID" Label="Week" DisplayMode="Text" />
                    <px:PXGridColumn DataField="TimeSpent" RenderEditorText="True" />
                    <px:PXGridColumn DataField="OvertimeSpent" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TotalTimeSpent" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TimeBillable" RenderEditorText="True" />
                    <px:PXGridColumn DataField="OvertimeBillable" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TotalTimeBillable" RenderEditorText="True" />
                    <px:PXGridColumn DataField="EPApprovalEx__ApprovedByID" />
                    <px:PXGridColumn DataField="EPEmployeeEx__AcctName" />
                    <px:PXGridColumn DataField="EPApprovalEx__ApproveDate" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
