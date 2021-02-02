<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA202600.aspx.cs"
	Inherits="Page_FA202600" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.FA.DepreciationTableMethodMaint" PrimaryView="Method">
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
		DataMember="Method" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" MarkRequired="Dynamic">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector runat="server" DataField="MethodCD" ID="edMethodCD" />
			<px:PXDropDown CommitChanges="True" runat="server" SelectedIndex="1" DataField="RecordType" AllowNull="False" ID="edRecordType" />
			<px:PXSelector runat="server" DataField="ParentMethodID" ID="edParentMethodID" />
			<px:PXNumberEdit CommitChanges="True" ID="edUsefulLife" runat="server" DataField="UsefulLife" AllowNull="True" />
			<px:PXNumberEdit runat="server" DataField="RecoveryPeriod" ID="edRecoveryPeriod" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" ColumnSpan="2" />
			<px:PXTextEdit runat="server" DataField="Description" ID="edDescription" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
			<px:PXDropDown CommitChanges="True" runat="server" DataField="AveragingConvention" ID="edAveragingConvention" />
			<px:PXNumberEdit CommitChanges="True" runat="server" DataField="AveragingConvPeriod" ID="edAveragingConvPeriod" />
			<px:PXNumberEdit ID="edTotalPercents" runat="server" DataField="DisplayTotalPercents" Enabled="False" />
			<px:PXDropDown runat="server" DataField="Source" ID="edSource" Enabled="false" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Details" Caption="Depreciation Method Details"
		AdjustPageSize="Auto" AllowPaging="True">
		<Levels>
			<px:PXGridLevel DataMember="details">
				<Columns>
					<px:PXGridColumn DataField="Year" TextAlign="Right" />
					<px:PXGridColumn DataField="DisplayRatioPerYear" TextAlign="Right" CommitChanges="True" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
