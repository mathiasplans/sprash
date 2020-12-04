Shader "Unlit/SkySphere" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Tags { "Queue"="Transparent" }
        LOD 100

        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise3D.hlsl"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : POSITION0;
                float3 worldpos : POSITION1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldpos = mul(UNITY_MATRIX_M, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float f = 0.0;

                f += snoise(i.worldpos * 3.0);
                f *= snoise(i.worldpos * 4.0);
                f *= snoise(i.worldpos * 5.0);
                f *= snoise(i.worldpos * 17.0);

                if (f > 0.1)
                    f = 1.0;
                else
                    f = 0.0;

                float4 col = float4(f, f, f + 0.07, 1.0);
                return col;
            }
            ENDCG
        }
    }
}
