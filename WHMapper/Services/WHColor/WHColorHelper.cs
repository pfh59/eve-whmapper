using System;
using System.Drawing;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Services.WHColor
{
    public class WHColorHelper : IWHColorHelper
    {

        private const string WH_MAGNETAR = "Magnetar";
        private const string WH_REDGIANT = "Red Giant";
        private const string WH_PULSAR = "Pulsar";
        private const string WH_WOLFRAYET = "Wolf-Rayet Star";
        private const string WH_CATACLYSMIC= "Cataclysmic Variable";
        private const string WH_BLACKHOLE = "Black Hole";


        private const string MAGNETAR_COLOR = "#e06fdf";
        private const string REDGIANT_COLOR = "#d9534f";
        private const string PULSAR_COLOR = "#428bca";
        private const string WOLFRAYER_COLOR = "#e28a0d";
        private const string CATACLYSMIC_COLOR = "#ffffbb";
        private const string BLACKHOLE_COLOR = "black";


        private const float SECUTIRTY_STATUS_00_VALUE = ((float)0.0);
        private const float SECUTIRTY_STATUS_01_VALUE = ((float)0.1);
        private const float SECUTIRTY_STATUS_02_VALUE = ((float)0.2);
        private const float SECUTIRTY_STATUS_03_VALUE = ((float)0.3);
        private const float SECUTIRTY_STATUS_04_VALUE = ((float)0.4);
        private const float SECUTIRTY_STATUS_05_VALUE = ((float)0.5);
        private const float SECUTIRTY_STATUS_06_VALUE = ((float)0.6);
        private const float SECUTIRTY_STATUS_07_VALUE = ((float)0.7);
        private const float SECUTIRTY_STATUS_08_VALUE = ((float)0.8);
        private const float SECUTIRTY_STATUS_09_VALUE = ((float)0.9);
        private const float SECUTIRTY_STATUS_10_VALUE = ((float)1.0);

        private const string SECUTIRTY_STATUS_00_COLOR = "#be0000";
        private const string SECUTIRTY_STATUS_01_COLOR = "#ab2600";
        private const string SECUTIRTY_STATUS_02_COLOR = "#be3900";
        private const string SECUTIRTY_STATUS_03_COLOR = "#c24e02";
        private const string SECUTIRTY_STATUS_04_COLOR = "#ab5f00";
        private const string SECUTIRTY_STATUS_05_COLOR = "#bebe00";
        private const string SECUTIRTY_STATUS_06_COLOR = "#73bf26";
        private const string SECUTIRTY_STATUS_07_COLOR = "#00bf00";
        private const string SECUTIRTY_STATUS_08_COLOR = "#00bf39";
        private const string SECUTIRTY_STATUS_09_COLOR = "#39bf99";
        private const string SECUTIRTY_STATUS_10_COLOR = "#28c0bf";


        private const string WH_CLASS_C1_VALUE = "C1";
        private const string WH_CLASS_C2_VALUE = "C2";
        private const string WH_CLASS_C3_VALUE = "C3";
        private const string WH_CLASS_C4_VALUE = "C4";
        private const string WH_CLASS_C5_VALUE = "C5";
        private const string WH_CLASS_C6_VALUE = "C6";
        private const string WH_CLASS_HS_VALUE = "H";
        private const string WH_CLASS_LS_VALUE = "L";
        private const string WH_CLASS_NS_VALUE = "NS";
        private const string WH_CLASS_00_VALUE = "0.0";

        private const string WH_CLASS_C1_COLOR = "#428bca";
        private const string WH_CLASS_C2_COLOR = "#428bca";
        private const string WH_CLASS_C3_COLOR = "#e28a0d";
        private const string WH_CLASS_C4_COLOR = "#e28a0d";
        private const string WH_CLASS_C5_COLOR = "#d9534f";
        private const string WH_CLASS_C6_COLOR = "#d9534f";
        private const string WH_CLASS_HS_COLOR = "#5cb85c";
        private const string WH_CLASS_LS_COLOR = "#e28a0d";
        private const string WH_CLASS_NS_COLOR = SECUTIRTY_STATUS_00_COLOR;
        private const string WH_CLASS_00_COLOR = SECUTIRTY_STATUS_00_COLOR;



        private const string WH_IS_EOL_COLOR = "#d747d6";
        private const string WH_MASS_NORMAL_COLOR = "#3C3F41";
        private const string WH_MASS_CRITICAL_COLOR = "#e28a0d";
        private const string WH_MASS_VERGE_COLOR = "#a52521";

        private const string SELECTED_LINK_COLOR= "white";

        public string GetSecurityStatusColor(float secStatus)
        {
            if (secStatus == SECUTIRTY_STATUS_10_VALUE)
                return SECUTIRTY_STATUS_10_COLOR;
            else if (secStatus == SECUTIRTY_STATUS_09_VALUE)
                return SECUTIRTY_STATUS_09_COLOR;
            else if (secStatus == SECUTIRTY_STATUS_08_VALUE)
                return SECUTIRTY_STATUS_08_COLOR;
            else if (secStatus == SECUTIRTY_STATUS_07_VALUE)
                return SECUTIRTY_STATUS_07_COLOR;
            else if (secStatus == SECUTIRTY_STATUS_06_VALUE)
                return SECUTIRTY_STATUS_06_COLOR;
            else if (secStatus == SECUTIRTY_STATUS_05_VALUE)
                return SECUTIRTY_STATUS_05_COLOR;
            else if (secStatus == SECUTIRTY_STATUS_04_VALUE)
                return SECUTIRTY_STATUS_04_COLOR;
            else if (secStatus == SECUTIRTY_STATUS_03_VALUE)
                return SECUTIRTY_STATUS_03_COLOR;
            else if (secStatus == SECUTIRTY_STATUS_02_VALUE)
                return SECUTIRTY_STATUS_02_COLOR;
            else if (secStatus == SECUTIRTY_STATUS_01_VALUE)
                return SECUTIRTY_STATUS_01_COLOR;
            else if (secStatus <= SECUTIRTY_STATUS_00_VALUE)
                return SECUTIRTY_STATUS_00_COLOR;

            return string.Empty;
        }

        public string GetSystemTypeColor(string systemType)
        {
            if (systemType.Contains(WH_CLASS_C1_VALUE))
                return WH_CLASS_C1_COLOR;
            else if (systemType.Contains(WH_CLASS_C2_VALUE))
                return WH_CLASS_C2_COLOR;
            else if (systemType.Contains(WH_CLASS_C3_VALUE))
                return WH_CLASS_C3_COLOR;
            else if (systemType.Contains(WH_CLASS_C4_VALUE))
                return WH_CLASS_C4_COLOR;
            else if (systemType.Contains(WH_CLASS_C5_VALUE))
                return WH_CLASS_C5_COLOR;
            else if (systemType.Contains(WH_CLASS_C6_VALUE))
                return WH_CLASS_C6_COLOR;
            else if (systemType.Contains(WH_CLASS_HS_VALUE))
                return WH_CLASS_HS_COLOR;
            else if (systemType.Contains(WH_CLASS_LS_VALUE))
                return WH_CLASS_LS_COLOR;
            else if (systemType.Contains(WH_CLASS_NS_VALUE) || systemType.Contains(WH_CLASS_00_VALUE))
                return WH_CLASS_NS_COLOR;

            return String.Empty;
            
        }

        public string GetEffectColor(string effectName)
        {
            if (effectName == WH_PULSAR)
                return PULSAR_COLOR;
            else if (effectName == WH_REDGIANT)
                return REDGIANT_COLOR;
            else if (effectName == WH_BLACKHOLE)
                return BLACKHOLE_COLOR;
            else if (effectName == WH_MAGNETAR)
                return  MAGNETAR_COLOR;
            else if (effectName == WH_WOLFRAYET)
                return  WOLFRAYER_COLOR;
            else if (effectName == WH_CATACLYSMIC)
                return  CATACLYSMIC_COLOR;

            return String.Empty;
        }


        #region System Link Color
        public string GetLinkEOLColor()
        {
            return WH_IS_EOL_COLOR;
        }


        public string GetLinkStatusColor(SystemLinkMassStatus status)
        {
            switch (status)
            {
                case SystemLinkMassStatus.Normal:
                    return WH_MASS_NORMAL_COLOR;
                case SystemLinkMassStatus.Critical:
                    return WH_MASS_CRITICAL_COLOR;
                case SystemLinkMassStatus.Verge:
                    return WH_MASS_VERGE_COLOR;
            }

            return WH_MASS_NORMAL_COLOR;
        }

        public string GetLinkSelectedColor()
        {
            return SELECTED_LINK_COLOR;
        }
        #endregion


    }
}

