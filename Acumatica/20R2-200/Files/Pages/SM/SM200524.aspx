<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM200524.aspx.cs" Inherits="Page_SM200524"
	Title="Untitled Page" %>

<%@ Register TagPrefix="px" Namespace="PX.Web.Controls" Assembly="PX.Web.Controls" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SM.SPWikiProductMaint" PrimaryView="WikiProduct">
		<CallbackCommands>			
		</CallbackCommands>	
        <DataTrees>
			<px:PXTreeDataMember TreeKeys="PageID" TreeView="Folders" />
		</DataTrees>	
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="WikiProduct" Caption="Wiki Product" TemplateContainer="" DefaultControlID="edProductID">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="XM" LabelsWidth="SM" />
			<px:PXSelector ID="edProductID" runat="server" DataField="ProductID" AutoRefresh="True" DataSourceID="ds"  />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />			
		</Template>
	</px:PXFormView>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" 
        AllowPaging="True" AdjustPageSize="Auto" SkinID="Details">
		<Levels>
			<px:PXGridLevel DataMember="WikiProductDetails">
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXTreeSelector ID="edParent" runat="server" DataField="PageName" PopulateOnDemand="True"
						ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="Folders" CommitChanges="true" >
					<DataBindings>
						<px:PXTreeItemBinding TextField="Title" ValueField="Name" />
					</DataBindings>
					</px:PXTreeSelector>
                </RowTemplate>
				<Columns>                    
                    <px:PXGridColumn DataField="PageName" Width ="300" CommitChanges="true"/>
                    <px:PXGridColumn DataField="PageTitle" Width ="300"/>                                                        
				</Columns>
			</px:PXGridLevel>
		</Levels>	
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />	
	</px:PXGrid>
</asp:Content>
