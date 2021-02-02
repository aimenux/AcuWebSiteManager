<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM500000.aspx.cs" Inherits="Page_AM500000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.AM.ReleaseOrd" PrimaryView="PlannedOrds" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Process" StartNewGroup="True" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Width="100%" AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Inquire" 
        SyncPosition="True" BatchUpdate="true" TabIndex="100" TemporaryFilterCaption="Filter Applied" >
		<Levels>
			<px:PXGridLevel DataMember="PlannedOrds" DataKeyNames="ProdOrdID">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" AllowEdit="True" />
                    <px:PXSelector ID="edProdOrdID" runat="server" DataField="ProdOrdID" Enabled="False" AllowEdit="True" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" DataKeyNames="InventoryCD" 
                        DataMember="_InventoryItem_AccessInfo.userName_" DataSourceID="ds" AllowEdit="True"/>
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID"/>
                    <px:PXSegmentMask ID="edSiteId" runat="server" DataField="SiteId" AllowEdit="True" />
                    <px:PXLayoutRule runat="server" />
                    <px:PXNumberEdit ID="edQtytoProd" runat="server" DataField="QtytoProd"  />
                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM"/>
                    <px:PXDateTimeEdit ID="edStartDate" runat="server" AllowNull="False" DataField="StartDate" />
                    <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate"/>
                    <px:PXDateTimeEdit ID="edProdDate" runat="server" DataField="ProdDate"/>
                    <px:PXTextEdit ID="edOrdTypeRef" runat="server" DataField="OrdTypeRef"  />
                    <px:PXSelector ID="edOrdNbr" runat="server" DataField="OrdNbr" AllowEdit="True" edit="1"/>
                    <px:PXTextEdit ID="edInventoryID_InventoryItem_descr" runat="server" DataField="InventoryID_InventoryItem_descr"/>
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="True"/>
                    <px:PXTextEdit ID="edCustomerID_Customer_acctName" runat="server" DataField="CustomerID_Customer_acctName"/>
                    <px:PXDropDown ID="edDetailSource" runat="server" DataField="DetailSource" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AutoCallBack="True" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="OrderType" />
                    <px:PXGridColumn DataField="ProdOrdID" Width="125px"/>
                    <px:PXGridColumn DataField="InventoryID" Width="125px" />
                    <px:PXGridColumn DataField="SubItemID" Width="120px"  />
                    <px:PXGridColumn DataField="SiteId" Width="125px" />
                    <px:PXGridColumn DataField="QtytoProd" Width="117px"/>
                    <px:PXGridColumn DataField="UOM" />
                    <px:PXGridColumn DataField="StartDate" Width="90px"/>
                    <px:PXGridColumn DataField="EndDate" Width="90px"/>
                    <px:PXGridColumn DataField="ProdDate" Width="90px"/>
                    <px:PXGridColumn DataField="OrdTypeRef"/>
                    <px:PXGridColumn DataField="OrdNbr"/>
                    <px:PXGridColumn DataField="InventoryID_InventoryItem_descr" Width="200px"/>
                    <px:PXGridColumn DataField="CustomerID" Width="120px"/>
                    <px:PXGridColumn DataField="CustomerID_Customer_acctName" Width="200px"/>
                    <px:PXGridColumn DataField="DetailSource" TextAlign="Left"/>
                    <px:PXGridColumn DataField="ProjectID" Width="90px"/>
                    <px:PXGridColumn DataField="TaskID" Width="90px"/>
                    <px:PXGridColumn DataField="CostCodeID" Width="90px"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="False">
		</ActionBar>
	</px:PXGrid>
</asp:Content>
