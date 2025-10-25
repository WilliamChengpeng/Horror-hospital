Shader "Hidden/VolFx/Scrolling"
{
    Properties
    {
		_Data("Data", Vector) = (1, 1, 0, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 0
        
        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off

        Pass
        {
            name "Scrolling"
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 

            Texture2D    _MainTex;
            SamplerState _point_clamp_sampler;

            float4 _Data; // offset, scale, alpha

            #define _offset _Data.xy
            #define _scale  _Data.z
            #define _alpha  _Data.w

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

            frag_in vert(const vert_in v)
            {
                frag_in o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }

            half4 frag(const frag_in i) : SV_Target
            {
                half4 main = _MainTex.Sample(_point_clamp_sampler, i.uv);
                half4 col  = _MainTex.Sample(_point_clamp_sampler, frac((i.uv - float2(.5, .5)) * _scale + float2(.5, .5) + _offset)); // sampler states doest work in webgl here is workaround
                return lerp(main, col, _alpha);
            }
            ENDHLSL
        }
    }
}
