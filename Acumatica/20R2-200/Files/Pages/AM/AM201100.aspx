<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="AM201100.aspx.cs" Inherits="Page_AM201100" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" PrimaryView="OrderType" TypeName="PX.Objects.AM.AMOrderTypeMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="Delete" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Previous" />
            <px:PXDSCallbackCommand Name="Next" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="OrderType" Caption="Order Types"
        DefaultControlID="edOrderType" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" TabIndex="100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector Size="xs" ID="edOrderType" runat="server" DataField="OrderType" CommitChanges="True" />
            <px:PXCheckBox CommitChanges="True" ID="chkActive" runat="server" DataField="Active" />
            <px:PXLayoutRule runat="server" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr"/>
            <px:PXDropDown ID="edFunction" runat="server" DataField="Function"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="CurrentOrderType">
		<Items>
			<px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="M" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Numbering Settings" />
                    <px:PXSelector ID="edProdNumberingID" runat="server" DataField="ProdNumberingID" AllowEdit="True"/>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Account Settings" LabelsWidth="XM" />
                    <px:PXSegmentMask ID="edWIPAcctID" runat="server" DataField="WIPAcctID" />
		            <px:PXSegmentMask ID="edWIPSubID" runat="server" DataField="WIPSubID" DataKeyNames="Value" />
		            <px:PXSegmentMask ID="edWIPVarianceAcct" runat="server" DataField="WIPVarianceAcctID" />
		            <px:PXSegmentMask ID="edWIPVarianceSub" runat="server" DataField="WIPVarianceSubID" DataKeyNames="Value" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Order Defaults" />
                    <px:PXDropDown ID="edDefaultCostMethod" runat="server" DataField="DefaultCostMethod"  />
                    <px:PXCheckBox ID="edExcludeFromMRP" runat="server" DataField="ExcludeFromMRP" />
                    <px:PXCheckBox ID="edSubstituteWorkCenters" runat="server" DataField="SubstituteWorkCenters" />
                    <px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="XM" StartGroup="True" GroupCaption="Printing"/>
                    <px:PXSelector ID="edProductionReportID" runat="server" DataField="ProductionReportID" ValueField="ScreenID" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Scheduling" />
                    <px:PXCheckBox ID="edCheckSchdMatlAvailability" runat="server" DataField="CheckSchdMatlAvailability" />
                    <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" GroupCaption="SCRAP" LabelsWidth="M" ControlSize="M" />
                    <px:PXDropDown ID="edScrapSource" runat="server" DataField="ScrapSource"  />
                    <px:PXSegmentMask ID="edScrapSiteID" runat="server" DataField="ScrapSiteID" AllowEdit="True" CommitChanges="True" AutoRefresh="True" AutoCallBack="True"  />
                    <px:PXSegmentMask ID="edScrapLocationID" runat="server" DataField="ScrapLocationID" AllowEdit="True" AutoRefresh="True" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="COPY BOM NOTES" />
                    <px:PXGroupBox ID="modulesGroupBox" runat="server" RenderStyle="Simple" RenderSimple="True">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True"/>
                            <px:PXCheckBox ID="PXCopyNotesItem" runat="server" DataField="CopyNotesItem" AlignLeft="True"/>
                            <px:PXCheckBox ID="PXCopyNotesOper" runat="server" DataField="CopyNotesOper" AlignLeft="True"/>
                            <px:PXLayoutRule runat="server" StartColumn="True"/>
                            <px:PXCheckBox ID="PXCopyNotesMatl" runat="server" DataField="CopyNotesMatl" AlignLeft="True"/>
                            <px:PXCheckBox ID="PXCopyNotesStep" runat="server" DataField="CopyNotesStep" AlignLeft="True"/>
                            <px:PXLayoutRule runat="server" StartColumn="True"/>
                            <px:PXCheckBox ID="PXCopyNotesTool" runat="server" DataField="CopyNotesTool" AlignLeft="True"/>
                            <px:PXCheckBox ID="PXCopyNotesOvhd" runat="server" DataField="CopyNotesOvhd" AlignLeft="True"/>
                        </Template>
                    </px:PXGroupBox>	
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="DATA ENTRY SETTINGS" LabelsWidth="M" ControlSize="S" />
                    <px:PXDropDown ID="edUnderIssueMaterial" runat="server" DataField="UnderIssueMaterial" />
                    <px:PXDropDown ID="edBackflushUnderIssueMaterial" runat="server" DataField="BackflushUnderIssueMaterial" />
                    <px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="S" Merge="True" />
                    <px:PXDropDown ID="edOverIssueMaterial" runat="server" DataField="OverIssueMaterial" CommitChanges="True" />
                    <px:PXCheckBox ID="edIncludeUnreleasedOverIssueMaterial" runat="server" DataField="IncludeUnreleasedOverIssueMaterial" AlignLeft="True"/>
                    <px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="S" />
                    <px:PXDropDown ID="edIssueMaterialOnTheFly" runat="server" DataField="IssueMaterialOnTheFly"  />
                    <px:PXDropDown ID="edMoveCompletedOrders" runat="server" DataField="MoveCompletedOrders" />
                    <px:PXDropDown ID="edOverCompleteOrders" runat="server" DataField="OverCompleteOrders" />
                    <px:PXCheckBox ID="edDefaultOperationMoveQty" runat="server" DataField="DefaultOperationMoveQty" />
                </Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="AttributesGrid" runat="server" DataSourceID="ds" SkinID="DetailsInTab" AutoAdjustColumns="True" SyncPosition="true" Width="100%">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="OrderType, LineNbr" DataMember="OrderTypeAttributes">
                                <Columns>
                                    <px:PXGridColumn DataField="OrderType"/>
                                    <px:PXGridColumn DataField="LineNbr"/>
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
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXTab>
</asp:Content>
