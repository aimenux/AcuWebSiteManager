<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL103060.aspx.cs" Inherits="Page_GL103060"
	Title="GL Branch Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.GLBranchAccess"
		PrimaryView="Group">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Delete" Visible="false" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="false" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="formGroup" runat="server" Width="100%" DataMember="Group" Caption="Restriction Group"
		DefaultControlID="edGroupName" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth='SM' ControlSize="M" />
			<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName">
				<AutoCallBack Command="Cancel" Target="ds" Enabled="False">
				</AutoCallBack>
			</px:PXSelector>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description">
			</px:PXTextEdit>
			<px:PXDropDown ID="edGroupType" runat="server" DataField="GroupType">
			</px:PXDropDown>
			<px:PXCheckBox ID="chkActive" runat="server" DataField="Active">
			</px:PXCheckBox>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="168%" Width="100%" DataMember="SegmentFilter"
		TabIndex="200">
		<Items>
			<px:PXTabItem Text="Branches">
				<Template>
					<px:PXGrid ID="gridBranchs" runat="server" Height="300px" Width="100%" AllowPaging="True"
						AdjustPageSize="Auto" AllowSearch="True" SkinID="DetailsInTab" TabIndex="300"
						FastFilterFields="BranchCD,AcctName">
						<Levels>
							<px:PXGridLevel DataMember="Branch" DataKeyNames="BranchCD">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth='SM' ControlSize="M" />
									<px:PXSegmentMask ID="edBranchCD" runat="server" DataField="BranchCD">
									</px:PXSegmentMask>
									<px:PXSelector ID="edRoleName" runat="server" DataField="RoleName">
									</px:PXSelector>
									<px:PXSelector ID="edLedgerID" runat="server" DataField="LedgerID">
									</px:PXSelector>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn Label="Included" TextAlign="Center" Type="CheckBox" DataField="Included">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="BranchCD">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="AcctName">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="LedgerID">
									</px:PXGridColumn>
								</Columns>
								<Mode AllowAddNew="True" AllowDelete="False" />
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowDelete="False" />
						<ActionBar>
							<Actions>
								<Save Enabled="False" />
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
								<EditRecord Enabled="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Subaccounts" BindingContext="tab" VisibleExp="DataControls[&quot;chkValidCombos&quot;].Value == true">
				<Template>
					<px:PXGrid ID="gridSubs" runat="server" Height="300px" Width="100%" AllowPaging="True"
						AdjustPageSize="Auto" AllowSearch="True" EditPageUrl="~/Pages/GL/GL104030.aspx"
						SkinID="DetailsInTab" TabIndex="400" FastFilterFields="SubCD,Description">
						<Levels>
							<px:PXGridLevel DataMember="Sub" DataKeyNames="SubCD">
								<Mode AllowAddNew="True" AllowDelete="False" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth='SM' ControlSize="M" />
									<px:PXSegmentMask ID="edSubCD" runat="server" DataField="SubCD">
									</px:PXSegmentMask>
									<px:PXCheckBox ID="chkSubActive" runat="server" Checked="True" DataField="Active">
									</px:PXCheckBox>
									<px:PXTextEdit ID="edSubDescription" runat="server" DataField="Description">
									</px:PXTextEdit>
									<px:PXCheckBox ID="chkSubIncluded" runat="server" DataField="Included">
									</px:PXCheckBox>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" AllowCheckAll="True">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="SubCD">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Description">
									</px:PXGridColumn>
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowDelete="False" />
						<EditPageParams>
							<px:PXControlParam ControlID="gridSubs" Name="SubCD" PropertyName="DataValues[&quot;SubCD&quot;]"
								Type="String" />
						</EditPageParams>
						<ActionBar>
							<Actions>
								<Save Enabled="False" />
								<Delete Enabled="False" />
								<EditRecord Enabled="False" Text="Membership" ImageUrl="main@Settings" GroupIndex="2" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Subaccount Segments" BindingContext="tab" VisibleExp="DataControls[&quot;chkValidCombos&quot;].Value == false"
				RepaintOnDemand="false">
				<Template>
					<px:PXPanel ID="PXPanel1" runat="server" RenderSimple="True" ContentLayout-OuterSpacing="Around">
						<px:PXLayoutRule runat="server" LabelsWidth='M' ControlSize="M" StartColumn="True">
						</px:PXLayoutRule>
						<px:PXSelector ID="edSegmentID" runat="server" CommitChanges="True" DataField="SegmentID">
						</px:PXSelector>
						<px:PXCheckBox ID="chkValidCombos" runat="server" DataField="ValidCombos" Hidden="True">
						</px:PXCheckBox>
					</px:PXPanel>
					<px:PXGrid ID="gridSegments" runat="server" Height="300px" Width="100%" Style="border-width: 1px 0px 0px 0px"
						AllowPaging="True" AdjustPageSize="Auto" AllowSearch="True" EditPageUrl="~/Pages/GL/GL104040.aspx"
						SkinID="Details" TabIndex="500" FastFilterFields="Value,Descr">
						<Levels>
							<px:PXGridLevel DataMember="Segment">
								<Mode AllowAddNew="True" AllowDelete="False" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" />
									<px:PXSelector ID="edValue" runat="server" DataField="Value">
									</px:PXSelector>
									<px:PXCheckBox ID="chkSegmentActive" runat="server" Checked="True" DataField="Active">
									</px:PXCheckBox>
									<px:PXTextEdit ID="edSegmentDescription" runat="server" DataField="Descr">
									</px:PXTextEdit>
									<px:PXCheckBox ID="chkSegmentIncluded" runat="server" DataField="Included">
									</px:PXCheckBox>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" AllowCheckAll="True">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Value">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" RenderEditorText="True">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Descr">
									</px:PXGridColumn>
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowDelete="False" />
						<EditPageParams>
							<px:PXControlParam ControlID="gridSegments" Name="SegmentID" PropertyName="DataValues[&quot;SegmentID&quot;]"
								Type="Int32" />
							<px:PXControlParam ControlID="gridSegments" Name="Value" PropertyName="DataValues[&quot;Value&quot;]"
								Type="String" />
						</EditPageParams>
						<ActionBar>
							<Actions>
								<Save Enabled="False" />
								<Delete Enabled="False" />
								<EditRecord Enabled="False" Text="Membership" ImageUrl="main@Settings" GroupIndex="2" Order="0" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
