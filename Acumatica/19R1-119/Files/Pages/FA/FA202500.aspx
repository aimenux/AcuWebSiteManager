<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA202500.aspx.cs"
	Inherits="Page_FA202500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.FA.DepreciationMethodMaint" PrimaryView="Method">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Depreciation Method Summary"
		DataMember="Method" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="L" />
			<px:PXSelector runat="server" DataField="MethodCD" ID="edMethodCD" />
			<px:PXDropDown ID="edDepreciationMethod" runat="server" DataField="DepreciationMethod" CommitChanges="True" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" ColumnSpan="2" />
			<px:PXTextEdit runat="server" DataField="Description" ID="edDescription" />
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="true" />
			<px:PXNumberEdit runat="server" DataField="DBMultiplier" ID="edDBMultiplier" />
			<px:PXCheckBox ID="chkSwitchToSL" runat="server" DataField="SwitchToSL" />
			<px:PXLayoutRule ID="PXLayoutRule4" runat="server" />
			<px:PXNumberEdit runat="server" DataField="PercentPerYear" ID="edPercentPerYear" />
			<px:PXDropDown runat="server" DataField="Source" ID="edSource" Enabled="false" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
			<px:PXDropDown CommitChanges="True" runat="server" DataField="AveragingConvention" ID="edAveragingConvention" />
			<px:PXCheckBox ID="chkYearlyAccountancy" runat="server" DataField="YearlyAccountancy" />
		</Template>
		<AutoSize Enabled="true" Container="Window" />
	</px:PXFormView>
</asp:Content>
