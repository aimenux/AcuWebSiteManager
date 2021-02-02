<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM502000.aspx.cs"
    Inherits="Page_PM502000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="true" PrimaryView="Filter" TypeName="PX.Objects.PM.AllocationProcess">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="ViewProject" Visible="False"/>
			<px:PXDSCallbackCommand Name="ViewTask" Visible="False"/>
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edAllocationID" runat="server" DataField="AllocationID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edTaskID" runat="server" DataField="TaskID" AutoRefresh="True" DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edDate" runat="server" DataField="Date" />
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartGroup="True" GroupCaption="Date Range Restrictions" />
            <px:PXDateTimeEdit CommitChanges="True" ID="PXDateTimeEdit1" runat="server" DataField="DateFrom" />
            <px:PXDateTimeEdit CommitChanges="True" ID="PXDateTimeEdit2" runat="server" DataField="DateTo" />
            </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" Caption="Projects"
        SkinID="PrimaryInquire" AdjustPageSize="Auto" AllowPaging="True" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edProjectID" runat="server" DataField="ProjectID" Enabled="False" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSegmentMask ID="edTaskCD" runat="server" DataField="TaskCD" Enabled="False" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False" />
                    <px:PXSelector ID="edAllocationID" runat="server" DataField="AllocationID" Enabled="False" AllowEdit="True"/>
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="ProjectID" Label="Project ID" LinkCommand ="ViewProject"/>
                    <px:PXGridColumn DataField="TaskCD" Label="Task ID" LinkCommand ="ViewTask"/>
                    <px:PXGridColumn DataField="Description" Label="Description" />
                    <px:PXGridColumn DataField="AllocationID" Label="Billing Rule" DisplayMode="Hint" />
                    <px:PXGridColumn DataField="CustomerID" Label="Customer" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar PagerVisible="False" />
    </px:PXGrid>
</asp:Content>
