<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM509000.aspx.cs" Inherits="Page_AM509000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        TypeName="PX.Objects.AM.BOMMassChange" PrimaryView="UpdateBomMatlRecs" BorderStyle="NotSet" >
		<CallbackCommands>
		    <px:PXDSCallbackCommand Name="ViewBOM" Visible="False" DependOnGrid="grid" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="UpdateBomMatlRecs" DefaultControlID="edCurrInvID">
        <Template>
            <px:PXLayoutRule ID="Col1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXSegmentMask CommitChanges="True"  AutoRefresh="true" ID="edCurrInvID"  runat="server" DataField="CurrInvID" 
                    DisplayMode="Hint" AllowEdit="True" />
                <px:PXSegmentMask CommitChanges="True" ID="edCurrSubItemID" runat="server" DataField="CurrSubItemID" AutoRefresh="True" />
                <px:PXDateTimeEdit ID="edEffStartDate" runat="server" CommitChanges="True" DataField="EffStartDate" />
            <px:PXLayoutRule ID="Col2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXSegmentMask CommitChanges="True"  AutoRefresh="true" ID="edNewInvID"  runat="server" DataField="NewInvID" 
                    DisplayMode="Hint" AllowEdit="True" />
                <px:PXSegmentMask CommitChanges="True" ID="edNewSubItemID" runat="server" DataField="NewSubItemID" AutoRefresh="True"/>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="200px" SkinID="Inquire" SyncPosition="True" TabIndex="1100" >
		<Levels>
            <px:PXGridLevel DataKeyNames="BOMID,RevisionID,OperationID" DataMember="SelectedBoms">
			    <RowTemplate>
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXTextEdit ID="edBOMID" runat="server" DataField="BOMID" />
                    <px:PXTextEdit ID="edRevisionID" runat="server" DataField="RevisionID" />
			        <px:PXDropDown ID="edAMBomItem__Status" runat="server" DataField="AMBomItem__Status" />
                    <px:PXSelector ID="edOperationID" runat="server" DataField="OperationID" />
                    <px:PXSegmentMask ID="edBInventoryID" runat="server" DataField="AMBomItem__InventoryID" AllowEdit="True" />
                    <px:PXTextEdit ID="edBInventoryID_description" runat="server" DataField="AMBomItem__InventoryID_description" />
			        <px:PXSelector ID="edBSiteID" runat="server" DataField="AMBomItem__SiteID" AllowEdit="True" />
			        <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True" />
                    <px:PXNumberEdit ID="edQtyReq" runat="server" DataField="QtyReq" />
                    <px:PXTextEdit ID="edUOM" runat="server" DataField="UOM" />
                </RowTemplate>
     		    <Columns>
                     <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AutoCallBack="True" Width="80px" />
                     <px:PXGridColumn DataField="BOMID" Width="130px" LinkCommand="ViewBOM" />
                     <px:PXGridColumn DataField="RevisionID" Width="100px" />
		             <px:PXGridColumn DataField="AMBomItem__Status" />
                     <px:PXGridColumn DataField="OperationID" Width="100px" />
		             <px:PXGridColumn DataField="AMBomItem__InventoryID" Width="130px" />
                     <px:PXGridColumn DataField="AMBomItem__InventoryID_description" Width="130px" />
		             <px:PXGridColumn DataField="AMBomItem__SiteID" Width="130px" />
                     <px:PXGridColumn DataField="QtyReq" TextAlign="Right" Width="108px"/> 
                     <px:PXGridColumn DataField="UOM" Width="75px" />
		             <px:PXGridColumn DataField="Descr" MaxLength="255" Width="200px" />                         
		             <px:PXGridColumn DataField="BatchSize" TextAlign="Right" Width="108px" />
		             <px:PXGridColumn DataField="MaterialType" TextAlign="Left" Width="100px" />
		             <px:PXGridColumn DataField="PhantomRouting" TextAlign="Left" Width="110px" />
		             <px:PXGridColumn DataField="BFlush" TextAlign="Center" Type="CheckBox" Width="85px" />
		             <px:PXGridColumn DataField="CompBOMID" />
		             <px:PXGridColumn DataField="CompBOMRevisionID" Width="85px" />
		             <px:PXGridColumn DataField="SiteID" Width="130px" />
		             <px:PXGridColumn DataField="LocationID" TextAlign="Right" Width="130px" />
		             <px:PXGridColumn DataField="ScrapFactor" TextAlign="Right" Width="108px" />
		             <px:PXGridColumn DataField="BubbleNbr" Width="90px" />
		             <px:PXGridColumn DataField="EffDate" Width="85px" />
		             <px:PXGridColumn DataField="ExpDate" Width="85px" />
		             <px:PXGridColumn DataField="AMBomItem__EffStartDate" Width="90px"/>
		             <px:PXGridColumn DataField="AMBomItem__EffEndDate" Width="90px"/>
		             <px:PXGridColumn DataField="LineID" />
                </Columns>
                <Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="False">
		</ActionBar>
	</px:PXGrid>
</asp:Content>


