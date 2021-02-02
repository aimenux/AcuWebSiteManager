<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CO409062.aspx.cs" Inherits="Pages_CR_CR409062"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Announcement"
		TypeName="PX.Objects.CR.CRAnnouncementMaint">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content> 
<asp:Content ID="Content1" ContentPlaceHolderID="phF" runat="Server">	
	<px:PXFormView ID="PXFormView1" runat="server" DataSourceID="ds" DataMember="Announcement" 
	Caption="Selection" Width="100%"  DefaultControlID="edannouncementsID">
        <Template>
        	<px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple" Style="margin: 10px; padding: 10px;"> 
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />

        	<px:PXSelector runat="server" ID="edannouncementsID" DataField="AnnouncementsID"  
			ValueField="AnnouncementsID" DisplayMode="Text" TextMode="Search" NullText="<NEW>">  
			<GridProperties>
				<Columns>
					<px:PXGridColumn DataField="Subject" Width="500px"/>  
					<px:PXGridColumn DataField="Status" Width="80px"/>   
					<px:PXGridColumn DataField="PublishedDateTime" Width="120px"/>
					<px:PXGridColumn DataField="AnnouncementsID" Width="0px" />
				</Columns>
			</GridProperties>
			</px:PXSelector> 
			<px:PXSelector runat="server" ID="edCategory" DataField="Category"> 
			<GridProperties>
				<Columns>
					<px:PXGridColumn DataField="Category" Width="120px"/> 
				</Columns>
			</GridProperties>
			</px:PXSelector> 
        	<px:PXCheckBox runat="server" ID="edPortalVisible" DataField="IsPortalVisible"/>
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server" LabelsWidth="S" ColumnSpan="2" />
			<px:PXTextEdit runat="server" ID="edsubject" DataField="Subject"/> 

			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
        	<px:PXDropDown runat="server" ID="edstatus" DataField="Status">
				<AutoCallBack Command="Save" Enabled="True" Target="PXFormView1"/> 
			</px:PXDropDown>
			<px:PXDateTimeEdit runat="server"  ID="edpublishedDateTime" DataField="PublishedDateTime" Enabled="False" DisplayFormat="g" EditFormat="d"  Size="SM"/> 
		    </px:PXPanel>
			<px:PXRichTextEdit ID="PXRichTextEdit" runat="server" DataField="Body" TextMode="MultiLine"
				MaxLength="50" Style="border-top: 1px solid #BBBBBB; border-left: 0px; border-right: 0px;
				margin: 0px; padding: 0px; width: 100%;">
				<AutoSize Container="Window" Enabled="True"></AutoSize>
			</px:PXRichTextEdit>
		</Template>
	</px:PXFormView>
</asp:Content>
