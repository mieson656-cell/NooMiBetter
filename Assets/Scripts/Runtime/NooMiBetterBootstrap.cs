using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace NooMiBetter
{
    public sealed class NooMiBetterBootstrap : MonoBehaviour
    {
        private Rigidbody body;
        private Transform bar;
        private Text status;
        private Text scoreText;
        private Text coinsText;
        private float score;
        private float airTime;
        private bool grabbed = true;
        private int coins;
        private readonly List<GameObject> obstacles = new List<GameObject>();
        private Material playerMaterial;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create() {
            if (FindObjectOfType<NooMiBetterBootstrap>() == null)
                new GameObject("NooMiBetter Runtime").AddComponent<NooMiBetterBootstrap>();
        }

        private void Start() {
            Application.targetFrameRate = 60;
            coins = PlayerPrefs.GetInt("nmb_coins", 0);
            BuildWorld();
            BuildUI();
        }

        private void BuildWorld() {
            Camera cam = Camera.main;
            if (cam == null) {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                cam = cameraObject.AddComponent<Camera>();
            }
            cam.transform.position = new Vector3(0, 3.2f, -8);
            cam.transform.rotation = Quaternion.Euler(12, 0, 0);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.035f, .045f, .08f);

            RenderSettings.ambientLight = new Color(.55f, .58f, .7f);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Landing Mat";
            floor.transform.position = new Vector3(0, -1.25f, 0);
            floor.transform.localScale = new Vector3(12, .3f, 8);
            floor.GetComponent<Renderer>().material = MakeMat(new Color(.09f,.12f,.2f));

            var left = CreateBarSupport(-2.4f);
            var right = CreateBarSupport(2.4f);
            bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            bar.name = "Gymnastics Bar";
            bar.position = new Vector3(0, 1.65f, 0);
            bar.rotation = Quaternion.Euler(0, 0, 90);
            bar.localScale = new Vector3(.075f, 2.4f, .075f);
            bar.GetComponent<Renderer>().material = MakeMat(new Color(.2f,.85f,1f));
            CreatePlayer();
            CreateObstacle(new Vector3(3.4f,-.5f,0), new Vector3(.8f,1.2f,.8f));
        }

        private GameObject CreateBarSupport(float x) {
            var support = GameObject.CreatePrimitive(PrimitiveType.Cube);
            support.transform.position = new Vector3(x, .1f, 0);
            support.transform.localScale = new Vector3(.12f, 2.7f, .12f);
            support.GetComponent<Renderer>().material = MakeMat(new Color(.28f,.32f,.42f));
            return support;
        }

        private void CreatePlayer() {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "NooMiBetter Gymnast";
            go.transform.position = new Vector3(0, .8f, 0);
            go.transform.localScale = new Vector3(.42f,.9f,.32f);
            playerMaterial = MakeMat(new Color(.55f,.3f,1f));
            go.GetComponent<Renderer>().material = playerMaterial;
            body = go.AddComponent<Rigidbody>();
            body.mass = 1.2f;
            body.constraints = RigidbodyConstraints.FreezePositionZ;
            body.useGravity = false;
            var joint = go.AddComponent<ConfigurableJoint>();
            joint.connectedBody = null;
            joint.xMotion = ConfigurableJointMotion.Free;
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Free;
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;
            joint.connectedAnchor = new Vector3(0, 1.65f, 0);
            joint.autoConfigureConnectedAnchor = false;
            body.useGravity = false;
        }

        private void CreateObstacle(Vector3 position, Vector3 scale) {
            var o = GameObject.CreatePrimitive(PrimitiveType.Cube);
            o.name = "Workshop Obstacle";
            o.transform.position = position;
            o.transform.localScale = scale;
            o.GetComponent<Renderer>().material = MakeMat(new Color(1f,.35f,.42f));
            obstacles.Add(o);
        }

        private Material MakeMat(Color color) {
            var shader = Shader.Find("Standard");
            var mat = new Material(shader);
            mat.color = color;
            return mat;
        }

        private void BuildUI() {
            var canvasObject = new GameObject("NooMiBetter HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            status = AddText(canvas.transform, "NOOMIBETTER  /  BAR LAB", 42, new Vector2(.5f,.94f), new Vector2(.9f,.06f));
            scoreText = AddText(canvas.transform, "SCORE  0", 32, new Vector2(.5f,.87f), new Vector2(.8f,.04f));
            coinsText = AddText(canvas.transform, "COINS  " + coins, 26, new Vector2(.85f,.94f), new Vector2(.25f,.04f));
            AddButton(canvas.transform, "GRAB / RELEASE", new Vector2(.5f,.12f), new Vector2(.7f,.075f), ToggleGrab);
            AddButton(canvas.transform, "FLIP", new Vector2(.18f,.12f), new Vector2(.22f,.075f), Flip);
            AddButton(canvas.transform, "RESET", new Vector2(.82f,.12f), new Vector2(.22f,.075f), ResetPlayer);
            AddButton(canvas.transform, "NEW OBSTACLE", new Vector2(.5f,.045f), new Vector2(.45f,.05f), AddWorkshopObstacle);
        }

        private Text AddText(Transform parent, string value, int size, Vector2 anchor, Vector2 dimensions) {
            var go = new GameObject(value, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = new Vector2(900*dimensions.x, 100*dimensions.y);
            rect.anchoredPosition = Vector2.zero;
            var text = go.GetComponent<Text>();
            text.text = value; text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size; text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white; text.horizontalOverflow = HorizontalWrapMode.Overflow;
            return text;
        }

        private void AddButton(Transform parent, string label, Vector2 anchor, Vector2 dimensions, UnityEngine.Events.UnityAction action) {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = new Vector2(900*dimensions.x, 120*dimensions.y);
            rect.anchoredPosition = Vector2.zero;
            go.GetComponent<Image>().color = new Color(.12f,.18f,.3f,.94f);
            go.GetComponent<Button>().onClick.AddListener(action);
            var text = AddText(go.transform, label, 25, new Vector2(.5f,.5f), new Vector2(1,1));
            text.rectTransform.sizeDelta = rect.sizeDelta;
        }

        private void ToggleGrab() {
            grabbed = !grabbed;
            body.useGravity = !grabbed;
            if (grabbed) {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.position = new Vector3(0, .8f, 0);
                body.rotation = Quaternion.identity;
                airTime = 0;
            } else {
                body.AddForce(new Vector3(0, 5.5f, 0), ForceMode.VelocityChange);
                airTime = 0;
            }
            UpdateStatus(grabbed ? "GRABBED" : "RELEASED — PERFORM A TRICK");
        }

        private void Flip() {
            if (body == null) return;
            body.AddTorque(Vector3.forward * 9f, ForceMode.Impulse);
            body.AddForce(Vector3.up * 1.8f, ForceMode.Impulse);
            score += 100;
            UpdateScore();
        }

        private void ResetPlayer() {
            body.velocity = Vector3.zero; body.angularVelocity = Vector3.zero;
            body.position = new Vector3(0,.8f,0); body.rotation = Quaternion.identity;
            body.useGravity = false; grabbed = true; airTime = 0;
            UpdateStatus("READY");
        }

        private void AddWorkshopObstacle() {
            float x = Random.Range(-4f,4f);
            CreateObstacle(new Vector3(x,-.45f,Random.Range(-.4f,.4f)), new Vector3(Random.Range(.35f,.8f),Random.Range(.4f,1.4f),.6f));
            UpdateStatus("WORKSHOP: OBSTACLE ADDED");
        }

        private void Update() {
            if (body == null || grabbed) return;
            airTime += Time.deltaTime;
            if (body.position.y < -.55f) {
                float grade = airTime > 1.2f && Mathf.Abs(body.velocity.x) < 2.5f ? 300 : 100;
                score += grade;
                coins += grade == 300 ? 15 : 5;
                PlayerPrefs.SetInt("nmb_coins", coins);
                coinsText.text = "COINS  " + coins;
                UpdateScore();
                UpdateStatus(grade == 300 ? "PERFECT LANDING  +15" : "CLEAN LANDING  +5");
                ResetPlayer();
            }
        }
        private void UpdateScore() { scoreText.text = "SCORE  " + Mathf.RoundToInt(score); }
        private void UpdateStatus(string value) { if(status) status.text = "NOOMIBETTER  /  " + value; }
    }
}
