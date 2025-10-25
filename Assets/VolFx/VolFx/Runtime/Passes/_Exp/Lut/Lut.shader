Shader "Hidden/VolFx/Lut"
{
    Properties
    {
        _Weight("Weight", Float) = 1
        _Eval("Eval", Float) = 0
        _Grades("Grades", Float) = 7
    }

    SubShader
    {
        name "Lut"
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 0

        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off
        
        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_local _ALPHA_OVERRIDE _ALPHA_KEEP _ALPHA_MULTIPLY
            #pragma multi_compile_local _LUT_SIZE_X16 _LUT_SIZE_X32 _LUT_SIZE_X64
            #pragma multi_compile_local _POINT _LINEAR
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
            
            Texture2D    _MainTex;
            Texture2D    _LutTableTex;
            SamplerState _point_clamp_sampler;
            SamplerState _linear_clamp_sampler;
            
            float        _Weight;
            float        _Eval;
            float        _Grades;
            
#if defined(_LUT_SIZE_X16)
            #define LUT_SIZE 16.
            #define LUT_SIZE_MINUS (16. - 1.)
#endif
            
#if defined(_LUT_SIZE_X32)
            #define LUT_SIZE 32.
            #define LUT_SIZE_MINUS (32. - 1.)
#endif
            
#if defined(_LUT_SIZE_X64)
            #define LUT_SIZE 64.
            #define LUT_SIZE_MINUS (64. - 1.)
#endif
            
            struct vert_in
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct frag_in
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            frag_in vert (vert_in v)
            {
                frag_in o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }

            real3 FastLinearToSRGB(real3 c)
            {
                return saturate(1.055 * PositivePow(c, 0.416666667) - 0.055);
            }
            
            real3 LinearToSRGB(real3 c)
            {
                real3 sRGBLo = c * 12.92;
                real3 sRGBHi = (PositivePow(c, real3(1.0/2.4, 1.0/2.4, 1.0/2.4)) * 1.055) - 0.055;
                real3 sRGB   = (c <= 0.0031308) ? sRGBLo : sRGBHi;
                return sRGB;
            }
            
            real3 FastSRGBToLinear(real3 c)
            {
                return c * (c * (c * 0.305306011 + 0.682171111) + 0.012522878);
            }
            
            real3 SRGBToLinear(real3 c)
            {
#if defined(UNITY_COLORSPACE_GAMMA) && REAL_IS_HALF
                c = min(c, 100.0); // Make sure not to exceed HALF_MAX after the pow() below
#endif
                real3 linearRGBLo  = c / 12.92;
                real3 linearRGBHi  = PositivePow((c + 0.055) / 1.055, real3(2.4, 2.4, 2.4));
                real3 linearRGB    = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
                return linearRGB;
            }
            
            real3 GetLinearToSRGB(real3 c)
            {
#if _USE_FAST_SRGB_LINEAR_CONVERSION
                return FastLinearToSRGB(c);
#else
                return LinearToSRGB(c);
#endif
            }

            real3 GetSRGBToLinear(real3 c)
            {
#if _USE_FAST_SRGB_LINEAR_CONVERSION
                return FastSRGBToLinear(c);
#else
                return SRGBToLinear(c);
#endif
            }

            void lut_pix_float(in float3 col, in float lum, out float4 result)
            {
                // sample the texture
#if !defined(UNITY_COLORSPACE_GAMMA)
                float3 uvw = GetLinearToSRGB(col);
#else
                float3 uvw = col;
#endif
                
                float2 uv;
                
                // get replacement color from the lut tables set
                uv.y = (uvw.y * (LUT_SIZE_MINUS / LUT_SIZE) + .5 * (1. / LUT_SIZE)) * (1. / _Grades) + floor(lum * _Grades) / _Grades;
                uv.x = uvw.x * (LUT_SIZE_MINUS / (LUT_SIZE * LUT_SIZE)) + .5 * (1. / (LUT_SIZE * LUT_SIZE)) + floor(uvw.z * LUT_SIZE) / LUT_SIZE;

#if defined(_POINT)
                float4 lutColor = _LutTableTex.Sample(_point_clamp_sampler, uv);
#else
                float4 lutColor = _LutTableTex.Sample(_linear_clamp_sampler, uv);
#endif
                
#if !defined(UNITY_COLORSPACE_GAMMA)
                lutColor = float4(GetSRGBToLinear(lutColor.xyz), lutColor.w);
#endif

                result = lutColor;
            }

            float4 lut_pix_smooth_float(in float3 col, in float lum)
            {
                // sample the texture
#if !defined(UNITY_COLORSPACE_GAMMA)
                float3 uvw = GetLinearToSRGB(col);
#else
                float3 uvw = col;
#endif
                
                float2 uv;

                // blended lut interpolation from the lut tables set
                uv.y = (uvw.y * (LUT_SIZE_MINUS / LUT_SIZE) + .5 * (1. / LUT_SIZE)) * (1. / _Grades) + floor(lum * _Grades) / _Grades;
                uv.x = uvw.x * (LUT_SIZE_MINUS / (LUT_SIZE * LUT_SIZE)) + .5 * (1. / (LUT_SIZE * LUT_SIZE)) + floor(uvw.z * LUT_SIZE) / LUT_SIZE;

#if defined(_POINT)
                float4 lutSource = _LutTableTex.Sample(_point_clamp_sampler, uv);
#else
                float4 lutSource = _LutTableTex.Sample(_linear_clamp_sampler, uv);
#endif
                
                uv.y = (uvw.y * (LUT_SIZE_MINUS / LUT_SIZE) + .5 * (1. / LUT_SIZE)) * (1. / _Grades) + clamp((floor(lum * _Grades) - 1), 0, _Grades - 1) / _Grades;
                uv.x = uvw.x * (LUT_SIZE_MINUS / (LUT_SIZE * LUT_SIZE)) + .5 * (1. / (LUT_SIZE * LUT_SIZE)) + floor(uvw.z * LUT_SIZE) / LUT_SIZE;
                
#if defined(_POINT)
                float4 lutDest = _LutTableTex.Sample(_point_clamp_sampler, uv);
#else
                float4 lutDest = _LutTableTex.Sample(_linear_clamp_sampler, uv);
#endif

                float4 lutColor = lerp(lutSource, lutDest, 1. - frac(lum * _Grades));
                
#if !defined(UNITY_COLORSPACE_GAMMA)
                lutColor = float4(GetSRGBToLinear(lutColor.xyz), lutColor.w);
#endif

                return lutColor;
            }
            
            half4 frag(frag_in i) : SV_Target
            {
#if defined(_POINT)
                float4 color = _MainTex.Sample(_point_clamp_sampler, i.uv);
#else
                float4 color = _MainTex.Sample(_linear_clamp_sampler, i.uv);
#endif
                float4 result = lut_pix_smooth_float(color.rgb, _Eval);

                  
#if defined(_ALPHA_MULTIPLY)
                result.a *= color.a;
#endif
#if defined(_ALPHA_KEEP)
                result.a = color.a;
#endif
#if defined(_ALPHA_OVERRIDE)
                color.a = result.a;
#endif
                return lerp(color, result, _Weight);
            }
            
            ENDHLSL
        }
    }
}