<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR503110.aspx.cs" Inherits="Page_CR503110"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" IsDefaultDatasourceWidth="100%"
		TypeName="PX.Objects.CR.AssignOpportunityMassProcess" PrimaryView="Items" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_BAccount_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_BAccountParent_ViewDetails" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="gridItems" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Height="150px" SkinID="PrimaryInquire" AdjustPageSize="Auto" AllowPaging="True" Caption="Matching Records" FastFilterFields="OpportunityID,Subject" 
        AutoGenerateColumns="AppendDynamic" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
						TextAlign="Center" Type="CheckBox" Width="40px" />
					<px:PXGridColumn DataField="OpportunityID" LinkCommand="Items_ViewDetails"/>
					<px:PXGridColumn AllowNull="False" DataField="Subject" />
					<px:PXGridColumn AllowNull="False" DataField="Status" />
					<px:PXGridColumn AllowNull="False" DataField="Resolution" />  
					<px:PXGridColumn AllowNull="False" DataField="StageID" />
					<px:PXGridColumn AllowNull="False" DataField="CROpportunityProbability__Probability" TextAlign="Right" /> 
                    <px:PXGridColumn DataField="CRActivityStatistics__LastIncomingActivityDate" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="CRActivityStatistics__LastOutgoingActivityDate" DisplayFormat="g" TimeMode="True" />
					<px:PXGridColumn DataField="LastActivity" /> 
					<px:PXGridColumn DataField="CloseDate" />
					<px:PXGridColumn DataField="CuryID" />
					<px:PXGridColumn AllowNull="False" DataField="CuryProductsAmount" TextAlign="Right" /> 
					<px:PXGridColumn DataField="ClassID" RenderEditorText="True" /> 
					
					<px:PXGridColumn DataField="Source" RenderEditorText="True"/>

					<px:PXGridColumn DataField="BAccount__AcctCD" RenderEditorText="True" LinkCommand="Items_BAccount_ViewDetails"/>
					<px:PXGridColumn DataField="BAccount__AcctName" />
					<px:PXGridColumn DataField="BAccountParent__AcctCD" RenderEditorText="True" LinkCommand="Items_BAccountParent_ViewDetails"/>
					<px:PXGridColumn DataField="BAccountParent__AcctName" />
					
					<px:PXGridColumn DataField="WorkgroupID" />
					<px:PXGridColumn DataField="OwnerID" DisplayMode="Text"/> 
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" SyncVisible="False" SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="CreatedDateTime" />
					<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" SyncVisible="False" SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="LastModifiedDateTime" /> 
				</Columns>
			</px:PXGridLevel>
		</Levels>
        <ActionBar PagerVisible="False"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
