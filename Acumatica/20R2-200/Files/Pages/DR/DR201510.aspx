<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="DR201510.aspx.cs" Inherits="Pages_DR_DR201510" Title="Deferred Schedules" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.DR.DRSchedulePrimary" PrimaryView="Items">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="ViewSchedule" DependOnGrid="grid" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewDoc" DependOnGrid="grid" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" AdjustPageSize="Auto"
		AllowPaging="True" Caption="Time Cards" FastFilterFields="BAccountID,RefNbr" SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<Columns>
					<px:PXGridColumn DataField="ScheduleNbr" LinkCommand="ViewSchedule" />
					<px:PXGridColumn DataField="Status" />
					<px:PXGridColumn DataField="BAccountType" />
					<px:PXGridColumn DataField="BAccountID" />
					<px:PXGridColumn DataField="DocumentTypeEx" />
					<px:PXGridColumn DataField="RefNbr" LinkCommand="ViewDoc" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowUpdate="False" AllowDelete="False" />
	</px:PXGrid>
</asp:Content>
