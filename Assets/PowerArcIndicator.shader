Shader "Unlit/PowerArcIndicator"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5
        _Thickness ("Stroke Thickness", Range(0, 0.5)) = 0.1
        _Radius ("Radius", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _FillAmount;
            float _Thickness;
            float _Radius;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Calculate the distance from the center
                float2 center = float2(0.5, 0.5);
                float2 dir = i.uv - center;
                float distance = length(dir);

                // Calculate the angle in radians (-1 to 1 normalized range)
                float angle = atan2(dir.y, dir.x) / 3.14159;
                
                // Normalize the angle to [0, 1] and only show the arc from -90 to 90 degrees
                angle = (angle + 1.0) / 2.0;

                // Determine if the pixel is within the half-circle range
                bool withinRadius = distance <= _Radius && distance >= (_Radius - _Thickness);
                bool withinArc = angle <= _FillAmount;

                // Apply the arc fill and stroke thickness
                if (withinRadius && withinArc)
                {
                    return _Color;
                }

                // Return transparent if outside the arc
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}
