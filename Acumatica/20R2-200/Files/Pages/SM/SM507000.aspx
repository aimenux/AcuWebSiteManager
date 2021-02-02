<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM507000.aspx.cs" Inherits="Page_SM507000" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.EmailProcessingMaint"
		PrimaryView="Filter">
		<CallbackCommands>			
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="ProcessAll" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="viewDetails" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%"
		DataMember="Filter" Caption="Selection" TemplateContainer=""> 
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="SM" StartColumn="True" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
			<px:PXSelector ID="PXSelector2" runat="server" DataField="Account" DataSourceID="ds"
				Size="XM" CommitChanges="True" DisplayMode="Text"
				TextMode="Search" AllowNull="True" >
			</px:PXSelector>
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server" />
			<px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" />
			<px:PXLabel ID="PXLabel1" runat="server" Width="79px" Style="margin-left: 9px" Text="Assigned to:"/>
			<px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="MyOwner" Size="XXS" Width="40px">
				<AutoCallBack Command="Save" Target="form">
				</AutoCallBack>
			</px:PXCheckBox>
			<px:PXSelector ID="PXSelector3" runat="server" DataField="OwnerID" DataSourceID="ds" Size="XM" SuppressLabel="True">
				<AutoCallBack Command="Save" Target="form">
				</AutoCallBack>
			</px:PXSelector>
			<px:PXLayoutRule ID="PXLayoutRule5" runat="server" />
			<px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="True" />
			<px:PXDropDown ID="PXDropDown1" runat="server" DataField="Type" Size="S" CommitChanges="True"/>            
            <px:PXCheckBox ID="PXCheckBox2" runat="server" DataField="IncludeFailed">
				<AutoCallBack Command="Save" Target="form">
				</AutoCallBack>
			</px:PXCheckBox>
			<px:PXLayoutRule ID="PXLayoutRule7" runat="server" />            
		</Template>	
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" ActionsPosition="Top" Caption="Emails" SkinID="Inquire" NoteIndicator="True"
		FilesIndicator="True" FilesField="NoteFiles">  
		<Levels>
			<px:PXGridLevel DataMember="FilteredItems">
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" DataType="Boolean"
						DefValueText="False" TextAlign="Center" Type="CheckBox" Width="20px" AllowShowHide="False">
						<Header Text="Selected">
						</Header>
					</px:PXGridColumn>
					<px:PXGridColumn DataField="EMailAccount__Description" Width="200px" />
					<px:PXGridColumn DataField="Subject" Width="200px" LinkCommand="viewDetails" />
					<px:PXGridColumn DataField="MailFrom" Width="200px" />
					<px:PXGridColumn DataField="MailTo" Width="200px" />
					<px:PXGridColumn DataField="CRActivity__StartDate" Width="120px" DataType="DateTime" />
					<px:PXGridColumn DataField="CRActivity__OwnerID"  Width="90px" DisplayMode="Text"/>
                    <px:PXGridColumn DataField="MPStatus"  Width="90px"/>                    
				</Columns>
				<Layout FormViewHeight=""></Layout>

			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="cmdViewDetails">
			<CustomItems>
				<px:PXToolBarButton Text="View Details" Tooltip="Shows Email Message Details" Key="cmdViewDetails"
					Visible="false">
					<AutoCallBack Command="viewDetails" Target="ds" Enabled="True">
						<Behavior CommitChanges="True" />
					</AutoCallBack>	
					<ActionBar GroupIndex="0" Order="0"/>
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>