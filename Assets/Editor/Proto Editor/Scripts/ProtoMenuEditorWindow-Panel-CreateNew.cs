using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditProto
{
    /// <summary>
    /// Proto编辑器窗口 - 创建新文件面板
    /// </summary>
    public partial class ProtoMenuEditorWindow : OdinMenuEditorWindow
    {
        /// <summary>
        /// 创建新文件面板的窗口引用
        /// </summary>
        private OdinEditorWindow _CreateNewFilePanelWindow;

        /// <summary>
        /// 创建新文件面板的数据
        /// </summary>
        public QuickCreateFilePanelDisplay CurQuickCreateFilePanelDisplay;


        #region 面板-创建新文件

        /// <summary>
        /// 创建文件面板de显示内容
        /// </summary>
        public class QuickCreateFilePanelDisplay : ProtoHelper.BasePanelDisplay
        {
            public QuickCreateFilePanelDisplay(Func<string> getOutputPathFunc, Action<string> setOutputPathAction, Action initiativeClosePanelAction, Action<string> createFinishAction)
            {
                _getOutputPathFunc = getOutputPathFunc;
                _setOutputPathAction = setOutputPathAction;
                _initiativeClosePanelAction = initiativeClosePanelAction;
                _createFinishAction = createFinishAction;
            }

            #region 私有字段

            /// <summary>
            /// 指定后缀
            /// </summary>
            private string _fileSuffix = ".proto";

            /// <summary>
            /// 获得文件输出路径的Func
            /// </summary>
            private Func<string> _getOutputPathFunc;

            /// <summary>
            /// 设置文件输出路径的 Action
            /// </summary>
            private Action<string> _setOutputPathAction;

            /// <summary>
            /// 主动关闭窗口 Action
            /// </summary>
            private Action _initiativeClosePanelAction;

            /// <summary>
            /// 创建完成 Action
            /// </summary>
            private Action<string> _createFinishAction;

            /// <summary>
            /// 检查错误标记(0为通过)(1为未填写)(2为有后缀)(3为输出路径错误)(4未文件重复)
            /// </summary>
            private int _errorKey = -1;


            #endregion

            /// <summary>
            /// 文件名
            /// </summary>
            [TitleGroup("      ▼", "", TitleAlignments.Split, Indent = true, HorizontalLine = false), BoxGroup("      ▼/创建新文件面板", ShowLabel = true, CenterLabel = true), PropertyOrder(0), LabelText("文件名", Icon = SdfIconType.StarFill, IconColor = "#CD60F3"), SuffixLabel("@this._fileSuffix"), ValidateInput("CheckNameValidityFun", "文件名填写有问题", InfoMessageType.Warning), GUIColor("#FFFFFF"), DelayedProperty]
            public string FileName;

            /// <summary>
            /// 全路径
            /// </summary>
            [TitleGroup("      ▼"), BoxGroup("      ▼/创建新文件面板"), PropertyOrder(1), LabelText("预   览", Icon = SdfIconType.Link), HideLabel, ReadOnly, GUIColor("#FFFFFF")]
            public string FileFullName;

            [TitleGroup("      ▼"), BoxGroup("      ▼/创建新文件面板"), PropertyOrder(2), LabelText("使用快照模板")]
            public bool UseSnapShootTemplate = false;

            [TitleGroup("      ▼"), BoxGroup("      ▼/创建新文件面板"), PropertyOrder(3), LabelText("模板", Icon = SdfIconType.Link), ShowIf("UseSnapShootTemplate"), AssetSelector(Filter = "snapshoot t:ScriptableObject", FlattenTreeView = true, DropdownWidth = 800), InfoBox("使用模板可能会出现“消息体”和“枚举”重复的情况，请根据需求修改“消息体”和“枚举”的命名，或添加“命名空间”来区分！", SdfIconType.ArrowDownCircle)]
            public SnapShootScriptableObject SnapShootTemplate;

            /// <summary>
            /// 创建按钮
            /// </summary>
            [PropertySpace(10, 4), TitleGroup("      ▼"), BoxGroup("      ▼/创建新文件面板"), PropertyOrder(4), GUIColor("#6FD48C"), Button("生成", ButtonHeight = 30, Icon = SdfIconType.CheckCircleFill), HideIf("@this._errorKey == 3"), DisableIf("@this._errorKey != 0")]
            public void CreateNowButton()
            {
                string initContent = "";

                //检查模板
                if (UseSnapShootTemplate)
                {
                    if (SnapShootTemplate == null)
                    {
                        EditorUtility.DisplayDialog("提示", $"未选择模板文件！", "确定");
                        GUIUtility.ExitGUI();
                        return;
                    }
                    if (SnapShootTemplate.wrapper == null)
                    {
                        EditorUtility.DisplayDialog("提示", $"选择的模板文件无效！", "确定");
                        GUIUtility.ExitGUI();
                        return;
                    }

                    //传入模板数据
                    initContent = ProtoHelper.EditorInfo2ProtoText(SnapShootTemplate.wrapper);
                }


                string message = "";
                InfoMessageType? infoType = InfoMessageType.None;
                bool pass = InternalCheckNameValidityFun(FileName, ref message, ref infoType, out _errorKey);

                if (pass)
                {
                    //新建逻辑
                    if (!File.Exists(FileFullName))
                    {
                        //创建新文件并写入空内容
                        File.WriteAllText(FileFullName, initContent);

                        _createFinishAction?.Invoke(FileName);
                    }
                    else
                        Debug.LogError("CreateNowButton FileFullName is Exists!");

                    _initiativeClosePanelAction?.Invoke();
                }
            }

            [PropertySpace(10, 4), TitleGroup("      ▼"), BoxGroup("      ▼/创建新文件面板"), PropertyOrder(5), GUIColor("#6FB9D4"), Button("立即设置输出目录", ButtonHeight = 30, Icon = SdfIconType.Tools), ShowIf("@this._errorKey == 3")]
            public void SetOutputButton()
            {
                //检查输出目录发生错误,让用户设置输出目录
                if (_errorKey == 3)
                {
                    string projectPath = Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets"));
                    string selectedPath = EditorUtility.SaveFolderPanel("输出目录", projectPath, "");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        //调整路径
                        string p = selectedPath.Replace(projectPath + "/", "");

                        //调用设置目录回调
                        _setOutputPathAction?.Invoke(p);

                        //关闭面板
                        _initiativeClosePanelAction?.Invoke();
                    }
                }
                else
                    Debug.LogError("SetOutput _errorKey error");
            }

            /// <summary>
            /// 输入检验：检查文件名的合法性
            /// </summary>
            /// <param name="value"> 文件名无后缀 </param>
            /// <returns> 返回是否通过 </returns>
            private bool CheckNameValidityFun(string value, ref string message, ref InfoMessageType? messageType)
            {
                bool pass = InternalCheckNameValidityFun(value, ref message, ref messageType, out _errorKey);
                return pass;
            }

            /// <summary>
            /// 输入检验：检查文件名的合法性 -内部方法
            /// </summary>
            /// <param name="value"></param>
            /// <param name="message"></param>
            /// <param name="messageType"></param>
            /// <param name="errorKey"> 错误码 </param>
            /// <returns></returns>
            private bool InternalCheckNameValidityFun(string value, ref string message, ref InfoMessageType? messageType, out int errorKey)
            {
                errorKey = 0;

                //未填写
                if (string.IsNullOrEmpty(value))
                {
                    messageType = InfoMessageType.None;
                    message = "";
                    _errorKey = 1;
                    FileFullName = "";//FullName赋值
                    return false;
                }

                //填写带后缀
                if (value.LastIndexOf('.') > 0)
                {
                    messageType = InfoMessageType.Error;
                    message = "无需填写后缀，请检查！";
                    errorKey = 2;
                    return false;
                }

                string outputPath = _getOutputPathFunc == null ? "" : _getOutputPathFunc.Invoke();
                string projectPath = Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets"));
                string folderPath = Path.Combine(projectPath, outputPath).Replace('\\', '/');
                /* 合并成预览的文件全路径，如果输出文件夹路径错误，那该路径就是错误的） */
                string fullName = Path.Combine(folderPath, string.Format($"{value}{_fileSuffix}"));
                FileFullName = fullName;//FullName赋值

                //检查文件夹合法性
                if (!Directory.Exists(folderPath))
                {
                    messageType = InfoMessageType.Error;
                    message = $"输出目录错误，请检查：{folderPath}";
                    errorKey = 3;
                    return false;
                }

                //检查文件重复
                if (File.Exists(fullName))
                {
                    messageType = InfoMessageType.Error;
                    message = $"已存在同名文件{value}，请更换命名！";
                    errorKey = 4;
                    return false;
                }

                return true;
            }

        }

        #endregion

        #region 方法

        /// <summary>
        /// 获得文件输出路径
        /// </summary>
        /// <returns></returns>
        private string GetFileOutputPathCallback()
        {
            return CurSettingPanelDisplay.SETTING_ProtoOutputFolderPath;
        }

        /// <summary>
        /// 设置文件输出路径
        /// </summary>
        /// <param name="folderPath"></param>
        private void SetFileOutputPathCallback(string folderPath)
        {
            CurSettingPanelDisplay.SETTING_ProtoOutputFolderPath = folderPath;
            CurSettingPanelDisplay.SaveSetting();
        }

        /// <summary>
        /// 关闭创建新文件的面板回调
        /// </summary>
        private void CloseFileCreatePanelCallback()
        {
            _CreateNewFilePanelWindow?.Close();
        }

        /// <summary>
        /// 创建完成新文件回调
        /// </summary>
        private void CreateFileFinishCallback(string fileName)
        {
            RefreshAllProtoFiles(false);
        }

        #endregion
    }
}