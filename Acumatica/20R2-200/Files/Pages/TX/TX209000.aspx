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
					<px:PXGridColumn DataField="StateCode" />
					<px:PXGridColumn DataField="StateName" />
					<px:PXGridColumn DataField="CityName" />
					<px:PXGridColumn DataField="CountyName" />
					<px:PXGridColumn DataField="ZipCode" />
					<px:PXGridColumn DataField="Origin" />
					<px:PXGridColumn DataField="TaxFreight" />
					<px:PXGridColumn DataField="TaxServices" />
					<px:PXGridColumn DataField="SignatureCode" />
					<px:PXGridColumn DataField="StateSalesTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="StateSalesTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="StateSalesTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="StateUseTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="StateUseTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="StateUseTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="StateTaxableMaximum" TextAlign="Right" />
					<px:PXGridColumn DataField="StateTaxOverMaximumRate" TextAlign="Right" />
					<px:PXGridColumn DataField="SignatureCodeCity" />
					<px:PXGridColumn DataField="CityTaxCodeAssignedByState" />
					<px:PXGridColumn DataField="CityLocalRegister" />
					<px:PXGridColumn DataField="CitySalesTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="CitySalesTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="CitySalesTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="CityUseTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="CityUseTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="CityUseTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="CityTaxableMaximum" TextAlign="Right" />
					<px:PXGridColumn DataField="CityTaxOverMaximumRate" TextAlign="Right" />
					<px:PXGridColumn DataField="SignatureCodeCounty" />
					<px:PXGridColumn DataField="CountyTaxCodeAssignedByState" />
					<px:PXGridColumn DataField="CountyLocalRegister" />
					<px:PXGridColumn DataField="CountySalesTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="CountySalesTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="CountySalesTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="CountyUseTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="CountyUseTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="CountyUseTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="CountyTaxableMaximum" TextAlign="Right" />
					<px:PXGridColumn DataField="CountyTaxOverMaximumRate" TextAlign="Right" />
					<px:PXGridColumn DataField="SignatureCodeTransit" />
					<px:PXGridColumn DataField="TransitTaxCodeAssignedByState" />
					<px:PXGridColumn DataField="TransitSalesTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="TransitSalesTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="TransitSalesTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="TransitUseTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="TransitUseTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="TransitUseTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="TransitTaxIsCity" />
					<px:PXGridColumn DataField="SignatureCodeOther1" />
					<px:PXGridColumn DataField="OtherTaxCode1AssignedByState" />
					<px:PXGridColumn DataField="Other1SalesTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other1SalesTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="Other1SalesTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other1UseTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other1UseTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="Other1UseTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other1TaxIsCity" />
					<px:PXGridColumn DataField="SignatureCodeOther2" />
					<px:PXGridColumn DataField="OtherTaxCode2AssignedByState" />
					<px:PXGridColumn DataField="Other2SalesTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other2SalesTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="Other2SalesTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other2UseTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other2UseTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="Other2UseTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other2TaxIsCity" />
					<px:PXGridColumn DataField="SignatureCodeOther3" />
					<px:PXGridColumn DataField="OtherTaxCode3AssignedByState" />
					<px:PXGridColumn DataField="Other3SalesTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other3SalesTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="Other3SalesTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other3UseTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other3UseTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="Other3UseTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other3TaxIsCity" />
					<px:PXGridColumn DataField="SignatureCodeOther4" />
					<px:PXGridColumn DataField="OtherTaxCode4AssignedByState" />
					<px:PXGridColumn DataField="Other4SalesTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other4SalesTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="Other4SalesTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other4UseTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other4UseTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="Other4UseTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="Other4TaxIsCity" />
					<px:PXGridColumn DataField="CombinedSalesTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="CombinedSalesTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="CombinedSalesTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="CombinedUseTaxRate" TextAlign="Right" />
					<px:PXGridColumn DataField="CombinedUseTaxRateEffectiveDate" />
					<px:PXGridColumn DataField="CombinedUseTaxPreviousRate" TextAlign="Right" />
					<px:PXGridColumn DataField="DateLastUpdated" />
					<px:PXGridColumn DataField="DeleteCode" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar PagerVisible="False">
			<PagerSettings Mode="NextPrevFirstLast" />
		</ActionBar>
	</px:PXGrid>
</asp:Content>
