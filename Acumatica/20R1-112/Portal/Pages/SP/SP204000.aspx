<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SP204000.aspx.cs" Inherits="Pages_SP204000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="true" Width="100%"
		PrimaryView="Filter" TypeName="SP.Objects.SP.SPCaseOpenInquiry" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="gridOpen" Name="viewCase" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" AllowCollapse="False">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM"/> 
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" ControlSize="M"/>
			<px:PXSelector runat="server" ID="edcurrentOwnerID" DataField="currentOwnerID" DisplayMode="Text">
				<AutoCallBack Command="Save" Target="form" />
			</px:PXSelector>
			<px:PXCheckBox runat="server" ID="edMyOwner" DataField="MyOwner" CommitChanges="True">
				<AutoCallBack Command="Save" Target="form" />
			</px:PXCheckBox> 
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server"/>
			<px:PXSelector runat="server" ID="edContractID" DataField="ContractID" 
				CommitChanges="True" DisplayMode="Text" TextMode="Search" AllowNull="True">
				<AutoCallBack Command="Save" Target="form" />
			</px:PXSelector> 		    
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server"/>				 
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="gridOpen" runat="server" DataSourceID="ds" ActionsPosition="Top" AllowPaging="true"
		AdjustPageSize="Auto" AllowSearch="true" SkinID="PrimaryInquire" Width="100%" FastFilterFields="CaseCD,Subject" 
        SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="FilteredItems">
			   <Columns>
                    <px:PXGridColumn DataField="CaseCD" Width="70px" LinkCommand="viewCase" />

					<px:PXGridColumn DataField="Subject" Width="450px" />
					<px:PXGridColumn DataField="Status" Width="70px"/>
                    <px:PXGridColumn DataField="Resolution" Width="120px"/>
                    <px:PXGridColumn DataField="ContractID" Width="70px" />                    
                    <px:PXGridColumn DataField="Severity" Width="70px" />
                    <px:PXGridColumn DataField="Priority" Width="70px" />
                    <px:PXGridColumn DataField="CaseClassID" Width="100px" />
                    <px:PXGridColumn DataField="Users__FullName" Width="130px" />
                    <px:PXGridColumn DataField="LastActivity" DisplayFormat="d" Width="130px" />
                    <px:PXGridColumn DataField="ContactID" Width="100px" DisplayMode="Text" />
					<px:PXGridColumn DataField="BAccount__AcctCD" Width="150px" />
					<px:PXGridColumn DataField="BAccount__AcctName" Width="200px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<Mode AllowUpdate="False" AllowAddNew="False"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="cmd_ViewDetails" PagerVisible="False">
			<CustomItems>
				<px:PXToolBarButton Key="cmd_ViewDetails" Visible="False">
					<ActionBar GroupIndex="0" />	  
					<AutoCallBack Command="viewCase" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
