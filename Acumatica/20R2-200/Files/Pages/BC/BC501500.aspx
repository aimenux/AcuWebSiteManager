<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="BC501500.aspx.cs" Inherits="Page_BC501500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
  <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="PX.Commerce.Core.BCProcessData"
        PrimaryView="ProcessFilter"
        >
    <CallbackCommands>
      <px:PXDSCallbackCommand Name="Skip" Visible="True" DependOnGrid="grid" ></px:PXDSCallbackCommand>
      <px:PXDSCallbackCommand Name="SetSynced" Visible="True" DependOnGrid="grid" ></px:PXDSCallbackCommand>
      <px:PXDSCallbackCommand Name="NavigateLocal" Visible="False" DependOnGrid="grid" ></px:PXDSCallbackCommand>
      <px:PXDSCallbackCommand Name="NavigateExtern" Visible="False" DependOnGrid="grid" ></px:PXDSCallbackCommand>
      <px:PXDSCallbackCommand Name="NavigateStore" Visible="False" DependOnGrid="grid" ></px:PXDSCallbackCommand>
      <px:PXDSCallbackCommand Name="navigateEntity" Visible="False" DependOnGrid="grid" ></px:PXDSCallbackCommand></CallbackCommands>
  </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
  <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="ProcessFilter" Width="100%" Height="" AllowAutoHide="false">
    <Template>
      <px:PXLayoutRule LabelsWidth="S" ControlSize="M" runat="server" ID="PXLayoutRule1" StartRow="True" ></px:PXLayoutRule>
      <px:PXSelector NullText="&lt;SELECT>" AutoRefresh="True" AllowNull="" CommitChanges="True" runat="server" ID="CstPXSelector1" DataField="BindingID" ></px:PXSelector>
      <px:PXDropDown runat="server" ID="CstPXDropDown7" DataField="EntityType" CommitChanges="True"  AutoSuggest="False" ></px:PXDropDown></Template>
  
    <AutoSize Enabled="True" ></AutoSize>
    <AutoSize Container="Window" ></AutoSize></px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid AdjustPageSize="Auto" runat="server" ID="grid" Height="150px" SkinID="PrimaryInquire" MatrixMode="True" Width="100%" AllowAutoHide="false" DataSourceID="ds">
		<AutoSize Enabled="True" Container="Window" MinHeight="150" ></AutoSize>
		<Levels>
			<px:PXGridLevel DataMember="Statuses">
				<Columns >
				  <px:PXGridColumn TextAlign="Center" Type="CheckBox" AllowCheckAll="True" DataField="Selected" Width="80" ></px:PXGridColumn>
				  <px:PXGridColumn DataField="ConnectorType" Width="100" ></px:PXGridColumn>
				  <px:PXGridColumn LinkCommand="NavigateStore" DataField="BindingID" Width="100" ></px:PXGridColumn>
				  <px:PXGridColumn LinkCommand="navigateEntity" DataField="EntityType" Width="80" ></px:PXGridColumn>
				  <px:PXGridColumn DataField="BCEntity__PrimarySystem" Width="70" Visible="False" />
				  <px:PXGridColumn DataField="BCEntity__Direction" Width="70" Visible="False" />
				  <px:PXGridColumn LinkCommand="NavigateLocal" DataField="Source" Width="200" AllowFilter="False" AllowSort="False"></px:PXGridColumn>
				  <px:PXGridColumn LinkCommand="NavigateExtern" DataField="ExternID" Width="140" ></px:PXGridColumn>
				  <px:PXGridColumn DataField="Status" Width="90" ></px:PXGridColumn>
				  <px:PXGridColumn DataField="LastErrorMessage" Width="350" ></px:PXGridColumn>
				  <px:PXGridColumn DataField="LastOperation" Width="170" ></px:PXGridColumn>
				  <px:PXGridColumn DisplayFormat="g" DataField="LocalTS" Width="120" ></px:PXGridColumn>
				  <px:PXGridColumn DisplayFormat="g" DataField="ExternTS" Width="120" ></px:PXGridColumn>
				  <px:PXGridColumn DataField="SyncID" Width="80" ></px:PXGridColumn>
				  <px:PXGridColumn DisplayFormat="g" DataField="LastOperationTS" Width="150" ></px:PXGridColumn></Columns>
				<RowTemplate>
				  <px:PXDateTimeEdit DisplayFormat="g" runat="server" ID="CstPXDateTimeEdit4" DataField="ExternTS" ></px:PXDateTimeEdit>
				  <px:PXDateTimeEdit DisplayFormat="g" runat="server" ID="CstPXDateTimeEdit5" DataField="LastOperationTS" ></px:PXDateTimeEdit>
				  <px:PXDateTimeEdit runat="server" ID="CstPXDateTimeEdit6" DataField="LocalTS" DisplayFormat="g" ></px:PXDateTimeEdit></RowTemplate></px:PXGridLevel></Levels>

		<ActionBar >		
			<CustomItems></CustomItems></ActionBar>
	</px:PXGrid></asp:Content>
