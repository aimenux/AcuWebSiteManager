<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL202000.aspx.cs" Inherits="Page_GL202000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.GL.AccountClassMaint"
		PrimaryView="AccountClassRecords" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowPaging="True"
		AdjustPageSize="Auto" AllowSearch="True" SkinID="Primary" FastFilterFields="AccountClassID,Descr">
		<Levels>
			<px:PXGridLevel DataMember="AccountClassRecords">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXMaskEdit ID="edAccountClassID" runat="server" DataField="AccountClassID" />
				    <px:PXDropDown ID="Type" runat="server" DataField="Type"/>
					<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="AccountClassID" />
					<px:PXGridColumn DataField="Type" Type="DropDownList" />
					<px:PXGridColumn DataField="Descr" />
				</Columns>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowFormEdit="True" AllowUpload="True" />
		<LevelStyles>
			<RowForm Height="80px" Width="250px">
			</RowForm>
		</LevelStyles>
		<ActionBar>
			<Actions>
				<NoteShow Enabled="False" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
