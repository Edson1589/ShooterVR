Shader "Custom/UI/URP_HoloCanvasUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HDR] _GlowColor ("Glow / Scan Color", Color) = (0.0, 0.85, 1.0, 1.0)
        _ScanlineDensity ("Scanline Density", Float) = 80.0
        _ScanlineSpeed ("Scanline Speed", Float) = 1.2
        _ScanlineIntensity ("Scanline Intensity", Range(0.0, 0.5)) = 0.08
        _GridDensity ("Grid Density", Float) = 40.0
        _GridIntensity ("Grid Intensity", Range(0.0, 0.3)) = 0.04
        _SweepSpeed ("Sweep Beam Speed", Float) = 0.4
        _SweepIntensity ("Sweep Beam Intensity", Range(0.0, 0.4)) = 0.10
        _UnscaledTime ("Unscaled Time", Float) = 0.0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _GlowColor;
                float4 _MainTex_ST;
                float _ScanlineDensity;
                float _ScanlineSpeed;
                float _ScanlineIntensity;
                float _GridDensity;
                float _GridIntensity;
                float _SweepSpeed;
                float _SweepIntensity;
                float _UnscaledTime;
                float4 _ClipRect;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.worldPosition = input.positionOS;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                // Compute active time (supports both unscaled time and fallback _Time.y)
                float t = _UnscaledTime > 0.001 ? _UnscaledTime : _Time.y;

                // Subtle horizontal holographic scanline wave
                float scan = sin(input.uv.y * _ScanlineDensity - t * _ScanlineSpeed * 3.14159);
                scan = scan * 0.5 + 0.5;
                scan = pow(scan, 2.0);

                // Subtle digital grid overlay
                float2 grid = abs(frac(input.uv * _GridDensity - 0.5) - 0.5) / fwidth(input.uv * _GridDensity);
                float gridLine = 1.0 - min(min(grid.x, grid.y), 1.0);

                // Slow vertical sweep beam
                float sweep = frac(input.uv.y * 0.5 - t * _SweepSpeed);
                float sweepBeam = smoothstep(0.0, 0.15, sweep) * smoothstep(0.4, 0.15, sweep);

                // Composite holographic effect onto RGB while preserving sprite alpha (for rounded corners)
                half3 holo = _GlowColor.rgb * (scan * _ScanlineIntensity + gridLine * _GridIntensity + sweepBeam * _SweepIntensity);
                color.rgb += holo * color.a;

                return color;
            }
            ENDHLSL
        }
    }
    FallBack "UI/Default"
}
