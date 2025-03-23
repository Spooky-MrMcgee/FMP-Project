Shader "Custom/TransparentDynamicGrain"
{
    Properties
    {
        _GrainIntensity("Grain Intensity", Range(0, 1)) = 0.1
        _GrainScale("Grain Scale", Float) = 100.0
        _BaseTexture("Base Texture", 2D) = "white" {}
        _GrainSpeed("Grain Speed", Float) = 1.0
        _GrainThreshold("Grain Threshold", Range(0, 1)) = 0.5
        _GrainFalloff("Grain Falloff", Range(0.1, 50)) = 10.0
        _ColorDampening("Color Dampening", Range(0, 1)) = 0.5
        _TargetLightIndices("Target Light Indices", Vector) = (-1, -1, -1, -1)
        _Transparency("Transparency", Range(0, 1)) = 0.0
        _GrainVisibility("Grain Visibility", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off // Disable depth writing for transparency
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };
            TEXTURE2D(_BaseTexture); SAMPLER(sampler_BaseTexture);
            CBUFFER_START(UnityPerMaterial)
            float _GrainIntensity, _GrainScale, _GrainSpeed, _GrainThreshold, _GrainFalloff, _ColorDampening, _Transparency, _GrainVisibility;
            float4 _TargetLightIndices;
            CBUFFER_END
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }
            float randomNoise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }
            float computeLightEffect(float3 worldPos, float3 normalWS)
            {
                Light mainLight = GetMainLight();
                float distanceFactor = saturate(1.0 - length(worldPos - mainLight.position) / _GrainFalloff);
                float intensity = saturate(dot(normalWS, mainLight.direction)) * mainLight.shadowAttenuation * mainLight.distanceAttenuation * distanceFactor;
                intensity = smoothstep(_GrainThreshold, 1.0, intensity);
                for (int i = 0; i < 4; i++)
                {
                    if (_TargetLightIndices[i] >= 0)
                    {
                        Light additionalLight = GetAdditionalLight((uint)_TargetLightIndices[i], worldPos);
                        float distFactor = saturate(1.0 - length(worldPos - additionalLight.position) / _GrainFalloff);
                        float lightEffect = saturate(dot(normalWS, additionalLight.direction)) * additionalLight.distanceAttenuation * additionalLight.shadowAttenuation * distFactor;
                        lightEffect = smoothstep(_GrainThreshold, 1.0, lightEffect);
                        intensity += lightEffect;
                    }
                }
                return saturate(intensity);
            }
            float4 frag(Varyings IN) : SV_Target
            {
                float grain = randomNoise(IN.uv * _GrainScale + _Time.y * _GrainSpeed);
                float lightEffect = computeLightEffect(IN.worldPos, IN.normalWS);
                grain *= lightEffect * _GrainIntensity;
                float4 baseColor = SAMPLE_TEXTURE2D(_BaseTexture, sampler_BaseTexture, IN.uv);
                float grainAlpha = _GrainVisibility * grain;
                float finalAlpha = max(_Transparency, grainAlpha * lightEffect);
                return float4(lerp(baseColor.rgb, float3(1, 1, 1), grain), finalAlpha);
            }
            ENDHLSL
        }
    }
}
