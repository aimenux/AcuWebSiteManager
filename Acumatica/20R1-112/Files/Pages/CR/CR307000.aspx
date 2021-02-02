<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR307000.aspx.cs" Inherits="CR307000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Message"
		TypeName="PX.Objects.CR.SendKBArticleMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="send" CommitChanges="True" PopupVisible="true" ClosePopup="true" />
			<px:PXDSCallbackCommand Name="InsertArticle" CommitChanges="True" PopupVisible="true" StartNewGroup="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXLayoutRule ID="PXLayoutRule3" runat="server" />								
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Message" CaptionVisible="False" SkinID="Transparent">
		<Template>
		<px:PXPanel ID="PXPanel1" runat="server" >
		<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XXL" />								
		<px:PXSelector ID="edMailAccountID" runat="server" DataField="MailAccountID" ReadOnly="true"  />
		<px:PXTextEdit ID="edMailTo" runat="server" DataField="MailTo" />
		<px:PXTextEdit ID="edMailCc" runat="server" DataField="MailCc" />
		<px:PXTextEdit ID="edMailBcc" runat="server" DataField="MailBcc" />
		<px:PXTextEdit CommitChanges="True" ID="edSubject" runat="server" DataField="Subject" />
		</px:PXPanel> 
		<px:PXWikiEdit ID="wikiEdit" runat="server" DataField="WikiText" WysiWygMode="true" Width="100%" >
			<AutoSize Enabled="True" Container="Window" />
		</px:PXWikiEdit>
		</Template>
		<ContentLayout OuterSpacing="None"/>
		<AutoSize Enabled="True" Container="Window" />
	</px:PXFormView>
	<px:PXSmartPanel ID="pnlInsertArticle" runat="server" Caption="Select Article" Key="InsertArticleFilter" 
		CaptionVisible="true" LoadOnDemand="true" Width="640px" Height="360px" DesignView="Content" 
		AutoCallBack-Enabled="true" AutoCallBack-Command="Refresh" AutoCallBack-Target="frmInsertArticle" >
		<px:PXFormView ID="frmInsertArticle" runat="server" DataMember="InsertArticleFilter" DataSourceID="ds"
			SkinID="Transparent" Width="100%" >
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
			    <px:PXSelector CommitChanges="True" ID="edArticle" runat="server"  DataField="ArticleID"  
					TextField="Title">
					<GridProperties>
					    <PagerSettings Mode="NextPrevFirstLast" />
                    </GridProperties>
				</px:PXSelector>
				<px:PXLabel ID="lblContent" runat="server" 
					>Content:</px:PXLabel>
				<pxa:KB.PXKBShow ID="edContent" runat="server" DataField="Content" TextMode="MultiLine" 
					 ReadOnly="true">
					<AutoSize Enabled="true" />
				</pxa:KB.PXKBShow>
            </Template>
			<AutoSize Enabled="true" />
		</px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnOk" runat="server" Text="OK" DialogResult="OK" />
			<px:PXButton ID="btnCancel" runat="server" Text="Cancel" DialogResult="Cancel" />
        </px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
