<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="PM504000.aspx.cs" Inherits="Page_PM504000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.PM.ProjectBalanceValidation"
        PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues" Visible="true" >
		 <CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewProject" Visible="False"/>
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds"
		Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Parameters" 
        DefaultControlID="edSiteID" NoteField="">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" />
            <px:PXCheckBox ID="chkRecalculateUnbilledSummary" runat="server" DataField="RecalculateUnbilledSummary" CommitChanges="true" AlignLeft="True" />
            <px:PXCheckBox ID="chkRebuildCommitments" runat="server" DataField="RebuildCommitments" CommitChanges="true" AlignLeft="True" />
             <px:PXCheckBox ID="chkRecalculateDraftInvoicesAmount" runat="server" DataField="RecalculateDraftInvoicesAmount" CommitChanges="true" AlignLeft="True" />
              <px:PXCheckBox ID="chkRecalculateChangeOrders" runat="server" DataField="RecalculateChangeOrders" CommitChanges="true" AlignLeft="True" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
        AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SyncPosition="True"
        SkinID="PrimaryInquire" Caption="Projects" FastFilterFields="ContractCD,Description,CustomerID">
        <Levels>
            <px:PXGridLevel DataKeyNames="ContractCD" DataMember="Items">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="true" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="ContractCD" LinkCommand="ViewProject"/>
                    <px:PXGridColumn DataField="Description" Label="Description" />
                    <px:PXGridColumn DataField="CustomerID" Label="Customer" />
                    <px:PXGridColumn DataField="Status" Label="Status" RenderEditorText="True" />
                    <px:PXGridColumn DataField="StartDate" Label="Start Date" />
                    <px:PXGridColumn DataField="ExpireDate" Label="End Date" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar PagerVisible="False" />
    </px:PXGrid>
</asp:Content>

