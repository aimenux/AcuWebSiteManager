<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS201010.aspx.cs" Inherits="Page_CS201010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%" PrimaryView="Header" TypeName="PX.Objects.CS.NumberingMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" DataMember="Header" Caption="Numbering Sequence Summary" TemplateContainer=""
         MarkRequired="Dynamic">
		<Parameters>
			<px:PXQueryStringParam Name="Numbering.NumberingID" QueryStringField="NumberingID" Type="String" OnLoadOnly="true" />
		</Parameters>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M"  />
			<px:PXSelector ID="edNumberingID" runat="server" DataField="NumberingID" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
			<px:PXCheckBox CommitChanges="True" ID="chkUserNumbering" runat="server" DataField="UserNumbering" />
			<px:PXMaskEdit ID="edNewSymbol" runat="server" DataField="NewSymbol" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Style="z-index: 100;" Width="100%" Caption="Numbering Sequence Details" SkinID="Details" Height = "300px">
		<Levels>
			<px:PXGridLevel DataMember="Sequence">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
					<px:PXSegmentMask ID="edNBranchID" runat="server" DataField="NBranchID" />
					<px:PXMaskEdit ID="edStartNbr" runat="server" DataField="StartNbr" />
					<px:PXMaskEdit ID="edEndNbr" runat="server" DataField="EndNbr" />
					<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" />
					<px:PXMaskEdit ID="edLastNbr" runat="server" DataField="LastNbr" />
					<px:PXMaskEdit ID="edWarnNbr" runat="server" DataField="WarnNbr" />
					<px:PXNumberEdit ID="edNbrStep" runat="server" DataField="NbrStep" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="NBranchID" AllowShowHide="Server" />
					<px:PXGridColumn DataField="StartNbr" Width="100px" AutoCallBack="true" />
					<px:PXGridColumn DataField="EndNbr" Width="100px" />
					<px:PXGridColumn DataField="StartDate" Width="100px" />
					<px:PXGridColumn DataField="LastNbr" Width="100px" />
					<px:PXGridColumn DataField="WarnNbr" Width="100px" />
					<px:PXGridColumn AllowNull="False" DataField="NbrStep" Width="100px" TextAlign="Right" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<CallbackCommands>
			<Save PostData="Page" />
		</CallbackCommands>
	</px:PXGrid>
</asp:Content>
