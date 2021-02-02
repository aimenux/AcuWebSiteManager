<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="PM203000.aspx.cs" Inherits="Page_PM203000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%"
		PrimaryView="Item" TypeName="PX.Objects.PM.ChangeOrderClassMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />			
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="Item" Caption="Change Order Class Summary" FilesIndicator="True" NoteIndicator="True"
		ActivityIndicator="true" ActivityField="NoteActivity" DefaultControlID="edChangeOrderClassID">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" />
            <px:PXSelector ID="edChangeOrderClassID" runat="server" DataField="ClassID" FilterByAllFields="True" />
             <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="true" />
            <px:PXCheckBox ID="chkIsActive" runat="server" DataField="IsActive" />

            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartRow="true" ColumnSpan="2" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXCheckBox runat="server" ID="edIsAdvance" DataField="IsAdvance" CommitChanges="True" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="253px" DataSourceID="ds" DataMember="ItemSettings"
		LoadOnDemand="True">
		<Items>
			<px:PXTabItem Text="Details" RepaintOnDemand="False">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule7" runat="server" LabelsWidth="XM" ControlSize="XM" />
					<px:PXCheckBox  AlignLeft="true" ID="chkIsCostBudgetEnabled"  runat="server" DataField="IsCostBudgetEnabled" CommitChanges="True" />
					<px:PXCheckBox AlignLeft="true" ID="chkIsRevenueBudgetEnabled" runat="server" DataField="IsRevenueBudgetEnabled" CommitChanges="True" />
                    <px:PXCheckBox AlignLeft="true" ID="chkIsPurchaseOrderEnabled" runat="server" DataField="IsPurchaseOrderEnabled" CommitChanges="True" />
                    
                </Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Attributes">
				<Template>
					<px:PXGrid ID="AttributesGrid" runat="server" SkinID="Details" ActionsPosition="Top"
						DataSourceID="ds" Width="100%" BorderWidth="0px" MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="Mapping">
								<RowTemplate>
									<px:PXSelector CommitChanges="True" ID="edAttributeID" runat="server" DataField="AttributeID" AllowEdit="True" FilterByAllFields="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="AttributeID" AutoCallBack="True" LinkCommand="CRAttribute_ViewDetails" />
									<px:PXGridColumn AllowNull="False" DataField="Description" />
									<px:PXGridColumn DataField="SortOrder" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" DataField="Required" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="True" DataField="CSAttribute__IsInternal" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList" />
                                    <px:PXGridColumn AllowNull="True" DataField="DefaultValue" RenderEditorText="False" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
