<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS200600.aspx.cs" Inherits="Page_FS200600" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" BorderStyle="NotSet" 
        PrimaryView="SkillRecords" SuspendUnloading="False" 
        TypeName="PX.Objects.FS.SkillMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		    <px:PXDSCallbackCommand Name="Insert" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="CopyPaste" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="Delete" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="First" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="Previous" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="Next" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="Last" PopupCommand="" PopupCommandTarget="" 
                PopupPanel="" Text="" Visible="False">
            </px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" 
        SkinID="Primary" TabIndex="1900">
		<Levels>
			<px:PXGridLevel DataMember="SkillRecords" DataKeyNames="SkillCD">
			    <RowTemplate>
                    <px:PXMaskEdit ID="SkillCD" runat="server" DataField="SkillCD" CommitChanges="True">
                    </px:PXMaskEdit>
                    <px:PXTextEdit ID="Descr" runat="server" DataField="Descr">
                    </px:PXTextEdit>
                    <px:PXCheckBox ID="edIsDriverSkill" runat="server" DataField="IsDriverSkill" 
                        Text="Driver Skill">
                    </px:PXCheckBox>
                </RowTemplate>
			    <Columns>
                    <px:PXGridColumn DataField="SkillCD" CommitChanges="True">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Descr">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="IsDriverSkill" TextAlign="Center" Type="CheckBox">
                    </px:PXGridColumn>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="False">
		</ActionBar>
	    <Mode AllowUpload="True" />
	</px:PXGrid>
</asp:Content>
