void GetLight_float(float3 WorldPos, out float3 Direction, out float3 Colour, out float ShadowAtten){
#if defined(SHADERGRAPH_PREVIEW)
    Direction = half3(0.5, 0.5, 0);
    Colour = 1;
    ShadowAtten = 1;
#else
    
#endif
}