<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="AM201500.aspx.cs" Inherits="Page_AM201500" Title="Production Order Maint" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:pxdatasource ID="ds" runat="server" Visible="True" BorderStyle="NotSet" PrimaryView="ProdMaintRecords" TypeName="PX.Objects.AM.ProdMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Release" />
            <px:PXDSCallbackCommand Name="Plan" />
		    <px:PXDSCallbackCommand Name="ReleaseMaterial" Visible="False" />
		    <px:PXDSCallbackCommand Name="Disassemble" Visible="False" />
		    <px:PXDSCallbackCommand Name="CreateLinkedOrders" Visible="False" />
            <px:PXDSCallbackCommand Name="ConfigureEntry" Visible="False" PostData="Self" ContainerID="form" />
            <px:PXDSCallbackCommand Name="Reconfigure" Visible="False" PostData="Self" ContainerID="form" />
		    <px:PXDSCallbackCommand CommitChanges="True" Name="LSAMProdItem_generateLotSerial" Visible="False" />
		    <px:PXDSCallbackCommand CommitChanges="True" Name="LSAMProdItem_binLotSerial" Visible="False" />
		    <px:PXDSCallbackCommand CommitChanges="True" Name="LSAMProdMatl_generateLotSerial" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="LSAMProdMatl_binLotSerial" Visible="False" />
            <px:PXDSCallbackCommand Visible="false" Name="AMProdMatlSplit$RefNoteID$Link" DependOnGrid="grid2" CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="false" Name="AMProdEvnt$RefNoteID$Link" DependOnGrid="grid" CommitChanges="True" />
		</CallbackCommands>
	</px:pxdatasource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:pxformview ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="ProdMaintRecords" 
        DataKeyNames="ProdOrdID" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" NoteIndicator="True" 
                   NotifyIndicator="True" DefaultControlID="edOrderType">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" AllowEdit="True" />
            <px:PXSelector ID="edProdOrdID" runat="server" DataField="ProdOrdID" AutoRefresh="True" DataSourceID="ds">
                <GridProperties FastFilterFields="InventoryID,InventoryItem__Descr,CustomerID,Customer__AcctName">
                    <Layout ColumnsMenu="False" />
                </GridProperties>
            </px:PXSelector>
            <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True"/>
            <px:PXSegmentMask CommitChanges="True" ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True"/>
            <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edItemLocationID" runat="server" DataField="LocationID" AutoRefresh="True" AllowEdit="True"/>
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr"/>
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="XM" />
            <px:PXDateTimeEdit ID="edProdDate" runat="server" CommitChanges="True" DataField="ProdDate" />
            <px:PXLayoutRule ID="PXLayoutRule33" runat="server" StartColumn="False" Merge="true" LabelsWidth="SM" ControlSize="S"/>
            <px:PXDropDown CommitChanges="True" ID="edStatusIDDropDown" runat="server" DataField="StatusID" />
            <px:PXCheckBox CommitChanges="True" ID="edHoldCheckBox" runat="server" DataField="Hold" />
            <px:PXLayoutRule ID="PXLayoutRule20" runat="server" StartColumn="False" LabelsWidth="SM" ControlSize="XM"/>
            <px:PXSelector CommitChanges="True" ID="edProductWorkgroupID" runat="server" DataField="ProductWorkgroupID" />
            <px:PXSelector ID="edProductManagerID" runat="server" DataField="ProductManagerID" AutoRefresh="True" />
        </Template>
        <Searches>
            <px:PXControlParam ControlID="form" Name="ProdOrdID" PropertyName="NewDataKey[&quot;ProdOrdID&quot;]"
                Type="String" />
        </Searches>
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
	</px:pxformview>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:pxtab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="ProdItemSelected" DataKeyNames="ProdOrdID" DynamicTabs="False">
		<Items>
			<px:PXTabItem Text="General" >
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="S" />
                    <px:PXNumberEdit ID="edQtytoProd" runat="server"  DataField="QtytoProd" CommitChanges="True" />
                    <px:PXSelector ID="edUnitDesc" runat="server" DataField="UOM" />
                    <px:PXNumberEdit ID="edQtyComplete" runat="server" DataField="QtyComplete" Enabled="False" />
                    <px:PXNumberEdit ID="edQtyScrapped" runat="server" DataField="QtyScrapped" Enabled="False" />
                    <px:PXNumberEdit ID="edQtyRemaining2" runat="server" DataField="QtyRemaining" Enabled="False"  />
                    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="XM" />
                    <px:PXDropDown ID="edSchedulingMethod" runat="server" AllowNull="False" DataField="SchedulingMethod" CommitChanges="True" />
                    <px:PXDateTimeEdit CommitChanges="True" ID="edConstDate" runat="server" DataField="ConstDate"  />
                    <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate"  />
                    <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate"  />
                    <px:PXCheckBox ID="chkFMLTime" runat="server" DataField="FMLTime" />
                    <px:PXCheckBox ID="chkFMLTMRPOrdorOP" runat="server" DataField="FMLTMRPOrdorOP" />
                    <px:PXCheckBox ID="edExcludeFromMRP" runat="server" DataField="ExcludeFromMRP" />
                    <px:PXNumberEdit ID="edSchPriority" runat="server" DataField="SchPriority" CommitChanges="True" />
                    <px:PXDropDown ID="edCostMethod" runat="server" DataField="CostMethod" CommitChanges="True" />
                    <px:PXCheckBox runat="server" DataField="ScrapOverride" ID="edScrapOverride" CommitChanges="True" />
			        <px:PXSegmentMask runat="server" ID="edScrapSiteID" DataField="ScrapSiteID" AllowEdit="True" CommitChanges="True" AutoRefresh="True" AutoCallBack="True" />
			        <px:PXSegmentMask runat="server" ID="edScrapLocationID" DataField="ScrapLocationID" AllowEdit="True" AutoRefresh="True" CommitChanges="True" />    
                </Template>
			</px:PXTabItem>
			<px:PXTabItem Text="References">
                <Template>
                    <%--First Column--%>
                    <px:PXLayoutRule ID="PXLayoutRule14" runat="server" GroupCaption="SO REFERENCES" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="True" />
                    <px:PXTextEdit ID="edOrdTypeRef" runat="server" DataField="OrdTypeRef" />
                    <px:PXSelector ID="edOrdNbr" runat="server" DataField="OrdNbr" AllowEdit="True" />
                    <px:PXNumberEdit ID="edOrdLineRef" runat="server" DataField="OrdLineRef"/>

                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" GroupCaption="LINKED ORDERS" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSelector ID="edProductOrderType" runat="server" DataField="ProductOrderType" AllowEdit="True" CommitChanges="True" />
                    <px:PXSelector ID="edProductOrdID" runat="server" DataField="ProductOrdID" AllowEdit="True" AutoRefresh="True" />
                    <px:PXSelector ID="edParentOrderType" runat="server" DataField="ParentOrderType" AllowEdit="True" CommitChanges="True" />
                    <px:PXSelector ID="edParentOrdID" runat="server" DataField="ParentOrdID" AllowEdit="True" AutoRefresh="True" />

                    <px:PXLayoutRule ID="PXLayoutRule9" runat="server" GroupCaption="FINANCIAL SETTINGS" LabelsWidth="M" ControlSize="XM" />
			        <px:PXSegmentMask ID="edWIPAcctID" runat="server"  DataField="WIPAcctID" CommitChanges="True" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edWIPSubID" runat="server" DataField="WIPSubID" DataKeyNames="Value" CommitChanges="True" AutoRefresh="True" />
                    <px:PXSegmentMask runat="server" DataField="WIPVarianceAcctID" CommitChanges="True" ID="edWIPVarianceAcctID" />
	                <px:PXSegmentMask runat="server" DataField="WIPVarianceSubID" AutoRefresh="True" CommitChanges="True" ID="edWIPVarianceSubID" />
                    
                    <%--Second Column--%>
                    <px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="True" GroupCaption="Source" LabelsWidth="M" ControlSize="XM" />
                    <px:PXDropDown ID="edDetailSource" runat="server" DataField="DetailSource" CommitChanges="true" />
                    <px:PXDateTimeEdit CommitChanges="True" ID="edBOMEffDate" runat="server" AllowNull="False" DataField="BOMEffDate" />
                    <px:PXSelector ID="edBOMID" runat="server" DataField="BOMID" CommitChanges="True" AllowEdit="True" AutoRefresh="True" />
                    <px:PXSelector CommitChanges="True" ID="edBOMRevisionID" runat="server" AutoRefresh="True" DataField="BOMRevisionID"/>
                    <px:PXSelector ID="edEstimateID" runat="server" DataField="EstimateID" CommitChanges="True" AllowEdit="True" />
                    <px:PXSelector CommitChanges="True" ID="edEstimateRevisionID" runat="server" AutoRefresh="True" DataField="EstimateRevisionID"/>
                    <px:PXSelector ID="edSourceOrderType" runat="server" DataField="SourceOrderType" AllowEdit="True" CommitChanges="True" />
                    <px:PXSelector ID="edSourceProductionNbr" runat="server" DataField="SourceProductionNbr" AllowEdit="True" AutoRefresh="True" />
                    <px:PXFormView ID="FVConfigure" runat="server" DataMember="ItemConfiguration" RenderStyle="Simple" DataSourceID="ds">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRuleConfig" runat="server" LabelsWidth="M" ControlSize="XM" />
                            <px:PXSelector ID="edConfigurationID" runat="server" DataField="ConfigurationID" CommitChanges="True" AutoRefresh="True"/>
                            <px:PXSelector ID="edConfigurationRevisionID" runat="server" DataField="Revision" CommitChanges="True" AutoRefresh="True" />
                            <px:PXSelector ID="edKeyID" runat="server" DataField="KeyID" CommitChanges="True" AutoRefresh="True" />
                            <px:PXLayoutRule runat="server" Merge="True" LabelsWidth="M" ControlSize="XM" />
                            <px:PXButton ID="pbConfigure" runat="server" Text="Configure" CommandName="ConfigureEntry" CommandSourceID="ds" ></px:PXButton>
                            <px:PXButton ID="pbReconfigure" runat="server" Text="Delete Config." CommandName="Reconfigure" CommandSourceID="ds" ></px:PXButton>
                        </Template>
                    </px:PXFormView>
                    <px:PXLayoutRule ID="PXLayoutRule10" runat="server" GroupCaption="Project" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSelector ID="edProjectID" runat="server" DataField="ProjectID" CommitChanges="True" AllowEdit="True" AutoRefresh="True" />
                    <px:PXSelector ID="edTaskID" runat="server" DataField="TaskID" CommitChanges="True" AutoRefresh="True" />
                    <px:PXSelector ID="edCostCodeID" runat="server" DataField="CostCodeID" CommitChanges="True" AutoRefresh="True" />
                    <px:PXCheckBox ID="edUpdateProject" runat="server" DataField="UpdateProject" />
                </Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Event History">
                <Template>
                    <px:PXGrid ID="grid" runat="server" AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True"
                        DataSourceID="ds" SkinID="DetailsInTab" Width="100%" TabIndex="2100" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="ProdEventRecords">
                                <RowTemplate>
                                    <px:PXTextEdit ID="edDescription" runat="server"  DataField="Description"  />
                                    <px:PXTextEdit ID="edCreatedByScreenIDTitle" runat="server" DataField="CreatedByScreenIDTitle"/>
                                    <px:PXMaskEdit ID="edCreatedByScreenID" runat="server" DataField="CreatedByScreenID" InputMask="aa.aa.aa.aa" />
                                    <px:PXTextEdit ID="edCrtdUser" runat="server" AllowNull="False" DataField="CreatedByID" />
                                    <px:PXDropDown ID="edEventType" runat="server" DataField="EventType"/>
                                    <px:PXDateTimeEdit ID="CreatedDateTime" runat="server" DataField="EventDate"/>
                                    <px:PXSelector ID="edRefBatNbr" runat="server" DataField="RefBatNbr"  AutoRefresh="True" AllowEdit="True"/>
                                    <px:PXDropDown ID="edRefDocType" runat="server" DataField="RefDocType"/>
                                    <px:PXTextEdit ID="edEvntLineNbr" runat="server"  DataField="LineNbr"  />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Width="130px" />
                                    <px:PXGridColumn DataField="EventType" Width="130px" TextAlign="Left"/>
                                    <px:PXGridColumn DataField="Description" Width="250px" />
                                    <px:PXGridColumn DataField="CreatedByScreenIDTitle" Width="225px" />
                                    <px:PXGridColumn DataField="CreatedByScreenID" DisplayFormat="aa.aa.aa.aa" MaxLength="10" Width="108px"/>
                                    <px:PXGridColumn DataField="CreatedByID" Width="115px"/>
                                    <px:PXGridColumn DataField="RefBatNbr" Width="108px"/>
                                    <px:PXGridColumn DataField="RefDocType" Width="108px"/>
                                    <px:PXGridColumn DataField="RefNoteID" RenderEditorText="True" LinkCommand="AMProdEvnt$RefNoteID$Link" Width="175px" />
                                    <px:PXGridColumn DataField="LineNbr" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="300" />
                        <Mode InitNewRow="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="AttributesGrid" runat="server" DataSourceID="ds" SkinID="DetailsInTab" AutoAdjustColumns="True" SyncPosition="true" Width="100%">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="OrderType,ProdOrdID,LineNbr" DataMember="ProductionAttributes">
                                <RowTemplate>
                                    <px:PXSelector ID="edAttributeID" runat="server" DataField="AttributeID" AllowEdit="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="OrderType"/>
                                    <px:PXGridColumn DataField="ProdOrdID"/>
                                    <px:PXGridColumn DataField="LineNbr"/>
                                    <px:PXGridColumn DataField="Level" TextAlign="Left" Width="90px" />
                                    <px:PXGridColumn DataField="OperationID" TextAlign="Left" Width="75px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="Source" TextAlign="Left" Width="75px" />
                                    <px:PXGridColumn DataField="AttributeID" Width="120px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="Label" Width="120px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="Descr" Width="200px" />
                                    <px:PXGridColumn DataField="Enabled" TextAlign="Center" Type="CheckBox" Width="80px" />
                                    <px:PXGridColumn DataField="TransactionRequired" TextAlign="Center" Type="CheckBox" Width="85px" />
                                    <px:PXGridColumn DataField="Value" Width="220px" MatrixMode="true" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <Mode AllowUpload="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Totals" >
                <Template >
                    <px:PXFormView ID="totalsform" runat="server" CaptionVisible="False" DataMember="ProdTotalRecs" DataSourceID="ds" 
                        SkinID="Transparent" TabIndex="-3236" Width="100%" DataKeyNames="ProdOrdID" >
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule16" runat="server" GroupCaption="Planned" StartColumn="True" LabelsWidth="125px" />
                            <px:PXMaskEdit ID="edPlanLaborTime" runat="server" DataField="PlanLaborTime" Width="150px" />
                            <px:PXNumberEdit ID="edPlanLabor" runat="server" DataField="PlanLabor" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edPlanMachine" runat="server" DataField="PlanMachine" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edPlanMaterial" runat="server" DataField="PlanMaterial" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edPlanTool" runat="server" DataField="PlanTool" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edPlanFixedOverhead" runat="server" DataField="PlanFixedOverhead" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edPlanVariableOverhead" runat="server" DataField="PlanVariableOverhead" Width="150px" />
                            <px:PXNumberEdit ID="edPlanSubcontract" runat="server" DataField="PlanSubcontract" Width="150px" />
                            <px:PXNumberEdit ID="edPlanQtyToProduce" runat="server" DataField="PlanQtyToProduce" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edPlanTotal" runat="server" DataField="PlanTotal" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edPlanUnitCost" runat="server" DataField="PlanUnitCost" Size="" Width="150px" />
                            <px:PXDateTimeEdit ID="edPlanCostDate" runat="server" DataField="PlanCostDate" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edPlanReferenceMaterial" runat="server" DataField="PlanReferenceMaterial" Width="150px" />
                            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" GroupCaption="Actual" StartColumn="True" LabelsWidth="125px" />
                            <px:PXMaskEdit ID="edActualLaborTime" runat="server" DataField="ActualLaborTime" Width="150px" />
                            <px:PXNumberEdit ID="edActualLabor" runat="server" DataField="ActualLabor" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edActualMachine" runat="server" DataField="ActualMachine" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edActualMaterial" runat="server" DataField="ActualMaterial" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edActualTool" runat="server" DataField="ActualTool" Size="" Width="150px"/>
                            <px:PXNumberEdit ID="edActualFixedOverhead" runat="server" DataField="ActualFixedOverhead" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edActualVariableOverhead" runat="server" DataField="ActualVariableOverhead" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edActualSubcontract" runat="server" DataField="ActualSubcontract" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edQtyCompleteTotals" runat="server" DataField="QtyComplete" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edWIPAdjustment" runat="server" DataField="WIPAdjustment" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edScrapAmount" runat="server" DataField="ScrapAmount" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edWIPTotal" runat="server" DataField="WIPTotal" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edWIPComp" runat="server" DataField="WIPComp" Size="" Width="150px" />
                            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" GroupCaption="Variance" StartColumn="True" LabelsWidth="125px" />
                            <px:PXMaskEdit ID="edVarianceLaborTime" runat="server" DataField="VarianceLaborTime" Width="150px" />
                            <px:PXNumberEdit ID="edVarianceLabor" runat="server" DataField="VarianceLabor" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edsVarianceMachine" runat="server" DataField="VarianceMachine" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edVarianceMaterial" runat="server" DataField="VarianceMaterial" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edVarianceTool" runat="server" DataField="VarianceTool" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edVarianceFixedOverhead" runat="server" DataField="VarianceFixedOverhead" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edVarianceVariableOverhead" runat="server" DataField="VarianceVariableOverhead" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edVarianceSubcontract" runat="server" DataField="VarianceSubcontract" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edQtyRemaining" runat="server" DataField="QtyRemaining" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edVarianceTotal" runat="server" DataField="VarianceTotal" Size="" Width="150px" />
                            <px:PXNumberEdit ID="edWIPBalance" runat="server" DataField="WIPBalance" Size="" Width="150px" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Allocations">
            <Template>
                <px:PXFormView ID="lsoptform" runat="server" Width="100%" DataMember="LSAMProdItem_lotseropts"
                    DataSourceID="ds" SkinID="Transparent">
                    <Template>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                        <px:PXNumberEdit ID="edUnassignedQty2" runat="server" DataField="UnassignedQty" Enabled="False" />
                        <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty">
                            <AutoCallBack>
                                <Behavior CommitChanges="True" />
                            </AutoCallBack>
                        </px:PXNumberEdit>
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                        <px:PXMaskEdit ID="edStartNumVal2" runat="server" DataField="StartNumVal" />
                        <px:PXButton ID="btnGenerate2" runat="server" Text="Generate" CommandName="LSAMProdItem_generateLotSerial"
                            CommandSourceID="ds" />
                    </Template>
                </px:PXFormView>
                <px:PXGrid runat="server" ID="serialNumbersGrid" DataSourceID="ds" BorderStyle="None" Width="100%" SkinID="DetailsInTab"> 
                    <Mode InitNewRow="True" />
                    <Levels>
                        <px:PXGridLevel DataMember="splits" DataKeyNames="OrderType,ProdOrdID,SplitLineNbr">
                            <RowTemplate>
                                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                <px:PXDateTimeEdit ID="edExpireDateDsn" runat="server" DataField="ExpireDate" DisplayFormat="d" />
                                <px:PXSegmentMask ID="edInventoryIDsn" runat="server" DataField="InventoryID" AllowEdit="True">
                                </px:PXSegmentMask>
                                <px:PXSegmentMask ID="edSubItemIDsn" runat="server" DataField="SubItemID" />
                                <px:PXSegmentMask ID="edLocationIDsn" runat="server" DataField="LocationID" />
                                <px:PXSelector ID="edLotSerialNbrsn" runat="server" DataField="LotSerialNbr" />
                                <px:PXSelector ID="edUOMsn" runat="server" DataField="UOM" Enabled="False" />
                                <px:PXNumberEdit ID="edQtysn" runat="server" DataField="Qty" />

                            </RowTemplate>
                            <Columns>
                                <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-&gt;AA" Width="81px" />
                                <px:PXGridColumn DataField="LocationID" AllowShowHide="Server" DisplayFormat="&gt;CCCCCC-CCCC" Width="81px" />
                                <px:PXGridColumn DataField="LotSerialNbr" AllowShowHide="Server" Width="198px" />
                                <px:PXGridColumn AllowUpdate="False" DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="63px" />
                                <px:PXGridColumn AllowNull="False" DataField="Qty" TextAlign="Right" Width="90px" />
                                <px:PXGridColumn DataField="ExpireDate" DisplayFormat="d" Width="90px" />
                            </Columns>
                            <Layout FormViewHeight="" />
                        </px:PXGridLevel>
                    </Levels>
                    <AutoSize Enabled="True" MinHeight="150" />
                </px:PXGrid>
            </Template>
        </px:PXTabItem>
		</Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
	    <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
	</px:pxtab>
</asp:Content>

