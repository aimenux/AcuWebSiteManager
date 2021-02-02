using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AP.InvoiceRecognition.DAC
{
    [PXInternalUseOnly]
    public sealed class EMailAccountExt : PXCacheExtension<EMailAccount>
    {
        [PXDBBool]
        [PXUIField(DisplayName = "Submit to Incoming Documents")]
        public bool? SubmitToIncomingAPDocuments { get; set; }
        public abstract class submitToIncomingAPDocuments : BqlBool.Field<submitToIncomingAPDocuments> { }
    }
}
