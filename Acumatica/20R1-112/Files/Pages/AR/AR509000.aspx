<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR509000.aspx.cs"
    Inherits="Page_AR509000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Filter" TypeName="PX.Objects.AR.ARClosingProcess" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server"  DataMember="Filter" Width="100%">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
			<px:PXSegmentMask ID="edOrganizationID" runat="server" DataField="OrganizationID" CommitChanges="True"/>
            <px:PXDropDown ID="edAction" runat="server" DataField="Action" CommitChanges="True"/>
			<px:PXSelector ID="edFromYear" runat="server" DataField="FromYear" CommitChanges="True" AutoRefresh="true"/>
			<px:PXSelector ID="edToYear" runat="server" DataField="ToYear" CommitChanges="True" AutoRefresh="true"/>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" DataSourceID="ds" AllowSearch="true" AllowPaging="true" AdjustPageSize="Auto"
        SkinID="PrimaryInquire" Caption="Financial Periods">
        <Levels>
            <px:PXGridLevel DataMember="FinPeriods">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False"
                        AllowMove="False" Width="30px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FinPeriodID" DisplayFormat="##-####" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Descr" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
