using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EditProto
{
    /// <summary>
    /// 助手类
    /// </summary>
    public static partial class ProtoHelper
    {
        public const string Version = "0.8";
        public const string Author = "虐人";
        public const string WECHAT = "k427006999";
        public const string QQ = "";
        public const string PushDate = "2023-07-07";
        public const string WebsiteURL = "https://github.com/k1104480005/ProtoEditor";
        public static string[] ChangeLog =
        {

        };
        public const string ExplainText =
            "1.这是管理.Proto文件的工具，可以快速新增、删除、编辑Proto文件，提升工具效率" +
            "\n2.目前可以一键转换为C#协议代码" +
            "\n3.(未完成)加入快照功能，在新建Proto文件时可以选择以前保存的快照作为模板" +
            "\n4.目前已增强工具的检错能力" +
            "\n5.如果有更好的建议或意见欢迎联系反馈";

        #region GUIStyle

        public static GUIStyle guiStyle_1 = new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = Color.gray },
            hover = new GUIStyleState() { textColor = Color.red }
        };

        public static GUIStyle guiStyle_2 = new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = color_FFC107 },
            hover = new GUIStyleState() { textColor = color_FFC107 }
        };

        //样式-（在OdinMenuStyleExample窗口设置好后Copy C# Snippet 粘贴过来的）
        public static OdinMenuStyle customMenuStyle = new OdinMenuStyle()
        {
            Height = 25,
            Offset = 12.00f,
            IndentAmount = 5.00f,
            IconSize = 18.00f,
            IconOffset = 0.00f,
            NotSelectedIconAlpha = 0.8f,
            IconPadding = 0.00f,
            TriangleSize = 18.00f,
            TrianglePadding = 10.00f,
            AlignTriangleLeft = false,
            DrawFoldoutTriangle = true,
            Borders = true,
            BorderPadding = 0.00f,
            BorderAlpha = 0.32f,
            SelectedColorDarkSkin = new Color(0.243f, 0.373f, 0.588f, 1.000f),
            SelectedColorLightSkin = new Color(0.243f, 0.490f, 0.900f, 1.000f)
        };

        #endregion

        #region GUIContent

        private static int iconSize18 = 18;
        private static int iconSize16 = 16;
        private static int iconSize14 = 14;

        public static GUIContent guiContent_panel_createBtn = new GUIContent("创建 ", SdfIcons.CreateTransparentIconTexture(SdfIconType.FileEarmarkPlusFill, color_6FD48C, iconSize18, iconSize18, 0), "展开创建新文件面板");

        public static GUIContent guiContent_panel_settingBtn = new GUIContent("设置 ", SdfIcons.CreateTransparentIconTexture(SdfIconType.GearFill, Color.white, iconSize18, iconSize18, 0), "展开设置面板");

        public static GUIContent guiContent_panel_settingBtn2 = new GUIContent("设置 ", SdfIcons.CreateTransparentIconTexture(SdfIconType.GearFill, Color.red, iconSize18, iconSize18, 0), "展开设置面板");

        public static GUIContent guiContent_panel_aboutBtn = new GUIContent("关于 ", SdfIcons.CreateTransparentIconTexture(SdfIconType.InfoCircleFill, Color.white, iconSize18, iconSize18, 0), "展开关于面板");

        public static GUIContent guiContent_panel_cameraBtn = new GUIContent("快照集 ", SdfIcons.CreateTransparentIconTexture(SdfIconType.GridFill, Color.white, iconSize18, iconSize18, 0), "展开快照管理面板");

        public static GUIContent guiContent_panel_2csAllBtn = new GUIContent("所有文件转为C#协议", SdfIcons.CreateTransparentIconTexture(SdfIconType.Translate, Color.white, iconSize18, iconSize18, 0), "");

        public static GUIContent guiContent_panel_refreshAssetBtn = new GUIContent("刷新新增资源", SdfIcons.CreateTransparentIconTexture(SdfIconType.CircleFill, Color.white, iconSize18, iconSize18, 0), "");

        public static GUIContent guiContent_panel_saveBtn = new GUIContent(" 保存 ", SdfIcons.CreateTransparentIconTexture(SdfIconType.Save, Color.gray, iconSize18, iconSize18, 0));

        public static GUIContent guiContent_panel_saveBtn2 = new GUIContent(" 保存 ", SdfIcons.CreateTransparentIconTexture(SdfIconType.SaveFill, Color.green, iconSize18, iconSize18, 0));

        public static GUIContent guiContent_panel_previewBtn = new GUIContent(" 预览 ", SdfIcons.CreateTransparentIconTexture(SdfIconType.EyeFill, Color.white, iconSize18, iconSize18, 0));

        public static GUIContent guiContent_panel_findfolderBtn = new GUIContent(" 定位 ", SdfIcons.CreateTransparentIconTexture(SdfIconType.Folder, Color.white, iconSize18, iconSize18, 0));

        public static GUIContent guiContent_panel_checkBtn = new GUIContent(" 检查 ", SdfIcons.CreateTransparentIconTexture(SdfIconType.BugFill, Color.white, iconSize18, iconSize18, 0));

        public static GUIContent guiContent_fastCamera_checkBtn = new GUIContent(" 快照 ", SdfIcons.CreateTransparentIconTexture(SdfIconType.CameraFill, Color.white, iconSize18, iconSize18, 0));

        public static GUIContent guiContent_wrapper_drawTools_bugchecker = new GUIContent("", SdfIcons.CreateTransparentIconTexture(SdfIconType.BugFill, Color.white, iconSize14, iconSize14, 0), "检查填写错误");

        public static GUIContent guiContent_2cs_btn = new GUIContent(" 转换为C#协议 ", SdfIcons.CreateTransparentIconTexture(SdfIconType.Translate, Color.white, iconSize18, iconSize18, 0));


        #endregion

        #region Color

        private static Color? _color_6FD48C;
        public static Color color_6FD48C
        {
            get
            {
                if (_color_6FD48C == null)
                {
                    Color c;
                    ColorUtility.TryParseHtmlString("#6FD48C", out c);
                    _color_6FD48C = c;
                }
                return (Color)_color_6FD48C;
            }
        }

        private static Color? _color_FFC107;
        public static Color color_FFC107
        {
            get
            {
                if (_color_FFC107 == null)
                {
                    Color c;
                    ColorUtility.TryParseHtmlString("#FFC107", out c);
                    _color_FFC107 = c;
                }
                return (Color)_color_FFC107;
            }
        }


        private static Color? _color_Error1;
        public static Color color_Error1
        {
            get
            {
                if (_color_Error1 == null)
                {
                    Color c;
                    ColorUtility.TryParseHtmlString("#5A4142", out c);
                    _color_Error1 = c;
                }
                return (Color)_color_Error1;
            }
        }




        #endregion

        #region Const

        //Debug日志开关
        public static bool enableDebugLog { get { return (PlayerPrefs.GetInt("PE_enableDebugLog", 0)) != 0 ? true : false; } set { PlayerPrefs.SetInt("PE_enableDebugLog", value ? 1 : 0); } }
        public static bool enableDebugError { get { return (PlayerPrefs.GetInt("PE_enableDebugError", 0)) != 0 ? true : false; } set { PlayerPrefs.SetInt("PE_enableDebugError", value ? 1 : 0); } }

        public static string MenuTreeRootName = "文件列表";
        public static string MenuTreeRootName_Null = "没找到文件";

        public static bool ShowEditorInfoTipToggle = false; //包装体界面显示字段提示信息

        #endregion

        #region 结构 & 类

        /// <summary>
        /// 面板的基类
        /// </summary>
        public class BasePanelDisplay
        {

        }

        #endregion

        #region 方法

        /// <summary>
        /// 判断字符串内是否含有中文
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasChinese(string str) { return Regex.IsMatch(str, @"[\u4e00-\u9fa5]"); }


        /// <summary>
        /// 是否坏名字？- 判断字符串是否包含除字母、数字、下划线以外的字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns> 是否有意外的字符 </returns>
        public static bool IsBadName(string str)
        {
            //空字符直接返回符合
            if (string.IsNullOrEmpty(str))
                return false;

            //是否有限定之外的字符？
            Match mat = Regex.Match(str, @"[^a-zA-Z0-9_+]");
            if (mat.Success)
                return true;
            else
            {
                //开头是否非字母
                mat = Regex.Match(str, @"^[^a-zA-Z+]");
                if (mat.Success)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 通用的 找到文件夹下的所有文件
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fullnames"></param>
        /// <param name="names"></param>
        /// <param name="suffix"> 后缀 </param>
        /// <returns></returns>
        public static int FindAllFiles(string folderPath, out List<string> fullnames, out List<string> names, string suffix = null)
        {
            fullnames = new List<string>();  //文件绝对路径
            names = new List<string>();      //文件带后缀名称

            int count = 0;

            //判断是否有此文件夹
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo direction = new DirectoryInfo(folderPath);
                FileInfo[] files = string.IsNullOrEmpty(suffix) ? direction.GetFiles("*") : direction.GetFiles($"*{suffix}");
                for (int i = 0; i < files.Length; i++)
                {
                    //去除Unity内部.meta文件
                    if (files[i].Name.EndsWith(".meta"))
                        continue;

                    fullnames.Add(files[i].FullName);
                    names.Add(files[i].Name);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Debug日志
        /// </summary>
        /// <param name="msg"></param>
        public static void Log(string msg)
        {
            if (enableDebugLog)
                Debug.Log(string.Format($"<color=#D4A42D>[ProtoEditor Debuger]</color> {msg}"));
        }

        /// <summary>
        /// Debug错误日志
        /// </summary>
        /// <param name="msg"></param>
        public static void LogError(string msg)
        {
            if (enableDebugError)
                Debug.LogError(string.Format($"<color=#D4A42D>[ProtoEditor Debuger]</color> {msg}"));
        }

        /// <summary>
        /// 用户日志信息
        /// </summary>
        /// <param name="msg"></param>
        public static void LogInfo(string msg)
        {
            Debug.Log(string.Format($"<color=#3380D2>[ProtoEditor][用户信息]</color> {msg}"));
        }

        /// <summary>
        /// 用户错误日志信息
        /// </summary>
        /// <param name="msg"></param>
        public static void LogErrorInfo(string msg)
        {
            Debug.LogError(string.Format($"<color=#D4A42D>[ProtoEditor][用户信息]</color> {msg}"));
        }

        /// <summary>
        /// 显示通知
        /// </summary>
        public static void ShowNotification(OdinEditorWindow window, GUIContent content)
        {
            if (window != null)
            {
                window.ShowNotification(content);
            }
            else
            {
                if (ProtoMenuEditorWindow.Instance != null)
                    ProtoMenuEditorWindow.Instance.ShowNotification(content);
            }
        }

        /// <summary>
        /// 获得 已存在的Proto文件的 Dropdown列表
        /// </summary>
        /// <param name="folder"> 指定寻找的文件夹 </param>
        /// <param name="blacklist"> 黑名单列表，排除该列表的名字 </param>
        /// <returns> 返回 Dropdown列表(命名/命名) </returns>
        public static ValueDropdownList<string> GetProtoFileNameValueDropdownList(string folder = null, List<string> blacklist = null)
        {
            //尝试获得输出文件夹
            string outPutFolder = "";
            {
                //(先尝试从参数读取)
                if (!string.IsNullOrEmpty(folder))
                    outPutFolder = folder;
                else
                {
                    outPutFolder = TryGetOutputPath();
                }

                if (string.IsNullOrEmpty(outPutFolder) || !Directory.Exists(outPutFolder))
                {
                    LogError("GetFilteredImprotFileList error outPutFolder:" + outPutFolder);
                    return new Sirenix.OdinInspector.ValueDropdownList<string>();
                }
            }

            //找到所有文件
            List<string> fileNames;
            List<string> filefullNames;
            FindAllFiles(outPutFolder, out filefullNames, out fileNames, ".proto");

            //过滤-排除黑名单
            foreach (var name in blacklist)
            {
                int i = fileNames.IndexOf(name);
                if (i >= 0)
                {
                    fileNames.RemoveAt(i);
                    filefullNames.RemoveAt(i);
                }
            }

            //组成 ValueDropdownList
            ValueDropdownList<string> dl = new Sirenix.OdinInspector.ValueDropdownList<string>();
            for (int i = 0; i < fileNames.Count; i++)
                dl.Add(fileNames[i], fileNames[i]);
            return dl;
        }

        /// <summary>
        /// 尝试获得文件输出文件夹
        /// </summary>
        /// <returns></returns>
        public static string TryGetOutputPath()
        {
            //尝试获得输出文件夹
            string outPutFolder = "";
            //(尝试从ProtoMenuEditorWindow窗口获取)
            if (ProtoMenuEditorWindow.Instance != null)
                outPutFolder = ProtoMenuEditorWindow.Instance.CurSettingPanelDisplay.SETTING_ProtoOutputFolderPath;
            //（实在不行则直接读取文件加载）
            else
            {
                ProtoMenuEditorWindow.SettingPanelDisplay temp = new ProtoMenuEditorWindow.SettingPanelDisplay(null, null);
                temp.ReadSetting();
                outPutFolder = temp.SETTING_ProtoOutputFolderPath;
                temp = null;
            }

            return outPutFolder;
        }

        /// <summary>
        /// 标记最后选择的MenuItem数据为脏
        /// </summary>
        public static void SetDirty()
        {
            SetDirtyLastSelected();
        }

        /// <summary>
        /// 标记最后选择的MenuItem数据为脏
        /// </summary>
        static void SetDirtyLastSelected()
        {
            if (ProtoMenuEditorWindow.sLastSelecetedMenuItemData != null)
                ProtoMenuEditorWindow.sLastSelecetedMenuItemData.IsDrity = true;
        }

        #endregion


    }
}