Shader "Custom/BallTracer" {
    Properties {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _TrailColor("Trail Color", Color) = (1, 0.5, 0, 1)
        _TrailLength("Trail Length", Range(0, 5)) = 1.5
        _TrailFalloff("Trail Falloff", Range(0, 5)) = 2
        _Velocity("Velocity", Vector) = (0,0,0,0) // Pass velocity from script
    }

    SubShader {
        Tags {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _TrailColor;
                float _TrailLength;
                float _TrailFalloff;
                float4 _Velocity;
            CBUFFER_END

            Varyings vert(Attributes IN) {
                Varyings OUT;
                
                // Offset vertices based on velocity direction
                float3 velocityDir = normalize(_Velocity.xyz);
                float3 offset = velocityDir * _TrailLength * length(_Velocity.xyz);
                IN.positionOS.xyz += offset;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = vertexInput.positionCS;
                OUT.positionWS = vertexInput.positionWS;
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                // Fade trail based on velocity magnitude
                float speed = length(_Velocity.xyz);
                float alpha = saturate(speed / _TrailFalloff);
                
                // Blend base color with trail color
                half4 col = lerp(_BaseColor, _TrailColor, alpha);
                col.a *= alpha; // Fade transparency
                return col;
            }
            ENDHLSL
        }
    }
}