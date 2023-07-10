using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;

namespace EditProto
{
    /// <summary>
    /// Proto编辑器窗口 - 关于面板
    /// </summary>
    public partial class ProtoMenuEditorWindow : OdinMenuEditorWindow
    {
        /// <summary>
        /// 关于面板的窗口引用
        /// </summary>
        private OdinEditorWindow _AboutPanelWindow;

        /// <summary>
        /// 关于信息
        /// </summary>
        public AboutPanelDisplay CurAboutPanelDisplay;

        #region 面板-关于

        /// <summary>
        /// 关于面板de显示内容
        /// </summary>
        public class AboutPanelDisplay : ProtoHelper.BasePanelDisplay
        {

            [PropertySpace(2, 8), TitleGroup("      ▼", ""), BoxGroup("      ▼/关于面板/XX", LabelText = "INTRODUCE"), HideLabel, PropertyOrder(-1), ReadOnly, MultiLineProperty(6)]
            public string About_explain = ProtoHelper.ExplainText;

            [PropertySpace(2), TitleGroup("      ▼", "", TitleAlignments.Split, Indent = true, HorizontalLine = false), BoxGroup("      ▼/关于面板", ShowLabel = true, CenterLabel = true), LabelText("VERSION:"), PropertyOrder(0), ReadOnly]
            public string About_Version = ProtoHelper.Version;

            [PropertySpace(2), TitleGroup("      ▼", ""), BoxGroup("      ▼/关于面板"), LabelText("RELEASED ON:"), PropertyOrder(1), ReadOnly]
            public string About_PushDate = ProtoHelper.PushDate;

            [PropertySpace(4), TitleGroup("      ▼", ""), BoxGroup("      ▼/关于面板"), LabelText("AUTHOR:"), PropertyOrder(2), ReadOnly]
            public string About_Author = ProtoHelper.Author;

            [PropertySpace(4), TitleGroup("      ▼", ""), BoxGroup("      ▼/关于面板"), LabelText("CONTACT-WECHAT:"), PropertyOrder(3), ReadOnly]
            public string About_WECHAT = ProtoHelper.WECHAT;


            [PropertySpace(4), TitleGroup("      ▼", ""), BoxGroup("      ▼/关于面板"), LabelText("CHANGE LOG:"), PropertyOrder(4), ReadOnly,DisableContextMenu,ListDrawerSettings(IsReadOnly = true)]
            public string[] About_ChangeLog = ProtoHelper.ChangeLog;

            [PropertySpace(4, 8), TitleGroup("      ▼", ""), BoxGroup("      ▼/关于面板"), PropertyOrder(5), Button("GITHUB")]
            public void About_Git()
            {
                UnityEngine.Application.OpenURL(ProtoHelper.WebsiteURL);
            }

        }


        #endregion
    }
}