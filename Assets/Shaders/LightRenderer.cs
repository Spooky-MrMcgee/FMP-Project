using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullscreenGrainRenderFeature : ScriptableRendererFeature
{
    class FullscreenGrainRenderPass : ScriptableRenderPass
    {
        private Material _material;
        private RTHandle _source;
        private RTHandle _tempTexture;

        public FullscreenGrainRenderPass(Material material)
        {
            _material = material;
            _tempTexture = RTHandles.Alloc("_TempTexture", name: "Temporary Grain Texture");
        }

        public void Setup(RTHandle source)
        {
            _source = source;
            Debug.Log("FullscreenGrainRenderPass: Setup called. Source assigned.");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_material == null || _source == null)
            {
                Debug.LogError("FullscreenGrainRenderPass: Material or Source is null. Skipping execution.");
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get("Fullscreen Grain Pass");

            // Ensure the temp texture is allocated
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            RenderingUtils.ReAllocateIfNeeded(ref _tempTexture, opaqueDesc, name: "Temporary Grain Texture");

            Debug.Log($"FullscreenGrainRenderPass: Blitting from {_source.name} to {_tempTexture.name}");

            // First Blit: Copy the screen color to tempTexture
            Blitter.BlitCameraTexture(cmd, _source, _tempTexture);

            // Second Blit: Apply the shader effect
            Blitter.BlitCameraTexture(cmd, _tempTexture, _source, _material, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (cmd != null)
            {
                Debug.Log("FullscreenGrainRenderPass: Cleaning up temp texture.");
                RTHandles.Release(_tempTexture);
            }
        }
    }

    [System.Serializable]
    public class FullscreenGrainSettings
    {
        public Material material = null;
    }

    public FullscreenGrainSettings settings = new FullscreenGrainSettings();

    private FullscreenGrainRenderPass _fullscreenGrainPass;

    public override void Create()
    {
        _fullscreenGrainPass = new FullscreenGrainRenderPass(settings.material)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents
        };
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        var cameraColorTarget = renderer.cameraColorTargetHandle;
        if (cameraColorTarget == null)
        {
            Debug.LogWarning("Camera color target is null.");
            return;
        }


        Debug.Log("FullscreenGrainRenderFeature: Adding render pass.");
        _fullscreenGrainPass.Setup(cameraColorTarget);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null)
        {
            Debug.LogWarning("Fullscreen Grain Material is missing.");
            return;
        }
        Debug.Log("Enqueing pass.");
        renderer.EnqueuePass(_fullscreenGrainPass);
    }
}