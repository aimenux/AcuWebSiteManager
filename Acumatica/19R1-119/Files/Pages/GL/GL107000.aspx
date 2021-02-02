<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL107000.aspx.cs" Inherits="Page_GL107000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.GL.GLNumberCodeMaint" Visible="True"
		PrimaryView="NumberCodes">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" Visible="true" />
            <px:PXDSCallbackCommand Name="Insert" Visible="False" />			
			<px:PXDSCallbackCommand Name="Delete" Visible="False" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="Next" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="Previous" PostData="Self" Visible="False" />            
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" 
		AllowPaging="True"  AdjustPageSize="Auto" AllowSearch="True"
		SkinID="Primary" TabIndex="100">
		<Levels>
			<px:PXGridLevel DataKeyNames="NumberCode" DataMember="NumberCodes">
				<RowTemplate>
					<px:PXLayoutRule ID="PXLayoutRule1" runat='server' StartColumn='True' LabelsWidth="SM" 
						ControlSize = "M" />
					<px:PXTextEdit ID="edNumberCode" runat="server" DataField="NumberCode" 
						MaxLength="5">
					</px:PXTextEdit>
					<px:PXSelector ID="edNumberingID" runat="server" AutoRefresh="True" DataField="NumberingID" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn AutoGenerateOption="NotSet" DataField="NumberCode" Label="Unique Number. Code"
						MaxLength="5" Width="90px">
					</px:PXGridColumn>
					<px:PXGridColumn AutoGenerateOption="NotSet" DataField="NumberingID" Label="Numbering ID"
						MaxLength="10" Width="120px">
					</px:PXGridColumn>
				</Columns>
				<Mode InitNewRow="True" />
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowFormEdit="True" AllowUpload="True" />
		<LevelStyles>
			<RowForm Height="150px" Width="270px">
			</RowForm>
		</LevelStyles>
		<ActionBar>
			<Actions>
				<NoteShow Enabled="False" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
