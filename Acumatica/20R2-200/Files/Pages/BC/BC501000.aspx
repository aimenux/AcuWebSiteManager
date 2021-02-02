<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="BC501000.aspx.cs" Inherits="Page_BC501000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
  <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="PX.Commerce.Core.BCPrepareData"
        PrimaryView="ProcessFilter"
        >
    <CallbackCommands>
      <px:PXDSCallbackCommand Name="NavigateStore" Visible="False" DependOnGrid="grid"></px:PXDSCallbackCommand>
      <px:PXDSCallbackCommand Name="navigateEntity" Visible="False" DependOnGrid="grid"></px:PXDSCallbackCommand>
      <px:PXDSCallbackCommand Name="NavigatePrepared" Visible="False" DependOnGrid="grid"></px:PXDSCallbackCommand>
      <px:PXDSCallbackCommand Name="NavigateProcessed" Visible="False" DependOnGrid="grid"></px:PXDSCallbackCommand>
    </CallbackCommands>
  </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
  <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="ProcessFilter" Width="100%" Height="" AllowAutoHide="false">
    <Template>
      <px:PXLayoutRule StartRow="True" LabelsWidth="S" ControlSize="M" runat="server" ID="CstPXLayoutRule1" StartColumn="True" ></px:PXLayoutRule>
      <px:PXSelector AutoRefresh="True" NullText="&lt;SELECT>" CommitChanges="True" runat="server" ID="CstPXSelector8" DataField="BindingID" ></px:PXSelector>
      <px:PXDropDown runat="server" ID="CstPXDropDown1" DataField="EntityType" CommitChanges="True"  AutoSuggest="False" />
      <px:PXLayoutRule runat="server" ID="CstPXLayoutRule4" StartColumn="True" ColumnWidth="250" />
      <px:PXDropDown runat="server" ID="CstPXDropDown2" CommitChanges="True" DataField="PrepareMode" />
      <px:PXLayoutRule runat="server" ID="CstPXLayoutRule5" StartColumn="True" ></px:PXLayoutRule>
      <px:PXDateTimeEdit runat="server" ID="CstPXDateTimeEdit6" DataField="StartDate" Width="180" />
      <px:PXDateTimeEdit runat="server" ID="CstPXDateTimeEdit5" DataField="EndDate" Width="180" /></Template>
  
    <AutoSize Container="Window" Enabled="True" ></AutoSize>
    <AutoSize Container="Window" ></AutoSize></px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
  <px:PXGrid AutoAdjustColumns="True" ID="grid" runat="server" MatrixMode="True" DataSourceID="ds" Width="100%" Height="" SkinID="PrimaryInquire"  AllowAutoHide="false">
    <Levels>
      <px:PXGridLevel DataMember="Entities">
          <Columns>
          <px:PXGridColumn Type="CheckBox" AllowCheckAll="False" TextAlign="Center" DataField="Selected" Width="60" ></px:PXGridColumn>
          <px:PXGridColumn LinkCommand="NavigateStore" DataField="BindingID" Width="140" ></px:PXGridColumn>
          <px:PXGridColumn LinkCommand="navigateEntity" DataField="EntityType" Width="120" ></px:PXGridColumn>
          <px:PXGridColumn DataField="ConnectorType" Width="70" ></px:PXGridColumn>
          <px:PXGridColumn DataField="Direction" Width="70" ></px:PXGridColumn>
          <px:PXGridColumn DataField="PrimarySystem" Width="70" ></px:PXGridColumn>
          <px:PXGridColumn DataField="SyncSortOrder" Width="70" ></px:PXGridColumn>
          <px:PXGridColumn DataField="ImportRealTimeStatus" Width="70" ></px:PXGridColumn>
          <px:PXGridColumn DataField="ExportRealTimeStatus" Width="70" ></px:PXGridColumn>
          <px:PXGridColumn DataField="RealTimeMode" Width="70" ></px:PXGridColumn>
          <px:PXGridColumn DataField="BCEntityStats__LastErrorMessage" Width="280" ></px:PXGridColumn>
          <px:PXGridColumn LinkCommand="navigatePrepared" DataField="PreparedRecords" Width="70" ></px:PXGridColumn>
          <px:PXGridColumn LinkCommand="navigateProcessed" DataField="ProcessedRecords" Width="70" ></px:PXGridColumn>
          <px:PXGridColumn DisplayFormat="g" DataField="BCEntityStats__LastIncrementalImportDateTime" Width="90" ></px:PXGridColumn>
          <px:PXGridColumn DisplayFormat="g" DataField="BCEntityStats__LastIncrementalExportDateTime" Width="90" ></px:PXGridColumn>
          <px:PXGridColumn DataField="BCEntityStats__LastReconciliationImportDateTime" Width="90" ></px:PXGridColumn>
          <px:PXGridColumn DataField="BCEntityStats__LastReconciliationExportDateTime" Width="90" ></px:PXGridColumn>
          </Columns>
      </px:PXGridLevel>
    </Levels>
    <AutoSize Container="Window" Enabled="True" MinHeight="150" ></AutoSize>
    <ActionBar >
    </ActionBar>
  </px:PXGrid>
</asp:Content>