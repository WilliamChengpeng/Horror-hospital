using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Scrolling")]
    public class ScrollingPass : VolFx.Pass
    {
        private static readonly int s_Data = Shader.PropertyToID("_Data");
		
		public override string ShaderName => string.Empty;
        
        private Vector2 _offset;

        // =======================================================================
        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<ScrollingVol>();

            if (settings.IsActive() == false)
                return false;
            
            _offset   -= settings.m_Speed.value * Mathf.Pow(settings.m_Weight.value, 2) * Time.deltaTime;
            _offset.x = (_offset.x) % (settings.m_Tiling.value > 1 ?  3 : 1);
            _offset.y = (_offset.y) % (settings.m_Tiling.value > 1 ?  3 : 1);
            mat.SetVector(s_Data, new Vector4(_offset.x * settings.m_Weight.value, _offset.y * settings.m_Weight.value, 
                                              Mathf.Lerp(1, settings.m_Tiling.value, settings.m_Weight.value), settings.m_Alpha.value));
            return true;
        }
    }
}