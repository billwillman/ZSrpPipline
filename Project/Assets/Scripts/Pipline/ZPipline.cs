using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ZPiplineAsset: RenderPipelineAsset
{
    protected override IRenderPipeline InternalCreatePipeline() {
        var ret = new ZPipline();
        return ret;
    }
#if UNITY_EDITOR
    [MenuItem("Assets/创建ZPipline")]
    public static void OnCreatePipline() {
        var asset = ScriptableObject.CreateInstance<ZPiplineAsset>();
        AssetDatabase.CreateAsset(asset, "Assets/Resources/Pipline/ZPipline.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
#endif
}

public class ZPipline: RenderPipeline {

    // 每个相机渲染
    private void CameraRender(ScriptableRenderContext ctx, Camera cam) {
        // 关联相机
        /*
         * SetupCameraProperties函数的注释是这样写的：Setup camera specific global shader variables. 
         * 也就是给摄像机设置好全局着色器变量，这里的全局着色器变量，
         * 应当就是Unity Shader中常常要用到的诸如WorldSpaceLightPos0之类的。
         */
        ctx.SetupCameraProperties(cam);
        BuildClearCmdBuf(cam);
        ctx.ExecuteCommandBuffer(m_ClearCmdBuf);
        // 预计算CullResults
        PrepareCulls(ctx, cam);
        // 绘制不透明物体
        DrawOpObjects(ctx, cam);
        // 最后绘制绘制天空盒（因为天空盒可以适用ZTEST，性能最高放最后）
        ctx.DrawSkybox(cam);
        ctx.Submit();
    }

    // 绘制不透明物体
    private void DrawOpObjects(ScriptableRenderContext ctx, Camera cam) {
        //var drawSettings = new DrawRendererSettings()
        //ctx.DrawRenderers(m_CamCullResults.visibleRenderers, )
    }

    private void PrepareCulls(ScriptableRenderContext ctx, Camera cam) {
        ScriptableCullingParameters cullParams;
        // 从摄像机获得当前相机的建材参数
        if (CullResults.GetCullingParameters(cam, out cullParams)) {
            CullResults.Cull(ref cullParams, ctx, ref m_CamCullResults);
        }
    }

    private void BuildClearCmdBuf(Camera cam) {
        m_ClearCmdBuf.Clear();
        var flag = cam.clearFlags;
        m_ClearCmdBuf.ClearRenderTarget((flag & CameraClearFlags.Depth) != 0, (flag & CameraClearFlags.Color) != 0, cam.backgroundColor);
    }

    // 相机清理Cmmand
    private CommandBuffer m_ClearCmdBuf = new CommandBuffer();
    private CullResults m_CamCullResults = new CullResults();

    public override void Dispose() {

        if (m_ClearCmdBuf != null) {
            m_ClearCmdBuf.Release();
            //  m_CmdBuf.Dispose();
            m_ClearCmdBuf = null;
        }
        base.Dispose();
    }

    // 相机渲染
    public override void Render(ScriptableRenderContext renderContext, Camera[] cameras) {
        if (cameras != null) {
            for (int i = 0; i < cameras.Length; ++i) {
                CameraRender(renderContext, cameras[i]);
            }
        }
    }
}
