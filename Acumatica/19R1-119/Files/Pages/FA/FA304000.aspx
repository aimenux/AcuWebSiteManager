<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA304000.aspx.cs" Inherits="Page_FA304000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="BookYear" TypeName="PX.Objects.FA.FABookPeriodsMaint" >
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="BookYear" TabIndex="1700">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" StartColumn="True"/>
			<px:PXSelector ID="edBookID" runat="server" DataField="BookID" CommitChanges="True">
			</px:PXSelector>
			<px:PXSegmentMask ID="edOrganizationID" runat="server" DataField="OrganizationID" CommitChanges="True">
			</px:PXSegmentMask>
			<px:PXSelector ID="edYear" runat="server" DataField="Year" AutoRefresh="True" CommitChanges="True">
			</px:PXSelector>
			<px:PXLayoutRule runat="server" StartColumn="True">
			</px:PXLayoutRule>
			<px:PXDateTimeEdit ID="edStartDate" runat="server" AlreadyLocalized="False" DataField="StartDate">
			</px:PXDateTimeEdit>
			<px:PXNumberEdit ID="edFinPeriods" runat="server" AlreadyLocalized="False" DataField="FinPeriods">
			</px:PXNumberEdit>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" SkinID="Inquire" TabIndex="1900">
		<Levels>
			<px:PXGridLevel DataKeyNames="BookID,OrganizationID,FinPeriodID" DataMember="BookPeriod">
				<Columns>
					<px:PXGridColumn DataField="FinPeriodID">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="StartDateUI">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="EndDateUI">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Descr">
					</px:PXGridColumn>
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
