<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM205050.aspx.cs" Inherits="Page_SM205050" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:AUNotificationProcessDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter"
		TypeName="PX.SM.AUNotificationProcess" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Delete" />
			<px:PXDSCallbackCommand StartNewGroup="True" CommitChanges="True" Name="Process" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" />
			<px:PXDSCallbackCommand Name="viewNotification" DependOnGrid="grid" Visible="False" />
			<px:PXDSCallbackCommand Name="viewReport" DependOnGrid="grid" Visible="False" />
			<px:PXDSCallbackCommand Name="viewMessage" DependOnGrid="grid" Visible="False" />
		</CallbackCommands>
	</px:AUNotificationProcessDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlViewMessage" runat="server" CaptionVisible="True" Caption="View Message" Width="800px" Height="400px"
		Key="HistoryCurrent" AutoCallBack-Target="frmViewMessage" AutoCallBack-Command="Refresh">
		<px:PXFormView ID="frmViewMessage" runat="server" SkinID="Transparent" DataMember="HistoryCurrent"
			TemplateContainer="" DataSourceID="ds" >			
			<AutoSize Enabled="True" Container="Parent"/>
			<Template>
				<px:PXPanel ID="layoutPanel" runat="server">
				<px:PXLayoutRule ID="r1" runat="server" StartColumn="True" LabelsWidth="XS"	 />
				<px:PXTextEdit ID="edAddressTo" runat="server" DataField="MailTo" Size="XL" />
				<px:PXTextEdit ID="edAddressCc" runat="server" DataField="mailCc" />
				<px:PXTextEdit ID="edAddressBcc" runat="server" DataField="mailBcc" />
				<px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" />
				<px:PXLabel ID="lm" runat="server" />						
					</px:PXPanel>
				<px:PXHtmlView ID="rteBody"  runat="server" DataField="BodyVirtual" TextMode="MultiLine"  Width="100%">
					<AutoSize Enabled="True" Container="Parent"/>
				</px:PXHtmlView>
																														
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="pnlViewMessageBtn" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnSave" runat="server" DialogResult= "Cancel" Text="OK" />			
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" Caption="Selection"
		DataMember="Filter" TemplateContainer="">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="M" 
				LabelsWidth="SM" />
			<px:PXDropDown CommitChanges="True" runat="server" DataField="Status" AllowNull="False"
				ID="edStatus" Size="M" />
			<px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="ExecutionDate"
				ID="edExecutionDate" Size="M" />
			<px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="DateSent" 
				ID="edDateSent" Size="M" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%"
		Caption="Notifications" SkinID="Inquire" BatchUpdate="True"  AutoAdjustColumns="True">
		<Levels>
			<px:PXGridLevel DataMember="History">
				<RowTemplate>
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" />
					<px:PXDateTimeEdit ID="edExecutionDate" runat="server" DataField="ExecutionDate"
						DisplayFormat="g" />
					<px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status" />
					<px:PXDateTimeEdit ID="edDateSent" runat="server" DataField="DateSent" DisplayFormat="g" />
					<px:PXTextEdit ID="edMailTo" runat="server" DataField="MailTo" />
					<px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" />
					<px:PXMaskEdit ID="edReportID" runat="server" DataField="ReportID" InputMask="CC.CC.CC.CC" /></RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" Width="30px" />
					<px:PXGridColumn DataField="NotificationID" TextAlign="Left" Width="150px" TextField="NotificationID_description" />
					<px:PXGridColumn DataField="ExecutionDate" Width="110px" />
					<px:PXGridColumn AllowNull="False" DataField="Status" RenderEditorText="True" Width="60px" />
					<px:PXGridColumn DataField="DateSent" Width="110px" />
					<px:PXGridColumn DataField="MailTo" Width="108px" />
					<px:PXGridColumn DataField="Subject" Width="500px" />
					<px:PXGridColumn DataField="ReportID" Width="80px" />
					<px:PXGridColumn DataField="Exception" Width="100px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton Text="View Notification" CommandName="viewNotification" CommandSourceID="ds" />
				<px:PXToolBarButton Text="View Message" CommandName="viewMessage" CommandSourceID="ds"/>
				<px:PXToolBarButton Text="View Report" CommandName="viewReport" CommandSourceID="ds" StateColumn="ReportID"/>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
