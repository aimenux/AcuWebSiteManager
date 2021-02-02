<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="DR201500.aspx.cs"
	Inherits="Page_DR201500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<script type="text/javascript">
		var pageName = px.getPageName();
		window.localStorage.setItem(pageName + "_iwidth", 1000);
		window.localStorage.setItem(pageName + "_iheight", 900);
		window.localStorage.setItem(pageName + "_owidth", 1000);
		window.localStorage.setItem(pageName + "_oheight", 900);
		</script>
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.DR.DraftScheduleMaint" PrimaryView="Schedule">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="True" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="ViewBatch" PostData="Self" DependOnGrid="grid" Visible="false" />
			<px:PXDSCallbackCommand Name="GenerateTransactions" DependOnGrid="componentGrid" PostData="Self" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewSchedule" PostData="Self" DependOnGrid="gridSchedules" Visible="false" />
			<px:PXDSCallbackCommand Name="Release" PostData="Self" RepaintControls="All" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Deferral Schedule" DataMember="Schedule"
		NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" TabIndex="100" MarkRequired="Dynamic">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXSelector ID="edScheduleNumber" runat="server" DataField="ScheduleNbr" DataSourceID="ds" />
			<px:PXDropDown ID="edScheduleStatus" runat="server" DataField="Status" Enabled="False" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edDocDate" runat="server" DataField="DocDate" ValidateRequestMode="Inherit" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" DataSourceID="ds" />
			<px:PXCheckBox ID="chkIsCustom" Visible="False" runat="server" DataField="IsCustom" ValidateRequestMode="Inherit" />
			<px:PXCheckBox ID="chkIsPoolVisible" Visible="False" runat="server" DataField="IsPoolVisible" ValidateRequestMode="Inherit" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXDropDown CommitChanges="True" ID="edDocumentType" runat="server" DataField="DocumentType" />
			<px:PXSelector CommitChanges="True" ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds" />
			<px:PXSelector CommitChanges="True" runat="server" DataField="LineNbr" ID="edLineNbr" AutoRefresh="True" DataSourceID="ds"/>
			<px:PXNumberEdit CommitChanges="True" runat="server" DataField="OrigLineAmt" ID="edOrigLineAmt" ValidateRequestMode="Inherit" />
			<px:PXNumberEdit CommitChanges="True" runat="server" DataField="CuryNetTranPrice" ID="edNetTranPrice" ValidateRequestMode="Inherit" />
			<px:PXSelector CommitChanges="True" ID="selCurrency" runat="server" DataField="CuryID" DataSourceID="ds" RateTypeView="CurrencyInfo" DataMember="CurrencyInfo"/>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSelector CommitChanges="True" ID="edBAccountID" runat="server" DataField="BAccountID" DataSourceID="ds" AutoRefresh="True" />
			<px:PXSegmentMask ID="edBAccountLocID" runat="server" DataField="BAccountLocID" DataSourceID="ds" />
			<px:PXSegmentMask ID="PXSegmentMask1" runat="server" DataField="ProjectID" CommitChanges="True" DataSourceID="ds" />
			<px:PXSegmentMask ID="PXSegmentMask2" runat="server" DataField="TaskID" DataSourceID="ds" />
			<px:PXCheckBox DataField="IsOverridden" ID="chkIsOverridden" runat="server" CommitChanges="True"/>
			<px:PXLayoutRule runat="server" ControlSize="S" LabelsWidth="S" StartColumn="True">
			</px:PXLayoutRule>
			<px:PXDateTimeEdit ID="edTermStartDate" runat="server" DataField="TermStartDate" CommitChanges="true">
			</px:PXDateTimeEdit>
			<px:PXDateTimeEdit ID="edTermEndDate" runat="server" DataField="TermEndDate" CommitChanges="true">
			</px:PXDateTimeEdit>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXNumberEdit CommitChanges="True" runat="server" DataField="ComponentsTotal" ID="edComponentsTotal" ValidateRequestMode="Inherit" />
			<px:PXNumberEdit CommitChanges="True" runat="server" DataField="DefTotal" ID="DefTotal" ValidateRequestMode="Inherit" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="DocumentProperties">
		<Items>
			<px:PXTabItem Text="Details">
				<Template>
					<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="200px" 
											Panel1MinSize="180" Panel2MinSize="180">
						<AutoSize Enabled="true" MinHeight="360" />
						<Template1>
							<px:PXGrid ID="componentGrid" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" Caption="Components" Height="100px" SyncPosition="True" KeepPosition="True" TabIndex="200">
								<AutoCallBack Target="grid" Command="Refresh"/>
								<AutoSize Enabled="true" />
								<Levels>
									<px:PXGridLevel DataMember="Components" >
										<RowTemplate>
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
											<px:PXSelector ID="edComponentID" runat="server" DataField="ComponentID" />
											<px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" />
											<px:PXDropDown ID="edStatus2" runat="server" DataField="Status" Enabled="False" />
											<px:PXSegmentMask ID="edAccountID2" runat="server" DataField="AccountID" />
											<px:PXNumberEdit ID="edTotalAmt" runat="server" DataField="TotalAmt" Enabled="False" />
											<px:PXSegmentMask ID="edSubID2" runat="server" DataField="SubID" />
											<px:PXNumberEdit ID="edDefAmt" runat="server" DataField="DefAmt" Enabled="False" />
											<px:PXSelector ID="edDefCode" runat="server" DataField="DefCode" Enabled="False" AutoRefresh="true"/>
											<px:PXSegmentMask ID="edDefAcctID" runat="server" DataField="DefAcctID" />
											<px:PXSegmentMask ID="edDefSubID" runat="server" DataField="DefSubID" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="ComponentID" AutoCallBack="True"/>
											<px:PXGridColumn DataField="DefCode" AutoCallBack="True" />
											<px:PXGridColumn DataField="DefAcctID" AutoCallBack="True"/>
											<px:PXGridColumn DataField="DefSubID" />
											<px:PXGridColumn DataField="AccountID" AutoCallBack="True"/>
											<px:PXGridColumn DataField="SubID" />
											<px:PXGridColumn DataField="TermStartDate" AutoCallBack="True"/>
											<px:PXGridColumn DataField="TermEndDate" AutoCallBack="True"/>
											<px:PXGridColumn DataField="ProjectID" AutoCallBack="True"/>
											<px:PXGridColumn DataField="TaskID" AutoCallBack="True"/>
											<px:PXGridColumn DataField="TotalAmt" TextAlign="Right" AutoCallBack="True"/>
											<px:PXGridColumn DataField="DefAmt" TextAlign="Right" />
											<px:PXGridColumn DataField="DefTotal" TextAlign="Right" />
											<px:PXGridColumn DataField="FinPeriodID" AutoCallBack="True"/>
											<px:PXGridColumn DataField="BranchID" AutoCallBack="True"/>
											<px:PXGridColumn DataField="Status" RenderEditorText="True" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<ActionBar DefaultAction="cmdViewBatch">
									<CustomItems>
										<px:PXToolBarButton Text="Generate Transactions" Key="cmdGenTran">
											<AutoCallBack Target="ds" Command="GenerateTransactions" />
										</px:PXToolBarButton>
									</CustomItems>
								</ActionBar>
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" Caption="Transactions" SyncPosition="True" TabIndex="600">
								<Levels>
									<px:PXGridLevel DataMember="Transactions">
										<Columns>
											<px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
											<px:PXGridColumn DataField="Status" RenderEditorText="True" />
											<px:PXGridColumn DataField="RecDate" AutoCallBack="True"/>
											<px:PXGridColumn DataField="TranDate" AutoCallBack="True"/>
											<px:PXGridColumn DataField="Amount" TextAlign="Right" AutoCallBack="True"/>
											<px:PXGridColumn DataField="AccountID" AutoCallBack="True" />
											<px:PXGridColumn DataField="SubID" />
											<px:PXGridColumn DataField="FinPeriodID" AutoCallBack="True"/>
											<px:PXGridColumn DataField="BranchID" />
											<px:PXGridColumn DataField="BatchNbr" LinkCommand="ViewBatch" />
											<px:PXGridColumn DataField="IsSamePeriod" TextAlign="Center" AllowShowHide="Server" Type="CheckBox" />
										</Columns>
										<RowTemplate>
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
											<px:PXNumberEdit ID="edScheduleID" runat="server" DataField="ScheduleID" ValidateRequestMode="Inherit" />
											<px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr" Enabled="False" ValidateRequestMode="Inherit" />
											<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
											<px:PXDateTimeEdit ID="edRecDate" runat="server" DataField="RecDate" ValidateRequestMode="Inherit" />
											<px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate" Enabled="False" ValidateRequestMode="Inherit" />
											<px:PXNumberEdit ID="edAmount" runat="server" DataField="Amount" ValidateRequestMode="Inherit" />
											<px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" AllowEdit="True" />
											<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" />
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
											<px:PXMaskEdit ID="edFinPeriodID" runat="server" DataField="FinPeriodID" Enabled="False" InputMask="##-####" ValidateRequestMode="Inherit" />
											<px:PXTextEdit ID="edBatchNbr" runat="server" DataField="BatchNbr" ValidateRequestMode="Inherit" />
										</RowTemplate>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" />
								<Mode AllowUpload="True" />
							</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Reallocation pool"  BindingContext="form" VisibleExp="DataControls[&quot;chkIsPoolVisible&quot;].Value == True">
				<Template>
					<px:PXGrid ID="gridReallocationPool" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" Caption="Components" Height="100px" SyncPosition="True" KeepPosition="True" TabIndex="200">
						<AutoCallBack Target="grid" Command="Refresh"/>
						<AutoSize Enabled="true" />
						<Levels>
							<px:PXGridLevel DataMember="ReallocationPool" >
								<Columns>
									<px:PXGridColumn DataField="RefNbr"/>
									<px:PXGridColumn DataField="ARTran__LineNbr" TextAlign="Right" />
									<px:PXGridColumn DataField="ARTran__BranchID"/>
									<px:PXGridColumn DataField="ARTran__InventoryID"/>
									<px:PXGridColumn DataField="ARTran__Qty" TextAlign="Right"/>
									<px:PXGridColumn DataField="ARTran__UOM"/>
									<px:PXGridColumn DataField="ARTran__CuryExtPrice" TextAlign="Right"/>
									<px:PXGridColumn DataField="ARTran__CuryTranAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="INComponent__ComponentID"/>
									<px:PXGridColumn DataField="INComponent__Qty" TextAlign="Right"/>
									<px:PXGridColumn DataField="INComponent__UOM"/>
									<px:PXGridColumn DataField="DRScheduleDetail__DefCode" />
									<px:PXGridColumn DataField="DRScheduleDetail__FairValuePrice" />
									<px:PXGridColumn DataField="DRScheduleDetail__EffectiveFairValuePrice" />
									<px:PXGridColumn DataField="DRScheduleDetail__FairValueCuryID" />
									<px:PXGridColumn DataField="DRScheduleDetail__Percentage" />
									<px:PXGridColumn DataField="DRScheduleDetail__TotalAmt" TextAlign="Right" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<Mode AllowAddNew="False" AllowDelete="False" />
						<ActionBar DefaultAction="gridReallocationPool">
							<Actions>
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem LoadOnDemand="true" Text="Original Schedules" BindingContext="form" VisibleExp="DataControls[&quot;chkIsCustom&quot;].Value == False">
				<Template>
					<px:PXGrid ID="gridSchedules" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="100%" SkinID="Details" 
						BorderStyle="None">
						<Levels>
							<px:PXGridLevel DataMember="Associated" DataKeyNames="ScheduleNbr">
								<Columns>
									<px:PXGridColumn DataField="ScheduleNbr" LinkCommand ="ViewSchedule"/>
									<px:PXGridColumn Width="200px" DataField="TranDesc" />
									<px:PXGridColumn DataField="DocumentTypeEx" />
									<px:PXGridColumn DataField="RefNbr" />
									<px:PXGridColumn DataField="FinPeriodID" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<Parameters>
							<px:PXControlParam ControlID="form" Name="ScheduleNbr" PropertyName="NewDataKey[&quot;ScheduleNbr&quot;]" Type="String" />
						</Parameters>
						<AutoSize Enabled="True" MinHeight="260" />
						<Mode AllowAddNew="False" AllowDelete="False" />
						<ActionBar DefaultAction="gridSchedules">
							<Actions>
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="View Schedule" Key="gridSchedules">
									<AutoCallBack Command="ViewSchedule" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Enabled="True" Container="Window" />
	</px:PXTab>
</asp:Content>
