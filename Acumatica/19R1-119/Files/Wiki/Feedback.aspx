<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="Feedback.aspx.cs" Inherits="Pages_Feedback"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.KBFeedbackMaint" PrimaryView="Responses">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Submit" CommitChanges="True" Visible="True" ClosePopup="True"/>			
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="formview" runat="server" DataMember="Responses" FilesIndicator="True" NoteIndicator="False" Width="100%">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="400px" ControlSize="XXL"/>
				<px:PXSelector ID="edFeedbackID" runat="server" DataField="FeedbackID" Visible="false"/>
                <px:PXDropDown ID="edIsFind" runat="server" DataField="IsFind" Size="S" />
				<px:PXDropDown ID="edSatisfaction" runat="server" DataField="Satisfaction" Size="M"/>
                <px:PXLabel ID="PXLabel" runat="server"/>
                <px:PXTextEdit ID="edSummary1" runat="server" DataField="Summary" TextMode="MultiLine" Height="200px" Width="100%" SuppressLabel="true" style="resize: none"/>                			
			</Template>
			<AutoSize Enabled="True" Container="Window" />
		</px:PXFormView> 
</asp:Content> 
