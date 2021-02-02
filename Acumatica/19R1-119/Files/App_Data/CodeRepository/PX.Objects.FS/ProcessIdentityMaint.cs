﻿using System.Collections;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.FS
{
    public class ProcessIdentityMaint : PXGraph<ProcessIdentityMaint, FSProcessIdentity>
    {
        [PXImport(typeof(FSProcessIdentity))]
        public PXSelect<FSProcessIdentity> processIdentityRecords;
    }
}