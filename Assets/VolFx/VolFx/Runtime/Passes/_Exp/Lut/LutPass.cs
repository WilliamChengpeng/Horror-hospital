using System;
using UnityEngine;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Lut")]
    public class LutPass : VolFx.Pass
    {
        private static readonly int s_LutTableTex = Shader.PropertyToID("_LutTableTex");
        private static readonly int s_Weight      = Shader.PropertyToID("_Weight");
        private static readonly int s_Eval        = Shader.PropertyToID("_Eval");
        private static readonly int s_Grades      = Shader.PropertyToID("_Grades");
		
		public override string ShaderName => string.Empty;
        
        [Tooltip("What to do with source image alpha relative to lut table alpha value")]
        public  Alpha _sourceAplha = Alpha.Multiply;
        private Alpha _sourceAplhaPrev = Alpha.Multiply;
        private int   _lutTableWidth;
        private bool  _lutTableFilter;
        private int   _lutSize;

        // =======================================================================
        public enum Alpha
        {
            Override,
            Multiply,
            Keep
        }
        
        // =======================================================================
        public override void Init()
        {
            _validateAlpha();
            _lutTableWidth = -1;
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<LutVol>();

            if (settings.IsActive() == false)
                return false;
            
            if (_sourceAplhaPrev != _sourceAplha)
                _validateAlpha();
            
            var lootTable = settings.m_LutTable.value;
            _validateLutSize(lootTable.width);
            _validateLutFilter(settings.m_PointFilter.value);
            
            var grades = (float)lootTable.height / _lutSize;
            var sharp  = !settings.m_LevelSmoot.value;
            
            mat.SetTexture(s_LutTableTex, lootTable);
            mat.SetFloat(s_Weight, settings.m_Weight.value);
            
            mat.SetFloat(s_Eval, 1f - (sharp ? settings.m_Level.value - (settings.m_Level.value % (1 / grades)) : settings.m_Level.value));
            mat.SetFloat(s_Grades, grades);
            
            return true;
        }

        // =======================================================================
        private void _validateLutFilter(bool value)
        {
            if (_lutTableFilter == value)
                return;
            
            _lutTableFilter = value;
            
            _material.DisableKeyword("_POINT");
            _material.DisableKeyword("_LINEAR");
            
            _material.EnableKeyword(value ? "_POINT": "_LINEAR");
        }
        
        private void _validateLutSize(int width)
        {
            if (_lutTableWidth == width)
                return;
            
            _lutTableWidth = width;

            _material.DisableKeyword("_LUT_SIZE_X16");
            _material.DisableKeyword("_LUT_SIZE_X32");
            _material.DisableKeyword("_LUT_SIZE_X64");
            
            switch (width)
            {
                case 16 * 16:
                    _material.EnableKeyword("_LUT_SIZE_X16");
                    _lutSize = 16;
                    break;
                case 32 * 32:
                    _material.EnableKeyword("_LUT_SIZE_X32");
                    _lutSize = 32;
                    break;
                case 64 * 64:
                    _material.EnableKeyword("_LUT_SIZE_X64");
                    _lutSize = 64;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unknown loot size");
            }
        }
        private void _validateAlpha()
        {
            _sourceAplhaPrev = _sourceAplha;
            _material.DisableKeyword("_ALPHA_OVERRIDE");
            _material.DisableKeyword("_ALPHA_KEEP");
            _material.DisableKeyword("_ALPHA_MULTIPLY");
            
            _material.EnableKeyword(_sourceAplha switch {
                Alpha.Override => "_ALPHA_OVERRIDE",
                Alpha.Multiply => "_ALPHA_MULTIPLY",
                Alpha.Keep     => "_ALPHA_KEEP",
                _              => throw new ArgumentOutOfRangeException()
            });
        }
    }
}