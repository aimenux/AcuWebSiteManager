<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM200530.aspx.cs" Inherits="Page_SM204200"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.SM.CertificateMaintenance"
		PrimaryView="Certificates" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowSearch="True" DataSourceID="ds" SkinID="Primary" FilesIndicator="true">
		<Levels>
			<px:PXGridLevel DataMember="Certificates">
				<Columns>
					<px:PXGridColumn DataField="Name" Width="250px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Password" Visible="False" Width="250px">
					</px:PXGridColumn>
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ColumnWidth="L" />
					<px:PXTextEdit Size="l" ID="edName" runat="server" DataField="Name">
					</px:PXTextEdit>
					<px:PXTextEdit Size="l" ID="edPassword" runat="server" DataField="Password">
					</px:PXTextEdit>
				</RowTemplate>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowFormEdit="True" />
		<LevelStyles>
			<RowForm Height="100px" Width="500px">
			</RowForm>
		</LevelStyles>
	</px:PXGrid>
</asp:Content>
