using UnityEngine;
using UnityEngine.UI;

namespace NooMiBetter
{
    public sealed class NooMiBetterApp : MonoBehaviour
    {
        Rigidbody gymnast;
        Text scoreLabel, walletLabel, stateLabel;
        int coins;
        int score;
        bool attached = true;
        float flightTime;

        private void Awake()
        {
            if (FindObjectsOfType<NooMiBetterApp>().Length > 1) { Destroy(gameObject); return; }
            DontDestroyOnLoad(gameObject);
            coins = PlayerPrefs.GetInt("nmb_coins", 0);
        }

        private void Start()
        {
            Application.targetFrameRate = PlayerPrefs.GetInt("nmb_low_power", 0) == 1 ? 30 : 60;
            BuildWorld();
            BuildHud();
        }

        private Material Mat(Color color)
        {
            var shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            var m = new Material(shader); m.color = color; return m;
        }

        private void BuildWorld()
        {
            Camera camera = Camera.main;
            if (camera == null) {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }
            camera.transform.position = new Vector3(0, 2.4f, -7.5f);
            camera.transform.rotation = Quaternion.Euler(10, 0, 0);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.025f, .035f, .065f);
            RenderSettings.ambientLight = new Color(.65f, .68f, .78f);
            var lightObject = new GameObject("Key Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.15f;
            lightObject.transform.rotation = Quaternion.Euler(45, -25, 0);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Landing Mat"; floor.transform.position = new Vector3(0, -1.25f, 0);
            floor.transform.localScale = new Vector3(12, .25f, 6);
            floor.GetComponent<Renderer>().material = Mat(new Color(.07f,.1f,.17f));

            foreach (float x in new[] {-2.3f, 2.3f}) {
                var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.transform.position = new Vector3(x, .1f, 0);
                post.transform.localScale = new Vector3(.12f, 2.7f, .12f);
                post.GetComponent<Renderer>().material = Mat(new Color(.28f,.34f,.45f));
            }
            var bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bar.name = "Training Bar"; bar.transform.position = new Vector3(0,1.45f,0);
            bar.transform.rotation = Quaternion.Euler(0,0,90);
            bar.transform.localScale = new Vector3(.07f,2.3f,.07f);
            bar.GetComponent<Renderer>().material = Mat(new Color(.15f,.8f,1f));

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Gymnast"; player.transform.position = new Vector3(0,.55f,0);
            player.transform.localScale = new Vector3(.4f,.8f,.3f);
            player.GetComponent<Renderer>().material = Mat(new Color(.55f,.3f,1f));
            gymnast = player.AddComponent<Rigidbody>();
            gymnast.mass = 1.1f; gymnast.constraints = RigidbodyConstraints.FreezePositionZ;
            gymnast.useGravity = false;
            var anchor = new GameObject("Bar Anchor").transform;
            anchor.position = new Vector3(0,1.45f,0);
            var joint = player.AddComponent<ConfigurableJoint>();
            joint.connectedBody = null; joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = anchor.position;
            joint.xMotion = ConfigurableJointMotion.Free;
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Free;
        }

        private void BuildHud()
        {
            var canvasObject = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080,1920);
            scoreLabel = Label(canvas.transform, "SCORE  0", 32, new Vector2(.5f,.92f), new Vector2(700,90));
            walletLabel = Label(canvas.transform, "COINS  "+coins, 25, new Vector2(.82f,.96f), new Vector2(330,80));
            stateLabel = Label(canvas.transform, "NOOMIBETTER  •  BAR TRAINING", 24, new Vector2(.5f,.86f), new Vector2(900,80));
            Button(canvas.transform,"FLIP",new Vector2(.18f,.12f),new Vector2(230,110),Flip);
            Button(canvas.transform,"GRAB / RELEASE",new Vector2(.5f,.12f),new Vector2(400,110),ToggleGrab);
            Button(canvas.transform,"RESET",new Vector2(.82f,.12f),new Vector2(230,110),ResetPlayer);
            Button(canvas.transform,"LOW POWER",new Vector2(.5f,.045f),new Vector2(360,85),TogglePerformance);
        }

        private Text Label(Transform parent,string value,int size,Vector2 anchor,Vector2 dimensions)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>(); rect.anchorMin=rect.anchorMax=anchor; rect.sizeDelta=dimensions;
            var t=go.GetComponent<Text>(); t.font=Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text=value;t.fontSize=size;t.color=Color.white;t.alignment=TextAnchor.MiddleCenter;return t;
        }
        private void Button(Transform parent,string title,Vector2 anchor,Vector2 dimensions,UnityEngine.Events.UnityAction action)
        {
            var go=new GameObject(title,typeof(RectTransform),typeof(Image),typeof(UnityEngine.UI.Button));go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=anchor;rect.sizeDelta=dimensions;
            var image=go.GetComponent<Image>();image.color=new Color(.1f,.17f,.29f,.96f);
            var button=go.GetComponent<UnityEngine.UI.Button>();button.targetGraphic=image;button.onClick.AddListener(action);
            var text=Label(go.transform,title,24,new Vector2(.5f,.5f),dimensions);text.rectTransform.sizeDelta=dimensions;
        }
        private void ToggleGrab()
        {
            attached=!attached; gymnast.useGravity=!attached;
            if(attached){ResetPlayer();stateLabel.text="GRABBED";}
            else{gymnast.AddForce(Vector3.up*5.5f,ForceMode.VelocityChange);flightTime=0;stateLabel.text="RELEASED";}
        }
        private void Flip(){gymnast.AddTorque(Vector3.forward*9f,ForceMode.Impulse);gymnast.AddForce(Vector3.up*1.5f,ForceMode.Impulse);score+=100;scoreLabel.text="SCORE  "+score;}
        private void ResetPlayer(){gymnast.velocity=Vector3.zero;gymnast.angularVelocity=Vector3.zero;gymnast.position=new Vector3(0,.55f,0);gymnast.rotation=Quaternion.identity;gymnast.useGravity=false;attached=true;flightTime=0;}
        private void TogglePerformance(){bool low=PlayerPrefs.GetInt("nmb_low_power",0)==0;PerformanceSettings.ApplyLowPower(low);stateLabel.text=low?"LOW POWER 30 FPS":"PERFORMANCE 60 FPS";}
        private void Update()
        {
            if(attached)return;flightTime+=Time.deltaTime;
            if(gymnast.position.y < -.55f){
                int reward=flightTime>1.1f?15:5;coins+=reward;score+=reward*20;
                PlayerPrefs.SetInt("nmb_coins",coins);PlayerPrefs.Save();
                walletLabel.text="COINS  "+coins;scoreLabel.text="SCORE  "+score;
                stateLabel.text=reward==15?"PERFECT LANDING  +15":"LANDING  +5";
                ResetPlayer();
            }
        }
    }

    public static class NooMiBetterBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Object.FindObjectOfType<NooMiBetterApp>() == null)
                new GameObject("NooMiBetterRoot").AddComponent<NooMiBetterApp>();
        }
    }
}
