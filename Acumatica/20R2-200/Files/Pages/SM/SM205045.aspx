<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM205045.aspx.cs" Inherits="Page_SM205045"
	Title="Automation Notification Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<pxa:AUDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.AUNotificationPopup"
		PrimaryView="Notification">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Refresh" Visible="false" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" ClosePopup="true" />
			<px:PXDSCallbackCommand Name="Delete" PopupVisible="true" ClosePopup="true" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="SiteMap" TreeKeys="NodeID" />
			<px:PXTreeDataMember TreeView="Graphs" TreeKeys="GraphName,IsNamespace" />
		</DataTrees>
	</pxa:AUDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="formNotification" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" Caption="Change Notification" DataMember="Notification" NoteIndicator="True"
		FilesIndicator="True" EmailingGraph="" LinkPage="" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" 
				LabelsWidth="M" />
			<px:PXTreeSelector ID="edScreenID" runat="server" DataField="ScreenID" PopulateOnDemand="True"
				ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="SiteMap" MinDropWidth="413">
				<DataBindings>
					<px:PXTreeItemBinding DataMember="SiteMap" TextField="Title" ValueField="ScreenID"
						ImageUrlField="Icon" />
				</DataBindings>
				<AutoCallBack Command="Refresh" Target="ds">
				</AutoCallBack>
			</px:PXTreeSelector>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" /></Template>
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Parameters>
			<px:PXControlParam ControlID="formNotification" Name="AUNotification.screenID" PropertyName="NewDataKey[&quot;ScreenID&quot;]"
				Type="String" />
		</Parameters>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="gridConditions" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" AdjustPageSize="Auto" AllowSearch="True" SkinID="Details" MatrixMode="true">
		<ActionBar>
		</ActionBar>
		<Levels>
			<px:PXGridLevel DataMember="Filters">
				<Mode InitNewRow="True" />
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
						Width="60px" />
					<px:PXGridColumn AllowNull="False" DataField="OpenBrackets" Type="DropDownList" Width="100px"
						AutoCallBack="true" />
					<px:PXGridColumn DataField="FieldName" Width="200px" Type="DropDownList" AutoCallBack="true" />
					<px:PXGridColumn AllowNull="False" DataField="Condition" Type="DropDownList" />
					<px:PXGridColumn AllowNull="False" DataField="IsRelative" TextAlign="Center" Type="CheckBox"
						Width="60px" />
					<px:PXGridColumn DataField="Value" Width="200px" />
					<px:PXGridColumn DataField="Value2" Width="200px" />
					<px:PXGridColumn AllowNull="False" DataField="CloseBrackets" Type="DropDownList"
						Width="60px" />
					<px:PXGridColumn AllowNull="False" DataField="Operator" Type="DropDownList" Width="60px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Enabled="True" MinHeight="150" Container="Window" />
	</px:PXGrid>
</asp:Content>
