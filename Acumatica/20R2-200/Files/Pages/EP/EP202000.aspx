<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP202000.aspx.cs"
    Inherits="Page_EP202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="EmployeeClass" TypeName="PX.Objects.EP.EmployeeClassMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Employee Class" DataMember="EmployeeClass"
        NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" DefaultControlID="edVendorClassID" TabIndex="500">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector ID="edVendorClassID" runat="server" DataField="VendorClassID" DataSourceID="ds" AutoRefresh="true" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="559px" Style="z-index: 100" Width="100%" Caption="Vendor Class"
        DataMember="CurEmployeeClassRecord" ActivityField="NoteActivity">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="M" StartGroup="True" />
                     <px:PXSelector ID="edTermsID" runat="server" DataField="TermsID" AllowEdit="True" DataSourceID="ds" />
                    <px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" AllowEdit="True" DataSourceID="ds" />
                    <px:PXSegmentMask CommitChanges="True" ID="edCashAcctID" runat="server" DataField="CashAcctID" AutoRefresh="True"
                        DataSourceID="ds" />
                    <px:PXSegmentMask CommitChanges="True" ID="edAPAcctID" runat="server" DataField="APAcctID" DataSourceID="ds" />
                    <px:PXSegmentMask ID="edAPSubID" runat="server" DataField="APSubID" DataSourceID="ds" />
                    <px:PXSegmentMask ID="edDiscTakenAcctID" runat="server" DataField="DiscTakenAcctID" DataSourceID="ds" CommitChanges="true" />
                    <px:PXSegmentMask ID="edDiscTakenSubID" runat="server" DataField="DiscTakenSubID" DataSourceID="ds" />
                    <px:PXSegmentMask CommitChanges="True" ID="edPrepaymentAcctID" runat="server" DataField="PrepaymentAcctID"
                                      DataSourceID="ds" />
                    <px:PXSegmentMask ID="edPrepaymentSubID" runat="server" DataField="PrepaymentSubID" DataSourceID="ds" />
                    <px:PXSegmentMask CommitChanges="True" ID="edExpenseAcctID" runat="server" DataField="ExpenseAcctID" DataSourceID="ds" />
                    <px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" AllowEdit="True" DataSourceID="ds" />
                    <px:PXSegmentMask CommitChanges="True" ID="edSalesAcctID" runat="server" DataField="SalesAcctID" DataSourceID="ds" />
                    <px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" DataSourceID="ds" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXSelector Size="S" ID="edCuryID" runat="server" DataField="CuryID" DataSourceID="ds" />
                    <px:PXCheckBox ID="chkAllowOverrideCury" runat="server" DataField="AllowOverrideCury" />
                    <px:PXLayoutRule runat="server" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXSelector Size="S" ID="edCuryRateTypeID" runat="server" DataField="CuryRateTypeID" DataSourceID="ds" />
                    <px:PXCheckBox ID="chkAllowOverrideRate" runat="server" DataField="AllowOverrideRate" />
                    <px:PXLayoutRule runat="server" />
                    <px:PXSelector ID="edCalendarID" runat="server" DataField="CalendarID" AllowEdit="True" DataSourceID="ds" />
                    <px:PXSelector ID="edTaxZoneID" runat="server" DataField="TaxZoneID" DataSourceID="ds" />
                    <px:PXDropDown ID="edHoursValidation" runat="server" AllowNull="False" DataField="HoursValidation"/>
                    <px:PXDropDown ID="edDefaultDateInActivity" runat="server" AllowNull="False" DataField="DefaultDateInActivity"/>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;
						border: 0px;" Width="100%" ActionsPosition="Top" SkinID="Details"  MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="Mapping">
								<RowTemplate>
									<px:PXSelector CommitChanges="True" ID="edAttributeID" runat="server" DataField="AttributeID" AllowEdit="True" FilterByAllFields="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="AttributeID" AutoCallBack="true" LinkCommand="CRAttribute_ViewDetails" />
									<px:PXGridColumn AllowNull="False" DataField="Description" />
									<px:PXGridColumn DataField="SortOrder" TextAlign="Right" SortDirection="Ascending" />
									<px:PXGridColumn AllowNull="False" DataField="Required" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="True" DataField="CSAttribute__IsInternal" TextAlign="Center" Type="CheckBox" />
					                <px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList" />
                                    <px:PXGridColumn AllowNull="True" DataField="DefaultValue" RenderEditorText="False" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            
        </Items>
        <AutoSize Enabled="true" Container="Window" />
    </px:PXTab>
</asp:Content>
