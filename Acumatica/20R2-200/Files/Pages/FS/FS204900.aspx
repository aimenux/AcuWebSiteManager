<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS204900.aspx.cs" Inherits="Page_FS204900" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        TypeName="PX.Objects.FS.ServiceTemplateMaint" 
        PrimaryView="ServiceTemplateRecords">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" FilesIndicator="true" 
		Width="100%" Height="100px" DataMember="ServiceTemplateRecords" TabIndex="1300" DefaultControlID="edServiceTemplateCD">
        <Template>
            <px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="SM">
            </px:PXLayoutRule>
            <px:PXSelector ID="edServiceTemplateCD" runat="server" 
                DataField="ServiceTemplateCD">
            </px:PXSelector>
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr">
            </px:PXTextEdit>
            <px:PXSelector ID="edSrvOrdType" runat="server" DataField="SrvOrdType" AllowEdit="True" AutoRefresh="True" CommitChanges="True">
            </px:PXSelector>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="100%" DataSourceID="ds" 
        DataMember="ServiceTemplateDetails" Style="z-index: 100">
        <Items>
            <px:PXTabItem Text="Details">
                <Template>
                   <px:PXGrid ID="PXGridService" runat="server" AutoAdjustColumns="True" 
                        DataSourceID="ds" Height="530px" SkinID="Details" TabIndex="4200" Width="100%" SyncPosition="True">
		                <Levels>
			                <px:PXGridLevel DataKeyNames="ServiceTemplateID,ServiceTemplateDetID" 
                                DataMember="ServiceTemplateDetails">
			                    <RowTemplate>
                                    <px:PXDropDown ID="edLineType" runat="server" DataField="LineType" 
                                        CommitChanges="True">
                                    </px:PXDropDown>
                                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" CommitChanges="True"
                                             AutoRefresh="True" AllowEdit="True">
                                    </px:PXSegmentMask>
                                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty"
                                        CommitChanges="True">
                                    </px:PXNumberEdit>
                                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc"
                                        CommitChanges="True">
                                    </px:PXTextEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="LineType" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="InventoryID" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TranDesc" CommitChanges="True">
                                    </px:PXGridColumn>
                                </Columns>
			                </px:PXGridLevel>
		                </Levels>
		                <AutoSize Enabled="True" MinHeight="150" />
		                <ActionBar ActionsText="False" PagerVisible="False">
		                </ActionBar>
	                </px:PXGrid>
               </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" />
    </px:PXTab>
</asp:Content>
