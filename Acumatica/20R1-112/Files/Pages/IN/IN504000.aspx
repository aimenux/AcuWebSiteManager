<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="IN504000.aspx.cs" Inherits="Page_IN504000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.Objects.IN.PIGenerator"
                     PageLoadBehavior="PopulateSavedValues" Visible="True" PrimaryView="GeneratorSettings"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="GeneratorSettings" DefaultControlID="edPIClassID">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="Preview">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="XM" />
						<px:PXSelector ID="edPIClassID" runat="server" DataField="PIClassID">
							<AutoCallBack Command="Save" Target="tab" />
						</px:PXSelector>
						<px:PXTextEdit ID="edPIDescr" runat="server" DataField="Descr" />
						<px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID">
							<GridProperties FastFilterFields="Descr">
								<Layout ColumnsMenu="False" />
								</GridProperties>
							<Items>
								<px:PXMaskItem EditMask="AlphaNumeric" Length="10" Separator="-" TextCase="Upper" />
							</Items>
							<AutoCallBack Command="Save" Target="tab" />
						</px:PXSegmentMask>
						<px:PXDropDown ID="edMethod" runat="server" AllowNull="False" DataField="Method" Enabled="False" />

						<px:PXDropDown ID="edSelectedMethod" runat="server" AllowNull="False" 
							DataField="SelectedMethod" Enabled="False" />
						<px:PXSelector ID="edCycleID" runat="server" DataField="CycleID">
							<AutoCallBack Command="Save" Target="tab" />
						</px:PXSelector>
 					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
						<px:PXSelector ID="edABCCodeID" runat="server" DataField="ABCCodeID">
							<AutoCallBack Command="Save" Target="tab" />
						</px:PXSelector>
						<px:PXSelector ID="edMovementClassID" runat="server" 
							DataField="MovementClassID">
							<AutoCallBack Command="Save" Target="tab" />
						</px:PXSelector>
						<px:PXNumberEdit ID="edRandomItemsLimit" runat="server" DataField="RandomItemsLimit" ValueType="Int16">
							<AutoCallBack Command="Save" Target="tab" />
						</px:PXNumberEdit>
						<px:PXCheckBox ID="chkByFrequency" runat="server" DataField="ByFrequency">
							<AutoCallBack Command="Save" Target="tab" />
						</px:PXCheckBox>
							<px:PXNumberEdit ID="edBlankLines" runat="server" DataField="BlankLines" ValueType="Int16" Width="51px">
							<AutoCallBack Command="Save" Target="tab" />
							</px:PXNumberEdit>
						<px:PXDateTimeEdit ID="edMaxLastCountDate" runat="server" 
							DataField="MaxLastCountDate" MaxValue="9999-06-06">
							<AutoCallBack Command="Save" Target="tab" />
						</px:PXDateTimeEdit>
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartRow="True" />
						<px:PXGrid ID="grid" runat="server" Style="z-index: 100; width: 100%" 
							AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True"
							BatchUpdate="True" SkinID="PrimaryInquire" TabIndex="200" Caption="Details" 
							DataSourceID="ds">
							<Levels>
								<px:PXGridLevel  DataMember="PreliminaryResultRecs">
									<Columns>
										<px:PXGridColumn DataField="LineNbr" DataType="Int32" TextAlign="Right" />
										<px:PXGridColumn DataField="TagNumber" DataType="Int32" TextAlign="Right" />
										<px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" />
										<px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A" />
										<px:PXGridColumn DataField="LocationID" DisplayFormat="&gt;AAAAAAAAAA" />
										<px:PXGridColumn DataField="LotSerialNbr" />
										<px:PXGridColumn AutoGenerateOption="NotSet" DataField="ExpireDate" 
											DataType="DateTime" Label="Expiry Date" />
										<px:PXGridColumn DataField="BookQty" DataType="Decimal" Decimals="2" TextAlign="Right" />
										<px:PXGridColumn DataField="BaseUnit" />
										<px:PXGridColumn DataField="Descr" />
										<px:PXGridColumn DataField="ItemClassID" />
									</Columns>
								</px:PXGridLevel>
							</Levels>
							<AutoSize Container="Window" Enabled="True" MinHeight="150" />
							<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
							<ActionBar>
								<Actions>
									<Refresh Enabled="False" />
								</Actions>
							</ActionBar>
						</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Location Selection">
				<Template>
					<px:PXPanel DataMember="CurrentGeneratorSettings" runat="server">
						<px:PXLayoutRule runat="server" StartRow="true" LabelsWidth="S" ControlSize="XM" />
						<px:PXSegmentMask ID="edSiteID2" runat="server" DataField="SiteID" Enabled="false" />
					</px:PXPanel>
					<px:PXSplitContainer ID="splitContainerLocations" runat="server" PositionInPercent="true" SplitterPosition="50" Orientation="Vertical">
						<AutoSize Enabled="true" Container="Window" />
						<Template1>
							<px:PXGrid ID="gridLocations"  runat="server" DataSourceID="ds" Width="100%" 
								BorderWidth="0px" SkinID="Details" Caption="Selected Locations" CaptionVisible="true" AllowPaging="True" TabIndex="1200" FilesIndicator="False" NoteIndicator="False">
								<EmptyMsg AnonFilteredAddMessage="No records found.
		Try to change filter to see records here." AnonFilteredMessage="No records found.
		Try to change filter to see records here." ComboAddMessage="No records found.
		Try to change filter or modify parameters above to see records here." FilteredAddMessage="No records found.
		Try to change filter to see records here." FilteredMessage="No records found.
		Try to change filter to see records here." NamedComboAddMessage="No records found as '{0}'.
		Try to change filter or modify parameters above to see records here." NamedComboMessage="No records found as '{0}'.
		Try to change filter or modify parameters above to see records here." NamedFilteredAddMessage="No records found as '{0}'.
		Try to change filter to see records here." NamedFilteredMessage="No records found as '{0}'.
		Try to change filter to see records here." />
								<Levels>
									<px:PXGridLevel DataMember="LocationsToLock">
										<RowTemplate>
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="LocationCD" />
											<px:PXGridColumn DataField="Descr" />
										</Columns>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" />
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXGrid ID="gridExcludedLocations" runat="server" DataSourceID="ds" Width="100%"
								BorderWidth="0px" SkinID="Details" Caption="Excluded Locations" CaptionVisible="true" AllowPaging="True" TabIndex="1200" FilesIndicator="False" NoteIndicator="False">
								<EmptyMsg  AnonFilteredAddMessage="No records found.
		Try to change filter to see records here." AnonFilteredMessage="No records found.
		Try to change filter to see records here." ComboAddMessage="No records found.
		Try to change filter or modify parameters above to see records here." FilteredAddMessage="No records found.
		Try to change filter to see records here." FilteredMessage="No records found.
		Try to change filter to see records here." NamedComboAddMessage="No records found as '{0}'.
		Try to change filter or modify parameters above to see records here." NamedComboMessage="No records found as '{0}'.
		Try to change filter or modify parameters above to see records here." NamedFilteredAddMessage="No records found as '{0}'.
		Try to change filter to see records here." NamedFilteredMessage="No records found as '{0}'.
		Try to change filter to see records here." />
								<Levels>
									<px:PXGridLevel DataMember="ExcludedLocations">
										<RowTemplate>
											<px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True">
											</px:PXSegmentMask>
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="LocationID" AutoCallBack="True">
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Descr" >
											</px:PXGridColumn>
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" />
							</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Item Selection">
				<Template>
					<px:PXPanel DataMember="CurrentGeneratorSettings" runat="server">
						<px:PXLayoutRule runat="server" StartRow="true" LabelsWidth="SM" ControlSize="XM" />
						<px:PXDropDown ID="edMethod2" runat="server" AllowNull="False" DataField="Method" Enabled="False" />
					</px:PXPanel>
					<px:PXSplitContainer ID="splitContainerItems" runat="server" PositionInPercent="true" SplitterPosition="50" Orientation="Vertical">
						<AutoSize Enabled="true" Container="Window" />
						<Template1>
							<px:PXGrid ID="gridInventoryItems" runat="server" DataSourceID="ds" Width="100%" 
								BorderWidth="0px" SkinID="Details" Caption="Selected Inventory Items" CaptionVisible="true" AllowPaging="True" TabIndex="1200" FilesIndicator="False" NoteIndicator="False">
								<EmptyMsg AnonFilteredAddMessage="No records found.
		Try to change filter to see records here." AnonFilteredMessage="No records found.
		Try to change filter to see records here." ComboAddMessage="No records found.
		Try to change filter or modify parameters above to see records here." FilteredAddMessage="No records found.
		Try to change filter to see records here." FilteredMessage="No records found.
		Try to change filter to see records here." NamedComboAddMessage="No records found as '{0}'.
		Try to change filter or modify parameters above to see records here." NamedComboMessage="No records found as '{0}'.
		Try to change filter or modify parameters above to see records here." NamedFilteredAddMessage="No records found as '{0}'.
		Try to change filter to see records here." NamedFilteredMessage="No records found as '{0}'.
		Try to change filter to see records here." />
								<Levels>
									<px:PXGridLevel DataMember="InventoryItemsToLock">
										<RowTemplate>
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="InventoryCD" />
											<px:PXGridColumn DataField="Descr" />
										</Columns>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" />
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXGrid ID="gridExcludedInventoryItems" runat="server" DataSourceID="ds" Width="100%"
								BorderWidth="0px" SkinID="Details" Caption="Excluded Inventory Items" CaptionVisible="true" AllowPaging="True" TabIndex="1200" FilesIndicator="False" NoteIndicator="False">
								<EmptyMsg  AnonFilteredAddMessage="No records found.
		Try to change filter to see records here." AnonFilteredMessage="No records found.
		Try to change filter to see records here." ComboAddMessage="No records found.
		Try to change filter or modify parameters above to see records here." FilteredAddMessage="No records found.
		Try to change filter to see records here." FilteredMessage="No records found.
		Try to change filter to see records here." NamedComboAddMessage="No records found as '{0}'.
		Try to change filter or modify parameters above to see records here." NamedComboMessage="No records found as '{0}'.
		Try to change filter or modify parameters above to see records here." NamedFilteredAddMessage="No records found as '{0}'.
		Try to change filter to see records here." NamedFilteredMessage="No records found as '{0}'.
		Try to change filter to see records here." />
								<Levels>
									<px:PXGridLevel DataMember="ExcludedInventoryItems">
										<RowTemplate>
											<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AutoRefresh="True">
											</px:PXSegmentMask>
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="InventoryID" AutoCallBack="True">
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Descr" >
											</px:PXGridColumn>
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" />
							</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
		</Items>
	</px:PXTab>
</asp:Content>
