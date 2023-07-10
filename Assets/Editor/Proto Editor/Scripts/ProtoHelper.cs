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
    /// ������
    /// </summary>
    public static partial class ProtoHelper
    {
        public const string Version = "0.8";
        public const string Author = "Ű��";
        public const string WECHAT = "k427006999";
        public const string QQ = "";
        public const string PushDate = "2023-07-07";
        public const string WebsiteURL = "https://github.com/k1104480005/ProtoEditor";
        public static string[] ChangeLog =
        {

        };
        public const string ExplainText =
            "1.���ǹ���.Proto�ļ��Ĺ��ߣ����Կ���������ɾ�����༭Proto�ļ�����������Ч��" +
            "\n2.Ŀǰ����һ��ת��ΪC#Э�����" +
            "\n3.(δ���)������չ��ܣ����½�Proto�ļ�ʱ����ѡ����ǰ����Ŀ�����Ϊģ��" +
            "\n4.Ŀǰ����ǿ���ߵļ������" +
            "\n5.����и��õĽ���������ӭ��ϵ����";

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

        //��ʽ-����OdinMenuStyleExample�������úú�Copy C# Snippet ճ�������ģ�
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

        public static GUIContent guiContent_panel_createBtn = new GUIContent("���� ", SdfIcons.CreateTransparentIconTexture(SdfIconType.FileEarmarkPlusFill, color_6FD48C, iconSize18, iconSize18, 0), "չ���������ļ����");

        public static GUIContent guiContent_panel_settingBtn = new GUIContent("���� ", SdfIcons.CreateTransparentIconTexture(SdfIconType.GearFill, Color.white, iconSize18, iconSize18, 0), "չ���������");

        public static GUIContent guiContent_panel_settingBtn2 = new GUIContent("���� ", SdfIcons.CreateTransparentIconTexture(SdfIconType.GearFill, Color.red, iconSize18, iconSize18, 0), "չ���������");

        public static GUIContent guiContent_panel_aboutBtn = new GUIContent("���� ", SdfIcons.CreateTransparentIconTexture(SdfIconType.InfoCircleFill, Color.white, iconSize18, iconSize18, 0), "չ���������");

        public static GUIContent guiContent_panel_cameraBtn = new GUIContent("���ռ� ", SdfIcons.CreateTransparentIconTexture(SdfIconType.GridFill, Color.white, iconSize18, iconSize18, 0), "չ�����չ������");

        public static GUIContent guiContent_panel_2csAllBtn = new GUIContent("�����ļ�תΪC#Э��", SdfIcons.CreateTransparentIconTexture(SdfIconType.Translate, Color.white, iconSize18, iconSize18, 0), "");

        public static GUIContent guiContent_panel_refreshAssetBtn = new GUIContent("ˢ��������Դ", SdfIcons.CreateTransparentIconTexture(SdfIconType.CircleFill, Color.white, iconSize18, iconSize18, 0), "");

        public static GUIContent guiContent_panel_saveBtn = new GUIContent(" ���� ", SdfIcons.CreateTransparentIconTexture(SdfIconType.Save, Color.gray, iconSize18, iconSize18, 0));

        public static GUIContent guiContent_panel_saveBtn2 = new GUIContent(" ���� ", SdfIcons.CreateTransparentIconTexture(SdfIconType.SaveFill, Color.green, iconSize18, iconSize18, 0));

        public static GUIContent guiContent_panel_previewBtn = new GUIContent(" Ԥ�� ", SdfIcons.CreateTransparentIconTexture(SdfIconType.EyeFill, Color.white, iconSize18, iconSize18, 0));

        public static GUIContent guiContent_panel_findfolderBtn = new GUIContent(" ��λ ", SdfIcons.CreateTransparentIconTexture(SdfIconType.Folder, Color.white, iconSize18, iconSize18, 0));

        public static GUIContent guiContent_panel_checkBtn = new GUIContent(" ��� ", SdfIcons.CreateTransparentIconTexture(SdfIconType.BugFill, Color.white, iconSize18, iconSize18, 0));

        public static GUIContent guiContent_fastCamera_checkBtn = new GUIContent(" ���� ", SdfIcons.CreateTransparentIconTexture(SdfIconType.CameraFill, Color.white, iconSize18, iconSize18, 0));

        public static GUIContent guiContent_wrapper_drawTools_bugchecker = new GUIContent("", SdfIcons.CreateTransparentIconTexture(SdfIconType.BugFill, Color.white, iconSize14, iconSize14, 0), "�����д����");

        public static GUIContent guiContent_2cs_btn = new GUIContent(" ת��ΪC#Э�� ", SdfIcons.CreateTransparentIconTexture(SdfIconType.Translate, Color.white, iconSize18, iconSize18, 0));


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

        //Debug��־����
        public static bool enableDebugLog { get { return (PlayerPrefs.GetInt("PE_enableDebugLog", 0)) != 0 ? true : false; } set { PlayerPrefs.SetInt("PE_enableDebugLog", value ? 1 : 0); } }
        public static bool enableDebugError { get { return (PlayerPrefs.GetInt("PE_enableDebugError", 0)) != 0 ? true : false; } set { PlayerPrefs.SetInt("PE_enableDebugError", value ? 1 : 0); } }

        public static string MenuTreeRootName = "�ļ��б�";
        public static string MenuTreeRootName_Null = "û�ҵ��ļ�";

        public static bool ShowEditorInfoTipToggle = false; //��װ�������ʾ�ֶ���ʾ��Ϣ

        #endregion

        #region �ṹ & ��

        /// <summary>
        /// ���Ļ���
        /// </summary>
        public class BasePanelDisplay
        {

        }

        #endregion

        #region ����

        /// <summary>
        /// �ж��ַ������Ƿ�������
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasChinese(string str) { return Regex.IsMatch(str, @"[\u4e00-\u9fa5]"); }


        /// <summary>
        /// �Ƿ����֣�- �ж��ַ����Ƿ��������ĸ�����֡��»���������ַ�
        /// </summary>
        /// <param name="str"></param>
        /// <returns> �Ƿ���������ַ� </returns>
        public static bool IsBadName(string str)
        {
            //���ַ�ֱ�ӷ��ط���
            if (string.IsNullOrEmpty(str))
                return false;

            //�Ƿ����޶�֮����ַ���
            Match mat = Regex.Match(str, @"[^a-zA-Z0-9_+]");
            if (mat.Success)
                return true;
            else
            {
                //��ͷ�Ƿ����ĸ
                mat = Regex.Match(str, @"^[^a-zA-Z+]");
                if (mat.Success)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// ͨ�õ� �ҵ��ļ����µ������ļ�
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fullnames"></param>
        /// <param name="names"></param>
        /// <param name="suffix"> ��׺ </param>
        /// <returns></returns>
        public static int FindAllFiles(string folderPath, out List<string> fullnames, out List<string> names, string suffix = null)
        {
            fullnames = new List<string>();  //�ļ�����·��
            names = new List<string>();      //�ļ�����׺����

            int count = 0;

            //�ж��Ƿ��д��ļ���
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo direction = new DirectoryInfo(folderPath);
                FileInfo[] files = string.IsNullOrEmpty(suffix) ? direction.GetFiles("*") : direction.GetFiles($"*{suffix}");
                for (int i = 0; i < files.Length; i++)
                {
                    //ȥ��Unity�ڲ�.meta�ļ�
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
        /// Debug��־
        /// </summary>
        /// <param name="msg"></param>
        public static void Log(string msg)
        {
            if (enableDebugLog)
                Debug.Log(string.Format($"<color=#D4A42D>[ProtoEditor Debuger]</color> {msg}"));
        }

        /// <summary>
        /// Debug������־
        /// </summary>
        /// <param name="msg"></param>
        public static void LogError(string msg)
        {
            if (enableDebugError)
                Debug.LogError(string.Format($"<color=#D4A42D>[ProtoEditor Debuger]</color> {msg}"));
        }

        /// <summary>
        /// �û���־��Ϣ
        /// </summary>
        /// <param name="msg"></param>
        public static void LogInfo(string msg)
        {
            Debug.Log(string.Format($"<color=#3380D2>[ProtoEditor][�û���Ϣ]</color> {msg}"));
        }

        /// <summary>
        /// �û�������־��Ϣ
        /// </summary>
        /// <param name="msg"></param>
        public static void LogErrorInfo(string msg)
        {
            Debug.LogError(string.Format($"<color=#D4A42D>[ProtoEditor][�û���Ϣ]</color> {msg}"));
        }

        /// <summary>
        /// ��ʾ֪ͨ
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
        /// ��� �Ѵ��ڵ�Proto�ļ��� Dropdown�б�
        /// </summary>
        /// <param name="folder"> ָ��Ѱ�ҵ��ļ��� </param>
        /// <param name="blacklist"> �������б��ų����б������ </param>
        /// <returns> ���� Dropdown�б�(����/����) </returns>
        public static ValueDropdownList<string> GetProtoFileNameValueDropdownList(string folder = null, List<string> blacklist = null)
        {
            //���Ի������ļ���
            string outPutFolder = "";
            {
                //(�ȳ��ԴӲ�����ȡ)
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

            //�ҵ������ļ�
            List<string> fileNames;
            List<string> filefullNames;
            FindAllFiles(outPutFolder, out filefullNames, out fileNames, ".proto");

            //����-�ų�������
            foreach (var name in blacklist)
            {
                int i = fileNames.IndexOf(name);
                if (i >= 0)
                {
                    fileNames.RemoveAt(i);
                    filefullNames.RemoveAt(i);
                }
            }

            //��� ValueDropdownList
            ValueDropdownList<string> dl = new Sirenix.OdinInspector.ValueDropdownList<string>();
            for (int i = 0; i < fileNames.Count; i++)
                dl.Add(fileNames[i], fileNames[i]);
            return dl;
        }

        /// <summary>
        /// ���Ի���ļ�����ļ���
        /// </summary>
        /// <returns></returns>
        public static string TryGetOutputPath()
        {
            //���Ի������ļ���
            string outPutFolder = "";
            //(���Դ�ProtoMenuEditorWindow���ڻ�ȡ)
            if (ProtoMenuEditorWindow.Instance != null)
                outPutFolder = ProtoMenuEditorWindow.Instance.CurSettingPanelDisplay.SETTING_ProtoOutputFolderPath;
            //��ʵ�ڲ�����ֱ�Ӷ�ȡ�ļ����أ�
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
        /// ������ѡ���MenuItem����Ϊ��
        /// </summary>
        public static void SetDirty()
        {
            SetDirtyLastSelected();
        }

        /// <summary>
        /// ������ѡ���MenuItem����Ϊ��
        /// </summary>
        static void SetDirtyLastSelected()
        {
            if (ProtoMenuEditorWindow.sLastSelecetedMenuItemData != null)
                ProtoMenuEditorWindow.sLastSelecetedMenuItemData.IsDrity = true;
        }

        #endregion


    }
}