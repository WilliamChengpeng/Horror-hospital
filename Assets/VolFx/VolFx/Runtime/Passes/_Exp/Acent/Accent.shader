//  Accent © NullTale - https://x.com/NullTale
Shader "Hidden/VolFx/Accent"
{    
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        ZClip false
            
        Pass    // 0
        {
            Name "Accent"
            
            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_local PIXELATE _
            #pragma multi_compile_local DITHER NOISE
            
            #define LUT_SIZE 16.
            #define LUT_SIZE_MINUS (16. - 1.)

            sampler2D _MainTex;
	        sampler2D _MeasureTex;
	        sampler2D _NoiseTex;
	        sampler2D _ColorTex;
            
            uniform float4  _Data;
			float4          _Sobel;
			float4          _Grid;
			float4          _Grad;
			float4          _NoiseMad;
            //
            #define _Saturation _Data.x
            #define _Contrast   _Data.y
            #define _Blur       _Data.zw

            struct vert_in
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct frag_in
            {
                float4 vertex : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            frag_in vert(vert_in v)
            {
                frag_in o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }
                        
            half3 GetLinearToSRGB(half3 c)
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
            
            real3 luma(real3 rgb)
            {
                return dot(rgb.rgb, real3(0.299, 0.587, 0.114));
            }

            real3 bright(real3 rgb)
            {
                return max(rgb.r, max(rgb.g, rgb.b));
            }

            float3 blur(float2 uv)
            {
                float2 texel = _Blur * 0.005;
                float3 sum = float3(0, 0, 0);
                sum += tex2D(_MainTex, uv + texel * float2(-1, -1)).rgb;
                sum += tex2D(_MainTex, uv + texel * float2( 1, -1)).rgb;
                sum += tex2D(_MainTex, uv + texel * float2(-1,  1)).rgb;
                sum += tex2D(_MainTex, uv + texel * float2( 1,  1)).rgb;
                
                sum *= 0.25;
                return sum;
            }
            
            float3 cont(float3 color, float contrast)
            {
                return (color - 0.5) * contrast + 0.5;
            }

            float3 sat(float3 color, float saturation)
            {
                return lerp(luma(color).xxx, color, saturation);
            }
            
			float4 _sobel(sampler2D tex, float2 uv)
            {
            	float4 color =  tex2D(tex, uv) * _Sobel.z;
                color += tex2D(tex, uv + float2( _Sobel.x, 0)) * _Sobel.w;
                color += tex2D(tex, uv + float2(-_Sobel.x, 0)) * _Sobel.w;
                color += tex2D(tex, uv + float2(0, _Sobel.y))  * _Sobel.w;
                color += tex2D(tex, uv + float2(0,-_Sobel.y))  * _Sobel.w;

            	return color;
            }
            
			float2 _snap(float2 uv)
            {
	            return float2(round(uv.x * _Grid.x) / _Grid.x, round(uv.y * _Grid.y) / _Grid.y);
            }
            
            float4 lut_sample(in float3 uvw, const sampler2D tex)
            {
                float2 uv;
                
                // get replacement color from the lut set
                uv.y = uvw.y * (LUT_SIZE_MINUS / LUT_SIZE) + .5 * (1. / LUT_SIZE);
                uv.x = uvw.x * (LUT_SIZE_MINUS / (LUT_SIZE * LUT_SIZE)) + .5 * (1. / (LUT_SIZE * LUT_SIZE)) + floor(uvw.z * LUT_SIZE) / LUT_SIZE;
                
                float4 lutColor = tex2D(tex, uv);

                return lutColor;
            }

            float2 GetGradientUV(float4 vertex, float2 offset, float2 scale, float rotation)
            {
                // Convert screen position to [0,1] UV
                float2 screenUV = vertex.xy / float2(800, 800);

                // Centered coordinates [-0.5, 0.5]
                screenUV -= 0.5;

                // Rotate coordinates (counter-clockwise)
                float cosR = cos(rotation);
                float sinR = sin(rotation);
                float2 rotated = float2(
                    screenUV.x * cosR - screenUV.y * sinR,
                    screenUV.x * sinR + screenUV.y * cosR
                );

                // Apply scaling and offset for animation
                return rotated * scale + offset + 0.5;
            }

            float GetSaturation01(float3 color)
            {
                float maxVal = max(color.r, max(color.g, color.b));
                float minVal = min(color.r, min(color.g, color.b));
                float delta = maxVal - minVal;

                return delta;
            }

            
            half4 frag(frag_in i) : COLOR
            {
                half4 col = tex2D(_MainTex, i.uv);
                half4 noise = tex2D(_NoiseTex, (i.uv + _NoiseMad.xy) * _NoiseMad.z);
                
#if !defined(UNITY_COLORSPACE_GAMMA)
                float3 uvw = GetLinearToSRGB(col);
#else
                float3 uvw = col;
#endif
                float4 measure = lut_sample(uvw, _MeasureTex);
                
                float3 blurred = blur(i.uv);
                //half4 pix = tex2D(_MainTex, _snap(i.uv));
                
                //col.rgb = lerp(col.rgb, pix, 1 - 1 - max(pow(measure.r, 2), measure.g));
                //float focusColor = measure.r;

                // blur focus
                //col.rgb = lerp(col.rgb, blurred, 1 - max(pow(measure.g, _Contrast * 3), measure.r));

                // zero saturation noise
                //col.rgb = lerp(col.rgb, sat(col.rgb,  0).rgb, 1 - measure.g * noise.g);
                
                float2 gradUV = GetGradientUV(i.vertex, _Grad.xy, _Grad.zw, _Contrast);
                half4 grad = tex2D(_ColorTex, gradUV);
                
                // saturation focus
                col.rgb  = lerp(luma(col.rgb), sat(col.rgb,  _Saturation).rgb, max(measure.r, measure.r));
                half3 glow = grad.rgb * grad.a * GetSaturation01(col.rgb) * measure.r;
                col.rgb += glow;

                // sharpness focus
                //col.rgb = lerp(col.rgb, col.rgb + _sobel(_MainTex, i.uv), measure.g);
                
                //col.rgb = lerp(col.rgb, cont(col.rgb, _Contrast).rgb, 1 - max(pow(measure.r, 2), measure.g));
                //col.rgb = lerp(col.rgb, col.rgb + _Contrast, 1 - max(measure.r, measure.g));
                return col;
            }
            ENDHLSL
        }
    }
}