Shader "Custom/DynamicStaticGrain"
{
	Properties
	{
		GrainIntensity("Grain Intensity", Range(0, 1)) = 0.1
		GrainScale("Grain Scale", Float) = 100.0
		BaseTexture("Base Texture", 2D) = "white" {}
		GrainSpeed("Grain Speed", Float) = 1.0
	}

		SubShader
		{
			Tags {"RenderPipleine" = "UniversalRenderPipeline"}
			Pass
			{
				Name "ForwardLit"
				Tags {"LightMode" = "UniversalForward"}

				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag

			// Enable shadow casting and receving
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
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

			float GrainIntensity;
			float GrainScale;
			float GrainSpeed;
			sampler2D BaseTexture;

			// Random noise
			float rand(float2 uv)
			{
				return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453); // Standard algorithm for noise
			}

			// Calculate brightness of a colour
			float Lum(float3 color)
			{
				return dot(colour, float3(0.2126, 0.7152, 0.0722)); // Standard luminance formula located in lighting.hlsl
			}

			Varyings vert(Attributes IN)
			{
				Varyings OUT;
				OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
				OUT.uv = IN.uv;
				OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
				OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);

				// Calculate shadow coords
				OUT.shadowCoord = TransformWorldToShadowCoord(OUT.positionWS);

				return OUT;
			}
			
			half4 frag(Varyings IN) : SV_Target
			{
				half4 baseColour = tex2D(BaseTexture, IN.uv);

				Light mainLight = GetMainlight(IN.shadowCoord);
				float3 lightDir = normalize(mainLight.direction);
				float3 normalWS = normalize(IN.normalWS);
				float NdotL = saturate(dot(normalWS, lightDir));

				float lightIntensity = Lum(mainLight.color) * NdotL * mainLight.shadowAttenuation;

				float timeOffset = float2(Time.y * GrainSpeed, Time.y * GrainSpeed);
				float2 dynamicUV = IN.uv * GrainScale + timeOffset;

				float grain = rand(floor(dynamicUV)) * GrainIntensity * lightIntensity;
				half3 grainColour = lerp(baseColour.rgb, mainLight.color.rgb, grain);

				half4 finalColour = half4(grainColour, baseColour.a);

				return finalColour;
			}
			} 
		}
}