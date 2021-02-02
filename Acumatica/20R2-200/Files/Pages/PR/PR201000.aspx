<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="PR201000.aspx.cs" Inherits="Page_PR201000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%"
		TypeName="PX.Objects.PR.PRPayPeriodMaint" PrimaryView="PayrollYear" PageLoadBehavior="GoLastRecord">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" Visible="true" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="AskAutoFill" />
			<px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
			<px:PXDSCallbackCommand Name="AutoFill" Visible="False" />
			<px:PXDSCallbackCommand Name="Delete" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%"
		DataMember="PayrollYear" Caption="Payroll Year" NoteIndicator="True" FilesIndicator="True"
		ActivityIndicator="True" ActivityField="NoteActivity" DefaultControlID="edPayGroupID">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
			<px:PXSelector ID="edPayGroupID" runat="server" DataField="PayGroupID" AutoRefresh="True" DataSourceID="ds">
				<GridProperties>
					<Layout ColumnsMenu="False" />
				</GridProperties>
				<AutoCallBack Command="Cancel" Target="ds" />
			</px:PXSelector>
			<px:PXSelector ID="edYear" runat="server" DataField="Year">
				<AutoCallBack Command="Cancel" Target="ds">
				</AutoCallBack>
			</px:PXSelector>
			<px:PXButton ID="pbGenerate" runat="server" CommandName="AskAutoFill" CommandSourceID="ds"
				Visible="False" Text="pbGenerate">
			</px:PXButton>
			<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" Enabled="False" />
			<px:PXNumberEdit ID="edFinPeriods" runat="server" DataField="FinPeriods" Enabled="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="150px"
		Width="100%" Caption="Financial Year Periods" SkinID="Details">
		<Levels>
			<px:PXGridLevel DataMember="Periods">
				<Columns>
					<px:PXGridColumn DataField="PeriodNbr" />
					<px:PXGridColumn DataField="FinPeriodID" Width="120px" />
					<px:PXGridColumn DataField="StartDate" Width="120px" />
					<px:PXGridColumn DataField="EndDateUI" Width="120px" AutoCallBack="true" />
					<px:PXGridColumn DataField="TransactionDate" Width="120px" CommitChanges="true" />
					<px:PXGridColumn DataField="Descr" Width="300px" />
					<px:PXGridColumn DataField="FinYear" />
				</Columns>
				<RowTemplate>
					<px:PXDateTimeEdit ID="EndDateUI" runat="server" DataField="EndDateUI" CommitChanges="true">
						<AutoCallBack Enabled="true" ActiveBehavior="true">
							<Behavior RepaintControlsIDs="grid" CommitChanges="true" RepaintControls="All" />
						</AutoCallBack>
					</px:PXDateTimeEdit>
				</RowTemplate>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode InitNewRow="True" AutoInsert="True" />
		<ActionBar>
			<Actions>
				<AddNew Enabled="true" />
				<Delete Enabled="true" />
				<NoteShow Enabled="False" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
	<px:PXSmartPanel runat="server" ID="pnlAutoFill" Caption="Create Periods" CaptionVisible="true" LoadOnDemand="true"
		Key="PeriodCreation" Width="600px" Height="200px" ShowAfterLoad="True" AutoCallBack-Target="formCreatePeriods" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
		CallBackMode-PostData="Page" AllowResize="True">
		<px:PXFormView ID="formCreatePeriods" runat="server" DataMember="PeriodCreation" DataSourceID="ds" Width="100%" SkinID="Transparent" Height="100%">
			<Template>
				<px:PXLayoutRule runat="server" StartRow="True" ControlSize="XL" LabelsWidth="M" />
					<px:PXCheckBox runat="server" ID="chkUseExceptions" DataField="UseExceptions" CommitChanges="true" AlignLeft="true" />
					<px:PXDropDown runat="server" ID="edExceptionDateBehavior" DataField="ExceptionDateBehavior" CommitChanges="true" />
 			</Template>
			<AutoSize Enabled="true" />
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="PeriodCreationOK" runat="server" DialogResult="Yes" Text="Generate" />
			<px:PXButton ID="PeriodCreationCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
