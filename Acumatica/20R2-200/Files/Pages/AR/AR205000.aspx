<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AR205000.aspx.cs" Inherits="Page_AR205000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Salesperson"
		TypeName="PX.Objects.AR.SalesPersonMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDetails" Visible="False" StartNewGroup="True" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" StartNewGroup="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="frmHeader" runat="server" Width="100%" Caption="Salesperson Info"
		DataMember="Salesperson" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="true"
		ActivityField="NoteActivity" BorderStyle="None" DefaultControlID="edSalesPersonCD"
		TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSegmentMask ID="edSalesPersonCD" runat="server" DataField="SalesPersonCD" />
			<px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
			<px:PXNumberEdit ID="edCommnPct" runat="server" DataField="CommnPct" />
			<px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="300px" Width="100%" DataMember="SalespersonCurrent"
		TabIndex="200">
		<Items>
			<px:PXTabItem Text="Customers">
				<Template>
					<px:PXGrid ID="grdSPCustomers" runat="server" Height="300px" Width="100%" SkinID="DetailsInTab"
						TabIndex="300">
						<Levels>
							<px:PXGridLevel DataMember="SPCustomers">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXSegmentMask ID="PXSegmentMask1" runat="server" DataField="BAccountID" />
									<px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="true" />
									<px:PXNumberEdit ID="edCommisionPct" runat="server" DataField="CommisionPct" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="BAccountID"/>
									<px:PXGridColumn DataField="BAccountID_Customer_acctName" />
									<px:PXGridColumn DataField="LocationID" />
									<px:PXGridColumn DataField="LocationID_Location_descr" />
									<px:PXGridColumn DataField="CommisionPct" TextAlign="Right" />
                                    <px:PXGridColumn DataField="IsDefault" Type="CheckBox" TextAlign="Center" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<Actions>
								<Save Enabled="False" />
							</Actions>
						</ActionBar>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text=" Commission History" RepaintOnDemand="false">
				<Template>
					<px:PXGrid ID="grid" runat="server" Height="300px" Width="100%" SkinID="DetailsInTab" TabIndex="200">
						<Levels>
							<px:PXGridLevel DataMember="CommissionsHistory">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
									<px:PXMaskEdit ID="edCommnPeriod" runat="server" DataField="CommnPeriod" />
									<px:PXNumberEdit ID="edCommnblAmt" runat="server" DataField="CommnblAmt" />
									<px:PXNumberEdit ID="edCommnAmt" runat="server" DataField="CommnAmt" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="CommnPeriod" />
									<px:PXGridColumn DataField="CommnblAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CommnAmt" TextAlign="Right" />
                                    <px:PXGridColumn  DataField="PRProcessedDate"  />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar DefaultAction="cmdViewDetails">
							<Actions>
								<Save Enabled="False" />
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
								<EditRecord Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="View Details" Key="cmdViewDetails">
								    <AutoCallBack Command="ViewDetails" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
