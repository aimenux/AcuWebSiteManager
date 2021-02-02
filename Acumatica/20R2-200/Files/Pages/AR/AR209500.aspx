<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR209500.aspx.cs"
	Inherits="Page_AR209500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Sequence" TypeName="PX.Objects.AR.ARDiscountSequenceMaint" PageLoadBehavior="GoFirstRecord"
		BorderStyle="NotSet">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand StartNewGroup="true" Name="UpdateDiscounts" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Discount Sequence Summary"
		DataMember="Sequence" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity"
		DefaultControlID="edDiscountID" AllowCollapse="false">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXSelector ID="edDiscountID" runat="server" DataField="DiscountID" />
			<px:PXSelector ID="edDiscountSequenceID" runat="server" DataField="DiscountSequenceID" AutoRefresh="True" />
			<px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" CommitChanges="true" />
			<px:PXCheckBox CommitChanges="True" ID="chkIsPromotion" runat="server" DataField="IsPromotion" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
			<px:PXDropDown CommitChanges="True" ID="edDiscountedFor" runat="server" AllowNull="False" DataField="DiscountedFor" />
			<px:PXDropDown CommitChanges="True" ID="edBreakBy" runat="server" AllowNull="False" DataField="BreakBy" SelectedIndex="1" />
			<px:PXLabel runat="server" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" />
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
			<px:PXCheckBox ID="chkProrate" runat="server" DataField="Prorate" AlignLeft="true" />
			<px:PXLabel runat="server" />
			<px:PXLabel runat="server" />
			<px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" />
			<px:PXCheckBox SuppressLabel="True" ID="chkShowFreeItem" runat="server" DataField="ShowFreeItem" />
			<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" SuppressLabel="True" />
			<px:PXFormView ID="DiscountForm" runat="server" DataMember="Discount" DataSourceID="ds" Caption="Hidden Form needed for VisibleExp of TabItems. Tabs are Hidden based on the values of Combo"
				Visible="False">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXCheckBox SuppressLabel="True" ID="chkShowListOfItems" runat="server" DataField="showListOfItems" />
					<px:PXCheckBox SuppressLabel="True" ID="chkShowCustomers" runat="server" DataField="ShowCustomers" />
					<px:PXCheckBox SuppressLabel="True" ID="chkShowCustomerPriceClass" runat="server" DataField="ShowCustomerPriceClass" />
					<px:PXCheckBox SuppressLabel="True" ID="chkShowInventoryPriceClass" runat="server" DataField="ShowInventoryPriceClass" />
					<px:PXCheckBox SuppressLabel="True" ID="chkShowBranches" runat="server" DataField="ShowBranches" />
					<px:PXCheckBox SuppressLabel="True" ID="chkShowSites" runat="server" DataField="ShowSites" />
				</Template>
			</px:PXFormView>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXSmartPanel ID="PanelUpdate" runat="server" Key="UpdateSettings" Caption="Update Discounts" CaptionVisible="True" LoadOnDemand="true"
		DesignView="Content" HideAfterAction="false" AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel">
		<px:PXFormView ID="formUpdateSettings" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="UpdateSettings"
			CaptionVisible="False">
			<ContentStyle BackColor="Transparent" BorderStyle="None" />
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
				<px:PXDateTimeEdit CommitChanges="True" ID="edFilterDate" runat="server" DataField="FilterDate" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButtonOK" runat="server" DialogResult="OK" Text="OK" />
			<px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXTab ID="tab" runat="server" Height="540px" Style="z-index: 100" Width="100%">
		<Items>
			<px:PXTabItem Text="Discount Breakpoints">
				<Template>
					<px:PXGrid runat="server" ID="grid" DataSourceID="ds" Height="144px" BorderStyle="None" SkinID="Details" Width="100%" AllowPaging="false">
						<Levels>
							<px:PXGridLevel DataMember="Details">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXCheckBox ID="chkIsActive" runat="server" DataField="IsActive"/>
									<px:PXNumberEdit ID="edQuantity" runat="server" DataField="Quantity" />
									<px:PXNumberEdit ID="edDiscountPercent" runat="server" DataField="DiscountPercent" />
									<px:PXNumberEdit ID="edLastDiscountPercent" runat="server" DataField="LastDiscountPercent" />
									<px:PXNumberEdit ID="edAmount" runat="server" DataField="Amount" />
									<px:PXNumberEdit ID="edPendingDiscountPercent" runat="server" DataField="PendingDiscountPercent" />
									<px:PXNumberEdit ID="edDiscount" runat="server" DataField="Discount" />
									<px:PXDateTimeEdit ID="edLastDate" runat="server" DataField="LastDate" />
									<px:PXNumberEdit ID="edFreeItemQty" runat="server" DataField="FreeItemQty" />
									<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" AllowNull="false" />
									<px:PXLayoutRule ID="PXLayoutRule21" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXNumberEdit ID="edPendingQuantity" runat="server" DataField="PendingQuantity" />
									<px:PXNumberEdit ID="edPendingAmount" runat="server" DataField="PendingAmount" />
									<px:PXNumberEdit ID="edPendingDiscount" runat="server" DataField="PendingDiscount" />
									<px:PXNumberEdit ID="edPendingFreeItemQty" runat="server" DataField="PendingFreeItemQty" />
									<px:PXLayoutRule ID="PXLayoutRule22" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXNumberEdit ID="edLastQuantity" runat="server" DataField="LastQuantity" />
									<px:PXNumberEdit ID="edLastAmount" runat="server" DataField="LastAmount" />
									<px:PXNumberEdit ID="edLastDiscount" runat="server" DataField="LastDiscount" />
									<px:PXNumberEdit ID="edLastFreeItemQty" runat="server" DataField="LastFreeItemQty" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" CommitChanges="true" />
									<px:PXGridColumn DataField="Quantity" TextAlign="Right" />
									<px:PXGridColumn DataField="Amount" TextAlign="Right" />
									<px:PXGridColumn DataField="Discount" TextAlign="Right" />
									<px:PXGridColumn DataField="DiscountPercent" TextAlign="Right" />
									<px:PXGridColumn DataField="FreeItemQty" TextAlign="Right" />
									<px:PXGridColumn DataField="PendingQuantity" TextAlign="Right" />
									<px:PXGridColumn DataField="PendingAmount" TextAlign="Right" />
									<px:PXGridColumn DataField="PendingDiscount" TextAlign="Right" />
									<px:PXGridColumn DataField="PendingDiscountPercent" TextAlign="Right" />
									<px:PXGridColumn DataField="PendingFreeItemQty" TextAlign="Right" />
									<px:PXGridColumn DataField="StartDate" />
									<px:PXGridColumn DataField="LastQuantity" TextAlign="Right" />
									<px:PXGridColumn DataField="LastAmount" TextAlign="Right" />
									<px:PXGridColumn DataField="LastDiscount" TextAlign="Right" />
									<px:PXGridColumn DataField="LastDiscountPercent" TextAlign="Right" />
									<px:PXGridColumn DataField="LastFreeItemQty" TextAlign="Right" />
									<px:PXGridColumn DataField="LastDate" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode InitNewRow="True" />
						<LevelStyles>
							<RowForm Height="160px" Width="900px" />
						</LevelStyles>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Items" BindingContext="DiscountForm" VisibleExp="DataControls[&quot;chkShowListOfItems&quot;].Value == true">
				<Template>
					<px:PXGrid ID="itemsGrid" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" AllowPaging="false">
						<AutoSize Enabled="True" />
						<Mode AllowUpload="True" />
						<Levels>
							<px:PXGridLevel DataMember="Items">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True">
										<GridProperties>
											<PagerSettings Mode="NextPrevFirstLast" />
										</GridProperties>
									</px:PXSegmentMask>
									<px:PXTextEdit ID="edInventoryItem__Descr" runat="server" DataField="InventoryItem__Descr" Enabled="False" />
									<px:PXSelector ID="edUOM2" runat="server" DataField="UOM" />
									<px:PXNumberEdit ID="edAmount2" runat="server" DataField="Amount" />
									<px:PXNumberEdit ID="edQuantity2" runat="server" DataField="Quantity" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" AutoCallBack="True" />
									<px:PXGridColumn AllowUpdate="False" DataField="InventoryItem__Descr" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Customers" BindingContext="DiscountForm" VisibleExp="DataControls[&quot;chkShowCustomers&quot;].Value == true">
				<Template>
					<px:PXGrid ID="customersGrid" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" AllowPaging="false">
						<AutoSize Enabled="True" />
						<Mode AllowUpload="True" />
						<Levels>
							<px:PXGridLevel DataMember="Customers">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" />
									<px:PXTextEdit ID="edCustomer__AcctName" runat="server" DataField="Customer__AcctName" Enabled="False" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="CustomerID" AutoCallBack="true" DisplayFormat="CCCCCCCCCC" />
									<px:PXGridColumn AllowUpdate="False" DataField="Customer__AcctName" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Customer Price Classes" BindingContext="DiscountForm" VisibleExp="DataControls[&quot;chkShowCustomerPriceClass&quot;].Value == true">
				<Template>
					<px:PXGrid ID="customerPriceClassGrid" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" AllowPaging="false">
						<Levels>
							<px:PXGridLevel DataMember="CustomerPriceClasses">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSelector ID="edCustomerPriceClassID" runat="server" DataField="CustomerPriceClassID" />
									<px:PXTextEdit ID="edARPriceClass__Description" runat="server" DataField="ARPriceClass__Description" Enabled="False" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="CustomerPriceClassID" DisplayFormat="&gt;aaaaaaaaaa" AutoCallBack="true" />
									<px:PXGridColumn AllowUpdate="False" DataField="ARPriceClass__Description" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Item Price Classes" BindingContext="DiscountForm" VisibleExp="DataControls[&quot;chkShowInventoryPriceClass&quot;].Value == true">
				<Template>
					<px:PXGrid ID="inventoryPriceClassGrid" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" AllowPaging="false">
						<Levels>
							<px:PXGridLevel DataMember="InventoryPriceClasses">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSelector ID="edInventoryPriceClassID" runat="server" DataField="InventoryPriceClassID" />
									<px:PXTextEdit ID="edINPriceClass__Description" runat="server" DataField="INPriceClass__Description" Enabled="False" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="InventoryPriceClassID" DisplayFormat="&gt;aaaaaaaaaa" AutoCallBack="true" />
									<px:PXGridColumn AllowUpdate="False" DataField="INPriceClass__Description" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Branches" BindingContext="DiscountForm" VisibleExp="DataControls[&quot;chkShowBranches&quot;].Value == true">
				<Template>
					<px:PXGrid ID="branchesGrid" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" AllowPaging="false">
						<Levels>
							<px:PXGridLevel DataMember="Branches">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" />
									<px:PXTextEdit ID="edBranch__Description" runat="server" DataField="Branch__AcctName" Enabled="False" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="BranchID" DisplayFormat="&gt;aaaaaaaaaa" AutoCallBack="true" />
									<px:PXGridColumn AllowUpdate="False" DataField="Branch__AcctName" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Warehouses" BindingContext="DiscountForm" VisibleExp="DataControls[&quot;chkShowSites&quot;].Value == true">
				<Template>
					<px:PXGrid ID="sitesGrid" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" AllowPaging="false">
						<Levels>
							<px:PXGridLevel DataMember="Sites">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" />
									<px:PXTextEdit ID="edINSite__Description" runat="server" DataField="INSite__Descr" Enabled="False" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="SiteID" DisplayFormat="&gt;aaaaaaaaaa" AutoCallBack="true" />
									<px:PXGridColumn AllowUpdate="False" DataField="INSite__Descr" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Free Item" BindingContext="form" VisibleExp="DataControls[&quot;chkShowFreeItem&quot;].Value == true">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
					<px:PXFormView ID="form2" runat="server" DataSourceID="ds" Width="100%"
						DataMember="CurrentSequence" ActivityField="NoteActivity"
						DefaultControlID="edDiscountID" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
							<px:PXSegmentMask ID="edFreeItemID" runat="server" DataField="FreeItemID" AllowEdit="true" CommitChanges="true" />
							<px:PXSegmentMask ID="edPendingFreeItemID" runat="server" DataField="PendingFreeItemID" AllowEdit="true">
								<GridProperties>
									<PagerSettings Mode="NextPrevFirstLast" />
								</GridProperties>
							</px:PXSegmentMask>
							<px:PXSegmentMask ID="edLastFreeItemID" runat="server" DataField="LastFreeItemID" AllowEdit="True" />
							<px:PXDateTimeEdit ID="edUpdateDate" runat="server" DataField="UpdateDate" Enabled="False" />
							<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" SuppressLabel="True" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Enabled="True" Container="Window" />
	</px:PXTab>
</asp:Content>
