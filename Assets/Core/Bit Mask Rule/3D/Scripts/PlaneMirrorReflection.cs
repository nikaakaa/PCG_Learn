using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//[ExecuteInEditMode]
public class PlaneMirrorReflection : MonoBehaviour
{
    public int textureSize = 512;
    public float clipPlaneOffset = 0.07f;
    public LayerMask reflectLayers;

    private Renderer m_Renderer;

    private RenderTexture m_ReflectionTexture;
    private int m_OldReflectionTextureSize;
    private Camera m_ReflectionCamera;
    private bool m_RenderingReflection;

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        if (m_ReflectionTexture != null)
        {
            m_ReflectionTexture.Release();
            DestroyImmediate(m_ReflectionTexture);
            m_ReflectionTexture = null;
        }
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera currentCamera)
    {
        //是否挂载渲染组件
        if (m_Renderer == null)
            m_Renderer = GetComponent<Renderer>();
        if (!enabled || !m_Renderer || !m_Renderer.sharedMaterial || !m_Renderer.enabled)
            return;

        if (currentCamera == null || m_RenderingReflection)
            return;
        if (m_ReflectionCamera && currentCamera == m_ReflectionCamera)
            return;

        //初始化反射贴图
        if (!m_ReflectionTexture || m_OldReflectionTextureSize != textureSize)
        {
            if (m_ReflectionTexture)
            {
                m_ReflectionTexture.Release();
                DestroyImmediate(m_ReflectionTexture);
            }
            m_ReflectionTexture = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32);
            m_ReflectionTexture.name = "_Reflection" + GetInstanceID();
            m_ReflectionTexture.isPowerOfTwo = true;
            m_ReflectionTexture.hideFlags = HideFlags.DontSave;
            m_ReflectionTexture.Create();
            m_OldReflectionTextureSize = textureSize;
        }

        //初始化反射相机
        if (!m_ReflectionCamera)
        {
            GameObject go = new GameObject("reflection camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
            m_ReflectionCamera = go.GetComponent<Camera>();
            m_ReflectionCamera.enabled = false;
            m_ReflectionCamera.gameObject.AddComponent<FlareLayer>();
            var cameraData = m_ReflectionCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = false;
            cameraData.requiresColorTexture = false;
            cameraData.requiresDepthTexture = false;
            go.hideFlags = HideFlags.HideAndDontSave;
        }
        m_ReflectionCamera.CopyFrom(currentCamera);

        //求出反射平面
        Vector3 pos = transform.position;
        Vector3 normal = transform.up;
        float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        //求出反射矩阵并应用到反射像机
        Matrix4x4 reflectionMatrix = CalculateReflectionMatrix(reflectionPlane);
        m_ReflectionCamera.worldToCameraMatrix = currentCamera.worldToCameraMatrix * reflectionMatrix;

        //用反射平面代替反射相机的近裁切面
        Vector4 clipPlane = m_ReflectionCamera.worldToCameraMatrix.inverse.transpose * reflectionPlane;
        m_ReflectionCamera.projectionMatrix = GetObliqueMatrix(m_ReflectionCamera, clipPlane);

        //渲染
        m_ReflectionCamera.cullingMask = reflectLayers;
        m_ReflectionCamera.targetTexture = m_ReflectionTexture;
        m_ReflectionCamera.forceIntoRenderTexture = true;
        m_RenderingReflection = true;
        GL.invertCulling = true;
        UniversalRenderPipeline.RenderSingleCamera(context, m_ReflectionCamera);
        GL.invertCulling = false;
        m_RenderingReflection = false;
        m_Renderer.sharedMaterial.SetTexture("_ReflectionTex", m_ReflectionTexture);
    }

    // 计算反射矩阵
    private Matrix4x4 CalculateReflectionMatrix(Vector4 plane)
    {
        Matrix4x4 m = default(Matrix4x4);
        m.m00 = -2 * plane.x * plane.x + 1;
        m.m01 = -2 * plane.x * plane.y;
        m.m02 = -2 * plane.x * plane.z;
        m.m03 = -2 * plane.x * plane.w;

        m.m10 = -2 * plane.x * plane.y;
        m.m11 = -2 * plane.y * plane.y + 1;
        m.m12 = -2 * plane.y * plane.z;
        m.m13 = -2 * plane.y * plane.w;

        m.m20 = -2 * plane.z * plane.x;
        m.m21 = -2 * plane.z * plane.y;
        m.m22 = -2 * plane.z * plane.z + 1;
        m.m23 = -2 * plane.z * plane.w;

        m.m30 = 0; m.m31 = 0;
        m.m32 = 0; m.m33 = 1;
        return m;
    }

    // 计算斜截矩阵
    private Matrix4x4 GetObliqueMatrix(Camera camera, Vector4 viewSpaceClipPlane)
    {
        // Custom
        var M = camera.projectionMatrix;
        var m4 = new Vector4(M.m30, M.m31, M.m32, M.m33);
        var viewC = viewSpaceClipPlane;
        var clipC = M.inverse.transpose * viewC;

        var clipQ = new Vector4(Mathf.Sign(clipC.x), Mathf.Sign(clipC.y), 1, 1);
        var viewQ = M.inverse * clipQ;

        var a = 2 * Vector4.Dot(m4, viewQ) / Vector4.Dot(viewC, viewQ);
        var aC = a * viewC;
        var newM3 = aC - m4;

        M.m20 = newM3.x;
        M.m21 = newM3.y;
        M.m22 = newM3.z;
        M.m23 = newM3.w;

        return M;

        // Unity API
        //return camera.CalculateObliqueMatrix(viewSpaceClipPlane);
    }
}
