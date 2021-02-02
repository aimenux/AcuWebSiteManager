<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	CodeFile="PM207500.aspx.cs" Inherits="Pages_PM_PM207500" Title="Allocate Rules"
	ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.AllocationMaint" PrimaryView="Allocations">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="Allocations" Caption="Summary" TemplateContainer="">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" />
			<px:PXSelector ID="edAllocationID" runat="server" DataField="AllocationID" DataSourceID="ds" Size="M" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Description" Size="XL" />
		</Template>
	</px:PXFormView>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
	<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="200" Panel1MinSize="150" Panel2MinSize="700" Panel2Overflow="true">
        <AutoSize Enabled="true" MinHeight="250" Container="Window" />
		<Template1>
			<px:PXGrid ID="grid" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" AllowFilter="False" NoteIndicator="False" FilesIndicator="False" SyncPosition="True">
				<Levels>
					<px:PXGridLevel DataMember="Steps">
						<Columns>
							<px:PXGridColumn DataField="StepID" Label="Step ID" SortDirection="Ascending" TextAlign="Right" CommitChanges="True" />
							<px:PXGridColumn DataField="Description" Label="Description" CommitChanges="True"/>
						</Columns>
					</px:PXGridLevel>
				</Levels>
				<AutoSize Enabled="True" Container="Parent"></AutoSize>
				<ActionBar PagerVisible="False">
				</ActionBar>
				<Mode AllowColMoving="False"   />
				<AutoCallBack Enabled="true" ActiveBehavior="True" Command="Refresh" Target="formRules">
                    <Behavior CommitChanges="True" RepaintControlsIDs="formRules,formSettings"/>
                </AutoCallBack>
			</px:PXGrid>
		</Template1>

		<Template2>
			<px:PXTab ID="tab" runat="server" DataSourceID="ds" DataMember="Step" Width="100%">
				<AutoSize Enabled="True" Container="Parent"></AutoSize>
				<Items>
					<px:PXTabItem Text="Calculation Rules" RepaintOnDemand="false">
						<Template>
							<px:PXFormView ID="formRules" runat="server" DataSourceID="ds" DataMember="StepRules" Width="100%" SkinID="Transparent">
								<Template>
									<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ColumnWidth="L" />
									<%--<px:PXSelector ID="edStepID" runat="server" DataField="StepID" DataSourceID="ds" />--%>
									<px:PXDropDown ID="edMethod" runat="server" DataField="Method" CommitChanges="True"/>
									<px:PXCheckBox ID="chkPost" runat="server" DataField="Post" CommitChanges="True"/>

									<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartRow="True" StartGroup="True" GroupCaption="Selection Criteria" />
									<px:PXPanel ID="formSelection" runat="server" Width="100%" RenderStyle="Simple">
										<px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="True" LabelsWidth="SM" ColumnWidth="L" />
										<px:PXDropDown ID="edSelectOption" runat="server" DataField="SelectOption" CommitChanges="True" />
										<px:PXSelector ID="edAccountGroupFrom" runat="server" DataField="AccountGroupFrom" />
										<px:PXNumberEdit ID="edRangeStart" runat="server" DataField="RangeStart" CommitChanges="True"/>
										<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" />
										<px:PXLabel ID="PXLabel1" runat="server" />
										<px:PXSelector ID="edAccountGroupTo" runat="server" DataField="AccountGroupTo" />
										<px:PXNumberEdit ID="PXNumberEdit1" runat="server" DataField="RangeEnd" CommitChanges="True"/>
									</px:PXPanel>

									<px:PXLayoutRule ID="PXLayoutRule14" runat="server" StartRow="True" StartGroup="True" GroupCaption="Rate Settings" />
									<px:PXPanel ID="PXFormView1" runat="server" Width="100%" RenderStyle="Simple">
										<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM" ColumnWidth="L" />
										<px:PXSelector ID="edRateTypeID" runat="server" DataField="RateTypeID" />
										<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" />
										<px:PXDropDown ID="edNoRateOption" runat="server" DataField="NoRateOption" />
									</px:PXPanel>

									<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartRow="True" StartGroup="True" GroupCaption="Calculation Settings"/>
                                    <px:PXLayoutRule ID="PXLayoutRule20" runat="server" LabelsWidth="SM"/>
									<pxa:PMFormulaEditor ID="edQtyFormula" runat="server" DataSourceID="ds" DataField="QtyFormula" Parameters="@Rate,@Price" />
									<pxa:PMFormulaEditor ID="edBillableQtyFormula" runat="server" DataSourceID="ds" DataField="BillableQtyFormula" Parameters="@Rate,@Price" />
									<pxa:PMFormulaEditor ID="edAmountFormula" runat="server" DataSourceID="ds" DataField="AmountFormula" Parameters="@Rate,@Price" />
									<pxa:PMFormulaEditor ID="edDescriptionFormula" runat="server" DataSourceID="ds" DataField="DescriptionFormula" Parameters="@Rate,@Price" />

								</Template>
								<CallbackCommands>
									<Save RepaintControls="None" RepaintControlsIDs="formSettings" CommitChanges="False" />
								</CallbackCommands>
							</px:PXFormView>
						</Template>
					</px:PXTabItem>
					<px:PXTabItem Text="Allocation Settings" RepaintOnDemand="false">
						<Template>
							<px:PXFormView ID="formSettings" runat="server" DataSourceID="ds" DataMember="StepSettings" Width="100%" SkinID="Transparent">
								<Template>
									<%--<px:PXSelector ID="edStepID" runat="server" DataField="StepID" DataSourceID="ds" />--%>
									<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartRow="True" StartGroup="True" GroupCaption="Transaction Options" />
									<px:PXPanel ID="formSelection" runat="server" Width="100%" RenderStyle="Simple">
										<px:PXLayoutRule ID="PXLayoutRule15" runat="server" StartColumn="True" LabelsWidth="SM" ColumnWidth="L" />
										<px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="UpdateGL" CommitChanges="True"/>
										<px:PXDropDown ID="edReverse" runat="server" DataField="Reverse" />
										<px:PXDropDown ID="PXDropDown1" runat="server" DataField="DateSource" />
										
										<px:PXLayoutRule ID="PXLayoutRule88" runat="server" StartColumn="True" LabelsWidth="SM" ColumnWidth="M"/>
										<px:PXCheckBox ID="PXCheckBox2" runat="server" DataField="AllocateZeroQty" AlignLeft="True" />
										<px:PXCheckBox ID="PXCheckBox3" runat="server" DataField="AllocateZeroAmount" AlignLeft="True" />
										<px:PXCheckBox ID="PXCheckBox4" runat="server" DataField="AllocateNonBillable" AlignLeft="True" />
                                        
                                        <px:PXLayoutRule ID="PXLayoutRule19" runat="server" StartRow="true" StartColumn="True" ColumnSpan="2" LabelsWidth="SM" />
                                        <px:PXCheckBox ID="PXCheckBox9" runat="server" DataField="MarkAsNotAllocated"/>
									</px:PXPanel>

                                    <px:PXLayoutRule ID="PXLayoutRule21" runat="server" StartRow="True" StartGroup="True" GroupCaption="Transaction Branch" />
									<px:PXPanel ID="PXPanel4" runat="server" Width="100%" RenderStyle="Simple">
                                        <px:PXLayoutRule ID="PXLayoutRule23" runat="server" StartColumn="True" LabelsWidth="SM" ColumnWidth="L"  />
                                        <px:PXDropDown ID="PXDropDown2" runat="server" DataField="BranchOrigin" CommitChanges="True"/>
                                        <px:PXDropDown ID="PXDropDown6" runat="server" DataField="OffsetBranchOrigin" CommitChanges="True"/>
                                        <px:PXLayoutRule ID="PXLayoutRule22" runat="server" StartColumn="True" ColumnWidth="M" />
                                        <px:PXSelector ID="edSourceBranchID" runat="server" DataField="SourceBranchID" SuppressLabel="True" CommitChanges="True"/>
                                        <px:PXSelector ID="PXSelector4" runat="server" DataField="TargetBranchID" SuppressLabel="True" CommitChanges="True"/>
                                    </px:PXPanel>
									
                                    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartRow="True" StartGroup="True" GroupCaption="Debit Transaction" />
									<px:PXPanel ID="PXPanel1" runat="server" Width="100%" RenderStyle="Simple">
										<px:PXLayoutRule ID="PXLayoutRule16" runat="server" StartColumn="True" LabelsWidth="SM" ColumnWidth="L"  />
										<px:PXDropDown ID="PXDropDown3" runat="server" DataField="ProjectOrigin" CommitChanges="True"/>
										<px:PXDropDown ID="PXDropDown4" runat="server" DataField="TaskOrigin" CommitChanges="True"/>
										<px:PXDropDown ID="PXDropDown5" runat="server" DataField="AccountGroupOrigin" CommitChanges="True"/>
										<px:PXDropDown ID="PXDropDown10" runat="server" DataField="AccountOrigin" CommitChanges="True"/>
										<px:PXSegmentMask ID="edSubMask" runat="server" DataField="SubMask" CommitChanges="True"/>
										<px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartColumn="True" ColumnWidth="M" />
										<px:PXSegmentMask ID="PXSelector1" runat="server" DataField="ProjectID" SuppressLabel="True" CommitChanges="True"/>
										<px:PXSegmentMask ID="PXSelector2" runat="server" DataField="TaskID" SuppressLabel="True" CommitChanges="True" AutoRefresh ="true"/>
										<px:PXTextEdit ID="edTaskCD" runat="server" DataField="TaskCD" SuppressLabel="True" CommitChanges="True"/>
										<px:PXSelector ID="PXSelector3" runat="server" DataField="AccountGroupID" SuppressLabel="True" CommitChanges="True"/>
										<px:PXSelector ID="PXSelector8" runat="server" DataField="AccountID" SuppressLabel="True" CommitChanges="True"/>
										<px:PXSegmentMask ID="PXSelector10" runat="server" DataField="SubID" SuppressLabel="True" CommitChanges="True"/>

									</px:PXPanel>

									<px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartRow="True" StartGroup="True" GroupCaption="Credit Transaction" />
									<px:PXPanel ID="PXPanel2" runat="server" Width="100%" RenderStyle="Simple">
										<px:PXLayoutRule ID="PXLayoutRule17" runat="server" StartColumn="True" LabelsWidth="SM" ColumnWidth="L" />
										<px:PXDropDown ID="PXDropDown7" runat="server" DataField="OffsetProjectOrigin" CommitChanges="True"/>
										<px:PXDropDown ID="PXDropDown8" runat="server" DataField="OffsetTaskOrigin" CommitChanges="True"/>
										<px:PXDropDown ID="PXDropDown9" runat="server" DataField="OffsetAccountGroupOrigin" CommitChanges="True"/>
										<px:PXDropDown ID="PXDropDown11" runat="server" DataField="OffsetAccountOrigin" CommitChanges="True"/>
										<px:PXSegmentMask ID="PXSegmentMask1" runat="server" DataField="OffsetSubMask" CommitChanges="True"/>
										<px:PXLayoutRule ID="PXLayoutRule13" runat="server" StartColumn="True" ColumnWidth="M" />
										<px:PXSegmentMask ID="PXSelector5" runat="server" DataField="OffsetProjectID" SuppressLabel="True" CommitChanges="True"/>
										<px:PXSegmentMask ID="PXSelector6" runat="server" DataField="OffsetTaskID" SuppressLabel="True" CommitChanges="True" AutoRefresh ="true"/>
										<px:PXTextEdit ID="edOffsetTaskCD" runat="server" DataField="OffsetTaskCD" SuppressLabel="True" CommitChanges="True"/>
										<px:PXSelector ID="PXSelector7" runat="server" DataField="OffsetAccountGroupID" SuppressLabel="True" CommitChanges="True"/>
										<px:PXSelector ID="PXSelector9" runat="server" DataField="OffsetAccountID" SuppressLabel="True" CommitChanges="True"/>
										<px:PXSegmentMask ID="PXSelector11" runat="server" DataField="OffsetSubID" SuppressLabel="True" CommitChanges="True"/>
									</px:PXPanel>

									<px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartRow="True" StartGroup="True" GroupCaption="Aggregate Transactions" />
									<px:PXPanel ID="PXPanel3" runat="server" Width="100%" RenderStyle="Simple">
										<px:PXLayoutRule ID="PXLayoutRule18" runat="server" StartColumn="True" LabelsWidth="SM" ColumnWidth="L" />
										<px:PXCheckBox ID="PXCheckBox5" runat="server" DataField="GroupByDate" />
										<px:PXCheckBox ID="PXCheckBox6" runat="server" DataField="GroupByEmployee" />
										<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" />
										<px:PXCheckBox ID="PXCheckBox7" runat="server" DataField="GroupByVendor" AlignLeft="True" />
										<px:PXCheckBox ID="PXCheckBox8" runat="server" DataField="GroupByItem" AlignLeft="True" />
									</px:PXPanel>
								</Template>
								<CallbackCommands>
									<Save RepaintControls="None" CommitChanges="False" />
								</CallbackCommands>
							</px:PXFormView>
						</Template>
					</px:PXTabItem>
				</Items>
			</px:PXTab>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
