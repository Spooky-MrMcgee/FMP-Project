Shader "Custom/LightAwareFullScreenGrainEffect"
{
    Properties
    {
        _GrainIntensity("Grain Intensity", Range(0, 1)) = 0.1
        _GrainScale("Grain Scale", Float) = 100.0
        _GrainSpeed("Grain Speed", Float) = 1.0
    }
        SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" }
        Pass
        {
            Name "FullScreenGrainPass"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _GrainIntensity;
            float _GrainScale;
            float _GrainSpeed;

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_ScreenSpaceShadowmapTexture);
            SAMPLER(sampler_ScreenSpaceShadowmapTexture);

            // Random noise function
            float rand(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Ensure UV coordinates are in the [0, 1] range
                OUT.uv = IN.uv;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Debug: Return a solid red color to verify the shader is working
                return half4(1, 0, 0, 1); // Uncomment to check if the shader is executed

                // Sample the main texture (final rendered image)
                half4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // Sample the screen-space shadow map (if available)
                float shadow = 1.0; // Default to no shadow
                #if defined(_SCREEN_SPACE_SHADOWS)
                    shadow = SAMPLE_TEXTURE2D(_ScreenSpaceShadowmapTexture, sampler_ScreenSpaceShadowmapTexture, IN.uv).r;
                #endif

                    // Debug: Visualize shadow map
                    // return half4(shadow, shadow, shadow, 1); // Uncomment to check shadow map

                    // Add time-based offset to UV coordinates for dynamic motion
                    float2 timeOffset = float2(_Time.y * _GrainSpeed, _Time.y * _GrainSpeed);
                    float2 dynamicUV = IN.uv * _GrainScale + timeOffset;

                    // Generate noise based on dynamic UV coordinates
                    float grain = rand(floor(dynamicUV)) * _GrainIntensity * shadow;

                    // Blend grain color between base texture color (black static) and light color (white static)
                    half3 grainColor = lerp(mainTexColor.rgb, float3(1, 1, 1), grain); // Blend with white for grain effect
                    half4 finalColor = half4(grainColor, mainTexColor.a);

                    return finalColor;
                }
                ENDHLSL
            }
    }
}