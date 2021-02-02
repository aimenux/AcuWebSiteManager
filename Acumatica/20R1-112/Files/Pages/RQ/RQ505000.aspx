<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="RQ505000.aspx.cs" Inherits="Page_RQ505000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.Objects.RQ.RQRequisitionProcess"
                     PrimaryView="Filter" BorderStyle="NotSet" Width="100%" PageLoadBehavior="PopulateSavedValues"/>
		
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edAction">
<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />			
            <px:PXDropDown ID="edAction" runat="server" DataField="Action" SelectedIndex="-1" CommitChanges="true"/>			
		    <px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSelector Size = "m" CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID"  />			
            <px:PXCheckBox CommitChanges="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />				
			<px:PXLayoutRule runat="server" Merge="False" />			
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSelector CommitChanges="True" Size="m" ID="edWorkGroupID" runat="server" DataField="WorkGroupID"  />			
			<px:PXCheckBox CommitChanges="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />			
            <px:PXLayoutRule runat="server" Merge="False" />
			<px:PXCheckBox CommitChanges="True" ID="chkMyEscalated" runat="server" DataField="MyEscalated" />			

			<px:PXLayoutRule runat="server" ColumnSpan="2" />

			<px:PXTextEdit CommitChanges="True" ID="edDescription" runat="server" DataField="Description"  />
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />

			<px:PXDropDown CommitChanges="True" ID="edSelectedPriority" runat="server" AllowNull="False" DataField="SelectedPriority"  />
			<px:PXSegmentMask CommitChanges="True" ID="edEmployeeID" runat="server" DataField="EmployeeID" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" ActionsPosition="Top" Caption="Requisitions" SkinID="PrimaryInquire" SyncPosition="True" FastFilterFields="ReqNbr,Description,EmployeeID,VendorID,VendorRefNbr">
		<Levels>
			<px:PXGridLevel DataMember="Records" >
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXSelector Size="xxs" ID="edCuryID" runat="server" DataField="CuryID"  />
					<px:PXSelector Size="s" ID="edReqNbr" runat="server" AllowEdit="True" DataField="ReqNbr" Enabled="False"  />
					<px:PXLayoutRule runat="server" Merge="False" />
					<px:PXDateTimeEdit ID="edOrderDate" runat="server" DataField="OrderDate" Enabled="False"  />
					<px:PXDropDown ID="edPriority" runat="server" AllowNull="False" DataField="Priority" Enabled="False" SelectedIndex="1"  />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False"  />
					<px:PXSegmentMask ID="edEmployeeID" runat="server" DataField="EmployeeID" Enabled="False" />
					<px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status" Enabled="False"  /></RowTemplate>
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="true" />
					<px:PXGridColumn AllowUpdate="False" DataField="Priority" AllowNull="False" RenderEditorText="True" />
					<px:PXGridColumn DataField="ReqNbr" AllowUpdate="False"  />
					<px:PXGridColumn DataField="OrderDate" AllowUpdate="False" />					
					<px:PXGridColumn AllowUpdate="False" DataField="Status" AllowNull="False" RenderEditorText="True" />
					<px:PXGridColumn AllowUpdate="False" DataField="Description" />
					<px:PXGridColumn DataField="EmployeeID" AllowUpdate="False" DisplayFormat="&gt;AAAAAAAAAA" />
					<px:PXGridColumn DataField="VendorID" AllowUpdate="False" DisplayFormat="&gt;AAAAAAAAAA" />
					<px:PXGridColumn DataField="VendorLocationID" AllowUpdate="False" DisplayFormat="&gt;AAAAAA" />
					<px:PXGridColumn AllowUpdate="False" DataField="VendorRefNbr"  />
					<px:PXGridColumn AllowUpdate="False" DataField="CuryID" DisplayFormat="&gt;LLLLL"  />
					<px:PXGridColumn AllowUpdate="False" DataField="CuryEstExtCostTotal" AllowNull="False" TextAlign="Right" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
	    <ActionBar  DefaultAction="Details"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
