Shader "Custom/VR/URP_VRDamageVignette"
{
    Properties
    {
        [HDR] _DamageColor ("Damage Color", Color) = (1.0, 0.08, 0.08, 1.0)
        _VignetteAmount ("Vignette Amount", Range(0.0, 1.0)) = 0.0
        _FlashAmount ("Flash Amount", Range(0.0, 1.0)) = 0.0
        _VignetteInner ("Vignette Inner Radius", Range(0.1, 0.8)) = 0.32
        _VignetteOuter ("Vignette Outer Radius", Range(0.3, 1.5)) = 0.72
        _ScanlineDensity ("Scanline Density", Float) = 280.0
        _ScanlineIntensity ("Scanline Intensity", Range(0.0, 0.3)) = 0.06
        _EdgeGlowBoost ("Edge Glow Boost", Range(1.0, 3.0)) = 1.6
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+500"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "DamageVignettePass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _DamageColor;
                float _VignetteAmount;
                float _FlashAmount;
                float _VignetteInner;
                float _VignetteOuter;
                float _ScanlineDensity;
                float _ScanlineIntensity;
                float _EdgeGlowBoost;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Compute distance from center of the screen
                float2 centered = input.uv - 0.5;
                float dist = length(centered);

                // Radial perimeter vignette
                float vignette = smoothstep(_VignetteInner, _VignetteOuter, dist) * _VignetteAmount;

                // Full-screen immediate red flash
                float flash = _FlashAmount * 0.42;

                // Subtle tactical visor scanlines
                float scan = sin(input.uv.y * _ScanlineDensity) * 0.5 + 0.5;
                float techFactor = 1.0 - (scan * _ScanlineIntensity);

                // Total transparency
                float alpha = saturate((vignette + flash) * techFactor);

                // Edge glow boost for intense HDR feel
                half3 color = _DamageColor.rgb * lerp(1.0, _EdgeGlowBoost, vignette);

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
