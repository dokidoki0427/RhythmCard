using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace CardRhythm.Tests.EditMode
{
    /// <summary>
    /// 冒烟测试：只验证“框架在、资源完好”，不验证玩法。
    /// 任何改动后先跑这里，保证自动化闭环有最低限度的回归保护。
    /// </summary>
    public class GameFrameworkSmokeTests
    {
        private const string GameFrameworkPrefabPath = "Packages/com.jiangyin.gameframework/GameFramework.prefab";

        [Test]
        public void GameFrameworkAssembly_IsLoaded()
        {
            Assert.IsNotNull(typeof(GameEntry).Assembly, "UnityGameFramework.Runtime 程序集未加载");
        }

        [Test]
        public void GameFrameworkPrefab_ExistsAndHasComponents()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameFrameworkPrefabPath);
            Assert.IsNotNull(prefab, "未找到 GameFramework.prefab，检查内嵌包是否被裁剪");

            var components = prefab.GetComponentsInChildren<Component>(true);
            Assert.GreaterOrEqual(components.Length, 20, "GameFramework.prefab 组件数异常，可能缺脚本");
            foreach (var component in components)
            {
                Assert.IsNotNull(component, "GameFramework.prefab 上存在 Missing Script");
            }
        }

        [Test]
        public void ProjectHasBuildSettings()
        {
            Assert.IsNotNull(EditorBuildSettings.scenes, "Build Settings 读取失败");
        }
    }
}
