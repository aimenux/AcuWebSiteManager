 <%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL106000.aspx.cs" Inherits="Page_GL106000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.GL.GLTranCodeMaint" Visible="True"
		PrimaryView="TranCodes">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" 
		AllowPaging="True"  AdjustPageSize="Auto" AllowSearch="True"
		SkinID="Primary" TabIndex="100">
		<Levels>
			<px:PXGridLevel DataKeyNames="Module,TranType" DataMember="TranCodes">
				<RowTemplate>
					<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth="SM" 
						ControlSize = "M" />
					<px:PXDropDown ID="edModule" runat="server" DataField="Module" CommitChanges="true">
					</px:PXDropDown>
					<px:PXDropDown ID="edTranType" runat="server" AutoRefresh="True" CommitChanges="True" DataField="TranType" >
					    <Parameters>
							<px:PXControlParam ControlID="grid" Name="module" PropertyName="DataValues[&quot;Module&quot;]" Type="String" />
						</Parameters>
					</px:PXDropDown>					
					<px:PXTextEdit ID="edTranCode" runat="server" DataField="TranCode" 
						MaxLength="5">
					</px:PXTextEdit>
					<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" MaxLength="60">
					</px:PXTextEdit>
					<px:PXCheckBox ID="chkActive" runat="server" DataField="Active">
					</px:PXCheckBox>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn AutoGenerateOption="NotSet" DataField="Module" Label="Module" MaxLength="2" RenderEditorText="True" AutoCallBack="true">
					</px:PXGridColumn>
					<px:PXGridColumn AutoGenerateOption="NotSet" DataField="TranType" AutoCallBack="True" Label="Module Tran. Type" Type="DropDownList" MatrixMode="true">
					</px:PXGridColumn>
					<px:PXGridColumn AutoGenerateOption="NotSet" DataField="TranCode" Label="Unique Tran. Code" MaxLength="5">
					</px:PXGridColumn>
					<px:PXGridColumn AutoGenerateOption="NotSet" DataField="Descr" Label="Description" MaxLength="60">
					</px:PXGridColumn>
					<px:PXGridColumn  AutoGenerateOption="NotSet" DataField="Active" Label="Active" TextAlign="Center" Type="CheckBox">
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
