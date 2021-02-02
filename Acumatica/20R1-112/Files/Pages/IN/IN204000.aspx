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
            <px:PXDSCallbackCommand Name="ViewTotesInCart" Visible="False" />
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
						<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="SM" LabelsWidth="SM" />
						<px:PXCheckBox ID="chkUseItemDefaultLocationForPicking" runat="server" DataField="UseItemDefaultLocationForPicking" />
                    </px:PXPanel>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" SkinID="DetailsWithFilter" Caption="Location Table" AdjustPageSize="Auto" AllowPaging="True" Width="100%" SyncPosition="true">
                        <ActionBar>
                            <Actions>
                                <FilesMenu Enabled="True" />
                            </Actions>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="location" DataKeyNames="SiteID,LocationCD">
                                <RowTemplate>
                                    <px:PXSegmentMask ID="edLocationCD" runat="server" DataField="LocationCD">
                                        <GridProperties FastFilterFields="Descr" />
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
                                    <px:PXCheckBox ID="chkSalesValid" runat="server" Checked="True" DataField="SalesValid" />
                                    <px:PXCheckBox ID="chkReceiptsValid" runat="server" Checked="True" DataField="ReceiptsValid" />
                                    <px:PXCheckBox ID="chkTransfersValid" runat="server" Checked="True" DataField="TransfersValid" />
                                    <px:PXDropDown ID="edPrimaryItemValid" runat="server" AllowNull="False" DataField="PrimaryItemValid" />
                                    <px:PXSegmentMask ID="edPrimaryItemID" runat="server" DataField="PrimaryItemID" AllowEdit="True">
                                        <GridProperties FastFilterFields="Descr" />
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edPrimaryItemClassID" runat="server" DataField="PrimaryItemClassID" />
                                    <px:PXNumberEdit ID="edPickPriority" runat="server" DataField="PickPriority" ValueType="Int16" />
                                    <px:PXSegmentMask ID="edProjectID" runat="server" DataField="ProjectID" Width="63px" />
                                    <px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" Width="63px" AutoRefresh="true" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="LocationCD"/>
                                    <px:PXGridColumn DataField="Descr"/>
                                    <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox"/>
                                    <px:PXGridColumn DataField="IsSorting" TextAlign="Center" Type="CheckBox"/>
                                    <px:PXGridColumn DataField="InclQtyAvail" TextAlign="Center" Type="CheckBox"/>
                                    <px:PXGridColumn DataField="IsCosted" TextAlign="Center" Type="CheckBox"/>
                                    <px:PXGridColumn DataField="SalesValid" TextAlign="Center" Type="CheckBox"/>
                                    <px:PXGridColumn DataField="ReceiptsValid" TextAlign="Center" Type="CheckBox"/>
                                    <px:PXGridColumn DataField="TransfersValid" TextAlign="Center" Type="CheckBox"/>
                                    <px:PXGridColumn DataField="AssemblyValid" TextAlign="Center" Type="CheckBox"/>
                                    <px:PXGridColumn DataField="PickPriority" CommitChanges="true" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PathPriority" CommitChanges="true" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PrimaryItemValid" />
                                    <px:PXGridColumn DataField="PrimaryItemID" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="PrimaryItemClassID" CommitChanges="true" />
                                    <px:PXGridColumn DataField="ProjectID" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="TaskID"/>
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
                    <px:PXGrid ID="gridCarts" runat="server" DataSourceID="ds" SkinID="DetailsWithFilter" AdjustPageSize="Auto" AllowPaging="True" Width="100%" SyncPosition="True" SyncPositionWithGraph="True">
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Assigned Totes" DependOnGrid="gridCarts">
                                    <AutoCallBack Command="ViewTotesInCart" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="carts" DataKeyNames="SiteID,CartCD">
                                <RowTemplate>
                                    <px:PXSelector ID="edCartCD" runat="server" DataField="CartCD"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="CartCD"/>
                                    <px:PXGridColumn DataField="Descr"/>
                                    <px:PXGridColumn DataField="AssignedNbrOfTotes" />
                                    <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowUpload="True"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Totes">
                <Template>
                    <px:PXGrid ID="gridTotes" runat="server" DataSourceID="ds" SkinID="DetailsWithFilter" AdjustPageSize="Auto" AllowPaging="True" Width="100%">
                        <Levels>
                            <px:PXGridLevel DataMember="totes" DataKeyNames="SiteID,ToteCD">
                                <RowTemplate>
                                    <px:PXSelector ID="edToteCD" runat="server" DataField="ToteCD"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ToteCD"/>
                                    <px:PXGridColumn DataField="Descr"/>
                                    <px:PXGridColumn DataField="AssignedCartID"/>
                                    <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox"/>
                                </Columns>
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
                    <px:PXSegmentMask ID="edInvtAcctID" runat="server" DataField="InvtAcctID" AutoRefresh="True" CommitChanges="true">
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
                    <px:PXSegmentMask ID="edSalesAcctID" runat="server" DataField="SalesAcctID" CommitChanges="true">
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
                    <px:PXSegmentMask ID="edCOGSAcctID" runat="server" DataField="COGSAcctID" CommitChanges="true">
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
                    <px:PXSegmentMask ID="edStdCstVarAcctID" runat="server" DataField="StdCstVarAcctID" CommitChanges="true">
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
                    <px:PXSegmentMask ID="edStdCstRevAcctID" runat="server" DataField="StdCstRevAcctID" CommitChanges="true">
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
                    <px:PXSegmentMask ID="edPOAccrualAcctID" runat="server" DataField="POAccrualAcctID" CommitChanges="true">
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
                    <px:PXSegmentMask ID="edPPVAcctID" runat="server" DataField="PPVAcctID" CommitChanges="true">
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
                    <px:PXSegmentMask ID="edLCVarianceAcctID" runat="server" DataField="LCVarianceAcctID" CommitChanges="true">
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
    
    <%-- Totes in Cart Dialog --%>
    <px:PXSmartPanel ID="ViewTotesInCartDialog" runat="server" Caption="Assigned Totes" CaptionVisible="True" Key="totesInCart" LoadOnDemand="True" AutoReload="True" AutoRepaint="True">
        <px:PXGrid ID="gridTotesInCart" runat="server" DataSourceID="ds" SkinID="Inquire" AdjustPageSize="Auto" AllowPaging="True" NoteIndicator="False" FilesIndicator="False" SyncPosition="True">
            <Levels>
                <px:PXGridLevel DataMember="totesInCart" DataKeyNames="SiteID,ToteCD">
                    <Columns>
                        <px:PXGridColumn DataField="ToteCD"/>
                        <px:PXGridColumn DataField="Descr"/>
                        <px:PXGridColumn DataField="Active" Type="CheckBox"/>
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="150" />
            <Mode AllowUpload="False" AllowAddNew="False" AllowUpdate="False" AllowDelete="False"/>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnOk" runat="server" DialogResult="OK" Text="OK"/>
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
