<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN508500.aspx.cs"
    Inherits="Page_IN508500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.IN.INUpdateReplenishmentRules"
                     BorderStyle="NotSet" />		
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edPurchaseDateID" TabIndex="900">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
			<px:PXDateTimeEdit ID="edForecastDate" runat="server" CommitChanges="True" DataField="ForecastDate" />
			<px:PXDropDown ID="edAction" runat="server" DataField="Action" CommitChanges="True" SelectedIndex="-1" />			
            <px:PXSegmentMask ID="edSiteID" runat="server" CommitChanges="True" DataField="SiteID" />
			<px:PXSelector ID="edReplenishmentPolicyID" runat="server" CommitChanges="True" DataField="ReplenishmentPolicyID"  />
			<px:PXSegmentMask ID="edItemClassCD" runat="server" CommitChanges="True" DataField="ItemClassCD" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" AllowPaging="true" Style="z-index: 100; left: 0px; top: 0px;"
		BatchUpdate="true" AdjustPageSize="Auto"
        Width="100%" SkinID="PrimaryInquire" Caption="Items Requiring Replenishment">
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Levels>		
            <px:PXGridLevel  DataMember="Records">                
                <Columns>                   
                	<px:PXGridColumn DataField="Selected" DataType="Boolean" TextAlign="Center" 
						Type="CheckBox" Width="30px" AllowCheckAll="true">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="SiteID" Width="80px"/>
					<px:PXGridColumn DataField="InventoryID" Width="150px">
					</px:PXGridColumn>
					<px:PXGridColumn AllowNull="False" DataField="PreferredVendorOverride" 
						DataType="Boolean" TextAlign="Center" Type="CheckBox" Width="60px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="PreferredVendorID" Width="120px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="PreferredVendorLocationID">
					</px:PXGridColumn>					
					<px:PXGridColumn DataField="ReplenishmentClassID">
					</px:PXGridColumn>
					<px:PXGridColumn AllowNull="False" DataField="ReplenishmentPolicyOverride" 
						DataType="Boolean" TextAlign="Center" Type="CheckBox" Width="60px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="ReplenishmentPolicyID">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="ReplenishmentMethod" RenderEditorText="True">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="ReplenishmentSource" RenderEditorText="True">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="ReplenishmentSourceSiteID" Width="120px">
					</px:PXGridColumn>
					<px:PXGridColumn AllowNull="False" DataField="MaxShelfLifeOverride" 
						DataType="Boolean" TextAlign="Center" Type="CheckBox" Width="60px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="MaxShelfLife" DataType="Int32" TextAlign="Right">
					</px:PXGridColumn>

					<px:PXGridColumn AllowNull="False" DataField="SafetyStockOverride" 
						DataType="Boolean" TextAlign="Center" Type="CheckBox" Width="60px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="SafetyStock" DataType="Decimal" TextAlign="Right" 
						Width="100px">
					</px:PXGridColumn>
					<px:PXGridColumn AllowNull="False" DataField="MinQtyOverride" 
						DataType="Boolean" TextAlign="Center" Type="CheckBox" Width="60px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="MinQty" DataType="Decimal" TextAlign="Right" 
						Width="100px">
					</px:PXGridColumn>
					<px:PXGridColumn AllowNull="False" DataField="MaxQtyOverride" 
						DataType="Boolean" TextAlign="Center" Type="CheckBox" Width="60px">
					</px:PXGridColumn>

					<px:PXGridColumn DataField="MaxQty" DataType="Decimal" TextAlign="Right" 
						Width="100px">
					</px:PXGridColumn>
					<px:PXGridColumn AllowNull="False" DataField="SubItemOverride" 
						DataType="Boolean" TextAlign="Center" Type="CheckBox" Width="60px">
					</px:PXGridColumn>

					<px:PXGridColumn DataField="LastForecastDate" 
						DataType="DateTime" TextAlign="Center" Width="90px">
					</px:PXGridColumn>

					<px:PXGridColumn DataField="DemandPerDayAverage" 
						DataType="Decimal" TextAlign="Right" Width="100px">
					</px:PXGridColumn>					
					<px:PXGridColumn DataField="DemandPerDaySTDEV" 
						DataType="Decimal" TextAlign="Right" Width="100px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="LeadTimeAverage" 
						DataType="Decimal" TextAlign="Right" Width="100px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="LeadTimeSTDEV" 
						DataType="Decimal" TextAlign="Right" Width="100px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="MinQtySuggested" 
						DataType="Decimal" TextAlign="Right" Width="100px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="SafetyStockSuggested" 
						DataType="Decimal" TextAlign="Right" Width="100px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="MaxQtySuggested" 
						DataType="Decimal" TextAlign="Right" Width="100px">
					</px:PXGridColumn>			
                   
                </Columns>
		<Layout FormViewHeight=""></Layout>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="True" Container="Window" MinHeight="150"></AutoSize>
    </px:PXGrid>
</asp:Content>
