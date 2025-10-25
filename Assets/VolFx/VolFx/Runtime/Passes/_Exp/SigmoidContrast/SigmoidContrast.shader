Shader "Hidden/VolFx/SigmoidContrast"
{
    Properties
    {
        _Weight("Weight", Range(0, 1)) = 1
        _Gamma("Gamma", Float) = 0.55
        _Power("Power", Float) = 0.55
    }

    SubShader
    {
        name "Sigmoid Contrast"
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 

            sampler2D       _MainTex;
            uniform float4  _MainTex_TexelSize;         // 1 / width, 1 / height, width, height
            
            float           _Weight;
            float           _Gamma;
            float           _Power;
            
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
            
            float _cubic(float val)
            {
                if (val < 0.5)
                    return val * val * val * val * val * _Power;
                
                val -= 1.0;
                
                return val * val * val * val * val * _Power + 1.0;
            }
            
            float _sigmoid(float val)
            {
	            //return 1.0 / (1.0 + (exp(-(x * 14.0 - 7.0))));
                return 1.0 / (1.0 + (exp(-(val - 0.5) * 14.0))); 
            }

            float4 frag (frag_in i) : SV_Target
            {
                float4 initial = tex2D(_MainTex, i.uv);
    
    	        float4 col = float4(_cubic(initial.r), _cubic(initial.g),_cubic(initial.b), initial.a);
       	        col = pow(abs(col), _Gamma);

                return lerp(initial, col, _Weight);
            }
            
            ENDHLSL
        }
    }
}