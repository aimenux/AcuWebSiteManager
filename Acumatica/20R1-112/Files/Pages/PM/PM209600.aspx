<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM209600.aspx.cs"
    Inherits="Page_PM209600" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.ForecastMaint" PrimaryView="Revisions"
        BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="AddPeriods" Visible="false" DependOnGrid="grid" />        
            <px:PXDSCallbackCommand CommitChanges="True" Name="SettleBalances" Visible="false" DependOnGrid="grid" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="AddMissingLines" Visible="false" DependOnGrid="grid" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Revisions" Caption="Forecast Revision">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" ColumnWidth="L" />
             <px:PXSegmentMask ID="edProjectID" runat="server" DataField="ProjectID" DataSourceID="ds" AutoRefresh="True" AllowEdit="True"  NullText="<SELECT>">
            </px:PXSegmentMask>
            <px:PXSelector ID="edRevisionID" runat="server" DataField="RevisionID" AutoRefresh="true"/>
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" ColumnWidth="L" />
            <px:PXSelector CommitChanges="True" ID="edProjectTaskID" runat="server" DataField="Filter.ProjectTaskID" />
            <px:PXDropDown CommitChanges="True" ID="edAccountGroupType" runat="server" DataField="Filter.AccountGroupType" />
            <px:PXLayoutRule runat="server" StartColumn="True" ColumnWidth="L" />
			<px:PXSelector CommitChanges="True" ID="edAccountGroupID" runat="server" DataField="Filter.AccountGroupID" />
            <px:PXSelector CommitChanges="True" ID="edInventoryID" runat="server" DataField="Filter.InventoryID" />
            <px:PXSelector CommitChanges="True" ID="edCostCodeID" runat="server" DataField="Filter.CostCodeID" />
			<px:PXSelector CommitChanges="True" ID="edCuryID" runat="server" DataField="Project.CuryID" />
        </Template>
    </px:PXFormView>    
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top"
        SkinID="Details" SyncPosition="True" OnRowDataBound="Grid_RowDataBound" AllowPaging="True" AdjustPageSize="None" PageSize="200" AllowFilter="False">
        <Levels>
            <px:PXGridLevel DataMember="Items" Visible="True">
                <Styles><ActiveRow CssClass="SummaryRecord"></ActiveRow></Styles>
                <Columns>
                    <px:PXGridColumn DataField="ProjectTask" AllowFilter="false" />
                    <px:PXGridColumn DataField="AccountGroup" AllowFilter="false"/>
                    <px:PXGridColumn DataField="Inventory" AllowFilter="false"/>
                    <px:PXGridColumn DataField="CostCode" AllowFilter="false"/>
                    <px:PXGridColumn DataField="Description" AllowFilter="false"/>
                    <px:PXGridColumn DataField="PlannedStartDate" AllowFilter="false"/>
                    <px:PXGridColumn DataField="PlannedEndDate" AllowFilter="false"/>
                    <px:PXGridColumn DataField="Period" AllowSort="false" AllowFilter="false" />
                    <px:PXGridColumn DataField="Qty" CommitChanges="true" AllowSort="false" AllowFilter="false"/>
                    <px:PXGridColumn DataField="CuryAmount" CommitChanges="true" AllowSort="false" AllowFilter="false"/>
                    <px:PXGridColumn DataField="RevisedQty" CommitChanges="true" AllowSort="false" AllowFilter="false"/>
                    <px:PXGridColumn DataField="CuryRevisedAmount" CommitChanges="true" AllowSort="false" AllowFilter="false"/>
                    <px:PXGridColumn DataField="DraftChangeOrderQty" AllowSort="false" AllowFilter="false"/>
                    <px:PXGridColumn DataField="CuryDraftChangeOrderAmount" AllowSort="false" AllowFilter="false"/>
                    <px:PXGridColumn DataField="ChangeOrderQty" AllowSort="false" AllowFilter="false"/>
                    <px:PXGridColumn DataField="CuryChangeOrderAmount" AllowSort="false" AllowFilter="false"/>
                    <px:PXGridColumn DataField="ActualQty" AllowSort="false" AllowFilter="false"/>
                    <px:PXGridColumn DataField="CuryActualAmount" AllowSort="false" AllowFilter="false"/>
                    <px:PXGridColumn DataField="VarianceQuantity" AllowSort="false" AllowFilter="false"/>
                    <px:PXGridColumn DataField="CuryVarianceAmount" AllowSort="false" AllowFilter="false"/>
                </Columns>
                <RowTemplate>
                    <px:PXSegmentMask ID="edInventoryIDGrid" runat="server" DataField="InventoryID" />
                </RowTemplate>
            </px:PXGridLevel>
        </Levels>
        <ActionBar>
            <CustomItems>
                <px:PXToolBarButton Key="cmdAddPeriods">
                    <AutoCallBack Command="AddPeriods" Target="ds" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Key="cmdSettleBalances">
                    <AutoCallBack Command="SettleBalances" Target="ds" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Key="cmdAddMissingLines">
                    <AutoCallBack Command="AddMissingLines" Target="ds" />
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowUpload="true" InitNewRow="false" AllowAddNew="false" AllowDelete="true" />
    </px:PXGrid>
</asp:Content>
<asp:Content ID="Dialogs" ContentPlaceHolderID="phDialogs" runat="Server">
	<px:PXSmartPanel ID="PanelAddPeriods" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="Add Periods"
		CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="AddPeriodDialog" AutoCallBack-Enabled="true" AutoCallBack-Target="formAddPeriods" AutoCallBack-Command="Refresh"
		CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel">
		<px:PXFormView ID="formAddPeriods" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Add Periods" CaptionVisible="False" SkinID="Transparent"
			DataMember="AddPeriodDialog">
			<Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
                <px:PXSelector ID="edStartPeriod" runat="server" DataField="StartPeriodID" CommitChanges="True" />
                <px:PXSelector ID="edEndPeriod" runat="server" DataField="EndPeriodID" CommitChanges="True" />	
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
                <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" CommitChanges="True" />
                <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" CommitChanges="True" />					
			</Template>
            <Parameters>
                <px:PXSyncGridParam ControlID="grid" />
            </Parameters>
		</px:PXFormView>
		<div style="padding: 5px; text-align: right;">
			<px:PXButton ID="PXButton3" runat="server" Text="OK" DialogResult="OK" Width="63px" Height="20px" DependOnGrid="grid"></px:PXButton>
			<px:PXButton ID="PXButton4" runat="server" DialogResult="No" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
		</div>
	</px:PXSmartPanel>

    <px:PXSmartPanel ID="PanelCopyRevision" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="Copy Revision"
		CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="CopyDialog" AutoCallBack-Enabled="true" AutoCallBack-Target="formNewRevision" AutoCallBack-Command="Refresh"
		CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel">
		<px:PXFormView ID="formNewRevision" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="New Revision" CaptionVisible="False" SkinID="Transparent"
			DataMember="CopyDialog">
			<Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
				<px:PXMaskEdit CommitChanges="True" ID="edRevisionID" runat="server" DataField="RevisionID" />
           </Template>
        </px:PXFormView>
		<div style="padding: 5px; text-align: right;">
			<px:PXButton ID="PXButton1" runat="server" Text="OK" DialogResult="OK" Width="63px" Height="20px"></px:PXButton>
			<px:PXButton ID="PXButton2" runat="server" DialogResult="No" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
		</div>
	</px:PXSmartPanel>

    <px:PXSmartPanel ID="PanelDistribute" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="Distribute"
		CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="DistributeDialog" AutoCallBack-Enabled="true" AutoCallBack-Target="formDistribute" AutoCallBack-Command="Refresh"
		CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButton5" CancelButtonID="PXButton6">
		<px:PXFormView ID="formDistribute" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Distribute" CaptionVisible="False" SkinID="Transparent"
			DataMember="DistributeDialog">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" GroupCaption="Value" />
                <px:PXGroupBox ID="gbValueOption" runat="server" Caption="Value" CommitChanges="True" DataField="ValueOption" RenderStyle="Simple">
                    <Template>
                        <px:PXRadioButton ID="gbQtyOption_0" runat="server" GroupName="gbQtyOption" Value="0" Checked="true" />
                        <px:PXRadioButton ID="gbQtyOption_1" runat="server" GroupName="gbQtyOption" Value="1" />
                    </Template>
                </px:PXGroupBox>
                 <px:PXLayoutRule runat="server" GroupCaption="Columns" />
                 <px:PXCheckBox ID="edQty" runat="server" DataField="Qty" AlignLeft="true" CommitChanges="True"/>
                <px:PXCheckBox ID="edAmount" runat="server" DataField="Amount" AlignLeft="true" CommitChanges="True"/>
                <px:PXCheckBox ID="edRevisedQty" runat="server" DataField="RevisedQty" AlignLeft="true" CommitChanges="True"/>
                <px:PXCheckBox ID="edRevisedAmount" runat="server" DataField="RevisedAmount" AlignLeft="true" CommitChanges="True"/>
                    
                 <px:PXLayoutRule runat="server" GroupCaption="Rows" />
                <px:PXGroupBox ID="bgApplyOption" runat="server" Caption="Apply Distribution To" CommitChanges="True" DataField="ApplyOption" RenderStyle="Simple">
                    <Template>
                        <px:PXRadioButton ID="bgApplyOption_0" runat="server" GroupName="bgApplyOption" Value="0" Checked="true" />
                        <px:PXRadioButton ID="bgApplyOption_1" runat="server" GroupName="bgApplyOption" Value="1" />
                    </Template>
                    <ContentLayout LabelsWidth="S" />
                </px:PXGroupBox>
            </Template>
        </px:PXFormView>
		<div style="padding: 5px; text-align: right;">
			<px:PXButton ID="PXButton5" runat="server" Text="OK" DialogResult="OK" Width="63px" Height="20px"></px:PXButton>
			<px:PXButton ID="PXButton6" runat="server" DialogResult="No" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
		</div>
	</px:PXSmartPanel>
</asp:Content>
