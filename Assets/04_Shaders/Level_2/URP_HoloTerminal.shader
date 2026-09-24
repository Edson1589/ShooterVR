Shader "Custom/Level2/URP_HoloTerminal"
{
    Properties
    {
        [HDR] _ScreenColor("Screen Glow Color", Color) = (0.2, 0.9, 1.2, 1.0)
        [HDR] _DarkColor("Background Color", Color) = (0.01, 0.03, 0.05, 1.0)
        _ScanlineDensity("Scanline Density", Float) = 45.0
        _ScanlineSpeed("Scanline Speed", Float) = 2.0
        _FlickerSpeed("Flicker Frequency", Float) = 15.0
        _FlickerAmount("Flicker Amount", Range(0.0, 0.3)) = 0.08
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
                float2 uv           : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _ScreenColor;
                float4 _DarkColor;
                float _ScanlineDensity;
                float _ScanlineSpeed;
                float _FlickerSpeed;
                float _FlickerAmount;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Scanline wave along UV y or world Y
                float scanline = sin((input.uv.y * _ScanlineDensity) - (_Time.y * _ScanlineSpeed));
                scanline = (scanline * 0.5 + 0.5);
                scanline = pow(scanline, 1.5);

                // Digital micro-flicker
                float flicker = 1.0 - (sin(_Time.y * _FlickerSpeed) * _FlickerAmount);

                half4 col = lerp(_DarkColor, _ScreenColor, scanline);
                col *= flicker;

                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
