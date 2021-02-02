<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN505000.aspx.cs" Inherits="Page_IN505000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INIntegrityCheck" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds"
		Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" 
        DefaultControlID="edSiteID" NoteField="">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="XM" />

			<px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID"  />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />

            <px:PXSelector Size="xs" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" CommitChanges ="true"  />
			<px:PXCheckBox CommitChanges="True" ID="chkRebuildHistory" runat="server" DataField="RebuildHistory" />
		    <px:PXCheckBox ID="chkReplanBackorders" runat="server" DataField="ReplanBackorders" />
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" 
		Style="z-index: 100" Width="100%" ActionsPosition="top" 
	BatchUpdate="true" AllowPaging="true" AdjustPageSize="Auto" TabIndex="100" 
		SkinID="PrimaryInquire" Caption="Details">	
	    <Levels>
			<px:PXGridLevel DataMember="INItemList" >
				<RowTemplate>
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
					<px:PXSegmentMask ID="edInventoryCD" runat="server" DataField="InventoryCD" Enabled="False" AllowEdit="True" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn AllowUpdate="False" DataField="InventoryCD" DisplayFormat="&gt;AAA-&gt;CCC-&gt;AA" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="INSiteStatusSummary__QtyOnHand" NullText="0.0" TextAlign="Right" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="INSiteStatusSummary__QtyAvail" NullText="0.0" TextAlign="Right" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="INSiteStatusSummary__QtyNotAvail" NullText="0.0" TextAlign="Right" />										
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
