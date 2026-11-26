using System.Collections.Generic;
using UnityEngine;

namespace NooMiBetter
{
    [System.Serializable]
    public struct ReplayFrame
    {
        public float time;
        public Vector3 position;
        public Quaternion rotation;
    }

    public sealed class ReplayRecorder : MonoBehaviour
    {
        public Rigidbody target;
        public float sampleRate = 20f;
        private readonly List<ReplayFrame> frames = new List<ReplayFrame>();
        private float elapsed;
        private float nextSample;
        public IReadOnlyList<ReplayFrame> Frames { get { return frames; } }

        public void Begin() { frames.Clear(); elapsed = 0; nextSample = 0; enabled = true; }
        public void End() { enabled = false; }
        private void Update() {
            if (!target) return;
            elapsed += Time.deltaTime;
            if (elapsed < nextSample) return;
            frames.Add(new ReplayFrame { time = elapsed, position = target.position, rotation = target.rotation });
            nextSample += 1f / Mathf.Clamp(sampleRate, 1f, 60f);
        }
        public void ApplyFrame(int index) {
            if (!target || index < 0 || index >= frames.Count) return;
            target.position = frames[index].position;
            target.rotation = frames[index].rotation;
        }
    }
}
