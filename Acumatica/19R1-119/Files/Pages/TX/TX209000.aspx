<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX209000.aspx.cs" Inherits="Page_TX209000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Data" TypeName="PX.Objects.TX.TaxImportDataMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary">
		<Levels>
			<px:PXGridLevel DataMember="Data">
				<Columns>
					<px:PXGridColumn DataField="StateCode" Width="100px" />
					<px:PXGridColumn DataField="StateName" Width="100px" />
					<px:PXGridColumn DataField="CityName" Width="100px" />
					<px:PXGridColumn DataField="CountyName" Width="100px" />
					<px:PXGridColumn DataField="ZipCode" Width="100px" />
					<px:PXGridColumn DataField="Origin" Width="100px" />
					<px:PXGridColumn DataField="TaxFreight" Width="100px" />
					<px:PXGridColumn DataField="TaxServices" Width="100px" />
					<px:PXGridColumn DataField="SignatureCode" Width="100px" />
					<px:PXGridColumn DataField="StateSalesTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="StateSalesTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="StateSalesTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="StateUseTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="StateUseTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="StateUseTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="StateTaxableMaximum" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="StateTaxOverMaximumRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="SignatureCodeCity" Width="100px" />
					<px:PXGridColumn DataField="CityTaxCodeAssignedByState" Width="100px" />
					<px:PXGridColumn DataField="CityLocalRegister" Width="100px" />
					<px:PXGridColumn DataField="CitySalesTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CitySalesTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="CitySalesTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CityUseTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CityUseTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="CityUseTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CityTaxableMaximum" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CityTaxOverMaximumRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="SignatureCodeCounty" Width="100px" />
					<px:PXGridColumn DataField="CountyTaxCodeAssignedByState" Width="100px" />
					<px:PXGridColumn DataField="CountyLocalRegister" Width="100px" />
					<px:PXGridColumn DataField="CountySalesTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CountySalesTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="CountySalesTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CountyUseTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CountyUseTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="CountyUseTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CountyTaxableMaximum" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CountyTaxOverMaximumRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="SignatureCodeTransit" Width="100px" />
					<px:PXGridColumn DataField="TransitTaxCodeAssignedByState" Width="100px" />
					<px:PXGridColumn DataField="TransitSalesTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="TransitSalesTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="TransitSalesTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="TransitUseTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="TransitUseTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="TransitUseTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="TransitTaxIsCity" Width="100px" />
					<px:PXGridColumn DataField="SignatureCodeOther1" Width="100px" />
					<px:PXGridColumn DataField="OtherTaxCode1AssignedByState" Width="100px" />
					<px:PXGridColumn DataField="Other1SalesTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other1SalesTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="Other1SalesTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other1UseTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other1UseTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="Other1UseTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other1TaxIsCity" Width="100px" />
					<px:PXGridColumn DataField="SignatureCodeOther2" Width="100px" />
					<px:PXGridColumn DataField="OtherTaxCode2AssignedByState" Width="100px" />
					<px:PXGridColumn DataField="Other2SalesTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other2SalesTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="Other2SalesTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other2UseTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other2UseTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="Other2UseTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other2TaxIsCity" Width="100px" />
					<px:PXGridColumn DataField="SignatureCodeOther3" Width="100px" />
					<px:PXGridColumn DataField="OtherTaxCode3AssignedByState" Width="100px" />
					<px:PXGridColumn DataField="Other3SalesTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other3SalesTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="Other3SalesTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other3UseTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other3UseTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="Other3UseTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other3TaxIsCity" Width="100px" />
					<px:PXGridColumn DataField="SignatureCodeOther4" Width="100px" />
					<px:PXGridColumn DataField="OtherTaxCode4AssignedByState" Width="100px" />
					<px:PXGridColumn DataField="Other4SalesTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other4SalesTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="Other4SalesTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other4UseTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other4UseTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="Other4UseTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Other4TaxIsCity" Width="100px" />
					<px:PXGridColumn DataField="CombinedSalesTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CombinedSalesTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="CombinedSalesTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CombinedUseTaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CombinedUseTaxRateEffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="CombinedUseTaxPreviousRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="DateLastUpdated" Width="90px" />
					<px:PXGridColumn DataField="DeleteCode" Width="100px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar PagerVisible="False">
			<PagerSettings Mode="NextPrevFirstLast" />
		</ActionBar>
	</px:PXGrid>
</asp:Content>
