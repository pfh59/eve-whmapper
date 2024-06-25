﻿using System;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Services.EveAPI.Universe;
using WHMapper.Services.WHColor;
using Xunit.Priority;

namespace WHMapper.Tests.WHHelper;


[Collection("C8-WHHelper")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class WHColorHelperTest
{
    private const string WH_MAGNETAR = "Magnetar";
    private const string WH_REDGIANT = "Red Giant";
    private const string WH_PULSAR = "Pulsar";
    private const string WH_WOLFRAYET = "Wolf-Rayet Star";
    private const string WH_CATACLYSMIC = "Cataclysmic Variable";
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


    private const string WH_CLASS_C1_COLOR = "#428bca";
    private const string WH_CLASS_C2_COLOR = "#428bca";
    private const string WH_CLASS_C3_COLOR = "#e28a0d";
    private const string WH_CLASS_C4_COLOR = "#e28a0d";
    private const string WH_CLASS_C5_COLOR = "#d9534f";
    private const string WH_CLASS_C6_COLOR = "#d9534f";
    private const string WH_CLASS_HS_COLOR = "#5cb85c";
    private const string WH_CLASS_LS_COLOR = "#e28a0d";
    private const string WH_CLASS_C13_COLOR = "#f2e9f0";
    private const string WH_CLASS_C14_COLOR = "#92ffde";
    private const string WH_CLASS_C15_COLOR = WH_CLASS_C14_COLOR;
    private const string WH_CLASS_C16_COLOR = WH_CLASS_C14_COLOR;
    private const string WH_CLASS_C17_COLOR = WH_CLASS_C14_COLOR;
    private const string WH_CLASS_C18_COLOR = WH_CLASS_C14_COLOR;
    private const string WH_CLASS_THERA_COLOR = "#fff952";
    private const string WH_CLASS_NS_COLOR = SECUTIRTY_STATUS_00_COLOR;
    private const string WH_CLASS_00_COLOR = SECUTIRTY_STATUS_00_COLOR;
    private const string WH_CLASS_POCHVEN_COLOR = "#b10c0c";

    private const string WH_IS_EOL_COLOR = "#d747d6";
    private const string WH_MASS_NORMAL_COLOR = "#3C3F41";
    private const string WH_MASS_CRITICAL_COLOR = "#e28a0d";
    private const string WH_MASS_VERGE_COLOR = "#a52521";

    private const string SELECTED_LINK_COLOR = "white";

    private const string NODE_STATUS_FRIENDLY_COLOR = "#428bca";
    private const string NODE_STATUS_OCCUPIED_COLOR = "#e28a0d";
    private const string NODE_STATUS_HOSTILE_COLOR = "#be0000";
    private const string NODE_STATUS_UNKNOWN_COLOR = IWHColorHelper.DEFAULT_COLOR;

    private IWHColorHelper _whHelper;

    public WHColorHelperTest()
    {
        _whHelper = new WHMapper.Services.WHColor.WHColorHelper();
    }

    [Fact]
    public void Get_Security_Status_Color()
    {
        Assert.Equal(SECUTIRTY_STATUS_00_COLOR, _whHelper.GetSecurityStatusColor(SECUTIRTY_STATUS_00_VALUE));
        Assert.Equal(SECUTIRTY_STATUS_01_COLOR, _whHelper.GetSecurityStatusColor(SECUTIRTY_STATUS_01_VALUE));
        Assert.Equal(SECUTIRTY_STATUS_02_COLOR, _whHelper.GetSecurityStatusColor(SECUTIRTY_STATUS_02_VALUE));
        Assert.Equal(SECUTIRTY_STATUS_03_COLOR, _whHelper.GetSecurityStatusColor(SECUTIRTY_STATUS_03_VALUE));
        Assert.Equal(SECUTIRTY_STATUS_04_COLOR, _whHelper.GetSecurityStatusColor(SECUTIRTY_STATUS_04_VALUE));
        Assert.Equal(SECUTIRTY_STATUS_05_COLOR, _whHelper.GetSecurityStatusColor(SECUTIRTY_STATUS_05_VALUE));
        Assert.Equal(SECUTIRTY_STATUS_06_COLOR, _whHelper.GetSecurityStatusColor(SECUTIRTY_STATUS_06_VALUE));
        Assert.Equal(SECUTIRTY_STATUS_07_COLOR, _whHelper.GetSecurityStatusColor(SECUTIRTY_STATUS_07_VALUE));
        Assert.Equal(SECUTIRTY_STATUS_08_COLOR, _whHelper.GetSecurityStatusColor(SECUTIRTY_STATUS_08_VALUE));
        Assert.Equal(SECUTIRTY_STATUS_09_COLOR, _whHelper.GetSecurityStatusColor(SECUTIRTY_STATUS_09_VALUE));
        Assert.Equal(SECUTIRTY_STATUS_10_COLOR, _whHelper.GetSecurityStatusColor(SECUTIRTY_STATUS_10_VALUE));
        Assert.Equal(IWHColorHelper.DEFAULT_COLOR, _whHelper.GetSecurityStatusColor(28));

    }

    [Fact]
    public void Get_System_Type_Color()
    {
        Assert.Equal(WH_CLASS_C1_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.C1));
        Assert.Equal(WH_CLASS_C2_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.C2));
        Assert.Equal(WH_CLASS_C3_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.C3));
        Assert.Equal(WH_CLASS_C4_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.C4));
        Assert.Equal(WH_CLASS_C5_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.C5));
        Assert.Equal(WH_CLASS_C6_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.C6));
        Assert.Equal(WH_CLASS_C13_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.C13));
        Assert.Equal(WH_CLASS_C14_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.C14));
        Assert.Equal(WH_CLASS_C15_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.C15));
        Assert.Equal(WH_CLASS_C16_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.C16));
        Assert.Equal(WH_CLASS_C17_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.C17));
        Assert.Equal(WH_CLASS_C18_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.C18));

        Assert.Equal(WH_CLASS_HS_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.HS));
        Assert.Equal(WH_CLASS_LS_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.LS));
        Assert.Equal(WH_CLASS_NS_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.NS));
        Assert.Equal(WH_CLASS_THERA_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.Thera));
        Assert.Equal(WH_CLASS_POCHVEN_COLOR, _whHelper.GetSystemTypeColor(EveSystemType.Pochven));
    }

    [Fact]
    public void Get_Effect_Color()
    {
        Assert.Equal(MAGNETAR_COLOR, _whHelper.GetEffectColor(WHEffect.Magnetar));
        Assert.Equal(REDGIANT_COLOR, _whHelper.GetEffectColor(WHEffect.RedGiant));
        Assert.Equal(PULSAR_COLOR, _whHelper.GetEffectColor(WHEffect.Pulsar));
        Assert.Equal(WOLFRAYER_COLOR, _whHelper.GetEffectColor(WHEffect.WolfRayet));
        Assert.Equal(CATACLYSMIC_COLOR, _whHelper.GetEffectColor(WHEffect.Cataclysmic));
        Assert.Equal(BLACKHOLE_COLOR, _whHelper.GetEffectColor(WHEffect.BlackHole));
        Assert.Equal(IWHColorHelper.DEFAULT_COLOR, _whHelper.GetEffectColor(WHEffect.None));
    }

    [Fact]
    public void Get_Link_EOL_Color()
    {
        Assert.Equal(WH_IS_EOL_COLOR, _whHelper.GetLinkEOLColor());
    }

    [Fact]
    public void Get_Link_Status_Color()
    {
        Assert.Equal(WH_MASS_NORMAL_COLOR, _whHelper.GetLinkStatusColor(SystemLinkMassStatus.Normal));
        Assert.Equal(WH_MASS_CRITICAL_COLOR, _whHelper.GetLinkStatusColor(SystemLinkMassStatus.Critical));
        Assert.Equal(WH_MASS_VERGE_COLOR, _whHelper.GetLinkStatusColor(SystemLinkMassStatus.Verge));
        Assert.Equal(WH_MASS_NORMAL_COLOR, _whHelper.GetLinkStatusColor((SystemLinkMassStatus)212));
    }

    [Fact]
    public void Get_Link_Selected_Color()
    {
        Assert.Equal(SELECTED_LINK_COLOR, _whHelper.GetLinkSelectedColor());
    }

    [Fact]
    public void Get_Node_Status_Color()
    {
        Assert.Equal(NODE_STATUS_FRIENDLY_COLOR, _whHelper.GetNodeStatusColor(WHSystemStatus.Friendly));
        Assert.Equal(NODE_STATUS_OCCUPIED_COLOR, _whHelper.GetNodeStatusColor(WHSystemStatus.Occupied));
        Assert.Equal(NODE_STATUS_HOSTILE_COLOR, _whHelper.GetNodeStatusColor(WHSystemStatus.Hostile));
        Assert.Equal(NODE_STATUS_UNKNOWN_COLOR, _whHelper.GetNodeStatusColor(WHSystemStatus.Unknown));
        Assert.Equal(IWHColorHelper.DEFAULT_COLOR, _whHelper.GetNodeStatusColor((WHSystemStatus)28));
    }
}



