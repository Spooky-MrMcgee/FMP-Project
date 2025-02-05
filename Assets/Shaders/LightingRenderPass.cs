using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LightAwareGrainPass : ScriptableRenderPass
{
    private Material _grainMaterial;
    private Material _debugMaterial; // Material for debug output
    private RTHandle _source;
    private RTHandle _tempTexture;

    public LightAwareGrainPass(Material grainMaterial)
    {
        _grainMaterial = grainMaterial;
        renderPassEvent = RenderPassEvent.AfterRenderingOpaques; // Execute after opaque objects are rendered
        _debugMaterial = CoreUtils.CreateEngineMaterial("Hidden/InternalErrorShader"); // Debug material
    }

    public void Setup(RTHandle source)
    {
        _source = source;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        // Create a temporary render texture
        RenderTextureDescriptor opaqueDesc = cameraTextureDescriptor;
        opaqueDesc.depthBufferBits = 0; // No depth buffer needed
        RenderingUtils.ReAllocateIfNeeded(ref _tempTexture, opaqueDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempGrainTexture");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_grainMaterial == null || _source == null || _tempTexture == null)
        {
            Debug.LogError("Grain material or RTHandle is missing.");
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get("LightAwareGrainPass");

        // Debug: Log the render pass execution
        Debug.Log("Executing LightAwareGrainPass");

        // Bind _MainTex to the shader
        cmd.SetGlobalTexture("_MainTex", _source);

        // Pass the screen-space shadow map to the shader (if available)
        cmd.EnableShaderKeyword("_SCREEN_SPACE_SHADOWS"); // Enable the shadow map keyword
        cmd.SetGlobalTexture("_ScreenSpaceShadowmapTexture", Shader.PropertyToID("_ScreenSpaceShadowmapTexture"));

        // Debug: Draw a solid red color to verify the render pass is working
        _debugMaterial.color = Color.red; // Set the debug material color to red
        Blitter.BlitCameraTexture(cmd, _source, _tempTexture, _debugMaterial, 0); // Draw red
        Blitter.BlitCameraTexture(cmd, _tempTexture, _source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        if (_tempTexture != null)
        {
            _tempTexture.Release();
        }
    }

    public void Dispose()
    {
        _tempTexture?.Release();
        CoreUtils.Destroy(_debugMaterial); // Clean up the debug material
    }

}