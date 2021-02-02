<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SP500000.aspx.cs" Inherits="Pages_NewComment"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="SP.Objects.SP.SPComment" PrimaryView="Comment">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="SaveClose" CommitChanges="True" Visible="True" ClosePopup="True"/>  
			<px:PXDSCallbackCommand Name="Close" Visible="True" ClosePopup="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="formview" runat="server" DataMember="Comment" 
		FilesIndicator="True" NoteIndicator="False" Width="100%" AllowCollapse="False">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" SuppressLabel="False" ControlSize="M"/>
				<px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" Visible="False"/>
				<px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="True"/>				
				<px:PXSelector ID="edType" runat="server" DataField="CommentType"/>
				<px:PXDateTimeEdit ID="edCommentStartDate" runat="server" DataField="CommentStartDate" DisplayMode="Text"/>	
				<px:PXLayoutRule ID="PXLayoutRule7" runat="server"/>
				<px:PXTextEdit ID="edSubject" runat="server" DataField="CommentSubject" Width="530"/>
				<px:PXTextEdit ID="edBody" runat="server" DataField="CommentBody" TextMode="MultiLine" Width="650" SuppressLabel="True" style="resize: none">
				<AutoSize Enabled="True" Container="Window" />
				</px:PXTextEdit>
			</Template>			
		</px:PXFormView> 
</asp:Content> 
