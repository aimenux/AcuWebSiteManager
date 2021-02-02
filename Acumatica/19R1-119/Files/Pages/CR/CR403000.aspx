<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR403000.aspx.cs" Inherits="Page_CR403000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.OpportunityEnq"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="FilteredItems_ViewDetails" />	
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="FilteredItems_BAccount_ViewDetails" />	
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="FilteredItems_BAccountParent_ViewDetails" />	
			<pxa:PXExtendedDSCallbackCommand Name="FilteredItems_AddNew" ForDashboard="true" />
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
			<px:PXSelector AutoRefresh="True" CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID" />
			<px:PXCheckBox CommitChanges="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="False" />
			<px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" ControlSize="XM"/>
			<px:PXSelector AutoRefresh="True" CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
			<px:PXCheckBox CommitChanges="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
			<px:PXLayoutRule  runat="server" Merge="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" 
		Width="100%" ActionsPosition="Top" Caption="Opportunities" AllowPaging="true"
		AdjustPageSize="auto" SkinID="Inquire" FastFilterFields="OpportunityID,Subject" AutoGenerateColumns="AppendDynamic" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="FilteredItems">
				<Columns>
					<px:PXGridColumn DataField="OpportunityID" Width="130px" LinkCommand="FilteredItems_ViewDetails"/>
					<px:PXGridColumn AllowNull="False" DataField="Subject" Width="300px"/>
					<px:PXGridColumn AllowNull="False" DataField="Status" Width="90px" />
					<px:PXGridColumn AllowNull="False" DataField="Resolution" Width="90px" />  
					<px:PXGridColumn AllowNull="False" DataField="StageID" Width="90px" />
					<px:PXGridColumn AllowNull="False" DataField="CROpportunityProbability__Probability" TextAlign="Right" Width="90px" /> 
                    <px:PXGridColumn DataField="CRActivityStatistics__LastIncomingActivityDate" Width="120px" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="CRActivityStatistics__LastOutgoingActivityDate" Width="120px" DisplayFormat="g" TimeMode="True" />
					<px:PXGridColumn DataField="LastActivity" Width="90px" /> 
					<px:PXGridColumn DataField="CloseDate" Width="90px" />
					<px:PXGridColumn DataField="CuryID" Width="60px" />
					<px:PXGridColumn AllowNull="False" DataField="CuryProductsAmount" TextAlign="Right" Width="90px" /> 
					<px:PXGridColumn DataField="ClassID" Width="80px" RenderEditorText="True" /> 
					
					<px:PXGridColumn DataField="Source" Width="90px" RenderEditorText="True"/>

					<px:PXGridColumn DataField="BAccount__AcctCD" Width="150px" RenderEditorText="True" LinkCommand="FilteredItems_BAccount_ViewDetails"/>
					<px:PXGridColumn DataField="BAccount__AcctName" Width="200px"/>
					<px:PXGridColumn DataField="BAccountParent__AcctCD" RenderEditorText="True" Width="150px" LinkCommand="FilteredItems_BAccountParent_ViewDetails"/>
					<px:PXGridColumn DataField="BAccountParent__AcctName" Width="200px"/>
					
					<px:PXGridColumn DataField="WorkgroupID" Width="110px" />
					<px:PXGridColumn DataField="OwnerID" Width="110px" DisplayMode="Text"/> 
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="110px" SyncVisible="False" SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="CreatedDateTime" Width="90px" />
					<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Width="110px" SyncVisible="False" SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="LastModifiedDateTime" Width="90px" /> 
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="cmdItemDetails" PagerVisible="False">
			<CustomItems>
				<px:PXToolBarButton Text="Opportunity Details" Tooltip="Opportunity Details" Key="cmdItemDetails" Visible="false">
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
