using System;
using System.Drawing;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Models.Db.Enums;

namespace WHMapper.Services.WHColor
{
    public class WHColorHelper : IWHColorHelper
    {
        private const float EPSILON = 0.0001f;

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

        private const string WH_CLASS_C1_COLOR = "#428bca";
        private const string WH_CLASS_C2_COLOR = "#428bca";
        private const string WH_CLASS_C3_COLOR = "#e28a0d";
        private const string WH_CLASS_C4_COLOR = "#e28a0d";
        private const string WH_CLASS_C5_COLOR = "#d9534f";
        private const string WH_CLASS_C6_COLOR = "#d9534f";
        private const string WH_CLASS_C13_COLOR = "#f2e9f0";
        private const string WH_CLASS_C14_COLOR = "#92ffde";
        private const string WH_CLASS_C15_COLOR = WH_CLASS_C14_COLOR;
        private const string WH_CLASS_C16_COLOR = WH_CLASS_C14_COLOR;
        private const string WH_CLASS_C17_COLOR = WH_CLASS_C14_COLOR;
        private const string WH_CLASS_C18_COLOR = WH_CLASS_C14_COLOR;
        private const string WH_CLASS_THERA_COLOR = "#fff952";
        private const string WH_CLASS_HS_COLOR = "#5cb85c";
        private const string WH_CLASS_LS_COLOR = "#e28a0d";
        private const string WH_CLASS_NS_COLOR = SECUTIRTY_STATUS_00_COLOR;
        private const string WH_CLASS_00_COLOR = SECUTIRTY_STATUS_00_COLOR;
        private const string WH_CLASS_POCHVEN_COLOR = "#b10c0c";

        private const string WH_IS_EOL_COLOR = "#d747d6";
        private const string WH_MASS_NORMAL_COLOR = "#3C3F41";
        private const string WH_MASS_CRITICAL_COLOR = "#e28a0d";
        private const string WH_MASS_VERGE_COLOR = "#a52521";

        private const string SELECTED_LINK_COLOR= "white";


        private const string NODE_STATUS_FRIENDLY_COLOR = "#428bca";
        private const string NODE_STATUS_OCCUPIED_COLOR = "#e28a0d";
        private const string NODE_STATUS_HOSTILE_COLOR = "#be0000";
        private const string NODE_STATUS_UNKNOWN_COLOR = IWHColorHelper.DEFAULT_COLOR;
        

        private bool FloatEquals(float a, float b)
        {
            return Math.Abs(a - b) < EPSILON;
        }


        public string GetNodeStatusColor(WHSystemStatusEnum status)
        {
            switch (status)
            {
                case WHSystemStatusEnum.Unknown:
                    return NODE_STATUS_UNKNOWN_COLOR;
                case WHSystemStatusEnum.Friendly:
                    return NODE_STATUS_FRIENDLY_COLOR;
                case WHSystemStatusEnum.Occupied:
                    return NODE_STATUS_OCCUPIED_COLOR;
                case WHSystemStatusEnum.Hostile:
                    return NODE_STATUS_HOSTILE_COLOR;
                default:
                    return IWHColorHelper.DEFAULT_COLOR;
            }
        }


        public string GetSecurityStatusColor(float secStatus)
        {
            float secStatusRounded = (float)Math.Round(secStatus, 1);
            if (secStatus<= -0.99)
                secStatusRounded= -1.0f;


            if (FloatEquals(secStatusRounded,SECUTIRTY_STATUS_10_VALUE))
                return SECUTIRTY_STATUS_10_COLOR;
            else if (FloatEquals(secStatusRounded,SECUTIRTY_STATUS_09_VALUE))
                return SECUTIRTY_STATUS_09_COLOR;
            else if (FloatEquals(secStatusRounded,SECUTIRTY_STATUS_08_VALUE))
                return SECUTIRTY_STATUS_08_COLOR;
            else if (FloatEquals(secStatusRounded,SECUTIRTY_STATUS_07_VALUE))
                return SECUTIRTY_STATUS_07_COLOR;
            else if (FloatEquals(secStatusRounded,SECUTIRTY_STATUS_06_VALUE))
                return SECUTIRTY_STATUS_06_COLOR;
            else if (FloatEquals(secStatusRounded,SECUTIRTY_STATUS_05_VALUE))
                return SECUTIRTY_STATUS_05_COLOR;
            else if (FloatEquals(secStatusRounded,SECUTIRTY_STATUS_04_VALUE))
                return SECUTIRTY_STATUS_04_COLOR;
            else if (FloatEquals(secStatusRounded,SECUTIRTY_STATUS_03_VALUE))
                return SECUTIRTY_STATUS_03_COLOR;
            else if (FloatEquals(secStatusRounded,SECUTIRTY_STATUS_02_VALUE))
                return SECUTIRTY_STATUS_02_COLOR;
            else if (FloatEquals(secStatusRounded,SECUTIRTY_STATUS_01_VALUE))
                return SECUTIRTY_STATUS_01_COLOR;
            else if (secStatusRounded <= SECUTIRTY_STATUS_00_VALUE)
                return SECUTIRTY_STATUS_00_COLOR;

            return IWHColorHelper.DEFAULT_COLOR;
        }

        public string GetSystemTypeColor(EveSystemType systemType)
        {
            switch (systemType)
            {
                case EveSystemType.HS:
                    return WH_CLASS_HS_COLOR;
                case EveSystemType.LS:
                    return WH_CLASS_LS_COLOR;
                case EveSystemType.NS:
                    return WH_CLASS_NS_COLOR;
                case EveSystemType.C1:
                    return WH_CLASS_C1_COLOR;
                case EveSystemType.C2:
                    return WH_CLASS_C2_COLOR;
                case EveSystemType.C3:
                    return WH_CLASS_C3_COLOR;
                case EveSystemType.C4:
                    return WH_CLASS_C4_COLOR;
                case EveSystemType.C5:
                    return WH_CLASS_C5_COLOR;
                case EveSystemType.C6:
                    return WH_CLASS_C6_COLOR;
                case EveSystemType.C13:
                    return WH_CLASS_C13_COLOR;
                case EveSystemType.C14:
                    return WH_CLASS_C14_COLOR;
                case EveSystemType.C15:
                    return WH_CLASS_C15_COLOR;
                case EveSystemType.C16:
                    return WH_CLASS_C16_COLOR;
                case EveSystemType.C17:
                    return WH_CLASS_C17_COLOR;
                case EveSystemType.C18:
                    return WH_CLASS_C18_COLOR;
                case EveSystemType.Thera:
                    return WH_CLASS_THERA_COLOR;
                case EveSystemType.Pochven:
                    return WH_CLASS_POCHVEN_COLOR;
                default:
                    return IWHColorHelper.DEFAULT_COLOR;
            }
        }

        public string GetEffectColor(WHEffect effect)
        {
            switch(effect)
            {
                case WHEffect.Magnetar:
                    return MAGNETAR_COLOR;
                case WHEffect.RedGiant:
                    return REDGIANT_COLOR;
                case WHEffect.Pulsar:
                    return PULSAR_COLOR;
                case WHEffect.WolfRayet:
                    return WOLFRAYER_COLOR;
                case WHEffect.BlackHole:
                    return BLACKHOLE_COLOR;
                case WHEffect.Cataclysmic:
                    return CATACLYSMIC_COLOR;
                default:
                    return IWHColorHelper.DEFAULT_COLOR;
            }
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

