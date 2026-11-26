using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace NooMiBetter
{
    public static class CustomContentLoader
    {
        public static Texture2D TextureFromBytes(byte[] bytes) {
            if (bytes == null || bytes.Length == 0) return null;
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(bytes, false)) { UnityEngine.Object.Destroy(texture); return null; }
            return texture;
        }

        public static byte[] ReadImportedFile(string absolutePath) {
            if (string.IsNullOrWhiteSpace(absolutePath) || !File.Exists(absolutePath)) return Array.Empty<byte>();
            var info = new FileInfo(absolutePath);
            if (info.Length > 20 * 1024 * 1024) throw new IOException("File exceeds 20 MB limit.");
            return File.ReadAllBytes(absolutePath);
        }

        public static IEnumerator LoadAudio(string path, Action<AudioClip> completed) {
            if (string.IsNullOrWhiteSpace(path)) { completed?.Invoke(null); yield break; }
            string uri = path.Contains("://") ? path : "file://" + path;
            using (var request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.UNKNOWN)) {
                yield return request.SendWebRequest();
                completed?.Invoke(request.result == UnityWebRequest.Result.Success
                    ? DownloadHandlerAudioClip.GetContent(request) : null);
            }
        }
    }
}
