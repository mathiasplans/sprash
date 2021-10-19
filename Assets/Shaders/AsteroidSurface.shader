Shader "Unlit/AsteroidSurface" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Seed ("Seed", Float) = 0.0
    }

    SubShader {
        Tags { "RenderType"="Opaque" "DisableBatching"="True"}
        LOD 100

        Pass {
            CGPROGRAM
            // Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members normal)
            #pragma exclude_renderers d3d11

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #pragma multi_compile SNOISE
            #pragma multi_compile _ THREED

            #include "UnityCG.cginc"
            #include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise3D.hlsl"

            struct appdata  {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: NORMAL;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : POSITION0;
                float3 normal : NORMAL0;
                float4 worldcoord : POSITION1;
                float3 worldnormal : NORMAL1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Seed;

            // This is vertex shader
            v2f vert (appdata v) {
                // v.vertex.xyz += snoise(v.vertex.xyz * 1.1 + _Seed) * 0.25;
                // v.vertex.xyz += snoise(v.vertex.xyz * 0.5 + _Seed) * 0.5;
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldcoord = mul(UNITY_MATRIX_M, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                o.normal = v.normal;
                o.worldnormal = normalize(mul((float3x3) UNITY_MATRIX_M, v.normal));
                return o;
            }

            // This is fragment shader
            fixed4 frag (v2f i) : SV_Target {
                // sample the texture
                // fixed4 col = tex2D(_MainTex, i.uv);
                // float3 src = float3(11.0, 7.0, -10.0);
                float3 light = _WorldSpaceLightPos0;
                float3 lightdir = light - i.worldcoord;
                float rcol = dot(normalize(i.worldnormal), normalize(lightdir)) / 2 + 0.5;
                float3 c = float3(rcol, rcol, rcol);

                fixed4 col = float4(c + float(0.1), 1.0);
                //fixed4 col = float4(1.0, 1.0, 1.0, 1.0);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }

            ENDCG
        }
    }
}
