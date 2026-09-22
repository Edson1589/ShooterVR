Shader "Custom/Level1/URP_IncandescentFurnace"
{
    Properties
    {
        [HDR] _CrustColor("Cool Iron Color", Color) = (0.1, 0.02, 0.02, 1.0)
        [HDR] _HeatColor("Incandescent Red", Color) = (1.5, 0.15, 0.02, 1.0)
        [HDR] _CoreColor("Molten Gold Core", Color) = (2.2, 1.1, 0.2, 1.0)
        _HeatIntensity("Heat Intensity", Range(0.5, 3.0)) = 1.3
        _FlickerSpeed("Thermal Flicker Speed", Float) = 4.0
        _NoiseScale("Heat Noise Scale", Float) = 8.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 100

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float2 uv           : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _CrustColor;
                float4 _HeatColor;
                float4 _CoreColor;
                float _HeatIntensity;
                float _FlickerSpeed;
                float _NoiseScale;
            CBUFFER_END

            // Fast procedural pseudo-noise
            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float smoothNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = normInputs.normalWS;
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float t = _Time.y * _FlickerSpeed;
                // Thermal pulsation waves
                float2 uvSample = input.uv * _NoiseScale + float2(sin(t * 0.4) * 0.1, t * 0.15);
                float n = smoothNoise(uvSample);
                
                // Secondary flicker
                float flicker = 0.9 + 0.1 * sin(t * 1.7 + input.positionWS.x * 2.0);

                // Multi-temperature gradient
                float heatLevel = saturate(n * flicker * _HeatIntensity);
                half4 col;
                if (heatLevel < 0.5)
                {
                    col = lerp(_CrustColor, _HeatColor, heatLevel * 2.0);
                }
                else
                {
                    col = lerp(_HeatColor, _CoreColor, (heatLevel - 0.5) * 2.0);
                }

                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
