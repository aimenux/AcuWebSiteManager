<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR205000.aspx.cs"
	Inherits="Page_PR205000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.PR.PRPayGroupMaint" PrimaryView="PayGroup" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="showCalendar" DependOnGrid="grid" StateColumn="IsPayGroupIDFilled" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Width="100%" Style="z-index: 100; left: 0px; top: 0px; height: 286px;" AllowPaging="True"
		DataSourceID="ds" AdjustPageSize="Auto" AllowSearch="True" SkinID="Primary" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="PayGroup">
				<Mode InitNewRow="True" />
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXMaskEdit ID="edPayGroupID" runat="server" DataField="PayGroupID" InputMask="&gt;CCCCCCCCCC" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXSelector ID="edEarningsAcctID" runat="server" DataField="EarningsAcctID" />
					<px:PXSegmentMask ID="edEarningsSubID" runat="server" DataField="EarningsSubID" SelectMode="Segment" />
					<px:PXSelector ID="edDedLiabilityAcctID" runat="server" DataField="DedLiabilityAcctID" />
					<px:PXSegmentMask ID="edDedLiabilitySubID" runat="server" DataField="DedLiabilitySubID" />
					<px:PXSelector ID="edBenefitExpenseAcctID" runat="server" DataField="BenefitExpenseAcctID" />
					<px:PXSegmentMask ID="edBenefitExpenseSubID" runat="server" DataField="BenefitExpenseSubID" />
					<px:PXSelector ID="edBenefitLiabilityAcctID" runat="server" DataField="BenefitLiabilityAcctID" />
					<px:PXSegmentMask ID="edBenefitLiabilitySubID" runat="server" DataField="BenefitLiabilitySubID" />
					<px:PXSelector ID="edTaxExpenseAcctID" runat="server" DataField="TaxExpenseAcctID" />
					<px:PXSegmentMask ID="edTaxExpenseSubID" runat="server" DataField="TaxExpenseSubID" />
					<px:PXSelector ID="edTaxLiabilityAcctID" runat="server" DataField="TaxLiabilityAcctID" />
					<px:PXSegmentMask ID="edTaxLiabilitySubID" runat="server" DataField="TaxLiabilitySubID" />
					<px:PXCheckBox CommitChanges="True" ID="chkIsDefault" runat="server" DataField="IsDefault" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="PayGroupID" Width="63px" DisplayFormat="&gt;CCCCCCCCCC" CommitChanges="true" />
					<px:PXGridColumn DataField="Description" Width="351px" />
					<px:PXGridColumn DataField="EarningsAcctID" />
					<px:PXGridColumn DataField="EarningsSubID" />
					<px:PXGridColumn DataField="DedLiabilityAcctID" />
					<px:PXGridColumn DataField="DedLiabilitySubID" />
					<px:PXGridColumn DataField="BenefitExpenseAcctID" />
					<px:PXGridColumn DataField="BenefitExpenseSubID" />
					<px:PXGridColumn DataField="BenefitLiabilityAcctID" />
					<px:PXGridColumn DataField="BenefitLiabilitySubID" />
					<px:PXGridColumn DataField="TaxExpenseAcctID" />
					<px:PXGridColumn DataField="TaxExpenseSubID" />
					<px:PXGridColumn DataField="TaxLiabilityAcctID" />
					<px:PXGridColumn DataField="TaxLiabilitySubID" />
					<px:PXGridColumn AllowNull="False" AutoCallBack="True" DataField="IsDefault" Label="Is Default" TextAlign="Center" Type="CheckBox"
						Width="60px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowFormEdit="True" InitNewRow="True" />
		<AutoCallBack />
		<LevelStyles>
			<RowForm Height="150px" Width="270px" />
		</LevelStyles>
		<ActionBar>
			<Actions>
				<NoteShow Enabled="False" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
