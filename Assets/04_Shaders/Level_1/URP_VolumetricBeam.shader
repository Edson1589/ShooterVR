Shader "Custom/Level1/URP_VolumetricBeam"
{
    Properties
    {
        [HDR] _Color("Beam Tint", Color) = (1.0, 0.75, 0.45, 0.25)
        _FalloffPower("Vertical Falloff Power", Float) = 1.8
        _EdgeFalloff("Radial Edge Falloff", Float) = 2.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend One One
            ZWrite Off
            Cull Off

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
                float4 _Color;
                float _FalloffPower;
                float _EdgeFalloff;
            CBUFFER_END

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

                // View rim effect (fresnel-like edge fading)
                float3 viewDir = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 normal = normalize(input.normalWS);
                float rim = abs(dot(normal, viewDir));
                rim = pow(rim, _EdgeFalloff);

                // Vertical falloff along UV.y
                float verticalFade = pow(saturate(input.uv.y), _FalloffPower);

                half4 col = _Color * (rim * verticalFade);
                return col;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
