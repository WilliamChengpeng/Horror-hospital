using System.Linq;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Masking")]
    public class MaskingPass : VolFx.Pass
    {
        private static readonly int s_Contrast   = Shader.PropertyToID("_Contrast");
        private static readonly int s_Hue        = Shader.PropertyToID("_Hue");
        private static readonly int s_Saturation = Shader.PropertyToID("_Saturation");
        private static readonly int s_Brightness = Shader.PropertyToID("_Brightness");
        private static readonly int s_Tint       = Shader.PropertyToID("_Tint");
        private static readonly int s_Value      = Shader.PropertyToID("_Value");
        private static readonly int s_ValueTex   = Shader.PropertyToID("_ValueTex");
        private static readonly int s_MaskTex    = Shader.PropertyToID("_MaskTex");
        private static readonly int s_ScaleTex   = Shader.PropertyToID("_MaskScale");
		
		public override string ShaderName => string.Empty;
        
        public float      _maskScale;
        public Texture2D  _maskTex;
        private Texture2D _valueTex;

        // =======================================================================
        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<MaskingVol>();

            if (settings.IsActive() == false)
                return false;
            
            mat.SetFloat(s_Contrast, settings.m_Contrast.value + 1f);
            mat.SetFloat(s_Hue, settings.m_Hue.value * Mathf.PI);
            mat.SetFloat(s_Saturation, settings.m_Saturation.value + 1f);
            mat.SetFloat(s_Brightness, settings.m_Brightness.value);
            
            mat.SetTexture(s_ValueTex, settings.m_Threshold.value.GetTexture(ref _valueTex));
            mat.SetTexture(s_MaskTex, _maskTex);
            mat.SetVector(s_ScaleTex, new Vector4(Screen.width / (float)Screen.height, 1f) * _maskScale);
            mat.SetColor(s_Tint, settings.m_Tint.value);
            
            return true;
        }

        protected override bool _editorValidate => _maskTex == null;
        protected override void _editorSetup(string folder, string asset)
        {
#if UNITY_EDITOR
			_maskTex = UnityEditor.AssetDatabase.FindAssets("t:texture", new string[] {$"{folder}\\Mask"})
							   .Select(n => UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(n)))
							   .Where(n => n != null)
							   .FirstOrDefault(n => n.name == "Checker");
#endif
        }
    }
}