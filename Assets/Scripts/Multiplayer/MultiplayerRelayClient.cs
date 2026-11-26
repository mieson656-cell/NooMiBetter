using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace NooMiBetter
{
    [Serializable] public class RoomCreateResponse { public string code; public string playerId; }
    [Serializable] public class RoomEventBatch { public int cursor; public RoomPose[] events; }
    [Serializable] public class RoomPose
    {
        public int cursor; public string playerId;
        public float x,y,z,qx,qy,qz,qw;
    }
    [Serializable] internal class RoomRequest { public string playerId; }
    [Serializable] internal class PoseRequest
    {
        public string playerId; public float x,y,z,qx,qy,qz,qw;
    }

    public sealed class MultiplayerRelayClient : MonoBehaviour
    {
        public string serverUrl = "http://127.0.0.1:8787";
        public string RoomCode { get; private set; }
        public string PlayerId { get; private set; }
        public bool Connected { get; private set; }
        public event Action<RoomPose> OnRemotePose;
        int cursor;
        float nextSend;
        float nextPollTime;
        public IEnumerator CreateRoom(Action<string> completed)
        {
            yield return Post("/api/lobbies", "{}", response => {
                var parsed=JsonUtility.FromJson<RoomCreateResponse>(response);
                if(parsed!=null){RoomCode=parsed.code;PlayerId=parsed.playerId;Connected=true;}
                completed?.Invoke(RoomCode);
            });
        }
        public IEnumerator JoinRoom(string code, Action<bool> completed)
        {
            RoomCode=(code??"").Trim().ToUpperInvariant();
            string player=Guid.NewGuid().ToString("N");
            yield return Post("/api/lobbies/"+UnityWebRequest.EscapeURL(RoomCode)+"/join",
                JsonUtility.ToJson(new RoomRequest{playerId=player}), response => {
                    var parsed=JsonUtility.FromJson<RoomCreateResponse>(response);
                    if(parsed!=null){PlayerId=parsed.playerId;Connected=true;}
                    completed?.Invoke(Connected);
                });
        }
        public void Leave(){Connected=false;RoomCode=null;PlayerId=null;cursor=0;polling=false;nextPollTime=0;}
        public void SendPose(Transform target)
        {
            if(!Connected || Time.unscaledTime<nextSend || target==null)return;
            nextSend=Time.unscaledTime+.05f;
            var p=target.position;var q=target.rotation;
            var body=JsonUtility.ToJson(new PoseRequest{playerId=PlayerId,x=p.x,y=p.y,z=p.z,qx=q.x,qy=q.y,qz=q.z,qw=q.w});
            StartCoroutine(Post("/api/lobbies/"+RoomCode+"/events",body,null));
        }
        private void Update(){if(Connected && !polling && Time.unscaledTime>=nextPollTime)StartCoroutine(Poll());}
        private IEnumerator Poll()
        {
            // Poll at 5 Hz; only one request is allowed at a time.
            if(polling || !Connected)yield break;
            polling=true;
            using(var request=UnityWebRequest.Get(serverUrl+"/api/lobbies/"+RoomCode+"/events?after="+cursor+"&playerId="+UnityWebRequest.EscapeURL(PlayerId)))
            {
                request.timeout=4;yield return request.SendWebRequest();
                if(request.result==UnityWebRequest.Result.Success){
                    var batch=JsonUtility.FromJson<RoomEventBatch>(request.downloadHandler.text);
                    if(batch!=null){cursor=batch.cursor;if(batch.events!=null)foreach(var item in batch.events)if(item.playerId!=PlayerId)OnRemotePose?.Invoke(item);}
                }
            }
            polling=false;
            nextPollTime=Time.unscaledTime+.2f;
        }
        bool polling;
        private IEnumerator Post(string path,string json,Action<string> success)
        {
            using(var request=new UnityWebRequest(serverUrl+path,"POST")){
                byte[] data=System.Text.Encoding.UTF8.GetBytes(json);
                request.uploadHandler=new UploadHandlerRaw(data);request.downloadHandler=new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type","application/json");request.timeout=6;
                yield return request.SendWebRequest();
                if(request.result==UnityWebRequest.Result.Success)success?.Invoke(request.downloadHandler.text);
                else {Connected=false;Debug.LogWarning("NooMiBetter relay: "+request.error);}
            }
        }
    }
}
