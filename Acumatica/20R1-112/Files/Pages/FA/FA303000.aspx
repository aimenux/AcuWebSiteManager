<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA303000.aspx.cs"
    Inherits="Page_FA303000" Title="Fixed Assets" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.FA.AssetMaint" PrimaryView="Asset"
        BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand DependOnGrid="gridTranHistory" Name="ViewDocument" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="gridTranHistory" Name="ViewBatch" Visible="False" />
            <px:PXDSCallbackCommand Name="RunReversal" Visible="False" />
            <px:PXDSCallbackCommand Name="RunDispReversal" Visible="False" />
            <px:PXDSCallbackCommand Name="RunDisposal" Visible="False" />
            <px:PXDSCallbackCommand Name="RunSplit" Visible="False" />
            <px:PXDSCallbackCommand Name="Action" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="calculateDepreciation" PostData="Self" CommitChanges="true" StartNewGroup="true" Visible="False" />
            <px:PXDSCallbackCommand Name="suspend" CommitChanges="true" Visible="False" DependOnGrid="gridAssetBalance" />
            <px:PXDSCallbackCommand Name="processAdditions" Visible="False" CommitChanges="true" DependOnGrid="gridGLTran" />
            <px:PXDSCallbackCommand Name="reduceUnreconCost" Visible="False" />
            <px:PXDSCallbackCommand Name="DisposalOK" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="formAsset" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Asset" Caption="Fixed Assets"
        NoteIndicator="True" FilesIndicator="True" LinkIndicator="true" NotifyIndicator="true" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects"
        ActivityIndicator="true" ActivityField="NoteActivity">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edAssetCD" runat="server" DataField="AssetCD" AutoRefresh="True">
                <GridProperties FastFilterFields="AssetCD,Description">
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
            </px:PXSelector>
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edParentAssetID" runat="server" DataField="ParentAssetID" AutoRefresh="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="600px" Width="100%" Style="z-index: 100" DataSourceID="ds" DataMember="CurrentAsset"
        NoteField="" MarkRequired="Dynamic">
        <AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
        <Items>
            <px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Asset Summary" />
					<px:PXSelector CommitChanges="True" ID="edClassID" runat="server" DataField="ClassID" AllowEdit="True" AllowAddNew="True" AutoRefresh="True" TabIndex ="1001" />
					<px:PXFormView ID="formAssetDetails2" runat="server" DataSourceID="ds" DataMember="AssetDetails" RenderStyle="Simple" MarkRequired="Dynamic">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
							<px:PXDropDown ID="edPropertyType" runat="server" AllowNull="False" DataField="PropertyType" TabIndex ="1002" />
							<px:PXDropDown ID="edStatus" runat="server" DataField="Status" SelectedIndex="1" AllowNull="False" Enabled="False" TabIndex ="1003" />
							<px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" TabIndex ="1004" />
                        </Template>
                    </px:PXFormView>                   
					<px:PXSelector ID="edAssetTypeID" runat="server" DataField="AssetTypeID" AllowEdit="True" AutoRefresh="True" CommitChanges="True" TabIndex ="1005" />
					<px:PXCheckBox CommitChanges="True" ID="chkIsTangible" runat="server" Checked="True" DataField="IsTangible" Enabled="False" TabIndex ="1006" />
					<px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" TabIndex ="1007" />
					<px:PXCheckBox CommitChanges="True" ID="chkDepreciable" runat="server" DataField="Depreciable" TabIndex ="1008" />
					<px:PXNumberEdit CommitChanges="True" ID="edUsefulLife" runat="server" DataField="UsefulLife" AllowNull="True" TabIndex ="1009" />
                    <px:PXFormView ID="formAssetDetails3" runat="server" DataSourceID="ds" DataMember="AssetDetails" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                            <px:PXDateTimeEdit CommitChanges="True" ID="edReceiptDate" runat="server" DataField="ReceiptDate" />
                            <px:PXDateTimeEdit CommitChanges="True" ID="edDepreciateFromDate" runat="server" DataField="DepreciateFromDate" />
                            <px:PXNumberEdit CommitChanges="True" ID="edAcquisitionCost" runat="server" DataField="AcquisitionCost" />
                            <px:PXNumberEdit CommitChanges="True" ID="edSalvageValue" runat="server" DataField="SalvageAmount" />
                            <px:PXNumberEdit ID="edReplacementCost" runat="server" DataField="ReplacementCost" />
                            <px:PXDateTimeEdit ID="edDisposalDate" runat="server" DataField="DisplayDisposalDate" Enabled="False" />
                            <px:PXSelector ID="edDisposalMethodID" runat="server" DataField="DisplayDisposalMethodID" Enabled="False" />
                            <px:PXNumberEdit ID="edSaleAmount" runat="server" DataField="DisplaySaleAmount" Enabled="False" />
                        </Template>
                        <Activity HighlightColor="" SelectedColor="" />
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" GroupCaption="Tracking Info" StartGroup="True" />
                    <px:PXFormView ID="formAssetLocation" runat="server" DataSourceID="ds" DataMember="AssetLocation" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                            <px:PXSegmentMask CommitChanges="True" ID="edLocationID" runat="server" DataField="LocationID" AllowEdit="True" DataSourceID="ds">
                                <CallBackMode CommitChanges="True" />
                            </px:PXSegmentMask>
                            <px:PXSelector CommitChanges="True" ID="edBuildingID" runat="server" DataField="BuildingID" AllowEdit="True" AutoRefresh="True"
                                DataSourceID="ds" />
                            <px:PXTextEdit CommitChanges="True" ID="edFloor" runat="server" DataField="Floor" />
                            <px:PXTextEdit CommitChanges="True" ID="edRoom" runat="server" DataField="Room" />
                            <px:PXSelector CommitChanges="True" ID="edEmployeeID" runat="server" DataField="EmployeeID" AllowEdit="True" DataSourceID="ds" />
                            <px:PXSelector CommitChanges="True" ID="edDepartment" runat="server" DataField="Department" AllowEdit="True" DataSourceID="ds" />
							<px:PXTextEdit CommitChanges="True" ID="edReason" runat="server" DataField="Reason" />
						</Template>
                        <Activity HighlightColor="" SelectedColor="" />
                    </px:PXFormView>
                    <px:PXFormView ID="formAssetDetails4" runat="server" DataSourceID="ds" DataMember="AssetDetails" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                            <px:PXMaskEdit ID="edTagNbr" runat="server" DataField="TagNbr" InputMask="&gt;CCCCCCCCCCCCCCCCCCCC" />
                        </Template>
                        <Activity HighlightColor="" SelectedColor="" />
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Purchase/Tangible Info">
                <Template>
                    <px:PXFormView ID="formAssetDetailsPurchaseAndDisposal" runat="server" DataSourceID="ds" SkinID="Transparent" Style="z-index: 100;"
                        Width="100%" DataMember="AssetDetails">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Purchase Details" />
                            <px:PXDateTimeEdit ID="edReceiptDate1" runat="server" DataField="ReceiptDate" />
                            <px:PXDropDown ID="edReceiptType" runat="server" DataField="ReceiptType" />
                            <px:PXSelector ID="edReceiptNbr" runat="server" DataField="ReceiptNbr" AllowEdit="True" />
                            <px:PXTextEdit ID="edPONumber" runat="server" DataField="PONumber" />
                            <px:PXTextEdit ID="edPurchaseBillNumber" runat="server" DataField="BillNumber" />
                            <px:PXTextEdit ID="edManufacturer" runat="server" DataField="Manufacturer" />
                            <px:PXTextEdit ID="edModel" runat="server" DataField="Model" />
                            <px:PXTextEdit ID="edSerialNumber" runat="server" DataField="SerialNumber" />
                            <px:PXDateTimeEdit ID="edWarrantyExpirationDate" runat="server" DataField="WarrantyExpirationDate" />
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Tangible Info" />
                            <px:PXDropDown ID="edReportingLineNbr" runat="server" AllowNull="False" DataField="ReportingLineNbr" />
                            <px:PXDropDown ID="edCondition" runat="server" AllowNull="False" DataField="Condition" />
                            <px:PXNumberEdit ID="edFairMarketValue" runat="server" DataField="FairMarketValue" />
                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Lease/Rent Info" />
                            <px:PXSegmentMask ID="edLessorID" runat="server" DataField="LessorID" />
                            <px:PXNumberEdit ID="edLeaseRentTerm" runat="server" DataField="LeaseRentTerm" />
                            <px:PXTextEdit ID="edLeaseNumber" runat="server" DataField="LeaseNumber" />
                            <px:PXNumberEdit ID="edRentAmount" runat="server" DataField="RentAmount" />
                            <px:PXNumberEdit ID="edRetailCost" runat="server" DataField="RetailCost" />
                            <px:PXTextEdit ID="edManufacturingYear" runat="server" DataField="ManufacturingYear" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="GL Accounts">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edConstructionAccountID" runat="server" DataField="ConstructionAccountID" />
                    <px:PXSegmentMask ID="edConstructionSubID" runat="server" DataField="ConstructionSubID">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="FixedAsset.constructionAccountID" PropertyName="DataControls[&quot;edConstructionAccountID&quot;].Value" />
                        </Parameters>
                    </px:PXSegmentMask>
					<px:PXSegmentMask ID="edFAAccountID" runat="server" DataField="FAAccountID" CommitChanges="True"/>
					<px:PXSegmentMask ID="edFASubID" runat="server" DataField="FASubID" CommitChanges="True">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="FixedAsset.fAAccountID" PropertyName="DataControls[&quot;edFAAccountID&quot;].Value" />
                        </Parameters>
                    </px:PXSegmentMask>
					<px:PXSegmentMask ID="edFAAccrualAcctID" runat="server" DataField="FAAccrualAcctID" CommitChanges="True"/>
					<px:PXSegmentMask ID="edFAAccrualSubID" runat="server" DataField="FAAccrualSubID" CommitChanges="True" />
					<px:PXSegmentMask ID="edAccumulatedDepreciationAccountID" runat="server" DataField="AccumulatedDepreciationAccountID" CommitChanges="True" />
					<px:PXSegmentMask ID="edAccumulatedDepreciationSubID" runat="server" DataField="AccumulatedDepreciationSubID" CommitChanges="True">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="FixedAsset.accumulatedDepreciationAccountID" PropertyName="DataControls[&quot;edAccumulatedDepreciationAccountID&quot;].Value" />
                        </Parameters>
                    </px:PXSegmentMask>
					<px:PXSegmentMask ID="edDepreciatedExpenseAccountID" runat="server" DataField="DepreciatedExpenseAccountID" CommitChanges="True" />
					<px:PXSegmentMask ID="edDepreciatedExpenseSubID" runat="server" DataField="DepreciatedExpenseSubID" CommitChanges="True">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="FixedAsset.depreciatedExpenseAccountID" PropertyName="DataControls[&quot;edDepreciatedExpenseAccountID&quot;].Value" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edDisposalAccountID" runat="server" DataField="DisposalAccountID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edDisposalSubID" runat="server" DataField="DisposalSubID">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="FixedAsset.disposalAccountID" PropertyName="DataControls[&quot;edDisposalAccountID&quot;].Value" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edRentAccountID" runat="server" DataField="RentAccountID" />
                    <px:PXSegmentMask ID="edGainAccountID" runat="server" DataField="GainAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edRentSubID" runat="server" DataField="RentSubID">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="FixedAsset.rentAccountID" PropertyName="DataControls[&quot;edRentAccountID&quot;].Value" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edGainSubID" runat="server" DataField="GainSubID">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="FixedAsset.gainAcctID" PropertyName="DataControls[&quot;edGainAccountID&quot;].Value" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edLeaseAccountID" runat="server" DataField="LeaseAccountID" />
                    <px:PXSegmentMask ID="edLossAccountID" runat="server" DataField="LossAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edLeaseSubID" runat="server" DataField="LeaseSubID">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="FixedAsset.leaseAccountID" PropertyName="DataControls[&quot;edLeaseAccountID&quot;].Value" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edLossSubID" runat="server" DataField="LossSubID">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="FixedAsset.gainAcctID" PropertyName="DataControls[&quot;edLossAccountID&quot;].Value" />
                        </Parameters>
                    </px:PXSegmentMask>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Balance" LoadOnDemand="False" RepaintOnDemand="False">
                <Template>
                    <px:PXGrid SkinID="Details" ID="gridAssetBalance" BorderWidth="0px" runat="server" DataSourceID="ds" 
                        Width="100%" AllowPaging="True" ActionsPosition="Top" AllowSearch="True"
                        Height="482px" SyncPosition="True" RepaintColumns="True">
                        <Levels>
                            <px:PXGridLevel DataMember="AssetBalance" DataKeyNames="AssetID,BookID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector CommitChanges="True" ID="edBookID" runat="server" DataField="BookID" AllowEdit="True" />
                                    <px:PXSelector CommitChanges="True" Height="18px" ID="edDepreciationMethodID" runat="server" AllowEdit="True" DataField="DepreciationMethodID" AutoRefresh="True"/>
                                    <px:PXDropDown ID="edAveragingConvention" runat="server" DataField="AveragingConvention" />
                                    <px:PXCheckBox ID="chkDepreciate" runat="server" DataField="Depreciate" />
                                    <px:PXNumberEdit ID="edUsefulLife" runat="server" DataField="UsefulLife" CommitChanges="True"/>
                                    <px:PXNumberEdit ID="edADSLife" runat="server" DataField="ADSLife" />
                                    <px:PXNumberEdit ID="edAcquisitionCost" runat="server" DataField="AcquisitionCost" />
                                    <px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status" />
                                    <px:PXNumberEdit ID="edBusinessUse" runat="server" DataField="BusinessUse" />
									<px:PXSelector CommitChanges="True" ID="edBonusID" runat="server" DataField="BonusID" AllowEdit="True" AutoRefresh="True" />
                                    <px:PXNumberEdit ID="edBonusAmount" runat="server" DataField="BonusAmount" />
                                    <px:PXNumberEdit ID="edBonusRate" runat="server" DataField="BonusRate" />
                                    <px:PXDropDown ID="edMidMonthType" runat="server" DataField="MidMonthType" AllowNull="False" />
                                    <px:PXNumberEdit ID="edMidMonthDay" runat="server" DataField="MidMonthDay" />
                                    <px:PXNumberEdit ID="edRecoveryPeriod" runat="server" DataField="RecoveryPeriod" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXNumberEdit ID="edYtdBonusRecap" runat="server" DataField="YtdBonusRecap" />
                                    <px:PXNumberEdit ID="edTax179Amount" runat="server" DataField="Tax179Amount" />
                                    <px:PXNumberEdit ID="edYtdTax179Recap" runat="server" DataField="YtdTax179Recap" />
                                    <px:PXDateTimeEdit CommitChanges="True" ID="edDeprFromDate" runat="server" DataField="DeprFromDate" />
                                    <px:PXSelector ID="edDeprFromPeriod" runat="server" DataField="DeprFromPeriod" />
                                    <px:PXNumberEdit ID="edYtdDeprBase" runat="server" DataField="YtdDeprBase" />
                                    <px:PXNumberEdit ID="edYtdDepreciated" runat="server" DataField="YtdDepreciated" />
                                    <px:PXSelector CommitChanges="True" ID="edDeprToPeriod" runat="server" DataField="DeprToPeriod" />
                                    <px:PXSelector CommitChanges="True" ID="edLastDeprPeriod" runat="server" DataField="LastDeprPeriod" />
                                    <px:PXNumberEdit ID="edYtdBal" runat="server" DataField="YtdBal" />
                                    <px:PXNumberEdit ID="edYtdRGOL" runat="server" DataField="YtdRGOL" />
									<px:PXNumberEdit ID="edSalvageAmount" runat="server" DataField="SalvageAmount" />
								</RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="BookID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="DepreciationMethodID" AutoCallBack="True" />
                                    <px:PXGridColumn RenderEditorText="True" AllowNull="False" DataField="Status" />
                                    <px:PXGridColumn DataField="UpdateGL" TextAlign="Center" AllowNull="False" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Depreciate" TextAlign="Center" AllowNull="False" Type="CheckBox" AutoCallBack="True" />
									<px:PXGridColumn DataField="DeprFromDate" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="DeprFromPeriod" DisplayFormat="##-####" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="LastDeprPeriod" DisplayFormat="##-####" Label="Last Depr. Period" />
                                    <px:PXGridColumn DataField="DeprToPeriod" Label="Last Period" AutoCallBack="True" DisplayFormat="##-####" />
                                    <px:PXGridColumn DataField="AcquisitionCost" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="YtdAcquired" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="BusinessUse" TextAlign="Right" />
                                    <px:PXGridColumn DataField="YtdDeprBase" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="SalvageAmount" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="YtdDepreciated" TextAlign="Right">
                                        <Header Text="Accum Depr.">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn AllowNull="False" DataField="YtdBal" Label="Net Value" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="YtdRGOL" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PercentPerYear" TextAlign="Right" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="UsefulLife" TextAlign="Right" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="ADSLife" Label="ADS Life, years" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RecoveryPeriod" Label="Recovery Periods" TextAlign="Right" Visible="False" />
									<px:PXGridColumn DataField="AveragingConvention" RenderEditorText="True" Label="Averaging Convention" MatrixMode="True"/>
                                    <px:PXGridColumn DataField="MidMonthType" Label="Mid-Month Type" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="MidMonthDay" TextAlign="Right">
                                        <Header Text="Mid-Month Day">
                                        </Header>
                                    </px:PXGridColumn>
                                    <px:PXGridColumn AllowNull="False" DataField="Tax179Amount" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="YtdTax179Recap" TextAlign="Right" />
									<px:PXGridColumn DataField="BonusID" CommitChanges="True" />
									<px:PXGridColumn DataField="BonusRate" TextAlign="Right" Label="Bonus Rate" CommitChanges="True" />
                                    <px:PXGridColumn DataField="BonusAmount" TextAlign="Right" AllowNull="False" Label="Bonus Amount" />
                                    <px:PXGridColumn DataField="YtdBonusRecap" TextAlign="Right" AllowNull="False" Label="Bonus Recapture Amount" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Components" Visible="True">
                <Template>
                    <px:PXGrid SkinID="Inquire" ID="gridComponents" BorderWidth="0px" runat="server" DataSourceID="ds" Style="z-index: 100; height: 458px;"
                        Width="100%" AllowPaging="True" AdjustPageSize="Auto" ActionsPosition="Top" AllowSearch="True" Height="458px">
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="AssetElements" DataKeyNames="AssetCD">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edAssetCD" runat="server" DataField="AssetCD" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="AssetCD" DisplayFormat="&gt;aaaaaaaaaaaaaaa" />
                                    <px:PXGridColumn DataField="Description" Label="Description" />
                                    <px:PXGridColumn DataField="ClassID" Label="Asset Class" />
                                    <px:PXGridColumn DataField="AssetTypeID" Label="Asset Type" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Status" RenderEditorText="True" AllowNull="False" AllowUpdate="False" Label="Status" />
                                    <px:PXGridColumn DataField="FADetails__AcquisitionCost" AllowNull="False" Label="Acquisition Cost" TextAlign="Right" />
                                    <px:PXGridColumn DataField="UsefulLife" Label="Useful Life, Years" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Active" AllowNull="False" Label="Active" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="FADetails__PropertyType" AllowNull="False" Label="Property Type" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="FADetails__Condition" AllowNull="False" Label="Condition" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="FADetails__ReceiptDate" Label="Receipt Date" />
                                    <px:PXGridColumn DataField="FADetails__ReceiptType" Label="Receipt Type" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="FADetails__ReceiptNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" Label="Receipt Nbr." />
                                    <px:PXGridColumn DataField="FADetails__PONumber" Label="PO Number" />
                                    <px:PXGridColumn DataField="FADetails__BillNumber" Label="Bill Number" />
                                    <px:PXGridColumn DataField="FADetails__Manufacturer" Label="Manufacturer" />
                                    <px:PXGridColumn DataField="FADetails__SerialNumber" Label="Serial Number" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="FADetails__TagNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCCCCCCC" Label="Tag Number"
                                        Width="80px" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Usage" Visible="False">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXSelector CommitChanges="True" ID="edUsageSchedule" runat="server" DataField="UsageScheduleID" AllowEdit="True" AutoRefresh="True" />
                    <px:PXFormView ID="formAssetUsage" runat="server" DataSourceID="ds" SkinID="Transparent" DataMember="AssetDetails" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXNumberEdit ID="edTotalExpectedUsage" runat="server" DataField="TotalExpectedUsage" />
                        </Template>
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
                    <px:PXFormView ID="formAssetNextUsage" runat="server" DataSourceID="ds" SkinID="Transparent" DataMember="AssetDetails" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXDateTimeEdit CommitChanges="True" ID="edLastMeasurementUsageDate" runat="server" DataField="LastMeasurementUsageDate" />
                            <px:PXDateTimeEdit ID="edNextMeasurementUsageDate" runat="server" DataField="NextMeasurementUsageDate" />
                        </Template>
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
                    <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
                    <px:PXGrid SkinID="DetailsWithFilter" ID="gridUsage" runat="server" DataSourceID="ds" AllowPaging="True" AdjustPageSize="Auto"
                        ActionsPosition="Top" AllowSearch="True">
                        <Levels>
                            <px:PXGridLevel DataMember="AssetUsage" DataKeyNames="AssetID,Number">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXCheckBox SuppressLabel="True" ID="chkDepreciated" runat="server" DataField="Depreciated" Enabled="False" />
                                    <px:PXSelector ID="edMeasurementBy" runat="server" DataField="MeasuredBy" AllowEdit="True" AutoRefresh="True" />
                                    <px:PXDateTimeEdit CommitChanges="True" ID="edMeasurementDate" runat="server" DataField="MeasurementDate" />
                                    <px:PXDateTimeEdit ID="edScheduledDate1" runat="server" DataField="ScheduledDate" />
                                    <px:PXNumberEdit CommitChanges="True" ID="edValue" runat="server" DataField="Value" />
                                    <px:PXNumberEdit ID="edDepreciationPercent" runat="server" DataField="DepreciationPercent" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="MeasuredBy" Label="Measured By" />
                                    <px:PXGridColumn Width="90px" DataField="ScheduledDate" />
                                    <px:PXGridColumn Width="90px" DataField="MeasurementDate" AutoCallBack="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="Value" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="UsageUOM" DisplayFormat="&gt;aaaaaa" Label="UOM" />
                                    <px:PXGridColumn AllowNull="False" DataField="DepreciationPercent" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Depreciated" Label="Depreciated" TextAlign="Center" Type="CheckBox" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Depreciation History" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid runat="server" DataSourceID="ds" BorderWidth="0px" SkinID="Details" Style="z-index: 100; height: 327px;" Width="100%"
                        ID="gridAssetHistory" AdjustPageSize="Auto" AllowPaging="True" AutoGenerateColumns="Append" AllowAutoHide="True">
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="AssetHistory">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXMaskEdit ID="edFinPeriodID" runat="server" DataField="FinPeriodID" InputMask="##-####" />
								</RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="FinPeriodID" DisplayFormat="##-####" Label="Period" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Depreciation History" RepaintOnDemand="false">
                <Template>
                    <px:PXFormView ID="formSheetFilter" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
                        DataMember="deprbookfilter" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
							<px:PXSelector CommitChanges="True" ID="edBookID1" runat="server" DataField="BookID" AutoRefresh="True"/>
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid runat="server" DataSourceID="ds" SkinID="DetailsWithFilter" Style="z-index: 100; height: 327px; left: 0px; top: 0px;"
                        Width="100%" ID="gridBookSheetHistory" AdjustPageSize="Auto" AllowPaging="False" AutoGenerateColumns="Append" PageSize="40" AllowAutoHide="True">
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="BookSheetHistory">
								<Columns />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Location History" RepaintOnDemand="false">
                <Template>
					<px:PXGrid SkinID="Details" ID="gridLocationHistory" BorderWidth="0px" runat="server" DataSourceID="ds" Style="z-index: 100; height: 327px;"
						Width="100%" AllowPaging="True" AdjustPageSize="Auto" ActionsPosition="Top" AllowSearch="True">
                        <Mode AllowAddNew="False" AllowUpdate="False" AllowDelete="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="LocationHistory">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXDateTimeEdit ID="edTransactionDate1" runat="server" DataField="TransactionDate" />
                                    <px:PXSelector ID="edClassID" runat="server" DataField="ClassID" AllowEdit="True" AutoRefresh="True" />
                                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AllowEdit="True" AutoRefresh="True" />
                                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AllowEdit="True" AutoRefresh="True" />
                                    <px:PXSelector ID="edBuildingID" runat="server" DataField="BuildingID" AllowEdit="True" AutoRefresh="True" />
                                    <px:PXTextEdit ID="edFloor" runat="server" DataField="Floor" />
                                    <px:PXTextEdit ID="edRoom" runat="server" DataField="Room" />
                                    <px:PXSelector ID="edEmployeeID" runat="server" DataField="EmployeeID" AllowEdit="True" AutoRefresh="True" />
                                    <px:PXSelector ID="edDepartment" runat="server" DataField="Department" AllowEdit="True" AutoRefresh="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn Width="99px" DataField="TransactionDate" />
                                    <px:PXGridColumn Width="80px" DataField="PeriodID" />
                                    <px:PXGridColumn Width="99px" DataField="ClassID" />
                                    <px:PXGridColumn Width="99px" DataField="RefNbr" />
                                    <px:PXGridColumn Width="80px" DataField="LocationID" DisplayFormat="&gt;AAAAAA" />
                                    <px:PXGridColumn DataField="BuildingID" />
                                    <px:PXGridColumn DataField="Floor" />
                                    <px:PXGridColumn DataField="Room" />
                                    <px:PXGridColumn DataField="EmployeeID" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="EmployeeID_EPEmployee_acctName" Label="Employee Name" />
                                    <px:PXGridColumn DataField="Department" DisplayFormat="&gt;aaaaaaaaaa" />
                                    <px:PXGridColumn DataField="Reason" />
                                    <px:PXGridColumn DataField="FAAccountID" />
                                    <px:PXGridColumn DataField="FASubID" />
                                    <px:PXGridColumn DataField="AccumulatedDepreciationAccountID" />
                                    <px:PXGridColumn DataField="AccumulatedDepreciationSubID" />
                                    <px:PXGridColumn DataField="DepreciatedExpenseAccountID" />
                                    <px:PXGridColumn DataField="DepreciatedExpenseSubID" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="LastModifiedByID_Modifier_username" Label="Last Modified By" />
                                    <px:PXGridColumn DataField="LastModifiedDateTime" Label="LastModifiedDateTime" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Transaction History" Visible="True">
                <Template>
                    <px:PXFormView ID="formFATranFilter" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
                        DataMember="bookfilter" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                            <px:PXSelector CommitChanges="True" ID="edBookID" runat="server" DataField="BookID" AutoRefresh="True" />
                        </Template>
                    </px:PXFormView>
					<px:PXGrid SkinID="DetailsWithFilter" ID="gridTranHistory" runat="server" DataSourceID="ds" Style="z-index: 100; height: 327px; top: 0px; left: 0px;"
						Width="100%" AllowPaging="True" AdjustPageSize="Auto" ActionsPosition="Top" AllowSearch="True"
                        SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="FATransactions">
                                <Columns>
                                    <px:PXGridColumn DataField="BookID" Label="Book" />
                                    <px:PXGridColumn DataField="TranDate" Label="Tran. Date" />
                                    <px:PXGridColumn DataField="FinPeriodID" DisplayFormat="##-####" Label="Tran. Period" />
                                    <px:PXGridColumn DataField="TranType" Label="Transaction Type" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="DebitAccountID" DisplayFormat="&gt;######" Label="Debit Account" />
                                    <px:PXGridColumn DataField="DebitAccountID_Account_description" />
                                    <px:PXGridColumn DataField="DebitSubID" DisplayFormat="&gt;AA-AA-AA-AA-AAA" Label="Debit Subaccount" />
                                    <px:PXGridColumn DataField="DebitSubID_Sub_description" />
                                    <px:PXGridColumn DataField="CreditAccountID" DisplayFormat="&gt;######" Label="Credit Account" />
                                    <px:PXGridColumn DataField="CreditAccountID_Account_description" />
                                    <px:PXGridColumn DataField="CreditSubID" DisplayFormat="&gt;AA-AA-AA-AA-AAA" Label="Credit Subaccount" />
                                    <px:PXGridColumn DataField="CreditSubID_Sub_description" />
                                    <px:PXGridColumn AllowNull="False" DataField="TranAmt" Label="Transaction Amount" TextAlign="Right" />
									<px:PXGridColumn AllowUpdate="False" DataField="RefNbr" Label="Reference Number" LinkCommand="ViewDocument" />
                                    <px:PXGridColumn DataField="BatchNbr" Label="Batch Nbr." LinkCommand="ViewBatch" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Released" Label="Released" TextAlign="Center" Type="CheckBox"
                                        Width="60px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="MethodDesc" Label="Method" />
                                    <px:PXGridColumn DataField="TranDesc" Label="Description" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Reconciliation" Visible="True" LoadOnDemand="True">
                <Template>
                    <px:PXFormView ID="formGLTranFilter" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DefaultControlID="edReconType"
                        CaptionVisible="False" SkinID="Transparent" DataMember="GLTrnFilter">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                            <px:PXDropDown CommitChanges="True" ID="edReconType" runat="server" AllowNull="False" DataField="ReconType" />
                            <px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
                            <px:PXSegmentMask CommitChanges="True" ID="edSubID" runat="server" DataField="SubID" />
                            <px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate" CommitChanges="True" />
                            <px:PXSelector ID="edPeriodID" runat="server" DataField="PeriodID" CommitChanges="True" AutoCallBack="True" />
							<px:PXCheckBox ID="chkShowReconciled" runat="server" DataField="ShowReconciled" CommitChanges="True" />
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                            <px:PXNumberEdit ID="edAcquisitionCost" runat="server" DataField="AcquisitionCost" Enabled="False" />
                            <px:PXNumberEdit ID="edCurrentCost" runat="server" DataField="CurrentCost" Enabled="False" />
                            <px:PXNumberEdit ID="edAccrualBalance" runat="server" DataField="AccrualBalance" Enabled="False" />
                            <px:PXNumberEdit ID="edUnreconciledAmt" runat="server" DataField="UnreconciledAmt" Enabled="False" />
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                            <px:PXNumberEdit ID="edSelectionAmt" runat="server" DataField="SelectionAmt" Enabled="False" />
                            <px:PXNumberEdit ID="edExpectedCost" runat="server" Enabled="False" DataField="ExpectedCost" />
                            <px:PXNumberEdit ID="edExpectedAccrualBal" runat="server" Enabled="False" DataField="ExpectedAccrualBal" />
                            <px:PXButton ID="btnReduceCost" runat="server" CommandName="reduceUnreconCost" CommandSourceID="ds" Text="Reduce Unreconciled Cost" />
                            
                        </Template>
                    </px:PXFormView>
					<px:PXGrid SkinID="Inquire" ID="gridGLTran" BorderWidth="0px" runat="server" DataSourceID="ds" Style="z-index: 100; height: 327px; top: 0px; left: 0px; border-top-width: 1px"
						Width="100%" AllowPaging="True" AdjustPageSize="Auto" ActionsPosition="Top"
                        AllowSearch="True">
                        <Levels>
                            <px:PXGridLevel DataMember="DsplAdditions">
                                <Columns>
                                    <px:PXGridColumn DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" Width="60px" AutoCallBack="True"
                                        AllowCheckAll="True" />
                                    <px:PXGridColumn DataField="Component" Label="Component" TextAlign="Center" Type="CheckBox" Width="60px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="ClassID" Label="Asset Class" AutoCallBack="True" Width="108px" />
									<px:PXGridColumn DataField="Reconciled" TextAlign="Center" Type="CheckBox" Width="60px" AutoCallBack="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="FAAccrualTran__GLTranBranchID" DisplayFormat="&gt;AAAAAAAAAA" Label="Transaction Branch" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="FAAccrualTran__GLTranInventoryID" DisplayFormat="&gt;AAAAAAAAAAAA" Label="Inventory ID" />
                                    <px:PXGridColumn DataField="FAAccrualTran__GLTranUOM" DisplayFormat="&gt;aaaaaa" Label="UOM" />
                                    <px:PXGridColumn DataField="SelectedQty" Label="Selected Quantity" TextAlign="Right" Width="100px" AutoCallBack="True" AllowNull="False" />
                                    <px:PXGridColumn DataField="SelectedAmt" Label="Selected Amount" TextAlign="Right" Width="100px" AutoCallBack="True" AllowNull="False" />
                                    <px:PXGridColumn DataField="OpenQty" Label="Open Quantity" TextAlign="Right" Width="100px" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="OpenAmt" Label="Open Amount" TextAlign="Right" Width="100px" AllowUpdate="False" />
                                    <px:PXGridColumn AllowUpdate="False" AutoCallBack="True" DataField="GLTranQty" Label="Quantity" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn AllowUpdate="False" AutoCallBack="True" DataField="UnitCost" Label="Unit Cost" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn AllowUpdate="False" AutoCallBack="True" DataField="GLTranAmt" Label="Amount" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="FAAccrualTran__GLTranDate" Label="Transaction Date" Width="90px" />
                                    <px:PXGridColumn DataField="FAAccrualTran__GLTranDesc" Label="Transaction Description" Width="200px" />
                                    <px:PXGridColumn DataField="FAAccrualTran__GLTranModule" Label="Original Transaction Module" Width="100px" />
                                    <px:PXGridColumn DataField="FAAccrualTran__GLTranBatchNbr" Label="Original Transaction Batch Number" Width="100px" />
                                    <px:PXGridColumn DataField="FAAccrualTran__GLTranRefNbr" Label="Original Transaction Reference Number" Width="100px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar PagerVisible="False">
                            <CustomItems>
                                <px:PXToolBarButton Text="Process" CommandName="processAdditions" CommandSourceID="ds" />
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
    </px:PXTab>
    <px:PXSmartPanel ID="spDisposeParamDlg" runat="server" Key="DispParams" AutoCallBack-Command="Refresh" AutoCallBack-Target="DisposePrm" AutoCallBack-Enabled="true"
        LoadOnDemand="True" AcceptButtonID="cbOk" CancelButtonID="cbCancel" Caption="Disposal Parameters" CaptionVisible="True"
        DesignView="Content" CommandName="DisposalOK" CommandSourceID="ds" AllowResize="False" Width="440px" Height="290px">
        <px:PXFormView ID="DisposePrm" runat="server" DataSourceID="ds" Style="z-index: 108" Width="100%" DataMember="DispParams"
            Caption="Dispose Parameters" SkinID="Transparent">
            <Activity Height="" HighlightColor="" SelectedColor="" Width="" />
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edDisposalDate" runat="server" DataField="DisposalDate" />
                <px:PXSelector CommitChanges="True" ID="edDisposalPeriodID" runat="server" DataField="DisposalPeriodID" />
                <px:PXDropDown CommitChanges="True" ID="edActionBeforeDisposal" runat="server" DataField="ActionBeforeDisposal" />
                <px:PXNumberEdit CommitChanges="True" ID="edDisposalAmt" runat="server" AllowNull="True" DataField="DisposalAmt" />
                <px:PXSelector CommitChanges="True" ID="edDisposalMethodID" runat="server" DataField="DisposalMethodID" />
                <px:PXSegmentMask CommitChanges="True" ID="edDisposalAccountID" runat="server" DataField="DisposalAccountID" />
                <px:PXSegmentMask CommitChanges="True" ID="edDisposalSubID" runat="server" DataField="DisposalSubID" AutoRefresh="True" />
                <px:PXTextEdit ID="edReason" runat="server" DataField="Reason" />
            </Template>
        </px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">       
            <px:PXButton ID="cbOk" runat="server" Text="OK" DialogResult="OK" />
			<px:PXButton ID="cbCancel" runat="server" Text="Cancel" DialogResult="Cancel" />
       </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="pnlSuspendParams" runat="server" Key="SuspendParams" AutoCallBack-Command="Refresh" AutoCallBack-Target="frmSuspendParams" AutoCallBack-Enabled="true"
        LoadOnDemand="True" AcceptButtonID="cbOk" CancelButtonID="cbCancel" Caption="Suspend Parameters" CaptionVisible="True">
        <px:PXFormView ID="frmSuspendParams" runat="server" DataSourceID="ds" Style="z-index: 108" Width="100%" DataMember="SuspendParams"
            Caption="Dispose Parameters" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
                <px:PXSelector CommitChanges="True" ID="edCurrentPeriodID" runat="server" DataField="CurrentPeriodID" />
            </Template>
        </px:PXFormView>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">        
            <px:PXButton ID="PXButton1" runat="server" Text="OK" DialogResult="OK" />
            <px:PXButton ID="PXButton2" runat="server" Text="Cancel" DialogResult="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="spRevDispInfoDlg" runat="server" Key="RevDispInfo"
        AutoCallBack-Command="Refresh" AutoCallBack-Target="RevDispInfo" AutoCallBack-Enabled="true"
        LoadOnDemand="True" AcceptButtonID="cbOk" CancelButtonID="cbCancel" Caption="Reverse Disposal Info"
		CaptionVisible="True" HideAfterAction="False" DesignView="Content">
        <div style="padding: 5px">
        <px:PXFormView ID="RevDispInfo" runat="server" DataSourceID="ds" Style="z-index: 108"
            Width="100%" DataMember="RevDispInfo" Caption="Reverse Disposal Info" SkinID="Transparent">
            <Activity Height="" HighlightColor="" SelectedColor="" Width="" />
            <Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />

                <px:PXDateTimeEdit ID="edDisposalDate" runat="server" DataField="DisposalDate" Enabled="False" />
                <px:PXMaskEdit ID="edDisposalPeriodID" runat="server" DataField="DisposalPeriodID" Enabled="False" />
                <px:PXNumberEdit ID="edDisposalAmt" runat="server" AllowNull="True" DataField="DisposalAmt" Enabled="False" />
                <px:PXSelector ID="edDisposalMethodID" runat="server" DataField="DisposalMethodID" Enabled="False" />
                <px:PXSegmentMask ID="edDisposalAccountID" runat="server" DataField="DisposalAccountID" Enabled="False" />
                <px:PXSegmentMask ID="edDisposalSubID" runat="server" DataField="DisposalSubID" Enabled="False" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edReverseDisposalDate" runat="server" DataField="ReverseDisposalDate" Enabled="True" />
					<px:PXSelector CommitChanges="True" ID="edReverseDisposalPeriodID" runat="server" DataField="ReverseDisposalPeriodID" />
				</Template>
        </px:PXFormView>
        </div>
		<px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">        
			<px:PXButton ID="PXButton3" runat="server" Text="OK" DialogResult="OK" />
			<px:PXButton ID="PXButton4" runat="server" Text="Cancel" DialogResult="Cancel" />
		</px:PXPanel> 
    </px:PXSmartPanel>
</asp:Content>
