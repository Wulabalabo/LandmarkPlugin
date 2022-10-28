using Landmark;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LocalSceneManager : MonoBehaviour
{
    public RenderPipelineAsset[] RenderPipelineAssetList;
    public void SceneLogic(int sceneId)
    {
        SwitchRenderPipeline(sceneId);
        GameManager.instance.IsSceneChangeDone = true;
    }

    void SwitchRenderPipeline(int id)
    {
        GraphicsSettings.defaultRenderPipeline = RenderPipelineAssetList[id];
        QualitySettings.renderPipeline = RenderPipelineAssetList[id];
    }

    public void Init()
    {
        GameManager.instance.OnSceneChangeCompleted += SceneLogic;
    }
}
