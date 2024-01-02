using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullScreenBlit : ScriptableRendererFeature
{
    [SerializeField] private Material _Material;
    private BlitPass _Pass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_Pass);
    }

    public override void Create()
    {
        _Pass = new BlitPass(_Material);
        _Pass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public class BlitPass : ScriptableRenderPass
    {
        private Material _Material;
        private RenderTargetIdentifier _CamColorRTID;
        private RTHandle _TempTexture;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
            _CamColorRTID = renderingData.cameraData.renderer.cameraColorTarget;
            _TempTexture = RTHandles.Alloc(new RenderTargetIdentifier("_BlitTempTex"), "_BlitTempTex");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Full Screen Render Feature");
            RenderTextureDescriptor targetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            targetDescriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(Shader.PropertyToID(_TempTexture.name), targetDescriptor, FilterMode.Point);

            Blit(cmd, _CamColorRTID, _TempTexture, _Material);
            Blit(cmd, _TempTexture, _CamColorRTID);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public BlitPass(Material material)
        {
            _Material = material;
        }
    }
}
