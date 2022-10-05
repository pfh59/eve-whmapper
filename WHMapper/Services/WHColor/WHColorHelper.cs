using System;
using System.Drawing;

namespace WHMapper.Services.WHColor
{
    public class WHColorHelper : IWHColorHelper
    {
        private const string _magnetar = "#e06fdf";
        private const string _redgiant = "#d9534f";
        private const string _pulsar = "#428bca";
        private const string _wolfrayef = "#e28a0d";
        private const string _cataclysmic = "#ffffbb";
        private const string _blackhole = "black";


        private const string _sec00 = "#be0000";
        private const string _sec01 = "#ab2600";
        private const string _sec02 = "#be3900";
        private const string _sec03 = "#c24e02";
        private const string _sec04 = "#ab5f00";
        private const string _sec05 = "#bebe00";
        private const string _sec06 = "#73bf26";
        private const string _sec07 = "#00bf00";
        private const string _sec08 = "#00bf39";
        private const string _sec09 = "#39bf99";
        private const string _sec10 = "#28c0bf";



        public string GetSecurityStatusColor(float secStatus)
        {
            if (secStatus == (float)1.0)
                return _sec10;
            else if (secStatus == (float)0.9)
                return _sec09;
            else if (secStatus == (float)0.8)
                return _sec08;
            else if (secStatus == (float)0.7)
                return _sec07;
            else if (secStatus == (float)0.6)
                return _sec06;
            else if (secStatus == (float)0.5)
                return _sec05;
            else if (secStatus == (float)0.4)
                return _sec04;
            else if (secStatus == (float)0.3)
                return _sec03;
            else if (secStatus == (float)0.2)
                return _sec02;
            else if (secStatus == (float)0.1)
                return _sec01;
            else if (secStatus <= (float)0)
                return _sec00;

            return string.Empty;
        }


        public string GetSystemTypeColor(string systemType)
        {
            if (systemType == "C1")
                return "#428bca";
            else if (systemType == "C2")
                return "#428bca";
            else if (systemType == "C3")
                return "#e28a0d";
            else if (systemType == "C4")
                return "#e28a0d";
            else if (systemType == "C5")
                return "#d9534f";
            else if (systemType == "C6")
                return "#d9534f";
            else if (systemType == "H" || systemType == "HS")
                return "#5cb85c";
            else if (systemType == "LS" || systemType == "L")
                return "#e28a0d";
            else if (systemType == "0.0" || systemType == "NS")
                return "e06fdf";

            return String.Empty;
            
        }

        public string GetEffectColor(string effectName)
        {
            if (effectName == "Pulsar")
                return _pulsar;
            else if (effectName == "Red Giant")
                return _redgiant;
            else if (effectName == "Black Hole")
                return  _blackhole;
            else if (effectName == "Magnetar")
                return  _magnetar;
            else if (effectName == "Wolf-Rayet Star")
                return  _wolfrayef;
            else if (effectName == "Cataclysmic Variable")
                return  _cataclysmic;

            return String.Empty;
        }
    }
}

