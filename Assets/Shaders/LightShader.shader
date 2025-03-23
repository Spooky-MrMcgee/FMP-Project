Shader "Custom/LitDynamicStaticGrain"
{
    Properties
    {
        _GrainIntensity("Grain Intensity", Range(0, 1)) = 0.1
        _GrainScale("Grain Scale", Float) = 100.0
        _BaseTexture("Base Texture", 2D) = "white" {}
        _GrainSpeed("Grain Speed", Float) = 1.0 // Speed of the grain motion
    }
        SubShader
        {
            Tags { "RenderPipeline" = "UniversalRenderPipeline" }
            Pass
            {
                Name "ForwardLit"
                Tags { "LightMode" = "UniversalForward" }

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag

            // Enable shadow casting and receiving
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3; // Shadow coordinates
            };

            float _GrainIntensity;
            float _GrainScale;
            float _GrainSpeed;
            sampler2D _BaseTexture;

            // Random noise function
            float rand(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Calculate luminance (brightness) of a color
            float Lum(float3 color)
            {
                return dot(color, float3(0.2126, 0.7152, 0.0722)); // Standard luminance formula
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);

                // Calculate shadow coordinates
                OUT.shadowCoord = TransformWorldToShadowCoord(OUT.positionWS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample the base texture
                half4 baseColor = tex2D(_BaseTexture, IN.uv);

                // Calculate lighting
                //Light additionalLights = GetAdditionalLight(IN.shadowCoord);
                Light mainLight = GetMainLight(IN.shadowCoord); // Apply shadows
                float3 lightDir = normalize(mainLight.direction);
                float3 normalWS = normalize(IN.normalWS);
                float NdotL = saturate(dot(normalWS, lightDir));

                // Calculate light intensity based on luminance
                float lightIntensity = Lum(mainLight.color) * NdotL * mainLight.shadowAttenuation; // Include shadow attenuation

                // Add time-based offset to UV coordinates for dynamic motion
                float2 timeOffset = float2(_Time.y * _GrainSpeed, _Time.y * _GrainSpeed);
                float2 dynamicUV = IN.uv * _GrainScale + timeOffset;

                // Generate noise based on dynamic UV coordinates
                float grain = rand(floor(dynamicUV)) * _GrainIntensity * lightIntensity;

                // Blend grain color between base texture color (black static) and light color (white static)
                half3 grainColor = lerp(baseColor.rgb, mainLight.color.rgb, grain);

                // Apply grain to the base color
                half4 finalColor = half4(grainColor, baseColor.a);

                return finalColor;
            }
            ENDHLSL
        }

            // Shadow casting pass
            Pass
            {
                Name "ShadowCaster"
                Tags { "LightMode" = "ShadowCaster" }

                HLSLPROGRAM
                #pragma vertex ShadowPassVertex
                #pragma fragment ShadowPassFragment

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

                struct Attributes
                {
                    float4 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                };

                struct Varyings
                {
                    float4 positionHCS : SV_POSITION;
                };

                Varyings ShadowPassVertex(Attributes IN)
                {
                    Varyings OUT;
                    OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                    return OUT;
                }

                half4 ShadowPassFragment(Varyings IN) : SV_Target
                {
                    return 0;
                }
                ENDHLSL
            }
        }
}