using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  Accent © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Accent")]
    public sealed class AccentVol : VolumeComponent, IPostProcessComponent
    {
        [InspectorName("Weight")]
        [Tooltip("Full effects impact")]
        public ClampedFloatParameter m_Impact = new ClampedFloatParameter(0, 0, 1);

        public Texture2DParameter     m_Palette    = new Texture2DParameter(null);
        public Texture2DParameter     m_Noise      = new Texture2DParameter(null);
        public NoInterpColorParameter m_Accent     = new NoInterpColorParameter(Color.red, false);
        public NoInterpColorParameter m_Сomplement = new NoInterpColorParameter(Color.clear, false);
        public NoInterpColorParameter m_SupportA   = new NoInterpColorParameter(Color.clear, false);
        public NoInterpColorParameter m_SupportB   = new NoInterpColorParameter(Color.clear, false);
        
        public GradientParameter      m_Pulsation  = new GradientParameter(new GradientValue(new Gradient()), false);
        
        public ClampedFloatParameter m_Saturation  = new ClampedFloatParameter(0, -2, 2);
        public ClampedFloatParameter m_Contrast  = new ClampedFloatParameter(0, -2, 2);
        public ClampedFloatParameter m_Sobel = new ClampedFloatParameter(0, -2, 2);
        public ClampedFloatParameter m_Blur  = new ClampedFloatParameter(0, -2, 2);
        public ClampedFloatParameter m_Pix  = new ClampedFloatParameter(0, 0, 1000);
        
        public Vector4Parameter m_Grad  = new Vector4Parameter(new Vector4(0, 0, 0, 0));
        
        [Header("Advanced")]
        [Header("Color grading")]
        public NoInterpClampedFloatParameter m_SaturationSpread  = new NoInterpClampedFloatParameter(.1f, 0, 1f);
        public NoInterpClampedFloatParameter m_HueSpread         = new NoInterpClampedFloatParameter(.1f, 0, 1f);
        public NoInterpClampedFloatParameter m_ValueSpread       = new NoInterpClampedFloatParameter(.1f, 0, 1f);
        
        public NoInterpClampedFloatParameter m_Softness          = new NoInterpClampedFloatParameter(1f, 0, 1f);
        
        // =======================================================================
        [Serializable]
        public class NoiseModeParameter : VolumeParameter<DitherPass.Mode>
        {
            public NoiseModeParameter(DitherPass.Mode value, bool overrideState) : base(value, overrideState) { }
        }
        
        // =======================================================================
        public bool IsActive() => active && (m_Impact.value > 0f);

        public bool IsTileCompatible() => false;
    }
}