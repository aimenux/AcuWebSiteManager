using System;

namespace PX.Objects.AM
{
    public interface IMfgConfigOrderLineExtension
    {
        Int32? AMParentLineNbr { get; set; }
        Boolean? AMIsSupplemental { get; set; }
        string AMConfigurationID { get; set; }
        string AMConfigKeyID { get; set; }
        bool? IsConfigurable { get; }
    }
}