Shader "Unlit/EnergyShield" {
    Properties {
        _Color ("Shield color", Color) = (1, 1, 1, 0.5)
    }
    SubShader {
        Tags { "Queue"="Transparent" }
        LOD 100

        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _Color;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 vertex : POSITION0;
                float3 worldposition : POSITION1;
                float3 worldnormal : NORMAL0;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldposition = mul(UNITY_MATRIX_M, v.vertex);
                o.worldnormal = normalize(mul((float3x3) UNITY_MATRIX_M, v.normal));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float3 camdir = normalize(_WorldSpaceCameraPos -  i.worldposition);
                float incidentality = 1.0 - (dot(normalize(i.worldnormal), camdir) / 2.0 + 0.5);
                incidentality = lerp(0.2, 1.0, incidentality);
                // sample the texture
                fixed4 col = incidentality * _Color;
                return col;
            }
            ENDCG
        }
    }
}
