using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CardRhythm.Cards;
using CardRhythm.UI;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace CardRhythm.EditorAutomation
{
    /// <summary>
    /// 无人值守（batchmode）入口 + 编辑器内菜单入口。
    /// 批处理调用：
    ///   CardRhythm.EditorAutomation.ProjectAutomation.Validate
    ///   CardRhythm.EditorAutomation.ProjectAutomation.CreateBootScene
    ///   CardRhythm.EditorAutomation.ProjectAutomation.BuildWindows
    /// 编辑器内：Tools/卡牌音游/* 菜单，不关编辑器也能跑同样的检查。
    /// 约定：结果写入 Logs/automation-report.json，用退出码表达成败（0 成功 / 1 失败）。
    /// 日志刻意只用 ASCII：不同终端/日志查看器的编码不一致，中文会变乱码干扰 agent 判断。
    /// </summary>
    public static class ProjectAutomation
    {
        private const string ReportPath = "Logs/automation-report.json";
        private const string GameFrameworkPrefabPath = "Packages/com.jiangyin.gameframework/GameFramework.prefab";
        private const string BootScenePath = "Assets/Scenes/Boot.unity";
        private const int ExpectedComponentCount = 20;

        private sealed class Check
        {
            public string Name;
            public string Status; // PASS / WARN / FAIL
            public string Detail;
        }

        public static void Validate()
        {
            var checks = RunChecks();
            WriteReport(checks);
            EditorApplication.Exit(HasFailure(checks) ? 1 : 0);
        }

        [MenuItem("Tools/卡牌音游/验证工程（不关编辑器）")]
        public static void ValidateInEditor()
        {
            var checks = RunChecks();
            WriteReport(checks);

            var summary = new StringBuilder();
            foreach (var check in checks)
            {
                summary.AppendLine(check.Status + "   " + check.Name + "   " + check.Detail);
            }

            bool ok = !HasFailure(checks);
            Debug.Log((ok ? "[AUTOMATION] PASS " : "[AUTOMATION] FAIL ") + "editor-validate\n" + summary);
            EditorUtility.DisplayDialog(
                ok ? "验证工程：通过" : "验证工程：有失败项",
                summary + "\n报告：Logs/automation-report.json",
                "好");
        }

        public static void CreateBootScene()
        {
            EditorApplication.Exit(EnsureBootScene() ? 0 : 1);
        }

        [MenuItem("Tools/卡牌音游/创建 Boot 场景")]
        public static void CreateBootSceneInEditor()
        {
            bool ok = EnsureBootScene();
            EditorUtility.DisplayDialog(
                "Boot 场景",
                ok ? "已创建 Assets/Scenes/Boot.unity 并加入 Build Settings。" : "创建失败，见 Console。",
                "好");
        }

        [MenuItem("Tools/卡牌音游/打开 Test Runner")]
        public static void OpenTestRunner()
        {
            EditorApplication.ExecuteMenuItem("Window/General/Test Runner");
        }

        public static void BuildWindows()
        {
            var scenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled && File.Exists(scene.path))
                {
                    scenes.Add(scene.path);
                }
            }

            if (scenes.Count == 0)
            {
                WriteBuildReport(false, "no enabled scenes in Build Settings");
                Log(LogType.Error, "FAIL build no enabled scenes in Build Settings");
                EditorApplication.Exit(1);
                return;
            }

            const string output = "Builds/Windows/CardRhythm.exe";
            Directory.CreateDirectory(Path.GetDirectoryName(output));

            var options = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            bool succeeded = summary.result == BuildResult.Succeeded;
            string detail = string.Format("result={0} sizeMB={1:F1} errors={2} warnings={3} output={4}",
                summary.result, summary.totalSize / (1024f * 1024f),
                summary.totalErrors, summary.totalWarnings, output);

            Log(succeeded ? LogType.Log : LogType.Error, (succeeded ? "PASS build " : "FAIL build ") + detail);
            WriteBuildReport(succeeded, detail);
            EditorApplication.Exit(succeeded ? 0 : 1);
        }

        private static List<Check> RunChecks()
        {
            var checks = new List<Check>();
            try
            {
                CheckGameFrameworkAssembly(checks);
                CheckGameFrameworkPrefab(checks);
                CheckMissingScripts(checks);
                CheckBuildScenes(checks);
                CheckUIForms(checks);
                CheckCards(checks);
            }
            catch (Exception exception)
            {
                checks.Add(new Check { Name = "unhandled-exception", Status = "FAIL", Detail = exception.ToString() });
                Log(LogType.Error, "FAIL unhandled-exception " + exception);
            }

            return checks;
        }

        private static bool EnsureBootScene()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameFrameworkPrefabPath);
            if (prefab == null)
            {
                Log(LogType.Error, "FAIL boot-scene missing " + GameFrameworkPrefabPath);
                return false;
            }

            Directory.CreateDirectory("Assets/Scenes");
            if (!File.Exists(BootScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = "GameFramework";
                EditorSceneManager.SaveScene(scene, BootScenePath);
            }

            var buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!buildScenes.Exists(s => s.path == BootScenePath))
            {
                buildScenes.Insert(0, new EditorBuildSettingsScene(BootScenePath, true));
                EditorBuildSettings.scenes = buildScenes.ToArray();
            }

            AssetDatabase.SaveAssets();
            Log(LogType.Log, "PASS boot-scene " + BootScenePath);
            return true;
        }

        private static void CheckGameFrameworkAssembly(List<Check> checks)
        {
            var assembly = typeof(GameEntry).Assembly.GetName();
            AddCheck(checks, "gameframework-runtime", "PASS", assembly.Name + " " + assembly.Version);
        }

        private static void CheckUIForms(List<Check> checks)
        {
            var missing = new List<string>();
            foreach (UIFormDef def in UIFormCatalog.All)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(def.AssetPath);
                if (prefab == null || prefab.GetComponent<UIFormBase>() == null)
                {
                    missing.Add(def.Id + "(" + def.AssetPath + ")");
                }
            }

            if (missing.Count == 0)
            {
                AddCheck(checks, "ui-forms", "PASS", "count=" + UIFormCatalog.All.Count);
            }
            else
            {
                AddCheck(checks, "ui-forms", "FAIL", string.Join("; ", missing) + " | run Tools/CardRhythm/UI/Rebuild");
            }
        }

        private static void CheckCards(List<Check> checks)
        {
            var cards = new List<CardDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets" }))
            {
                var card = AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (card != null)
                {
                    cards.Add(card);
                }
            }

            var errors = new List<string>();
            foreach (CardDefinition card in cards)
            {
                foreach (CardIssue issue in CardValidator.Validate(card, cards))
                {
                    if (issue.Level == CardIssueLevel.Error)
                    {
                        errors.Add(card.name + ": " + issue.Message);
                    }
                }
            }

            if (errors.Count == 0)
            {
                AddCheck(checks, "cards", "PASS", "count=" + cards.Count);
            }
            else
            {
                AddCheck(checks, "cards", "FAIL", string.Join(" | ", errors));
            }
        }

        private static void CheckGameFrameworkPrefab(List<Check> checks)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameFrameworkPrefabPath);
            if (prefab == null)
            {
                AddCheck(checks, "gameframework-prefab", "FAIL", "missing " + GameFrameworkPrefabPath);
                return;
            }

            int componentCount = prefab.GetComponentsInChildren<Component>(true).Length;
            AddCheck(checks, "gameframework-prefab", componentCount >= ExpectedComponentCount ? "PASS" : "FAIL",
                "components=" + componentCount + " expected>=" + ExpectedComponentCount);
        }

        private static void CheckMissingScripts(List<Check> checks)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            var broken = new List<string>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null)
                {
                    continue;
                }

                foreach (Component component in asset.GetComponentsInChildren<Component>(true))
                {
                    if (component == null)
                    {
                        broken.Add(path);
                        break;
                    }
                }
            }

            AddCheck(checks, "missing-scripts", broken.Count == 0 ? "PASS" : "FAIL",
                broken.Count == 0 ? "prefabs=" + guids.Length : string.Join("; ", broken));
        }

        private static void CheckBuildScenes(List<Check> checks)
        {
            int enabled = 0;
            var missing = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                {
                    continue;
                }

                enabled++;
                if (!File.Exists(scene.path))
                {
                    missing.Add(scene.path);
                }
            }

            if (missing.Count > 0)
            {
                AddCheck(checks, "build-scenes", "FAIL", "missing scenes: " + string.Join("; ", missing));
            }
            else if (enabled == 0)
            {
                AddCheck(checks, "build-scenes", "WARN", "no scenes in Build Settings, build will fail");
            }
            else
            {
                AddCheck(checks, "build-scenes", "PASS", "enabled=" + enabled);
            }
        }

        private static void AddCheck(List<Check> checks, string name, string status, string detail)
        {
            checks.Add(new Check { Name = name, Status = status, Detail = detail });

            LogType type = LogType.Log;
            if (status == "FAIL")
            {
                type = LogType.Error;
            }
            else if (status == "WARN")
            {
                type = LogType.Warning;
            }

            Log(type, status + " " + name + " " + detail);
        }

        private static void Log(LogType type, string message)
        {
            Debug.LogFormat(type, LogOption.NoStacktrace, null, "[AUTOMATION] {0}", message);
        }

        private static bool HasFailure(List<Check> checks)
        {
            foreach (var check in checks)
            {
                if (check.Status == "FAIL")
                {
                    return true;
                }
            }

            return false;
        }

        private static void WriteBuildReport(bool succeeded, string detail)
        {
            WriteJson("{\n  \"result\": \"" + (succeeded ? "PASS" : "FAIL") + "\",\n  \"build\": \"" + Escape(detail) + "\"\n}\n");
        }

        private static void WriteReport(List<Check> checks)
        {
            var builder = new StringBuilder();
            builder.Append("{\n  \"result\": \"").Append(HasFailure(checks) ? "FAIL" : "PASS").Append("\",\n");
            builder.Append("  \"unity\": \"").Append(Application.unityVersion).Append("\",\n");
            builder.Append("  \"checks\": [\n");
            for (int i = 0; i < checks.Count; i++)
            {
                Check check = checks[i];
                builder.Append("    { \"name\": \"").Append(Escape(check.Name))
                    .Append("\", \"status\": \"").Append(check.Status)
                    .Append("\", \"detail\": \"").Append(Escape(check.Detail)).Append("\" }");
                builder.Append(i == checks.Count - 1 ? "\n" : ",\n");
            }

            builder.Append("  ]\n}\n");
            WriteJson(builder.ToString());
        }

        private static void WriteJson(string content)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
                File.WriteAllText(ReportPath, content, new UTF8Encoding(false));
            }
            catch (Exception exception)
            {
                Log(LogType.Warning, "cannot write report: " + exception.Message);
            }
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", string.Empty);
        }
    }
}
