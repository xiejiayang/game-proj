Shader "Dujiangyan/InkTrail"
{
    Properties
    {
        _Color ("Trail Color", Color) = (0.227, 0.353, 0.416, 0.6)
        _FadeEdge ("Fade Edge", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _FadeEdge;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float alpha = _Color.a;
                // Fade toward the tail (uv.x from 0 = head to 1 = tail)
                alpha *= saturate(1.0 - input.uv.x * _FadeEdge);
                // Soft edge
                float edge = 1.0 - abs(input.uv.y - 0.5) * 2.0;
                alpha *= smoothstep(0, 0.2, edge);
                return half4(_Color.rgb, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
