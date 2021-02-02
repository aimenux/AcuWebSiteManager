<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SP408033.aspx.cs" Inherits="Page_SP408033"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="SP.Objects.SP.SPBussnessAccountInquire" 
        PrimaryView="FilteredItems" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="viewDetails" />			
		</CallbackCommands>		
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px"
		Width="100%" ActionsPosition="Top" Caption="Business Accounts" AllowPaging="true" AdjustPageSize="auto" 
		SkinID="PrimaryInquire" FastFilterFields="AcctCD,AcctName" AutoGenerateColumns="AppendDynamic">
		<Levels>
			<px:PXGridLevel DataMember="FilteredItems">
				<Columns>
					<px:PXGridColumn DataField="AcctCD" Width="150px" LinkCommand="ViewDetails"/>
					<px:PXGridColumn DataField="AcctName" Width="200px" />
					<px:PXGridColumn DataField="Type" Width="80px" />
					<px:PXGridColumn DataField="ClassID" Width="90px" />
					<px:PXGridColumn DataField="Contact__EMail" Width="190px" />
					<px:PXGridColumn DataField="Contact__Phone1" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Address__AddressLine1" Width="300px" />
					<px:PXGridColumn DataField="Address__City" Width="80px" />
					<px:PXGridColumn DataField="Address__CountryID" Width="160px" /> 
					 <%--<px:PXGridColumn DataField="Status" Width="90px" />--%>						
					 <%--<px:PXGridColumn DataField="BAccountParent__AcctCD" Width="150px"/>
					<px:PXGridColumn DataField="BAccountParent__AcctName" Width="200px" />
					<px:PXGridColumn DataField="State__name" Width="160px" />
                    <px:PXGridColumn DataField="Address__AddressLine2" Width="300px" />
					<px:PXGridColumn DataField="Contact__Phone2" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Contact__Phone3" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Contact__Fax" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Contact__WebSite" Width="140px" />
                    <px:PXGridColumn DataField="Contact__DuplicateStatus" Width="140px" />
					<px:PXGridColumn DataField="TaxZoneID" Width="80px" />
					<px:PXGridColumn DataField="Location__CCarrierID" Width="100px" />
					<px:PXGridColumn DataField="WorkgroupID" Width="110px" />
					<px:PXGridColumn DataField="OwnerID" Width="110px" DisplayMode="Text" />
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="110px" SyncVisible="False" SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="CreatedDateTime" Width="90px" />
					<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Width="110px" SyncVisible="False" SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="LastModifiedDateTime" Width="90px" />--%>
				</Columns>
			</px:PXGridLevel>
		</Levels>
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
		<ActionBar DefaultAction="cmdItemDetails" PagerVisible="False">
		    <PagerSettings Mode="NextPrevFirstLast" />
			<CustomItems>
				<px:PXToolBarButton Text="Business Accounts Details" Tooltip="Business Accounts Details" 
                    Key="cmdItemDetails" Visible="false">
				    <Images Normal="main@DataEntry" />
					<AutoCallBack Command="ViewDetails" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>	
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar> 
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<CallbackCommands>
			<Refresh CommitChanges="True" PostData="Page" />
		</CallbackCommands>
	</px:PXGrid>
</asp:Content>
