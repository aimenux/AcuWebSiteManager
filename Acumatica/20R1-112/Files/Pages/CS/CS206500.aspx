<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS206500.aspx.cs" Inherits="Page_CS206500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" TypeName="PX.Objects.CS.TermsMaint" runat="server" Visible="True" Width="100%" PrimaryView="TermsDef">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="TermsDef" Caption="Credit Terms" DefaultControlID="edTermsID" DataSourceID="ds" TabIndex="2500">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" GroupCaption="General Settings" StartGroup="True" />
			<px:PXSelector ID="edTermsID" runat="server" DataField="TermsID" AutoRefresh="True" DataSourceID="ds" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
			<px:PXDropDown ID="edVisibleTo" runat="server" DataField="VisibleTo" />
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Due Day Settings" />
			<px:PXDropDown CommitChanges="True" ID="edDueType" runat="server" DataField="DueType" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXNumberEdit ID="edDayDue00" runat="server" DataField="DayDue00" Size="XXS" />
			<px:PXNumberEdit Size="XXS" LabelWidth="71px" ID="edDayFrom00" runat="server" DataField="DayFrom00" />
			<px:PXNumberEdit Size="XXS" LabelWidth="71px" ID="edDayTo00" runat="server" DataField="DayTo00" CommitChanges="true"/>
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXNumberEdit ID="edDayDue01" runat="server" DataField="DayDue01" Size="XXS" CommitChanges="true" />
			<px:PXNumberEdit Size="XXS" LabelWidth="71px" ID="edDayFrom01" runat="server" DataField="DayFrom01" CommitChanges="true"/>
			<px:PXNumberEdit Size="XXS" LabelWidth="71px" ID="edDayTo01" runat="server" DataField="DayTo01" />
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Cash Discount Settings" />
			<px:PXDropDown CommitChanges="True" ID="edDiscType" runat="server" DataField="DiscType" />
			<px:PXNumberEdit Size="XXS" ID="edDayDisc" runat="server" DataField="DayDisc" CommitChanges="True"/>
			<px:PXNumberEdit ID="edDiscPercent" runat="server" DataField="DiscPercent" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Installments Settings" StartGroup="True" />
			<px:PXDropDown CommitChanges="True" ID="edInstallmentType" runat="server" DataField="InstallmentType" />
			<px:PXNumberEdit ID="edInstallmentCntr" runat="server" DataField="InstallmentCntr" Size="XXS" />
			<px:PXDropDown ID="edInstallmentFreq" runat="server" DataField="InstallmentFreq" />
			<px:PXDropDown CommitChanges="True" ID="edInstallmentMthd" runat="server" DataField="InstallmentMthd" />
			<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Caption="Installments Schedule" AllowSearch="True" SkinID="ShortList" Height="250px" Width="400px" TabIndex="2700">
				<Levels>
					<px:PXGridLevel DataMember="Installments">
						<Columns>
							<px:PXGridColumn AllowNull="False" DataField="InstDays" TextAlign="Right" Width="70px" />
							<px:PXGridColumn DataField="InstPercent" TextAlign="Right" Width="100px" />
						</Columns>
						<RowTemplate>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
							<px:PXNumberEdit ID="edInstDays" runat="server" DataField="InstDays" />
							<px:PXNumberEdit ID="edInstPercent" runat="server" DataField="InstPercent" />
						</RowTemplate>
						<Layout FormViewHeight="" />
					</px:PXGridLevel>
				</Levels>
				<Mode AutoInsert="True" InitNewRow="True" />
			</px:PXGrid>
		</Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="375" />
	</px:PXFormView>
</asp:Content>
