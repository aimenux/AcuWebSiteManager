<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="SP404000.aspx.cs" Inherits="Page_SP404000" 
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server"  Width="100%" AutoCallBack="True" Visible="true"
        PrimaryView="Filter" TypeName="SP.Objects.SP.SPContractInquiry">
        <CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="viewContract" Visible="False" />
		</CallbackCommands>
    </px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" AllowCollapse="False">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM"/> 
			<px:PXDropDown runat="server" ID="PXDropDown1" DataField="Status" CommitChanges="True"/>
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server"/>
		</Template>
	</px:PXFormView>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" ActionsPosition="Top" AllowPaging="true"
		AdjustPageSize="Auto" AllowSearch="true" SkinID="PrimaryInquire" Width="100%" FastFilterFields="ContractCD" 
        SyncPosition="True" NoteIndicator="false" FilesIndicator="false">
   	    <Levels>
			<px:PXGridLevel DataKeyNames="ContractCD" DataMember="FilteredItems">
                <Columns>
                    <px:PXGridColumn DataField="ContractCD" Width="90px" LinkCommand="viewContract"/>
                    <px:PXGridColumn DataField="Status"/>
                    <px:PXGridColumn DataField="Description" Width="300px"/>
                    <px:PXGridColumn DataField="Customer__AcctName" Width="200px"/>
                    <px:PXGridColumn DataField="ContractBillingSchedule__AccountID" Width="200px"/>
                    <px:PXGridColumn DataField="StartDate" Width="90px"/>
                    <px:PXGridColumn DataField="ExpireDate" Width="120px"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar DefaultAction="cmd_ViewDetails" PagerVisible="False">
		    <CustomItems>
                <px:PXToolBarButton Key="cmd_ViewDetails" Visible="False">
                    <ActionBar GroupIndex="0" />
                    <AutoCallBack Command="viewContract" Target="ds"/>
                </px:PXToolBarButton>
            </CustomItems>
		</ActionBar>
	    <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
