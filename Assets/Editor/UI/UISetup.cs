using System;
using System.IO;
using CardRhythm.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace CardRhythm.EditorUI
{
    /// <summary>
    /// UI 层级框架的编辑器端：生成占位界面预制体、把界面组写进场景里的 GameFramework 对象、生成预览场景。
    /// 菜单：Tools/卡牌音游/UI/*
    /// 批处理：-executeMethod CardRhythm.EditorUI.UISetup.RebuildAll
    /// </summary>
    public static class UISetup
    {
        private const string GameFrameworkPrefabPath = "Packages/com.jiangyin.gameframework/GameFramework.prefab";
        private const string UIFormFolder = "Assets/GameMain/UIForms";
        private const string BootScenePath = "Assets/Scenes/Boot.unity";
        private const string PreviewScenePath = "Assets/Scenes/UIPreview.unity";

        [MenuItem("Tools/卡牌音游/UI/重建占位界面")]
        public static void RebuildPlaceholdersMenu()
        {
            EnsureFolder(UIFormFolder);
            RebuildPlaceholders();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UI-SETUP] placeholder forms rebuilt.");
        }

        [MenuItem("Tools/卡牌音游/UI/配置当前场景的界面组")]
        public static void ConfigureCurrentSceneMenu()
        {
            var uiComponent = UnityEngine.Object.FindFirstObjectByType<UIComponent>();
            if (uiComponent == null)
            {
                EditorUtility.DisplayDialog("配置界面组", "当前场景里找不到 UIComponent。", "好");
                return;
            }

            ConfigureUIComponent(uiComponent);
            var scene = uiComponent.gameObject.scene;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[UI-SETUP] UI groups configured in scene: " + scene.path);
        }

        [MenuItem("Tools/卡牌音游/UI/生成 UI 预览场景")]
        public static void BuildPreviewSceneMenu()
        {
            BuildPreviewScene();
            EditorUtility.DisplayDialog("UI 预览", "已生成 Assets/Scenes/UIPreview.unity，直接按 Play。", "好");
        }

        [MenuItem("Tools/卡牌音游/UI/打开 UI 预览场景")]
        public static void OpenPreviewSceneMenu()
        {
            if (!File.Exists(PreviewScenePath))
            {
                BuildPreviewScene();
            }

            EditorSceneManager.OpenScene(PreviewScenePath, OpenSceneMode.Single);
        }

        /// <summary>批处理入口：重建占位界面 + 配置 Boot + 生成预览场景。</summary>
        public static void RebuildAll()
        {
            try
            {
                EnsureFolder(UIFormFolder);
                RebuildPlaceholders();

                ConfigureScene(BootScenePath);
                BuildPreviewScene();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[UI-SETUP] PASS rebuild-all");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError("[UI-SETUP] FAIL rebuild-all " + exception);
                EditorApplication.Exit(1);
            }
        }

        private static void RebuildPlaceholders()
        {
            foreach (UIFormDef def in UIFormCatalog.All)
            {
                string path = def.AssetPath;
                var root = CreatePlaceholder(def, out Color color);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                UnityEngine.Object.DestroyImmediate(root);
                Debug.Log("[UI-SETUP] created " + path + " layer=" + def.Layer + " color=" + ColorUtility.ToHtmlStringRGBA(color));
            }
        }

        private static GameObject CreatePlaceholder(UIFormDef def, out Color color)
        {
            color = LayerColor(def.Layer);

            var root = new GameObject(def.Id + "Form", typeof(RectTransform), typeof(CanvasGroup));
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = root.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            var title = CreateText(root.transform, "Title", TitleOf(def), 44, TextAnchor.UpperLeft);
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -32f);
            titleRect.sizeDelta = new Vector2(-96f, 72f);

            var hint = CreateText(root.transform, "Hint", "placeholder", 28, TextAnchor.MiddleCenter);
            var hintRect = hint.rectTransform;
            hintRect.anchorMin = new Vector2(0.5f, 0.5f);
            hintRect.anchorMax = new Vector2(0.5f, 0.5f);
            hintRect.sizeDelta = new Vector2(900f, 120f);

            Type formType = FormTypeOf(def.Id);
            if (formType == null)
            {
                throw new InvalidOperationException("No UIFormLogic type for " + def.Id);
            }

            root.AddComponent(formType);
            return root;
        }

        private static Text CreateText(Transform parent, string name, string content, int size, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = anchor;
            text.text = content;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static string TitleOf(UIFormDef def)
        {
            return def.Id.ToString().ToUpperInvariant() + "   |   layer " + def.Layer + " (" + (int)def.Layer + ")";
        }

        private static Color LayerColor(UILayer layer)
        {
            switch (layer)
            {
                case UILayer.Background: return new Color(0.05f, 0.06f, 0.08f, 1f);
                case UILayer.Main: return new Color(0.12f, 0.16f, 0.22f, 1f);
                case UILayer.Hud: return new Color(0f, 0f, 0f, 0.25f);
                case UILayer.Window: return new Color(0.06f, 0.09f, 0.13f, 0.92f);
                case UILayer.Popup: return new Color(0.16f, 0.12f, 0.18f, 0.95f);
                default: return new Color(0f, 0f, 0f, 0.75f);
            }
        }

        private static Type FormTypeOf(UIFormId id)
        {
            switch (id)
            {
                case UIFormId.Menu: return typeof(MenuForm);
                case UIFormId.Map: return typeof(MapForm);
                case UIFormId.BattleHud: return typeof(BattleHudForm);
                case UIFormId.Result: return typeof(ResultForm);
                case UIFormId.Settings: return typeof(SettingsForm);
                case UIFormId.Deck: return typeof(DeckForm);
                default: return null;
            }
        }

        private static void BuildPreviewScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameFrameworkPrefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException("GameFramework.prefab not found: " + GameFrameworkPrefabPath);
            }

            var gameFramework = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            gameFramework.name = "GameFramework";

            var uiComponent = gameFramework.GetComponentInChildren<UIComponent>(true);
            if (uiComponent == null)
            {
                throw new InvalidOperationException("UIComponent not found on GameFramework.prefab");
            }

            ConfigureUIComponent(uiComponent);

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            new GameObject("UIPreview", typeof(UIPreviewRunner));

            EditorSceneManager.SaveScene(scene, PreviewScenePath);
            Debug.Log("[UI-SETUP] preview scene saved: " + PreviewScenePath);
        }

        private static void ConfigureScene(string scenePath)
        {
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning("[UI-SETUP] scene not found, skipped: " + scenePath);
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var uiComponent = UnityEngine.Object.FindFirstObjectByType<UIComponent>();
            if (uiComponent == null)
            {
                Debug.LogWarning("[UI-SETUP] no UIComponent in " + scenePath);
                return;
            }

            ConfigureUIComponent(uiComponent);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[UI-SETUP] configured " + scenePath);
        }

        /// <summary>把界面组与界面组辅助器写进 UIComponent 的序列化字段。</summary>
        private static void ConfigureUIComponent(UIComponent uiComponent)
        {
            var serialized = new SerializedObject(uiComponent);

            var helperType = serialized.FindProperty("m_UIGroupHelperTypeName");
            if (helperType != null)
            {
                helperType.stringValue = typeof(UIGroupCanvasHelper).FullName;
            }

            var groups = serialized.FindProperty("m_UIGroups");
            if (groups == null)
            {
                throw new InvalidOperationException("m_UIGroups not found on UIComponent");
            }

            groups.arraySize = UILayers.All.Count;
            for (int i = 0; i < UILayers.All.Count; i++)
            {
                UILayer layer = UILayers.All[i];
                var element = groups.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("m_Name").stringValue = UILayers.NameOf(layer);
                element.FindPropertyRelative("m_Depth").intValue = UILayers.DepthOf(layer);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(uiComponent);
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            string name = Path.GetFileName(folder);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
