namespace PX.Objects.AM.Upgrade
{
    /// <summary>
    /// Upgrade status attribute for the Manufacturing modules
    /// </summary>
    public static class UpgradeVersions
    {
        public const int CurrentVersion = Version2020R1Ver26;

        // ***************************************************************
        // Version Number Format 17.200.04 = [17][200][0004] = 172000004
        // ***************************************************************

        /// <summary>
        /// No upgrade has been performed
        /// </summary>
        public const int NoUpgrade = 0;

        /// <summary>
        /// Upgrade to 2017R2 release
        /// </summary>
        public const int Version2017R2Ver00 = 172000000;
        public const int Version2017R2Ver01 = 172000001;
        public const int Version2017R2Ver12 = 172000012;
        public const int Version2017R2Ver21 = 172010021;
        /// <summary>
        /// Regenerate MRP screen changes
        /// </summary>
        public const int Version2017R2Ver28 = 172060028;
        /// <summary>
        /// Orphan Configuration Rules Delete (2017R2)
        /// </summary>
        public const int Version2017R2Ver32 = 172080032;
        /// <summary>
        /// Missed RTrim fields (2017R2)
        /// </summary>
        public const int Version2017R2Ver38 = 172080038;
        /// <summary>
        /// New processing pages and data fixes related to bugs
        /// </summary>
        public const int Version2017R2Ver49 = 172080049;
        public const int Version2017R2Ver50 = 172080050;

        /// <summary>
        /// APS release version for 2017R2 (FP)
        /// </summary>
        public const int Version2017R2Ver1001 = 172081001;
        public const int Version2017R2Ver1015 = 172081015;

        /// <summary>
        /// Release 2018R1 updates
        /// </summary>
        public const int Version2018R1Ver00 = 181000003;
        /// <summary>
        /// Orphan Configuration Rules Delete (2018R1)
        /// </summary>
        public const int Version2018R1Ver05 = 181000005;
        /// <summary>
        /// Missed RTrim fields (Missed in 2017R2 upgrade - however possible in 2018R1 due to released before fix applied)
        /// </summary>
        public const int Version2018R1Ver20 = 181040020;
        /// <summary>
        /// New processing pages and data fixes related to bugs
        /// </summary>
        public const int Version2018R1Ver27 = 181040027;

        public const int Version2018R1Ver29 = 181080029;

        /// <summary>
        /// APS release version for 2018R1 (FP)
        /// </summary>
        public const int Version2018R1Ver1000 = 181121000;
        public const int Version2018R1Ver1006 = 181121006;
        public const int Version2018R1Ver1013 = 181121013;
        public const int Version2018R1Ver1016 = 181121016;
        public const int Version2018R1Ver1024 = 181121024;

        /// <summary>
        /// Release 2018R2 updates
        /// </summary>
        public const int Version2018R2Ver00 = 182000000;
        public const int Version2018R2Ver03 = 182000003;
        public const int Version2018R2Ver17 = 182000017;
        public const int Version2018R2Ver21 = 182000021;
        public const int Version2018R2Ver26 = 182000026;
        public const int Version2018R2Ver43 = 182000043;
        public const int Version2018R2Ver53 = 182000053;
        public const int Version2018R2Ver58 = 182000058;
        public const int Version2018R2Ver82 = 182000082;

        //Need to update to the release version of APS2 in 18R2
        public const int Version2018R2Ver34 = 182000034;

        public const int Version2019R1Ver00 = 191000000;
        public const int Version2019R1Ver01 = 191000001;
        public const int Version2019R1Ver02 = 191000002;
        public const int Version2019R1Ver03 = 191000003;
        public const int Version2019R1Ver31 = 191000031;
        public const int Version2019R1Ver52 = 191000052;

        /// <summary>
        /// Release 2019R2
        /// </summary>
        public const int Version2019R2Ver07 = 192000007;
        public const int Version2019R2Ver08 = 192000008;
        public const int Version2019R2Ver61 = 192000061;

        public const int Version2020R1Ver26 = 201000026;

        /// <summary>
        /// No upgrade has been performed
        /// </summary>
        public class noUpgrade : PX.Data.BQL.BqlInt.Constant<noUpgrade>
        {
            public noUpgrade()
                : base(NoUpgrade)
            {
            }
        }

        public static class MaxVersionNumbers
        {
            public const int Version2017R2 = 172999999;
            public const int Version2018R1 = 181999999;
            public const int Version2018R2 = 182999999;
            public const int Version2019R1 = 191999999;
            public const int Version2019R2 = 192999999;
        }
    }
}