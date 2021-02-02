<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="EP404100.aspx.cs" Inherits="Pages_EP404100"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<script type="text/javascript">
		function refreshTasksAndEvents(sender, args)
		{
			var top = window.top;
			if (top != window && top.MainFrame != null) top.MainFrame.refreshEventsInfo();
		}
	</script>

    <px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="true" Width="100%"
                     PrimaryView="Filter" TypeName="PX.Objects.EP.EPEventEnq" PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
			<px:PXDSCallbackCommand Name="ViewEvent" Visible="false" />
            <px:PXDSCallbackCommand DependOnGrid="gridEvents" Name="complete" RepaintControls="All" />
			<px:PXDSCallbackCommand DependOnGrid="gridEvents" Name="cancelActivity" RepaintControls="All" />
        </CallbackCommands>
    </px:PXDataSource>
		
	
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Caption="Selection"
        Style="z-index: 100" Width="100%">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
			<px:PXSelector ID="edOwnerID" runat="server" DataKeyNames="AcctCD" 
				Size="XM" DataField="OwnerID" CommitChanges="True" FilterByAllFields="True">
				<GridProperties>
					<Columns>
						<px:PXGridColumn DataField="AcctCD" MaxLength="64">
							<Header Text="User Name">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowUpdate="False" DataField="AcctName" MaxLength="10">
							<Header Text="Employee Name">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowUpdate="False" DataField="DepartmentID" MaxLength="10">
							<Header Text="Department ID">
							</Header>
						</px:PXGridColumn>
					</Columns>
					<Layout ColumnsMenu="False" />
				</GridProperties>
			</px:PXSelector> 
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
	<px:PXGrid ID="gridEvents" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top"
		Caption="Events" OnRowDataBound="grid_RowDataBound" NoteField="NoteText" FilesField="NoteFiles" SyncPosition="True"
		MatrixMode="true" BlankFilterHeader="All Events" AdjustPageSize="Auto" SkinID="PrimaryInquire" FastFilterFields="Subject" RestrictFields="False">
		<ClientEvents AfterRefresh="refreshTasksAndEvents" />
		<Levels>
			<px:PXGridLevel DataMember="Events">
				<Columns>
					<px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" Label="Priority Icon"
						AllowResize="False" ForceExport="True" />
				    <px:PXGridColumn DataField="CRReminder__ReminderIcon" Width="26px" AllowShowHide="False" Label="Reminder Icon" AllowResize="False" ForceExport="True" />
					<px:PXGridColumn DataField="Subject" LinkCommand="ViewEvent" />
					<px:PXGridColumn DataField="UIStatus" AllowNull="False"  />
					<px:PXGridColumn DataField="DayOfWeek" />
					<px:PXGridColumn DataField="StartDate_Date" DataType="DateTime" DisplayFormat="d" />
					<px:PXGridColumn DataField="StartDate_Time" DataType="DateTime" DisplayFormat="t" />
					<px:PXGridColumn DataField="EndDate_Time" DataType="DateTime" DisplayFormat="t" />
					<px:PXGridColumn DataField="OwnerID" LinkCommand="viewOwner" SyncVisible="False"
						SyncVisibility="False" Visible="False" DisplayMode="Text" />
					<px:PXGridColumn DataField="Source" LinkCommand="viewEntity" AllowSort="false" AllowFilter="false"/>
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Parent" Enabled="True" />
		<ActionBar DefaultAction="DoubleClick" PagerVisible="False">
			<Actions>
				<AddNew Enabled="False" />
				<Delete Enabled="False" />
				<EditRecord Enabled="False" />
				<PageFirst Enabled="true" />
			</Actions>
		</ActionBar>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
