using System;
using UnityEngine;

namespace NooMiBetter
{
    [Serializable]
    public class ObstacleDefinition
    {
        public Vector3 position;
        public Vector3 scale = Vector3.one;
        public Color color = Color.cyan;
    }
}
