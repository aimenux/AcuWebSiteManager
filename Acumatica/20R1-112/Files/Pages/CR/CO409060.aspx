<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CO409060.aspx.cs" Inherits="Pages_CR_CR409060"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Filter"
		TypeName="PX.Objects.CR.CRCommunicationAnnouncement">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="gridAnnouncements" Name="ViewDetails" Visible="False" /> 
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>	

<asp:Content ID="Filter" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Filter" Caption="Filter">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
			<px:PXSelector ID="edCategoryID" runat="server" DataField="Category" DataSourceID="ds">
				<GridProperties>
				<Columns>
					<px:PXGridColumn DataField="Category" Width="120px"/> 
				</Columns>
				</GridProperties>	
				<AutoCallBack Command="Save" Target="form">
				</AutoCallBack>
			</px:PXSelector> 

			<px:PXSelector ID="edCreatedByID" runat="server" DataField="CreatedByID" DataSourceID="ds" DisplayMode="Text">
				<AutoCallBack Command="Save" Target="form" />
			</px:PXSelector>
		</Template>	
	</px:PXFormView>
	</asp:Content> 

<asp:Content ID="cont2" ContentPlaceHolderID="phG" runat="Server">
	<%--<px:PXSmartPanel ID="PanelView" runat="server" AcceptButtonID="PXButtonOK" AutoReload="true"
		CancelButtonID="PXButtonCancel" Caption="Announcement Viewer" CaptionVisible="True"
		ShowMaximizeButton="true" DesignView="Content" HideAfterAction="false" Key="AnnouncementsDetails"
		LoadOnDemand="true" Height="500px" Width="800px">
		<px:PXFormView ID="formview" runat="server" CaptionVisible="False" DataMember="AnnouncementsDetails"
			Width="100%" Height="100%">
			<ContentStyle BackColor="White">
			</ContentStyle>
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" />
				<pxa:PXHtmlView ID="PXHtmlView1" runat="server" DataField="Body" TextMode="MultiLine"
					MaxLength="50" Width="100%" Height="100%" SkinID="Label" />
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>--%>
	
	<px:PXGrid ID="gridAnnouncements" runat="server" DataSourceID="ds" ActionsPosition="Top"
		AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" SkinID="Inquire"
		Width="100%" MatrixMode="True" FastFilterFields="Subject" FilesIndicator="False"
		NoteIndicator="False" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="Announcements">
				<Columns>
					<px:PXGridColumn DataField="Subject" Width="400px" LinkCommand="ViewDetails"/>
					<px:PXGridColumn DataField="Category" Width="120px" />  
					<px:PXGridColumn AllowUpdate="False" DataField="PublishedDateTime" Width="120px" />	
					<px:PXGridColumn AllowUpdate="False" DataField="CreatedDateTime" Width="120px" />  
					<px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username" Width="120px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="cmd_ViewDetails" PagerVisible="False">
			<CustomItems>
				<px:PXToolBarButton Key="cmd_ViewDetails" Visible="False">
					<ActionBar GroupIndex="0" />
					<AutoCallBack Command="ViewDetails" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
