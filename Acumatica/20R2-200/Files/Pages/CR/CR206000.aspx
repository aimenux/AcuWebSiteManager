<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR206000.aspx.cs" Inherits="Page_CR206000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%"
		PrimaryView="CaseClasses" TypeName="PX.Objects.CR.CRCaseClassMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />			
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="CaseClasses" Caption="Case Class Summary" FilesIndicator="True" NoteIndicator="True"
		ActivityIndicator="true" ActivityField="NoteActivity" DefaultControlID="edCaseClassID">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="s"
				ControlSize="XL" />
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True"/>
			<px:PXSelector ID="edCaseClassID" runat="server" DataField="CaseClassID" Size="SM" FilterByAllFields="True" />
            <px:PXCheckBox ID="chkInternal" runat="server" DataField="IsInternal"/>
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="s"
				ControlSize="S" /> 
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="253px" DataSourceID="ds" DataMember="CaseClassesCurrent"
		LoadOnDemand="True">
		<Items>
			<px:PXTabItem Text="Details" RepaintOnDemand="False">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule7" runat="server" LabelsWidth="XM" ControlSize="XM" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkRequireCustomer" runat="server" DataField="RequireCustomer" />
					<px:PXCheckBox SuppressLabel="True" ID="chkRequireContact" runat="server" DataField="RequireContact" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkRequireContract" runat="server" DataField="RequireContract" />
					<px:PXSelector ID="edDefaultEMailAccount" runat="server" DataField="DefaultEMailAccountID" DisplayMode="Text" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsBillable" runat="server" DataField="IsBillable" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkAllowOverrideBillable" runat="server" DataField="AllowOverrideBillable" />
				    <px:PXDropDown ID="edPerItemBilling" runat="server" CommitChanges="True" AllowNull="False" DataField="PerItemBilling"/>
					<%--<px:PXDropDown ID="PXDropDown1" runat="server" AllowNull="False" DataField="PerItemBilling" Size="SM" CommitChanges="True" />--%>
                    <px:PXSegmentMask ID="edLabourItemID" runat="server" DataField="LabourItemID" AllowEdit="true" />
                    <px:PXSegmentMask ID="edOvertimeItemID" runat="server" DataField="OvertimeItemID" AllowEdit="true" />
					<px:PXTimeSpan TimeMode="True" ID="edRoundingInMinutes" runat="server" DataField="RoundingInMinutes" InputMask="hh:mm" AllowNull="False" Size="XS" />
					<px:PXTimeSpan TimeMode="True" ID="edMinBillTimeInMinutes" runat="server" DataField="MinBillTimeInMinutes" InputMask="hh:mm" AllowNull="False" Size="XS" />
                    <px:PXNumberEdit ID="edReopenCaseTimeInDays" runat="server" DataField="ReopenCaseTimeInDays" Size="XS" AllowNull="True"/>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Reaction">
				<Template>
					<px:PXGrid ID="gridCaseClassesReaction" runat="server" SkinID="Details" DataSourceID="ds"
						Height="100px" Style="z-index: 100; left: 0px; position: absolute; top: 0px"
						Width="100%" BorderWidth="0">
						<Levels>
							<px:PXGridLevel DataMember="CaseClassesReaction">
                                <RowTemplate>
					                <px:PXMaskEdit ID="edTimeReaction" runat="server" DataField="TimeReaction" InputMask="### d\ays ## hrs ## mins"
										EmptyChar="0" Text="0" />
				                </RowTemplate>
								<Columns>
									<px:PXGridColumn DisplayFormat="&gt;a" DataField="Severity" Type="DropDownList" />
									<px:PXGridColumn TextAlign="Right" DataField="TimeReaction" AllowNull="False" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Labor Items" VisibleExp="DataControls[&quot;edPerItemBilling&quot;].Value = 1" >
				<Template>
					<px:PXGrid ID="LaborClassesGrid" runat="server" SkinID="Details" ActionsPosition="Top"
						DataSourceID="ds" Width="100%" BorderWidth="0px" MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="LaborMatrix">
								<Columns>
									<px:PXGridColumn DataField="EarningType" CommitChanges="True" />
									<px:PXGridColumn DataField="LabourItemID" CommitChanges="True" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Attributes">
				<Template>
					<px:PXGrid ID="AttributesGrid" runat="server" SkinID="Details" ActionsPosition="Top"
						DataSourceID="ds" Width="100%" BorderWidth="0px" MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="Mapping">
								<RowTemplate>
									<px:PXSelector CommitChanges="True" ID="edAttributeID" runat="server" DataField="AttributeID" AllowEdit="True" FilterByAllFields="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="AttributeID" AutoCallBack="True" LinkCommand="CRAttribute_ViewDetails" />
									<px:PXGridColumn AllowNull="False" DataField="Description" />
									<px:PXGridColumn DataField="SortOrder" TextAlign="Right" />
									<px:PXGridColumn AllowNull="False" DataField="Required" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="True" DataField="CSAttribute__IsInternal" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList"/>
                                    <px:PXGridColumn AllowNull="True" DataField="DefaultValue" RenderEditorText="False" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
