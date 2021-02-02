<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SP203000.aspx.cs" Inherits="Pages_SP203000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="SP.Objects.SP.SPCaseNewEntry" PrimaryView="Case">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" Visible="False"/>
            <px:PXDSCallbackCommand Name="Submit" CommitChanges="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Case" Width="100%" AllowCollapse="False" DefaultControlID="edCaseCD" MarkRequired="Dynamic">
		<Template>
		<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" /> 
		<px:PXSelector ID="edcaseCD" runat="server" DataField="caseCD"  DisplayMode="Text" Visible="False" NullText="<NEW>"/>
		<px:PXSelector CommitChanges="True" ID="edContractID" runat="server" 
                DataField="ContractID" DisplayMode="Text" TextMode="Search" TextField="description" AutoRefresh="True" FilterByAllFields="True" />
			<px:PXDropDown ID="edPriority" runat="server" DataField="Priority"  CommitChanges="True"/> 
            <px:PXSelector ID="edcaseClassID" runat="server" DataField="caseClassID" CommitChanges="True" DisplayMode="Text" />
            <px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" Size="XXL" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="CaseCurrent">
		<Items>
			<px:PXTabItem Text="Details">
				<Template>
					<px:PXRichTextEdit ID="PXRichTextEdit1" runat="server" Style="border-width: 0px; width: 100%;" DataField="Description" 
						AllowDatafields="false" AllowAttached="true">
						<AutoSize Enabled="True" />
					</px:PXRichTextEdit>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Attributes">
				<Template>
					<px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" Height="200px" MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="Answers">
								<Columns>
								    <px:PXGridColumn DataField="IsRequired" Width="80px" AllowShowHide="False" AllowSort="False" Type="CheckBox"/>	
									<px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="250px" AllowShowHide="False"
										TextField="AttributeID_description" />
									<px:PXGridColumn DataField="Value" Width="300px" AllowShowHide="False" AllowSort="False" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="200" />
						<ActionBar>
							<Actions>
								<Search Enabled="False" />
							</Actions>
						</ActionBar>
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="450" MinWidth="300" />
	</px:PXTab>
</asp:Content>

