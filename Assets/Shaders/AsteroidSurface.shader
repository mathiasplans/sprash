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
                float4 localPos : POSITION2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Seed;

            // This is vertex shader
            v2f vert (appdata v) {
                // v.vertex.xyz += snoise(v.vertex.xyz * 1.1 + _Seed) * 0.25;
                // v.vertex.xyz += snoise(v.vertex.xyz * 0.5 + _Seed) * 0.5;
                v2f o;
                o.localPos = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldcoord = mul(UNITY_MATRIX_M, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                o.normal = v.normal;
                o.worldnormal = normalize(mul((float3x3) UNITY_MATRIX_M, v.normal));
                return o;
            }

            float fnoise(float3 p, float seed, int octaves, float persistence) {
                  // Total value so far
                    float total = 0.0;

                    // Accumulates highest theoretical amplitude
                    float maxAmplitude = 0.0;

                    float amplitude = 1.0;
                    for (int i = 0; i < octaves; i++) {
                        // Get the noise sample
                        total += snoise(p + seed) * amplitude;

                        // Make the wavelength twice as small
                        p *= 2.0;

                        // Add to our maximum possible amplitude
                        maxAmplitude += amplitude;

                        // Reduce amplitude according to persistence for the next octave
                        amplitude *= persistence;
                    }

                // Scale the result by the maximum amplitude
                return (total / maxAmplitude);
            }

            // This is fragment shader
            fixed4 frag (v2f i) : SV_Target {
                // sample the texture
                // fixed4 col = tex2D(_MainTex, i.uv);
                // float3 src = float3(11.0, 7.0, -10.0);

                float3 normal = normalize(i.worldnormal);
                float3 position = i.localPos.xyz;
                float3 noisePosition = position / 5.0;
                float3 worldPosition = i.worldcoord.xyz;

                // Calculate f by combining multiple noise layers using different density
                float f = 0.0;
                f += 2.5 * fnoise(0.5 * noisePosition, _Seed, 10, 0.7);
                f += 2.3 * fnoise(1.0 * noisePosition, _Seed, 8, 0.6);
                f += 2.7 * fnoise(2.0 * noisePosition, _Seed, 5, 0.2);
                f += 1.5 * fnoise(5.0 * noisePosition, _Seed, 5, 0.5);
                f += 1.1 * fnoise(8.0 * noisePosition, _Seed, 5, 0.8);

                float normalizer = 5.5;

                f += normalizer;
                f /= 2.0 * normalizer;

                float3 light = _WorldSpaceLightPos0;
                float3 lightdir = light - i.worldcoord;
                float rcol = dot(normal, normalize(lightdir)) / 2 + 0.5;
                float3 c = float3(rcol, rcol, rcol);

                fixed4 col = float4((c * f + float(0.1)), 1.0);
                //fixed4 col = float4(1.0, 1.0, 1.0, 1.0);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }

            ENDCG
        }
    }
}
