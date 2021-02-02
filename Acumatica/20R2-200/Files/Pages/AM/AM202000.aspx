<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM202000.aspx.cs" Inherits="Page_AM202000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AM.Forecast" PrimaryView="ForecastRecords" BorderStyle="NotSet" >
		<CallbackCommands>
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Width="100%" AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SyncPosition="True" SkinID="Primary">
		<Levels>
			<px:PXGridLevel DataMember="ForecastRecords" DataKeyNames="ForecastID">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />    
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SiteID" />                  
                    <px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True" />
                    <px:PXDropDown ID="edInterval" runat="server" DataField="Interval"/>
                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty"  />
                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM"/>
                    <px:PXDateTimeEdit ID="edBeginDate" runat="server" DataField="BeginDate" />
                    <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" />
                    <px:PXCheckBox ID="chkDependent" runat="server" DataField="Dependent" />
                    <px:PXCheckBox ID="chkActiveFlg" runat="server" DataField="ActiveFlg" />
                    <px:PXTextEdit ID="edForecastID" runat="server" DataField="ForecastID" />
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="True"/>
                    <px:PXTextEdit ID="edCustomerID_description" runat="server" DataField="CustomerID_description" />
                    <px:PXTextEdit ID="edInventoryID_description" runat="server" DataField="InventoryID_description" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="InventoryID"  Width="120px" CommitChanges="True"/>
                    <px:PXGridColumn DataField="SubItemID"  Width="120px"/>
                    <px:PXGridColumn DataField="SiteID" Width="120px" />
                    <px:PXGridColumn DataField="Interval" Width="85px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="117px" />
                    <px:PXGridColumn DataField="UOM" AutoCallBack="True" />
                    <px:PXGridColumn DataField="BeginDate"  Width="90px" CommitChanges="True" />
                    <px:PXGridColumn DataField="EndDate" Width="90px" />
                    <px:PXGridColumn DataField="Dependent" TextAlign="Center" Type="CheckBox" Width="90px" />
                    <px:PXGridColumn DataField="ActiveFlg" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="ForecastID"/>
                    <px:PXGridColumn DataField="CustomerID" Width="120px"/>
                    <px:PXGridColumn DataField="CustomerID_description" Width="220px"/>
                    <px:PXGridColumn DataField="InventoryID_description" Width="280px"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" />
	    <ActionBar ActionsText="False"/>
        <Mode InitNewRow="True" AllowUpload="True"/>
	</px:PXGrid>
</asp:Content>
