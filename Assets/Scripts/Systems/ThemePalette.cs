using UnityEngine;

namespace NooMiBetter
{
    public static class ThemePalette
    {
        public static Color Accent {
            get {
                int index = PlayerPrefs.GetInt("nmb_theme", 0);
                switch (index) {
                    case 1: return new Color(1f,.35f,.48f);
                    case 2: return new Color(.35f,1f,.68f);
                    case 3: return new Color(1f,.72f,.25f);
                    default: return new Color(.25f,.78f,1f);
                }
            }
        }
        public static void Select(int index) {
            PlayerPrefs.SetInt("nmb_theme", Mathf.Clamp(index, 0, 3));
            PlayerPrefs.Save();
        }
    }
}
