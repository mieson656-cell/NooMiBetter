using UnityEngine;

namespace NooMiBetter
{
    public static class AchievementTracker
    {
        public static bool Unlock(string id)
        {
            if(string.IsNullOrWhiteSpace(id))return false;
            string key="nmb_achievement_"+id;
            if(PlayerPrefs.GetInt(key,0)==1)return false;
            PlayerPrefs.SetInt(key,1);PlayerPrefs.Save();return true;
        }
        public static bool IsUnlocked(string id){return PlayerPrefs.GetInt("nmb_achievement_"+id,0)==1;}
        public static int CountLanding(bool perfect)
        {
            int total=PlayerPrefs.GetInt("nmb_landings",0)+1;
            PlayerPrefs.SetInt("nmb_landings",total);
            if(total>=1)Unlock("first_landing");
            if(total>=10)Unlock("ten_landings");
            if(perfect)Unlock("perfect_landing");
            PlayerPrefs.Save();return total;
        }
    }
}
