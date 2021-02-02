<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN204000.aspx.cs"
    Inherits="Page_IN204000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INSiteMaint" PrimaryView="site">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="ViewRestrictionGroups" Visible="False" />
            <px:PXDSCallbackCommand Name="ValidateAddresses" Visible="False" StartNewGroup="True" />
            <px:PXDSCallbackCommand Visible="False" Name="INLocationLabels" />
            <px:PXDSCallbackCommand Visible="False" Name="ViewOnMap" />
            <px:PXDSCallbackCommand Name="Action" StartNewGroup="true" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Report" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlChangeID" runat="server" Caption="Specify New ID"
		CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="ChangeIDDialog" CreateOnDemand="false" AutoCallBack-Enabled="true"
		AutoCallBack-Target="formChangeID" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
		AcceptButtonID="btnOK">
		<px:PXFormView ID="formChangeID" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
			DataMember="ChangeIDDialog">
			<ContentStyle BackColor="Transparent" BorderStyle="None" />
			<Template>
				<px:PXLayoutRule ID="rlAcctCD" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
				<px:PXSegmentMask ID="edAcctCD" runat="server" DataField="CD" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="pnlChangeIDButton" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK">
				<AutoCallBack Target="formChangeID" Command="Save" />
			</px:PXButton>
			<px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />						
		</px:PXPanel>
	</px:PXSmartPanel>
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="site" Caption="Warehouse Summary"
        NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" DefaultControlID="edSiteCD"
        TabIndex="6900" AllowCollapse="true" Expanded="True">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
            <px:PXSegmentMask ID="edSiteCD" runat="server" DataField="SiteCD" AutoRefresh="True" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" DataSourceID="ds" />
            <px:PXSelector ID="edReplenishmentClassID" runat="server" DataField="ReplenishmentClassID" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXDropDown ID="edLocationValid" runat="server" AllowNull="False" DataField="LocationValid" />
            <px:PXDropDown ID="edAvgDefaultCost" runat="server" AllowNull="False" DataField="AvgDefaultCost" />
            <px:PXDropDown ID="edFIFODefaultCost" runat="server" AllowNull="False" DataField="FIFODefaultCost" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="621px" DataSourceID="ds" DataMember="siteaccounts">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Location Table">
                <Template>
                    <px:PXPanel ID="locationForm" runat="server" Style="z-index: 100" Width="100%" CaptionVisible="False">
                        <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="SM" LabelsWidth="SM" />
                        <px:PXSegmentMask ID="edReceiptLocationID" runat="server" DataField="ReceiptLocationID" DataSourceID="ds" AutoRefresh="True" CommitChanges="True">
                            <AutoCallBack Command="Save" Target="tab" />
                        </px:PXSegmentMask>
                        <px:PXSegmentMask ID="edShipLocationID" runat="server" DataField="ShipLocationID" DataSourceID="ds" AutoRefresh="True" CommitChanges="True">
                            <AutoCallBack Command="Save" Target="tab" />
                        </px:PXSegmentMask>
                        <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="SM" LabelsWidth="SM" />
                        <px:PXSegmentMask ID="edReturnLocationID" runat="server" DataField="ReturnLocationID" DataSourceID="ds"  AutoRefresh="True" CommitChanges="True"/>
                        <px:PXSegmentMask ID="edDropShipLocationID" runat="server" DataField="DropShipLocationID" DataSourceID="ds"  AutoRefresh="True" CommitChanges="True"/>
                    </px:PXPanel>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%"
                        ActionsPosition="Top" TabIndex="150" SkinID="DetailsWithFilter" Caption="Location Table" AdjustPageSize="Auto" AllowPaging="True">
                        <ActionBar>
                            <Actions>
                                <FilesMenu Enabled="True" />
                            </Actions>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="location" DataKeyNames="SiteID,LocationCD">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" />
                                    <px:PXSegmentMask ID="edLocationCD" runat="server" DataField="LocationCD" Width="108px">
                                        <GridProperties FastFilterFields="Descr" />
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" TabIndex="11" Width="351px" />
                                    <px:PXCheckBox ID="chkSalesValid" runat="server" Checked="True" DataField="SalesValid" Text="Sales Allowed" />
                                    <px:PXCheckBox ID="chkReceiptsValid" runat="server" Checked="True" DataField="ReceiptsValid" Text="Receipts Allowed" />
                                    <px:PXCheckBox ID="chkTransfersValid" runat="server" Checked="True" DataField="TransfersValid" Text="Transfers Allowed" />
                                    <px:PXDropDown ID="edPrimaryItemValid" runat="server" AllowNull="False" DataField="PrimaryItemValid" Width="126px" />
                                    <px:PXSegmentMask ID="edPrimaryItemID" runat="server" DataField="PrimaryItemID" Width="108px" AllowEdit="True">
                                        <GridProperties FastFilterFields="Descr" />
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edPrimaryItemClassID" runat="server" DataField="PrimaryItemClassID" Width="63px" />
                                    <px:PXNumberEdit ID="edPickPriority" runat="server" DataField="PickPriority" ValueType="Int16" Width="54px" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="LocationCD" DisplayFormat="&gt;aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" Width="108px"/>
                                    <px:PXGridColumn DataField="Descr" Width="351px"/>
                                    <px:PXGridColumn AllowNull="False" DataField="Active" DataType="Boolean" DefValueText="True" TextAlign="Center" Type="CheckBox"
                                        Width="60px"/>
                                    <px:PXGridColumn AllowNull="False" DataField="InclQtyAvail" DataType="Boolean" DefValueText="True" TextAlign="Center" Type="CheckBox"
                                        Width="80px"/>
                                    <px:PXGridColumn AllowNull="False" DataField="IsCosted" DataType="Boolean" DefValueText="True" TextAlign="Center" Type="CheckBox"
                                        Width="80px"/>
                                    <px:PXGridColumn AllowNull="False" DataField="SalesValid" DataType="Boolean" DefValueText="True" TextAlign="Center" Type="CheckBox"
                                        Width="80px"/>
                                    <px:PXGridColumn AllowNull="False" DataField="ReceiptsValid" DataType="Boolean" DefValueText="True" TextAlign="Center" Type="CheckBox"
                                        Width="80px"/>
                                    <px:PXGridColumn AllowNull="False" DataField="TransfersValid" DataType="Boolean" DefValueText="True" TextAlign="Center" Type="CheckBox"
                                        Width="80px"/>
                                    <px:PXGridColumn AllowNull="False" DataField="AssemblyValid" DataType="Boolean" DefValueText="True" TextAlign="Center" Type="CheckBox"
                                        Width="60px"/>
                                    <px:PXGridColumn AllowNull="False" DataField="PickPriority" DataType="Int16" TextAlign="Right" Width="80px" CommitChanges="true" />
                                    <px:PXGridColumn AllowNull="False" DataField="PrimaryItemValid" DefValueText="N" RenderEditorText="True" Width="126px"/>
                                    <px:PXGridColumn DataField="PrimaryItemID" Width="108px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="PrimaryItemClassID" CommitChanges="true" Width="63px" />
                                    <px:PXGridColumn DataField="ProjectID" Width="108px"  AutoCallBack="True"/>
                                    <px:PXGridColumn DataField="TaskID" Width="108px"/>
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowUpload="True"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Carts">
                <Template>
                    <px:PXGrid ID="gridCarts" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%"
                        ActionsPosition="Top" TabIndex="150" SkinID="DetailsWithFilter" Caption="Carts" AdjustPageSize="Auto" AllowPaging="True">
                        <ActionBar>
                            <Actions>
                                <FilesMenu Enabled="True" />
                            </Actions>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="carts" DataKeyNames="SiteID,CartCD">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" />
                                    <px:PXSelector ID="edCartCD" runat="server" DataField="CartCD" Width="108px">
                                        <GridProperties FastFilterFields="Descr" />
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edCartDescr" runat="server" DataField="Descr" TabIndex="11" Width="351px" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="CartCD" DisplayFormat="&gt;aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" Width="108px"/>
                                    <px:PXGridColumn DataField="Descr" Width="351px"/>
                                    <px:PXGridColumn AllowNull="False" DataField="Active" DataType="Boolean" DefValueText="True" TextAlign="Center" Type="CheckBox" Width="60px"/>
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowUpload="True"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="GL Accounts">
                <Template>
                    <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="M" StartColumn="True" />
                    <px:PXCheckBox ID="chkOverrideAccSub" runat="server" DataField="OverrideInvtAccSub" />
                    <px:PXSegmentMask ID="edInvtAcctID" runat="server" DataField="InvtAcctID" AutoRefresh="True">
                        <Items>
                            <px:PXMaskItem EditMask="Numeric" Length="4" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edInvtSubID" runat="server" DataField="InvtSubID" AutoRefresh="True">
                        <Items>
                            <px:PXMaskItem EditMask="AlphaNumeric" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INSite.invtAcctID" PropertyName="DataControls[&quot;edInvtAcctID&quot;].Value" Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edReasonCodeSubID" runat="server" DataField="ReasonCodeSubID">
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <Items>
                            <px:PXMaskItem EditMask="AlphaNumeric" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                        </Items>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edSalesAcctID" runat="server" DataField="SalesAcctID">
                        <Items>
                            <px:PXMaskItem EditMask="Numeric" Length="4" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" AutoRefresh="True">
                        <Items>
                            <px:PXMaskItem EditMask="AlphaNumeric" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INSite.salesAcctID" PropertyName="DataControls[&quot;edSalesAcctID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edCOGSAcctID" runat="server" DataField="COGSAcctID">
                        <Items>
                            <px:PXMaskItem EditMask="Numeric" Length="4" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edCOGSSubID" runat="server" DataField="COGSSubID" AutoRefresh="True">
                        <Items>
                            <px:PXMaskItem EditMask="AlphaNumeric" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INSite.cOGSAcctID" PropertyName="DataControls[&quot;edCOGSAcctID&quot;].Value" Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edStdCstVarAcctID" runat="server" DataField="StdCstVarAcctID">
                        <Items>
                            <px:PXMaskItem EditMask="Numeric" Length="4" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edStdCstVarSubID" runat="server" DataField="StdCstVarSubID" AutoRefresh="True">
                        <Items>
                            <px:PXMaskItem EditMask="AlphaNumeric" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INSite.stdCstVarAcctID" PropertyName="DataControls[&quot;edStdCstVarAcctID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edStdCstRevAcctID" runat="server" DataField="StdCstRevAcctID">
                        <Items>
                            <px:PXMaskItem EditMask="Numeric" Length="4" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edStdCstRevSubID" runat="server" DataField="StdCstRevSubID" AutoRefresh="True">
                        <Items>
                            <px:PXMaskItem EditMask="AlphaNumeric" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INSite.stdCstRevAcctID" PropertyName="DataControls[&quot;edStdCstRevAcctID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edPOAccrualAcctID" runat="server" DataField="POAccrualAcctID">
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <Items>
                            <px:PXMaskItem EditMask="Numeric" Length="4" Separator="-" TextCase="Upper" />
                        </Items>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edPOAccrualSubID" runat="server" DataField="POAccrualSubID" AutoRefresh="True">
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <Items>
                            <px:PXMaskItem EditMask="AlphaNumeric" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                        </Items>
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INSite.pOAccrualAcctID" PropertyName="DataControls[&quot;edPOAccrualAcctID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edPPVAcctID" runat="server" DataField="PPVAcctID">
                        <Items>
                            <px:PXMaskItem EditMask="Numeric" Length="4" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edPPVSubID" runat="server" DataField="PPVSubID" AutoRefresh="True">
                        <Items>
                            <px:PXMaskItem EditMask="AlphaNumeric" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INSite.pPVAcctID" PropertyName="DataControls[&quot;edPPVAcctID&quot;].Value" Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edDiscAcctID" runat="server" DataField="DiscAcctID">
                        <Items>
                            <px:PXMaskItem EditMask="Numeric" Length="4" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edDiscSubID" runat="server" DataField="DiscSubID" AutoRefresh="True">
                        <Items>
                            <px:PXMaskItem EditMask="AlphaNumeric" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INSite.discAcctID" PropertyName="DataControls[&quot;edDiscAcctID&quot;].Value" Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edMiscAcctID" runat="server" DataField="MiscAcctID" AllowEdit="True">
                        <Items>
                            <px:PXMaskItem EditMask="Numeric" Length="4" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edMiscSubID" runat="server" DataField="MiscSubID" AutoRefresh="True">
                        <Items>
                            <px:PXMaskItem EditMask="AlphaNumeric" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INSite.miscAcctID" PropertyName="DataControls[&quot;edMiscAcctID&quot;].Value" Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edFreightAcctID" runat="server" DataField="FreightAcctID" AllowEdit="True">
                        <Items>
                            <px:PXMaskItem EditMask="Numeric" Length="4" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edFreightSubID" runat="server" DataField="FreightSubID" AutoRefresh="True">
                        <Items>
                            <px:PXMaskItem EditMask="AlphaNumeric" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                        </Items>
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INSite.freightAcctID" PropertyName="DataControls[&quot;edFreightAcctID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edLCVarianceAcctID" runat="server" DataField="LCVarianceAcctID">
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <Items>
                            <px:PXMaskItem EditMask="Numeric" Length="4" Separator="-" TextCase="Upper" />
                        </Items>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edLCVarianceSubID" runat="server" DataField="LCVarianceSubID" AutoRefresh="True">
                        <GridProperties FastFilterFields="Description">
                            <Layout ColumnsMenu="False" />
                        </GridProperties>
                        <Items>
                            <px:PXMaskItem EditMask="AlphaNumeric" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                            <px:PXMaskItem EditMask="AlphaNumeric" Length="2" Separator="-" TextCase="Upper" />
                        </Items>
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INSite.lCVarianceAcctID" PropertyName="DataControls[&quot;edLCVarianceAcctID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Address Information">
                <Template>
                    <px:PXFormView runat="server" DataSourceID="ds" DataMember="Contact" ID="colForm" RenderStyle="Simple" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" GroupCaption="Contact" StartGroup="True" ControlSize="XM" LabelsWidth="SM" />
                            <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
                            <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
                            <px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommitChanges="True"/>
                            <px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
                            <px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
                            <px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" />
                            <px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" />
                        </Template>
                    </px:PXFormView>
                    <px:PXFormView runat="server" DataSourceID="ds" Caption="Building" DataMember="siteaccounts" ID="buildForm" RenderStyle="Simple" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Building" ControlSize="XM" LabelsWidth="SM" />
                            <px:PXSelector ID="edBuilding" runat="server" DataField="BuildingID" AutoRefresh="True" />
                        </Template>
                    </px:PXFormView>
                    <px:PXFormView ID="addrForm" runat="server" Caption="Address" DataMember="Address" DataSourceID="ds" RenderStyle="Simple" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" GroupCaption="Address" StartGroup="True" ControlSize="XM" LabelsWidth="SM" />
                            <px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                            <px:PXSelector CommitChanges="True" ID="edCountryID" runat="server" DataField="CountryID" AllowEdit="True" DataSourceID="ds" />
                            <px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" AllowEdit="True" DataSourceID="ds">
                                <CallBackMode PostData="Container" />
                            </px:PXSelector>
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXMaskEdit Size="s" ID="edPostalCode" runat="server" DataField="PostalCode" CommitChanges="True" />
                            <px:PXButton Size="xs" ID="btnViewMainOnMap" runat="server" CommandName="ViewOnMap" CommandSourceID="ds" Text="View on Map">
                            </px:PXButton>
                            <px:PXLayoutRule runat="server" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
