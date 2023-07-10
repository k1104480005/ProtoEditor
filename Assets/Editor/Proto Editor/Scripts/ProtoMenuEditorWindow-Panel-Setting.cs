using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EditProto
{
    /// <summary>
    /// Proto编辑器窗口 - 设置面板
    /// </summary>
    public partial class ProtoMenuEditorWindow : OdinMenuEditorWindow
    {
        /// <summary>
        /// 设置面板的窗口引用
        /// </summary>
        private OdinEditorWindow _SettingPanelWindow;

        /// <summary>
        /// 设置信息
        /// </summary>
        public SettingPanelDisplay CurSettingPanelDisplay;



        #region 面板-设置

        /// <summary>
        /// 设置面板de显示内容
        /// </summary>
        public class SettingPanelDisplay : ProtoHelper.BasePanelDisplay
        {
            public SettingPanelDisplay(Action<ProtoHelper.BasePanelDisplay, string> showNotificationAction, Action refreshMenuTreeAction)
            {
                SETTING_prePath = Application.dataPath.Remove(Application.dataPath.LastIndexOf("Assets"));
                _showNotificationAction = showNotificationAction;
                _refreshMenuTreeAction = refreshMenuTreeAction;
            }

            [PropertyOrder(-20), OnInspectorGUI]
            void SHOWTIP()
            {
                SirenixEditorGUI.IconMessageBox("注意：如果正在编辑文件请不要修改路径配置，否则将丢失所有对文件的修改！", SdfIconType.ExclamationTriangleFill, Color.yellow);
            }


            [TitleGroup("      ▼", "", TitleAlignments.Split, Indent = true, HorizontalLine = false), BoxGroup("      ▼/设置面板", ShowLabel = true, CenterLabel = true), LabelText("配置文件"), PropertyOrder(-2), ReadOnly, DisplayAsString]
            public string SETTING_FileName = "__SETTING__.json";

            [PropertySpace(2), TitleGroup("      ▼"), BoxGroup("      ▼/设置面板"), LabelText("前置路径"), PropertyOrder(-1), ReadOnly, DisplayAsString]
            public string SETTING_prePath = "";

            [PropertySpace(2), TitleGroup("      ▼"), BoxGroup("      ▼/设置面板"), PropertyOrder(0), OnValueChanged("SaveSetting"), DetailedInfoBox("↓Tips", "①开启时将自动保存该设置文件的修改\n②关闭时则需要手动保存文件的修改", InfoMessageType.None), LabelText("自动保存设置")]
            public bool SETTING_AutoSaveSetting = true;

            [PropertySpace(4), TitleGroup("      ▼"), BoxGroup("      ▼/设置面板"), PropertyOrder(3), FolderPath, OnValueChanged("SaveSettingButCheckAutoFun"), Required("必须填写此项才能持久化设置"), LabelText("配置文件所在目录"), DetailedInfoBox("↓Tips", "工具的配置文件将保存在此目录！", InfoMessageType.None), ValidateInput("CheckFolderValidityFun_1")]
            public string SETTING_FolderPath = "";

            [PropertySpace(6), TitleGroup("      ▼"), BoxGroup("      ▼/设置面板"), PropertyOrder(4), FolderPath, OnValueChanged("SaveSettingButCheckAutoFun"), ValidateInput("CheckFolderValidityFun_1"), DetailedInfoBox("↓Tips", "工具的缓存文件将保存到此目录，如果不填写将会限制使用部分功能！", InfoMessageType.None), Required("必须填写此项才能正常使用全部功能"), LabelText("缓存文件所在目录")]
            public string SETTING_CachePath = "";

            [PropertySpace(6, 4), TitleGroup("      ▼"), BoxGroup("      ▼/设置面板"), PropertyOrder(5), FolderPath, OnValueChanged("SaveSettingButCheckAutoFun"), LabelText("Proto文件输出目录"), DetailedInfoBox("↓Tips", "生成Proto文件时输出到此目录！", InfoMessageType.None), ValidateInput("CheckFolderValidityFun_2")]
            public string SETTING_ProtoOutputFolderPath = "";

            [PropertySpace(6, 4), TitleGroup("      ▼"), BoxGroup("      ▼/设置面板"), PropertyOrder(5), FilePath(Extensions = "exe"), OnValueChanged("SaveSettingButCheckAutoFun"), LabelText("ProtoC转换器目录"), DetailedInfoBox("↓Tips", "把Proto文件转为C#协议文件的转换器(protoc.exe)所在目录！", InfoMessageType.None)]
            public string SETTING_ExeFullName = "";

            [PropertySpace(6, 4), TitleGroup("      ▼"), BoxGroup("      ▼/设置面板"), PropertyOrder(5), FolderPath, OnValueChanged("SaveSettingButCheckAutoFun"), LabelText("C#协议存放目录"), DetailedInfoBox("↓Tips", "从Proto文件转换成C#协议文件后将存放在此目录！", InfoMessageType.None)]
            public string SETTING_CSOutputFolder = "";


            [PropertySpace(6, 4), TitleGroup("      ▼"), BoxGroup("      ▼/设置面板"), PropertyOrder(10), ShowInInspector, OnValueChanged("SaveSettingButCheckAutoFun"), LabelText("调试-普通日志开关"), DetailedInfoBox("↓Tips", "在Console显示用于开发调试的普通日志！", InfoMessageType.None)]
            public bool SETTING_EnableDebugLog { get { return ProtoHelper.enableDebugLog; } set { ProtoHelper.enableDebugLog = value; } }

            [PropertySpace(6, 4), TitleGroup("      ▼"), BoxGroup("      ▼/设置面板"), PropertyOrder(11), ShowInInspector, OnValueChanged("SaveSettingButCheckAutoFun"), LabelText("调试-错误日志开关"), DetailedInfoBox("↓Tips", "在Console显示用于开发调试的错误日志！", InfoMessageType.None)]
            public bool SETTING_EnableDebugError { get { return ProtoHelper.enableDebugError; } set { ProtoHelper.enableDebugError = value; } }


            [PropertySpace(6, 4), TitleGroup("      ▼"), BoxGroup("      ▼/设置面板"), PropertyOrder(20), DetailedInfoBox("↓Tips", "在未开启自动保存设置的情况下，请点此进行手动保存！", InfoMessageType.None), Button(SdfIconType.CheckCircleFill, "手动保存", ButtonHeight = 25), GUIColor("#44DB7F"), HideIf("SETTING_AutoSaveSetting")]
            public void SETTING_ManualSave()
            {
                SaveSetting(true);
            }

            #region 私有方法 & 字段

            private bool isDirty = false;

            private Action _refreshMenuTreeAction;

            private Action<ProtoHelper.BasePanelDisplay, string> _showNotificationAction;

            private bool CheckFolderValidityFun_1(string value, ref string message, ref InfoMessageType? messageType)
            {
                return InternalCheckFolderValidityFun(value, ref message, ref messageType, "", InfoMessageType.None);
            }

            private bool CheckFolderValidityFun_2(string value, ref string message, ref InfoMessageType? messageType)
            {
                return InternalCheckFolderValidityFun(value, ref message, ref messageType, "请填写此项！", InfoMessageType.Warning);
            }

            /// <summary>
            /// 检查文件夹路径有效性
            /// </summary>
            /// <param name="value"> 文件夹路径 </param>
            /// <param name="message"></param>
            /// <param name="messageType"></param>
            /// <returns></returns>
            private bool InternalCheckFolderValidityFun(string value, ref string message, ref InfoMessageType? messageType, string nullTip, InfoMessageType? nullTipType)
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (string.IsNullOrEmpty(nullTip))
                    {
                        message = "";
                        messageType = InfoMessageType.None;
                    }
                    else
                    {
                        message = nullTip;
                        messageType = nullTipType;
                    }
                    return false;
                }

                string folderPath = SETTING_prePath + value;

                //检查文件夹合法性
                if (!Directory.Exists(folderPath))
                {
                    messageType = InfoMessageType.Error;
                    message = $"目录不存在，请检查!";
                    return false;
                }

                return true;
            }


            /// <summary>
            /// 字段触发的保存设置方法（判断是否自动保存）
            /// </summary>
            private void SaveSettingButCheckAutoFun()
            {
                //仅自动保存处于开始状态才会保存
                if (SETTING_AutoSaveSetting)
                    SaveSetting(false);
            }

            /// <summary>
            /// 结束渲染时调用-判断是否更改了配置需要刷新MenuTree
            /// </summary>
            [OnInspectorDispose]
            void OnDispose()
            {
                if (isDirty)
                {
                    isDirty = false;
                    _refreshMenuTreeAction?.Invoke();
                }
            }

            #endregion


            /// <summary>
            /// 保存SETTING
            /// </summary>
            public void SaveSetting(bool showNotify = false)
            {
                if (string.IsNullOrEmpty(SETTING_FolderPath))
                {
                    ProtoHelper.LogErrorInfo("[配置文件所在目录] 未设置");
                    return;
                }

                string path = Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets")), SETTING_FolderPath).Replace('\\', '/');
                if (!Directory.Exists(path))
                {
                    ProtoHelper.LogErrorInfo($"[配置文件所在目录] 错误路径 :{path}");
                    return;
                }

                string path2 = Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets")), SETTING_CachePath).Replace('\\', '/');
                if (!Directory.Exists(path2))
                {
                    ProtoHelper.LogErrorInfo($"[缓存文件所在目录] 错误路径 :{path2}");
                    return;
                }


                Dictionary<string, object> dataDic = new Dictionary<string, object>();
                dataDic.Add("SETTING_FolderPath", SETTING_FolderPath);
                dataDic.Add("SETTING_ProtoOutputFolderPath", SETTING_ProtoOutputFolderPath);
                dataDic.Add("SETTING_CachePath", SETTING_CachePath);
                dataDic.Add("SETTING_AutoSaveSetting", SETTING_AutoSaveSetting);
                dataDic.Add("SETTING_ExeFullName", SETTING_ExeFullName);
                dataDic.Add("SETTING_CSOutputFolder", SETTING_CSOutputFolder);


                string json = JsonConvert.SerializeObject(dataDic);

                //写入
                string p = path + "/" + SETTING_FileName;
                if (File.Exists(p))
                    ProtoHelper.LogInfo("[Proto工具设置] 完成覆盖保存");
                else
                    ProtoHelper.LogInfo("[Proto工具设置] 完成保存");
                File.WriteAllText(p, json);

                if (showNotify)
                    _showNotificationAction?.Invoke(this, "设置保存成功");

                isDirty = true;
            }

            /// <summary>
            /// 初始自动寻找到配置所在的位置
            /// </summary>
            /// <returns></returns>
            string AutoFindSetting()
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("__SETTING__");
                if (guids.Length > 0)
                {
                    foreach (var id in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(id);
                        ProtoHelper.LogInfo($"已自动找到{path}");

                        string settingFullname = Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets")), path).Replace('\\', '/');
                       return Path.GetDirectoryName(settingFullname);
                    }
                }
                return string.Empty;
            }

            /// <summary>
            /// 读取SETTING
            /// </summary>
            public void ReadSetting()
            {
                //初始打开时SETTING_FolderPath是空的，所以自动寻找文件位置
                if (string.IsNullOrEmpty(SETTING_FolderPath))
                    SETTING_FolderPath = AutoFindSetting();

                //这里证明自动寻找也没找到，证明从未创建过
                if (string.IsNullOrEmpty(SETTING_FolderPath))
                {
                    ProtoHelper.LogErrorInfo("[配置文件所在目录] 未设置");
                    AutoFindSetting();
                    return;
                }

                string path = Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets")), SETTING_FolderPath).Replace('\\', '/');
                if (!Directory.Exists(path))
                {
                    ProtoHelper.LogErrorInfo($"[配置文件所在目录] 错误路径 :{path}");
                    return;
                }

                string filePath = path + "/" + SETTING_FileName;
                if (!File.Exists(filePath))
                {
                    ProtoHelper.LogErrorInfo($"[配置文件] 不存在 :{path}");
                    return;
                }

                //读取
                string json = File.ReadAllText(filePath);
                Dictionary<string, object> dataDic = new Dictionary<string, object>();
                dataDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                bool hasError = false;
                object obj;
                if (dataDic.TryGetValue("SETTING_FolderPath", out obj))
                    SETTING_FolderPath = (string)obj;
                else hasError = true;
                if (dataDic.TryGetValue("SETTING_ProtoOutputFolderPath", out obj))
                    SETTING_ProtoOutputFolderPath = (string)obj;
                else hasError = true;
                if (dataDic.TryGetValue("SETTING_CachePath", out obj))
                    SETTING_CachePath = (string)obj;
                else hasError = true;
                if (dataDic.TryGetValue("SETTING_AutoSaveSetting", out obj))
                    SETTING_AutoSaveSetting = (bool)obj;
                else hasError = true;
                if (dataDic.TryGetValue("SETTING_ExeFullName", out obj))
                    SETTING_ExeFullName = (string)obj;
                else hasError = true;
                if (dataDic.TryGetValue("SETTING_CSOutputFolder", out obj))
                    SETTING_CSOutputFolder = (string)obj;
                else hasError = true;


                if (hasError)
                    ProtoHelper.LogErrorInfo($"[设置文件] 读取有错误,请检查");
                else
                    ProtoHelper.LogInfo($"[设置文件] 成功读取 :{filePath}");
            }

        }

        #endregion



    }
}