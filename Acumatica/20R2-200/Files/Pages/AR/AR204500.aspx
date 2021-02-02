<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AR204500.aspx.cs" Inherits="Page_AR204500"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.AR.ARFinChargesMaint" Visible="True"
		PrimaryView="ARFinChargesList">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server" >
            <px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" DataMember="ARFinChargesList" DataSourceID="ds" TabIndex="100"  >
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"/>
					<px:PXLayoutRule runat="server" GroupCaption="General Settings" StartGroup="True" />
                    <px:PXSelector ID="edFinChargeID" runat="server" DataField="FinChargeID" AutoRefresh="True" />
                    <px:PXTextEdit ID="edFinChargeDesc" runat="server" DataField="FinChargeDesc" DefaultLocale="" />
                    <px:PXDropDown ID="edCalcMethod" runat="server" DataField="CalculationMethod" />
                    <px:PXSelector ID="edTermsID" runat="server" DataField="TermsID" />
                    <px:PXCheckBox ID="chkBaseCurFlag" runat="server" DataField="BaseCurFlag" />
                    <px:PXSegmentMask CommitChanges="True" ID="edFinChargeAccountID" runat="server" DataField="FinChargeAccountID" />
					<px:PXSegmentMask ID="edFinChargeSubID" runat="server" DataField="FinChargeSubID" />
					<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AutoRefresh="True"/>
                   <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M"/>
					<px:PXLayoutRule runat="server" GroupCaption="Overdue Fee Settings" StartGroup="True" />
                    <px:PXNumberEdit ID="edFeeAmt" runat="server" DataField="FeeAmount" CommitChanges="True" DefaultLocale="" />
                    <px:PXSegmentMask CommitChanges="True" ID="edFeeAccountID" runat="server" DataField="FeeAccountID" />
					<px:PXSegmentMask ID="edFeeSubID" runat="server" DataField="FeeSubID"  CommitChanges="True" />
                    <px:PXTextEdit ID="edFeeDesc" runat="server" DataField="FeeDesc" DefaultLocale="" />
					<px:PXLayoutRule runat="server" GroupCaption="Charging Settings" StartGroup="True" />
                    <px:PXNumberEdit ID="edMinChargeDocumentAmt" runat="server" DataField="MinChargeDocumentAmt" DefaultLocale="" />
                    <px:PXDropDown ID="edChargingMethod" runat="server" DataField="ChargingMethod" CommitChanges="True"/>        
                    <px:PXNumberEdit ID="edFixedAmount" runat="server" DataField="FixedAmount" DefaultLocale="" />
            
                    <px:PXNumberEdit ID="edLineThreshold" runat="server" DataField="LineThreshold" DefaultLocale="">
                    </px:PXNumberEdit>
                    <px:PXNumberEdit ID="edMinFinChargeAmount" runat="server" DataField="MinFinChargeAmount" DefaultLocale="">
                    </px:PXNumberEdit>

			<px:PXGrid ID="gridNS" runat="server" SkinID="ShortList" Width="400px" Height="300px" TabIndex="200" 
				DataSourceID="ds">
				<Mode InitNewRow="True" AllowSort="False" />
				<Levels>
					<px:PXGridLevel DataMember="PercentList" DataKeyNames="FinChargeID,BeginDate,PercentID">
						<Columns>
							<px:PXGridColumn DataField="BeginDate" TextAlign="Right" CommitChanges="True" />
							<px:PXGridColumn DataField="FinChargePercent" TextAlign="Right" />
						</Columns>
						<RowTemplate>
							<px:PXNumberEdit runat="server" ID="edFinChargePercent" DataField="FinChargePercent"></px:PXNumberEdit>
						</RowTemplate>
					</px:PXGridLevel>
				</Levels>
				<ActionBar>
					<Actions>
						<ExportExcel Enabled="False" />
					</Actions>
				</ActionBar>
			</px:PXGrid>
                   
                </Template>
</px:PXFormView>
</asp:Content>
