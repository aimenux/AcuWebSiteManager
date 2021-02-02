<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="BC301000.aspx.cs" Inherits="Page_BC301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
		TypeName="PX.Commerce.Core.BCSyncHistoryMaint"
		PrimaryView="MasterView">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Skip" Visible="True" DependOnGrid="grid"></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="SetSynced" Visible="True" DependOnGrid="grid"></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="StatusAddAction" Visible="True" DependOnGrid="grid"></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="StatusEditAction" Visible="True" DependOnGrid="grid"></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="StatusDeatilsAction" Visible="True" DependOnGrid=""></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="NavigateLocal" Visible="False" DependOnGrid="grid"></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="NavigateExtern" Visible="False" DependOnGrid="grid"></px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="MasterView" Width="100%" Height="100px" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule LabelsWidth="S" ControlSize="M" StartColumn="True" ID="PXLayoutRule1" runat="server" StartRow="True"></px:PXLayoutRule>
			<px:PXSelector AutoRefresh="True" NullText="&lt;SELECT>" CommitChanges="True" runat="server" ID="CstPXSelector13" DataField="BindingID"></px:PXSelector>
			<px:PXDropDown NullText="&lt;SELECT>" AllowNull="True" CommitChanges="True" runat="server" ID="CstPXDropDown2" DataField="EntityType" AutoSuggest="False"></px:PXDropDown>
		</Template>
		<AutoSize Container="Window" Enabled="True"></AutoSize>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid AllowPaging="True" AdjustPageSize="Auto" SyncPosition="True" MatrixMode="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="PrimaryInquire" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="DetailsView">
				<Columns>
					<px:PXGridColumn CommitChanges="True" Type="CheckBox" TextAlign="Center" DataField="Selected" Width="80" AllowCheckAll="True"></px:PXGridColumn>
					<px:PXGridColumn LinkCommand="NavigateEntity" DataField="SyncID" Width="70"></px:PXGridColumn>
					<px:PXGridColumn CommitChanges="True" DataField="ConnectorType" Width="70"></px:PXGridColumn>
					<px:PXGridColumn LinkCommand="NavigateEntity" DataField="EntityType" Width="70"></px:PXGridColumn>
					<px:PXGridColumn LinkCommand="NavigateStore" DataField="BindingID" Width="100"></px:PXGridColumn>
					<px:PXGridColumn LinkCommand="NavigateLocal" DataField="LocalID" Width="200"></px:PXGridColumn>
					<px:PXGridColumn LinkCommand="NavigateLocal" DataField="Source" Width="200"></px:PXGridColumn>
					<px:PXGridColumn LinkCommand="NavigateExtern" DataField="ExternID" Width="120"></px:PXGridColumn>
					<px:PXGridColumn DataField="Status" Width="100"></px:PXGridColumn>
					<px:PXGridColumn TextAlign="Center" Type="CheckBox" DataField="PendingSync" Width="80"></px:PXGridColumn>
					<px:PXGridColumn DataField="BCEntity__PrimarySystem" Width="70" />
					<px:PXGridColumn DataField="BCEntity__Direction" Width="80" />
					<px:PXGridColumn DisplayFormat="g" DataField="LocalTS" Width="120"></px:PXGridColumn>
					<px:PXGridColumn DisplayFormat="g" DataField="ExternTS" Width="120"></px:PXGridColumn>
					<px:PXGridColumn DataField="ExternHash" Width="100"></px:PXGridColumn>
					<px:PXGridColumn DataField="LastErrorMessage" Width="280"></px:PXGridColumn>
					<px:PXGridColumn DataField="LastOperation" Width="70"></px:PXGridColumn>
					<px:PXGridColumn DisplayFormat="g" DataField="LastOperationTS" Width="120"></px:PXGridColumn>
					<px:PXGridColumn DataField="AttemptCount" Width="70"></px:PXGridColumn>
					<px:PXGridColumn DataField="BCEntity__IsActive" Width="60" Type="CheckBox" TextAlign="Center" />
				</Columns>
				<RowTemplate>
					<px:PXDateTimeEdit DisplayFormat="g" runat="server" ID="CstPXDateTimeEdit8" DataField="ExternTS"></px:PXDateTimeEdit>
					<px:PXDateTimeEdit DisplayFormat="g" runat="server" ID="CstPXDateTimeEdit9" DataField="LastOperationTS"></px:PXDateTimeEdit>
					<px:PXDateTimeEdit DisplayFormat="g" runat="server" ID="CstPXDateTimeEdit10" DataField="LocalTS"></px:PXDateTimeEdit>
				</RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150"></AutoSize>
		<ActionBar>
			<CustomItems>
			</CustomItems>
		</ActionBar>
		<Mode InitNewRow="True" AllowAddNew="False"></Mode>
	</px:PXGrid>
	<px:PXPanel runat="server" ID="CstPanel15" />
	<px:PXSmartPanel AutoRepaint="True" Caption="Add or Edit Sync Status" AutoReload="False" CaptionVisible="True" CommandName="" runat="server" ID="CstSmartPanel16" Key="StatusEditPanel">
		<px:PXFormView DataMember="StatusEditPanel" runat="server" ID="CstFormView17">
			<Template>
				<px:PXLayoutRule ControlSize="M" LabelsWidth="S" StartColumn="True" runat="server" ID="CstPXLayoutRule18" StartRow="True"></px:PXLayoutRule>
				<px:PXDropDown CommitChanges="True" runat="server" ID="CstPXDropDown20" DataField="ConnectorType"></px:PXDropDown>
				<px:PXSelector CommitChanges="True" runat="server" ID="CstPXSelector19" DataField="BindingID"></px:PXSelector>
				<px:PXDropDown CommitChanges="True" runat="server" ID="CstPXDropDown21" DataField="EntityType"></px:PXDropDown>
				<px:PXLayoutRule LabelsWidth="S" ControlSize="L" runat="server" ID="CstPXLayoutRule25" StartColumn="True"></px:PXLayoutRule>
				<pxa:PXDynamicSelector CommitChanges="True" runat="server" ID="CstPXNumberEdit23" DataField="LocalID"></pxa:PXDynamicSelector>
				<px:PXTextEdit CommitChanges="True" runat="server" ID="CstPXTextEdit22" DataField="ExternID"></px:PXTextEdit>
				<px:PXCheckBox CommitChanges="True" runat="server" ID="CstPXCheckBox24" DataField="NeedSync"></px:PXCheckBox>
				<px:PXLayoutRule runat="server" ID="CstPXLayoutRule26" StartRow="True"></px:PXLayoutRule>
				<px:PXPanel runat="server" ID="CstPanel27" SkinID="Buttons">
					<px:PXButton runat="server" ID="CstButton28" DialogResult="OK" Text="OK">
						<AutoCallBack Enabled="True">
							<Behavior CommitChanges="True" PostData="Container"></Behavior>
						</AutoCallBack>
					</px:PXButton>
				</px:PXPanel>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>
	<px:PXSmartPanel LoadOnDemand="True" CaptionVisible="True" Caption="Sync Record Details" Height="420px" Width="700px" runat="server" ID="PXDetailsPanel" Key="StatusDetailsPanel" AutoCallBack-Command="Refresh" AutoCallBack-Target="gridRecordDetails">
		<px:PXGrid runat="server" ID="gridRecordDetails" Style="height: 250px;" SyncPosition="true" 
			AutoAdjustColumns="true" Width="100%" SkinID="Details" AdjustPageSize="Auto" DataSourceID="ds" AllowPaging="True">
			<ActionBar DefaultAction="navigate">
				<Actions>
					<AddNew ToolBarVisible="False"></AddNew>
					<Delete ToolBarVisible="False"></Delete>
					<ExportExcel ToolBarVisible="False"></ExportExcel>
				</Actions>
			</ActionBar>
			<Levels>
				<px:PXGridLevel DataMember="StatusDetailsPanel">
					<Columns>
						<px:PXGridColumn DataField="LocalID" Width="200" />
						<px:PXGridColumn DataField="ExternID" Width="200" />
						<px:PXGridColumn DataField="EntityType" Width="250" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize  Enabled="True" MinHeight="150"></AutoSize>
		</px:PXGrid>
		<px:PXPanel runat="server" ID="CstPanel27" SkinID="Buttons">
			<px:PXButton runat="server" ID="CstButton28" DialogResult="OK" Text="OK">
				<AutoCallBack Enabled="True">
				</AutoCallBack>
			</px:PXButton>
		</px:PXPanel>

	</px:PXSmartPanel>
</asp:Content>
