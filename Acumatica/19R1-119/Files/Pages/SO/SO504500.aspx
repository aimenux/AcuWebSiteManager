<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO504500.aspx.cs" Inherits="Page_SO504500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SO.SOPostOrder" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Process" StartNewGroup="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" />			
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Height="72px" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection Filter">
		<Template>
			<px:PXLabel ID="lblSiteID" runat="server" Style="z-index: 102; left: 9px; position: absolute;
				top: 9px">Warehouse ID :</px:PXLabel>
			<px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID" 
				  HintField="descr" HintLabelID="lblSiteIDH"
				LabelID="lblSiteID" Style="z-index: 103; left: 126px; position: absolute; top: 9px"
				TabIndex="-1" Width="81px">
				<Items>
					<px:PXMaskItem EditMask="AlphaNumeric" EmptyChar="_" Length="10" Separator="-" TextCase="Upper" />
				</Items>
				<GridProperties FastFilterFields="Descr">
					
				<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
				<AutoCallBack Enabled="true" Target="form" Command="Save"></AutoCallBack>
			</px:PXSegmentMask>
			<px:PXLabel ID="lblSiteIDH" runat="server" Style="z-index: 104; left: 216px; position: absolute;
				top: 9px"></px:PXLabel>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" AllowPaging="true" AdjustPageSize="Auto" SkinID="Details" AllowSearch="true" BatchUpdate="true" Caption="Orders">
		<Levels>
			<px:PXGridLevel DataMember="Orders" >
				<RowTemplate>
					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" Style="z-index: 100;
						position: absolute; left: 126px; top: 9px;" TabIndex="10" Text="Selected">
					</px:PXCheckBox>
					<px:PXLabel ID="lblOrderType" runat="server" Style="z-index: 100; left: 9px; position: absolute;
						top: 9px">Type :</px:PXLabel>
					<px:PXSelector ID="edOrderType" runat="server" AllowNull="True" DataField="OrderType" 
						  LabelID="lblOrderType"
						 Style="z-index: 103; left: 126px; position: absolute; top: 9px"
						TabIndex="1" Width="90px" AutoRefresh="true">
						<GridProperties>
							
							<Layout ColumnsMenu="False" />
							<PagerSettings Mode="NextPrevFirstLast" />
						</GridProperties>
						<AutoCallBack Command="Cancel" Enabled="True" Target="ds">
						</AutoCallBack>
					</px:PXSelector>
					<px:PXLabel ID="lblOrderNbr" runat="server" Style="z-index: 102; left: 9px; position: absolute;
						top: 36px">Reference Nbr :</px:PXLabel>
					<px:PXSelector ID="edOrderNbr" runat="server" AllowNull="True" DataField="OrderNbr" 
						  LabelID="lblOrderNbr"
						 Style="z-index: 103; left: 126px; position: absolute; top: 36px"
						TabIndex="2" Width="108px" AutoRefresh="true">
						<GridProperties>
							<Columns>
								<px:PXGridColumn DataField="OrderType"  Width="70px">
									<Header Text="Type">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="OrderNbr"  Width="70px">
									<Header Text="Reference Nbr">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="OrderDate" DataType="DateTime" Width="90px">
									<Header Text="Date">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="CustomerID" Width="70px">
									<Header Text="Customer ID">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="CustomerLocationID"  Width="70px">
									<Header Text="Location ID">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="CuryID"  Width="70px">
									<Header Text="Currency">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn AllowNull="False" DataField="CuryControlTotal" DataType="Decimal"
									Width="100px">
									<Header Text="Amount">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryOrderTotal" DataType="Decimal"
									Width="100px">
									<Header Text="Balance">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn AllowNull="False" DataField="CuryOrigDiscAmt" DataType="Decimal" Width="100px">
									<Header Text="Discount">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDiscBal" DataType="Decimal"
									Width="100px">
									<Header Text="Discount Balance">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" 
									Width="70px">
									<Header Text="Status">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="DueDate" DataType="DateTime" Width="90px">
									<Header Text="Due Date">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn DataField="DiscDate" DataType="DateTime" Width="90px">
									<Header Text="Discount Date">
									</Header>
								</px:PXGridColumn>
								<px:PXGridColumn AllowNull="False" DataField="InvoiceNbr"  Width="70px">
									<Header Text="Customer Ref">
									</Header>
								</px:PXGridColumn>
							</Columns>
							<Layout ColumnsMenu="False" />
							<PagerSettings Mode="NextPrevFirstLast" />
						</GridProperties>
					</px:PXSelector>
					<px:PXLabel ID="lblCustomerID" runat="server" Style="z-index: 104; position: absolute;
						left: 9px; top: 63px;">Customer ID :</px:PXLabel>
					<px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" 
						  Enabled="False" HintField="acctName"
						HintLabelID="lblCustomerIDH" LabelID="lblCustomerID" Style="z-index: 105; position: absolute;
						left: 126px; top: 63px;" TabIndex="-1" Width="81px">
						<Items>
							<px:PXMaskItem EditMask="AlphaNumeric" Length="10" Separator="-" />
						</Items>
						<GridProperties FastFilterFields="AcctName">
							
						<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
					</px:PXSegmentMask>
					<px:PXLabel ID="lblCustomerIDH" runat="server" Style="z-index: 106; position: absolute;
						left: 216px; top: 63px;"></px:PXLabel>
					<px:PXLabel ID="lblCustomerLocationID" runat="server" Style="z-index: 107; position: absolute;
						left: 9px; top: 90px;">Location ID :</px:PXLabel>
					<px:PXSegmentMask ID="edCustomerLocationID" runat="server" DataField="CustomerLocationID"
						 
						 Enabled="False" HintField="descr" HintLabelID="lblCustomerLocationIDH"
						LabelID="lblCustomerLocationID" Style="z-index: 108; position: absolute; left: 126px;
						top: 90px;" TabIndex="-1" Width="63px">
						<Items>
							<px:PXMaskItem EditMask="AlphaNumeric" EmptyChar="_" Length="7" Separator="-" TextCase="Upper" />
						</Items>
						<GridProperties FastFilterFields="Descr">
							
						<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
					</px:PXSegmentMask>
					<px:PXLabel ID="lblCustomerLocationIDH" runat="server" Style="z-index: 109; position: absolute;
						left: 198px; top: 90px;"></px:PXLabel>
					<px:PXLabel ID="lblShipDate" runat="server" Style="z-index: 110; position: absolute;
						left: 9px; top: 117px;">Date :</px:PXLabel>
					<px:PXDateTimeEdit ID="edShipDate" runat="server" DataField="ShipDate" Enabled="False"
						LabelID="lblShipDate" Style="z-index: 111; position: absolute; left: 126px; top: 117px;"
						TabIndex="-1" Width="90px">
					</px:PXDateTimeEdit>
					<px:PXLabel ID="lblCuryID" runat="server" Style="z-index: 112; position: absolute;
						left: 9px; top: 144px;">Currency :</px:PXLabel>
					<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" 
						  Enabled="False" 
						LabelID="lblCuryID"  Style="z-index: 113; position: absolute; left: 126px;
						top: 144px;" TabIndex="-1" Width="54px">
						<GridProperties>
							
						<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
					</px:PXSelector>
					<px:PXLabel ID="lblOrderQty" runat="server" Style="z-index: 114; position: absolute;
						left: 9px; top: 171px;">Shipment Total :</px:PXLabel>
					<px:PXNumberEdit ID="edOrderQty" runat="server" AllowNull="False" DataField="OrderQty"
						Decimals="2" Enabled="False" LabelID="lblOrderQty" 
						 Style="z-index: 115; position: absolute; left: 126px;
						top: 171px;" TabIndex="16" ValueType="Decimal" Width="81px">
					</px:PXNumberEdit>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" DataType="Boolean" DefValueText="False"
						TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" Width="20px">
						<Header Text="Selected">
						</Header>
					</px:PXGridColumn>
					<px:PXGridColumn AllowUpdate="False" DataField="OrderType"  Width="60px">
						<Header Text="Order Type">
						</Header>
					</px:PXGridColumn>
					<px:PXGridColumn AllowUpdate="False" DataField="OrderNbr"  Width="108px">
						<Header Text="Order Nbr">
						</Header>
					</px:PXGridColumn>
					<px:PXGridColumn AllowUpdate="False" DataField="CustomerID" DisplayFormat="AAAAAAAAAA"
						Width="81px">
						<Header Text="Customer ID">
						</Header>
					</px:PXGridColumn>
					<px:PXGridColumn AllowUpdate="False" DataField="CustomerLocationID" DisplayFormat="&gt;AAAAAAA"
						Width="63px">
						<Header Text="Location ID">
						</Header>
					</px:PXGridColumn>
					<px:PXGridColumn AllowUpdate="False" DataField="ShipDate" DataType="DateTime" Width="90px">
						<Header Text="Date">
						</Header>
					</px:PXGridColumn>
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="OrderQty"
						DataType="Decimal" Decimals="2" DefValueText="0.0" TextAlign="Right" Width="81px">
						<Header Text="Order Qty">
						</Header>
					</px:PXGridColumn>
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
