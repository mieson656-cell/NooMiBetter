using System;
using System.Collections.Generic;
using UnityEngine;

namespace NooMiBetter
{
    public static class WorkshopSave
    {
        [Serializable]
        private class MapData { public List<ObstacleDefinition> obstacles = new List<ObstacleDefinition>(); }
        public static void Save(string name, List<ObstacleDefinition> obstacles) {
            var data = new MapData { obstacles = obstacles ?? new List<ObstacleDefinition>() };
            PlayerPrefs.SetString("nmb_map_" + Safe(name), JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
        public static List<ObstacleDefinition> Load(string name) {
            string json = PlayerPrefs.GetString("nmb_map_" + Safe(name), "");
            if (string.IsNullOrEmpty(json)) return new List<ObstacleDefinition>();
            return JsonUtility.FromJson<MapData>(json).obstacles;
        }
        private static string Safe(string value) {
            if (string.IsNullOrWhiteSpace(value)) return "untitled";
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
            return value.Trim().ToLowerInvariant();
        }
    }
}
