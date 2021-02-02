<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="IN506100.aspx.cs" Inherits="Page_IN506100" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.Objects.IN.INUpdateMCAssignment" 
                     PageLoadBehavior="PopulateSavedValues" Visible="True" TabIndex="1" 
                     PrimaryView="UpdateSettings" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds"
		Style="z-index: 100" Width="100%" Caption="Selection" CaptionAlign="Justify" 
		DataMember="UpdateSettings"  DefaultControlID="edSiteID" TabIndex="100">
<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
	    
	    <Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" 
                ControlSize="XM" />

			<px:PXSegmentMask CommitChanges="True" ID="edSiteID" 
                runat="server" DataField="SiteID" AllowEdit="True" DataSourceID="ds" />
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" 
                ControlSize="SM" />

			<px:PXSelector CommitChanges="True" ID="edYear" runat="server" DataField="Year" 
                DataSourceID="ds"  />
			<px:PXNumberEdit CommitChanges="True" ID="edPeriodNbr" runat="server" DataField="PeriodNbr"  />
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" 
                ControlSize="SM" />

			<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" Enabled="False" AllowNull="False" />
			<px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" Enabled="False" AllowNull="False" />
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
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />

					<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID"
						 AllowAddNew="True" AllowEdit="True">
						<GridProperties>
						 <PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
					</px:PXSegmentMask>
					<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr"  />
					<px:PXTextEdit ID="edInventoryID_description" runat="server" DataField="InventoryID_description" Enabled="False"  />
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />

					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />

					<px:PXSelector Size="xs" ID="edOldMC" runat="server" DataField="OldMC" AllowEdit="True" />
					<px:PXCheckBox ID="chkMCFixed" runat="server" DataField="MCFixed" />
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="False" />

					<px:PXSelector ID="edNewMC" runat="server" DataField="NewMC" AllowEdit="True" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" />
					<px:PXGridColumn DataField="Descr" />
					<px:PXGridColumn DataField="OldMC" />
                    <px:PXGridColumn DataField="MCFixed" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="NewMC" />	
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
    </px:PXGrid>
</asp:Content>
