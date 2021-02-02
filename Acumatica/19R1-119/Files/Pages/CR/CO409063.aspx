<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CO409063.aspx.cs" Inherits="Pages_CR_CO409063"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="AnnouncementsDetails"
		TypeName="PX.Objects.CR.CRCommunicationAnnouncementPreview">
	</px:PXDataSource>
</asp:Content>	

<asp:Content ID="Filter" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="formview" runat="server" CaptionVisible="False" DataMember="AnnouncementsDetails"
			Height="500px" Width="800px">
			<ContentStyle BackColor="White">
			</ContentStyle>
			<Template>
				<px:PXHtmlView ID="PXHtmlView1" runat="server" DataField="Body" TextMode="MultiLine"
					MaxLength="50" Width="100%" Height="100%" SkinID="Label" >
                    <AutoSize Container="Parent" Enabled="true" />
                    </px:PXHtmlView>
			</Template>
		</px:PXFormView>
</asp:Content> 



