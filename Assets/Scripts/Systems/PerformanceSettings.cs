using UnityEngine;

namespace NooMiBetter
{
    public static class PerformanceSettings
    {
        public static void ApplyLowPower(bool enabled) {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = enabled ? 30 : 60;
            QualitySettings.shadows = enabled ? ShadowQuality.Disable : ShadowQuality.HardOnly;
            QualitySettings.antiAliasing = enabled ? 0 : 2;
            PlayerPrefs.SetInt("nmb_low_power", enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
