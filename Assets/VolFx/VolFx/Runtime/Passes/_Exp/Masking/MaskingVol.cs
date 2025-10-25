using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Masking")]
    public sealed class MaskingVol : VolumeComponent, IPostProcessComponent
    {
        public ColorParameter        m_Tint       = new ColorParameter(Color.white);
        public CurveParameter        m_Threshold  = new CurveParameter(new CurveValue(new AnimationCurve(
                                                                                         new Keyframe(0f, 1f),
                                                                                         new Keyframe(1f, 1f))), false);
        
        public ClampedFloatParameter m_Hue        = new ClampedFloatParameter(0, -1, 1);
        public ClampedFloatParameter m_Saturation = new ClampedFloatParameter(0, -1, 1);
        
        public ClampedFloatParameter m_Brightness = new ClampedFloatParameter(0, -1, 1);
        public ClampedFloatParameter m_Contrast   = new ClampedFloatParameter(0, -1, 1);
        
        // =======================================================================
        public bool IsActive() => active &&(m_Hue.value != 0
                                         || m_Saturation.value != 0 
                                         || m_Contrast.value != 0 
                                         || m_Brightness.value != 0
                                         || m_Tint.value != Color.white);

        public bool IsTileCompatible() => false;
    }
}