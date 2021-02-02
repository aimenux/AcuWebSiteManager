using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.CA
{
	public class PXModule
	{
		public const string AR = "AR";
		public const string AP = "AP";
		public const string CR = "CR";
		public const string PO = "PO";
		public const string SO = "SO";
		public const string RQ = "RQ";
		public const string PM = "PM";
		public const string IN = "IN";
		public const string Sc = "SC";
		public const string Cl = "CL";

		public class ar : PX.Data.BQL.BqlString.Constant<ar>
		{
			public ar() : base(AR) { }
		}

		public class ap : PX.Data.BQL.BqlString.Constant<ap>
		{
			public ap() : base(AP) { }
		}

		public class cr : PX.Data.BQL.BqlString.Constant<cr>
		{
			public cr() : base(CR) { }
		}

		public class po : PX.Data.BQL.BqlString.Constant<po>
		{
			public po() : base(PO) { }
		}

		public class sc : PX.Data.BQL.BqlString.Constant<sc>
		{
			public sc()
				: base(Sc)
			{
			}
		}

		public class cl : BqlString.Constant<cl>
		{
			public cl()
				: base(Cl)
			{
			}
		}

		public class so : PX.Data.BQL.BqlString.Constant<so>
		{
			public so() : base(SO) { }
		}

		public class rq : PX.Data.BQL.BqlString.Constant<rq>
		{
			public rq() : base(RQ) { }
		}

		public class pm : PX.Data.BQL.BqlString.Constant<pm>
		{
			public pm() : base(PM) { }
		}

		public class @in : PX.Data.BQL.BqlString.Constant<@in>
		{
			public @in() : base(IN) { }
		}

		public class ar_ : PX.Data.BQL.BqlString.Constant<ar_>
		{
			public ar_() : base(AR + "%") { }
		}

		public class ap_ : PX.Data.BQL.BqlString.Constant<ap_>
		{
			public ap_() : base(AP + "%") { }
		}

		public class cr_ : PX.Data.BQL.BqlString.Constant<cr_>
		{
			public cr_() : base(CR + "%") { }
		}

		public class po_ : PX.Data.BQL.BqlString.Constant<po_>
		{
			public po_() : base(PO + "%") { }
		}

		public class sc_ : PX.Data.BQL.BqlString.Constant<sc_>
		{
			public sc_()
				: base($"{Sc}%")
			{
			}
		}

		public class cl_ : BqlString.Constant<cl_>
		{
			public cl_()
				: base($"{Cl}%")
			{
			}
		}

		public class so_ : PX.Data.BQL.BqlString.Constant<so_>
		{
			public so_() : base(SO + "%") { }
		}

		public class rq_ : PX.Data.BQL.BqlString.Constant<rq_>
		{
			public rq_() : base(RQ + "%") { }
		}

		public class pm_ : PX.Data.BQL.BqlString.Constant<pm_>
		{
			public pm_() : base(PM + "%") { }
		}

		public class in_ : PX.Data.BQL.BqlString.Constant<in_>
		{
			public in_() : base(IN + "%") { }
		}

		public class namespaceAR : PX.Data.BQL.BqlString.Constant<namespaceAR>
		{
			public namespaceAR() : base("PX.Objects.AR.Reports") { }
		}

		public class namespaceAP : PX.Data.BQL.BqlString.Constant<namespaceAP>
		{
			public namespaceAP() : base("PX.Objects.AP.Reports") { }
		}

		public class namespacePO : PX.Data.BQL.BqlString.Constant<namespacePO>
		{
			public namespacePO() : base("PX.Objects.PO.Reports") { }
		}

		public class namespaceSO : PX.Data.BQL.BqlString.Constant<namespaceSO>
		{
			public namespaceSO() : base("PX.Objects.SO.Reports") { }
		}

		public class namespaceCR : PX.Data.BQL.BqlString.Constant<namespaceCR>
		{
			public namespaceCR() : base("PX.Objects.CR.Reports") { }
		}

		public class namespaceRQ : PX.Data.BQL.BqlString.Constant<namespaceRQ>
		{
			public namespaceRQ() : base("PX.Objects.RQ.Reports") { }
		}

		public class namespaceIN : Constant<string>
		{
			public namespaceIN() : base("PX.Objects.IN.Reports") { }
		}
	}
}
