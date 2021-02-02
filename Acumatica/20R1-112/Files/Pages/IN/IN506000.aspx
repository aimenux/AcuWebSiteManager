<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="IN506000.aspx.cs" Inherits="Page_IN506000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.Objects.IN.INUpdateABCAssignment" 
                     PageLoadBehavior="PopulateSavedValues" Visible="True" TabIndex="1" 
                     PrimaryView="UpdateSettings" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds"
		Style="z-index: 100" Width="100%" Caption="Selection" CaptionAlign="Justify" 
		TabIndex="2" DataMember="UpdateSettings" DefaultControlID="edSiteID">
	    <Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="XM" />

			<px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID"  />
            <px:PXSelector CommitChanges="True" ID="edPeriodID" runat="server" DataField="FinPeriodID"  />
	    </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="144px" 
		Style="z-index: 100; left: 0px; top: 0px;" Width="100%" AdjustPageSize="Auto" 
		AllowPaging="True" AllowSearch="True" BatchUpdate="True" TabIndex="100" 
		Caption="Details" DataSourceID="ds" SkinID="PrimaryInquire">
		<Levels>
			<px:PXGridLevel  DataMember="ResultPreview">
				<RowTemplate>
					<px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" />
					<px:PXGridColumn DataField="Descr" />
					<px:PXGridColumn DataField="OldABCCode" />
                    <px:PXGridColumn DataField="ABCCodeFixed" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="YtdCost" TextAlign="Right" />
					<px:PXGridColumn DataField="Ratio" TextAlign="Right" />
					<px:PXGridColumn DataField="CumulativeRatio" TextAlign="Right" />
					<px:PXGridColumn DataField="NewABCCode" />	
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
    </px:PXGrid>
</asp:Content>
