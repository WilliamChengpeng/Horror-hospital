using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/ColorAccent")]
    public sealed class ColorAccentVol : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter   m_Weight    = new ClampedFloatParameter(0, 0, 1);
        public ColorQualifierParameter m_Qualifier = new ColorQualifierParameter(null, false);
        
        // =======================================================================
        [Serializable]
        public class ColorQualifierParameter : VolumeParameter<ColorQualifierAsset>
        {
            public ColorQualifierParameter(ColorQualifierAsset value, bool overrideState) : base(value, overrideState) { }
        }
        
        // =======================================================================
        
        // Can be used to skip rendering if false
        public bool IsActive() => active && m_Weight.value > 0 && m_Qualifier.value != null;

        public bool IsTileCompatible() => false;
    }
}