using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [ShaderName("Hidden/VolFx/SigmoidContrast")]
    public class SigmoidContrastPass : VolFx.Pass
    {
        private static readonly int s_Weight = Shader.PropertyToID("_Weight");
        private static readonly int s_Gamma  = Shader.PropertyToID("_Gamma");
		
		public override string ShaderName => string.Empty;
        
        protected override bool Invert => true;

        // =======================================================================
        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<SigmoidContrastVol>();

            if (settings.IsActive() == false)
                return false;
            
            mat.SetFloat(s_Weight, settings._weight.value);
            mat.SetFloat(s_Gamma, settings._gamma.value);

            return true;
        }
    }
}