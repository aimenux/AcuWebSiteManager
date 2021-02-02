<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN507000.aspx.cs" Inherits="Page_IN507000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INCreateAssembly" PrimaryView="Filter">
		<%--<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>--%>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"  
        DataMember="Filter" Caption="Selection"
		Width="100%" Height="64px" DefaultControlID="edSiteID" TabIndex="300">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" 
                ControlSize="XL" />
			<px:PXSegmentMask CommitChanges="True" runat="server" DataField="SiteID" 
                ID="edSiteID" DataSourceID="ds"  />
			<px:PXSegmentMask CommitChanges="True" runat="server" DataField="LocationID" 
                ID="edLocationID" DataSourceID="ds"  />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100"  Caption="Kits to Assemble"
		Width="100%" Height="150px" SkinID="Inquire" TabIndex="100">
		<Levels>
			<px:PXGridLevel DataMember="Records">
				<RowTemplate>
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" Enabled="False"  />
					<px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" Enabled="False"  />
					<px:PXNumberEdit ID="edQtyOnHand" runat="server" DataField="QtyOnHand" Enabled="False"  />
					<px:PXNumberEdit ID="edQtyAvail" runat="server" DataField="QtyAvail" Enabled="False"  />
					<px:PXTextEdit ID="edInventoryItem__Descr" runat="server" DataField="InventoryItem__Descr"  />
					<px:PXSelector ID="edInventoryItem__BaseUnit" runat="server" DataField="InventoryItem__BaseUnit"  /></RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn AllowUpdate="False" DataField="InventoryID" DisplayFormat="&gt;AAA-&gt;CCC-&gt;AA" Width="81px" />
					<px:PXGridColumn DataField="InventoryItem__Descr" Width="351px" />
					<px:PXGridColumn AllowUpdate="False" DataField="SubItemID" DisplayFormat="&gt;AA-A" Width="45px" />
					<px:PXGridColumn AllowUpdate="False" DataField="SiteID" DisplayFormat="&gt;AAAAAAAAAA" Visible="False" />
					<px:PXGridColumn AllowUpdate="False" DataField="LocationID" DisplayFormat="&gt;AAAAAAAAAA" Visible="False" />
					<px:PXGridColumn DataField="InventoryItem__BaseUnit" DisplayFormat="&gt;aaaaaa" Width="63px" />					
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtyOnHand" TextAlign="Right" Width="81px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtyINAssemblySupply" TextAlign="Right" Width="81px" />					
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="QtyAvail" TextAlign="Right" Width="81px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
