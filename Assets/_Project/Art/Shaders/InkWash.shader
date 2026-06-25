Shader "Dujiangyan/InkWash"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.91, 0.88, 0.82, 1)
        _InkColor ("Ink Color", Color) = (0.16, 0.16, 0.16, 1)
        _NoiseScale ("Noise Scale", Float) = 2.0
        _InkStrength ("Ink Strength", Range(0, 1)) = 0.5
        _EdgeDarken ("Edge Darken", Range(0, 1)) = 0.4
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _InkColor;
                float _NoiseScale;
                float _InkStrength;
                float _EdgeDarken;
            CBUFFER_END

            // Simple 2D hash noise
            float2 hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            float noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float2 a = hash22(i);
                float2 b = hash22(i + float2(1, 0));
                float2 c = hash22(i + float2(0, 1));
                float2 d = hash22(i + float2(1, 1));

                return lerp(
                    lerp(dot(a - 0.5, f), dot(b - 0.5, f - float2(1, 0)), f.x),
                    lerp(dot(c - 0.5, f - float2(0, 1)), dot(d - 0.5, f - float2(1, 1)), f.x),
                    f.y
                ) * 0.5 + 0.5;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                output.positionHCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDirWS = normalize(GetWorldSpaceNormalizeViewDir(input.positionWS));
                float3 normalWS = normalize(input.normalWS);

                float n = noise2D(input.positionWS.xz * _NoiseScale);
                float n2 = noise2D(input.positionWS.xz * _NoiseScale * 2.3 + 1.7);
                float paper = (n * 0.7 + n2 * 0.3);

                // Vertical gradient: lower/darker = more ink pooling
                float gradient = saturate(1.0 - input.positionWS.y * 0.08);

                // Fresnel edge darkening
                float ndv = saturate(dot(normalWS, viewDirWS));
                float edge = pow(1.0 - ndv, 2.0) * _EdgeDarken;

                float inkMask = saturate((gradient + paper + edge) * _InkStrength);
                float3 color = lerp(_BaseColor.rgb, _InkColor.rgb, inkMask);

                return half4(color, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 DepthVert(float4 position : POSITION) : SV_POSITION
            {
                return TransformObjectToHClip(position.xyz);
            }

            half4 DepthFrag() : SV_Target { return 0; }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
