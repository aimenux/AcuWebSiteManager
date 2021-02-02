<%@ Page Title="Untitled Page" Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" CodeFile="SM205070.aspx.cs" Inherits="Pages_SM_SM205070" ValidateRequest="False" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource runat="server"
		ID="ds"
		Visible="True"
		TypeName="PX.SM.PerformanceMonitorMaint"
		PrimaryView="Filter"
		Width="100%">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="actionFlushSamples" RepaintControls="Bound" />
			<px:PXDSCallbackCommand Name="actionClearSamples" RepaintControls="Bound" />
			<px:PXDSCallbackCommand Name="actionViewScreen" DependOnGrid="grid" Visible="False" />

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView runat="server"
		ID="form"
		Width="100%"
		DataMember="Filter" CaptionVisible="False">
		<Template>
			<px:PXLayoutRule ID="Column0" runat="server" StartColumn="True" GroupCaption="Profiler Options" SuppressLabel="True" />
			<px:PXCheckBox runat="server" ID="SqlProfiler" DataField="SqlProfiler" CommitChanges="True" />
			<px:PXCheckBox runat="server" ID="SqlProfilerStackTrace" DataField="SqlProfilerStackTrace" CommitChanges="True" />
			<px:PXCheckBox runat="server" ID="TraceEnabled" DataField="TraceEnabled" CommitChanges="True" />
			<px:PXCheckBox runat="server" ID="TraceExceptionsEnabled" DataField="TraceExceptionsEnabled" CommitChanges="True" />

			<px:PXLayoutRule ID="Column1" runat="server" StartColumn="True" GroupCaption="Filters" />
			<px:PXTextEdit runat="server" DataField="ScreenId" ID="edScreenId" CommitChanges="True" LabelWidth="135" />
			<px:PXTextEdit runat="server" DataField="UserId" ID="edUserId" CommitChanges="True" LabelWidth="135" />
			<px:PXTextEdit runat="server" DataField="TimeLimit" ID="edTimeLimit" CommitChanges="True" LabelWidth="135" />
			<px:PXTextEdit runat="server" DataField="SqlCounterLimit" ID="SqlCounterLimit" CommitChanges="True" LabelWidth="135" />
		</Template>
	</px:PXFormView>
</asp:Content>



<asp:Content ID="Content3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid runat="server" ID="grid" SkinID="Details" Width="100%" Height="400px"
		SyncPosition="True" NoteIndicator="False" FilesIndicator="False"
		AutoGenerateColumns="Append" AutoAdjustColumns="True" AllowPaging="True" AdjustPageSize="Auto">
		<Levels>
			<px:PXGridLevel DataMember="Samples">
				<Columns>
					<px:PXGridColumn DataField="RequestStartTime" DisplayFormat="dd MMM HH:mm" />
					<px:PXGridColumn DataField="UserId" />
					<px:PXGridColumn DataField="ScreenId" Width="170" LinkCommand="actionViewScreen" />
					<px:PXGridColumn DataField="CommandTarget" />
					<px:PXGridColumn DataField="CommandName" />
					<px:PXGridColumn DataField="ScriptTimeMs" />
					<px:PXGridColumn DataField="RequestTimeMs" />
					<px:PXGridColumn DataField="SelectTimeMs" AllowShowHide="True" SyncVisible="False" Visible="False" />
					<px:PXGridColumn DataField="SqlTimeMs" />
					<px:PXGridColumn DataField="RequestCpuTimeMs" />
					<px:PXGridColumn DataField="SqlCounter" />
					<px:PXGridColumn DataField="SelectCounter" AllowShowHide="True" SyncVisible="False" Visible="False" />

					<px:PXGridColumn DataField="MemBefore" DisplayFormat="0,0" />
					<px:PXGridColumn DataField="MemDelta" DisplayFormat="0,0" AllowShowHide="True" SyncVisible="False" Visible="False" />
					<px:PXGridColumn DataField="SessionLoadTimeMs" AllowShowHide="True" SyncVisible="False" Visible="False" />
					<px:PXGridColumn DataField="SessionSaveTimeMs" AllowShowHide="True" SyncVisible="False" Visible="False" />
					<px:PXGridColumn DataField="Headers" AllowShowHide="True" SyncVisible="False" Visible="False" />
				</Columns>

			</px:PXGridLevel>
		</Levels>
		<%--        <CallbackCommands>
            <Refresh RepaintControls="Bound" />
        </CallbackCommands>--%>
		<AutoSize Enabled="True" Container="Window" />
		<ActionBar PagerVisible="False" DefaultAction="buttonSql">
			<CustomItems>
				<px:PXToolBarButton Text="SQL" PopupPanel="PanelSqlProfiler" Key="buttonSql" />
				<px:PXToolBarButton Text="Trace" PopupPanel="PanelTraceProfiler" Key="buttonTrace" />
				<px:PXToolBarButton Text="Open URL" Key="ViewScreen">
					<AutoCallBack Target="ds" Command="actionViewScreen"></AutoCallBack>
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>


	</px:PXGrid>

	<px:PXSmartPanel runat="server" ID="PanelSqlProfiler" Width="90%" Height="650px"
		ShowMaximizeButton="True"
		CaptionVisible="True"
		Caption="SQL Profiler"
		AutoRepaint="True" Key="SqlSamples">


		<px:PXGrid runat="server" ID="GridProfiler"
			Width="100%"
			SkinID="Details"
			AdjustPageSize="Auto"
			AllowPaging="True" FeedbackMode="DisableAll">
			<Mode AllowFormEdit="True"></Mode>
			<Levels>

				<px:PXGridLevel DataMember="Sql">
					<Columns>
						<px:PXGridColumn DataField="QueryOrderID" />
						<px:PXGridColumn DataField="TableList" Width="300" />
						<px:PXGridColumn DataField="NRows" />
						<px:PXGridColumn DataField="RequestStartTime" />
						<px:PXGridColumn DataField="SqlTimeMs" />
						<px:PXGridColumn DataField="ShortParams" Width="250" />


					</Columns>
					<RowTemplate>
						<px:PXLayoutRule runat="server" />

						<px:PXTextEdit runat="server" ID="tables" DataField="TableList" SelectOnFocus="False" Width="600px" />

						<px:PXTextEdit runat="server" ID="SqlText" DataField="SQLWithStackTrace" SelectOnFocus="False" TextMode="MultiLine" Width="600px" Height="490px" />

					</RowTemplate>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="True" Container="Parent" />
			<ActionBar PagerVisible="False" />

		</px:PXGrid>
	</px:PXSmartPanel>

	<px:PXSmartPanel runat="server" ID="PanelTraceProfiler" Width="90%" Height="650px"
		ShowMaximizeButton="True"
		CaptionVisible="True"
		Caption="Trace Profiler"
		AutoRepaint="True" Key="TraceEvents">


		<px:PXGrid runat="server" ID="TraceEventsGrid" FeedbackMode="DisableAll"
			Width="100%"
			SkinID="Details"
			PageSize="25"
			AllowPaging="True">
			<Mode AllowFormEdit="True"></Mode>
			<Levels>

				<px:PXGridLevel DataMember="TraceMessage">
					<Columns>
						<px:PXGridColumn DataField="RequestStartTime" Width="50px" />
						<px:PXGridColumn DataField="Source" Width="60px" />
						<px:PXGridColumn DataField="TraceType" Width="60px" />
						<px:PXGridColumn DataField="ShortMessage" Width="250px" />


					</Columns>
					<RowTemplate>
						<px:PXLayoutRule runat="server" />

						<px:PXTextEdit runat="server" ID="MessageText" DataField="MessageWithStackTrace" SelectOnFocus="False" TextMode="MultiLine" Width="600px" Height="490px" />

					</RowTemplate>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="True" Container="Parent" />
			<ActionBar PagerVisible="False" />

		</px:PXGrid>
	</px:PXSmartPanel>

</asp:Content>

