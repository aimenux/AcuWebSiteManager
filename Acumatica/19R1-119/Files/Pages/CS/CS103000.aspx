<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS103000.aspx.cs" Inherits="Page_CS103000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CS.AddressValidatorPluginMaint" PrimaryView="Plugin">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="Certify" CommitChanges="true" PostData="Page" Visible="false" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Tax Plug-in Summary" DataMember="Plugin" FilesIndicator="True" NoteIndicator="True" TemplateContainer=""
		TabIndex="5100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edAddressValidatorPluginID" runat="server" DataField="AddressValidatorPluginID" DataSourceID="ds" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXSelector CommitChanges="True" ID="edPluginTypeName" runat="server" DataField="PluginTypeName" DataSourceID="ds" />
			<px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="365px">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="Plug-in Parameters">
				<Template>
					<px:PXGrid ID="PXGridSettings" runat="server" DataSourceID="ds" AllowFilter="False" Width="100%" SkinID="DetailsInTab" Height="100%" MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="Details">
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXMaskEdit ID="edSettingID" runat="server" DataField="SettingID" />
									<px:PXTextEdit ID="edDescription" runat="server" AllowNull="False" DataField="Description" />
									<px:PXTextEdit ID="edValue" runat="server" DataField="Value" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="SettingID" Width="90px" />
									<px:PXGridColumn AllowNull="False" DataField="Description" Width="300px" />
									<px:PXGridColumn DataField="Value" Width="300px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowAddNew="False" AllowDelete="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>			
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="180" />
	</px:PXTab>
</asp:Content>
