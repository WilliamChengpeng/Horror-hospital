using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Lut")]
    public sealed class LutVol : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter m_Weight      = new ClampedFloatParameter(0, 0, 1);
        public Texture2DParameter    m_LutTable    = new Texture2DParameter(null, true);
        public BoolParameter         m_PointFilter = new BoolParameter(false);
        public ClampedFloatParameter m_Level       = new ClampedFloatParameter(0, 0, 1);
        public BoolParameter         m_LevelSmoot  = new BoolParameter(true);

        // =======================================================================
        // Can be used to skip rendering if false
        public bool IsActive() => active && (m_Weight.value > 0f) && m_LutTable.value != null;

        public bool IsTileCompatible() => false;
    }
}