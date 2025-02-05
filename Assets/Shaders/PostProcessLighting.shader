Shader "Custom/FullscreenDynamicGrain"
{
    Properties
    {
        _GrainIntensity("Grain Intensity", Range(0, 1)) = 0.1
        _GrainScale("Grain Scale", Float) = 100.0
        _GrainSpeed("Grain Speed", Float) = 1.0 // Speed of the grain motion
    }
        SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            Name "FullscreenGrain"
            ZTest Always // Ensure the shader runs regardless of depth
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Screen texture (provided by URP)
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float _GrainIntensity;
            float _GrainScale;
            float _GrainSpeed;

            // Random noise function
            float rand(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Full-screen quad vertex shader
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

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample the screen texture
                half4 baseColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv);

                // Add time-based offset to UV coordinates for dynamic motion
                float2 timeOffset = float2(_Time.y * _GrainSpeed, _Time.y * _GrainSpeed);
                float2 dynamicUV = IN.uv * _GrainScale + timeOffset;

                // Generate noise based on dynamic UV coordinates
                float grain = rand(floor(dynamicUV)) * _GrainIntensity;

                // Apply grain to the base color
                half3 grainColor = lerp(baseColor.rgb, float3(1, 1, 1), grain); // Blend between base color and white

                // Output the final color
                return half4(grainColor, baseColor.a);
                //return half4(0, 1, 0, 1);
            }


            ENDHLSL
        }
    }
}