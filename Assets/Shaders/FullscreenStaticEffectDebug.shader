Shader "Custom/LightAwareFullScreenGrainEffectDebug"
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
            Name "LightAwareGrainPass"
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

            TEXTURE2D(_ScreenSpaceShadowMap);
            SAMPLER(sampler_ScreenSpaceShadowMap);

            // Random noise function
            float rand(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Debug: Return a solid color to verify the shader is working
                // return half4(1, 1, 1, 1); // Uncomment this line to test

                // Sample the main texture (final rendered image)
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // Sample screen-space shadow map
                float shadow = SAMPLE_TEXTURE2D(_ScreenSpaceShadowMap, sampler_ScreenSpaceShadowMap, IN.uv).r;

                // Calculate light intensity (e.g., using shadow value)
                float lightIntensity = shadow; // Use shadow value as light intensity

                // Add time-based offset to UV coordinates for dynamic motion
                float2 timeOffset = float2(_Time.y * _GrainSpeed, _Time.y * _GrainSpeed);
                float2 dynamicUV = IN.uv * _GrainScale + timeOffset;

                // Generate noise based on dynamic UV coordinates
                float grain = rand(floor(dynamicUV)) * _GrainIntensity * lightIntensity;

                // Apply grain to the screen
                color.rgb += grain;

                return color;
            }
            ENDHLSL
        }
    }
}
