using System;
using UnityEngine;

namespace VolFx
{
    [Serializable]
    public class ColorQualifier
    {
        public Vector2 _hue;
        public Vector2 _sat;
        public Vector2 _val;
        
        // =======================================================================
        public bool IsMet(Color c)
        {
            Color.RGBToHSV(c, out var h, out var s, out var v);
            
            return _hue.x <= h && h <= _hue.y &&
                   _sat.x <= s && s <= _sat.y && 
                   _val.x <= v && v <= _val.y;
        }
    }
}