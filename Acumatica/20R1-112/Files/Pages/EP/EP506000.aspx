<%@ Page Language="C#" MasterPageFile="~/MasterPages/ClearWorkspace.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="EP506000.aspx.cs" Inherits="Page_EP506000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ClearWorkspace.master" %>
<asp:content id="cont1" contentplaceholderid="phDS" runat="Server">

	<script language="javascript" type="text/javascript">
		var lastRowId;
		function gridRowChanged(sender, arg)
		{
			if (sender.activeRow)
			{
				var tastId = sender.activeRow.getCell('taskID').getValue();
				if (tastId != lastRowId)
				{
					lastRowId = tastId;
					var index = sender.ID.indexOf("grid");
					var form = px_all[sender.ID.substring(0, index) + "form"];
					form.executeCommand(PXPanelCommand.Refresh);
				}
			}
			else
			{
				lastRowId = null;
				var index = sender.ID.indexOf("grid");
				var form = px_all[sender.ID.substring(0, index) + "form"];
				form.executeCommand(PXPanelCommand.Refresh);
			}
		} 
	</script>

	<px:PXDataSource ID="ds" runat="server" Visible="False" Width="100%" TypeName="PX.Objects.EP.TasksAndEventsReminder"
		PrimaryView="ReminderListCurrent" PageLoadBehavior="GoFirstRecord">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewActivity" Visible="false" />
			<px:PXDSCallbackCommand DependOnGrid="grid" CommitChanges="true" Name="DismissCurrent" Visible="false" RepaintControlsIDs="grid" />
			<px:PXDSCallbackCommand Name="DismissAll" Visible="false" ClosePopup="true" />
			<px:PXDSCallbackCommand DependOnGrid="grid" CommitChanges="true" Name="DeferCurrent" Visible="false" RepaintControlsIDs="grid" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:content>
<asp:content id="cont2" contentplaceholderid="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Height="63px" Width="100%" Visible="false"
		DataMember="ReminderListCurrent" SkinID="Transparent">
		<Template>
			<px:PXImageView ID="edTypeIcon" runat="server" DataField="ClassIcon" Style="z-index: 110; left: 9px;
				position: absolute; top: 9px;" />
			<px:PXTextEdit ID="edSubject" runat="server" Style="z-index: 110; left: 36px; position: absolute; top: 9px;
				font-weight: bold;" Width="100%" DataField="Subject" SkinID="Label" />
		</Template>
		<Parameters>
			<px:PXControlParam ControlID="grid" Name="taskID" PropertyName="DataKey[&quot;TaskID&quot;]" Type="Int32" />
		</Parameters>
	</px:PXFormView>
	
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%"
		ActionsPosition="Top" SkinID="Inquire" OnLoad="grid_OnLoad" FilesIndicator="False" NoteIndicator="False">
		<Levels>
			<px:PXGridLevel DataMember="ReminderList">
				<Columns>
					<px:PXGridColumn DataField="ClassIcon" Width="30px" AllowUpdate="False" AllowShowHide="False" />
					<px:PXGridColumn DataField="Subject" AllowUpdate="False" AllowShowHide="False" LinkCommand="ViewActivity" />
					<px:PXGridColumn DataField="StartDate" DataType="DateTime" AllowUpdate="False" AllowShowHide="False" />
				</Columns>
				<RowTemplate>
					<px:PXSelector ID="edTaskId" runat="server" DataField="Subject" AllowEdit="true" />
				</RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="cmdViewActivity" ActionsVisible="false">
			<CustomItems>
				<px:PXToolBarButton Text="View" Key="cmdViewActivity">
					<AutoCallBack Command="ViewActivity" Enabled="True" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
			<Actions>
				<ExportExcel Enabled="false" />
				<AdjustColumns Enabled="False" />
				<FilterShow Enabled="False" />
				<FilterSet Enabled="False" />
				<PageFirst Enabled="False" />
				<PageLast Enabled="False" />
				<PagePrev Enabled="False" />
				<PageNext Enabled="False" />
			</Actions>
		</ActionBar>
		<AutoSize Enabled="true" Container="Window" />
		<%--<ClientEvents AfterRowChange="gridRowChanged" />--%>
	</px:PXGrid>

	<px:PXFormView ID="frmSnooze" runat="server" Width="100%" DataSourceID="ds" DataMember="DeferFilter" 
		OverflowY="Hidden" AllowAutoHide="True" SkinID="Transparent" Height="40px">
		<Template>
			<px:PXLayoutRule runat="server" ID="PXLayoutRule1" StartColumn="true" />
			<px:PXPanel runat="server" ID="panel1" RenderStyle="Simple" ContentLayout-OuterSpacing="Around" Style="float:left">
				<px:PXLayoutRule runat="server" ID="rule1" Merge="true" />
				<px:PXButton ID="btnSnooze" runat="server" Text="Snooze" Width="90px" AlignLeft="true" >
					<AutoCallBack Command="DeferCurrent" Enabled="true" Target="ds" />
				</px:PXButton>
				<px:PXDropDown ID="edSnooze" runat="server" DataField="Type" SuppressLabel="true" Size="SM" Height="26px" />
			</px:PXPanel>

			<px:PXPanel runat="server" ID="panel2" RenderStyle="Simple" ContentLayout-OuterSpacing="Around" Style="float:right">
				<px:PXLayoutRule runat="server" ID="rule2" Merge="true" />
				<px:PXButton ID="btnViewActivity" runat="server" Text="Open" Width="110px" AlignLeft="true">
					<AutoCallBack Command="ViewActivity" Enabled="true" Target="ds" />
					<PopupCommand Command="Refresh" Enabled="true" Target="ds" />
				</px:PXButton>
				<px:PXButton ID="btnDismissCurrent" runat="server" Text="Dismiss" Width="110px" >
					<AutoCallBack Command="DismissCurrent" Enabled="true" Target="ds" />
				</px:PXButton>
				<px:PXButton ID="btnDismissAll" runat="server" Text="Dismiss All" Width="110px" >
					<AutoCallBack Command="DismissAll" Enabled="true" Target="ds" />
				</px:PXButton>
			</px:PXPanel>
		</Template>
	</px:PXFormView>
</asp:content>
