using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEngine;

namespace EditProto
{
    /// <summary>
    /// Proto�༭������ - ���չ������ 
    /// </summary>
    public partial class ProtoMenuEditorWindow : OdinMenuEditorWindow
    {
        /// <summary>
        /// �������Ĵ�������
        /// </summary>
        private OdinEditorWindow _SnapShootPanelWindow;

        /// <summary>
        /// ������Ϣ
        /// </summary>
        public SnapShootPanelDisplay CurSnapShootPanelDisplay;


        /// <summary>
        /// �������de��ʾ����
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