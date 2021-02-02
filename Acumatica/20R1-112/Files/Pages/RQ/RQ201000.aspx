<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="RQ201000.aspx.cs" Inherits="Page_PO201000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Classes"
		TypeName="PX.Objects.RQ.RQRequestClassMaint" BorderStyle="NotSet">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%"  DataMember="Classes" DefaultControlID="edReqClassID"
		Caption="Request Class"
		 NoteIndicator="True" 
		 FilesIndicator="True" 
		ActivityIndicator="true" ActivityField="NoteActivity" >
		
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
			<px:PXSelector ID="edReqClassID" runat="server" DataField="ReqClassID" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr"  />
			<px:PXNumberEdit ID="edPromisedLeadTime" runat="server" DataField="PromisedLeadTime"  />

            <px:PXLayoutRule runat="server" StartColumn="True" StartRow="true" LabelsWidth="M" ControlSize="M"/>
			<px:PXCheckBox CommitChanges="True" ID="chkCustomerRequest" runat="server" DataField="CustomerRequest" />		    			
			<px:PXCheckBox CommitChanges="True" ID="chkVendorNotRequest" runat="server" DataField="VendorNotRequest" />
			<px:PXCheckBox ID="chkVendorMultiply" runat="server" DataField="VendorMultiply" />
		    
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S"/>
            <px:PXCheckBox ID="chkIssueRequestor" runat="server" DataField="IssueRequestor" AlignLeft="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkRestrictItemList" runat="server" DataField="RestrictItemList" AlignLeft="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkHideInventoryID" runat="server" DataField="HideInventoryID" AlignLeft="True" />
         </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab runat="server" ID="tab" DataMember="CurrentClass" 
		Width="100%" Height="400px" DataSourceID="ds">
<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="Request Class Item List">
				<Template>				
					<px:PXGrid runat="server" ID="grid" DataSourceID="ds" SkinID="DetailsInTab" 
						Width="100%">						
						<Levels>
							<px:PXGridLevel DataMember="ClassItems" >
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="M" />

									<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
									<px:PXTextEdit ID="edRQInventoryItem__Descr" runat="server" DataField="RQInventoryItem__Descr"  />
									<px:PXSegmentMask ID="edRQInventoryItem__ItemClassID" runat="server" DataField="RQInventoryItem__ItemClassID" AllowEdit="True" />
									<px:PXDropDown ID="edRQInventoryItem__ItemStatus" runat="server" AllowNull="False" DataField="RQInventoryItem__ItemStatus"  />
									<px:PXDropDown ID="edRQInventoryItem__ItemType" runat="server" DataField="RQInventoryItem__ItemType"  /></RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;AAAAAAAAAAAAAAAAAAAAAAAAA" Width="225px" AutoCallBack="true" />
									<px:PXGridColumn DataField="RQInventoryItem__Descr" Width="200px" />
									<px:PXGridColumn DataField="RQInventoryItem__ItemClassID" />
									<px:PXGridColumn AllowNull="False" DataField="RQInventoryItem__ItemStatus" RenderEditorText="True" />
									<px:PXGridColumn DataField="RQInventoryItem__ItemType" RenderEditorText="True" Width="100px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="true" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="GL Accounts">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="M" />

                    <px:PXDropDown CommitChanges="True" ID="edBudgetValidation" runat="server" AllowNull="False" DataField="BudgetValidation"  />
					<px:PXDropDown ID="edExpenseAccountDefault" runat="server" AllowNull="False" DataField="ExpenseAccountDefault" SelectedIndex="2"  />
					<px:PXSegmentMask ID="edExpenseSubMask" runat="server" DataField="ExpenseSubMask" SelectMode="Segment" />
					<px:PXSegmentMask CommitChanges="True" ID="edExpenseAcctID" runat="server" DataField="ExpenseAcctID" />
					<px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" AutoRefresh="true" /></Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Enabled="true" Container="Window" />
	</px:PXTab>
</asp:Content>
