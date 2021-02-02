<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM200535.aspx.cs" Inherits="Page_SM200535"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.SM.CertificateChangeProcess"
		PageLoadBehavior="PopulateSavedValues" Visible="True" PrimaryView="Filter">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Process" StartNewGroup="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Caption="Encryption Certificate Selection" CaptionAlign="Justify" DataMember="Filter"
		NoteField="" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" 
				ControlSize="M" />
			<px:PXSelector ID="edPendingCertificate" runat="server" DataField="PendingCertificate"
				DataSourceID="ds">
			</px:PXSelector>
			<px:PXSelector ID="edCurrentCertificate" runat="server" DataField="CurrentCertificate"
				DataSourceID="ds">
			</px:PXSelector>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="144px" Style="z-index: 100; left: 0px;
		top: 0px;" Width="100%" AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True"
		BatchUpdate="True" Caption="Encrypted Entities" DataSourceID="ds" SkinID="Inquire">
		<Levels>
			<px:PXGridLevel DataMember="Process">
				<Columns>
					<px:PXGridColumn DataField="EntityType" Label="Table Name" Width="220px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="EntityName" Label="Table Name" Width="300px">
					</px:PXGridColumn>
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
		<ActionBar>
			<Actions>
			    <ExportExcel Enabled="True" GroupIndex="0" />
                <AdjustColumns Enabled="True" GroupIndex="0" />
                <Refresh Enabled="False" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
