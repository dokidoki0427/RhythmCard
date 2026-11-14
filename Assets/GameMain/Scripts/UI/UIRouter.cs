using System.Collections.Generic;
using UnityGameFramework.Runtime;
using UnityEngine;

namespace CardRhythm.UI
{
    /// <summary>
    /// 界面路由。业务代码只认 UIFormId，不关心资源路径和层级。
    /// 维护一个“打开顺序栈”，供返回键 / Esc 使用。
    /// 注意：GF 打开界面是异步的，Open 返回的是 serialId，界面实例可能下一帧才可用。
    /// </summary>
    public static class UIRouter
    {
        private static readonly List<int> s_OpenSerialIds = new List<int>();
        private static readonly List<int> s_ModalSerialIds = new List<int>();

        public static int OpenCount
        {
            get { return s_OpenSerialIds.Count; }
        }

        /// <summary>是否有模态弹窗（设置/牌组）打开。有的话流程不应响应 Space。</summary>
        public static bool HasModal
        {
            get { return s_ModalSerialIds.Count > 0; }
        }

        public static int Open(UIFormId id, object userData = null, bool pauseCovered = true, bool modal = false)
        {
            var uiComponent = GameEntry.GetComponent<UIComponent>();
            if (uiComponent == null)
            {
                Debug.LogError("[UI] UIComponent 不存在，检查 GameFramework 对象是否在场景里。");
                return 0;
            }

            UIFormDef def = UIFormCatalog.Get(id);
            int serialId = uiComponent.OpenUIForm(def.AssetPath, UILayers.NameOf(def.Layer), 0, pauseCovered, userData);
            if (serialId > 0)
            {
                s_OpenSerialIds.Add(serialId);
                if (modal)
                {
                    s_ModalSerialIds.Add(serialId);
                }
            }

            return serialId;
        }

        public static void Close(UIFormId id)
        {
            var uiComponent = GameEntry.GetComponent<UIComponent>();
            if (uiComponent == null)
            {
                return;
            }

            UIFormDef def = UIFormCatalog.Get(id);
            var form = uiComponent.GetUIForm(def.AssetPath);
            if (form == null)
            {
                return;
            }

            s_OpenSerialIds.Remove(form.SerialId);
            s_ModalSerialIds.Remove(form.SerialId);
            uiComponent.CloseUIForm(form);
        }

        /// <summary>关闭最近打开的一个界面（返回键）。</summary>
        public static bool Back()
        {
            var uiComponent = GameEntry.GetComponent<UIComponent>();
            if (uiComponent == null || s_OpenSerialIds.Count == 0)
            {
                return false;
            }

            int index = s_OpenSerialIds.Count - 1;
            int serialId = s_OpenSerialIds[index];
            s_OpenSerialIds.RemoveAt(index);
            s_ModalSerialIds.Remove(serialId);
            uiComponent.CloseUIForm(serialId);
            return true;
        }

        /// <summary>关闭所有由路由打开的界面（切流程时用）。</summary>
        public static void CloseAll()
        {
            var uiComponent = GameEntry.GetComponent<UIComponent>();
            if (uiComponent == null)
            {
                return;
            }

            for (int i = s_OpenSerialIds.Count - 1; i >= 0; i--)
            {
                uiComponent.CloseUIForm(s_OpenSerialIds[i]);
            }

            s_OpenSerialIds.Clear();
            s_ModalSerialIds.Clear();
        }
    }
}
