using System.Collections.Generic;

namespace PX.Objects.PR.AUF
{
	public enum AufRecordType
	{
		Ver,
		Dat,
		Cmp,
		Emp,
		Cji,
		Gto,
		Gen,
		Ejw,
		Pim,
		Csi,
		Cli,
		Esi,
		Eli,
		Csp,
		Clp,
		Cfb,
		Efb,
		Ale,
		Agg,
		Dge,
		Ecv,
		Eci
	}

	public static class AufConstants
	{
		public static Dictionary<AufRecordType, string> RecordNames = new Dictionary<AufRecordType, string>()
		{
			{ AufRecordType.Ver, "VER" },
			{ AufRecordType.Dat, "DAT" },
			{ AufRecordType.Cmp, "CMP" },
			{ AufRecordType.Emp, "EMP" },
			{ AufRecordType.Cji, "CJI" },
			{ AufRecordType.Gto, "GTO" },
			{ AufRecordType.Gen, "GEN" },
			{ AufRecordType.Ejw, "EJW" },
			{ AufRecordType.Pim, "PIM" },
			{ AufRecordType.Csi, "CSI" },
			{ AufRecordType.Cli, "CLI" },
			{ AufRecordType.Esi, "ESI" },
			{ AufRecordType.Eli, "ELI" },
			{ AufRecordType.Csp, "CSP" },
			{ AufRecordType.Clp, "CLP" },
			{ AufRecordType.Cfb, "CFB" },
			{ AufRecordType.Efb, "EFB" },
			{ AufRecordType.Ale, "ALE" },
			{ AufRecordType.Agg, "AGG" },
			{ AufRecordType.Dge, "DGE" },
			{ AufRecordType.Ecv, "ECV" },
			{ AufRecordType.Eci, "ECI" }
		};

		public const object UnusedField = null;
		public const object ManualInput = null;

		public const char DefaultDelimiterCharacter = '\t';
		public const string DefaultEndline = "\r\n";

		#region VER records
		public const string VendorName = "Acumatica";
		public const string AufVersionNumber = "2.76";
		public const string SourceProgram = "Acumatica The Cloud ERP";
		#endregion
	}
}
