using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NooMiBetter
{
    public sealed class NooMiBetterApp : MonoBehaviour
    {
        RagdollController gymnast;
        MultiplayerRelayClient multiplayer;
        ReplayRecorder replay;
        bool recordingReplay;
        Texture2D generatedSkin;
        Text scoreLabel,walletLabel,stateLabel,roomLabel;
        InputField roomInput,serverInput;
        readonly Dictionary<string,Transform> remotePlayers=new Dictionary<string,Transform>();
        Material gymnastMaterial;
        int coins,score;
        bool airborne;
        float flightTime,nextNetworkSend;

        private void Awake()
        {
            if(FindObjectsOfType<NooMiBetterApp>().Length>1){Destroy(gameObject);return;}
            DontDestroyOnLoad(gameObject);coins=PlayerPrefs.GetInt("nmb_coins",0);
        }
        private void Start()
        {
            PerformanceSettings.ApplyLowPower(PlayerPrefs.GetInt("nmb_low_power",0)==1);
            BuildWorld();BuildHud();
            replay=gameObject.AddComponent<ReplayRecorder>();replay.target=gymnast.Torso;
            ApplySkin();
            multiplayer=gameObject.AddComponent<MultiplayerRelayClient>();
            multiplayer.OnRemotePose+=OnRemotePose;
            multiplayer.serverUrl=PlayerPrefs.GetString("nmb_relay_url","http://127.0.0.1:8787");
            if(serverInput!=null)serverInput.text=multiplayer.serverUrl;
        }
        private Material Mat(Color color)
        {
            var shader=Shader.Find("Standard");if(shader==null)shader=Shader.Find("Universal Render Pipeline/Lit");
            var m=new Material(shader);m.color=color;return m;
        }
        private void BuildWorld()
        {
            Camera camera=Camera.main;
            if(camera==null){var go=new GameObject("Main Camera");go.tag="MainCamera";camera=go.AddComponent<Camera>();}
            camera.transform.position=new Vector3(0,2.2f,-8);camera.transform.rotation=Quaternion.Euler(8,0,0);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.025f,.035f,.065f);
            RenderSettings.ambientLight=new Color(.65f,.68f,.78f);
            var lightGo=new GameObject("Key Light");var light=lightGo.AddComponent<Light>();
            light.type=LightType.Directional;light.intensity=1.15f;lightGo.transform.rotation=Quaternion.Euler(45,-25,0);
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="Landing Mat";
            floor.transform.position=new Vector3(0,-1.35f,0);floor.transform.localScale=new Vector3(12,.25f,6);
            floor.GetComponent<Renderer>().material=Mat(new Color(.07f,.1f,.17f));
            foreach(float x in new[]{-2.3f,2.3f}){
                var post=GameObject.CreatePrimitive(PrimitiveType.Cube);post.name="Bar Support";
                post.transform.position=new Vector3(x,.1f,0);post.transform.localScale=new Vector3(.12f,2.7f,.12f);
                post.GetComponent<Renderer>().material=Mat(new Color(.28f,.34f,.45f));
            }
            var bar=GameObject.CreatePrimitive(PrimitiveType.Cylinder);bar.name="Training Bar";
            bar.transform.position=new Vector3(0,1.45f,0);bar.transform.rotation=Quaternion.Euler(0,0,90);
            bar.transform.localScale=new Vector3(.07f,2.3f,.07f);bar.GetComponent<Renderer>().material=Mat(new Color(.15f,.8f,1));
            gymnastMaterial=Mat(new Color(.55f,.3f,1));
            gymnast=gameObject.AddComponent<RagdollController>();gymnast.Build(new Vector3(0,.25f,0),transform,gymnastMaterial);
        }
        private void BuildHud()
        {
            var canvasGo=new GameObject("HUD",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGo.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1080,1920);
            scoreLabel=Label(canvas.transform,"SCORE  0",30,new Vector2(.5f,.96f),new Vector2(700,80));
            walletLabel=Label(canvas.transform,"COINS  "+coins,24,new Vector2(.82f,.99f),new Vector2(300,70));
            stateLabel=Label(canvas.transform,"NOOMIBETTER  /  RAGDOLL TRAINING",22,new Vector2(.5f,.90f),new Vector2(950,70));
            Button(canvas.transform,"FLIP",new Vector2(.17f,.14f),new Vector2(220,105),Flip);
            Button(canvas.transform,"GRAB / RELEASE",new Vector2(.5f,.14f),new Vector2(390,105),ToggleGrab);
            Button(canvas.transform,"RESET",new Vector2(.83f,.14f),new Vector2(220,105),ResetPlayer);
            Button(canvas.transform,"LOW POWER",new Vector2(.5f,.075f),new Vector2(330,75),TogglePerformance);
            roomInput=Input(canvas.transform,"ROOM CODE",new Vector2(.5f,.045f),new Vector2(300,70));
            Button(canvas.transform,"CREATE ROOM",new Vector2(.22f,.045f),new Vector2(260,75),CreateRoom);
            Button(canvas.transform,"JOIN",new Vector2(.78f,.045f),new Vector2(180,75),JoinRoom);
            roomLabel=Label(canvas.transform,"MULTIPLAYER: OFFLINE",18,new Vector2(.5f,.005f),new Vector2(850,60));
            serverInput=Input(canvas.transform,"https://your-relay.example",new Vector2(.38f,.20f),new Vector2(620,70));
            Button(canvas.transform,"SET SERVER",new Vector2(.82f,.20f),new Vector2(230,75),SetRelayUrl);
            Button(canvas.transform,"SKIN / SHOP",new Vector2(.2f,.82f),new Vector2(260,75),BuySkin);
            Button(canvas.transform,"TEXTURE LAB",new Vector2(.8f,.82f),new Vector2(260,75),GenerateSkin);
            Button(canvas.transform,"REPLAY",new Vector2(.5f,.82f),new Vector2(220,75),ToggleReplay);
        }
        private Text Label(Transform parent,string value,int size,Vector2 anchor,Vector2 dimensions)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=anchor;rect.sizeDelta=dimensions;
            var t=go.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text=value;t.fontSize=size;t.color=Color.white;t.alignment=TextAnchor.MiddleCenter;return t;
        }
        private void Button(Transform parent,string title,Vector2 anchor,Vector2 dimensions,UnityEngine.Events.UnityAction action)
        {
            var go=new GameObject(title,typeof(RectTransform),typeof(Image),typeof(UnityEngine.UI.Button));go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=anchor;rect.sizeDelta=dimensions;
            var image=go.GetComponent<Image>();image.color=new Color(.1f,.17f,.29f,.96f);
            var button=go.GetComponent<UnityEngine.UI.Button>();button.targetGraphic=image;button.onClick.AddListener(action);
            var label=Label(go.transform,title,20,new Vector2(.5f,.5f),dimensions);label.rectTransform.sizeDelta=dimensions;
        }
        private InputField Input(Transform parent,string placeholder,Vector2 anchor,Vector2 dimensions)
        {
            var go=new GameObject("Room Code Input",typeof(RectTransform),typeof(Image),typeof(InputField));go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=anchor;rect.sizeDelta=dimensions;
            go.GetComponent<Image>().color=new Color(.08f,.1f,.16f,.98f);
            var field=go.GetComponent<InputField>();
            var text=Label(go.transform,"",20,new Vector2(.5f,.5f),dimensions);text.color=Color.white;
            field.textComponent=text;field.characterLimit=6;field.contentType=InputField.ContentType.Alphanumeric;
            field.placeholder=Label(go.transform,placeholder,17,new Vector2(.5f,.5f),dimensions);field.placeholder.color=new Color(.65f,.68f,.75f);
            return field;
        }
        private void ApplySkin()
        {
            if(generatedSkin!=null)Destroy(generatedSkin);
            gymnastMaterial=Mat(CosmeticShop.CurrentColor);
            if(generatedSkin!=null)gymnastMaterial.mainTexture=generatedSkin;
            gymnast.SetMaterial(gymnastMaterial);
        }
        private void BuySkin()
        {
            if(CosmeticShop.PurchaseNext()){
                coins=LocalProfile.Coins;walletLabel.text="COINS  "+coins;ApplySkin();
                stateLabel.text="COSMETIC UNLOCKED";
            }else stateLabel.text="NOT ENOUGH COINS — NEXT SKIN";
        }
        private void GenerateSkin()
        {
            if(generatedSkin!=null)Destroy(generatedSkin);
            generatedSkin=TexturePatternGenerator.Create(128,128,CosmeticShop.CurrentColor,Color.white,Random.Range(0,99999));
            gymnastMaterial.mainTexture=generatedSkin;gymnast.SetMaterial(gymnastMaterial);
            stateLabel.text="TEXTURE LAB — PATTERN APPLIED";
        }
        private void ToggleReplay()
        {
            if(!recordingReplay){replay.Begin();recordingReplay=true;stateLabel.text="REPLAY RECORDING";}
            else{replay.End();recordingReplay=false;stateLabel.text="REPLAY SAVED IN MEMORY";StartCoroutine(PlayReplay());}
        }
        private IEnumerator PlayReplay()
        {
            var frames=replay.Frames;
            for(int i=0;i<frames.Count;i++){
                replay.ApplyFrame(i);
                yield return null;
            }
        }
        private void ToggleGrab()
        {
            if(gymnast.IsAttached){gymnast.Release();airborne=true;flightTime=0;stateLabel.text="RELEASED — CONTROL YOUR LANDING";}
            else{gymnast.Grab();airborne=false;stateLabel.text="GRABBED";}
        }
        private void Flip(){gymnast.AddTrickImpulse(10);score+=100;scoreLabel.text="SCORE  "+score;}
        private void ResetPlayer(){gymnast.ResetBody(new Vector3(0,.25f,0));airborne=false;flightTime=0;stateLabel.text="READY";}
        private void TogglePerformance()
        {
            bool low=PlayerPrefs.GetInt("nmb_low_power",0)==0;PerformanceSettings.ApplyLowPower(low);
            stateLabel.text=low?"LOW POWER 30 FPS":"PERFORMANCE 60 FPS";
        }
        private void SetRelayUrl()
        {
            string value=(serverInput.text??"").Trim().TrimEnd('/');
            if(!(value.StartsWith("https://")||value.StartsWith("http://"))){roomLabel.text="SERVER URL MUST START WITH HTTP(S)";return;}
            PlayerPrefs.SetString("nmb_relay_url",value);PlayerPrefs.Save();
            multiplayer.serverUrl=value;multiplayer.Leave();roomLabel.text="RELAY SERVER SAVED";
        }
        private void CreateRoom(){StartCoroutine(multiplayer.CreateRoom(code=>{if(!string.IsNullOrEmpty(code)){roomInput.text=code;roomLabel.text="ROOM "+code+" — SHARE THIS CODE";}else roomLabel.text="ROOM CREATION FAILED";}));}
        private void JoinRoom(){StartCoroutine(multiplayer.JoinRoom(roomInput.text,ok=>roomLabel.text=ok?"JOINED ROOM "+multiplayer.RoomCode:"COULD NOT JOIN ROOM"));}
        private void OnRemotePose(RoomPose pose)
        {
            if(!remotePlayers.TryGetValue(pose.playerId,out var avatar)){
                var go=GameObject.CreatePrimitive(PrimitiveType.Capsule);go.name="Remote Player";
                go.GetComponent<Renderer>().material=Mat(new Color(1,.45f,.25f,.7f));avatar=go.transform;
                remotePlayers[pose.playerId]=avatar;
            }
            avatar.position=Vector3.Lerp(avatar.position,new Vector3(pose.x,pose.y,pose.z),.65f);
            avatar.rotation=Quaternion.Slerp(avatar.rotation,new Quaternion(pose.qx,pose.qy,pose.qz,pose.qw),.65f);
        }
        private void Update()
        {
            if(airborne){
                flightTime+=Time.deltaTime;
                if(gymnast.LowestY<-.75f){
                    int reward=flightTime>1.1f?15:5;coins+=reward;score+=reward*20;AchievementTracker.CountLanding(reward==15);
                    PlayerPrefs.SetInt("nmb_coins",coins);PlayerPrefs.Save();
                    walletLabel.text="COINS  "+coins;scoreLabel.text="SCORE  "+score;
                    stateLabel.text=reward==15?"PERFECT LANDING  +15":"LANDING  +5";ResetPlayer();
                }
            }
            if(multiplayer!=null && multiplayer.Connected && Time.unscaledTime>=nextNetworkSend){
                nextNetworkSend=Time.unscaledTime+.05f;multiplayer.SendPose(gymnast.Torso.transform);
            }
        }
    }
    public static class NooMiBetterBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot(){if(Object.FindObjectOfType<NooMiBetterApp>()==null)new GameObject("NooMiBetterRoot").AddComponent<NooMiBetterApp>();}
    }
}
