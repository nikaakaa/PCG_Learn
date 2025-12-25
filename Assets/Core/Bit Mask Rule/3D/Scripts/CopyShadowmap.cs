using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class CopyShadowmap : MonoBehaviour
{
    [ReadOnly]public RenderTexture m_ShadowmapCopy;

    Light lightC;

    CommandBuffer cb;

    void OnEnable()
    {
        RenderTargetIdentifier shadowmap = BuiltinRenderTextureType.CurrentActive;
        m_ShadowmapCopy = new RenderTexture(1024, 1024, 0);
        if (cb == null)
        {
            cb = new CommandBuffer();
        }
        else
        {
            cb.Clear();
        }

        if (lightC == null) { lightC = GetComponent<Light>(); }
        lightC.RemoveAllCommandBuffers();

        // Change shadow sampling mode for m_Light's shadowmap.
        cb.SetShadowSamplingMode(shadowmap, ShadowSamplingMode.RawDepth);

        // The shadowmap values can now be sampled normally - copy it to a different render texture.
        cb.Blit(shadowmap, new RenderTargetIdentifier(m_ShadowmapCopy));

        // Execute after the shadowmap has been filled.
        lightC.AddCommandBuffer(LightEvent.AfterShadowMap, cb);

        // Sampling mode is restored automatically after this command buffer completes, so shadows will render normally.
        Shader.SetGlobalTexture("_MainDirectionalShadowMap", m_ShadowmapCopy);

    }
}

