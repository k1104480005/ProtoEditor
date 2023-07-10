using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditProto
{
    /// <summary>
    /// Proto�༭������ - �������ļ����
    /// </summary>
    public partial class ProtoMenuEditorWindow : OdinMenuEditorWindow
    {
        /// <summary>
        /// �������ļ����Ĵ�������
        /// </summary>
        private OdinEditorWindow _CreateNewFilePanelWindow;

        /// <summary>
        /// �������ļ���������
        /// </summary>
        public QuickCreateFilePanelDisplay CurQuickCreateFilePanelDisplay;


        #region ���-�������ļ�

        /// <summary>
        /// �����ļ����de��ʾ����
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

            #region ˽���ֶ�

            /// <summary>
            /// ָ����׺
            /// </summary>
            private string _fileSuffix = ".proto";

            /// <summary>
            /// ����ļ����·����Func
            /// </summary>
            private Func<string> _getOutputPathFunc;

            /// <summary>
            /// �����ļ����·���� Action
            /// </summary>
            private Action<string> _setOutputPathAction;

            /// <summary>
            /// �����رմ��� Action
            /// </summary>
            private Action _initiativeClosePanelAction;

            /// <summary>
            /// ������� Action
            /// </summary>
            private Action<string> _createFinishAction;

            /// <summary>
            /// ��������(0Ϊͨ��)(1Ϊδ��д)(2Ϊ�к�׺)(3Ϊ���·������)(4δ�ļ��ظ�)
            /// </summary>
            private int _errorKey = -1;


            #endregion

            /// <summary>
            /// �ļ���
            /// </summary>
            [TitleGroup("      ��", "", TitleAlignments.Split, Indent = true, HorizontalLine = false), BoxGroup("      ��/�������ļ����", ShowLabel = true, CenterLabel = true), PropertyOrder(0), LabelText("�ļ���", Icon = SdfIconType.StarFill, IconColor = "#CD60F3"), SuffixLabel("@this._fileSuffix"), ValidateInput("CheckNameValidityFun", "�ļ�����д������", InfoMessageType.Warning), GUIColor("#FFFFFF"), DelayedProperty]
            public string FileName;

            /// <summary>
            /// ȫ·��
            /// </summary>
            [TitleGroup("      ��"), BoxGroup("      ��/�������ļ����"), PropertyOrder(1), LabelText("Ԥ   ��", Icon = SdfIconType.Link), HideLabel, ReadOnly, GUIColor("#FFFFFF")]
            public string FileFullName;

            [TitleGroup("      ��"), BoxGroup("      ��/�������ļ����"), PropertyOrder(2), LabelText("ʹ�ÿ���ģ��")]
            public bool UseSnapShootTemplate = false;

            [TitleGroup("      ��"), BoxGroup("      ��/�������ļ����"), PropertyOrder(3), LabelText("ģ��", Icon = SdfIconType.Link), ShowIf("UseSnapShootTemplate"), AssetSelector(Filter = "snapshoot t:ScriptableObject", FlattenTreeView = true, DropdownWidth = 800), InfoBox("ʹ��ģ����ܻ���֡���Ϣ�塱�͡�ö�١��ظ������������������޸ġ���Ϣ�塱�͡�ö�١�������������ӡ������ռ䡱�����֣�", SdfIconType.ArrowDownCircle)]
            public SnapShootScriptableObject SnapShootTemplate;

            /// <summary>
            /// ������ť
            /// </summary>
            [PropertySpace(10, 4), TitleGroup("      ��"), BoxGroup("      ��/�������ļ����"), PropertyOrder(4), GUIColor("#6FD48C"), Button("����", ButtonHeight = 30, Icon = SdfIconType.CheckCircleFill), HideIf("@this._errorKey == 3"), DisableIf("@this._errorKey != 0")]
            public void CreateNowButton()
            {
                string initContent = "";

                //���ģ��
                if (UseSnapShootTemplate)
                {
                    if (SnapShootTemplate == null)
                    {
                        EditorUtility.DisplayDialog("��ʾ", $"δѡ��ģ���ļ���", "ȷ��");
                        GUIUtility.ExitGUI();
                        return;
                    }
                    if (SnapShootTemplate.wrapper == null)
                    {
                        EditorUtility.DisplayDialog("��ʾ", $"ѡ���ģ���ļ���Ч��", "ȷ��");
                        GUIUtility.ExitGUI();
                        return;
                    }

                    //����ģ������
                    initContent = ProtoHelper.EditorInfo2ProtoText(SnapShootTemplate.wrapper);
                }


                string message = "";
                InfoMessageType? infoType = InfoMessageType.None;
                bool pass = InternalCheckNameValidityFun(FileName, ref message, ref infoType, out _errorKey);

                if (pass)
                {
                    //�½��߼�
                    if (!File.Exists(FileFullName))
                    {
                        //�������ļ���д�������
                        File.WriteAllText(FileFullName, initContent);

                        _createFinishAction?.Invoke(FileName);
                    }
                    else
                        Debug.LogError("CreateNowButton FileFullName is Exists!");

                    _initiativeClosePanelAction?.Invoke();
                }
            }

            [PropertySpace(10, 4), TitleGroup("      ��"), BoxGroup("      ��/�������ļ����"), PropertyOrder(5), GUIColor("#6FB9D4"), Button("�����������Ŀ¼", ButtonHeight = 30, Icon = SdfIconType.Tools), ShowIf("@this._errorKey == 3")]
            public void SetOutputButton()
            {
                //������Ŀ¼��������,���û��������Ŀ¼
                if (_errorKey == 3)
                {
                    string projectPath = Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets"));
                    string selectedPath = EditorUtility.SaveFolderPanel("���Ŀ¼", projectPath, "");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        //����·��
                        string p = selectedPath.Replace(projectPath + "/", "");

                        //��������Ŀ¼�ص�
                        _setOutputPathAction?.Invoke(p);

                        //�ر����
                        _initiativeClosePanelAction?.Invoke();
                    }
                }
                else
                    Debug.LogError("SetOutput _errorKey error");
            }

            /// <summary>
            /// ������飺����ļ����ĺϷ���
            /// </summary>
            /// <param name="value"> �ļ����޺�׺ </param>
            /// <returns> �����Ƿ�ͨ�� </returns>
            private bool CheckNameValidityFun(string value, ref string message, ref InfoMessageType? messageType)
            {
                bool pass = InternalCheckNameValidityFun(value, ref message, ref messageType, out _errorKey);
                return pass;
            }

            /// <summary>
            /// ������飺����ļ����ĺϷ��� -�ڲ�����
            /// </summary>
            /// <param name="value"></param>
            /// <param name="message"></param>
            /// <param name="messageType"></param>
            /// <param name="errorKey"> ������ </param>
            /// <returns></returns>
            private bool InternalCheckNameValidityFun(string value, ref string message, ref InfoMessageType? messageType, out int errorKey)
            {
                errorKey = 0;

                //δ��д
                if (string.IsNullOrEmpty(value))
                {
                    messageType = InfoMessageType.None;
                    message = "";
                    _errorKey = 1;
                    FileFullName = "";//FullName��ֵ
                    return false;
                }

                //��д����׺
                if (value.LastIndexOf('.') > 0)
                {
                    messageType = InfoMessageType.Error;
                    message = "������д��׺�����飡";
                    errorKey = 2;
                    return false;
                }

                string outputPath = _getOutputPathFunc == null ? "" : _getOutputPathFunc.Invoke();
                string projectPath = Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets"));
                string folderPath = Path.Combine(projectPath, outputPath).Replace('\\', '/');
                /* �ϲ���Ԥ�����ļ�ȫ·�����������ļ���·�������Ǹ�·�����Ǵ���ģ� */
                string fullName = Path.Combine(folderPath, string.Format($"{value}{_fileSuffix}"));
                FileFullName = fullName;//FullName��ֵ

                //����ļ��кϷ���
                if (!Directory.Exists(folderPath))
                {
                    messageType = InfoMessageType.Error;
                    message = $"���Ŀ¼�������飺{folderPath}";
                    errorKey = 3;
                    return false;
                }

                //����ļ��ظ�
                if (File.Exists(fullName))
                {
                    messageType = InfoMessageType.Error;
                    message = $"�Ѵ���ͬ���ļ�{value}�������������";
                    errorKey = 4;
                    return false;
                }

                return true;
            }

        }

        #endregion

        #region ����

        /// <summary>
        /// ����ļ����·��
        /// </summary>
        /// <returns></returns>
        private string GetFileOutputPathCallback()
        {
            return CurSettingPanelDisplay.SETTING_ProtoOutputFolderPath;
        }

        /// <summary>
        /// �����ļ����·��
        /// </summary>
        /// <param name="folderPath"></param>
        private void SetFileOutputPathCallback(string folderPath)
        {
            CurSettingPanelDisplay.SETTING_ProtoOutputFolderPath = folderPath;
            CurSettingPanelDisplay.SaveSetting();
        }

        /// <summary>
        /// �رմ������ļ������ص�
        /// </summary>
        private void CloseFileCreatePanelCallback()
        {
            _CreateNewFilePanelWindow?.Close();
        }

        /// <summary>
        /// ����������ļ��ص�
        /// </summary>
        private void CreateFileFinishCallback(string fileName)
        {
            RefreshAllProtoFiles(false);
        }

        #endregion
    }
}