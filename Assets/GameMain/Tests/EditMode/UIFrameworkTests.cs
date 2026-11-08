using System;
using System.Collections.Generic;
using CardRhythm.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.Tests.EditMode
{
    /// <summary>
    /// UI 层级框架的回归测试：层级顺序、目录完整性、占位界面资源是否还在。
    /// 这些测试不需要进入 Play 模式，跑得很快。
    /// </summary>
    public class UIFrameworkTests
    {
        [Test]
        public void Layers_AreStrictlyIncreasing()
        {
            int previous = int.MinValue;
            foreach (UILayer layer in UILayers.All)
            {
                int depth = UILayers.DepthOf(layer);
                Assert.Greater(depth, previous, "层级深度必须严格递增：" + layer);
                previous = depth;
            }
        }

        [Test]
        public void Layers_SpanTheExpectedOrder()
        {
            Assert.AreEqual(UILayer.Background, UILayers.All[0], "最底层必须是 Background");
            Assert.AreEqual(UILayer.Tips, UILayers.All[UILayers.All.Count - 1], "最上层必须是 Tips");
        }

        [Test]
        public void Catalog_CoversEveryId()
        {
            foreach (UIFormId id in Enum.GetValues(typeof(UIFormId)))
            {
                UIFormDef def = UIFormCatalog.Get(id);
                Assert.AreEqual(id, def.Id, "目录里的 Id 与键不一致");
                Assert.IsFalse(string.IsNullOrEmpty(def.AssetPath), "缺少资源路径：" + id);
            }
        }

        [Test]
        public void Catalog_PathsAreUnique()
        {
            var seen = new HashSet<string>();
            foreach (UIFormDef def in UIFormCatalog.All)
            {
                Assert.IsTrue(seen.Add(def.AssetPath), "界面资源路径重复：" + def.AssetPath);
            }
        }

        [Test]
        public void Catalog_PrefabsExistAndHaveFormLogic()
        {
            foreach (UIFormDef def in UIFormCatalog.All)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(def.AssetPath);
                Assert.IsNotNull(prefab, "占位界面缺失：" + def.AssetPath + "（跑一次 Tools/卡牌音游/UI/重建占位界面）");
                Assert.IsNotNull(prefab.GetComponent<UIFormBase>(), "界面缺少 UIFormBase 派生脚本：" + def.AssetPath);
            }
        }

        [Test]
        public void GroupCanvasHelper_CreatesOverlayCanvasAtDepth()
        {
            var go = new GameObject("UIGroupTest");
            try
            {
                var helper = go.AddComponent<UIGroupCanvasHelper>();
                helper.SetDepth(300);

                var canvas = go.GetComponent<Canvas>();
                Assert.IsNotNull(canvas, "UIGroupCanvasHelper 没有创建 Canvas");
                Assert.AreEqual(RenderMode.ScreenSpaceOverlay, canvas.renderMode);
                Assert.AreEqual(300, canvas.sortingOrder, "depth 必须映射为 sortingOrder");
                Assert.IsNotNull(go.GetComponent<CanvasScaler>(), "缺少 CanvasScaler");
                Assert.IsNotNull(go.GetComponent<GraphicRaycaster>(), "缺少 GraphicRaycaster");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
