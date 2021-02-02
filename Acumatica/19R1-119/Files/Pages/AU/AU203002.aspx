<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AU203002.aspx.cs" Inherits="Page_AU203002" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">

	<px:PXFormView runat="server" SkinID="transparent" ID="formTitle"
		DataSourceID="ds" DataMember="ViewPageTitle" Width="100%">
		<Template>
			<px:PXTextEdit runat="server" ID="PageTitle" DataField="PageTitle" SelectOnFocus="False"
				SkinID="Label" SuppressLabel="true"
				Width="90%"
				Style="padding: 10px">
				<font size="14pt" names="Arial,sans-serif;" />
			</px:PXTextEdit>
		</Template>
	</px:PXFormView>

	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
		TypeName="PX.SM.GraphTableEdit"
		PrimaryView="Filter">
		<CallbackCommands>
			<%-- <px:PXDSCallbackCommand name=""/>--%>
			<px:PXDSCallbackCommand Name="actionSelectField" RepaintControls="None" RepaintControlsIDs="form,FormDataField" />

			<px:PXDSCallbackCommand Name="actionAddColumns" Visible="false" />
			<px:PXDSCallbackCommand Name="actionColumnUp" Visible="false" />
			<px:PXDSCallbackCommand Name="actionColumnDown" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%">
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">

	<px:PXSplitContainer runat="server" ID="splitFields" SplitterPosition="250">
		<Template1>
			<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px"
				SkinID="Details" SyncPosition="True" SyncPositionWithGraph="True" AutoAdjustColumns="True">
				<Levels>
					<px:PXGridLevel DataMember="ViewFields">
						<Columns>
							<px:PXGridColumn DataField="FieldName" Width="200px" />
						</Columns>
					</px:PXGridLevel>
				</Levels>
				<AutoSize Container="Parent" Enabled="True" MinHeight="150" />
				<ActionBar Position="Top" ActionsHidden="False">
					<Actions>
						<ExportExcel ToolBarVisible="False" />
						<AdjustColumns ToolBarVisible="False" />
						<AddNew ToolBarVisible="False" />
					</Actions>
				</ActionBar>
				<AutoCallBack Enabled="True" Target="ds" Command="actionSelectField">
					<Behavior CommitChanges="True" RepaintControls="None" RepaintControlsIDs="form,FormDataField" />
				</AutoCallBack>
			</px:PXGrid>
		</Template1>

		<Template2>
			<px:PXFormView ID="FormDataField" runat="server"
				SkinID="Transparent"
				DataSourceID="ds"
				DataMember="ViewAttributes"
				Width="100%"
				AutoRepaint="True"
				AllowFocus="False">
				<AutoSize Enabled="True" />
				<Template>

					<px:PXSplitContainer runat="server" ID="SplitAttributes" SplitterPosition="300"
						Orientation="Horizontal" Width="100%" Height="100px" Style="padding: 10px;"
						SkinID="Horizontal" CssClass="splitContainer transparent">
						<AutoSize Enabled="True" />
						<Template1>
							<%--		<px:PXPanel ID="PXPanel10" runat="server" SkinID="Transparent">
										<px:PXLayoutRule ID="PXLayoutRule10" runat="server" Merge="True" />
										<px:PXDropDown ID="edMethod" runat="server" DataField="Method" Size="M" CommitChanges="True" />
									</px:PXPanel>--%>


							<px:PXTextEdit runat="server" ID="FieldTitle" DataField="FieldTitle" SkinID="Label" Width="100%"
								>
								<font size="14pt" names="Arial,sans-serif;" />
							</px:PXTextEdit>
							<px:PXPanel ID="PXPanel10" runat="server" SkinID="Transparent" Width="100%">

								<px:PXLayoutRule ID="PXLayoutRule10" runat="server" LabelsWidth="170px" ControlSize="XXL" />

								<px:PXDropDown ID="PXDropDown1" runat="server" DataField="Method"  CommitChanges="True" />
								<px:PXDropDown ID="StorageType" runat="server" DataField="StorageType"  />
							    <px:PXTextEdit runat="server" ID="BqlFieldName" DataField="BqlFieldName" Enabled="False"/>
							</px:PXPanel>
							<px:PXTextEdit ID="edDacAttrEdit" runat="server"
								DataField="DacAttrEdit"
								Height="100px"
								Style="padding: 10px 0px 0px 10px; margin-bottom: 10px; margin-top: 10px;"
								TextMode="MultiLine"
								Width="100%"
								Wrap="False"
								SelectOnFocus="False"
								Font-Names="Courier New"
								Font-Size="10pt"
								DisableSpellcheck="True"
								LabelID="lblCustom">
								<AutoSize Enabled="True" />
							</px:PXTextEdit>
						</Template1>

						<Template2>
							<%--<px:PXLayoutRule ID="PXLayoutRule3" runat="server" />--%>
							<px:PXLabel ID="lblReadonly" runat="server">Original attributes</px:PXLabel>
							<br />
							<px:PXTextEdit ID="edReadonly" runat="server"
								Width="100%"
								DataField="DacAttrReadonly"
								SuppressLabel="true"
								Height="100px"
								LabelID="lblReadonly"
								Style="padding-left: 10px; padding-top: 10px; margin-top: 10px;"
								TextMode="MultiLine"
								Wrap="False" SelectOnFocus="False"
								Font-Names="Courier New" Font-Size="10pt">
								<AutoSize Enabled="True" Container="Parent" />
							</px:PXTextEdit>




						</Template2>


					</px:PXSplitContainer>


				</Template>

				<AutoSize Enabled="True" />

			</px:PXFormView>

		</Template2>
		<AutoSize Enabled="true" Container="Window" />
	</px:PXSplitContainer>

	<px:PXSmartPanel ID="DlgNewField" runat="server"
		CaptionVisible="True"
		Caption="Create New Field"
		AutoRepaint="True"
		Key="NewFieldWizard">
		<px:PXFormView ID="FormSelectTable" runat="server"
			SkinID="Transparent" DataMember="NewFieldWizard">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" />

				<px:PXTextEdit runat="server" ID="edNewFieldName" DataField="NewFieldName" CommitChanges="true" />
				<px:PXTextEdit runat="server" ID="DisplayName" DataField="DisplayName" />
				<px:PXDropDown runat="server" ID="edStorageType" DataField="StorageType" CommitChanges="True" />
				<px:PXDropDown runat="server" ID="edDataType" DataField="DataType" CommitChanges="true" />
				<px:PXNumberEdit runat="server" ID="Length" DataField="Length" AllowNull="True" />
				<px:PXNumberEdit runat="server" ID="Precision" DataField="Precision" AllowNull="True" />

			</Template>
		</px:PXFormView>

		<px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartRow="True" />
		<px:PXPanel ID="PXPanel7" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton13" runat="server" DialogResult="OK" Text="OK">
			</px:PXButton>

			<px:PXButton ID="PXButton14" runat="server" DialogResult="Cancel" Text="Cancel" CausesValidation="False">
			</px:PXButton>
		</px:PXPanel>

	</px:PXSmartPanel>

	<px:PXSmartPanel ID="DlgExistingField" runat="server"
		CaptionVisible="True"
		Caption="Change Existing Field"
		AutoRepaint="True"
		Key="FieldWizard">
		<px:PXFormView ID="PXFormView1" runat="server"
			SkinID="Transparent" DataMember="FieldWizard">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="S" ControlSize="M" />

				<px:PXDropDown runat="server" ID="edCustFieldName" DataField="baseFieldName" CommitChanges="true" />
			</Template>
		</px:PXFormView>

		<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartRow="True" />
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="OK">
			</px:PXButton>

			<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" CausesValidation="False">
			</px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>

	<px:PXSmartPanel ID="PanelSelectorColumns" runat="server"
		CaptionVisible="True"
		Caption="Customize Selector Columns"
		AutoRepaint="True"
		Width="600px"
		Height="400px"
		Key="ViewSelectorCols">


		<px:PXGrid runat="server" ID="GridSelectorCols" Width="100%" SyncPosition="True"
			SyncPositionWithGraph="True"
			AllowPaging="False" AutoAdjustColumns="True"
			SkinID="DetailsInTab" Height="400px">
			<AutoSize Enabled="True" Container="Parent" />
			<ActionBar>
				<Actions>
					<ExportExcel ToolBarVisible="False" />
					<AdjustColumns ToolBarVisible="False" />
					<AddNew ToolBarVisible="False" />

				</Actions>


				<CustomItems>

					<px:PXToolBarButton>
						<AutoCallBack Command="actionAddColumns" Target="ds" />
					</px:PXToolBarButton>

					<px:PXToolBarButton>
						<AutoCallBack Command="actionColumnUp" Target="ds" />
					</px:PXToolBarButton>

					<px:PXToolBarButton>
						<AutoCallBack Command="actionColumnDown" Target="ds" />
					</px:PXToolBarButton>


				</CustomItems>
			</ActionBar>

			<Levels>
				<px:PXGridLevel DataMember="ViewSelectorCols">
					<Columns>


						<px:PXGridColumn DataField="DisplayName" Width="200px" />
						<px:PXGridColumn DataField="Name" Width="200px" />

					</Columns>
				</px:PXGridLevel>
			</Levels>
			<Mode AllowSort="false" AllowAddNew="false" AllowUpdate="false" />

		</px:PXGrid>


		<px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton5" runat="server" DialogResult="OK" Text="OK">
			</px:PXButton>

			<px:PXButton ID="PXButton6" runat="server" DialogResult="Cancel" Text="Cancel" CausesValidation="False">
			</px:PXButton>
		</px:PXPanel>

	</px:PXSmartPanel>



	<px:PXSmartPanel ID="PanelAddSelectorCols" runat="server"
		CaptionVisible="True"
		Caption="Add Columns to Selector"
		AutoRepaint="True"
		Width="600px"
		Height="400px"
		Key="ViewAddSelectorCols">


		<px:PXGrid runat="server" ID="GridAddSelectorCols" Width="100%"
			AllowPaging="False" AutoAdjustColumns="True"
			FilterView="ViewSelectorFilters"
			AllowFilter="True"
			BlankFilterHeader="ALL"
			SkinID="Attributes" Height="300px">

			<ActionBar Position="Top" ActionsHidden="True">
				<Actions>
					<FilterSet ToolBarVisible="Top" GroupIndex="0" Order="0" Enabled="True" />
					<%--<ExportExcel ToolBarVisible="False"/>
										<AdjustColumns ToolBarVisible="False"/>
										<AddNew ToolBarVisible="False"/>
										<Delete ToolBarVisible="False"/>--%>
				</Actions>





				<CustomItems>

<%--					<px:PXToolBarButton Text="" DisplayStyle="Text">
						<ActionBar GroupIndex="0" />
					</px:PXToolBarButton>--%>

					<%--		<px:PXToolBarLabel>
											<ActionBar GroupIndex="0"  />
										</px:PXToolBarLabel>--%>

<%--					<px:PXToolBarLabel>
						<ActionBar GroupIndex="2" Order="1" />
					</px:PXToolBarLabel>--%>
				</CustomItems>



			</ActionBar>

			<Levels>
				<px:PXGridLevel DataMember="ViewAddSelectorCols">
					<Columns>
						<px:PXGridColumn DataField="Selected" Type="CheckBox" />

						<px:PXGridColumn DataField="DisplayName" Width="200px" />
						<px:PXGridColumn DataField="Name" Width="200px" />

					</Columns>
				</px:PXGridLevel>
			</Levels>
			<Mode AllowSort="false" />
			<AutoSize Enabled="True" Container="Parent" />
		</px:PXGrid>



		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="OK">
			</px:PXButton>

			<px:PXButton ID="PXButton4" runat="server" DialogResult="Cancel" Text="Cancel" CausesValidation="False">
			</px:PXButton>
		</px:PXPanel>

	</px:PXSmartPanel>





	<px:PXSmartPanel ID="PanelAttributes" runat="server"
		CaptionVisible="True"
		Caption="Customize Attributes"
		AutoRepaint="True"
		Width="800px"
		Height="600px"
		ShowMaximizeButton="True"
		Key="ViewListAttributes">

		<px:PXSplitContainer runat="server" ID="PXSplitContainer1" SplitterPosition="250"  Orientation="Vertical" Width="100%" Height="300px">
			<AutoSize Enabled="True" Container="Parent"></AutoSize>
			<Template1>

				<px:PXGrid runat="server" ID="GridAttributes"
					Width="100%"
					AllowPaging="False"
					AutoAdjustColumns="True"
					SyncPosition="True"
					SkinID="DetailsInTab"
					Height="300px">
					<ActionBar>
						<Actions>
							<ExportExcel ToolBarVisible="False"/>
							<AdjustColumns ToolBarVisible="False"/>
						</Actions>
					</ActionBar>

					<AutoSize Enabled="True" Container="Parent"/>
					<AutoCallBack Target="ds" Command="actionSelectAttribute">
						<Behavior CommitChanges="False" RepaintControls="None" RepaintControlsIDs="form,GridProps"/>
					</AutoCallBack>
					<Levels>
						<px:PXGridLevel DataMember="ViewListAttributes">
							<Columns>

								<px:PXGridColumn DataField="DisplayName" Width="200px" />

							</Columns>
						</px:PXGridLevel>
					</Levels>
					
					<Mode AllowSort="false" />

				</px:PXGrid>

			</Template1>

			<Template2>

				<px:PXGrid runat="server" ID="GridProps"
					Width="100%"
					AllowPaging="False"
					AutoAdjustColumns="True"
					SyncPosition="True"
					SkinID="DetailsInTab"
					MatrixMode="True"
					Height="300px">

					<AutoSize Enabled="True" Container="Parent"/>
					
					<ActionBar>
						<Actions>
							<ExportExcel ToolBarVisible="False"/>
							<AdjustColumns ToolBarVisible="False"/>
							<AddNew ToolBarVisible="False"/>
							<Delete ToolBarVisible="False"/>
						</Actions>
					</ActionBar>

					<Levels>
						<px:PXGridLevel DataMember="ViewListProps">
							<Columns>

								<px:PXGridColumn DataField="Name" Width="200px" />
								<px:PXGridColumn DataField="PropValue" Width="200px" />

							</Columns>
						</px:PXGridLevel>
					</Levels>
					<Mode AllowSort="false" AllowAddNew="false" AllowDelete="false"/>

				</px:PXGrid>

			</Template2>


		</px:PXSplitContainer>



		<px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton7" runat="server" DialogResult="OK" Text="OK">
			</px:PXButton>

			<px:PXButton ID="PXButton8" runat="server" DialogResult="Cancel" Text="Cancel" CausesValidation="False">
			</px:PXButton>
		</px:PXPanel>

	</px:PXSmartPanel>
</asp:Content>
