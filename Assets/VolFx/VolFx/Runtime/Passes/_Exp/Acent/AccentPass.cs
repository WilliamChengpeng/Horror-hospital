	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using UnityEngine;
	using Random = UnityEngine.Random;

	//  Accent © NullTale - https://x.com/NullTale
	namespace VolFx
	{
	    [ShaderName("Hidden/VolFx/Accent")]
	    public class AccentPass : VolFx.Pass
	    {
	        private static readonly int s_QuantTex   = Shader.PropertyToID("_QuantTex");
	        private static readonly int s_MeasureTex = Shader.PropertyToID("_MeasureTex");
			
	        public override string ShaderName => string.Empty;
			
			public Texture2D _pulsation;
			
			[CurveRange]
			public AnimationCurve _checkUp;
			[CurveRange]
			public AnimationCurve _checkDown;
			public AnimationCurve _hueCompensation = new AnimationCurve(
    new Keyframe[]
    {
        new Keyframe(0.0f, 0.8827171f, 0.3583592f, 0.3583592f),
        new Keyframe(0.32727742f, 1.0f, -0.2830323f, -0.2830323f),
        new Keyframe(0.615887f, 0.73320246f, -0.7901262f, -0.7901262f),
        new Keyframe(0.8114624f, 0.6049385f, -0.6558286f, -0.6558286f)
    }
);
			
	        private LutGenerator.LutSize _lutSize = LutGenerator.LutSize.x16;
	        private LutGenerator.Gamma   _gamma   = LutGenerator.Gamma.rec601;
	        
	        private Dictionary<Texture2D, PaletteCash> _paletteCash = new Dictionary<Texture2D, PaletteCash>();
	        
			private Color _prev;
			
	        // =======================================================================
	        public class PaletteCash
	        {
	            public  Texture2D _palette;
	            public  Texture2D _quant;
	            public  Texture2D _measure;
	        }

	        // =======================================================================
	        public override void Init()
	        {
	            _paletteCash.Clear();
	        }
	        
	        public override bool Validate(Material mat)
	        {
	            var settings = Stack.GetComponent<AccentVol>();

	            if (settings.IsActive() == false)
	                return false;

				settings.m_Pulsation.value.GetTexture(ref _pulsation);
				_pulsation.wrapMode = TextureWrapMode.Repeat;
				
				var accent = settings.m_Accent.value;

				var colors = new Color[] { settings.m_Accent.value, settings.m_Сomplement.value, settings.m_SupportA.value, settings.m_SupportB.value };
				var pal = new Texture2D(colors.Length, 1, TextureFormat.RGBA32, false, false);
				pal.SetPixels(colors);
				pal.Apply();
				
				if (_prev != settings.m_Accent.value)
				{
					_prev = settings.m_Accent.value;
					_paletteCash.Clear();
				}
				
				if (_paletteCash.TryGetValue(settings.m_Palette.value as Texture2D, out var palCash) == false)
				{
					palCash = LutGenerator.Generate(pal, new Vector3((settings.m_HueSpread.value) * 120f, 
																	 (settings.m_ValueSpread.value) * 1.5f, 
																	 (settings.m_SaturationSpread.value) * 1.5f),
						_checkUp, _checkDown, settings.m_Softness.value);
					_paletteCash.Add(pal, palCash);
				}
				
				var aspect = Screen.width / (float)Screen.height;

				mat.SetTexture("_PaletteTex", palCash._palette);
				mat.SetTexture("_MeasureTex", palCash._measure);
				mat.SetTexture("_QuantTex"  , palCash._quant);
				mat.SetTexture("_NoiseTex"  , settings.m_Noise.value);
				mat.SetTexture("_ColorTex", _pulsation);
				
				mat.SetVector("_Data", new Vector4(settings.m_Saturation.value, settings.m_Contrast.value, 
												   settings.m_Blur.value, settings.m_Blur.value * aspect));
				
				mat.SetVector("_Sobel", new Vector4(settings.m_Sobel.value * aspect * 0.003f, settings.m_Sobel.value * 0.003f, 4f, -1f));
				
				var _crushPix = settings.m_Pix.value;
	            var pix = new Vector4(aspect * _crushPix, _crushPix);
				if (settings.m_Pix.value <= 0f)
					pix = new Vector4(10000, 10000);
				
				mat.SetVector("_Grid", pix);
				mat.SetVector("_Grad", settings.m_Grad.value + new Vector4(Time.time * settings.m_Blur.value, 0f, 0f, 0f));
				mat.SetVector("_NoiseMad", new Vector4(Random.value, Random.value, settings.m_Pix.value / 100f));
				
				return true;
			}
			
			// =======================================================================
			public static class LutGenerator
			{
				private static Texture2D _lut16;
				private static Texture2D _lut32;
				private static Texture2D _lut64;

				// =======================================================================
				[Serializable]
				public enum LutSize
				{
					x16,
					x32,
					x64
				}

				[Serializable]
				public enum Gamma
				{
					rec601,
					rec709,
					rec2100,
					average,
				}
				
				// =======================================================================
				public static PaletteCash Generate(Texture2D _palette, Vector3 spread, AnimationCurve up, AnimationCurve down, float weight,
												   LutSize lutSize = LutSize.x16, Gamma gamma = Gamma.rec601)
				{
					var clean  = _getLut(lutSize);
					var lut    = clean.GetPixels();
					var colors = _palette.GetPixels();
					
					var _lutMeasure = new Texture2D(clean.width, clean.height, TextureFormat.ARGB32, false);
					
					var measure = lut.Select(lutColor =>
									{
										/*var set = colors
												  .Select(gradeColor => (grade: compare(lutColor, gradeColor, gradeColor.a  * 3f), color: gradeColor))
												  .OrderByDescending(n => n.grade)
												  .ToArray();
										
										// soft color measure
										var measure = 1f - Mathf.Clamp01(set[0].grade);
										*/
										var qual = colors
												  //.Select(gradeColor => (grade: compare(lutColor, gradeColor, gradeColor.a  * 3f), color: gradeColor))
												  .Select(gradeColor => (grade: _compareHsl(lutColor, gradeColor,  spread.x,  spread.y,  spread.z) * gradeColor.a, color: gradeColor))
												  .OrderByDescending(n => n.grade)
												  .ToArray();

										// color qualification (discreet)
										var qualificator = qual[0].grade * qual[0].color.a;
										
										return new Color(qualificator, 0f, 0f);
									})
									.ToArray();

					_lutMeasure.SetPixels(measure);
					_lutMeasure.filterMode = FilterMode.Bilinear;
					_lutMeasure.wrapMode   = TextureWrapMode.Clamp;
					_lutMeasure.Apply();
					
					var result = new AccentPass.PaletteCash()
					{
						_measure  = _lutMeasure,
					};
					
					return result;

					// -----------------------------------------------------------------------
					float compare(Color a, Color b, float weightMul = 1f)
					{
						// compare colors by grayscale distance
						var weight = gamma switch
						{
							Gamma.rec601  => new Vector3(0.299f, 0.587f, 0.114f),
							Gamma.rec709  => new Vector3(0.2126f, 0.7152f, 0.0722f),
							Gamma.rec2100 => new Vector3(0.2627f, 0.6780f, 0.0593f),
							Gamma.average => new Vector3(0.33333f, 0.33333f, 0.33333f),
							_             => throw new ArgumentOutOfRangeException()
						};
						weight *= weightMul;
						
						// var c = a.ToVector3().Mul(weight) - b.ToVector3().Mul(weight);
						var c = new Vector3(a.r * weight.x, a.g * weight.y, a.b * weight.z) - new Vector3(b.r * weight.x, b.g * weight.y, b.b * weight.z);
						
						return c.magnitude;
					}
					
					float _compareHsl(Color a, Color b, float spreadH, float spreadS, float spreadL)
					{
					    var aH = GetHue(a) * 360f;
					    var bH = GetHue(b) * 360f;
					    
						//RGBToHSL(a, out var aH, out var aS, out var aL);
					    //RGBToHSL(b, out var bH, out var bS, out var bL);

					    // Разность по Hue (угловая дистанция)
					    var dH = Mathf.Clamp01(Mathf.Abs(Mathf.DeltaAngle(aH, bH)) / spreadH);
					    //var dS = Mathf.Clamp01(Mathf.Abs(aS - bS) / spreadS);
					    //var dL = Mathf.Clamp01(Mathf.Abs(aL - bL) / spreadL);

					    // Плавное падение (0..1), clamp чтобы не выйти за границы
					    var wH = Mathf.Clamp01(1f - dH);
					    //var wS = Mathf.Clamp01(1f - dS);
					    //var wL = Mathf.Clamp01(1f - dL);

					    // Итоговая степень схожести (можно заменить на среднее, если нужно)
					    return (1f - _weight(wH)) /* * wS * wL*/;

					    // -------------------------
						float _weight(float val)
						{
							var curveA = up.Evaluate(val);
							var curveB = down.Evaluate(val);
							
							return Mathf.Lerp(curveA, curveB, weight);
						}
						
					    static void RGBToHSL(Color color, out float h, out float s, out float l)
					    {
					        var r = color.r;
					        var g = color.g;
					        var b = color.b;

					        var max = Mathf.Max(r, Mathf.Max(g, b));
					        var min = Mathf.Min(r, Mathf.Min(g, b));
					        var delta = max - min;

					        l = (max + min) * 0.5f;

					        if (Mathf.Approximately(delta, 0f))
					        {
					            s = 0f;
					            h = 0f;
					            return;
					        }

					        s = delta / (1f - Mathf.Abs(2f * l - 1f));

					        if (Mathf.Approximately(max, r))
					            h = (g - b) / delta % 6f;
					        else if (Mathf.Approximately(max, g))
					            h = (b - r) / delta + 2f;
					        else
					            h = (r - g) / delta + 4f;

					        h *= 60f;
					        if (h < 0f) h += 360f;
					    }
						static float GetHue(Color color)
	                    {
					        Color.RGBToHSV(color, out float h, out _, out _);
							return h;
							
	                        float r = color.r;
	                        float g = color.g;
	                        float b = color.b;
	                    
	                        float max = Mathf.Max(r, Mathf.Max(g, b));
	                        float min = Mathf.Min(r, Mathf.Min(g, b));
	                        float delta = max - min;
	                    
	                        if (delta == 0f)
	                            return 0f; // Undefined hue, achromatic color
	                    
	                        float hue;
	                        if (max == r)
	                            hue = (g - b) / delta + (g < b ? 6f : 0f);
	                        else if (max == g)
	                            hue = (b - r) / delta + 2f;
	                        else // max == b
	                            hue = (r - g) / delta + 4f;
	                    
	                        hue /= 6f;
	                    
	                        return hue; // [0..1]
	                    }
					}

					/*float _compareHsl(Color a, Color b, float spreadH, float spreadS, float spreadL)
					{
					    float GetHue01(Color c)
					    {
					        Color.RGBToHSV(c, out float h, out _, out _);
					        return h;
					    }

					    float hA = GetHue01(a) * 360f;
					    float hB = GetHue01(b) * 360f;

					    float delta = Mathf.DeltaAngle(hA, hB);

					    // Получаем значение компенсации с кривой
					    float compensation = _hueCompensation.Evaluate(hB / 360f);
					    float adjustedSpread = spreadH * compensation;

					    // Расчёт по формуле гауссовой кривой
					    float sigma = adjustedSpread / 2f;
					    float similarity = Mathf.Exp(-0.5f * (delta * delta) / (sigma * sigma));

					    return similarity;
					}*/

					Color _lutAt(Color c)
					{
						if (c.r >= 1f) c.r = 0.999f;
						if (c.g >= 1f) c.g = 0.999f;
						if (c.b >= 1f) c.b = 0.999f;
						
						var _lutSize = _getLutSize(lutSize);
						var scale   = (_lutSize - 1f) / _lutSize;
						var offset  = .5f * (1f / _lutSize);
						var step    = 1f / _lutSize;
						// y / (lutSize - 1f)
						var x = Mathf.FloorToInt((c.r * scale + offset) / step);
						var y = Mathf.FloorToInt((c.g * scale + offset) / step);
						var z = Mathf.FloorToInt((c.b * scale + offset) / step);

						return lutAt(x, y, z);
						
						// -----------------------------------------------------------------------
						Color lutAt(int x, int y, int z)
						{
							return new Color(x / (_lutSize - 1f), y / (_lutSize - 1f), z / (_lutSize - 1f), 1f);
						}
					}
				}

				// =======================================================================
				internal static int _getLutSize(LutSize lutSize)
				{
					return lutSize switch
					{
						LutSize.x16 => 16,
						LutSize.x32 => 32,
						LutSize.x64 => 64,
						_           => throw new ArgumentOutOfRangeException()
					};
				}
				
				internal static Texture2D _getLut(LutSize lutSize)
				{
					var size = _getLutSize(lutSize);
					var _lut = lutSize switch
					{
						LutSize.x16 => _lut16,
						LutSize.x32 => _lut32,
						LutSize.x64 => _lut64,
						_           => throw new ArgumentOutOfRangeException(nameof(lutSize), lutSize, null)
					};
					
					if (_lut != null && _lut.height == size)
						 return _lut;
					
					_lut            = new Texture2D(size * size, size, TextureFormat.RGBA32, 0, false);
					_lut.filterMode = FilterMode.Bilinear;
					_lut.wrapMode   = TextureWrapMode.Clamp;

					for (var y = 0; y < size; y++)
					for (var x = 0; x < size * size; x++)
						_lut.SetPixel(x, y, _lutAt(x, y));
					
					_lut.Apply();
					return _lut;

					// -----------------------------------------------------------------------
					Color _lutAt(int x, int y)
					{
						return new Color((x % size) / (size - 1f), y / (size - 1f), Mathf.FloorToInt(x / (float)size) * (1f / (size - 1f)), 1f);
					}
				}
			}
	    }
	}