using System;
using CardRhythm.Procedure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace CardRhythm.EditorFlow
{
    /// <summary>
    /// 把流程写进 Boot 场景：ProcedureComponent 的可用流程列表 + 入口流程，并挂上调试覆盖层。
    /// 菜单：Tools/卡牌音游/流程/配置 Boot 场景
    /// 批处理：-executeMethod CardRhythm.EditorFlow.FlowSetup.RebuildAll
    /// </summary>
    public static class FlowSetup
    {
        private const string BootScenePath = "Assets/Scenes/Boot.unity";

        [MenuItem("Tools/卡牌音游/流程/配置 Boot 场景", false, 120)]
        public static void ConfigureMenu()
        {
            Configure();
            EditorUtility.DisplayDialog("流程", "Boot 场景已配置：5 个流程 + 入口 ProcedureLaunch。", "好");
        }

        public static void RebuildAll()
        {
            try
            {
                Configure();
                AssetDatabase.SaveAssets();
                Debug.Log("[FLOW-SETUP] PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError("[FLOW-SETUP] FAIL " + exception);
                EditorApplication.Exit(1);
            }
        }

        private static void Configure()
        {
            var scene = EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single);
            var procedureComponent = UnityEngine.Object.FindFirstObjectByType<ProcedureComponent>();
            if (procedureComponent == null)
            {
                throw new InvalidOperationException("Boot 场景里找不到 ProcedureComponent");
            }

            var serialized = new SerializedObject(procedureComponent);
            var names = serialized.FindProperty("m_AvailableProcedureTypeNames");
            if (names == null)
            {
                throw new InvalidOperationException("找不到 m_AvailableProcedureTypeNames");
            }

            names.arraySize = ProcedureCatalog.AvailableTypeNames.Length;
            for (int i = 0; i < ProcedureCatalog.AvailableTypeNames.Length; i++)
            {
                names.GetArrayElementAtIndex(i).stringValue = ProcedureCatalog.AvailableTypeNames[i];
            }

            serialized.FindProperty("m_EntranceProcedureTypeName").stringValue = ProcedureCatalog.EntranceTypeName;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(procedureComponent);

            if (UnityEngine.Object.FindFirstObjectByType<FlowOverlay>() == null)
            {
                var overlay = new GameObject("FlowOverlay");
                overlay.AddComponent<FlowOverlay>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[FLOW-SETUP] configured " + BootScenePath + " entrance=" + ProcedureCatalog.EntranceTypeName);
        }
    }
}
