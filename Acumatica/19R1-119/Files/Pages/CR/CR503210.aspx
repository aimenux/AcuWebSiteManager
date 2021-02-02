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
					<px:PXGridColumn DataField="CaseCD" DisplayFormat="&gt;aaaaaaaaaa" Width="90px" LinkCommand="Items_ViewDetails"/>
					<px:PXGridColumn DataField="Subject" Width="300px"/>
					<px:PXGridColumn AllowNull="False" DataField="Status" Width="90px"/>
					<px:PXGridColumn AllowNull="False" DataField="Resolution" Width="90px"/>
					<px:PXGridColumn AllowNull="False" DataField="Severity" Width="90px"/>
					<px:PXGridColumn AllowNull="False" DataField="Priority" Width="90px"/>
					<px:PXGridColumn DataField="ETA" Width="90px"/>
					<px:PXGridColumn DataField="TimeEstimated" Width="90px"/> 	
					<px:PXGridColumn DataField="RemaininingDate" Width="90px"/>					
                    <px:PXGridColumn DataField="Age" Width="120px"/>
                    <px:PXGridColumn DataField="CRActivityStatistics__LastIncomingActivityDate" Width="120px" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="CRActivityStatistics__LastOutgoingActivityDate" Width="120px" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="LastActivity" Width="120px" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="LastActivityAge" Width="120px"/>
                    <px:PXGridColumn DataField="LastModified" Width="120px" DisplayFormat="g" TimeMode="True" />
					<px:PXGridColumn DataField="CaseClassID" DisplayFormat="&gt;aaaaaaaaaa" Width="80px"/>	 
					<px:PXGridColumn DataField="BAccount__AcctCD" Width="150px" LinkCommand="Items_BAccount_ViewDetails"/>
					<px:PXGridColumn DataField="BAccount__AcctName" Width="200px" />  
					<px:PXGridColumn DataField="BAccountParent__AcctCD" RenderEditorText="True" Width="150px" LinkCommand="Items_BAccountParent_ViewDetails"/>
					<px:PXGridColumn DataField="BAccountParent__AcctName" Width="200px"/>
					<px:PXGridColumn DataField="Contact__DisplayName" Width="150px" LinkCommand="Items_Contact_ViewDetails"/>                    
					<px:PXGridColumn DataField="Location__LocationCD" Width="90px" LinkCommand="Items_Location_ViewDetails"/>	
					<px:PXGridColumn DataField="Contract__ContractCD" Width="90px" LinkCommand="Items_Contract_ViewDetails"/> 
					<px:PXGridColumn DataField="Contract__Description" Width="90px"/> 
                    <px:PXGridColumn DataField="Contract__CustomerID" Width="150px" LinkCommand="Items_Contract_CustomerID_ViewDetails"/>
                    <px:PXGridColumn DataField="BAccountContract__AcctName" Width="150px"/>                    
					<px:PXGridColumn DataField="InitResponse" Width="90px"/>  
					<px:PXGridColumn DataField="TimeResolution" Width="90px"/>  
					<px:PXGridColumn DataField="TimeSpent" Width="90px"/> 
					<px:PXGridColumn DataField="OvertimeSpent" Width="90px"/>  
					<px:PXGridColumn DataField="TimeBillable" Width="90px"/>  
					<px:PXGridColumn DataField="OvertimeBillable" Width="90px"/> 
					<px:PXGridColumn DataField="WorkgroupID" Width="110px" />
					<px:PXGridColumn DataField="OwnerID" Width="110px" DisplayMode="Text"/>
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="110px" /> 
					<px:PXGridColumn DataField="CreatedDateTime" Width="90px"  DisplayFormat="g" TimeMode="True"/> 
					<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Width="110px" SyncVisible="False" SyncVisibility="False" Visible="False"/>	  
					<px:PXGridColumn DataField="LastModifiedDateTime" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
        <ActionBar PagerVisible="False"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
