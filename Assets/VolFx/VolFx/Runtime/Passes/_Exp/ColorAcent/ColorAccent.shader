Shader "Hidden/VolFx/ColorAccent"
{
    SubShader
    {
        name "ColorAcent"
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 0
        
        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off
        
        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_local _LUT_SIZE_X16 _LUT_SIZE_X32 _LUT_SIZE_X64
            
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
            
            sampler2D _MainTex;
            sampler2D _Lut;
            half      _Weight;
            
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
            
            float4 _lut(in half3 col)
            {
                // sample the texture
#if !defined(UNITY_COLORSPACE_GAMMA)
                float3 uvw = GetLinearToSRGB(col);
#else
                float3 uvw = col;
#endif
                float2 uv;
                
                // get replacement color from the lut tables set
                uv.y = uvw.y * (LUT_SIZE_MINUS / LUT_SIZE) + .5 * (1. / LUT_SIZE);
                uv.x = uvw.x * (LUT_SIZE_MINUS / (LUT_SIZE * LUT_SIZE)) + .5 * (1. / (LUT_SIZE * LUT_SIZE)) + floor(uvw.z * LUT_SIZE) / LUT_SIZE;

                float4 lutColor = tex2D(_Lut, uv);
                
#if !defined(UNITY_COLORSPACE_GAMMA)
                lutColor = float4(GetSRGBToLinear(lutColor.xyz), lutColor.w);
#endif

                return lutColor;
            }
            
            half luma(half3 rgb)
            {
                return dot(rgb, half3(.299, .587, .114));
            }
            
            half4 frag(frag_in i) : SV_Target
            {
                half4 init = tex2D(_MainTex, i.uv);
                half3 gray = luma(init.rgb);
                
                return half4(lerp(init, gray, (1 - _lut(init.rgb).r) * _Weight), init.a);
            }
            
            /*half4 frag (frag_in i) : SV_Target
            {
                half4 initial = tex2D(_MainTex, i.uv);
                half3 grayscale = luma(initial.rgb);
                
                float3 hvs = RgbToHsv(initial.rgb);

                float weight = 0;
                if (_qH.x < hvs.x && hvs.x < _qH.y &&
                    _qV.x < hvs.y && hvs.y < _qV.y &&
                    _qS.x < hvs.z && hvs.z < _qS.y)
                        weight = 1;

                return half4(lerp(grayscale, initial, weight), initial.a);
            }*/
            ENDHLSL
        }
    }
}