using UnityEngine;

namespace NooMiBetter
{
    public static class CosmeticShop
    {
        private static readonly Color[] colors = {
            new Color(.55f,.3f,1f), new Color(.1f,.8f,1f),
            new Color(1f,.35f,.48f), new Color(.3f,1f,.55f),
            new Color(1f,.72f,.2f), new Color(.92f,.92f,.98f)
        };
        public static int Selected { get { return PlayerPrefs.GetInt("nmb_skin",0); } }
        public static Color CurrentColor { get { return colors[Mathf.Clamp(Selected,0,colors.Length-1)]; } }
        public static bool PurchaseNext()
        {
            int next=(Selected+1)%colors.Length;
            int price=next==0?0:100+next*50;
            int owned=PlayerPrefs.GetInt("nmb_skin_owned_"+next,next==0?1:0);
            if(owned==0 && !LocalProfile.SpendCoins(price))return false;
            PlayerPrefs.SetInt("nmb_skin_owned_"+next,1);
            PlayerPrefs.SetInt("nmb_skin",next);PlayerPrefs.Save();return true;
        }
    }
}
