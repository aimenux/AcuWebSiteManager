<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL104000.aspx.cs" Inherits="Page_GL104000"
	Title="GL Account Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.GLAccess"
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
	<px:PXFormView ID="formGroup" runat="server"  
		Width="100%" DataMember="Group" Caption="Restriction Group" 
		DefaultControlID="edGroupName" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth='SM' ControlSize="M" />
			<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" >
			</px:PXSelector>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<px:PXDropDown ID="edGroupType" runat="server"  DataField="GroupType" />
			<px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="168%" Width="100%"  
		DataMember="SegmentFilter" TabIndex="200">
		<Items>
			<px:PXTabItem Text="Users">
				<Template>
					<px:PXGrid ID="gridUsers" BorderWidth="0px" runat="server"  Height="150px"
						 Width="100%" AllowPaging="True" AdjustPageSize="Auto" AllowSearch="True"
						EditPageUrl="~/Pages/SM/SM201035.aspx" SkinID="DetailsInTab" 
						FastFilterFields="Username,FullName" TabIndex="300">
						<Levels>
							<px:PXGridLevel DataMember="Users" DataKeyNames="Username">
								<Mode AllowAddNew="True" AllowDelete="False" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth='SM' ControlSize="M" />
									<px:PXSelector ID="edUsername" runat="server" DataField="Username" TextField="Username" />
									<px:PXTextEdit ID="edComment" runat="server" DataField="Comment" />
									<px:PXCheckBox ID="chkIncluded" runat="server" DataField="Included" />
									<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName">
									</px:PXTextEdit>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
									<px:PXGridColumn DataField="Username" />
									<px:PXGridColumn DataField="FullName" />
									<px:PXGridColumn DataField="Comment" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowDelete="False" />
						<EditPageParams>
							<px:PXControlParam ControlID="gridUsers" Name="Username" PropertyName="DataValues[&quot;Username&quot;]"
								Type="String" />
						</EditPageParams>
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
			<px:PXTabItem Text="Accounts">
				<Template>
					<px:PXGrid ID="gridAccounts" runat="server" BorderWidth="0px"  Height="150px"
						Width="100%" AllowPaging="True" AdjustPageSize="Auto" AllowSearch="True" EditPageUrl="~/Pages/GL/GL104020.aspx"
						SkinID="DetailsInTab" FastFilterFields="AccountCD,Description" TabIndex="400">
						<Levels>
							<px:PXGridLevel DataMember="Account">
								<Mode AllowAddNew="True" AllowDelete="False" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth='SM' ControlSize="M" />
									<px:PXSegmentMask ID="edAccountCD" runat="server" DataField="AccountCD" />
									<px:PXDropDown ID="edType" runat="server" DataField="Type" />
									<px:PXSelector ID="edAccountClassID" runat="server" DataField="AccountClassID" />
									<px:PXCheckBox ID="chkAccountIncluded" runat="server" DataField="Included" />
									<px:PXCheckBox ID="chkActive" runat="server" Checked="True" DataField="Active" />
									<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
									<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
									<px:PXGridColumn DataField="AccountCD" />
									<px:PXGridColumn  DataField="Type" Type="DropDownList" />
									<px:PXGridColumn DataField="AccountClassID" />
									<px:PXGridColumn  DataField="Active" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="Description" />
									<px:PXGridColumn DataField="CuryID" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowDelete="False" />
						<EditPageParams>
							<px:PXControlParam ControlID="gridAccounts" Name="AccountCD" PropertyName="DataValues[&quot;AccountCD&quot;]"
								Type="String" />
						</EditPageParams>
						<ActionBar>
							<Actions>
								<Save Enabled="False" />
								<Delete Enabled="False" />
                                <EditRecord Enabled="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Subaccounts" BindingContext="tab" VisibleExp="DataControls[&quot;chkValidCombos&quot;].Value == true">
				<Template>
					<px:PXGrid ID="gridSubs" BorderWidth="0px" runat="server"  Height="150px"
						 Width="100%" AllowPaging="True" AdjustPageSize="Auto" AllowSearch="True"
						EditPageUrl="~/Pages/GL/GL104030.aspx" SkinID="DetailsInTab" 
						FastFilterFields="SubCD,Description" TabIndex="500">
						<Levels>
							<px:PXGridLevel DataMember="Sub">
								<Mode AllowAddNew="True" AllowDelete="False" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth='SM' ControlSize="M" />
									<px:PXSegmentMask ID="edSubCD" runat="server" DataField="SubCD" AllowEdit="True" />
									<px:PXCheckBox ID="chkSubActive" runat="server" Checked="True" DataField="Active" />
									<px:PXLayoutRule runat="server" Merge="True" />
									<px:PXTextEdit Size="xxl" ID="edSubDescription" runat="server" DataField="Description" />
									<px:PXCheckBox ID="chkSubIncluded" runat="server" DataField="Included" />
									<px:PXLayoutRule runat="server" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" Width="30px"
										AllowCheckAll="True" RenderEditorText="True" />
									<px:PXGridColumn DataField="SubCD" RenderEditorText="True" />
									<px:PXGridColumn  DataField="Active" TextAlign="Center" Type="CheckBox" RenderEditorText="True"  />
									<px:PXGridColumn DataField="Description"  />
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
                                <EditRecord Enabled="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Subaccount Segments" BindingContext="tab" VisibleExp="DataControls[&quot;chkValidCombos&quot;].Value == false"
				RepaintOnDemand="false">
				<Template>
					<px:PXPanel ID="PXPanel1" runat="server" RenderSimple="True" ContentLayout-OuterSpacing="Around">
						<px:PXLayoutRule runat="server"  LabelsWidth='SM' ControlSize="M" StartColumn="True">
						</px:PXLayoutRule>
						<px:PXSelector ID="edSegmentID" runat="server" CommitChanges="True" DataField="SegmentID">
						</px:PXSelector>
						<px:PXCheckBox ID="chkValidCombos" runat="server" DataField="ValidCombos" Enabled="False"
							Hidden="True">
						</px:PXCheckBox>
					</px:PXPanel>
					<px:PXGrid ID="gridSegments" runat="server"  Height="150px" Style="border-width: 1px 0px 0px 0px"
						Width="100%" AllowPaging="True" AdjustPageSize="Auto" AllowSearch="True" EditPageUrl="~/Pages/GL/GL104040.aspx"
						SkinID="Details" FastFilterFields="Value,Descr" TabIndex="600">
						<Levels>
							<px:PXGridLevel DataMember="Segment">
								<Mode AllowAddNew="True" AllowDelete="False" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXSelector ID="edValue" runat="server" DataField="Value" />
									<px:PXCheckBox ID="chkSegmentActive" runat="server" Checked="True" DataField="Active" />
									<px:PXTextEdit ID="edSegmentDescription" runat="server" DataField="Descr" />
									<px:PXCheckBox ID="chkSegmentIncluded" runat="server" DataField="Included" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
									<px:PXGridColumn DataField="Value" />
									<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="Descr" />
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
                                <EditRecord Enabled="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
