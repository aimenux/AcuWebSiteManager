<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM502000.aspx.cs" Inherits="Page_AM502000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        TypeName="PX.Objects.AM.GenerateForecastProcess" PrimaryView="SelectedRecs" BorderStyle="NotSet" >
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="filter" runat="server" DataSourceID="ds" Width="100%" DataMember="Settings" DefaultControlID="edType" >
        <Activity HighlightColor="" SelectedColor="" Width="" Height="50px"></Activity>
     	<Template>
            <px:PXLayoutRule ID="Col1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                <px:PXDateTimeEdit ID="edForecastDate" runat="server" DataField="ForecastDate" MaxValue="2079-06-06" Enabled="True" />
                <px:PXDropDown ID="edType" runat="server" DataField="Type" AutoCallBack="True" CommitChanges="True" />
                <px:PXSelector ID="edSeasonality"  runat="server" DataField="Seasonality" DisplayMode="Hint" AllowEdit="True" />
                <px:PXNumberEdit ID="edGrowthRate" runat="server" DataField="GrowthRate" />
                <px:PXNumberEdit ID="edGrowthFactor" runat="server" DataField="GrowthFactor" />
                <px:PXCheckBox CommitChanges="True" ID="edCalculateByMonth" runat="server" DataField="CalculateByMonth" />   
            <px:PXLayoutRule ID="Col2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                <px:PXNumberEdit ID="edYears" runat="server" DataField="Years" />
                <px:PXCheckBox CommitChanges="True" ID="edDependent" runat="server" DataField="Dependent" /> 
                <px:PXSegmentMask CommitChanges="True" ID="edBAccountID" runat="server" DataField="BAccountID" AllowEdit="True" DataSourceID="ds" />
                <px:PXCheckBox CommitChanges="True" ID="edProcessByCustomer" runat="server" DataField="ProcessByCustomer" /> 
                <px:PXSelector ID="edSiteId" runat="server" DataField="SiteId" HintField="descr" AllowEdit="True" />
                <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" HintField="descr" AllowEdit="True" />
                <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" AllowEdit="True" />
                <px:PXSegmentMask CommitChanges="True" AutoRefresh="true" ID="edItemClassID"  runat="server" DataField="ItemClassID" DisplayMode="Hint" AllowEdit="True" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="200px" SkinID="Details" SyncPosition="true" AllowPaging="True"  >
		<Levels>
            <px:PXGridLevel DataKeyNames="InventoryID,SubItemID,SiteID,BeginDate,EndDate" DataMember="SelectedRecs">
			    <RowTemplate>
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSegmentMask ID="edgInventoryID" runat="server" DataField="InventoryID" CommitChanges="True" AllowEdit="True"/>
                    <px:PXSegmentMask ID="edgSubItemID" runat="server" DataField="SubItemID"  />
                    <px:PXSelector ID="edgSiteID" runat="server" DataField="SiteID" AllowEdit="True" />
                    <px:PXDateTimeEdit ID="edBeginDate" runat="server" DataField="BeginDate" />
                    <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" />
                    <px:PXNumberEdit ID="edForecastQty" runat="server" DataField="ForecastQty" />
                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" AllowEdit="True"/>
                    <px:PXNumberEdit ID="edChangeUnits" runat="server" DataField="ChangeUnits"/>
                    <px:PXNumberEdit ID="edPercentChange" runat="server" DataField="PercentChange" />
                    <px:PXSelector ID="edCustomerID" runat="server" DataField="CustomerID" />
                    <px:PXCheckBox ID="edgDependent" runat="server" DataField="Dependent" />
                    <px:PXNumberEdit ID="edLastYearSalesQty" runat="server" DataField="LastYearSalesQty" />
                    <px:PXNumberEdit ID="edLastYearBaseQty" runat="server" DataField="LastYearBaseQty" />
                    <px:PXTextEdit ID="edSeasonality" runat="server" DataField="Seasonality" />
                </RowTemplate>
     		    <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Width="70px" Type="CheckBox" AutoCallBack="True" />
                    <px:PXGridColumn DataField="InventoryID" Width="130px" AutoCallBack="True" />
                    <px:PXGridColumn DataField="SubItemID" Width="81px" />
                    <px:PXGridColumn DataField="SiteID" Width="130px" />
                    <px:PXGridColumn DataField="BeginDate" Width="85px"/>
                    <px:PXGridColumn DataField="EndDate" Width="85px" />
                    <px:PXGridColumn DataField="ForecastQty" TextAlign="Right" Width="108px" CommitChanges="True" AutoCallBack="True" /> 
                    <px:PXGridColumn DataField="UOM" Width="75px" />
                    <px:PXGridColumn DataField="ChangeUnits" TextAlign="Right" Width="108px" />
                    <px:PXGridColumn DataField="PercentChange" TextAlign="Right" Width="108px" />
                    <px:PXGridColumn DataField="CustomerID" Width="130px"/>
                    <px:PXGridColumn DataField="Dependent" TextAlign="Center" Type="CheckBox" Width="108px" />
                    <px:PXGridColumn DataField="LastYearSalesQty" TextAlign="Right" Width="108px" />
                    <px:PXGridColumn DataField="LastYearBaseQty" TextAlign="Right" Width="108px" />
                    <px:PXGridColumn DataField="Seasonality" Width="130px"/>
                </Columns>
                <Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
        <Mode InitNewRow="True" AllowUpload="True"/>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="False">
		</ActionBar>
	</px:PXGrid>
</asp:Content>


