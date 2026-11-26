using UnityEngine;

namespace NooMiBetter
{
    public static class LocalProfile
    {
        public static int Coins { get { return PlayerPrefs.GetInt("nmb_coins", 0); } }
        public static void AddCoins(int amount) {
            PlayerPrefs.SetInt("nmb_coins", Mathf.Max(0, Coins + amount));
            PlayerPrefs.Save();
        }
        public static bool SpendCoins(int amount) {
            if (amount < 0 || Coins < amount) return false;
            PlayerPrefs.SetInt("nmb_coins", Coins - amount);
            PlayerPrefs.Save();
            return true;
        }
        public static void SetString(string key, string value) {
            if (string.IsNullOrWhiteSpace(key)) return;
            PlayerPrefs.SetString("nmb_" + key, value ?? string.Empty);
            PlayerPrefs.Save();
        }
        public static string GetString(string key, string fallback = "") {
            return PlayerPrefs.GetString("nmb_" + key, fallback);
        }
    }
}
