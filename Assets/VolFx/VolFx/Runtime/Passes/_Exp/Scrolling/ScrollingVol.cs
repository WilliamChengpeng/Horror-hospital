using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Scrolling")]
    public sealed class ScrollingVol : VolumeComponent, IPostProcessComponent
    { 
        public ClampedFloatParameter m_Weight = new ClampedFloatParameter(0, 0, 1, false);
        public ClampedFloatParameter m_Tiling = new ClampedFloatParameter(1, .03f, 10f);
        public ClampedFloatParameter m_Alpha  = new ClampedFloatParameter(1, .0f, 1f);
        public Vector2Parameter      m_Speed  = new Vector2Parameter(Vector2.zero, false);

        // =======================================================================
        public bool IsActive() => active && (m_Speed.value != Vector2.zero || m_Tiling.value != 1f) && m_Weight.value > 0;
        public bool IsTileCompatible() => false;
    }
}