<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX207000.aspx.cs" Inherits="Page_TX207000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.TX.TaxYearMaint" PrimaryView="TaxYearFilterSelectView">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="AddPeriod" Visible="false" />
            <px:PXDSCallbackCommand Name="DeletePeriod" Visible="false"/>
            <px:PXDSCallbackCommand Visible="false" Name="ViewTaxPeriodDetails" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="TaxYearFilterSelectView">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask ID="edOrganizationID" runat="server" DataField="OrganizationID" AllowEdit="True" CommitChanges="True"/>
            <px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" AllowEdit="True" CommitChanges="True"/>
            <px:PXLayoutRule runat="server" Merge="True"/>
                <px:PXSelector ID="edYear" runat="server" DataField="Year" Size="S" DataSourceID="ds" CommitChanges="True" AutoRefresh="True"/>
                <px:PXCheckBox ID="chkHold" runat="server" DataField="ShortTaxYear" OnValueChange="Commit" />
			<px:PXLayoutRule runat="server" />
            <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" CommitChanges="True"/>
            <px:PXDropDown ID="edTaxPeriodType" runat="server" DataField="TaxPeriodType" CommitChanges="True"/>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" SkinID="Details" SyncPosition ="True">
		<Levels>
			<px:PXGridLevel DataMember="TaxPeriodExSelectView">
			    <RowTemplate>
			        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />   
                    <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDateUI"/>
                    <px:PXDateTimeEdit ID="edEndDateUI" runat="server" DataField="EndDateUI"/>
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status"/>
                    <px:PXNumberEdit ID="edNetTaxAmt" runat="server" DataField="NetTaxAmt" />
			    </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="TaxPeriodID"/>
                    <px:PXGridColumn DataField="StartDateUI" DisplayFormat="d" />
                    <px:PXGridColumn DataField="EndDateUI" DisplayFormat="d" />
                    <px:PXGridColumn DataField="Status"/>
                    <px:PXGridColumn DataField="NetTaxAmt" LinkCommand="ViewTaxPeriodDetails"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
         <ActionBar PagerVisible="False">
            <Actions>
			   <AddNew ToolBarVisible="False"/> 
               <Delete ToolBarVisible="False"/>
		    </Actions>
			<CustomItems>
				<px:PXToolBarButton ImageKey="RecordAdd" DisplayStyle="Image">
				    <AutoCallBack Command="AddPeriod" Target="ds" />
				</px:PXToolBarButton>
                <px:PXToolBarButton ImageKey="RecordDel" DisplayStyle="Image">
                     <AutoCallBack Command="DeletePeriod" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
