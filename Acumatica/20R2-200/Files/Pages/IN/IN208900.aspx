<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false"
	CodeFile="IN208900.aspx.cs" Inherits="Page_IN208900" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.Objects.IN.INPIClassMaint" Visible="True"
		TabIndex="1" PrimaryView="Classes">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="addLocation" CommitChanges="True" Visible="false" />
			<px:PXDSCallbackCommand Name="addItem" CommitChanges="True" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Classes"
		Caption="Physical Inventory Type Summary" DefaultControlID="edPIClassID">
		<Template>
			<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edPIClassID" runat="server" DataField="PIClassID">
				<AutoCallBack Enabled="true" Command="Cancel" Target="ds" />
			</px:PXSelector>
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
			<px:PXDropDown ID="edMethod" runat="server" DataField="Method">
				<AutoCallBack Enabled="true" Command="Save" Target="form" />
			</px:PXDropDown>
			<px:PXCheckBox ID="chkIncludeZeroItems" runat="server" DataField="IncludeZeroItems"/>
			<px:PXCheckBox ID="chkUnlockSiteOnCountingFinish" runat="server" DataField="UnlockSiteOnCountingFinish" CommitChanges="true"/>
			<px:PXCheckBox ID="chkHideBookQty" runat="server" DataField="HideBookQty" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="261px" Width="100%" DataMember="CurrentClass">
		<Items>
			<px:PXTabItem Text="Inventory Item Selection" BindingContext="form" VisibleExp="DataControls[&quot;edMethod&quot;].Value == I">
				<Template>
					<px:PXPanel runat="server" ID="pnlInvItemSel">
						<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" Merge="True" />
						<px:PXDropDown ID="edSelectedMethod" runat="server" AllowNull="False" DataField="SelectedMethod">
							<AutoCallBack Command="Save" Target="tab" />
						</px:PXDropDown>
						<px:PXNumberEdit ID="edRandomItemsLimit" runat="server" DataField="RandomItemsLimit" ValueType="Int16" />
						<px:PXNumberEdit ID="edLastCountPeriod" runat="server" DataField="LastCountPeriod" ValueType="Int16" />
					</px:PXPanel>
					<px:PXGrid ID="gridItems" runat="server" DataSourceID="ds" Height="150px" Width="100%" Caption="Items"
						SkinID="DetailsInTab" AllowPaging="True">
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Add Item">
									<AutoCallBack Command="addItem" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Items" DataKeyNames="PIClassID,InventoryID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" />
									<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" HintField="descr" Width="225px" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="PIClassID" Visible="False" Width="120px" />
									<px:PXGridColumn DataField="InventoryID" AutoCallBack="True" />
									<px:PXGridColumn DataField="InventoryItem__Descr" Label="InventoryItem-Description" Width="200px" />
									<px:PXGridColumn AllowNull="False" DataField="InventoryItem__ItemStatus" DefValueText="AC" RenderEditorText="True" />
									<px:PXGridColumn DataField="InventoryItem__ItemClassID" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="ABC Code Selection" BindingContext="form" VisibleExp="DataControls[&quot;edMethod&quot;].Value == A">
				<Template>
					<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" />
					<px:PXSelector ID="edABCCodeID" runat="server" DataField="ABCCodeID" HintField="descr" />
					<px:PXCheckBox ID="chkByFrequency" runat="server" DataField="ByABCFrequency" Text="By Frequency">
						<AutoCallBack Command="Save" Target="tab" />
					</px:PXCheckBox>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Movement Class Selection" BindingContext="form" VisibleExp="DataControls[&quot;edMethod&quot;].Value == M">
				<Template>
					<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" />
					<px:PXSelector ID="edMovementClassID" runat="server" DataField="MovementClassID" />
					<px:PXCheckBox ID="chkByMovementClassFrequency" runat="server" DataField="ByMovementClassFrequency" Text="By Frequency">
						<AutoCallBack Command="Save" Target="tab" />
					</px:PXCheckBox>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="PI Cycle Selection" BindingContext="form" VisibleExp="DataControls[&quot;edMethod&quot;].Value == Y">
				<Template>
					<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" />
					<px:PXSelector ID="edCycleID" runat="server" DataField="CycleID" />
					<px:PXCheckBox ID="chkByCycleFrequency" runat="server" DataField="ByCycleFrequency">
						<AutoCallBack Command="Save" Target="tab" />
					</px:PXCheckBox>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Item Class Selection" BindingContext="form" VisibleExp="DataControls[&quot;edMethod&quot;].Value == C">
				<Template>
					<px:PXGrid ID="gridItemClasses" runat="server" DataSourceID="ds" Height="150px" Width="100%" Caption="Item Classes"
						SkinID="DetailsInTab" AllowPaging="True">
						<Levels>
							<px:PXGridLevel DataMember="ItemClasses" DataKeyNames="PIClassID,ItemClassID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" />
									<px:PXSegmentMask ID="edItemClassID" runat="server" DataField="ItemClassID" HintField="Descr" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="PIClassID" Visible="False" />
									<px:PXGridColumn DataField="ItemClassID" Width="120px" AutoCallBack="True" />
									<px:PXGridColumn DataField="INItemClass__Descr" Width="250px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Warehouse/Location Selection">
				<Template>
					<px:PXPanel runat="server" ID="pnlWarehouseSel">
						<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" />
						<px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID">
							<AutoCallBack Command="Save" Target="tab" />
							<GridProperties FastFilterFields="Descr">
								<Layout ColumnsMenu="False" />
							</GridProperties>
						</px:PXSegmentMask>
					</px:PXPanel>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="200px" Width="100%" Caption="Locations"
						SkinID="DetailsInTab">
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Add Location">
									<AutoCallBack Command="addLocation" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Locations" DataKeyNames="PIClassID,LocationID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" />
									<px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="LocationID" AutoCallBack="True" />
									<px:PXGridColumn DataField="INLocation__Descr" Width="200px" />
									<px:PXGridColumn AllowNull="False" DataField="INLocation__PickPriority" DataType="Int16" DefValueText="1"
										TextAlign="Right" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Assignment Order">
				<Template>
					<px:PXPanel ID="pnlNumberAssignmentOrder" runat="server" Caption="Line and Tag Number Assignment Order"
						RenderStyle="Fieldset">
						<px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="S" StartColumn="True" />
						<px:PXLabel ID="PXLabel1" runat="server" Width="75px"></px:PXLabel>
						<px:PXLayoutRule runat="server" LabelsWidth="XXS" ControlSize="M" StartColumn="True" />
						<px:PXDropDown ID="edNAO1" runat="server" DataField="NAO1">
							<AutoCallBack Command="Save" Target="form" />
						</px:PXDropDown>
						<px:PXDropDown ID="edNAO2" runat="server" DataField="NAO2">
							<AutoCallBack Command="Save" Target="form" />
						</px:PXDropDown>
						<px:PXDropDown ID="edNAO3" runat="server" DataField="NAO3">
							<AutoCallBack Command="Save" Target="form" />
						</px:PXDropDown>
						<px:PXDropDown ID="edNAO4" runat="server" DataField="NAO4">
							<AutoCallBack Command="Save" Target="form" />
						</px:PXDropDown>
					</px:PXPanel>
					<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" />
					<px:PXNumberEdit ID="edBlankLines" runat="server" DataField="BlankLines">
					</px:PXNumberEdit>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Enabled="true" Container="Window" />
	</px:PXTab>
	<px:PXSmartPanel ID="PanelLF" runat="server" DesignView="Content" Width="400px" Caption="Add Locations" Key="LocationFilter" AutoCallBack-Enabled="True" AutoCallBack-Target="optform" AutoCallBack-Command="Refresh" CaptionVisible="True">
		<px:PXFormView ID="optform" runat="server" Width="100%" CaptionVisible="False" DataMember="LocationFilter" DataSourceID="ds" SkinID="Transparent">
			<Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SSM" ControlSize="XM" />
			    <px:PXSegmentMask ID="edStartLocationID" runat="server" DataField="StartLocationID" AutoRefresh="true"/>
			    <px:PXSegmentMask ID="edEndLocationID" runat="server" DataField="EndLocationID" AutoRefresh="true"/>
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton7" runat="server" Text="Add" DialogResult="OK" />
			<px:PXButton ID="PXButton8" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelIF" runat="server" DesignView="Content" Width="400px" Caption="Add Items" Key="InventoryFilter" AutoCallBack-Enabled="True" AutoCallBack-Target="ifilter" AutoCallBack-Command="Refresh" CaptionVisible="true">
		<px:PXFormView ID="ifilter" runat="server" Width="100%" CaptionVisible="False" DataMember="InventoryFilter" DataSourceID="ds" SkinID="Transparent">
			<Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SSM" ControlSize="XM" />
			    <px:PXSegmentMask ID="edStartInventoryID" runat="server" DataField="StartInventoryID" />
			    <px:PXSegmentMask ID="edEndInventoryID" runat="server" DataField="EndInventoryID" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton1" runat="server" Text="Add" DialogResult="OK" />
			<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
