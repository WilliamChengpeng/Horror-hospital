using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [ShaderName("Hidden/VolFx/ColorAccent")]
    public class ColorAccentPass : VolFx.Pass
    {
        private static readonly int s_Weight = Shader.PropertyToID("_Weight");
        private static readonly int s_Lut    = Shader.PropertyToID("_Lut");
		
		public override string ShaderName => string.Empty;
        
        // =======================================================================
        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<ColorAccentVol>();

            if (settings.IsActive() == false)
                return false;
            
            mat.SetFloat(s_Weight, settings.m_Weight.value);
            mat.SetTexture(s_Lut, settings.m_Qualifier.value.Lut);
            
            return true;
        }
    }
}