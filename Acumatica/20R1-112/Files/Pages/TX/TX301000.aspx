<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX301000.aspx.cs" Inherits="Page_TX301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.TX.TaxAdjustmentEntry" PrimaryView="Document">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="Release" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="ReverseAdjustment" CommitChanges="true"/>
			<px:PXDSCallbackCommand Visible="false" Name="ViewBatch" />
            <px:PXDSCallbackCommand Visible="False" Name="ViewOriginalDocument"/>
			<px:PXDSCallbackCommand Visible="false" Name="NewVendor" />
			<px:PXDSCallbackCommand Visible="false" Name="EditVendor" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Document" Caption="Document Summary" NoteIndicator="True" FilesIndicator="True" LinkIndicator="True" NotifyIndicator="True"
		ActivityIndicator="True" TabIndex="17300">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ColumnWidth ="220px"/>
			<px:PXDropDown ID="edDocType" runat="server" DataField="DocType" />
			<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" DataSourceID="ds" />
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
			<px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="s" ControlSize="M" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" DataSourceID="ds" AllowAddNew="True" AllowEdit="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" DataField="VendorLocationID" DataSourceID="ds" />
			<px:PXSelector ID="edTaxPeriod" runat="server" DataField="TaxPeriod" Size = "S" DataSourceID="ds" CommitChanges="True" AutoRefresh="True"/>
            <px:PXDateTimeEdit CommitChanges="True" ID="edDocDate" runat="server" DataField="DocDate" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" Width="100%" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<pxa:PXCurrencyRate ID="edCury" DataField="CuryID" runat="server" 
                RateTypeView="_TaxAdjustment_CurrencyInfo_" DataMember="_Currency_" 
                DataSourceID="ds"></pxa:PXCurrencyRate>
			<px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
			<px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="180px" Width="100%">
		<Items>
			<px:PXTabItem Text="Document Details">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel DataMember="Transactions">
								<Columns>
									<px:PXGridColumn DataField="TaxID" AutoCallBack="true" />
									<px:PXGridColumn DataField="TaxRate" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryTaxableAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="TaxZoneID" />
									<px:PXGridColumn DataField="AccountID" AutoCallBack="true" />
									<px:PXGridColumn DataField="SubID" />
                                    <px:PXGridColumn DataField="Description" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
									<px:PXSelector SuppressLabel="True" ID="edTaxID" runat="server" DataField="TaxID" AutoRefresh="true" />
									<px:PXSelector ID="edTaxZoneID" runat="server" DataField="TaxZoneID" />
									<px:PXNumberEdit ID="edTaxRate" runat="server" DataField="TaxRate" Enabled="False" />
									<px:PXNumberEdit ID="edCuryTaxableAmt" runat="server" DataField="CuryTaxableAmt" />
									<px:PXNumberEdit ID="edCuryTaxAmt" runat="server" DataField="CuryTaxAmt" />
									<px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" CommitChanges="true" />
									<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" AutoRefresh="true" />
                                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
								</RowTemplate>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<Mode InitNewRow="false" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Financial Details">
				<Template>
					<px:PXFormView ID="form2" runat="server" DataSourceID="ds" Width="100%" DataMember="CurrentDocument" CaptionVisible="False" SkinID="Transparent">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
							<px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" />
							<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
							<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
							<px:PXSegmentMask ID="edAdjAccountID" runat="server" DataField="AdjAccountID" CommitChanges="true" />
							<px:PXSegmentMask ID="edAdjSubID" runat="server" DataField="AdjSubID" AutoRefresh="true" AutoGenerateColumns="true" />
                            <px:PXTextEdit ID="edOrigRefNbr" runat="server" DataField="OrigRefNbr" Enabled="False" AllowEdit="True">
                                <LinkCommand Target="ds" Command="ViewOriginalDocument"/>
                            </px:PXTextEdit>
						</Template>
						<AutoSize Enabled="True" />
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
		</Items>
		<CallbackCommands>
			<Search CommitChanges="True" PostData="Page" />
			<Refresh CommitChanges="True" PostData="Page" />
		</CallbackCommands>
		<AutoSize Container="Window" Enabled="True" MinHeight="180" />
	</px:PXTab>
</asp:Content>
