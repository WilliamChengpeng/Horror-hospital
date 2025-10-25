using System;
using UnityEngine;
using Random = UnityEngine.Random;

//  VolFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Crt")]
    public class CrtPass : VolFx.Pass
    {
        private static readonly int s_NoiseTex    = Shader.PropertyToID("_NoiseTex");
        private static readonly int s_NoiseOffset = Shader.PropertyToID("_NoiseOffset");
        private static readonly int s_Distortion  = Shader.PropertyToID("_Distortion");
        private static readonly int s_Scanlines   = Shader.PropertyToID("_Scanlines");
        private static readonly int s_Noise       = Shader.PropertyToID("_Noise");
		
		public override string ShaderName => string.Empty;
        
        
        public  float            _flipRelease = .1f;
        [CurveRange]
        public  AnimationCurve   _flipCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public                  NoiseSettings _noiseSettings;
        private                 float         _flip;
        private                 float         _flicker;
        private                 Texture2D     _noise;

        [Serializable]
        public class NoiseSettings
        {
            public int        _height = 180;
            [Range(0, 1)]
            public float      _aspect = .3f;
            public bool       _point = true;
            [CurveRange]
            public AnimationCurve _intencityToHardness = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
        
        // =======================================================================
        private void OnValidate()
        {
            _validateNoise();
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<CrtVol>();

            if (settings.IsActive() == false)
                return false;
            
            _validateNoise();

            mat.SetTexture(s_NoiseTex, _noise);
            mat.SetVector(s_NoiseOffset, new Vector4(Random.value, Random.value, Random.value, Random.value));
            
            var flipSpeed = settings.m_Flip.value;
            var hasFlip = flipSpeed > 0;
            
            if (hasFlip || _flip != 0)
                _flip += (hasFlip ? flipSpeed : (_flip > .5f ? 1f / _flipRelease : -1f / _flipRelease)) * Time.deltaTime;
            
            if (hasFlip == false && (_flip > 1f || _flip < 0f))
                _flip = 0f;
            
            _flip %= 1f;
            
            _flicker += Time.deltaTime;
            
            if (_flicker > settings.m_FlickerPeriod.value)
                _flicker -= settings.m_FlickerPeriod.value;
            
            var flicker = Mathf.Sin((_flicker / (settings.m_FlickerPeriod.value + 0.07f)) * Mathf.PI * 2f) * settings.m_FlickerPower.value;
            var noise   = settings.m_NoiseIntensity.value;
            if (noise == 0)
                noise = -1;
            
            mat.SetVector(s_Distortion, new Vector4(settings.m_DistortionPower.value, settings.m_DistortionPeriod.value, settings.m_DistortionDensity.value, _flipCurve.Evaluate(_flip)));
            mat.SetVector(s_Scanlines, new Vector4(settings.m_ScanlinesCount.value, settings.m_ScanlinesIntensity.value, flicker, 0));
            mat.SetVector(s_Noise, new Vector4(Mathf.Clamp01(settings.m_NoiseHardness.value + _noiseSettings._intencityToHardness.Evaluate(settings.m_NoiseIntensity.value)), noise, 0, 0));
            
            return true;
        }

        private void _validateNoise()
        {
            var aspect   = Screen.width / (float)Screen.height;
            var noiseRes = new Vector2Int((int)(_noiseSettings._height * aspect * _noiseSettings._aspect), _noiseSettings._height);
            if (noiseRes.x < 4)
                noiseRes.x = 4;
            if (noiseRes.y < 4)
                noiseRes.y = 4;

            if (_noise == null || _noise.width != noiseRes.x || _noise.height != noiseRes.y)
            {
                _noise = new Texture2D(noiseRes.x, noiseRes.y, TextureFormat.RGBA32, false);

                _noise.filterMode = _noiseSettings._point ? FilterMode.Point : FilterMode.Bilinear;
                _noise.wrapMode   = TextureWrapMode.Repeat;
                for (var x = 0; x < _noise.width; x++)
                for (var y = 0; y < _noise.height; y++)
                    _noise.SetPixel(x, y, new Color(Random.value, Random.value, Random.value, Random.value));

                _noise.Apply();
            }
        }
    }
}