using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Crt")]
    public sealed class CrtVol : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter m_Flip               = new ClampedFloatParameter(0, 0, 10);
        public ClampedFloatParameter m_FlickerPower       = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter m_FlickerPeriod      = new ClampedFloatParameter(1, 0, 7);
        public ClampedFloatParameter m_DistortionPower    = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter m_DistortionPeriod   = new ClampedFloatParameter(0, 0, 400); 
        public ClampedFloatParameter m_DistortionDensity  = new ClampedFloatParameter(3, 0, 400);
        public ClampedFloatParameter m_NoiseIntensity     = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter m_NoiseHardness      = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter m_ScanlinesIntensity = new ClampedFloatParameter(0, 0, 1.2f);
        public ClampedFloatParameter m_ScanlinesCount     = new ClampedFloatParameter(500, 100, 1000);

        // =======================================================================

        // Can be used to skip rendering if false
        public bool IsActive() => active && (m_Flip.value != 0 || m_FlickerPower.value != 0 || m_DistortionPower.value != 0 || m_NoiseIntensity.value != 0 || m_ScanlinesIntensity.value != 0);

        public bool IsTileCompatible() => false;
    }
}