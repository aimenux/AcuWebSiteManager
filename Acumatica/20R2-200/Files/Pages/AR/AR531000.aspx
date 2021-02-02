<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR531000.aspx.cs" Inherits="Page_AR531000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Filter" TypeName="PX.Objects.RUTROT.ClaimRUTROT" Visible="true" >
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Process" StartNewGroup="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" DataMember="Filter"
		Width="100%" >
        <Template>			
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"  LabelsWidth="S"  />
			<px:PXDropDown Size = "m" CommitChanges="True" ID="edAction" runat="server" AllowNull="False" DataField="Action" />
            <px:PXGroupBox CommitChanges="True" DataField="RUTROTType" RenderStyle="Simple" ID="edDeductionType" runat="server" >
                <ContentLayout Layout="Stack" Orientation="Horizontal" />
                <Template>
                    <px:PXLabel Text="" runat="server" Width="90px" />
                    <px:PXRadioButton Value="O" ID="edDeductionTypeROT" GroupName="edDeductionType" Text="ROT" runat="server" />
                    <px:PXRadioButton Value="U" ID="edDeductionTypeRUT" GroupName="edDeductionType" Text="RUT" runat="server" />
                </Template>
            </px:PXGroupBox>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Inquire" TabIndex="800" Caption="Documents to claim" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="Documents">
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px" AllowCheckAll="True" AllowSort="False" AllowMove="False" AllowUpdate="False" AutoCallBack="True">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="DocType">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="ViewDocument">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="RUTROTType" Width="100px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CustomerID" Width="120px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="DocDate" Width="90px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="FinPeriodID" Width="90px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="DocDesc" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CuryDistributedAmt" TextAlign="Right" Width="150px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" Width="150px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right" Width="100px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="ClaimDate" Width="90px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="ExportRefNbr" Width="90px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="BalancingCreditMemoRefNbr" LinkCommand="ViewCreditMemo">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="BalancingDebitMemoRefNbr" LinkCommand="ViewDebitMemo">
                    </px:PXGridColumn>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
