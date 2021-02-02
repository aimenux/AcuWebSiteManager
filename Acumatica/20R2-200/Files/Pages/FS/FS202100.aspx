<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" 
ValidateRequest="false" CodeFile="FS202100.aspx.cs" Inherits="Page_FS202100"
Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="Filter" TypeName="PX.Objects.FS.WFStageMaint">
		<CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="Delete" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="Down" Visible="False" DependOnGrid="grid" CommitChanges="True"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="Up" Visible="False" DependOnGrid="grid" CommitChanges="True"></px:PXDSCallbackCommand>
		</CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="Nodes" TreeKeys="WFStageID" ></px:PXTreeDataMember>
        </DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="75px" DataMember="Filter" TabIndex="1700" DefaultControlID="edWFID">        
        <Template>
            <px:PXLayoutRule runat="server" StartRow="True" ControlSize="M" LabelsWidth="SM"></px:PXLayoutRule>
            <px:PXSelector ID="edWFID" runat="server" DataField="WFID" DataSourceID="ds">
            </px:PXSelector>
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr">
            </px:PXTextEdit>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
        <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
        <AutoSize Enabled="true" Container="Window" ></AutoSize>
          <Template1>
                <px:PXTreeView ID="tree" runat="server" DataSourceID="ds" PopulateOnDemand="True" ShowRootNode="False"
                ExpandDepth="1" Caption="Workflow Stages" AllowCollapse="true">
                    <ToolBarItems>
                        <px:PXToolBarButton Tooltip="Refresh Stages" ImageKey="Refresh"><AutoCallBack Target="tree" Command="Refresh" ></AutoCallBack></px:PXToolBarButton>
                    </ToolBarItems>
                    <AutoCallBack Target="grid" Command="Refresh" ></AutoCallBack>
                    <DataBindings>
                        <px:PXTreeItemBinding DataMember="Nodes" TextField="WFStageCD" ValueField="WFStageID" ImageUrlField="Icon" ></px:PXTreeItemBinding>
                    </DataBindings>
                    <AutoSize Enabled="true" ></AutoSize>
                </px:PXTreeView>
         </Template1>
         <Template2>
            <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100"
                Width="100%" SkinID="Details" TabIndex="700" AllowSearch="True"
                KeepPosition="True" ActionsPosition="None"  Height="200px" 
                Caption="List of Stages">
		        <Levels>
			        <px:PXGridLevel DataMember="Items">
			            <RowTemplate>
                            <px:PXMaskEdit ID="WFStageCD" runat="server" DataField="WFStageCD"></px:PXMaskEdit>
                            <px:PXCheckBox ID="edAllowComplete" runat="server" DataField="AllowComplete" Text="Allow Complete"></px:PXCheckBox>
                            <px:PXCheckBox ID="edAllowCancel" runat="server" DataField="AllowCancel" Text="Allow Cancel"></px:PXCheckBox>
                            <px:PXCheckBox ID="edAllowReopen" runat="server" DataField="AllowReopen" Text="Allow Reopen"></px:PXCheckBox>
                            <px:PXCheckBox ID="edAllowClose" runat="server" DataField="AllowClose" Text="Allow Close">
                            </px:PXCheckBox>
                            <px:PXCheckBox ID="edAllowModify" runat="server" DataField="AllowModify" Text="Allow Update"></px:PXCheckBox>
                            <px:PXCheckBox ID="edAllowDelete" runat="server" DataField="AllowDelete" Text="Allow Delete">
                            </px:PXCheckBox>
                            <px:PXTextEdit ID="Descr" runat="server" DataField="Descr"></px:PXTextEdit>
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="WFStageCD"></px:PXGridColumn>
                            <px:PXGridColumn DataField="AllowComplete" TextAlign="Center" Type="CheckBox"></px:PXGridColumn>
                            <px:PXGridColumn DataField="AllowCancel" TextAlign="Center" Type="CheckBox"></px:PXGridColumn>
                            <px:PXGridColumn DataField="AllowReopen" TextAlign="Center" Type="CheckBox"></px:PXGridColumn>
                            <px:PXGridColumn DataField="AllowClose" TextAlign="Center"   
                                Type="CheckBox"></px:PXGridColumn>
                            <px:PXGridColumn DataField="AllowModify" TextAlign="Center" Type="CheckBox">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="AllowDelete" TextAlign="Center" Type="CheckBox">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="Descr"></px:PXGridColumn>
                        </Columns>
			        </px:PXGridLevel>
		        </Levels>
                <Parameters>
                     <px:PXControlParam ControlID="tree" Name="parent" PropertyName="SelectedValue" Type="String" ></px:PXControlParam>
                </Parameters>
                <AutoSize Enabled="True" MinHeight="150" />
                <ActionBar ActionsText="False" >
                    <CustomItems>
                        <px:PXToolBarSeperator></px:PXToolBarSeperator>
                        <px:PXToolBarButton Tooltip="Move Node Up" ImageSet="main" ImageKey="ArrowUp"><AutoCallBack Command="Up" Target="ds"></AutoCallBack></px:PXToolBarButton>
                        <px:PXToolBarButton Tooltip="Move Node Down" ImageSet="main" ImageKey="ArrowDown"><AutoCallBack Command="Down" Target="ds"></AutoCallBack></px:PXToolBarButton>
                    </CustomItems>
                    <Actions>
                        <Save Enabled="False" ></Save>
                        <Search Enabled="False" ></Search>
                    </Actions>
                </ActionBar>
	        </px:PXGrid>
        </Template2>
    </px:PXSplitContainer>
</asp:Content>
