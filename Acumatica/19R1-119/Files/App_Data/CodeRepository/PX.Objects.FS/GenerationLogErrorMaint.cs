﻿using System.Collections;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.FS
{
    public class GenerationLogErrorMaint : PXGraph<GenerationLogErrorMaint, FSGenerationLogError>
    {
        [PXImport(typeof(FSGenerationLogError))]
        public PXSelect<FSGenerationLogError> LogErrorMessageRecords;
    }
}