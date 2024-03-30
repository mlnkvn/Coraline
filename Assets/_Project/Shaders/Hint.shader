Shader "Universal Render Pipeline/Hint"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SparkleIntensity ("Sparkle Intensity", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent+1" }

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _SparkleIntensity;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, IN.uv);
                half noise = tex2D(_MainTex, IN.uv * _Time.y).r;
                half3 lightGreen = half3(0.56, 1, 0.56);
                half3 color = lerp(texColor.rgb, lightGreen, _SparkleIntensity);
                color += noise * _SparkleIntensity;
                return half4(color, texColor.a);
            }
            ENDHLSL
        }
    }
}