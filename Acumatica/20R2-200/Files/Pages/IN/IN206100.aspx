<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="IN206100.aspx.cs" Inherits="Page_IN206100" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.EquipmentMaint"
		PrimaryView="Equipment">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="Save" PopupVisible="True" ClosePopup="True" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Height="170px" Style="z-index: 100"
		Width="100%"  DataMember="Equipment" Caption="Summary" 
		 NoteIndicator="True" 
		 FilesIndicator="True" 
		ActivityIndicator="true" ActivityField="NoteActivity" >
		<Template>
			<px:PXLabel ID="lblServiceItemCD" runat="server" Style="z-index: 100; position: absolute;
				left: 9px; top: 9px;">Equipment ID:</px:PXLabel>
			<px:PXSelector ID="edServiceItemCD" runat="server" DataField="ServiceItemCD" 
				 LabelID="lblServiceItemCD"  Style="z-index: 101;
				position: absolute; left: 117px; top: 9px;" TabIndex="-1" TextMode="ReadOnly" Width="125px">
				<AutoCallBack Command="Cancel" Enabled="True" Target="ds">
				</AutoCallBack>
				<GridProperties>
					<Columns>
						<px:PXGridColumn DataField="ServiceItemCD" >
							<Header Text="Reference Nbr">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn DataField="InventoryItem__InventoryCD" >
							<Header Text="Inventory ID">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn DataField="LotSerialNbr"  Width="200px">
							<Header Text="Lot/Serial Nbr.">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn DataField="WarrantyNbr"  Width="200px">
							<Header Text="Warranty Nbr.">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn DataField="Customer__AcctName"  Width="200px">
							<Header Text="Customer Name">
							</Header>
						</px:PXGridColumn>
					</Columns>
					<PagerSettings Mode="NextPrevFirstLast" />
				<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
			</px:PXSelector>
			<px:PXLabel ID="lblInventoryID" runat="server" Style="z-index: 102; left: 9px; position: absolute;
				top: 36px">Inventory ID :</px:PXLabel>
			<px:PXSelector ID="edInventoryID" runat="server" DataField="InventoryID" 
				 LabelID="lblInventoryID"
				Style="z-index: 103; left: 117px; position: absolute; top: 36px" TabIndex="10"
				Width="125px" ValueField="InventoryID" TextField="InventoryCD" AllowEdit="True" TextMode="Search" >
				<GridProperties>
					
					<Layout ColumnsMenu="False" />
					<PagerSettings Mode="NextPrevFirstLast" />
				</GridProperties>
				<AutoCallBack Command="Save" Enabled="True" Target="form">
				</AutoCallBack>
			</px:PXSelector>
			<px:PXLabel ID="lblLotSerialNbr" runat="server" Style="z-index: 102; left: 9px; position: absolute;
				top: 64px">Serial Number :</px:PXLabel>
			<px:PXSelector ID="edLotSerialNbr" runat="server" DataField="LotSerialNbr" 
				 LabelID="lblLotSerialNbr" ValueField="LotSerialNbr"
				Style="z-index: 103; left: 117px; position: absolute; top: 64px" TabIndex="10"
				Width="125px" AllowEdit="True" AutoRefresh="true" TextMode="Search" >
				<GridProperties>
					<Columns>
						<px:PXGridColumn AllowUpdate="False" DataField="LotSerialNbr"  Width="180px">
							<Header Text="Lot/Serial Nbr.">
							</Header>
						</px:PXGridColumn>
					</Columns>
					<Layout ColumnsMenu="False" />
					<PagerSettings Mode="NextPrevFirstLast" />
				</GridProperties>
				<AutoCallBack Command="Save" Enabled="True" Target="form">
				</AutoCallBack>
			</px:PXSelector>
			<px:PXLabel ID="lblWarrantyNbr" runat="server" Style="z-index: 102; left: 9px; position: absolute;
				top: 90px">Warranty Nbr :</px:PXLabel>
			<px:PXTextEdit ID="edWarrantyNbr" runat="server" 
				DataField="WarrantyNbr" LabelID="lblWarrantyNbr"
				Style="z-index: 103; left: 117px; position: absolute; top: 90px" TabIndex="10"
				Width="119px" />
			<px:PXLabel ID="lblModel" runat="server" Style="z-index: 102; left: 9px; position: absolute;
				top: 117px">Model :</px:PXLabel>
			<px:PXTextEdit ID="edModel" runat="server" DataField="Model" LabelID="lblModel" 
				Style="z-index: 102; left: 117px; position: absolute; top: 117px;" />
			<px:PXLabel ID="lblManufacture" runat="server" Style="z-index: 102; left: 9px; position: absolute;
				top: 144px">Manufacture :</px:PXLabel>
			<px:PXTextEdit ID="edManufacture" runat="server" DataField="Manufacture" LabelID="lblManufacture"
				Style="z-index: 102; left: 117px; position: absolute; top: 144px;" />
			<px:PXLabel ID="lblStartDate" runat="server" Style="z-index: 107; left: 297px;
				position: absolute; top: 9px">Starts On :</px:PXLabel>
			<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate"
				DisplayFormat="d" Enabled="False" LabelID="lblStartDate" Style="z-index: 108;
				left: 378px; position: absolute; top: 9px" TabIndex="5" Width="125px">
			</px:PXDateTimeEdit>
			<px:PXLabel ID="lblExpireDate" runat="server" Style="z-index: 107; left: 297px;
				position: absolute; top: 36px">Expires On :</px:PXLabel>
			<px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate"
				DisplayFormat="d" Enabled="False" LabelID="lblExpireDate" Style="z-index: 108;
				left: 378px; position: absolute; top: 36px" TabIndex="5" Width="125px">
			</px:PXDateTimeEdit>
			<px:PXLabel ID="lblCustomerID" runat="server" Style="z-index: 101; left: 297px; position: absolute;
				top: 63px">Customer ID :</px:PXLabel>
			<px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" 
				 HintField="acctName"
			                  LabelID="lblCustomerID" Style="z-index: 102; left: 378px; position: absolute; top: 63px"
				TabIndex="7" Width="126px" AllowEdit="True">
				<GridProperties FastFilterFields="AcctName">
					
					<Layout ColumnsMenu="False" />
				<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
				<Items>
					<px:PXMaskItem Length="10" Separator="-" />
				</Items>
				<AutoCallBack Command="Save" Enabled="True" Target="form" ActiveBehavior="true" >
					<Behavior CommitChanges="true" PostData="Page" RepaintControlsIDs="frmBillAddress" />
				</AutoCallBack>
			</px:PXSegmentMask>
			<px:PXLabel ID="lblContactID" runat="server" Style="z-index: 102; left: 297px; position: absolute;
				top: 90px">Contact ID :</px:PXLabel>
			<px:PXSelector ID="edContactID" runat="server" DataField="ContactID" 
				 LabelID="lblContactID"
				Style="z-index: 103; left: 378px; position: absolute; top: 90px" TabIndex="10"
				Width="126px" TextField="displayName" AutoRefresh="True" AllowEdit="True" HintField="displayName">
				<GridProperties FastFilterFields="DisplayName">
					<Columns>
						<px:PXGridColumn AllowUpdate="False" DataField="DisplayName"  Width="200px">
							<Header Text="Display Name">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn DataField="BAccountID">
							<Header Text="Business Account ID">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn DataField="Salutation"  Width="200px">
							<Header Text="Job Title">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn DataField="EMail"  Width="200px">
							<Header Text="Email">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn DataField="Phone1"  Width="200px">
							<Header Text="Phone 1">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn DataField="WorkgroupID">
							<Header Text="Workgroup">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn DataField="OwnerID"  DisplayMode="Text">
							<Header Text="Owner">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn DataField="UserID">
							<Header Text="User">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn DataField="ContactType">
							<Header Text="Type">
							</Header>
						</px:PXGridColumn>
					</Columns>
					<Layout ColumnsMenu="False" />
				<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
				<AutoCallBack Command="Save" Enabled="True" Target="form">
				</AutoCallBack>
			</px:PXSelector>
			<px:PXLabel ID="lblLastServiceDate" runat="server" Style="z-index: 107; left: 574px;
				position: absolute; top: 9px">Last Service Date :</px:PXLabel>
			<px:PXDateTimeEdit ID="edLastServiceDate" runat="server" DataField="LastServiceDate"
				DisplayFormat="d" Enabled="False" LabelID="lblLastServiceDate" Style="z-index: 108;
				left: 675px; position: absolute; top: 9px" TabIndex="5" Width="127px">
			</px:PXDateTimeEdit>
			<px:PXLabel ID="lblLastIncidentDate" runat="server" Style="z-index: 107; left: 574px;
				position: absolute; top: 36px">Last Incident :</px:PXLabel>
			<px:PXDateTimeEdit ID="edLastIncidentDate" runat="server" DataField="LastIncidentDate"
				DisplayFormat="d" Enabled="False" LabelID="lblLastIncidentDate" Style="z-index: 108;
				left: 675px; position: absolute; top: 36px" TabIndex="5" Width="127px">
			</px:PXDateTimeEdit>
			<px:PXLabel ID="lblNextServiceDate" runat="server" Style="z-index: 107; left: 574px;
				position: absolute; top: 63px">Next Service Date :</px:PXLabel>
			<px:PXDateTimeEdit ID="edNextServiceDate" runat="server" DataField="NextServiceDate"
				DisplayFormat="d" Enabled="False" LabelID="lblNextServiceDate" Style="z-index: 108;
				left: 675px; position: absolute; top: 63px" TabIndex="5" Width="127px">
			</px:PXDateTimeEdit>
			<px:PXLabel ID="lblNextServiceCode" runat="server" Style="z-index: 107; left: 574px;
				position: absolute; top: 90px">Next Service Code :</px:PXLabel>
			<px:PXTextEdit ID="edNextServiceCode" runat="server" DataField="NextServiceCode"
				Enabled="False" LabelID="lblNextServiceCode" Style="z-index: 108;
				left: 675px; position: absolute; top: 90px" TabIndex="5" Width="121px">
			</px:PXTextEdit>
		</Template>
		
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="400px">
		<Items>
			<px:PXTabItem Text="Service Calls">
				<Template>
					<px:PXGrid ID="grdServiceCalls" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px;
						top: 0px; height: 332px;" Width="100%" TabIndex="100" NoteField="NoteText" 
						BorderWidth="0px" SkinID="Details" >
						<levels>
							<px:PXGridLevel DataMember="ServiceCalls" DataKeyNames="ServiceCaseID">
								<Columns>
									<px:PXGridColumn DataField="ServiceCaseCD" Width="81px" />
									<px:PXGridColumn DataField="Subject" Width="180px" />
									<px:PXGridColumn DataField="CreatedDateTime" Width="80px" />
									<px:PXGridColumn DataField="BAccount__AcctCD" Width="81px" />
									<px:PXGridColumn DataField="BAccount__AcctName" Width="100px" />
									<px:PXGridColumn DataField="Contact__ContactID" AllowShowHide="False" Visible="false" />
									<px:PXGridColumn DataField="Contact__DisplayName" Width="100px" >
										<NavigateParams>
											<px:PXControlParam Name="ContactID" ControlID="grdServiceCalls" 
												PropertyName="DataValues[&quot;Contact__ContactID&quot;]" />
										</NavigateParams>
									</px:PXGridColumn>
								</Columns>
								<RowTemplate>
									<px:PXSelector ID="edServiceCaseCD_Dtl" runat="server" 
										DataField="ServiceCaseCD" DataSourceID="ds" DataKeyNames="ServiceCaseCD"
										AllowEdit="true" />
									<px:PXSelector ID="edCustomer_Dtl" runat="server" 
										DataField="BAccount__AcctCD" DataSourceID="ds" DataKeyNames="BAccountID"
										AllowEdit="true" />
									<px:PXSelector ID="edContact_Dtl" runat="server" 
										DataField="Contact__DisplayName" DataSourceID="ds" DataKeyNames="ContactID"
										AllowEdit="true" />
								</RowTemplate>
							</px:PXGridLevel>
						</levels>
						<AutoSize enabled="True" minheight="150" />
						<Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />
						<ActionBar>
							<Actions>
								<AddNew Enabled="false" />
								<Delete Enabled="false" />
								<EditRecord Enabled="false" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="100" MinWidth="300" />
	</px:PXTab>
</asp:Content>
