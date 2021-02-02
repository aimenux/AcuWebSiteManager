<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP406000.aspx.cs" Inherits="Pages_EP_EP406000" Title="My Timecards" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.TimecardPrimary" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="create" Visible="True" RepaintControls="All" HideText="True" />
            <px:PXDSCallbackCommand Name="update" Visible="True" RepaintControls="All" DependOnGrid="grid" HideText="True" />
			<px:PXDSCallbackCommand Name="delete" Visible="True" RepaintControls="All" DependOnGrid="grid" HideText="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="phF" runat="Server">
   <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edEmployee" NoteField="">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edEmployee" runat="server" DataField="EmployeeID"  />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" AdjustPageSize="Auto"
        AllowPaging="True" Caption="Time Cards" FastFilterFields="TimecardCD,Description" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <RowTemplate>
                    <px:PXSelector ID="PXSelector1" runat="server" DataField="WeekID" Enabled="False" AllowEdit="False" />
                    <px:PXSelector ID="edRefNbr" runat="server" DataField="TimeCardCD" Enabled="False" AllowEdit="False" />
                    <px:PXSelector CommitChanges="True" ID="edEmployee" runat="server" DataField="EmployeeID" />
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" GroupCaption="Time" StartColumn="True" StartGroup="True" />
                    <px:PXTimeSpan runat="server" DataField="TimeSpentCalc" ID="RegularTime" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" SummaryMode="true"/>
                    <px:PXTimeSpan ID="BillableTime" runat="server" DataField="TimeBillableCalc" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" SummaryMode="true"/>
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" GroupCaption="Overtime" StartColumn="True" StartGroup="True" />
                    <px:PXTimeSpan runat="server" DataField="OvertimeSpentCalc" ID="OvertimeSpentCalc" Enabled="False" SuppressLabel="True" Size="XS" InputMask="hh:mm" MaxHours="99" SummaryMode="true"/>
                    <px:PXTimeSpan ID="BillableOvertime" runat="server" DataField="OvertimeBillableCalc" Enabled="False" SuppressLabel="True" Size="XS" InputMask="hh:mm" MaxHours="99" SummaryMode="true"/>
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" GroupCaption="Total" StartColumn="True" StartGroup="True" />
                    <px:PXTimeSpan ID="edTimeSpent" runat="server" DataField="TotalSpentCalc" Enabled="false" Size="XS" SuppressLabel="True" InputMask="hh:mm" MaxHours="99" SummaryMode="true"/>
                    <px:PXTimeSpan ID="PXMaskEdit1" runat="server" DataField="TotalBillableCalc" Enabled="false" SuppressLabel="True" Size="XS" InputMask="hh:mm" MaxHours="99" SummaryMode="true"/>
                </RowTemplate>
                <Columns>

                    <px:PXGridColumn DataField="EmployeeID" Label="Employee ID" />
                    <px:PXGridColumn DataField="EmployeeID_description" />
                    <px:PXGridColumn DataField="WeekID" Label="Week" DisplayMode="Text" />
                    <px:PXGridColumn DataField="Status" Label="Status" Type="DropDownList" />
                    <px:PXGridColumn DataField="TimeCardCD" Label="TimeCardCD" />
                    
                    <px:PXGridColumn DataField="TimeSpentCalc" Label="TimeSpentCalc" RenderEditorText="True" />
                    <px:PXGridColumn DataField="OvertimeSpentCalc" Label="OvertimeSpentCalc" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TotalSpentCalc" Label="TotalSpentCalc" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TimeBillableCalc" Label="TimeBillableCalc" RenderEditorText="True" />
                    <px:PXGridColumn DataField="OvertimeBillableCalc" Label="OvertimeBillableCalc" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TotalBillableCalc" Label="TotalBillableCalc" RenderEditorText="True" />
                    <px:PXGridColumn DataField="BillingRateCalc" Label="BillingRateCalc" />
                    <px:PXGridColumn DataField="WeekStartDate" Label="WeekStartDate" />
                 </Columns>
            </px:PXGridLevel>
        </Levels>
         <ActionBar DefaultAction="update"/>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
	    <Mode AllowAddNew="False" AllowUpdate="False" AllowDelete="False"/>
    </px:PXGrid>
</asp:Content>
