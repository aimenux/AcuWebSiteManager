using System;
using PX.Data;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [Serializable]
    [PXCacheName(Messages.ConfigurationRule)]
    public class AMConfigurationRule : IBqlTable
	{
        internal string DebuggerDisplay => $"[{ConfigurationID}:{Revision}] RuleSource={RuleSource}; SourceLineNbr={SourceLineNbr}; LineNbr={LineNbr}";

        #region ConfigurationID (key)
        /// <summary>
        /// key field
        /// </summary>
		public abstract class configurationID : PX.Data.BQL.BqlString.Field<configurationID> { }

        /// <summary>
        /// key field
        /// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(AMConfiguration.configurationID))]
        [PXUIField(DisplayName = "Configuration ID", Visible = false, Enabled = false)]
        [PXParent(typeof(Select<AMConfiguration, 
                            Where<AMConfiguration.configurationID, 
                                Equal<Current<AMConfigurationRule.configurationID>>,
                            And<AMConfiguration.revision, 
                                Equal<Current<AMConfigurationRule.revision>>>>>))]
        public virtual string ConfigurationID { get; set; }
		#endregion
        #region Revision (key)
        /// <summary>
        /// key field
        /// </summary>
		public abstract class revision : PX.Data.BQL.BqlString.Field<revision> { }

        /// <summary>
        /// key field
        /// </summary>
        [PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "Revision", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMConfiguration.revision))]
        public virtual string Revision { get; set; }
        #endregion
        #region RuleSource (key)
        /// <summary>
        /// key field
        /// </summary>
        public abstract class ruleSource : PX.Data.BQL.BqlString.Field<ruleSource> { }

        /// <summary>
        /// key field
        /// </summary>
        [PXDBString(1, IsFixed = true, IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Rule Source", Visible = false, Enabled = false)]
        [RuleTargetSource.SourceList]
        public virtual string RuleSource { get; set; }
        #endregion
        #region SourceLineNbr (key)
        /// <summary>
        /// key field
        /// </summary>
        public abstract class sourceLineNbr : PX.Data.BQL.BqlInt.Field<sourceLineNbr> { }
		
        /// <summary>
        /// key field
        /// </summary>
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Source Line Nbr", Visible = false, Enabled = false)]
        public virtual int? SourceLineNbr { get; set; }
        #endregion
        #region LineNbr (key)
        /// <summary>
        /// key field
        /// </summary>
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        /// <summary>
        /// key field
        /// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault]
        [PXUIField(DisplayName = "Line Nbr", Visible = false, Enabled = false)]
        public virtual int? LineNbr { get; set; }
        #endregion
        #region RuleType
        public abstract class ruleType : PX.Data.BQL.BqlString.Field<ruleType> { }

		[PXDBString(1, IsFixed = true)]
        [PXDefault(RuleTypes.Include)]
        [PXUIField(DisplayName = "Rule")]
        [RuleTypes.List]
        public virtual string RuleType { get; set; }
        #endregion
        #region Condition
        public abstract class condition : PX.Data.BQL.BqlString.Field<condition> { }

        [PXDBString(2, InputMask = "")]
        [PXUIField(DisplayName = "Condition")]
        [RuleFormulaConditions.List]
        [PXDefault(RuleFormulaConditions.Equal)]
        public virtual string Condition { get; set; }
        #endregion
        #region IsExpression
        public abstract class isExpression : PX.Data.BQL.BqlBool.Field<isExpression> { }

        [PXDBBool]
        [PXDefault]
        [PXUIField(DisplayName = "From Schema", Visible = false, Enabled = false)]
        public virtual bool? IsExpression { get; set; }
        #endregion
        #region Value1
        public abstract class value1 : PX.Data.BQL.BqlString.Field<value1> { }

        [PXDBString(InputMask = "", IsUnicode = true)]
        [PXUIField(DisplayName = "Value 1")]
        [PXStringList(new string[] { null }, new string[] { "" }, ExclusiveValues = false)]
        public virtual string Value1 { get; set; }
        #endregion
        #region Value2
        public abstract class value2 : PX.Data.BQL.BqlString.Field<value2> { }

        [PXDBString(InputMask = "", IsUnicode = true)]
        [PXUIField(DisplayName = "Value 2")]
        [PXStringList(new string[] { null }, new string[] { "" }, ExclusiveValues = false)]
        public virtual string Value2 { get; set; }
        #endregion
        #region TargetFeatureLineNbr
        public abstract class targetFeatureLineNbr : PX.Data.BQL.BqlInt.Field<targetFeatureLineNbr> { }

		[PXDBInt]
		[PXDefault]
		[PXUIField(DisplayName = "Target Feature")]
        [PXSelector(typeof(Search<AMConfigurationFeature.lineNbr,
                                Where<AMConfigurationFeature.configurationID,
                                    Equal<Current<configurationID>>,
                                And<AMConfigurationFeature.revision,
                                    Equal<Current<revision>>>>>),
                    typeof(AMConfigurationFeature.featureID),
                    typeof(AMConfigurationFeature.label),
                    typeof(AMConfigurationFeature.descr),
                    SubstituteKey = typeof(AMConfigurationFeature.label), DirtyRead = true)]
        public virtual int? TargetFeatureLineNbr { get; set; }
        #endregion
        #region TargetOptionLineNbr
        public abstract class targetOptionLineNbr : PX.Data.BQL.BqlInt.Field<targetOptionLineNbr> { }

		[PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Target Option")]
        [PXSelector(typeof(Search<AMConfigurationOption.lineNbr,
                                Where<AMConfigurationOption.configurationID,
                                    Equal<Current<configurationID>>,
                                And<AMConfigurationOption.revision,
                                    Equal<Current<revision>>,
                                And<AMConfigurationOption.configFeatureLineNbr,
                                    Equal<Current<targetFeatureLineNbr>>>>>>),
                    SubstituteKey = typeof(AMConfigurationOption.label), DirtyRead = true)]
        public virtual int? TargetOptionLineNbr { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }


		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }


		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }


		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }


		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }


		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }


		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }


		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		#endregion
	}

    [System.Diagnostics.DebuggerDisplay("[{ConfigurationID}:{Revision}] SourceLineNbr={SourceLineNbr}; LineNbr={LineNbr}")]
    [Serializable]
    [PXCacheName(Messages.ConfigurationAttributeRule)]
    [PXProjection(typeof(Select<AMConfigurationRule, Where<AMConfigurationRule.ruleSource, Equal<RuleTargetSource.attribute>>>), Persistent = true)]
    public class AMConfigurationAttributeRule : IBqlTable
    {
        #region ConfigurationID
        public abstract class configurationID : PX.Data.BQL.BqlString.Field<configurationID> { }


        [PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(AMConfigurationRule.configurationID))]
        [PXDBDefault(typeof(AMConfigurationAttribute.configurationID))]
        [PXUIField(Visible = false)]
        public virtual string ConfigurationID { get; set; }
        #endregion
        #region Revision
        public abstract class revision : PX.Data.BQL.BqlString.Field<revision> { }


        [PXDBString(10, IsUnicode = true, IsKey = true, BqlField = typeof(AMConfigurationRule.revision))]
        [PXUIField(Visible = false)]
        [PXDBDefault(typeof(AMConfigurationAttribute.revision))]
        public virtual string Revision { get; set; }
        #endregion
        #region RuleSource
        public abstract class ruleSource : PX.Data.BQL.BqlString.Field<ruleSource> { }


        [PXDBString(1, IsFixed = true, IsKey = true, BqlField = typeof(AMConfigurationRule.ruleSource))]
        [PXDefault(RuleTargetSource.Attribute)]
        [PXUIField(DisplayName = "Rule Source", Visible = false, Enabled = false)]
        public virtual string RuleSource { get; set; }
        #endregion
        #region SourceLineNbr
        public abstract class sourceLineNbr : PX.Data.BQL.BqlInt.Field<sourceLineNbr> { }


        [PXDBInt(IsKey = true, BqlField = typeof(AMConfigurationRule.sourceLineNbr))]
        [PXDBDefault(typeof(AMConfigurationAttribute.lineNbr))]
        [PXParent(typeof(Select<AMConfigurationAttribute,
                            Where<AMConfigurationAttribute.configurationID,
                                Equal<Current<AMConfigurationAttributeRule.configurationID>>,
                            And<AMConfigurationAttribute.revision,
                                Equal<Current<AMConfigurationAttributeRule.revision>>,
                            And<AMConfigurationAttribute.lineNbr,
                                Equal<Current<AMConfigurationAttributeRule.sourceLineNbr>>>>>>))]
        [PXUIField(Visible = false)]
        public virtual int? SourceLineNbr { get; set; }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }


        [PXDBInt(IsKey = true, BqlField = typeof(AMConfigurationRule.lineNbr))]
        [PXDefault]
        [PXLineNbr(typeof(AMConfigurationAttribute.lineCntrRule))]
        [PXUIField(Visible = false)]
        public virtual int? LineNbr { get; set; }
        #endregion
        #region RuleType
        public abstract class ruleType : PX.Data.BQL.BqlString.Field<ruleType> { }


        [PXDBString(1, IsFixed = true, BqlField = typeof(AMConfigurationRule.ruleType))]
        [PXDefault(RuleTypes.Include)]
        [PXUIField(DisplayName = "Rule")]
        [RuleTypes.List]
        public virtual string RuleType { get; set; }
        #endregion
        #region Condition
        public abstract class condition : PX.Data.BQL.BqlString.Field<condition> { }


        [PXDBString(2, InputMask = "", BqlField = typeof(AMConfigurationRule.condition))]
        [PXUIField(DisplayName = "Condition")]
        [RuleFormulaConditions.List]
        [PXDefault(RuleFormulaConditions.Equal)]
        public virtual string Condition { get; set; }
        #endregion
        #region IsExpression
        public abstract class isExpression : PX.Data.BQL.BqlBool.Field<isExpression> { }


        [PXDBBool(BqlField = typeof(AMConfigurationRule.isExpression))]
        [PXDefault(true)]
        [PXUIField(DisplayName = "From Schema", Visible = false, Enabled = false)]
        public virtual bool? IsExpression { get; set; }
        #endregion
        #region Value1
        public abstract class value1 : PX.Data.BQL.BqlString.Field<value1> { }


        [PXDBString(InputMask = "", IsUnicode = true, BqlField = typeof(AMConfigurationRule.value1))]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Value 1")]
        [PXStringList(new string[] { null }, new string[] { "" }, ExclusiveValues = false)]
        public virtual string Value1 { get; set; }
        #endregion
        #region Value2
        public abstract class value2 : PX.Data.BQL.BqlString.Field<value2> { }


        [PXDBString(InputMask = "", IsUnicode = true, BqlField = typeof(AMConfigurationRule.value2))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Value 2")]
        [PXStringList(new string[] { null }, new string[] { "" }, ExclusiveValues = false)]
        public virtual string Value2 { get; set; }
        #endregion
        #region TargetFeatureLineNbr
        public abstract class targetFeatureLineNbr : PX.Data.BQL.BqlInt.Field<targetFeatureLineNbr> { }


        [PXDBInt(BqlField = typeof(AMConfigurationRule.targetFeatureLineNbr))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Target Feature")]
        [PXSelector(typeof(Search<AMConfigurationFeature.lineNbr,
                        Where<AMConfigurationFeature.configurationID,
                            Equal<Current<configurationID>>,
                        And<AMConfigurationFeature.revision,
                            Equal<Current<revision>>>>>),
            typeof(AMConfigurationFeature.featureID),
            typeof(AMConfigurationFeature.label),
            typeof(AMConfigurationFeature.descr),
            SubstituteKey = typeof(AMConfigurationFeature.label), DirtyRead = true)]
        public virtual int? TargetFeatureLineNbr { get; set; }
        #endregion
        #region TargetOptionLineNbr
        public abstract class targetOptionLineNbr : PX.Data.BQL.BqlInt.Field<targetOptionLineNbr> { }


        [PXDBInt(BqlField = typeof(AMConfigurationRule.targetOptionLineNbr))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Target Option")]
        [PXSelector(typeof(Search<AMConfigurationOption.lineNbr,
                                Where<AMConfigurationOption.configurationID,
                                    Equal<Current<configurationID>>,
                                And<AMConfigurationOption.revision,
                                    Equal<Current<revision>>,
                                And<AMConfigurationOption.configFeatureLineNbr,
                                    Equal<Current<targetFeatureLineNbr>>>>>>),
            SubstituteKey = typeof(AMConfigurationOption.label),
            DescriptionField = typeof(AMConfigurationOption.descr),
            DirtyRead = true)]
        [AMEmptySelectorValue(Messages.SelectorAll)]
        public virtual int? TargetOptionLineNbr { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }


        [PXDBCreatedDateTime(BqlField = typeof(AMConfigurationRule.createdDateTime))]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }


        [PXDBCreatedByScreenID(BqlField = typeof(AMConfigurationRule.createdByScreenID))]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }


        [PXDBCreatedByID(BqlField = typeof(AMConfigurationRule.createdByID))]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }


        [PXDBLastModifiedDateTime(BqlField = typeof(AMConfigurationRule.lastModifiedDateTime))]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }


        [PXDBLastModifiedByScreenID(BqlField = typeof(AMConfigurationRule.lastModifiedByScreenID))]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }


        [PXDBLastModifiedByID(BqlField = typeof(AMConfigurationRule.lastModifiedByID))]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }


        [PXDBTimestamp(BqlField = typeof(AMConfigurationRule.Tstamp))]
        public virtual byte[] tstamp { get; set; }
        #endregion
    }

    [System.Diagnostics.DebuggerDisplay("[{ConfigurationID}:{Revision}] SourceLineNbr={SourceLineNbr}; LineNbr={LineNbr}")]
    [Serializable]
    [PXCacheName(Messages.ConfigurationFeatureRule)]
    [PXProjection(typeof(Select<AMConfigurationRule, Where<AMConfigurationRule.ruleSource, Equal<RuleTargetSource.feature>>>), Persistent = true)]
    public class AMConfigurationFeatureRule : IBqlTable
    {
        #region ConfigurationID
        public abstract class configurationID : PX.Data.BQL.BqlString.Field<configurationID> { }


        [PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(AMConfigurationRule.configurationID))]
        [PXDBDefault(typeof(AMConfigurationFeature.configurationID))]
        [PXUIField(Visible = false)]
        public virtual string ConfigurationID { get; set; }
        #endregion
        #region Revision
        public abstract class revision : PX.Data.BQL.BqlString.Field<revision> { }


        [PXDBString(10, IsUnicode = true, IsKey = true, BqlField = typeof(AMConfigurationRule.revision))]
        [PXUIField(Visible = false)]
        [PXDBDefault(typeof(AMConfigurationFeature.revision))]
        public virtual string Revision { get; set; }
        #endregion
        #region RuleSource
        public abstract class ruleSource : PX.Data.BQL.BqlString.Field<ruleSource> { }


        [PXDBString(1, IsFixed = true, IsKey = true, BqlField = typeof(AMConfigurationRule.ruleSource))]
        [PXDefault(RuleTargetSource.Feature)]
        [PXUIField(DisplayName = "Rule Source", Visible = false, Enabled = false)]
        public virtual string RuleSource { get; set; }
        #endregion
        #region SourceLineNbr
        public abstract class sourceLineNbr : PX.Data.BQL.BqlInt.Field<sourceLineNbr> { }


        [PXDBInt(IsKey = true, BqlField = typeof(AMConfigurationRule.sourceLineNbr))]
        [PXDBDefault(typeof(AMConfigurationFeature.lineNbr))]
        [PXParent(typeof(Select<AMConfigurationFeature,
                            Where<AMConfigurationFeature.configurationID,
                                Equal<Current<AMConfigurationFeatureRule.configurationID>>,
                            And<AMConfigurationFeature.revision,
                                Equal<Current<AMConfigurationFeatureRule.revision>>,
                            And<AMConfigurationFeature.lineNbr,
                                Equal<Current<AMConfigurationFeatureRule.sourceLineNbr>>>>>>))]
        [PXUIField(Visible = false)]
        public virtual int? SourceLineNbr { get; set; }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }


        [PXDBInt(IsKey = true, BqlField = typeof(AMConfigurationRule.lineNbr))]
        [PXDefault]
        [PXLineNbr(typeof(AMConfigurationFeature.lineCntrRule))]
        [PXUIField(Visible = false)]
        public virtual int? LineNbr { get; set; }
        #endregion
        #region RuleType
        public abstract class ruleType : PX.Data.BQL.BqlString.Field<ruleType> { }


        [PXDBString(1, IsFixed = true, BqlField = typeof(AMConfigurationRule.ruleType))]
        [PXDefault(RuleTypes.Include)]
        [PXUIField(DisplayName = "Rule")]
        [RuleTypes.ListNoValidate]
        public virtual string RuleType { get; set; }
        #endregion
        #region Condition
        public abstract class condition : PX.Data.BQL.BqlString.Field<condition> { }


        [PXDBString(2, InputMask = "", BqlField = typeof(AMConfigurationRule.condition))]
        [PXUIField(DisplayName = "Condition", Visible = false, Enabled = false)]
        [PXDefault("E")]
        public virtual string Condition { get; set; }
        #endregion
        #region IsExpression
        public abstract class isExpression : PX.Data.BQL.BqlBool.Field<isExpression> { }


        [PXDBBool(BqlField = typeof(AMConfigurationRule.isExpression))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "From Schema", Visible = false, Enabled = false)]
        public virtual bool? IsExpression { get; set; }
        #endregion
        #region SourceOptionLineNbr
        [PXInt]
        [PXUIField(DisplayName = "Source Option")]
        [PXSelector(typeof(Search<AMConfigurationOption.lineNbr,
                        Where<AMConfigurationOption.configurationID,
                            Equal<Current<AMConfigurationFeatureRule.configurationID>>,
                        And<AMConfigurationOption.revision,
                            Equal<Current<AMConfigurationFeatureRule.revision>>,
                        And<AMConfigurationOption.configFeatureLineNbr,
                            Equal<Current<AMConfigurationFeatureRule.sourceLineNbr>>>>>>),
            SubstituteKey = typeof(AMConfigurationOption.label), 
            DescriptionField = typeof(AMConfigurationOption.descr),
            DirtyRead = true)]
        [AMEmptySelectorValue(Messages.SelectorAny)]
        public virtual int? SourceOptionLineNbr
        {
            [PXDependsOnFields(typeof(value1))]
            get
            {
                int value1;
                if (int.TryParse(Value1, out value1))
                    return value1;
                return null;
            }
            set
            {
                Value1 = value.ToString();
            }
        }
        #endregion
        #region Value1
        public abstract class value1 : PX.Data.BQL.BqlString.Field<value1> { }


        [PXDBString(InputMask = "", IsUnicode = true, BqlField = typeof(AMConfigurationRule.value1))]
        [PXUIField(Visible = false, Enabled = false)]
        public virtual string Value1 { get; set; }
        #endregion
        #region Value2
        public abstract class value2 : PX.Data.BQL.BqlString.Field<value2> { }


        [PXDBString(InputMask = "", IsUnicode = true, BqlField = typeof(AMConfigurationRule.value2))]
        [PXUIField(Visible = false, Enabled = false)]
        public virtual string Value2 { get; set; }
        #endregion
        #region TargetFeatureLineNbr
        public abstract class targetFeatureLineNbr : PX.Data.BQL.BqlInt.Field<targetFeatureLineNbr> { }


        [PXDBInt(BqlField = typeof(AMConfigurationRule.targetFeatureLineNbr))]
        [PXDefault]
        [PXUIField(DisplayName = "Target Feature")]
        [PXSelector(typeof(Search<AMConfigurationFeature.lineNbr,
                                Where<AMConfigurationFeature.configurationID,
                                    Equal<Current<configurationID>>,
                                And<AMConfigurationFeature.revision,
                                    Equal<Current<revision>>>>>),
                    typeof(AMConfigurationFeature.featureID),
                    typeof(AMConfigurationFeature.label),
                    typeof(AMConfigurationFeature.descr),
                    SubstituteKey = typeof(AMConfigurationFeature.label), DirtyRead = true)]
        public virtual int? TargetFeatureLineNbr { get; set; }
        #endregion
        #region TargetOptionLineNbr
        public abstract class targetOptionLineNbr : PX.Data.BQL.BqlInt.Field<targetOptionLineNbr> { }


        [PXDBInt(BqlField = typeof(AMConfigurationRule.targetOptionLineNbr))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Target Option")]
        [PXSelector(typeof(Search<AMConfigurationOption.lineNbr,
                                Where<AMConfigurationOption.configurationID,
                                    Equal<Current<configurationID>>,
                                And<AMConfigurationOption.revision,
                                    Equal<Current<revision>>,
                                And<AMConfigurationOption.configFeatureLineNbr,
                                    Equal<Current<targetFeatureLineNbr>>>>>>),
            SubstituteKey = typeof(AMConfigurationOption.label),
            DescriptionField = typeof(AMConfigurationOption.descr),
            DirtyRead = true)]
        [AMEmptySelectorValue(Messages.SelectorAll)]
        public virtual int? TargetOptionLineNbr { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }


        [PXDBCreatedDateTime(BqlField = typeof(AMConfigurationRule.createdDateTime))]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }


        [PXDBCreatedByScreenID(BqlField = typeof(AMConfigurationRule.createdByScreenID))]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }


        [PXDBCreatedByID(BqlField = typeof(AMConfigurationRule.createdByID))]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }


        [PXDBLastModifiedDateTime(BqlField = typeof(AMConfigurationRule.lastModifiedDateTime))]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }


        [PXDBLastModifiedByScreenID(BqlField = typeof(AMConfigurationRule.lastModifiedByScreenID))]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }


        [PXDBLastModifiedByID(BqlField = typeof(AMConfigurationRule.lastModifiedByID))]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }


        [PXDBTimestamp(BqlField = typeof(AMConfigurationRule.Tstamp))]
        public virtual byte[] tstamp { get; set; }
        #endregion
    }
}