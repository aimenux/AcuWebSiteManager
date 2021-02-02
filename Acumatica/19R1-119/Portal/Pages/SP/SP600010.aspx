<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" 
CodeFile="SP600010.aspx.cs" Inherits="Page_SP600010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="CasesTotal" TypeName="SP.Objects.SP.SPTotalCaseEnq">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Width="100%"  AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" 
        DataSourceID="ds" BatchUpdate="True" SkinID="PrimaryInquire" SyncPosition="True">
	    <Levels> 
		    <px:PXGridLevel DataMember="CasesTotal">
			    <RowTemplate>
				    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			    </RowTemplate>
			    <Columns>
				    <px:PXGridColumn DataField="AggregationStatus" Width="200px" LinkCommand="ViewStatus"/>
				    <px:PXGridColumn DataField="Count" Width="100px" />
			    </Columns>
		    </px:PXGridLevel>
	    </Levels>
	    <ActionBar DefaultAction="ViewStatus"/>
	    <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>