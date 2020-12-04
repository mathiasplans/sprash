Shader "Unlit/AsteroidExplosion" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : POSITION0;
                float4 color : COLOR;
                float3 worldcorrected : POSITION1;
                float3 worldnormal : NORMAL0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldcorrected = mul(UNITY_MATRIX_M, v.vertex) + float3(0.0, 0.0, 9.0);
                o.color = v.color;
                o.worldnormal = normalize(mul((float3x3) UNITY_MATRIX_M, v.normal));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // sample the texture
                float3 light = _WorldSpaceLightPos0;
                float3 lightdir = light - i.worldcorrected;
                float rcol = dot(normalize(i.worldnormal), normalize(lightdir)) / 2 + 0.5;
                rcol = min(1.0, rcol + 0.1);

                fixed4 col = float4(rcol * i.color.xyz, i.color.w);
                return col;
            }

            ENDCG
        }
    }
}
