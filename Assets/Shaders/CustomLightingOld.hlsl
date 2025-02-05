#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

#ifndef UNIVERSAL_LIGHTING_DATA
#define UNIVERSAL_LIGHTING_DATA

struct CustomLightingData
{
	// Position and orientation
	float3 normalWorldSpace;
	// Surface attributes
	float3 albedo;
};

#ifndef SHADERGRAPH_PREVIEW
float3 CustomLightHandling(CustomLightingData lightingData, Light light)
{
	float3 radiance = light.color;

	float diffuse = saturate(dot(lightingData.normalWorldSpace, light.direction));

	float3 color = lightingData.albedo * radiance * diffuse;

	return color;

}
#endif

float3 CalculateCustomLighting(CustomLightingData lightingData){
#ifdef SHADERGRAPH_PREVIEW
	float3 lightDir = float3(0.5, 0.5, 0);
	float intensity = saturate(dot(lightingData.normalWorldSpace, lightDir));
	return lightingData.albedo * intensity;
#else
	// mainLight grabbed from URP/ShaderLibrary/Lighting.hlsl
	Light mainlight = GetMainLight();
	
	float3 color = 0;
	color += CustomLightHandling(lightingData, mainLight);

	return color;
#endif
}

void CalculateCustomLighting_float(float3 Normal, float3 Albedo, out float3 Color)
{
	CustomLightingData lightingData;
	lightingData.albedo = Albedo;
	lightingData.normalWorldSpace = Normal;
	Color = CalculateCustomLighting(lightingData);
}
#endif
#endif
