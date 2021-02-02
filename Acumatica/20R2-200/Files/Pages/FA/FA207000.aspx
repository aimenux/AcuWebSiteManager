<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="FA207000.aspx.cs" Inherits="Page_FA207000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="DisposalMethods"
		TypeName="PX.Objects.FA.DisposalMethodMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary">
		<Levels>
			<px:PXGridLevel  DataMember="DisposalMethods">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />

					<px:PXMaskEdit ID="edDisposalMethodCD" runat="server" DataField="DisposalMethodCD" InputMask="&gt;CCCCCCCCCC"  />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description"  />
					<px:PXSegmentMask ID="edProceedsAcctID" runat="server" DataField="ProceedsAcctID"  />
					<px:PXSegmentMask ID="edProceedsSubID" runat="server" DataField="ProceedsSubID"  />
                </RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="DisposalMethodID" />
					<px:PXGridColumn DataField="DisposalMethodCD" DisplayFormat="&gt;CCCCCCCCCC" Label="Disposal Method" />
					<px:PXGridColumn DataField="Description" Label="Description" />
					<px:PXGridColumn DataField="ProceedsAcctID" DisplayFormat="&gt;######" Label="Proceeds Account" />
					<px:PXGridColumn DataField="ProceedsSubID" DisplayFormat="&gt;AA-AA-AA-AA-AAA" Label="Proceeds Subaccount" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
