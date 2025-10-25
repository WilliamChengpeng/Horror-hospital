using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Contrast")]
    public sealed class SigmoidContrastVol : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter         _weight = new ClampedFloatParameter(0f, 0f, 1f);
        public NoInterpClampedFloatParameter _gamma  = new NoInterpClampedFloatParameter(.55f, 0f, 1f);

        // =======================================================================
        public bool IsActive() => active && _weight.value > 0f;

        public bool IsTileCompatible() => true;
    }
}