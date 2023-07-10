using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEngine;

namespace EditProto
{
    /// <summary>
    /// Proto编辑器窗口 - 快照管理面板 
    /// </summary>
    public partial class ProtoMenuEditorWindow : OdinMenuEditorWindow
    {
        /// <summary>
        /// 快照面板的窗口引用
        /// </summary>
        private OdinEditorWindow _SnapShootPanelWindow;

        /// <summary>
        /// 快照信息
        /// </summary>
        public SnapShootPanelDisplay CurSnapShootPanelDisplay;


        /// <summary>
        /// 快照面板de显示内容
        /// </summary>
        public class SnapShootPanelDisplay : ProtoHelper.BasePanelDisplay
        {
            // [AssetList(CustomFilterMethod = "IsTargetConfig", AutoPopulate = true)]
            [AssetList(AutoPopulate = true)]
            public List<SnapShootScriptableObject> AssetList = new List<SnapShootScriptableObject>();

            //private bool IsTargetConfig(ScriptableObject obj)
            //{
            //    return obj is SnapShootScriptableObject;
            //}
        }
    }
}