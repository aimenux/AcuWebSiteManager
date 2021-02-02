<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR104000.aspx.cs" Inherits="Page_PR104000" Title="Overtime Rules" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="OvertimeRules" TypeName="PX.Objects.PR.PROvertimeRuleMaint" Visible="True" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary">
		<Levels>
			<px:PXGridLevel DataMember="OvertimeRules">
                <RowTemplate>
                    <px:PXSelector ID="edDisbursingTypeCD" runat="server" DataField="DisbursingTypeCD" />
                    <px:PXNumberEdit ID="edOvertimeThreshold" runat="server" DataField="OvertimeThreshold" />
                    <px:PXSelector ID="edState" runat="server" DataField="State" />
                    <px:PXSelector ID="edUnionID" runat="server" DataField="UnionID" />
                    <px:PXSelector ID="edProjectID" runat="server" DataField="ProjectID" />
                </RowTemplate>
			    <Columns>
			        <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" CommitChanges="True" Width="60px" />
                    <px:PXGridColumn DataField="OvertimeRuleID" CommitChanges="True" Width="170px" />
			        <px:PXGridColumn DataField="Description" CommitChanges="True" Width="240px" />
                    <px:PXGridColumn DataField="DisbursingTypeCD" CommitChanges="True" Width="100px" />
                    <px:PXGridColumn DataField="OvertimeMultiplier" TextAlign="Right" Width="70px" />
                    <px:PXGridColumn DataField="RuleType" CommitChanges="True"  Width="70px" />
                    <px:PXGridColumn DataField="WeekDay" CommitChanges="True" Width="90px" />
                    <px:PXGridColumn DataField="OvertimeThreshold" CommitChanges="True" Width="100px" />
                    <px:PXGridColumn DataField="State" CommitChanges="True" Width="53px" />
                    <px:PXGridColumn DataField="UnionID" CommitChanges="True" Width="150px" />
                    <px:PXGridColumn DataField="ProjectID" CommitChanges="True" Width="150px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>