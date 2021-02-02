<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM205030.aspx.cs" Inherits="Page_SM205030"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Schedule" TypeName="PX.SM.AUScheduleInq">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ViewScreen" CommitChanges="True" Visible="False" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="ViewHistory" CommitChanges="True" Visible="True" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="AUScheduleExt_View" CommitChanges="True" Visible="False" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="RunSchedule" CommitChanges="True" Visible="False" DependOnGrid="grid" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds"
		SkinID="Inquire" AutoAdjustColumns="True">
		<Levels>
			<px:PXGridLevel DataMember="Schedule" >
				<Columns>
					<px:PXGridColumn AllowUpdate="False" DataField="LastRunStatus" Width="40px" Type="Icon" TextAlign="Center" />
					<px:PXGridColumn DataField="ScreenID" DisplayFormat="CC.CC.CC.CC" Label="Screen ID" LinkCommand="AUScheduleExt_View" />
					<px:PXGridColumn DataField="Description" Label="Description" Width="200px" />
					<px:PXGridColumn AllowNull="False" DataField="IsActive" Label="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
					<px:PXGridColumn DataField="StartDate" Label="Starts On" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="EndDate" Label="Expires On" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="TimeZoneID" Label="TimeZone" Width="60px" />
					<px:PXGridColumn AllowUpdate="False" DataField="LastRunDate" DisplayFormat="g" Label="Last Executed On"	Width="110px" />
					<px:PXGridColumn AllowUpdate="False" DataField="LastRunResult" Label="Last Execution Result" Width="110px" />
					<px:PXGridColumn DataField="NextRunDateTime" Label="Next Execution Date" Width="120px" DisplayFormat="g" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar DefaultAction="View">
			<CustomItems>
				<px:PXToolBarButton Text="View Screen" Key="cmdViewScreen">
				    <AutoCallBack Command="ViewScreen" Target="ds" />
				</px:PXToolBarButton>
                <px:PXToolBarButton Text="View History" Key="cmdViewHistory">
				    <AutoCallBack Command="ViewHistory" Target="ds" />
				</px:PXToolBarButton>
                <px:PXToolBarButton Text="View" Key="View" Enabled="true">
                    <AutoCallBack Command="AUScheduleExt_View" Target="ds" />
                    <PopupCommand Command="Refresh" Target="grid" />
                    <ActionBar GroupIndex="0" Order="0" />
                </px:PXToolBarButton>
				<px:PXToolBarButton Text="Initialise Scheduler">
				    <AutoCallBack Command="RunSchedule" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
