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
    /// Proto�༭������ - �������
    /// </summary>
    public partial class ProtoMenuEditorWindow : OdinMenuEditorWindow
    {
        /// <summary>
        /// �������Ĵ�������
        /// </summary>
        private OdinEditorWindow _SettingPanelWindow;

        /// <summary>
        /// ������Ϣ
        /// </summary>
        public SettingPanelDisplay CurSettingPanelDisplay;



        #region ���-����

        /// <summary>
        /// �������de��ʾ����
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
                SirenixEditorGUI.IconMessageBox("ע�⣺������ڱ༭�ļ��벻Ҫ�޸�·�����ã����򽫶�ʧ���ж��ļ����޸ģ�", SdfIconType.ExclamationTriangleFill, Color.yellow);
            }


            [TitleGroup("      ��", "", TitleAlignments.Split, Indent = true, HorizontalLine = false), BoxGroup("      ��/�������", ShowLabel = true, CenterLabel = true), LabelText("�����ļ�"), PropertyOrder(-2), ReadOnly, DisplayAsString]
            public string SETTING_FileName = "__SETTING__.json";

            [PropertySpace(2), TitleGroup("      ��"), BoxGroup("      ��/�������"), LabelText("ǰ��·��"), PropertyOrder(-1), ReadOnly, DisplayAsString]
            public string SETTING_prePath = "";

            [PropertySpace(2), TitleGroup("      ��"), BoxGroup("      ��/�������"), PropertyOrder(0), OnValueChanged("SaveSetting"), DetailedInfoBox("��Tips", "�ٿ���ʱ���Զ�����������ļ����޸�\n�ڹر�ʱ����Ҫ�ֶ������ļ����޸�", InfoMessageType.None), LabelText("�Զ���������")]
            public bool SETTING_AutoSaveSetting = true;

            [PropertySpace(4), TitleGroup("      ��"), BoxGroup("      ��/�������"), PropertyOrder(3), FolderPath, OnValueChanged("SaveSettingButCheckAutoFun"), Required("������д������ܳ־û�����"), LabelText("�����ļ�����Ŀ¼"), DetailedInfoBox("��Tips", "���ߵ������ļ��������ڴ�Ŀ¼��", InfoMessageType.None), ValidateInput("CheckFolderValidityFun_1")]
            public string SETTING_FolderPath = "";

            [PropertySpace(6), TitleGroup("      ��"), BoxGroup("      ��/�������"), PropertyOrder(4), FolderPath, OnValueChanged("SaveSettingButCheckAutoFun"), ValidateInput("CheckFolderValidityFun_1"), DetailedInfoBox("��Tips", "���ߵĻ����ļ������浽��Ŀ¼���������д��������ʹ�ò��ֹ��ܣ�", InfoMessageType.None), Required("������д�����������ʹ��ȫ������"), LabelText("�����ļ�����Ŀ¼")]
            public string SETTING_CachePath = "";

            [PropertySpace(6, 4), TitleGroup("      ��"), BoxGroup("      ��/�������"), PropertyOrder(5), FolderPath, OnValueChanged("SaveSettingButCheckAutoFun"), LabelText("Proto�ļ����Ŀ¼"), DetailedInfoBox("��Tips", "����Proto�ļ�ʱ�������Ŀ¼��", InfoMessageType.None), ValidateInput("CheckFolderValidityFun_2")]
            public string SETTING_ProtoOutputFolderPath = "";

            [PropertySpace(6, 4), TitleGroup("      ��"), BoxGroup("      ��/�������"), PropertyOrder(5), FilePath(Extensions = "exe"), OnValueChanged("SaveSettingButCheckAutoFun"), LabelText("ProtoCת����Ŀ¼"), DetailedInfoBox("��Tips", "��Proto�ļ�תΪC#Э���ļ���ת����(protoc.exe)����Ŀ¼��", InfoMessageType.None)]
            public string SETTING_ExeFullName = "";

            [PropertySpace(6, 4), TitleGroup("      ��"), BoxGroup("      ��/�������"), PropertyOrder(5), FolderPath, OnValueChanged("SaveSettingButCheckAutoFun"), LabelText("C#Э����Ŀ¼"), DetailedInfoBox("��Tips", "��Proto�ļ�ת����C#Э���ļ��󽫴���ڴ�Ŀ¼��", InfoMessageType.None)]
            public string SETTING_CSOutputFolder = "";


            [PropertySpace(6, 4), TitleGroup("      ��"), BoxGroup("      ��/�������"), PropertyOrder(10), ShowInInspector, OnValueChanged("SaveSettingButCheckAutoFun"), LabelText("����-��ͨ��־����"), DetailedInfoBox("��Tips", "��Console��ʾ���ڿ������Ե���ͨ��־��", InfoMessageType.None)]
            public bool SETTING_EnableDebugLog { get { return ProtoHelper.enableDebugLog; } set { ProtoHelper.enableDebugLog = value; } }

            [PropertySpace(6, 4), TitleGroup("      ��"), BoxGroup("      ��/�������"), PropertyOrder(11), ShowInInspector, OnValueChanged("SaveSettingButCheckAutoFun"), LabelText("����-������־����"), DetailedInfoBox("��Tips", "��Console��ʾ���ڿ������ԵĴ�����־��", InfoMessageType.None)]
            public bool SETTING_EnableDebugError { get { return ProtoHelper.enableDebugError; } set { ProtoHelper.enableDebugError = value; } }


            [PropertySpace(6, 4), TitleGroup("      ��"), BoxGroup("      ��/�������"), PropertyOrder(20), DetailedInfoBox("��Tips", "��δ�����Զ��������õ�����£����˽����ֶ����棡", InfoMessageType.None), Button(SdfIconType.CheckCircleFill, "�ֶ�����", ButtonHeight = 25), GUIColor("#44DB7F"), HideIf("SETTING_AutoSaveSetting")]
            public void SETTING_ManualSave()
            {
                SaveSetting(true);
            }

            #region ˽�з��� & �ֶ�

            private bool isDirty = false;

            private Action _refreshMenuTreeAction;

            private Action<ProtoHelper.BasePanelDisplay, string> _showNotificationAction;

            private bool CheckFolderValidityFun_1(string value, ref string message, ref InfoMessageType? messageType)
            {
                return InternalCheckFolderValidityFun(value, ref message, ref messageType, "", InfoMessageType.None);
            }

            private bool CheckFolderValidityFun_2(string value, ref string message, ref InfoMessageType? messageType)
            {
                return InternalCheckFolderValidityFun(value, ref message, ref messageType, "����д���", InfoMessageType.Warning);
            }

            /// <summary>
            /// ����ļ���·����Ч��
            /// </summary>
            /// <param name="value"> �ļ���·�� </param>
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

                //����ļ��кϷ���
                if (!Directory.Exists(folderPath))
                {
                    messageType = InfoMessageType.Error;
                    message = $"Ŀ¼�����ڣ�����!";
                    return false;
                }

                return true;
            }


            /// <summary>
            /// �ֶδ����ı������÷������ж��Ƿ��Զ����棩
            /// </summary>
            private void SaveSettingButCheckAutoFun()
            {
                //���Զ����洦�ڿ�ʼ״̬�Żᱣ��
                if (SETTING_AutoSaveSetting)
                    SaveSetting(false);
            }

            /// <summary>
            /// ������Ⱦʱ����-�ж��Ƿ������������Ҫˢ��MenuTree
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
            /// ����SETTING
            /// </summary>
            public void SaveSetting(bool showNotify = false)
            {
                if (string.IsNullOrEmpty(SETTING_FolderPath))
                {
                    ProtoHelper.LogErrorInfo("[�����ļ�����Ŀ¼] δ����");
                    return;
                }

                string path = Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets")), SETTING_FolderPath).Replace('\\', '/');
                if (!Directory.Exists(path))
                {
                    ProtoHelper.LogErrorInfo($"[�����ļ�����Ŀ¼] ����·�� :{path}");
                    return;
                }

                string path2 = Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets")), SETTING_CachePath).Replace('\\', '/');
                if (!Directory.Exists(path2))
                {
                    ProtoHelper.LogErrorInfo($"[�����ļ�����Ŀ¼] ����·�� :{path2}");
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

                //д��
                string p = path + "/" + SETTING_FileName;
                if (File.Exists(p))
                    ProtoHelper.LogInfo("[Proto��������] ��ɸ��Ǳ���");
                else
                    ProtoHelper.LogInfo("[Proto��������] ��ɱ���");
                File.WriteAllText(p, json);

                if (showNotify)
                    _showNotificationAction?.Invoke(this, "���ñ���ɹ�");

                isDirty = true;
            }

            /// <summary>
            /// ��ʼ�Զ�Ѱ�ҵ��������ڵ�λ��
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
                        ProtoHelper.LogInfo($"���Զ��ҵ�{path}");

                        string settingFullname = Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets")), path).Replace('\\', '/');
                       return Path.GetDirectoryName(settingFullname);
                    }
                }
                return string.Empty;
            }

            /// <summary>
            /// ��ȡSETTING
            /// </summary>
            public void ReadSetting()
            {
                //��ʼ��ʱSETTING_FolderPath�ǿյģ������Զ�Ѱ���ļ�λ��
                if (string.IsNullOrEmpty(SETTING_FolderPath))
                    SETTING_FolderPath = AutoFindSetting();

                //����֤���Զ�Ѱ��Ҳû�ҵ���֤����δ������
                if (string.IsNullOrEmpty(SETTING_FolderPath))
                {
                    ProtoHelper.LogErrorInfo("[�����ļ�����Ŀ¼] δ����");
                    AutoFindSetting();
                    return;
                }

                string path = Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets")), SETTING_FolderPath).Replace('\\', '/');
                if (!Directory.Exists(path))
                {
                    ProtoHelper.LogErrorInfo($"[�����ļ�����Ŀ¼] ����·�� :{path}");
                    return;
                }

                string filePath = path + "/" + SETTING_FileName;
                if (!File.Exists(filePath))
                {
                    ProtoHelper.LogErrorInfo($"[�����ļ�] ������ :{path}");
                    return;
                }

                //��ȡ
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
                    ProtoHelper.LogErrorInfo($"[�����ļ�] ��ȡ�д���,����");
                else
                    ProtoHelper.LogInfo($"[�����ļ�] �ɹ���ȡ :{filePath}");
            }

        }

        #endregion



    }
}