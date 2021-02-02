<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO511000.aspx.cs" Inherits="Page_SO511000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues" TypeName="PX.Objects.SO.GeneratePaymentFromCardTran">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Filter" TabIndex="12400">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
		    <px:PXDateTimeEdit ID="edBeginDate" runat="server" DataField="StartDate" CommitChanges="True">
            </px:PXDateTimeEdit>
            <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" CommitChanges="True">
            </px:PXDateTimeEdit>
		</Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" SkinID="Inquire" TabIndex="1000" AllowPaging="true" AdjustPageSize="Auto">
		<Levels>
			<px:PXGridLevel DataMember="ExternalTransactionList">
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="SODocType" Width="150px" >
					</px:PXGridColumn>
					<px:PXGridColumn DataField="SORefNbr" Width="150px">
					</px:PXGridColumn>
				    <px:PXGridColumn DataField="BAccountID" Width="150px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Descr" Width="300px">
                    </px:PXGridColumn>
					<px:PXGridColumn DataField="TranNumber" Width="170px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Amount">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="PreAuthorized" Type="CheckBox" Width="80px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Captured" Type="CheckBox" Width="80px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="LastActivityDate" Width="100px">
					</px:PXGridColumn>
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar ActionsText="False">
		</ActionBar>
		<Mode AllowAddNew="False" />
	</px:PXGrid>
</asp:Content>
