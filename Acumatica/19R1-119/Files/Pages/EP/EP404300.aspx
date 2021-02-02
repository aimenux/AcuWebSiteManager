<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="EP404300.aspx.cs" Inherits="Page_EP404300" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="true" Width="100%"
		PrimaryView="Activities" TypeName="PX.Objects.EP.ActivitiesEnq">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="gridActivities" Name="viewEntity" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="gridActivities" Name="ViewActivity" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="gridActivities" Name="viewOwner" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phL" runat="server">
	<px:PXGrid ID="gridActivities" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top"
		AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" SkinID="PrimaryInquire" SyncPosition="True"
		Caption="Activities" RestrictFields="True" BlankFilterHeader="All Activities">
		<Levels>
			<px:PXGridLevel DataMember="Activities">
				 <RowTemplate>
					  <px:PXTimeSpan TimeMode="True" ID="edTimeSpent" runat="server" DataField="TimeSpent" InputMask="hh:mm" MaxHours="99" />
					  <px:PXTimeSpan TimeMode="True" ID="PXTimeSpan1" runat="server" DataField="OvertimeSpent" InputMask="hh:mm" MaxHours="99" />
					  <px:PXTimeSpan TimeMode="True" ID="PXTimeSpan2" runat="server" DataField="TimeBillable" InputMask="hh:mm" MaxHours="99" />
					  <px:PXTimeSpan TimeMode="True" ID="PXTimeSpan3" runat="server" DataField="OvertimeBillable" InputMask="hh:mm" MaxHours="99" />
				</RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="IsCompleteIcon" Width="20px" AllowShowHide="False" Label="Complete Icon" AllowResize="False" ForceExport="True" />
                    <px:PXGridColumn DataField="Subject" Width="300px" LinkCommand="ViewActivity" />
                    <px:PXGridColumn DataField="UIStatus" Width="90px" AllowNull="False" />
                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="130px" />
                    <px:PXGridColumn DataField="EndDate" DisplayFormat="g" Width="130px" />
                    <px:PXGridColumn DataField="DayOfWeek" Width="90px" />
                    <px:PXGridColumn DataField="TimeSpent" Width="100px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="OvertimeSpent" Width="100px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TimeBillable" Width="100px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="OvertimeBillable" Width="100px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="ClassID"/>
                    <px:PXGridColumn DataField="Type"/>
                    <px:PXGridColumn DataField="ProjectID" Width="90px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="ProjectTaskID" Width="90px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="WorkgroupID" Width="150px" SyncVisible="False" SyncVisibility="False" Visible="False" />
                    <px:PXGridColumn DataField="OwnerID" Width="150px" LinkCommand="viewOwner" SyncVisible="False" SyncVisibility="False" Visible="False" DisplayMode="Text" />
                    <px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="110px" SyncVisibility="False" SyncVisible="False" Visible="False" />
		            <px:PXGridColumn DataField="Source" Width="150px" LinkCommand="viewEntity" AllowSort="false" AllowFilter="false"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="DoubleClick" />
		<AutoSize Container="Window" Enabled="True" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
