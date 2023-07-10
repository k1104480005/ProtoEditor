
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ����ṹ�İ�װ�壨��Proto Editor-Structure�ж���Ľṹ����һ�£�
///     - ��װ����Ҫ��Ϊ���Զ�����ʾ
///     - *�������Կ������ԭ���ݺ�Wrapper�Ƿ�ƥ��
/// </summary>
namespace EditProto
{
    /// <summary>
    /// ProtoEditorInfo Wrapper
    /// </summary>
    [Serializable]
    public class ProtoEditorInfoWrapper
    {
        /// <summary>
        /// ����ԭ����
        /// </summary>
        private ProtoEditorInfo _info;
        public ProtoEditorInfo Info { get { return this._info; } }

        public ProtoEditorInfoWrapper(ref ProtoEditorInfo info)
        {
            this._info = info;

            //�б��װ
            this.MessageInfoList = info.MessageInfoList.Select(i => new MessageInfoWrapper(ref i)).ToList();
            this.EnumInfoList = info.EnumInfoList.Select(i => new EnumInfoWrapper(ref i)).ToList();
        }

        [HorizontalGroup("CON/HGP", Gap = 10), BoxGroup("CON", ShowLabel = false), ShowInInspector, PropertyOrder(0), LabelText("�﷨Э��"), LabelWidth(60), InfoBox("syntax ��������﷨Э�����,����֧��proto3", SdfIconType.ArrowDownCircle, "@ProtoHelper.ShowEditorInfoTipToggle", IconColor = "#DCE741"), DisableContextMenu]
        public YntaxType Yntax
        { get { return this._info.Yntax; } set { this._info.Yntax = value; } }

        [HorizontalGroup("CON/HGP"), BoxGroup("CON"), ShowInInspector, PropertyOrder(1), LabelText("�����ռ�"), LabelWidth(60), InfoBox("package �����ռ���Բ�����Ǳ�Ҫ��", SdfIconType.ArrowDownCircle, "@ProtoHelper.ShowEditorInfoTipToggle", IconColor = "#DCE741"), OnValueChanged("@ProtoHelper.SetDirty()", IncludeChildren = true), DisableContextMenu, InfoBox("$_selfErrorTip", InfoMessageType.Error, VisibleIf = "@_selfErrorTip!=\"\"")]
        public string PackageName
        { get { return this._info.PackageName; } set { this._info.PackageName = value; } }


        [PropertySpace(6), BoxGroup("CON"), TabGroup("CON/TAB", "�����ļ�", SdfIconType.FileEarmarkTextFill, true, TextColor = "#CB967F"), ShowInInspector, PropertyOrder(2), LabelText("���������ļ�"), InfoBox("import \n��1����������proto�ļ�,��д���·��\n��2��ֻ���������ã����ܽ��͡�../����..\\·����\n��3����ֹproto�ļ��ֿ����\n��4����Ҫ��д��׺.proto", SdfIconType.ArrowDownCircle, "@ProtoHelper.ShowEditorInfoTipToggle", IconColor = "#DCE741"), ListDrawerSettings(ShowFoldout = false, ElementColor = "OnImportElementColor"), ValueDropdown("@this.GetFilteredImprotFileList()", DropdownTitle = "��ѡ�������ļ�", ExcludeExistingValuesInList = true, IsUniqueList = true), OnValueChanged("@ProtoHelper.SetDirty()", IncludeChildren = true), InfoBox("$_importErrorTip", InfoMessageType.Error, VisibleIf = "@_importErrorTip!=\"\""), DisableContextMenu(true, true), InlineButton("CheckImportFile", SdfIconType.BugFill, ""), OnCollectionChanged("OnImportInfoListChangedAfter")]
        public List<string> ImportProtoFileName
        { get { return this._info.ImportProtoFileName; } set { this._info.ImportProtoFileName = value; } }


        [PropertySpace(6), BoxGroup("CON"), TabGroup("CON/TAB", "ö��", SdfIconType.Box, TextColor = "#ADC4A0"), ShowInInspector, PropertyOrder(3), LabelText("����ö��"), InfoBox("enum ö��ֵ���ֶα��ֱ�ӱ�ʾ��ʵ��ֵ", SdfIconType.ArrowDownCircle, "@ProtoHelper.ShowEditorInfoTipToggle", IconColor = "#DCE741"), OnValueChanged("@ProtoHelper.SetDirty()", IncludeChildren = true), TableList(ShowIndexLabels = true, AlwaysExpanded = true, DrawScrollView = false, CellPadding = 6), DisableContextMenu(true, true), InlineButton("CheckEnumList", SdfIconType.BugFill, ""), InfoBox("$_enumErrorTip", InfoMessageType.Error, VisibleIf = "@_enumErrorTip!=\"\""), OnCollectionChanged("OnEnumInfoListChangedBefore", "OnEnumInfoListChangedAfter")]
        public List<EnumInfoWrapper> EnumInfoList = new List<EnumInfoWrapper>();


        [PropertySpace(6), BoxGroup("CON"), TabGroup("CON/TAB", "��Ϣ��", SdfIconType.Box, TextColor = "#45A995"), ShowInInspector, PropertyOrder(4), LabelText("������Ϣ��"), InfoBox("message\n���ֶ����Ρ���proto3������֧��repeated�ֶ����Σ����ʹ��required��optional����ᱨ��\n�������ռ䡿�����ⲿ�����������ռ��һ��Ҫ�������ռ���������\n����Ź��򡿱�ű���������������Ȼ�ᱨ��\n����Ź��򡿱��˳��Ǳ�����������Ϊ���ɵĴ�������switch����", SdfIconType.ArrowDownCircle, "@ProtoHelper.ShowEditorInfoTipToggle", IconColor = "#DCE741"), OnValueChanged("@ProtoHelper.SetDirty()", IncludeChildren = true), TableList(AlwaysExpanded = true, DrawScrollView = false, CellPadding = 6), DisableContextMenu(true, true), InlineButton("CheckMessageList", SdfIconType.BugFill, ""), InfoBox("$_messageErrorTip", InfoMessageType.Error, VisibleIf = "@_messageErrorTip!=\"\""), OnCollectionChanged("OnMessageInfoListChangedBefore", "OnMessageInfoListChangedAfter")]
        public List<MessageInfoWrapper> MessageInfoList = new List<MessageInfoWrapper>();


        #region �ڲ��ֶ�

        /*���Import�Ĵ�����Ϣ*/
        private string _importErrorTip = "";

        /*Import�Ĵ�����Ϣ���б�����*/
        private List<int> _importErrorIndex = new List<int>();

        /*��� enum �Ĵ�����Ϣ*/
        private string _enumErrorTip = "";

        /*enum �Ĵ�����Ϣ���б�����*/
        private List<int> _enumErrorIndex = new List<int>();

        /*��� message �Ĵ�����Ϣ*/
        private string _messageErrorTip = "";

        /*message �Ĵ�����Ϣ���б�����*/
        private List<int> _messageErrorIndex = new List<int>();

        /*��� self �Ĵ�����Ϣ*/
        private string _selfErrorTip = "";


        //������ʾ��Ϣ��
        [PropertyOrder(int.MinValue), OnInspectorGUI, ShowIf("@ProtoHelper.ShowEditorInfoTipToggle")]
        private void DrawIntroInfoBox()
        {
            SirenixEditorGUI.IconMessageBox("�� ����ͼ��İ�ť�Ǽ����д����ť��������û��Ӧ�ʹ���û�д��󣬳��ִ��󲢽�����ٵ��һ�μ������������Ϣ", SdfIconType.BugFill);
        }
        #endregion

        #region �ڲ�����

        /// <summary>
        /// EnumInfoList ö���б����仯ʱ Before
        /// </summary>
        void OnEnumInfoListChangedBefore(CollectionChangeInfo info, object value)
        {
            //ɾ��֮ǰ�Ѷ�Ӧ����������ɾ��
            if (info.ChangeType == CollectionChangeType.RemoveIndex)
            {
                List<EnumInfoWrapper> list = value as List<EnumInfoWrapper>;
                EnumInfoWrapper deleteItem = list[info.Index];
                _info.EnumInfoList.RemoveAt(_info.EnumInfoList.IndexOf(deleteItem.Info));
            }
        }

        /// <summary>
        /// EnumInfoList ö���б����仯ʱ After
        /// </summary>
        void OnEnumInfoListChangedAfter(CollectionChangeInfo info, object value)
        {
            //����֮��ͬʱ�������õ����ݣ������¸�ֵ
            if (info.ChangeType == CollectionChangeType.Add)
            {
                EnumInfoWrapper changedItem = info.Value as EnumInfoWrapper;
                EnumInfo newEnumInfo = new EnumInfo("");
                _info.EnumInfoList.Add(newEnumInfo);
                changedItem.Info = newEnumInfo;
            }

        }


        /// <summary>
        /// MessageInfoList �б����仯ʱ Before
        /// </summary>
        void OnMessageInfoListChangedBefore(CollectionChangeInfo info, object value)
        {
            //ɾ��ʱ�Ѷ�Ӧ����������ɾ��
            if (info.ChangeType == CollectionChangeType.RemoveIndex)
            {
                List<MessageInfoWrapper> list = value as List<MessageInfoWrapper>;
                MessageInfoWrapper deleteItem = list[info.Index];
                _info.MessageInfoList.RemoveAt(_info.MessageInfoList.IndexOf(deleteItem.Info));
            }
        }

        /// <summary>
        /// MessageInfoList �б����仯ʱ After
        /// </summary>
        void OnMessageInfoListChangedAfter(CollectionChangeInfo info, object value)
        {
            //����֮��ͬʱ�������õ����ݣ������¸�ֵ
            if (info.ChangeType == CollectionChangeType.Add)
            {
                MessageInfoWrapper changedItem = info.Value as MessageInfoWrapper;
                MessageInfo newData = new MessageInfo();
                _info.MessageInfoList.Add(newData);
                changedItem.Info = newData;
            }

        }


        /// <summary>
        /// Import �б����仯ʱ After
        /// </summary>
        void OnImportInfoListChangedAfter(CollectionChangeInfo info, object value)
        {
            //..s
        }


        /// <summary>
        /// ��ù��˺�����п����õ��ļ������б�
        /// </summary>
        /// <returns></returns>
        IEnumerable GetFilteredImprotFileList()
        {
            //���Թ���
            if (ProtoMenuEditorWindow.sLastSelecetedMenuItemData != null)
                return EditProto.ProtoHelper.GetProtoFileNameValueDropdownList(null, new List<string>() { ProtoMenuEditorWindow.sLastSelecetedMenuItemData.FileName });
            //����ʧ��
            else
            {
                ProtoHelper.LogError("GetFilteredImprotFileList ����ʧ��");
                return EditProto.ProtoHelper.GetProtoFileNameValueDropdownList(null);
            }
        }

        /// <summary>
        /// [�����д����] - Import
        /// </summary>
        public bool CheckImportFile()
        {
            _importErrorIndex.Clear();
            _importErrorTip = "";

            StringBuilder sb = new StringBuilder();
            int num = 0, index = 0; string str;
            foreach (var name in ImportProtoFileName)
            {
                string folder = ProtoHelper.TryGetOutputPath();
                string fullname = Path.Combine(folder, name).Replace('\\', '/');

                if (!File.Exists(fullname))
                {
                    num++;
                    str = num != 1 ? "\n" : "";
                    sb.Append($"{str}�����ļ�������:��{num}��{fullname}");
                    _importErrorIndex.Add(index);
                }
                index++;
            }
            _importErrorTip = sb.ToString();
            return string.IsNullOrEmpty(_importErrorTip);
        }

        /// <summary>
        /// ����Import�б��������ɫ
        /// </summary>
        /// <param name="index"></param>
        /// <param name="defaultColor"></param>
        /// <returns></returns>
        Color OnImportElementColor(int index, Color defaultColor)
        {
            if (_importErrorIndex.Contains(index))
                return ProtoHelper.color_Error1;
            else
                return defaultColor;
        }

        /// <summary>
        /// [�����д����] - Enum
        /// </summary>
        public bool CheckEnumList()
        {
            _enumErrorIndex.Clear();
            _enumErrorTip = "";

            StringBuilder sb = new StringBuilder();
            int num = 0, index = 0; string str;

            foreach (var info in EnumInfoList)
            {
                string errorStr;
                if (!info.CheckValueList(out errorStr))
                {
                    num++;
                    str = num != 1 ? "\n" : "";
                    sb.Append($"{str}����������[{index}]��({info.TypeName})�з��ִ���: {errorStr}");
                    _enumErrorIndex.Add(index);
                }
                index++;
            }

            _enumErrorTip = sb.ToString();
            return string.IsNullOrEmpty(_enumErrorTip);
        }

        /// <summary>
        /// [�����д����] - Message
        /// </summary>
        public bool CheckMessageList()
        {
            _messageErrorTip = "";

            StringBuilder sb = new StringBuilder();
            int num = 0, index = 0; string str;

            foreach (var info in MessageInfoList)
            {
                string errorStr;
                if (!info.CheckInput(out errorStr))
                {
                    num++;
                    str = num != 1 ? "\n" : "";
                    sb.Append($"{str}��������Ϣ��({info.Name})�з��ִ���: {errorStr}");
                }
                index++;
            }

            _messageErrorTip = sb.ToString();
            return string.IsNullOrEmpty(_messageErrorTip);
        }


        /// <summary>
        /// �������
        /// </summary>
        /// <returns></returns>
        public bool CheckSelf()
        {
            if (ProtoHelper.IsBadName(PackageName))
            {
                _selfErrorTip = "�����ռ� ���������(��ĸ)(����)(�»���)������ַ�";
                return false;
            }

            return true;
        }

        #endregion

    }

    /// <summary>
    /// MessageInfo Wrapper
    /// </summary>
    [Serializable]
    public class MessageInfoWrapper
    {
        /// <summary>
        /// ����ԭ����
        /// </summary>
        private MessageInfo _info;
        public MessageInfo Info { get { return this._info; } set { this._info = value; } }

        public MessageInfoWrapper()
        {
            this._info = new MessageInfo();
        }

        public MessageInfoWrapper(ref MessageInfo info)
        {
            this._info = info;

            //�б��װ
            this.FieldsList = info.fieldsList.Select(i => new FieldsInfoWrapper(ref i)).ToList();
            this.EnumInfoList = info.enumInfoList.Select(i => new EnumInfoWrapper(ref i)).ToList();

            RefreshInternalEnumInfoButton();
        }

        [PropertyOrder(0), TableColumnWidth(150), ShowInInspector, VerticalGroup("��Ϣ������"), LabelText(""), LabelWidth(30), InlineButton("OnRandomMessageName", SdfIconType.Dice5Fill, Label = "", ShowIf = "@Name==\"\""), InlineButton("OnDisplayAnnotation", SdfIconType.ChatLeftTextFill, Label = ""), DelayedProperty, DisableContextMenu, Required("���������")]
        public string Name
        { get { return this._info.name; } set { this._info.name = value; } }

        [PropertyOrder(1), TableColumnWidth(150), ShowInInspector, VerticalGroup("��Ϣ������"), LabelText("ע��"), LabelWidth(30), DisableContextMenu, MultiLineProperty(5), ShowIf("_showAnnotation")]
        public string Annotation
        { get { return this._info.annotation; } set { this._info.annotation = value; } }


        [PropertyOrder(2), TableColumnWidth(400), ShowInInspector, HideLabel, VerticalGroup("�ֶ��б�"), DisableContextMenu(DisableForCollectionElements = true), LabelText("���չ��"), TableList(ShowIndexLabels = false, AlwaysExpanded = false, DrawScrollView = false, CellPadding = 4), OnCollectionChanged("OnFieldsListChangedBefore", "OnFieldsListChangedAfter")]
        public List<FieldsInfoWrapper> FieldsList = new List<FieldsInfoWrapper>();

        [HideInInspector, OnCollectionChanged("OnEnumInfoListChangedBeforeInternal", "OnEnumInfoListChangedAfterInternal")]
        public List<EnumInfoWrapper> EnumInfoList = new List<EnumInfoWrapper>();


        [PropertyOrder(-2), TableColumnWidth(100, false), ShowInInspector, VerticalGroup("�ڲ�ö��"), HorizontalGroup("�ڲ�ö��/��ť��"), Button(SdfIconType.ArrowRepeat, "")]
        private void RefreshInternalEnumInfoButton()
        {
            InternalEnumInfoNameList.Clear();
            foreach (var i in EnumInfoList)
                InternalEnumInfoNameList.Add(i.TypeName);
        }

        [PropertyOrder(-2), TableColumnWidth(100, false), ShowInInspector, VerticalGroup("�ڲ�ö��"), HorizontalGroup("�ڲ�ö��/��ť��"), Button(SdfIconType.PencilFill, "")]
        private void EditInternalEnumInfoButton()
        {
            //ǰ���༭�ڲ�ö��
            var window = OdinEditorWindow.InspectObject(new InternalEnumDisplayInfo(ref _info) { InternalEnumInfoList = this.EnumInfoList });
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(600, 400);
            window.OnClose += () =>
            {
                //�ر�ʱˢ��һ���ڲ�ö����ʾ
                RefreshInternalEnumInfoButton();
            };
            window.OnBeginGUI += () =>
            {
                //����ƶ�ʱ�����ж��
                if (window != null && EditorWindow.focusedWindow != window)
                {
                    window.Close();
                    window = null;
                    GUIUtility.ExitGUI(); //�����������һ��,������ʧ
                }
            };
        }

        //����ֻԤ���ڲ�ö�ٵ�����
        [PropertyOrder(-1), TableColumnWidth(100, false), ShowInInspector, VerticalGroup("�ڲ�ö��"), ReadOnly, ListDrawerSettings(ShowItemCount = false), LabelText("ö���б�"), DisableContextMenu(DisableForCollectionElements = true)]
        public List<string> InternalEnumInfoNameList = new List<string>();


        #region �ڲ��ֶ�

        /// <summary>
        /// ��ʾע���ֶ�
        /// </summary>
        private bool _showAnnotation = false;

        #endregion


        #region �ڲ�����

        /// <summary>
        /// EnumInfoList ö���б����仯ʱ Before  
        /// </summary>
        void OnEnumInfoListChangedBeforeInternal(CollectionChangeInfo info, object value)
        {
            //ɾ��֮ǰ�Ѷ�Ӧ����������ɾ��
            if (info.ChangeType == CollectionChangeType.RemoveIndex)
            {
                List<EnumInfoWrapper> list = value as List<EnumInfoWrapper>;
                EnumInfoWrapper deleteItem = list[info.Index];
                _info.enumInfoList.RemoveAt(_info.enumInfoList.IndexOf(deleteItem.Info));
            }
        }

        /// <summary>
        /// EnumInfoList ö���б����仯ʱ After   
        /// </summary>
        void OnEnumInfoListChangedAfterInternal(CollectionChangeInfo info, object value)
        {
            //����֮��ͬʱ�������õ����ݣ������¸�ֵ
            if (info.ChangeType == CollectionChangeType.Add)
            {
                EnumInfoWrapper changedItem = info.Value as EnumInfoWrapper;
                EnumInfo newEnumInfo = new EnumInfo("");
                _info.enumInfoList.Add(newEnumInfo);
                changedItem.Info = newEnumInfo;
            }

            ProtoHelper.SetDirty();
        }


        /// <summary>
        /// FieldsList ö���б����仯ʱ Before
        /// </summary>
        void OnFieldsListChangedBefore(CollectionChangeInfo info, object value)
        {
            //ɾ��֮ǰ�Ѷ�Ӧ����������ɾ��
            if (info.ChangeType == CollectionChangeType.RemoveIndex)
            {
                List<FieldsInfoWrapper> list = value as List<FieldsInfoWrapper>;
                FieldsInfoWrapper deleteItem = list[info.Index];
                _info.fieldsList.RemoveAt(_info.fieldsList.IndexOf(deleteItem.Info));
            }
        }

        /// <summary>
        /// FieldsList ö���б����仯ʱ After
        /// </summary>
        void OnFieldsListChangedAfter(CollectionChangeInfo info, object value)
        {
            //����֮��ͬʱ�������õ����ݣ������¸�ֵ
            if (info.ChangeType == CollectionChangeType.Add)
            {
                FieldsInfoWrapper changedItem = info.Value as FieldsInfoWrapper;
                FieldsInfo newEnumInfo = new FieldsInfo(0);
                _info.fieldsList.Add(newEnumInfo);
                changedItem.Info = newEnumInfo;
            }
        }

        /// <summary>
        /// ���һ����Ϣ������
        /// </summary>
        void OnRandomMessageName()
        {
            Name = string.Format($"Message{UnityEngine.Random.Range(10, 99)}");
        }

        /// <summary>
        /// ����ע����ʾ
        /// </summary>
        void OnDisplayAnnotation()
        {
            _showAnnotation = !_showAnnotation;
        }

        /// <summary>
        /// �����ļ����д
        /// </summary>
        /// <returns> �Ƿ�ͨ�� </returns>
        public bool CheckInput(out string errorLocation)
        {
            errorLocation = "";

            if (string.IsNullOrEmpty(Name))
            {
                errorLocation = "��Ϣ����Ϊ��";
                return false;
            }

            if (ProtoHelper.IsBadName(Name))
            {
                errorLocation = "��Ϣ���� ���������(��ĸ)(����)(�»���)������ַ�";
                return false;
            }

            //�ֶμ��
            List<uint> tempCodeList = new List<uint>();
            for (int i = 0; i < FieldsList.Count; i++)
            {
                var item = FieldsList[i];
                string vError = "";
                //�������
                if (!item.CheckInput(out vError))
                {
                    errorLocation = $"�ֶ�({item.Name})�� - {vError}";
                    return false;
                }

                //�ֶα�ű����1��ʼ�ķǸ���
                if(item.Code <= 0)
                {
                    errorLocation = $"�ֶ�({item.Name})�� �ֶα��{item.Code} �������0";
                    return false;
                }

                //Code�Ƿ����ظ�
                if (tempCodeList.Contains(item.Code))
                {
                    errorLocation = $"�ֶ�({item.Name})�� �ֶα��{item.Code}�ظ�";
                    return false;
                }
                else
                    tempCodeList.Add(item.Code);
            }

            //�ڲ�ö��ֵ���
            {
                StringBuilder sb = new StringBuilder();
                int num = 0, index = 0; string str;

                foreach (var info in EnumInfoList)
                {
                    string errorStr;
                    if (!info.CheckValueList(out errorStr))
                    {
                        num++;
                        str = num != 1 ? "\n" : "";
                        sb.Append($"{str}�������ڲ�ö��������[{index}]��({info.TypeName})���ִ���: {errorStr}");
                    }
                    index++;
                }

                errorLocation = sb.ToString();
                if (!string.IsNullOrEmpty(errorLocation))
                    return false;
            }


            return true;
        }

        #endregion

    }

    /// <summary>
    /// FieldsInfo Wrapper
    /// </summary>
    [Serializable]
    public class FieldsInfoWrapper
    {
        /// <summary>
        /// ����ԭ����
        /// </summary>
        private FieldsInfo _info;
        public FieldsInfo Info { get { return this._info; } set { this._info = value; } }

        public FieldsInfoWrapper()
        {
            this._info = new FieldsInfo(0);
        }
        public FieldsInfoWrapper(ref FieldsInfo info)
        {
            this._info = info;
        }


        [PropertyOrder(1), TableColumnWidth(85, false), ShowInInspector, VerticalGroup("���η�"), HideLabel, DisableContextMenu(DisableForCollectionElements = true)]
        public ModifierType Modifier
        { get { return this._info.modifier; } set { this._info.modifier = value; } }

        [PropertyOrder(2), TableColumnWidth(50), ShowInInspector, VerticalGroup("����"), HideLabel, DisableContextMenu]
        public FieldsType Type
        { get { return this._info.type; } set { this._info.type = value; } }

        [PropertyOrder(3), TableColumnWidth(50), ShowInInspector, VerticalGroup("����"), HideLabel, DisableContextMenu, ShowIf("@Type == FieldsType.Custom"), DelayedProperty, ValueDropdown("GetRefMsgOrEnum", AppendNextDrawer = true, DisableGUIInAppendedDrawer = false)]
        public string CustomTypeName
        { get { return this._info.customTypeName; } set { this._info.customTypeName = value; } }


        [PropertyOrder(6), TableColumnWidth(50), ShowInInspector, VerticalGroup("����"), HideLabel, DisableContextMenu, DelayedProperty, Required("����д����"), InlineButton("OnRandomFieldName", SdfIconType.Dice5Fill, Label = "", ShowIf = "@Name==\"\""), InlineButton("OnDisplayAnnotation", SdfIconType.ChatLeftTextFill, Label = "")]
        public string Name
        { get { return this._info.name; } set { this._info.name = value; } }

        [PropertyOrder(7), TableColumnWidth(50), ShowInInspector, VerticalGroup("����"), LabelText("ע��"), LabelWidth(30), DisableContextMenu, ShowIf("_showAnnotation"), MultiLineProperty(5)]
        public string Annotation
        { get { return this._info.annotation; } set { this._info.annotation = value; } }

        [PropertyOrder(10), TableColumnWidth(35, false), ShowInInspector, VerticalGroup("���"), HideLabel, DisableContextMenu, MinValue(0)]
        public uint Code
        { get { return this._info.code; } set { this._info.code = value; } }


        #region �ڲ��ֶ�

        /// <summary>
        /// ��ʾע���ֶ�
        /// </summary>
        private bool _showAnnotation = false;

        #endregion


        #region �ڲ�����

        /// <summary>
        /// ������п������õ���Ϣ���ö��
        /// </summary>
        /// <returns></returns>
        IEnumerable GetRefMsgOrEnum()
        {
            if (Type != FieldsType.Custom)
                return null;

            if (ProtoMenuEditorWindow.sLastSelecetedMenuItemData == null)
            {
                Debug.LogError("GetRefMsgOrEnum sLastSelecetedMenuItemData is null");
                return null;
            }

            ValueDropdownList<string> outList = new Sirenix.OdinInspector.ValueDropdownList<string>();
            List<RefCollectionData> totalDataList = new List<RefCollectionData>();

            //����Լ��ļ�������
            ProtoMenuEditorWindow.UpdateRefCollectionData(ProtoMenuEditorWindow.sLastSelecetedMenuItemData.FileName, true);//ǿ�Ƹ����Լ���
            RefCollectionData refData = ProtoMenuEditorWindow.GetRefCollectionData(ProtoMenuEditorWindow.sLastSelecetedMenuItemData.FileName);
            if (refData != null)
                totalDataList.Add(refData);

            //�������������ļ�������
            List<string> fileNames = ProtoMenuEditorWindow.sLastSelecetedMenuItemData.EditorInfo.ImportProtoFileName;
            foreach (var name in fileNames)
            {
                ProtoMenuEditorWindow.UpdateRefCollectionData(name, false);//��ǿ�Ƹ���
                refData = ProtoMenuEditorWindow.GetRefCollectionData(name);
                if (refData != null)
                    totalDataList.Add(refData);
            }

            if (totalDataList.Count == 0)
                return outList;

            RefCollectionData all = totalDataList.CombineData();

            foreach (var i in all.enumRefString)
            {
                outList.Add($"ö��/{i.Replace('.', '/')}", i);
            }
            foreach (var i in all.messageRefString)
            {
                outList.Add($"��Ϣ��/{i.Replace('.', '/')}", i);
            }

            return outList;
        }

        /// <summary>
        /// ���һ���ֶ�����
        /// </summary>
        void OnRandomFieldName()
        {
            Name = string.Format($"Field{UnityEngine.Random.Range(10, 99)}");
        }

        /// <summary>
        /// ����ע����ʾ
        /// </summary>
        void OnDisplayAnnotation()
        {
            _showAnnotation = !_showAnnotation;
        }

        /// <summary>
        /// �����д
        /// </summary>
        /// <param name="errorLocation"></param>
        /// <returns></returns>
        public bool CheckInput(out string errorLocation)
        {
            errorLocation = "";

            if (string.IsNullOrEmpty(Name))
            {
                errorLocation = "�ֶ���Ϊ��";
                return false;
            }

            if (ProtoHelper.IsBadName(Name))
            {
                errorLocation = "�ֶ��� ���������(��ĸ)(����)(�»���)������ַ�";
                return false;
            }


            if (Type == FieldsType.Custom)
            {
                if (string.IsNullOrEmpty(CustomTypeName))
                {
                    errorLocation = "�Զ���������Ϊ��";
                    return false;
                }

                if (ProtoHelper.IsBadName(CustomTypeName))
                {
                    errorLocation = "�Զ��������� ���������(��ĸ)(����)(�»���)������ַ�";
                    return false;
                }

                //���ݵ�ǰ�������ݼ���Ƿ������Ϣ��ö�٣���Ϊ���¶�ȡ�����ļ�����̫�أ�����ֻ����ȼ��
                bool found = false;
                List<RefCollectionData> checkRefList = ProtoMenuEditorWindow.s_RefCollectionDataCacheDic.Values.ToArray().ToList();
                RefCollectionData checkData = checkRefList.CombineData();
                found = checkData.enumRefString.Find(e => e == CustomTypeName) != null;
                if (!found)
                    found = checkData.messageRefString.Find(e => e == CustomTypeName) != null;
                if (!found)
                {
                    errorLocation = $"û�ҵ����õ�����Ϊ{CustomTypeName}����Ϣ���ö�٣��Ƿ�û��������ļ���";
                    return false;
                }
            }

            if (Code < 0)
            {
                errorLocation = "�ֶα��Ϊ����";
                return false;
            }

            return true;
        }

        #endregion

    }


    /// <summary>
    /// EnumInfo Wrapper
    /// </summary>
    [Serializable]
    public class EnumInfoWrapper
    {
        /// <summary>
        /// ����ԭ����
        /// </summary>
        private EnumInfo _info;
        public EnumInfo Info { get { return this._info; } set { this._info = value; } }

        public EnumInfoWrapper()
        {
            this._info = new EnumInfo("");//��new����������ʱ�ģ�������������,�ⲿ�ḳ��������
        }

        public EnumInfoWrapper(ref EnumInfo info)
        {
            this._info = info;

            //�б��װ
            this.ValueList = info.valueList.Select(i => new EnumValueInfoWrapper(ref i)).ToList();
        }

        [PropertyOrder(0), TableColumnWidth(200, false), ShowInInspector, VerticalGroup("ö������"), LabelText("����"), LabelWidth(30), InlineButton("OnRandomTypeName", SdfIconType.Dice5Fill, Label = "", ShowIf = "@TypeName==\"\""), InlineButton("OnDisplayAnnotation", SdfIconType.ChatLeftTextFill, Label = ""), DelayedProperty, Required("���������")]
        public string TypeName
        { get { return this._info.typeName; } set { this._info.typeName = value; } }


        [PropertyOrder(1), TableColumnWidth(100), ShowInInspector, VerticalGroup("ö������"), LabelText("ע��"), LabelWidth(30), MultiLineProperty(5), ShowIf("_showAnnotation")]
        public string Annotation
        { get { return this._info.annotation; } set { this._info.annotation = value; } }

        [PropertyOrder(2), ShowInInspector, VerticalGroup("ֵ�б�"), LabelText("���չ��"), TableList(ShowIndexLabels = true, AlwaysExpanded = false, DrawScrollView = false, CellPadding = 4), OnCollectionChanged("OnValueListChangedBefore", "OnValueListChangedAfter")]
        public List<EnumValueInfoWrapper> ValueList = new List<EnumValueInfoWrapper>();


        #region �ڲ��ֶ�

        /// <summary>
        /// ��ʾע���ֶ�
        /// </summary>
        private bool _showAnnotation = false;

        #endregion

        #region �ڲ�����

        /// <summary>
        /// ValueList ö���б����仯ʱ Before
        /// </summary>
        void OnValueListChangedBefore(CollectionChangeInfo info, object value)
        {
            //ɾ��֮ǰ�Ѷ�Ӧ����������ɾ��
            if (info.ChangeType == CollectionChangeType.RemoveIndex)
            {
                List<EnumValueInfoWrapper> list = value as List<EnumValueInfoWrapper>;
                EnumValueInfoWrapper deleteItem = list[info.Index];
                _info.valueList.RemoveAt(_info.valueList.IndexOf(deleteItem.Info));
            }
        }

        /// <summary>
        /// ValueList ö���б����仯ʱ After
        /// </summary>
        void OnValueListChangedAfter(CollectionChangeInfo info, object value)
        {
            //����֮��ͬʱ�������õ����ݣ������¸�ֵ
            if (info.ChangeType == CollectionChangeType.Add)
            {
                EnumValueInfoWrapper changedItem = info.Value as EnumValueInfoWrapper;
                EnumValueInfo newinfo = new EnumValueInfo("");
                _info.valueList.Add(newinfo);
                changedItem.Info = newinfo;
            }
        }

        /// <summary>
        /// ���һ��ö��������
        /// </summary>
        void OnRandomTypeName()
        {
            if (string.IsNullOrEmpty(TypeName))
                TypeName = $"Enum_{UnityEngine.Random.Range(100, 999)}";
        }

        /// <summary>
        /// ��ʾע���ֶ�
        /// </summary>
        void OnDisplayAnnotation()
        {
            _showAnnotation = !_showAnnotation;
        }

        /// <summary>
        /// �����ļ����д  - ֵ�������ȷ��
        /// </summary>
        /// <returns> �Ƿ�ͨ�� </returns>
        public bool CheckValueList(out string errorLocation)
        {
            errorLocation = "";

            if (string.IsNullOrEmpty(TypeName))
            {
                errorLocation = "ö��������Ϊ��";
                return false;
            }

            if (ProtoHelper.IsBadName(TypeName))
            {
                errorLocation = "ö�����������������(��ĸ)(����)(�»���)������ַ�";
                return false;
            }

            if (ValueList.Count == 0)
            {
                errorLocation = "ö�ٱ������ٰ���һ��ֵ";
                return false;
            }

            List<int> tempValueList = new List<int>();
            if (ValueList != null)
            {
                for (int i = 0; i < ValueList.Count; i++)
                {
                    var item = ValueList[i];

                    if (i == 0 && item.Value != 0)
                    {
                        errorLocation = "proto3�еĵ�һ��ö��ֵ����Ϊ0";
                        return false;
                    }

                    string vError = "";
                    //ֵ�Ļ������
                    if (!item.CheckValue(out vError))
                    {
                        errorLocation = $"ֵ�б�������[{i}]��({item.ValueName}) - {vError}";
                        return false;
                    }

                    //ֵ��������-ֵ�Ƿ����ظ�
                    if (tempValueList.Contains(item.Value))
                    {
                        errorLocation = $"ֵ�б�������[{i}]��({item.ValueName}) - ö��ֵ�ظ�";
                        return false;
                    }
                    else
                        tempValueList.Add(item.Value);
                }
            }

            return true;
        }

        #endregion

    }


    /// <summary>
    /// EnumValueInfo Wrapper
    /// </summary>
    [Serializable]
    public class EnumValueInfoWrapper
    {
        /// <summary>
        /// ����ԭ����
        /// </summary>
        private EnumValueInfo _info;
        public EnumValueInfo Info { get { return this._info; } set { this._info = value; } }

        public EnumValueInfoWrapper()
        {
            this._info = new EnumValueInfo("");//��new����ʱ�ģ�����ḳ����ʽ��������
        }

        public EnumValueInfoWrapper(ref EnumValueInfo info)
        {
            this._info = info;
        }


        [PropertyOrder(0), ShowInInspector, VerticalGroup("ֵ��"), LabelText("����"), LabelWidth(30), DelayedProperty, TableColumnWidth(150), InlineButton("OnRandomValueName", SdfIconType.Dice5Fill, Label = "", ShowIf = "@ValueName==\"\""), InlineButton("OnDisplayAnnotation", SdfIconType.ChatLeftTextFill, Label = ""), Required("����д����")]
        public string ValueName
        { get { return this._info.valueName; } set { this._info.valueName = value; } }


        [PropertyOrder(1), ShowInInspector, VerticalGroup("ֵ��"), LabelText("ע��"), LabelWidth(30), ShowIf("_showAnnotation")]
        public string Annotation
        { get { return this._info.annotation; } set { this._info.annotation = value; } }

        [PropertyOrder(2), ShowInInspector, VerticalGroup("ʵ��ֵ"), HideLabel, DelayedProperty, TableColumnWidth(100, false), MinValue(0)]
        public int Value
        { get { return this._info.value; } set { this._info.value = value; } }


        #region �ڲ��ֶ�

        /// <summary>
        /// ��ʾע���ֶ�
        /// </summary>
        private bool _showAnnotation = false;

        #endregion

        #region �ڲ�����

        /// <summary>
        /// ���һ��ֵ������
        /// </summary>
        void OnRandomValueName()
        {
            if (string.IsNullOrEmpty(ValueName))
                ValueName = $"VALUE_{UnityEngine.Random.Range(1000, 9999)}";
        }

        /// <summary>
        /// ��ʾע���ֶ�
        /// </summary>
        void OnDisplayAnnotation()
        {
            _showAnnotation = !_showAnnotation;
        }

        /// <summary>
        /// �����ļ����д 
        /// </summary>
        /// <returns>�Ƿ�ͨ��</returns>
        public bool CheckValue(out string errorLocation)
        {
            errorLocation = "";
            if (string.IsNullOrEmpty(ValueName))
            {
                errorLocation = "ֵ����Ϊ��";
                return false;
            }

            if (ProtoHelper.IsBadName(ValueName))
            {
                errorLocation = "ֵ���� ���������(��ĸ)(����)(�»���)������ַ�";
                return false;
            }

            if (Value < 0)
            {
                errorLocation = "ֵΪ����";
                return false;
            }

            return true;
        }

        #endregion

    }

    /// <summary>
    /// ��Ϣ���ڲ�ö����ʾ �����ڲ�ö�ٱ༭����ʹ�ã�
    /// </summary>
    [Serializable]
    public class InternalEnumDisplayInfo
    {
        public MessageInfo Info { get; set; }

        public InternalEnumDisplayInfo(ref MessageInfo pInfo)
        {
            Info = pInfo;
        }


        [ShowInInspector, LabelText("�����ڲ�ö��"), InfoBox("enum ö��ֵ���ֶα��ֱ�ӱ�ʾ��ʵ��ֵ", SdfIconType.ArrowDownCircle, IconColor = "#DCE741"), TableList(ShowIndexLabels = true, AlwaysExpanded = true, DrawScrollView = false, CellPadding = 6), DisableContextMenu(DisableForCollectionElements = true), InlineButton("CheckEnumList", SdfIconType.BugFill, ""), InfoBox("$_enumErrorTip", InfoMessageType.Error, VisibleIf = "@_enumErrorTip!=\"\""), OnCollectionChanged("OnEnumInfoListChangedBefore", "OnEnumInfoListChangedAfter"), OnValueChanged("@ProtoHelper.SetDirty()", IncludeChildren = true)]
        public List<EnumInfoWrapper> InternalEnumInfoList = new List<EnumInfoWrapper>();


        #region �ڲ��ֶη���

        /*��� enum �Ĵ�����Ϣ*/
        private string _enumErrorTip = "";

        /*enum �Ĵ�����Ϣ���б�����*/
        private List<int> _enumErrorIndex = new List<int>();

        /// <summary>
        /// [�����д����] - Enum
        /// </summary>
        void CheckEnumList()
        {
            _enumErrorIndex.Clear();
            _enumErrorTip = "";

            StringBuilder sb = new StringBuilder();
            int num = 0, index = 0; string str;

            foreach (var info in InternalEnumInfoList)
            {
                string errorStr;
                if (!info.CheckValueList(out errorStr))
                {
                    num++;
                    str = num != 1 ? "\n" : "";
                    sb.Append($"{str}����������[{index}]��({info.TypeName})�з��ִ���: {errorStr}");
                    _enumErrorIndex.Add(index);
                }
                index++;
            }

            _enumErrorTip = sb.ToString();
        }


        /// <summary>
        /// EnumInfoList ö���б����仯ʱ Before  
        /// </summary>
        void OnEnumInfoListChangedBefore(CollectionChangeInfo info, object value)
        {
            Debug.Log(info);
            //ɾ��֮ǰ�Ѷ�Ӧ����������ɾ��
            if (info.ChangeType == CollectionChangeType.RemoveIndex)
            {
                List<EnumInfoWrapper> list = value as List<EnumInfoWrapper>;
                EnumInfoWrapper deleteItem = list[info.Index];
                Info.enumInfoList.RemoveAt(Info.enumInfoList.IndexOf(deleteItem.Info));
            }
        }

        /// <summary>
        /// EnumInfoList ö���б����仯ʱ After   
        /// </summary>
        void OnEnumInfoListChangedAfter(CollectionChangeInfo info, object value)
        {
            Debug.Log(info);
            //����֮��ͬʱ�������õ����ݣ������¸�ֵ
            if (info.ChangeType == CollectionChangeType.Add)
            {
                EnumInfoWrapper changedItem = info.Value as EnumInfoWrapper;
                EnumInfo newEnumInfo = new EnumInfo("");
                Info.enumInfoList.Add(newEnumInfo);
                changedItem.Info = newEnumInfo;
            }
        }

        #endregion

    }


    /// <summary>
    /// ����Ԥ����ʾ��������Ԥ�����봰��ʹ�ã�
    /// </summary>
    [Serializable]
    public class CodePreviewDisplayInfo
    {
        public CodePreviewDisplayInfo(string fileName, string content)
        {
            _CodeContent = content;
            titleText2 = $"����{DateTime.Now.ToString()}";
            titleText = $"����Ԥ��{fileName}";
        }

        [ShowInInspector, TitleGroup("$titleText", "$titleText2", TitleAlignments.Centered, GroupID = "TG"), BoxGroup("TG/B", ShowLabel = false), HideLabel, DisplayAsString(false), GUIColor("@this.codeColor")]
        public string CodeContent { get { return _CodeContent; } }
        private string _CodeContent = "";

        /// <summary>
        /// �ı���ɫ
        /// </summary>
        Color codeColor = Color.yellow;

        /// <summary>
        /// ������
        /// </summary>
        private string titleText2;

        /// <summary>
        /// ������
        /// </summary>
        private string titleText;
    }


    /// <summary>
    /// ��ǰ�������õ����ݣ���Ϣ�塢ö�٣������ڱ༭�ֶε��Զ������ͣ�
    /// </summary>
    public class RefCollectionData
    {
        /// <summary>
        /// �����ļ���
        /// </summary>
        public string aboutFileName;

        public List<string> messageRefString = new List<string>();

        public List<string> enumRefString = new List<string>();

        /// <summary>
        /// ������һ��FileDisplayInfo���õ���Ϣ��ö�ٵļ���
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static RefCollectionData Parse(ProtoMenuEditorWindow.FileDisplayInfo fileInfo)
        {
            if (fileInfo == null || fileInfo.EditorInfo == null)
                return null;

            return Parse(fileInfo.FileName, fileInfo.EditorInfo);
        }

        public static RefCollectionData Parse(string abountFileName, ProtoEditorInfoWrapper wrapper)
        {
            RefCollectionData data = new RefCollectionData();

            data.aboutFileName = abountFileName;

            string packName = string.IsNullOrEmpty(wrapper.PackageName) ? "" : $"{wrapper.PackageName}.";

            //ö��
            foreach (var i in wrapper.EnumInfoList)
                data.enumRefString.Add($"{packName}{i.TypeName}");

            //��Ϣ��
            foreach (var msg in wrapper.MessageInfoList)
            {
                data.messageRefString.Add($"{packName}{msg.Name}");

                //�ڲ�ö��
                foreach (var i in msg.EnumInfoList)
                    data.enumRefString.Add($"{packName}{msg.Name}.{i.TypeName}");
            }
            return data;
        }

    }

    public static class RefCollectionDataHelper
    {
        /// <summary>
        /// �ϲ�����
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static RefCollectionData CombineData(this List<RefCollectionData> list)
        {
            RefCollectionData temp = new RefCollectionData();
            temp.aboutFileName = "";
            foreach (var i in list)
            {
                temp.enumRefString.AddRange(i.enumRefString);
                temp.messageRefString.AddRange(i.messageRefString);
            }
            return temp;
        }
    }
}

