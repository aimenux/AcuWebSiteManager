<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="WZ501000.aspx.cs" Inherits="Page_WZ501000" 
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="ScheduleList" TypeName="PX.Objects.WZ.WZScenarioActivateProcess">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Process" StartNewGroup="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="true" 
        Caption="Scenarios" DataSourceID="ds" BatchUpdate="True" AdjustPageSize="Auto"
		SkinID="PrimaryInquire">
		<Levels>
			<px:PXGridLevel DataMember="ScheduleList">
				<RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" Width="30px" />
					<px:PXGridColumn DataField="ScheduleID" TextAlign="Right" />
					<px:PXGridColumn DataField="ScheduleName" Width="180px" />
					<px:PXGridColumn DataField="StartDate" Width="140px" />
					<px:PXGridColumn DataField="EndDate" Width="140px" />
					<px:PXGridColumn DataField="RunCntr" Width="80px" />
					<px:PXGridColumn DataField="RunLimit" Width="80px" />
					<px:PXGridColumn DataField="LastRunDate" Width="140px" />
                    <px:PXGridColumn DataField="NextRunDate" Width="140px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />	
	</px:PXGrid>
</asp:Content>
