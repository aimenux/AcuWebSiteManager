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
					<px:PXGridColumn DataField="OpportunityID" Width="90px" LinkCommand="Items_ViewDetails"/>
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

					<px:PXGridColumn DataField="BAccount__AcctCD" Width="150px" RenderEditorText="True" LinkCommand="Items_BAccount_ViewDetails"/>
					<px:PXGridColumn DataField="BAccount__AcctName" Width="200px"/>
					<px:PXGridColumn DataField="BAccountParent__AcctCD" RenderEditorText="True" Width="150px" LinkCommand="Items_BAccountParent_ViewDetails"/>
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
        <ActionBar PagerVisible="False"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
