<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR405000.aspx.cs" Inherits="Page_CR405000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.CaseEnq"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="FilteredItems_ViewDetails" />  
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="FilteredItems_BAccount_ViewDetails"/>
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="FilteredItems_BAccountParent_ViewDetails"/>
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="FilteredItems_Contact_ViewDetails"/>
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="FilteredItems_Location_ViewDetails"/>	
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="FilteredItems_Contract_ViewDetails"/> 
            <px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="FilteredItems_Contract_CustomerID_ViewDetails"/>             
			<pxa:PXExtendedDSCallbackCommand Name="FilteredItems_AddNew" ForDashboard="true" PopupCommandTarget="ds" PopupCommand="Cancel" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="TakeCase"/> 
			<px:PXDSCallbackCommand Name="Save" Visible="False"/> 
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Filter" Caption="Selection">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"/>
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" ControlSize="XM"/>
			<px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" AutoRefresh="True" CommitChanges="True"/>
			<px:PXCheckBox  ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" CommitChanges="True"/>
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="False" />
			<px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" ControlSize="XM"/>
			<px:PXSelector ID="edWorkGroupID" runat="server" DataField="WorkGroupID" CommitChanges="True" AutoRefresh="True"/>
			<px:PXCheckBox ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" CommitChanges="True"/>
			<px:PXLayoutRule ID="PXLayoutRule5"  runat="server" Merge="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" 
		Width="100%" ActionsPosition="Top" Caption="Cases" AllowPaging="True" AdjustPageSize="Auto"
		SkinID="Inquire" FastFilterFields="CaseCD,Subject" AutoGenerateColumns="AppendDynamic" PageSize="40" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="FilteredItems">
			    <RowTemplate>
			        <px:PXTimeSpan ID="PXTimeSpan1" runat="server" DataField="Age" InputMask="dddd:hh:mm"/>
                    <px:PXTimeSpan ID="PXTimeSpan2" runat="server" DataField="LastActivityAge" InputMask="dddd:hh:mm"/>
			    </RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="CaseCD" DisplayFormat="&gt;aaaaaaaaaa" Width="90px" LinkCommand="FilteredItems_ViewDetails"/>
					<px:PXGridColumn DataField="Subject" Width="300px"/>
					<px:PXGridColumn AllowNull="False" DataField="Status" Width="90px"/>
					<px:PXGridColumn AllowNull="False" DataField="Resolution" Width="90px"/>
					<px:PXGridColumn AllowNull="False" DataField="Severity" Width="90px"/>
					<px:PXGridColumn AllowNull="False" DataField="Priority" Width="90px"/>
					<px:PXGridColumn DataField="ETA" Width="90px"/>
					<px:PXGridColumn DataField="TimeEstimated" Width="90px"/> 	
					<px:PXGridColumn DataField="RemaininingDate" Width="90px"/>					
                    <px:PXGridColumn DataField="Age" Width="120px"/>
                    <px:PXGridColumn DataField="CRActivityStatistics__LastIncomingActivityDate" Width="120px" DisplayFormat="g" />
                    <px:PXGridColumn DataField="CRActivityStatistics__LastOutgoingActivityDate" Width="120px" DisplayFormat="g" />
                    <px:PXGridColumn DataField="LastActivity" Width="120px" DisplayFormat="g" />
                    <px:PXGridColumn DataField="LastActivityAge" Width="120px"/>
                    <px:PXGridColumn DataField="LastModified" Width="120px" />
					<px:PXGridColumn DataField="CaseClassID" DisplayFormat="&gt;aaaaaaaaaa" Width="80px"/>	 
					<px:PXGridColumn DataField="BAccount__AcctCD" Width="150px" LinkCommand="FilteredItems_BAccount_ViewDetails"/>
					<px:PXGridColumn DataField="BAccount__AcctName" Width="200px" />  
					<px:PXGridColumn DataField="BAccountParent__AcctCD" RenderEditorText="True" Width="150px" LinkCommand="FilteredItems_BAccountParent_ViewDetails"/>
					<px:PXGridColumn DataField="BAccountParent__AcctName" Width="200px"/>
					<px:PXGridColumn DataField="Contact__DisplayName" Width="150px" LinkCommand="FilteredItems_Contact_ViewDetails"/>                    
					<px:PXGridColumn DataField="LocationID" Width="90px" LinkCommand="FilteredItems_Location_ViewDetails"/>	
					<px:PXGridColumn DataField="Contract__ContractCD" Width="90px" LinkCommand="FilteredItems_Contract_ViewDetails"/> 
					<px:PXGridColumn DataField="Contract__Description" Width="90px"/> 
                    <px:PXGridColumn DataField="Contract__CustomerID" Width="150px" LinkCommand="FilteredItems_Contract_CustomerID_ViewDetails"/>
                    <px:PXGridColumn DataField="BAccountContract__AcctName" Width="150px"/>                    
					<px:PXGridColumn DataField="InitResponse" Width="90px"/>  
					<px:PXGridColumn DataField="TimeResolution" Width="90px"/>  
					<px:PXGridColumn DataField="WorkgroupID" Width="110px" />
					<px:PXGridColumn DataField="OwnerID" Width="110px" DisplayMode="Text"/>
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="110px" /> 
					<px:PXGridColumn DataField="CreatedDateTime" Width="90px" /> 
					<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Width="110px" SyncVisible="False" SyncVisibility="False" Visible="False"/>	  
					<px:PXGridColumn DataField="LastModifiedDateTime" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="cmdItemDetails" PagerVisible="False">
			<CustomItems>
				<px:PXToolBarButton Text="Cases Details" Tooltip="Cases Details" Key="cmdItemDetails" Visible="false">
					<Images Normal="main@DataEntry" />
					<AutoCallBack Command="FilteredItems_ViewDetails" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>
					<ActionBar GroupIndex="0" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<CallbackCommands>
			<Refresh CommitChanges="True" PostData="Page" />
		</CallbackCommands>
	</px:PXGrid>
</asp:Content>
