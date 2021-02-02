<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR307010.aspx.cs" Inherits="CR307010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Responses"
		TypeName="PX.Objects.CR.KBResponseMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" PopupVisible="True" ClosePopup="True" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXLayoutRule ID="PXLayoutRule3" runat="server"/>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" SkinID="Transparent" Width="100%" DataMember="Responses">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="SM"/>
			<px:PXDropDown ID="edMark" runat="server" DataField="Mark" AllowNull="False" 
				MaxLength="255" Width="100px"/>
			<px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date" Size="SM"/>
			<px:PXSelector ID="edArticle" runat="server" DataField="PageID" 
				TextField="Title" DataSourceID="ds"  />
			<px:PXTextEdit ID="edLanguage" runat="server" DataField="Language_DisplayName"  />
			<px:PXTextEdit ID="edRevision" runat="server" DataField="RevisionID" Size="XXS"/> 
		</Template>
	</px:PXFormView>
	<px:PXFormView ID="PXFormView2" runat="server" DataSourceID="ds" SkinID="Transparent"
		Width="100%"  DataMember="Responses" Style="margin-top: 9px;">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="XS"/>
			<px:PXTextEdit ID="edSummary" runat="server" DataField="Summary"
				  Height="117px" TextMode="MultiLine" Width="100%" >
			<AutoSize Enabled="True" Container="Window" />
			</px:PXTextEdit>
		</Template>
		<ContentLayout AutoSizeControls = "True"/> 
	</px:PXFormView>
</asp:Content>
