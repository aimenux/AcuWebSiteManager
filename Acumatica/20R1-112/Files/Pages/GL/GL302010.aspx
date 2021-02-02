<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	CodeFile="GL302010.aspx.cs" Inherits="Page_GL302010" Title="Budget Entry" ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <script language="javascript" type="text/javascript">
        function showTreeChanged(owner, args)
    	{
    		var visible = owner.getValue();
    		if (window['px_alls'] && px_alls["sp1"]) px_alls["sp1"].setDisabledPanel(visible ? 0 : 1);
    	}

	    function showTreeChk(e)
	    {
	    	if (!loaded)
	    	{
	    		var px = __px_alls(this);
	    		var TreeView = px_alls["chkShowTree"];
	    		setTimeout(function ()
	    		{
	    			if (window['px_alls'] && px_alls["chkShowTree"] && px_alls["sp1"])
	    			{
	    				TreeView = px_alls["chkShowTree"];
	    				px_alls["sp1"].setDisabledPanel(TreeView.getValue() ? 0 : 1);
	    			}
	    			loaded = true;
	    		}, 1);
	    	}
	    }

	    function showTreeLoad(e)
	    {
	    	if (window['__px_alls'])
	    	{
	    		var pxs = __px_alls(this);
	    		if (pxs)
	    		{
	    			setTimeout(function ()
	    			{
	    				var frm = pxs["form"];
						if (frm) frm.events.addEventHandler("afterRepaint", showTreeChk);
	    			}, 100);
	    		}
	    	}
	    }
	    var loaded = false;
	    window.addEventListener("load", showTreeLoad);
	</script>
	<px:PXDataSource  EnableAttributes="true" ID="ds" Width="100%" runat="server" PrimaryView="Filter" TypeName="PX.Objects.GL.GLBudgetEntry"
		Visible="True" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" RepaintControls="All" RepaintControlsIDs="grid" />
			<px:PXDSCallbackCommand Name="distribute" Visible="False" CommitChanges="True" DependOnGrid="grid"
				RepaintControls="Bound" />
			<px:PXDSCallbackCommand Name="distributeOK" Visible="False" CommitChanges="True"
				DependOnGrid="grid" />
			<px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="preload" DependOnGrid="grid" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="showPreload" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="wNext" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="showManage" />
            <px:PXDSCallbackCommand Name="manageOK" Visible="False" CommitChanges="True"/>
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="Tree" TreeKeys="GroupID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Width="100%" DataMember="Filter" Caption="Budget Filter"
		LinkIndicator="True" NotifyIndicator="True" DataSourceID="ds">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM"
				ControlSize="M" />
			<px:PXSelector CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID"
				DataSourceID="ds" />
			<px:PXSelector CommitChanges="True" ID="edLedgerId" runat="server" DataField="LedgerId"
				DataSourceID="ds" AutoRefresh="True"/>
			<px:PXSelector CommitChanges="True" ID="edFinYear" runat="server" DataField="FinYear"
				DataSourceID="ds" AutoRefresh="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowTree" runat="server" DataField="ShowTree">
				<ClientEvents ValueChanged="showTreeChanged" />
			</px:PXCheckBox>
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" LabelsWidth="SM" StartColumn="True"
				ControlSize="M" />
			<px:PXSelector CommitChanges="True" ID="edCompareToBranchID" runat="server" DataField="CompareToBranchID"
				DataSourceID="ds" />
			<px:PXSelector CommitChanges="True" ID="edCompareToLedgerId" runat="server" DataField="CompareToLedgerID"
				DataSourceID="ds" AutoRefresh="True"/>
			<px:PXSelector CommitChanges="True" ID="edCompareToFiscalYear" runat="server" DataField="CompareToFinYear"
				DataSourceID="ds" />
			<px:PXSegmentMask CommitChanges="True" ID="edSubCD" runat="server" DataField="SubIDFilter"
				DataSourceID="ds" />
			<px:PXTextEdit CommitChanges="True" ID="edTreeFilter" runat="server" DataField="TreeNodeFilter"
				DataSourceID="ds" />
		</Template>
		<CallbackCommands>
			<Save PostData="Page" RepaintControls="All" RepaintControlsIDs="grid, tree" />
		</CallbackCommands>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="220" CollapseDirection="Panel1"
		Panel1MinSize="1" DisabledPanel="Panel1">
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<px:PXTreeView ID="tree" runat="server" PopulateOnDemand="True" ExpandDepth="3" DataSourceID="ds"
				ShowRootNode="false" SelectFirstNode="true" >
				<AutoCallBack Target="grid" Command="Refresh" />
                <Images>
                    <ParentImages Normal="tree@Folder" Selected="tree@FolderS" />
                    <LeafImages Normal="tree@Folder" Selected="tree@FolderS" />
                </Images>
				<DataBindings>
					<px:PXTreeItemBinding DataMember="Tree" TextField="Description" ValueField="GroupID" />
				</DataBindings>
				<AutoSize Enabled="True" />
			</px:PXTreeView>
		</Template1>
		<Template2>
			<px:PXGrid ID="grid" runat="server" Height="150px" Width="100%" Caption="Budget Articles"
				SkinID="Details" SyncPosition="True">
				<AutoCallBack Command="Refresh" Target="form" ActiveBehavior="True">
					<Behavior RepaintControlsIDs="ds" BlockPage="True" CommitChanges="True" />
				</AutoCallBack>
				<Levels>
					<px:PXGridLevel DataMember="BudgetArticles">
						<Columns>
							<px:PXGridColumn DataField="IsGroup" TextAlign="Center" Type="CheckBox" />
							<px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox" Width="53px"
								AutoCallBack="True" />
							<px:PXGridColumn DataField="AccountID" AutoCallBack="True" />
							<px:PXGridColumn DataField="SubID" AutoCallBack="True" />
							<px:PXGridColumn DataField="Description" AllowSort="False" />
							<px:PXGridColumn DataField="Amount" TextAlign="Right" />
							<px:PXGridColumn DataField="AllocatedAmount" TextAlign="Right" />
                            <px:PXGridColumn DataField="CreatedByID" AllowShowHide="True" />
                            <px:PXGridColumn DataField="LastModifiedByID" AllowShowHide="True" />
						</Columns>
						<RowTemplate>
							<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="SM"
								ControlSize="M" />
							<px:PXCheckBox ID="chkGroup" runat="server" DataField="IsGroup" />
							<px:PXCheckBox ID="chkReleased" runat="server" DataField="Released" />
							<px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" AutoRefresh="True" />
							<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" />
							<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
							<px:PXNumberEdit ID="edAmount" runat="server" DataField="Amount" />
							<px:PXNumberEdit ID="edAllocatedAmount" runat="server" DataField="AllocatedAmount" />
						</RowTemplate>
					</px:PXGridLevel>
				</Levels>
				<Parameters>
					<px:PXControlParam ControlID="tree" Name="GroupID" PropertyName="SelectedValue" />
				</Parameters>
				<AutoSize Enabled="True" Container="Parent" />
				<Mode AllowUpload="True" />
				<ActionBar>
					<CustomItems>
						<px:PXToolBarButton Text="Distribute">
							<AutoCallBack Command="distribute" Target="ds" />
						</px:PXToolBarButton>
					</CustomItems>
				</ActionBar>
			</px:PXGrid>
		</Template2>
	</px:PXSplitContainer>
	<px:PXSmartPanel ID="spDistributeDlg" runat="server" Key="DistrFilter" AutoCallBack-Command="Refresh"
		AutoCallBack-Target="DistributePrm" AutoCallBack-Enabled="true" LoadOnDemand="True"
		AcceptButtonID="cbOk" CancelButtonID="cbCancel" Caption="Distribute Year Amount by Periods"
		CaptionVisible="True" DesignView="Content" CommandName="distributeOK" CommandSourceID="ds" Width ="390px">
		<px:PXFormView ID="DistributePrm" runat="server" DataSourceID="ds" Style="z-index: 108"
			Width="100%" DataMember="DistrFilter" Caption="Dispose Parameters" SkinID="Transparent">
			<Activity Height="" HighlightColor="" SelectedColor="" Width="" />
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule2" runat="server" />
				<px:PXDropDown ID="edMethod" runat="server" DataField="Method" Width="220" />
                <px:PXLayoutRule ID="PXLayoutRule6" runat="server" />
                <px:PXCheckBox CommitChanges="True" ID="ApplyToAll" runat="server" DataField="ApplyToAll" />
                <px:PXCheckBox CommitChanges="True" ID="ApplyToSubGroups" runat="server" DataField="ApplyToSubGroups" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="cbOk" runat="server" Text="OK" DialogResult="OK" />
			<px:PXButton ID="cbCancel" runat="server" Text="Cancel" DialogResult="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>

    <px:PXSmartPanel ID="spManageDlg" runat="server" Key="ManageDialog" AutoCallBack-Command="Refresh"
		AutoCallBack-Target="ManagePrm" AutoCallBack-Enabled="true" LoadOnDemand="True"
		AcceptButtonID="cbOk" CancelButtonID="cbCancel" Caption="Manage Budget"
		CaptionVisible="True" DesignView="Content" CommandName="manageOK" CommandSourceID="ds">
		<px:PXFormView ID="ManagePrm" runat="server" DataSourceID="ds" Style="z-index: 108"
			Width="100%" DataMember="ManageDialog" Caption="Manage Budget" SkinID="Transparent">
			<Activity Height="" HighlightColor="" SelectedColor="" Width="" />
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule2" runat="server" />
                <px:PXDropDown ID="edMethod" runat="server" DataField="Method" Width="300" CommitChanges="true"/>
                <px:PXTextEdit ID="edMessage" runat="server" DataField="Message" TextMode="MultiLine" Height="35px" SuppressLabel="true" SkinID="none" Style="background-color:#E5E9EE; resize:none"  >
                    <Padding Bottom="0px" />
                    <Border> 
                        <Bottom Width="0px" />
                        <Top Width="0px" />
                        <Left Width="0px" />
                        <Right Width="0px" />
                    </Border>
				</px:PXTextEdit>
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton1" runat="server" Text="OK" DialogResult="OK" />
			<px:PXButton ID="PXButton2" runat="server" Text="Cancel" DialogResult="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>

	<px:PXSmartPanel ID="spPreloadArticlesDlg" runat="server" CommandName="preload" CommandSourceID="ds"
		Width="435" Caption="Preload Budget Articles Wizard" Key="BudgetArticles" CaptionVisible="True"
		Style="display: none;" AutoCallBack-Enabled="true" AutoCallBack-Target="PXWizard1" AutoCallBack-Command="WizCancel">
        <px:PXWizard ID="PXWizard1" runat="server" Width="400" Height="240" DataMember="PreloadFilter"
			SkinID="Flat">
			<NextCommand Target="ds" Command="wNext" />
			<Pages>
				<px:PXWizardPage>
					<Template>
						<px:PXFormView ID="formPanel" runat="server" SkinID="Transparent" DataMember="PreloadFilter">
							<Template>
								<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="SM" ControlSize="XM"
									StartColumn="True" ColumnSpan="2" />
								<px:PXLayoutRule ID="PXLayoutRule7" runat="server" ControlSize="S" Merge="true" />
								<px:PXLabel runat="server" ID="lblStep1" Width="370px" Style="font-weight: bold;
									text-align: right">Step 1 of 3</px:PXLabel>
								<px:PXLayoutRule ID="PXLayoutRule5" runat="server" ColumnSpan="1" />
								<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" ControlSize="M"
									LabelsWidth="S" />
								<px:PXLabel runat="server" ID="lblDetails1" Width="370px" Height="35px" Style="font-style: italic;
									overflow: visible; padding-left: 10px">This wizard allows you to preload the budget articles from the actual ledger or from another budget.</px:PXLabel>
								<px:PXLabel runat="server" ID="PXLabel1" Width="370px" Height="20px" Style="font-style: italic;
									overflow: visible; padding-left: 10px">Select the source parameters:</px:PXLabel>
								<px:PXPanel ID="pnl2" runat="server" Caption="Test" RenderStyle="Simple" Style="padding-left: 10px">
									<px:PXLayoutRule ID="PXLayoutRule4" runat="server" LabelsWidth="SM" ControlSize="M"
										StartColumn="True" ColumnSpan="2" />
									<px:PXSelector ID="prBranchID" runat="server" CommitChanges="True" DataField="branchID" />
									<px:PXSelector ID="prLedgerID" runat="server" CommitChanges="True" DataField="ledgerID" AutoRefresh="True"/>
									<px:PXLayoutRule ID="PXLayoutRule3" runat="server" ControlSize="S" />
									<px:PXSelector ID="prFinYear" runat="server" DataField="finYear" CommitChanges="true"/>
									<px:PXNumberEdit ID="PXNumberEdit1" runat="server" DataField="changePercent" CommitChanges="True">
									</px:PXNumberEdit>
								</px:PXPanel>
							</Template>
						</px:PXFormView>
					</Template>
				</px:PXWizardPage>
				<px:PXWizardPage>
					<Template>
						<px:PXFormView ID="formPanel" runat="server" SkinID="Transparent" DataMember="PreloadFilter">
							<Template>
								<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="SM" ControlSize="XM"
									StartColumn="True" ColumnSpan="2" />
								<px:PXLayoutRule ID="PXLayoutRule7" runat="server" ControlSize="S" Merge="true" />
								<px:PXLabel runat="server" ID="lblStep2" Width="370px" Style="font-weight: bold;
									text-align: right">Step 2 of 3</px:PXLabel>
								<px:PXLayoutRule ID="PXLayoutRule5" runat="server" ColumnSpan="1" />
								<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" ControlSize="M"
									LabelsWidth="S" />
								<px:PXLabel runat="server" ID="lblDetails2" Width="370px" Height="25px" Style="font-style: italic;
									overflow: visible; padding-left: 10px">Select the range of accounts and the account/subaccount masks:</px:PXLabel>
								<px:PXLabel runat="server" ID="PXLabel2" Width="370px" Height="30px" Style="font-style: italic;
									overflow: visible; padding-left: 10px">All existing accounts/subaccounts will be preloaded if no account/subaccount mask is specified.</px:PXLabel>
								<px:PXPanel ID="pnl2" runat="server" Caption="Accounts" RenderStyle="Simple" Style="padding-left: 10px">
									<px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartColumn="True" LabelsWidth="SM"
										ControlSize="M" />
									<px:PXSelector ID="prFromAccount" runat="server" DataField="fromAccount" LabelsWidth="SM"
										ControlSize="M" CommitChanges="true" />
									<px:PXSelector ID="prToAccount" runat="server" DataField="toAccount" LabelsWidth="SM"
										ControlSize="M" CommitChanges="true" />
                                    <px:PXSegmentMask CommitChanges="True" ID="prAccountCD" runat="server" DataField="AccountIDFilter"
										DataSourceID="ds" LabelsWidth="SM" ControlSize="SM" Wildcard="?"/>
									<px:PXSegmentMask CommitChanges="True" ID="prSubCD" runat="server" DataField="SubIDFilter"
										DataSourceID="ds" LabelsWidth="SM" ControlSize="SM" Wildcard="?"/>
								</px:PXPanel>
							</Template>
						</px:PXFormView>
					</Template>
				</px:PXWizardPage>
				<px:PXWizardPage>
					<Template>
						<px:PXFormView ID="formPanel" runat="server" SkinID="Transparent" DataMember="PreloadFilter">
							<Template>
								<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="SM" ControlSize="XM"
									StartColumn="True" ColumnSpan="2" />
								<px:PXLayoutRule ID="PXLayoutRule7" runat="server" ControlSize="S" Merge="true" />
								<px:PXLabel runat="server" ID="lblStep3" Width="370px" Style="font-weight: bold;
									text-align: right">Step 3 of 3</px:PXLabel>
								<px:PXLayoutRule ID="PXLayoutRule5" runat="server" ColumnSpan="1" />
								<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" ControlSize="M"
									LabelsWidth="S" />
								<px:PXLabel runat="server" ID="lblDetails3" Width="370px" Height="35px" Style="font-style: italic;
									overflow: visible; padding-left: 10px">Specify the preload action if the articles are already entered for the budget:</px:PXLabel>
								<px:PXPanel ID="pnl2" runat="server" Caption="Test" RenderStyle="Simple" Style="padding-left: 10px">
								
								<px:PXGroupBox ID="gbPreloadMode" runat="server" CommitChanges="True" DataField="PreloadAction" RenderStyle="Simple">
									<Template>
										<px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartColumn="True" LabelsWidth="XM" ColumnWidth="L" />
										<px:PXRadioButton ID="prUpdate" runat="server" GroupName="PreloadMode" Text="Update Existing Articles Only"
											Value="1" />
										<px:PXRadioButton ID="prUpdateAdd" runat="server" GroupName="PreloadMode" Text="Update Existing Articles and Load Nonexistent Articles"
											Value="2" />
										<px:PXRadioButton ID="prAdd" runat="server" GroupName="PreloadMode" Text="Load Nonexistent Articles Only"
											Value="3" />
									</Template>
								</px:PXGroupBox>
								</px:PXPanel>
							</Template>
						</px:PXFormView>
					</Template>
				</px:PXWizardPage>
			</Pages>
		</px:PXWizard>
    </px:PXSmartPanel>
</asp:Content>
