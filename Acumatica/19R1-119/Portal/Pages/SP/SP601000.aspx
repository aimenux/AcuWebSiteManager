<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SP601000.aspx.cs" Inherits="Page_SP601000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="SP.Objects.SP.SPCaseStatusInquiry"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="viewCase" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="PXFormView1" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" AllowCollapse="False">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM"/> 
			<px:PXDropDown runat="server" ID="PXDropDown1" DataField="Status" CommitChanges="True"/>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" 
		Width="100%" ActionsPosition="Top" Caption="Cases" AllowPaging="true"
		AdjustPageSize="auto" SkinID="Inquire" FastFilterFields="CaseCD,Subject">
		<Levels>
			<px:PXGridLevel DataMember="FilteredItems">
				<Columns>
					<px:PXGridColumn DataField="CaseCD" Width="90px" LinkCommand="viewCase"/>
                    <px:PXGridColumn AllowNull="False" DataField="Subject" Width="300px"/>
					<px:PXGridColumn AllowNull="False" DataField="Status" Width="90px" />
					<px:PXGridColumn AllowNull="False" DataField="Resolution" Width="90px" />  
					<px:PXGridColumn AllowNull="False" DataField="ContractID" Width="90px" />
					<px:PXGridColumn DataField="LastActivity" Width="90px" /> 
					<px:PXGridColumn DataField="CustomerID" Width="150px"/>
                    <px:PXGridColumn DataField="OwnerID" Width="110px"  DisplayMode="Text"/> 
                    <px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="110px" Visible="False" />
					<px:PXGridColumn DataField="CreatedDateTime" Width="90px" />
					<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Width="110px" Visible="False" />
					<px:PXGridColumn DataField="LastModifiedDateTime" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="cmd_ViewDetails" PagerVisible="False">
			<CustomItems>
				<px:PXToolBarButton Key="cmd_ViewDetails" Visible="False">
					<ActionBar GroupIndex="0" />
					<AutoCallBack Command="viewCase" Target="ds"/>
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar> 
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<CallbackCommands>
			<Refresh CommitChanges="True" PostData="Page" />
		</CallbackCommands>
	</px:PXGrid>
</asp:Content>