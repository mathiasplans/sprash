Shader "Unlit/Aluminium" {
    Properties {
        _Color ("Diffuse color", Color) = (1, 1, 1, 1)
        // _SpecColor("Specular highlight color", Color) = (1, 1, 1, 1)
        _TangentVector ("Tangent Vector", Vector) = (1.0, 0.0, 0.0, 0.0)
        _AlphaX ("Roughness in brush direction", Float) = 1.0
        _AlphaY ("Roughness orthogonal to brush direction", Float) = 1.0
        _Ambient ("Ambient light", Range (0.0, 1.0)) = 0.0
        _BrushBoldnessDiffuse ("Brush Boldness for Diffuse", Range (0, 100)) = 5
        _BrushBoldnessSpecular ("Brush Boldness for Specular", Range (0, 100)) = 5
        _BrushScale ("Brush Scale", Float) = 30
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
            #include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise2D.hlsl"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : POSITION0;
                float3 normal : NORAML0;
                float3 worldpos : POSITION1;
                float3 worldnormal : NORMAL1;
                float3 tangent : NORAMAL2;
                float3 worldtangent : NORMAL3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 _Color;
            float3 _TangentVector;
            float _AlphaX;
            float _AlphaY;
            float _Ambient;

            float _BrushBoldnessDiffuse;
            float _BrushBoldnessSpecular;
            float _BrushScale;

            v2f vert (appdata v) {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldpos = mul(UNITY_MATRIX_M, v.vertex);
                o.normal = v.normal;
                o.worldnormal = normalize(mul((float3x3) UNITY_MATRIX_M, v.normal));
                o.tangent = normalize(float3(0.0, _TangentVector.xy));
                o.worldtangent = normalize(mul((float3x3) UNITY_MATRIX_M, o.tangent));

                return o;
            }

            float sinusoid(float3 position) {
                float tangentAngle = atan2(_TangentVector.y, _TangentVector.x);

                float2 rotpos = float2(0.0, 0.0);
                // position.y /= _BrushScale;
                rotpos.x = position.x * cos(tangentAngle) - position.y * sin(tangentAngle);
                rotpos.y = position.x * sin(tangentAngle) + position.y * cos(tangentAngle);
                rotpos.y *= _BrushScale;
                return snoise(rotpos);
            }

            fixed4 frag (v2f i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i);

                // Normal and the tangent, in worldspace
                float3 normal = normalize(i.worldnormal);
                float3 tangent = normalize(i.worldtangent);

                // Camera and light
                float3 eyedir = normalize(_WorldSpaceCameraPos - i.worldpos);
                float3 lightdir = normalize(_WorldSpaceLightPos0);

                // Intermediate vectors
                float3 halfway = normalize(lightdir + eyedir);
                float3 binormal = cross(normal, tangent);

                // Angle between normal and light
                float lightangle = dot(lightdir, normal);

                // Ambient light
                float3 ambient = _Ambient * _Color.xyz;

                // Diffuse reflection
                float3 diffuse = _LightColor0 * _Color.xyz * max(0.0, lightangle);

                // Specular reflection
                float3 specular;
                if (lightangle < 0.0) {
                    // Light source is not visible by the fragment
                    specular = float3(0.0, 0.0, 0.0);
                }

                else {
                    float halfwayangle = dot(halfway, normal);
                    float eyeangle = dot(eyedir, normal);

                    float halfway_tangent_ax = dot(halfway, tangent) / _AlphaX;
                    float halfway_binormal_ay = dot(halfway, binormal) / _AlphaY;

                    specular = /*_SpecColor.xyz*/ float3(1.0, 1.0, 1.0) * sqrt(max(0.0, lightangle / eyeangle))
                              * exp(-2.0 * (halfway_tangent_ax * halfway_tangent_ax + halfway_binormal_ay * halfway_binormal_ay)
                              / (1.0 + halfwayangle));
                }

                // Get brush texture
                float brush = sinusoid(i.worldpos);
                float brush_diffuse = lerp(1.0 - _BrushBoldnessDiffuse / 100.0, 1.0, brush);
                float brush_specular = lerp(1.0 - _BrushBoldnessSpecular / 100.0, 1.0, brush);

                fixed4 col = float4(ambient + brush_diffuse * diffuse + brush_specular * specular, 1.0);
                return col;
            }

            ENDCG
        }
    }
}
