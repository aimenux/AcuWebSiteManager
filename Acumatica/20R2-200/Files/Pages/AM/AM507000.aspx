<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM507000.aspx.cs" Inherits="Page_AM507000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        TypeName="PX.Objects.AM.CreateInventoryItemProcess" PrimaryView="Filter" BorderStyle="NotSet" >
		<CallbackCommands>
          <px:PXDSCallbackCommand Name="Cancel" />  
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Filter" DefaultControlID="edCurrInvID">
        <Template>
            <px:PXLayoutRule ID="Col1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXSelector CommitChanges="True" AutoRefresh="true" ID="edEstimateID"  runat="server" DataField="EstimateID" 
                    DisplayMode="Hint" AllowEdit="True" FilterByAllFields="True" TextMode="Search" />
                <px:PXSelector CommitChanges="True" ID="edRevisionID" runat="server" DataField="RevisionID" AutoRefresh="True" FilterByAllFields="True" TextMode="Search" />
                <px:PXCheckBox ID="chkUpdateAllRevisions" runat="server" DataField="UpdateAllRevisions" CommitChanges="True" />
                <px:PXCheckBox ID="chkReuseExistingInventoryID" runat="server" DataField="ReuseExistingInventoryID" CommitChanges="True" />
            <px:PXLayoutRule ID="Col2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
                <px:PXDateTimeEdit ID="edRevisionDate" DataField="RevisionDate" runat="server"/>
                <px:PXCheckBox ID="chkCopyDetailedDescription" runat="server" DataField="CopyDetailedDescription" CommitChanges="True" />
                <px:PXCheckBox ID="chkCopyFiles" runat="server" DataField="CopyFiles" CommitChanges="True" />
                <px:PXCheckBox ID="chkCopyNotes" runat="server" DataField="CopyNotes" CommitChanges="True" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="200px" SyncPosition="True" 
        BatchUpdate="True" AdjustPageSize="Auto" SkinID="Inquire" TabIndex="1100" >
		<Levels>
            <px:PXGridLevel DataMember="NonInventoryItems" DataKeyNames="LineNbr">
			    <RowTemplate>
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXDropDown ID="edLevel" runat="server" DataField="Level" />
                    <px:PXSelector ID="edInventoryCD" runat="server" DataField="InventoryCD" FilterByAllFields="True" TextMode="Search" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			        <px:PXSegmentMask ID="edItemClassID" runat="server" DataField="ItemClassID"/>
                    <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID"  />
                    <px:PXSelector ID="edBaseUnit" runat="server" DataField="BaseUnit" />
                    <px:PXSelector ID="edTaxCategoryID" runat="server"  DataField="TaxCategoryID" />
                    <px:PXSelector ID="edPostClassID" runat="server" DataField="PostClassID" />
                    <px:PXSelector ID="edLotSerClassID" runat="server" DataField="LotSerClassID" />
                    <px:PXCheckBox ID="edStockItem" runat="server" DataField="StockItem" />
                    <px:PXTextEdit ID="edOriginalInventoryCD" runat="server" DataField="OriginalInventoryCD" />
                </RowTemplate>
     		    <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="70px" />
                    <px:PXGridColumn DataField="Level" Width="80px" TextAlign="Left"/>
                    <px:PXGridColumn DataField="InventoryCD" Width="100px" CommitChanges="True"/>
                    <px:PXGridColumn DataField="Description" Width="150px" />
                    <px:PXGridColumn DataField="ItemClassID" Width="80px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="SiteID" Width="100px" />
                    <px:PXGridColumn DataField="BaseUnit" Width="80px" />
                    <px:PXGridColumn DataField="TaxCategoryID" Width="80px"/>
                    <px:PXGridColumn DataField="PostClassID" Width="80px"/> 
                    <px:PXGridColumn DataField="LotSerClassID" Width="80px"/>
                    <px:PXGridColumn DataField="StockItem" Width="70px" TextAlign="Center" Type="CheckBox" />
                     <px:PXGridColumn DataField="OriginalInventoryCD" Width="100px" />
                </Columns>
                <Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="False">
		</ActionBar>
	</px:PXGrid>
</asp:Content>


