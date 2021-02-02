<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" 
CodeFile="IP501000.aspx.cs" Inherits="Page_IP501000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Setup" TypeName="PX.ObjectsExt.IP.CRTfsItemSyncProcess">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="PXGrid1" runat="server" Height="50px" Width="100%" Style="z-index: 100;" AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" 
            DataSourceID="ds" BatchUpdate="True" SkinID="Inquire" Caption="CR Setup">
		    <Levels>
			    <px:PXGridLevel DataMember="Setup">
				    <RowTemplate>
					    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
				    </RowTemplate>
				    <Columns>
					    <px:PXGridColumn DataField="TfsUrl" Width="100px" />
					    <px:PXGridColumn DataField="LastSyncTime" Width="100px" />
				    </Columns>
			    </px:PXGridLevel>
		    </Levels>
	    </px:PXGrid>
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" 
        DataSourceID="ds" BatchUpdate="True" SkinID="Inquire" Caption="TFS Items">
		<Levels>
			<px:PXGridLevel DataMember="TfsItemList">
				<RowTemplate>
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="TfsID" Width="100px" />
					<px:PXGridColumn DataField="Title" Width="200px" />
					<px:PXGridColumn DataField="State" Width="100px" />
					<px:PXGridColumn DataField="Reason" Width="100px" />
					<px:PXGridColumn DataField="FixedIn" Width="100px" />
					
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		
		<Layout ShowRowStatus="False" />
	</px:PXGrid>
</asp:Content>