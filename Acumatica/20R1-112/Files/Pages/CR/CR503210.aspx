<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR503210.aspx.cs" Inherits="Page_CR503210"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" AutoCallBack="True" Visible="True"
		IsDefaultDatasourceWidth="100%" TypeName="PX.Objects.CR.AssignCaseMassProcess" PrimaryView="Items"
		PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_BAccount_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_BAccountParent_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_Contact_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_Location_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_Contract_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_Contract_CustomerID_ViewDetails" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="gridItems" runat="server" DataSourceID="ds" Width="100%" Height="150px"
		SkinID="PrimaryInquire" AdjustPageSize="Auto" AllowPaging="True" Caption="Matching Records"  AutoGenerateColumns="AppendDynamic" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
						TextAlign="Center" Type="CheckBox" Width="40px" />
					<px:PXGridColumn DataField="CaseCD" DisplayFormat="&gt;aaaaaaaaaa" LinkCommand="Items_ViewDetails"/>
					<px:PXGridColumn DataField="Subject" />
					<px:PXGridColumn AllowNull="False" DataField="Status" />
					<px:PXGridColumn AllowNull="False" DataField="Resolution" />
					<px:PXGridColumn AllowNull="False" DataField="Severity" />
					<px:PXGridColumn AllowNull="False" DataField="Priority" />
					<px:PXGridColumn DataField="ETA" />
					<px:PXGridColumn DataField="TimeEstimated" />
					<px:PXGridColumn DataField="RemaininingDate" />
                    <px:PXGridColumn DataField="Age" />
                    <px:PXGridColumn DataField="CRActivityStatistics__LastIncomingActivityDate" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="CRActivityStatistics__LastOutgoingActivityDate" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="LastActivity" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="LastActivityAge" />
                    <px:PXGridColumn DataField="LastModified" DisplayFormat="g" TimeMode="True" />
					<px:PXGridColumn DataField="CaseClassID" DisplayFormat="&gt;aaaaaaaaaa" />
					<px:PXGridColumn DataField="BAccount__AcctCD" LinkCommand="Items_BAccount_ViewDetails"/>
					<px:PXGridColumn DataField="BAccount__AcctName" />
					<px:PXGridColumn DataField="BAccountParent__AcctCD" RenderEditorText="True" LinkCommand="Items_BAccountParent_ViewDetails"/>
					<px:PXGridColumn DataField="BAccountParent__AcctName" />
					<px:PXGridColumn DataField="Contact__DisplayName" LinkCommand="Items_Contact_ViewDetails"/>
					<px:PXGridColumn DataField="Location__LocationCD" LinkCommand="Items_Location_ViewDetails"/>
					<px:PXGridColumn DataField="Contract__ContractCD" LinkCommand="Items_Contract_ViewDetails"/>
					<px:PXGridColumn DataField="Contract__Description" />
                    <px:PXGridColumn DataField="Contract__CustomerID" LinkCommand="Items_Contract_CustomerID_ViewDetails"/>
                    <px:PXGridColumn DataField="BAccountContract__AcctName" />
					<px:PXGridColumn DataField="InitResponse" />
					<px:PXGridColumn DataField="TimeResolution" />
					<px:PXGridColumn DataField="TimeSpent" />
					<px:PXGridColumn DataField="OvertimeSpent" />
					<px:PXGridColumn DataField="TimeBillable" />
					<px:PXGridColumn DataField="OvertimeBillable" />
					<px:PXGridColumn DataField="WorkgroupID" />
					<px:PXGridColumn DataField="OwnerID" DisplayMode="Text"/>
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" />
					<px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" TimeMode="True"/>
					<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" SyncVisible="False" SyncVisibility="False" Visible="False"/>
					<px:PXGridColumn DataField="LastModifiedDateTime" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
        <ActionBar PagerVisible="False"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
