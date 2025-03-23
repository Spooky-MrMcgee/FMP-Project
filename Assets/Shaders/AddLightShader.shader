Shader "Custom/LitDynamicStaticGrainRealtimeLocalizedGrainOnlyAAA"
{
    Properties
    {
        _GrainIntensity("Grain Intensity", Range(0, 1)) = 0.1
        _GrainScale("Grain Scale", Float) = 100.0
        _BaseTexture("Base Texture", 2D) = "white" {}
        _GrainSpeed("Grain Speed", Float) = 1.0 // Speed of the grain motion
        _GrainThreshold("Grain Threshold", Range(0, 1)) = 0.5 // Threshold for light intensity to trigger grain
        _GrainFalloff("Grain Falloff", Range(0.1, 50)) = 10.0 // Controls how sharply the grain effect fades at the edges
        _ColorDampening("Color Dampening", Range(0, 1)) = 0.5 // Dampens the intensity of the light color at the edges
        _TargetLightIndices("Target Light Indices", Vector) = (-1, -1, -1, -1) // Indices of the GrainLights (up to 4 lights)
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
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

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
            float _GrainThreshold;
            float _GrainFalloff;
            float _ColorDampening;
            float4 _TargetLightIndices; // Indices of the GrainLights (up to 4 lights)
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

            // Check if a light index matches any of the target indices
            bool IsGrainLight(int lightIndex)
            {
                return lightIndex == _TargetLightIndices.x ||
                       lightIndex == _TargetLightIndices.y ||
                       lightIndex == _TargetLightIndices.z ||
                       lightIndex == _TargetLightIndices.w;
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

                // Initialize grain-specific light intensity and color
                float grainLightIntensity = 0.0;
                float3 grainLightColor = float3(0, 0, 0);

                // Loop through additional lights (realtime only)
                int additionalLightsCount = GetAdditionalLightsCount();
                for (int i = 0; i < additionalLightsCount; ++i)
                {
                    Light additionalLight = GetAdditionalLight(i, IN.positionWS);

                    // Only process GrainLights
                    if (IsGrainLight(i))
                    {
                        float3 additionalLightDir = normalize(additionalLight.direction);
                        float3 normalWS = normalize(IN.normalWS);
                        float additionalNdotL = saturate(dot(normalWS, additionalLightDir));

                        // Calculate additional light intensity based on luminance
                        float additionalLightIntensity = Lum(additionalLight.color) * additionalNdotL * additionalLight.shadowAttenuation;

                        // Apply a falloff to the light intensity to localize the grain effect
                        float falloff = pow(additionalNdotL, _GrainFalloff);
                        additionalLightIntensity *= falloff;

                        // Accumulate grain-specific light intensity and color
                        grainLightIntensity += additionalLightIntensity;
                        grainLightColor += additionalLight.color * additionalLightIntensity;
                    }
                }

                // Normalize the accumulated grain light color
                if (grainLightIntensity > 0)
                {
                    grainLightColor /= grainLightIntensity;
                }

                // Calculate the grain effect based on grain light intensity and falloff
                float grain = 0.0;
                if (grainLightIntensity > _GrainThreshold)
                {
                    // Add time-based offset to UV coordinates for dynamic motion
                    float2 timeOffset = float2(_Time.y * _GrainSpeed, _Time.y * _GrainSpeed);
                    float2 dynamicUV = IN.uv * _GrainScale + timeOffset;

                    // Generate noise based on dynamic UV coordinates
                    grain = rand(floor(dynamicUV)) * _GrainIntensity * grainLightIntensity;
                }

                // Calculate standard lighting for the object (ignoring GrainLights)
                Light mainLight = GetMainLight(IN.shadowCoord);
                float3 normalWS = normalize(IN.normalWS);
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                float3 mainLightColor = mainLight.color * NdotL * mainLight.shadowAttenuation;

                // Combine main light and additional lights for standard lighting (ignoring GrainLights)
                float3 standardLighting = mainLightColor;
                for (int i = 0; i < additionalLightsCount; ++i)
                {
                    if (!IsGrainLight(i)) // Skip GrainLights
                    {
                        Light additionalLight = GetAdditionalLight(i, IN.positionWS);
                        float3 additionalLightDir = normalize(additionalLight.direction);
                        float additionalNdotL = saturate(dot(normalWS, additionalLightDir));
                        float additionalLightIntensity = Lum(additionalLight.color) * additionalNdotL * additionalLight.shadowAttenuation;
                        standardLighting += additionalLight.color * additionalLightIntensity;
                    }
                }

                // Dampen the grain light color at the edges of the grain effect
                float dampeningFactor = smoothstep(_GrainThreshold - _GrainFalloff, _GrainThreshold + _GrainFalloff, grainLightIntensity);
                dampeningFactor = lerp(1.0, _ColorDampening, dampeningFactor);
                grainLightColor *= dampeningFactor;

                // Blend grain color between base texture color (black static) and the dampened grain light color
                half3 grainColor = lerp(baseColor.rgb, grainLightColor, grain);

                // Combine standard lighting with the grain effect
                half3 finalColor = baseColor.rgb * standardLighting; // Start with standard lighting
                finalColor = lerp(finalColor, grainColor, grain); // Apply the grain effect

                return half4(finalColor, baseColor.a);
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
