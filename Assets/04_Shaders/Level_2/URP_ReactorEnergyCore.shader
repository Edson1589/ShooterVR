Shader "Custom/Level2/URP_ReactorEnergyCore"
{
    Properties
    {
        [HDR] _BaseColor("Base Color", Color) = (0.8, 0.05, 0.02, 1.0)
        [HDR] _CoreColor("Core Energy Color", Color) = (1.5, 0.3, 0.1, 1.0)
        [HDR] _FresnelColor("Fresnel Rim Color", Color) = (2.0, 0.8, 0.2, 1.0)
        _FresnelPower("Fresnel Power", Range(0.5, 5.0)) = 2.0
        _BandSpeed("Band Flow Speed", Float) = 1.5
        _BandFrequency("Band Frequency", Float) = 12.0
        _PulseSpeed("Core Pulse Speed", Float) = 3.0
        _PulseIntensity("Pulse Intensity", Range(0.0, 1.0)) = 0.4
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
                UNITY_VERTEX_OUTPUT_STEREO
                float3 positionWS   : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float2 uv           : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _CoreColor;
                float4 _FresnelColor;
                float _FresnelPower;
                float _BandSpeed;
                float _BandFrequency;
                float _PulseSpeed;
                float _PulseIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

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
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // View direction & Fresnel
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 normalWS = normalize(input.normalWS);
                float NdotV = saturate(dot(normalWS, viewDirWS));
                float fresnel = pow(1.0 - NdotV, _FresnelPower);

                // Vertical moving energy bands along world Y or local UV
                float wave = sin((input.positionWS.y * _BandFrequency) + (_Time.y * _BandSpeed));
                wave = smoothstep(-0.2, 0.8, wave);

                // Periodic pulse
                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseIntensity;

                // Color composite
                half4 col = lerp(_BaseColor, _CoreColor, wave);
                col += _FresnelColor * fresnel;
                col *= pulse;

                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
