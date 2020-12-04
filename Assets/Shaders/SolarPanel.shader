Shader "Unlit/SolarPanel" {
    Properties {
        _Color ("Panel color", Color) = (1, 1, 1, 1)
        _StreakColor("Streak color", Color) = (1, 1, 1, 1)
        _StripeProportion("Secondary color duty cycle", Range(0.0, 100.0)) = 50.0
        _Scale("Scale", Float) = 100.0
    }
    SubShader {
        Tags { "LightMode"="ForwardBase" }
        LOD 200

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise3D.hlsl"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : POSITION0;
                float4 localvertex : POSITION1;
                float2 uv : TEXCOORD0;
                float3 worldnormal : NORMAL0;
                float3 worldpos : POSITION2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 _Color, _StreakColor;
            float _InverseScale, _StripeProportion, _Scale;

            v2f vert (appdata v) {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.localvertex = v.vertex;
                o.uv = v.uv;
                o.worldpos = mul(UNITY_MATRIX_M, v.vertex);
                o.worldnormal = normalize(mul((float3x3) UNITY_MATRIX_M, v.normal));
                return o;
            }

            float4 streak(float x) {
                x += 10;
                float mo = x % 10;
                if (mo > _StripeProportion / 100.0 * 10)
                    return _StreakColor;

                return _Color;
            }

            fixed4 frag (v2f i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i);

                float3 normal = normalize(i.worldnormal);
                float3 eyedir = normalize(_WorldSpaceCameraPos - i.worldpos);

                // Surface color
                float x = (i.uv.x + _Scale) * _Scale;
                float4 col = streak(x);

                // Light direction
                float3 lightdir = normalize(_WorldSpaceLightPos0 - i.worldpos);
                float lightangle = dot(normal, lightdir);

                // Diffuse
                float3 diffuse = max(0, lightangle) * col;

                // Specular
                float3 lightref = reflect(-lightdir, normal);
                float specangle = dot(normal, lightref);
                float specular = pow(max(0, specangle), 20.0);

                if (specular > 0.15)
                    specular = 1;

                float f = 0.0;

#ifdef UNITY_INSTANCING_ENABLED
                f += snoise(i.localvertex.xyz * 500.0 + unity_InstanceID);
                f *= snoise(i.localvertex.xyz * 700.0 + unity_InstanceID);
                f *= snoise(i.localvertex.xyz * 900.0 + unity_InstanceID);
                f *= snoise(i.localvertex.xyz * 600.0 + unity_InstanceID);
#endif

                if (f > 0.08)
                    f = 1.0;
                else
                    f = 0.0;

                // Stars
                float skyinstensity = 1.0 - pow(max(0, specangle), 0.6);
                float3 sky = skyinstensity * float3(f, f, f + 0.03);

                float3 color = lerp(diffuse, sky, 0.5) + specular * float3(0.90, 0.90, 1.0);

                return float4(color, 0.0);
            }

            ENDCG
        }
    }
}
