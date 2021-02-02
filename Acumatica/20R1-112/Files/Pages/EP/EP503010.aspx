<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP503010.aspx.cs"
    Inherits="Page_EP503010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <script type="text/javascript">
		function refreshTasksAndEvents(sender, args)
		{
			var top = window.top;
			if (top != window && top.MainFrame != null) top.MainFrame.refreshEventsInfo();
		}
	</script>
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.EPApprovalProcess" PrimaryView="Records">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont4" ContentPlaceHolderID="phL" runat="server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top"
        Caption="Claims" OnRowDataBound="grid_RowDataBound" SkinID="PrimaryInquire" SyncPosition="True">
        <ClientEvents AfterRefresh="refreshTasksAndEvents" />       
        <Levels>            
            <px:PXGridLevel DataMember="Records" >
                <RowTemplate>                    
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="Escalated" TextAlign="Center" Type="CheckBox" Visible="False" AllowShowHide="True" SyncVisible="false"/>
                    <px:PXGridColumn DataField="DocType" Width = "100px" />
                    <px:PXGridColumn DataField="RefNoteID" Width = "100px" />
                    <px:PXGridColumn DataField="DocumentOwnerID"  DisplayMode="Text"/>
                    <px:PXGridColumn DataField="DocDate" />
                    <px:PXGridColumn DataField="BAccountID"/>
                    <px:PXGridColumn DataField="BAccountID_BAccount_acctName" />
                    <px:PXGridColumn DataField="Descr" />
                    <px:PXGridColumn DataField="Details" />
                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" />
                    <px:PXGridColumn DataField="CuryID"/>
                    <px:PXGridColumn DataField="CuryTotalAmount" TextAlign="Right" MatrixMode="true" />
                    <px:PXGridColumn DataField="WorkgroupID" />
                    <px:PXGridColumn DataField="OwnerID"  DisplayMode="Text"/>
	                <px:PXGridColumn DataField="ApprovalID" Visible="False" SyncVisible="False" SyncVisibility="False" />
					<px:PXGridColumn DataField="AssignmentMapID" Visible="False" SyncVisible="False" SyncVisibility="False"/>
                    <px:PXGridColumn DataField="RuleID" Visible="False" SyncVisible="False" SyncVisibility="False"/>
                    <px:PXGridColumn DataField="Reason" Visible="False" SyncVisible="False" SyncVisibility="False"/>
                </Columns>
                <Layout FormViewHeight=""></Layout>
            </px:PXGridLevel>
        </Levels>
        <ActionBar DefaultAction="EditDetail"/>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowUpdate="False" />
    </px:PXGrid>
</asp:Content>
