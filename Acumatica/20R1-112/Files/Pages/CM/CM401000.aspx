<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CM401000.aspx.cs" Inherits="Page_CM401000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.CM.TranslationEnq"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSelector CommitChanges="True" ID="edTranslDefId" runat="server" DataField="TranslDefId" DataSourceID="ds" />
			<px:PXCheckBox CommitChanges="True" ID="chkUnreleased" runat="server" Checked="True" DataField="Unreleased" />
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" DataSourceID="ds" />
			<px:PXCheckBox CommitChanges="True" ID="chkReleased" runat="server" Checked="True" DataField="Released" />
			<px:PXLayoutRule runat="server" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top" Caption="Translation List" SkinID="PrimaryInquire"
        SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="TranslationHistoryRecords">
				<Columns>
					<px:PXGridColumn DataField="ReferenceNbr" LinkCommand="ViewDetails" />
					<px:PXGridColumn DataField="Status" />
					<px:PXGridColumn DataField="Description" />
					<px:PXGridColumn DataField="DateEntered" />
					<px:PXGridColumn DataField="TranslDefId" />
                    <px:PXGridColumn DataField="BranchID" />
					<px:PXGridColumn DataField="LedgerID" />
					<px:PXGridColumn DataField="FinPeriodID" />
					<px:PXGridColumn DataField="CuryEffDate" />
					<px:PXGridColumn DataField="BatchNbr"  LinkCommand="viewTranslatedBatch" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	    <ActionBar DefaultAction="ViewDetails"/>
			
		
	</px:PXGrid>
</asp:Content>
