using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class LightingRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class GrainSettings
    {
        public Material grainMaterial = null;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public GrainSettings settings = new GrainSettings();

    private LightAwareGrainPass _grainPass;

    public override void Create()
    {
        _grainPass = new LightAwareGrainPass(settings.grainMaterial)
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.grainMaterial == null)
        {
            Debug.LogWarning("Grain material is missing.");
            return;
        }

        // Get the camera color target as an RTHandle
        renderer.EnqueuePass(_grainPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        var cameraColorTarget = renderer.cameraColorTargetHandle;
        if (cameraColorTarget == null)
        {
            Debug.LogWarning("Camera color target is null.");
            return;
        }

        _grainPass.Setup(cameraColorTarget);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _grainPass?.Dispose();
        }
    }


}
