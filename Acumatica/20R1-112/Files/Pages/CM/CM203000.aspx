<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CM203000.aspx.cs" Inherits="Page_CM203000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CM.TranslationDefinitionMaint" PrimaryView="TranslDefRecords">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" DataMember="TranslDefRecords" Caption="Translation Definition Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True"
		ActivityField="NoteActivity" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="PXSelector1" runat="server" DataField="TranslDefId" />
			<px:PXSelector CommitChanges="True" ID="edSourceLedgerId" runat="server" DataField="SourceLedgerId" />
			<px:PXSelector CommitChanges="True" ID="edDestLedgerId" runat="server" DataField="DestLedgerId"/>
            <px:PXSelector CommitChanges="True" ID="edBranchId" runat="server" DataField="BranchID"/>
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXCheckBox CommitChanges="True" ID="chkActive" runat="server" Checked="True" DataField="Active" />
			<px:PXTextEdit ID="edSourceCuryID" runat="server" DataField="SourceCuryID" Enabled="False" Size="S" />
			<px:PXTextEdit ID="edDestCuryID" runat="server" DataField="DestCuryID" Enabled="False" Size="S" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="150px" Style="z-index: 100" Width="100%" Caption="Translation Definition Details" ActionsPosition="Top" AllowSearch="True" SkinID="Details">
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Levels>
			<px:PXGridLevel DataMember="TranslDefDetailsRecords">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXSegmentMask ID="edAccountIdFrom" runat="server" DataField="AccountIdFrom" />
					<px:PXSegmentMask ID="edSubIdFrom" runat="server" DataField="SubIdFrom" />
					<px:PXSegmentMask ID="edAccountIdTo" runat="server" DataField="AccountIdTo" />
					<px:PXSegmentMask ID="edSubIdTo" runat="server" DataField="SubIdTo" />
					<px:PXDropDown ID="edCalcMode" runat="server" DataField="CalcMode" />
					<px:PXSelector ID="edRateTypeId" runat="server" DataField="RateTypeId" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="AccountIdFrom"  />
					<px:PXGridColumn DataField="AccountIdFrom_Account_description"  />
					<px:PXGridColumn DataField="SubIdFrom"  />
					<px:PXGridColumn DataField="AccountIdTo"  />
					<px:PXGridColumn DataField="AccountIdTo_Account_description" />
					<px:PXGridColumn DataField="SubIdTo" />
					<px:PXGridColumn DataField="CalcMode"  AutoCallBack="True" Type = "DropDownList" />
					<px:PXGridColumn DataField="RateTypeId"  />
					<px:PXGridColumn DataField="TranslDefId" Visible="False" AllowShowHide="False" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<Mode InitNewRow="False" />
		<ActionBar>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
