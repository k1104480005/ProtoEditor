
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
/// 定义结构的包装体（与Proto Editor-Structure中定义的结构保持一致）
///     - 包装体主要是为了自定义显示
///     - *后续可以开发检测原数据和Wrapper是否匹配
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
        /// 引用原数据
        /// </summary>
        private ProtoEditorInfo _info;
        public ProtoEditorInfo Info { get { return this._info; } }

        public ProtoEditorInfoWrapper(ref ProtoEditorInfo info)
        {
            this._info = info;

            //列表包装
            this.MessageInfoList = info.MessageInfoList.Select(i => new MessageInfoWrapper(ref i)).ToList();
            this.EnumInfoList = info.EnumInfoList.Select(i => new EnumInfoWrapper(ref i)).ToList();
        }

        [HorizontalGroup("CON/HGP", Gap = 10), BoxGroup("CON", ShowLabel = false), ShowInInspector, PropertyOrder(0), LabelText("语法协议"), LabelWidth(60), InfoBox("syntax 必须阐明语法协议规则,最新支持proto3", SdfIconType.ArrowDownCircle, "@ProtoHelper.ShowEditorInfoTipToggle", IconColor = "#DCE741"), DisableContextMenu]
        public YntaxType Yntax
        { get { return this._info.Yntax; } set { this._info.Yntax = value; } }

        [HorizontalGroup("CON/HGP"), BoxGroup("CON"), ShowInInspector, PropertyOrder(1), LabelText("命名空间"), LabelWidth(60), InfoBox("package 命名空间可以不填，不是必要的", SdfIconType.ArrowDownCircle, "@ProtoHelper.ShowEditorInfoTipToggle", IconColor = "#DCE741"), OnValueChanged("@ProtoHelper.SetDirty()", IncludeChildren = true), DisableContextMenu, InfoBox("$_selfErrorTip", InfoMessageType.Error, VisibleIf = "@_selfErrorTip!=\"\"")]
        public string PackageName
        { get { return this._info.PackageName; } set { this._info.PackageName = value; } }


        [PropertySpace(6), BoxGroup("CON"), TabGroup("CON/TAB", "引用文件", SdfIconType.FileEarmarkTextFill, true, TextColor = "#CB967F"), ShowInInspector, PropertyOrder(2), LabelText("引用其他文件"), InfoBox("import \n【1】引用其他proto文件,填写相对路径\n【2】只能向下引用，不能解释“../”或“..\\路径”\n【3】禁止proto文件分开存放\n【4】需要填写后缀.proto", SdfIconType.ArrowDownCircle, "@ProtoHelper.ShowEditorInfoTipToggle", IconColor = "#DCE741"), ListDrawerSettings(ShowFoldout = false, ElementColor = "OnImportElementColor"), ValueDropdown("@this.GetFilteredImprotFileList()", DropdownTitle = "请选择引用文件", ExcludeExistingValuesInList = true, IsUniqueList = true), OnValueChanged("@ProtoHelper.SetDirty()", IncludeChildren = true), InfoBox("$_importErrorTip", InfoMessageType.Error, VisibleIf = "@_importErrorTip!=\"\""), DisableContextMenu(true, true), InlineButton("CheckImportFile", SdfIconType.BugFill, ""), OnCollectionChanged("OnImportInfoListChangedAfter")]
        public List<string> ImportProtoFileName
        { get { return this._info.ImportProtoFileName; } set { this._info.ImportProtoFileName = value; } }


        [PropertySpace(6), BoxGroup("CON"), TabGroup("CON/TAB", "枚举", SdfIconType.Box, TextColor = "#ADC4A0"), ShowInInspector, PropertyOrder(3), LabelText("定义枚举"), InfoBox("enum 枚举值的字段编号直接表示了实际值", SdfIconType.ArrowDownCircle, "@ProtoHelper.ShowEditorInfoTipToggle", IconColor = "#DCE741"), OnValueChanged("@ProtoHelper.SetDirty()", IncludeChildren = true), TableList(ShowIndexLabels = true, AlwaysExpanded = true, DrawScrollView = false, CellPadding = 6), DisableContextMenu(true, true), InlineButton("CheckEnumList", SdfIconType.BugFill, ""), InfoBox("$_enumErrorTip", InfoMessageType.Error, VisibleIf = "@_enumErrorTip!=\"\""), OnCollectionChanged("OnEnumInfoListChangedBefore", "OnEnumInfoListChangedAfter")]
        public List<EnumInfoWrapper> EnumInfoList = new List<EnumInfoWrapper>();


        [PropertySpace(6), BoxGroup("CON"), TabGroup("CON/TAB", "消息体", SdfIconType.Box, TextColor = "#45A995"), ShowInInspector, PropertyOrder(4), LabelText("定义消息体"), InfoBox("message\n【字段修饰】“proto3”仅仅支持repeated字段修饰，如果使用required，optional编译会报错\n【命名空间】引用外部类型有命名空间的一定要加命名空间延伸描述\n【编号规则】编号必须是正整数，不然会报错\n【编号规则】编号顺序非必须连续，因为生成的代码是用switch检索", SdfIconType.ArrowDownCircle, "@ProtoHelper.ShowEditorInfoTipToggle", IconColor = "#DCE741"), OnValueChanged("@ProtoHelper.SetDirty()", IncludeChildren = true), TableList(AlwaysExpanded = true, DrawScrollView = false, CellPadding = 6), DisableContextMenu(true, true), InlineButton("CheckMessageList", SdfIconType.BugFill, ""), InfoBox("$_messageErrorTip", InfoMessageType.Error, VisibleIf = "@_messageErrorTip!=\"\""), OnCollectionChanged("OnMessageInfoListChangedBefore", "OnMessageInfoListChangedAfter")]
        public List<MessageInfoWrapper> MessageInfoList = new List<MessageInfoWrapper>();


        #region 内部字段

        /*检查Import的错误信息*/
        private string _importErrorTip = "";

        /*Import的错误信息的列表索引*/
        private List<int> _importErrorIndex = new List<int>();

        /*检查 enum 的错误信息*/
        private string _enumErrorTip = "";

        /*enum 的错误信息的列表索引*/
        private List<int> _enumErrorIndex = new List<int>();

        /*检查 message 的错误信息*/
        private string _messageErrorTip = "";

        /*message 的错误信息的列表索引*/
        private List<int> _messageErrorIndex = new List<int>();

        /*检查 self 的错误信息*/
        private string _selfErrorTip = "";


        //绘制提示信息框
        [PropertyOrder(int.MinValue), OnInspectorGUI, ShowIf("@ProtoHelper.ShowEditorInfoTipToggle")]
        private void DrawIntroInfoBox()
        {
            SirenixEditorGUI.IconMessageBox("← 这种图标的按钮是检查填写错误按钮，如果点击没反应就代表没有错误，出现错误并解决后再点击一次即可清理错误信息", SdfIconType.BugFill);
        }
        #endregion

        #region 内部方法

        /// <summary>
        /// EnumInfoList 枚举列表发生变化时 Before
        /// </summary>
        void OnEnumInfoListChangedBefore(CollectionChangeInfo info, object value)
        {
            //删除之前把对应的引用数据删除
            if (info.ChangeType == CollectionChangeType.RemoveIndex)
            {
                List<EnumInfoWrapper> list = value as List<EnumInfoWrapper>;
                EnumInfoWrapper deleteItem = list[info.Index];
                _info.EnumInfoList.RemoveAt(_info.EnumInfoList.IndexOf(deleteItem.Info));
            }
        }

        /// <summary>
        /// EnumInfoList 枚举列表发生变化时 After
        /// </summary>
        void OnEnumInfoListChangedAfter(CollectionChangeInfo info, object value)
        {
            //新增之后同时新增引用的数据，并重新赋值
            if (info.ChangeType == CollectionChangeType.Add)
            {
                EnumInfoWrapper changedItem = info.Value as EnumInfoWrapper;
                EnumInfo newEnumInfo = new EnumInfo("");
                _info.EnumInfoList.Add(newEnumInfo);
                changedItem.Info = newEnumInfo;
            }

        }


        /// <summary>
        /// MessageInfoList 列表发生变化时 Before
        /// </summary>
        void OnMessageInfoListChangedBefore(CollectionChangeInfo info, object value)
        {
            //删除时把对应的引用数据删除
            if (info.ChangeType == CollectionChangeType.RemoveIndex)
            {
                List<MessageInfoWrapper> list = value as List<MessageInfoWrapper>;
                MessageInfoWrapper deleteItem = list[info.Index];
                _info.MessageInfoList.RemoveAt(_info.MessageInfoList.IndexOf(deleteItem.Info));
            }
        }

        /// <summary>
        /// MessageInfoList 列表发生变化时 After
        /// </summary>
        void OnMessageInfoListChangedAfter(CollectionChangeInfo info, object value)
        {
            //新增之后同时新增引用的数据，并重新赋值
            if (info.ChangeType == CollectionChangeType.Add)
            {
                MessageInfoWrapper changedItem = info.Value as MessageInfoWrapper;
                MessageInfo newData = new MessageInfo();
                _info.MessageInfoList.Add(newData);
                changedItem.Info = newData;
            }

        }


        /// <summary>
        /// Import 列表发生变化时 After
        /// </summary>
        void OnImportInfoListChangedAfter(CollectionChangeInfo info, object value)
        {
            //..s
        }


        /// <summary>
        /// 获得过滤后的所有可引用的文件命名列表
        /// </summary>
        /// <returns></returns>
        IEnumerable GetFilteredImprotFileList()
        {
            //尝试过滤
            if (ProtoMenuEditorWindow.sLastSelecetedMenuItemData != null)
                return EditProto.ProtoHelper.GetProtoFileNameValueDropdownList(null, new List<string>() { ProtoMenuEditorWindow.sLastSelecetedMenuItemData.FileName });
            //过滤失败
            else
            {
                ProtoHelper.LogError("GetFilteredImprotFileList 过滤失败");
                return EditProto.ProtoHelper.GetProtoFileNameValueDropdownList(null);
            }
        }

        /// <summary>
        /// [检查填写错误] - Import
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
                    sb.Append($"{str}引用文件不存在:【{num}】{fullname}");
                    _importErrorIndex.Add(index);
                }
                index++;
            }
            _importErrorTip = sb.ToString();
            return string.IsNullOrEmpty(_importErrorTip);
        }

        /// <summary>
        /// 控制Import列表子项的颜色
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
        /// [检查填写错误] - Enum
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
                    sb.Append($"{str}【错误】索引[{index}]的({info.TypeName})中发现错误: {errorStr}");
                    _enumErrorIndex.Add(index);
                }
                index++;
            }

            _enumErrorTip = sb.ToString();
            return string.IsNullOrEmpty(_enumErrorTip);
        }

        /// <summary>
        /// [检查填写错误] - Message
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
                    sb.Append($"{str}【错误】消息体({info.Name})中发现错误: {errorStr}");
                }
                index++;
            }

            _messageErrorTip = sb.ToString();
            return string.IsNullOrEmpty(_messageErrorTip);
        }


        /// <summary>
        /// 检查自身
        /// </summary>
        /// <returns></returns>
        public bool CheckSelf()
        {
            if (ProtoHelper.IsBadName(PackageName))
            {
                _selfErrorTip = "命名空间 不允许出现(字母)(数字)(下划线)以外的字符";
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
        /// 引用原数据
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

            //列表包装
            this.FieldsList = info.fieldsList.Select(i => new FieldsInfoWrapper(ref i)).ToList();
            this.EnumInfoList = info.enumInfoList.Select(i => new EnumInfoWrapper(ref i)).ToList();

            RefreshInternalEnumInfoButton();
        }

        [PropertyOrder(0), TableColumnWidth(150), ShowInInspector, VerticalGroup("消息体名称"), LabelText(""), LabelWidth(30), InlineButton("OnRandomMessageName", SdfIconType.Dice5Fill, Label = "", ShowIf = "@Name==\"\""), InlineButton("OnDisplayAnnotation", SdfIconType.ChatLeftTextFill, Label = ""), DelayedProperty, DisableContextMenu, Required("请填入该项")]
        public string Name
        { get { return this._info.name; } set { this._info.name = value; } }

        [PropertyOrder(1), TableColumnWidth(150), ShowInInspector, VerticalGroup("消息体名称"), LabelText("注释"), LabelWidth(30), DisableContextMenu, MultiLineProperty(5), ShowIf("_showAnnotation")]
        public string Annotation
        { get { return this._info.annotation; } set { this._info.annotation = value; } }


        [PropertyOrder(2), TableColumnWidth(400), ShowInInspector, HideLabel, VerticalGroup("字段列表"), DisableContextMenu(DisableForCollectionElements = true), LabelText("点击展开"), TableList(ShowIndexLabels = false, AlwaysExpanded = false, DrawScrollView = false, CellPadding = 4), OnCollectionChanged("OnFieldsListChangedBefore", "OnFieldsListChangedAfter")]
        public List<FieldsInfoWrapper> FieldsList = new List<FieldsInfoWrapper>();

        [HideInInspector, OnCollectionChanged("OnEnumInfoListChangedBeforeInternal", "OnEnumInfoListChangedAfterInternal")]
        public List<EnumInfoWrapper> EnumInfoList = new List<EnumInfoWrapper>();


        [PropertyOrder(-2), TableColumnWidth(100, false), ShowInInspector, VerticalGroup("内部枚举"), HorizontalGroup("内部枚举/按钮组"), Button(SdfIconType.ArrowRepeat, "")]
        private void RefreshInternalEnumInfoButton()
        {
            InternalEnumInfoNameList.Clear();
            foreach (var i in EnumInfoList)
                InternalEnumInfoNameList.Add(i.TypeName);
        }

        [PropertyOrder(-2), TableColumnWidth(100, false), ShowInInspector, VerticalGroup("内部枚举"), HorizontalGroup("内部枚举/按钮组"), Button(SdfIconType.PencilFill, "")]
        private void EditInternalEnumInfoButton()
        {
            //前往编辑内部枚举
            var window = OdinEditorWindow.InspectObject(new InternalEnumDisplayInfo(ref _info) { InternalEnumInfoList = this.EnumInfoList });
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(600, 400);
            window.OnClose += () =>
            {
                //关闭时刷新一下内部枚举显示
                RefreshInternalEnumInfoButton();
            };
            window.OnBeginGUI += () =>
            {
                //鼠标移动时会运行多次
                if (window != null && EditorWindow.focusedWindow != window)
                {
                    window.Close();
                    window = null;
                    GUIUtility.ExitGUI(); //在这里添加这一句,报错消失
                }
            };
        }

        //这里只预览内部枚举的名称
        [PropertyOrder(-1), TableColumnWidth(100, false), ShowInInspector, VerticalGroup("内部枚举"), ReadOnly, ListDrawerSettings(ShowItemCount = false), LabelText("枚举列表"), DisableContextMenu(DisableForCollectionElements = true)]
        public List<string> InternalEnumInfoNameList = new List<string>();


        #region 内部字段

        /// <summary>
        /// 显示注释字段
        /// </summary>
        private bool _showAnnotation = false;

        #endregion


        #region 内部方法

        /// <summary>
        /// EnumInfoList 枚举列表发生变化时 Before  
        /// </summary>
        void OnEnumInfoListChangedBeforeInternal(CollectionChangeInfo info, object value)
        {
            //删除之前把对应的引用数据删除
            if (info.ChangeType == CollectionChangeType.RemoveIndex)
            {
                List<EnumInfoWrapper> list = value as List<EnumInfoWrapper>;
                EnumInfoWrapper deleteItem = list[info.Index];
                _info.enumInfoList.RemoveAt(_info.enumInfoList.IndexOf(deleteItem.Info));
            }
        }

        /// <summary>
        /// EnumInfoList 枚举列表发生变化时 After   
        /// </summary>
        void OnEnumInfoListChangedAfterInternal(CollectionChangeInfo info, object value)
        {
            //新增之后同时新增引用的数据，并重新赋值
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
        /// FieldsList 枚举列表发生变化时 Before
        /// </summary>
        void OnFieldsListChangedBefore(CollectionChangeInfo info, object value)
        {
            //删除之前把对应的引用数据删除
            if (info.ChangeType == CollectionChangeType.RemoveIndex)
            {
                List<FieldsInfoWrapper> list = value as List<FieldsInfoWrapper>;
                FieldsInfoWrapper deleteItem = list[info.Index];
                _info.fieldsList.RemoveAt(_info.fieldsList.IndexOf(deleteItem.Info));
            }
        }

        /// <summary>
        /// FieldsList 枚举列表发生变化时 After
        /// </summary>
        void OnFieldsListChangedAfter(CollectionChangeInfo info, object value)
        {
            //新增之后同时新增引用的数据，并重新赋值
            if (info.ChangeType == CollectionChangeType.Add)
            {
                FieldsInfoWrapper changedItem = info.Value as FieldsInfoWrapper;
                FieldsInfo newEnumInfo = new FieldsInfo(0);
                _info.fieldsList.Add(newEnumInfo);
                changedItem.Info = newEnumInfo;
            }
        }

        /// <summary>
        /// 随机一个消息体命名
        /// </summary>
        void OnRandomMessageName()
        {
            Name = string.Format($"Message{UnityEngine.Random.Range(10, 99)}");
        }

        /// <summary>
        /// 开关注释显示
        /// </summary>
        void OnDisplayAnnotation()
        {
            _showAnnotation = !_showAnnotation;
        }

        /// <summary>
        /// 基础的检查填写
        /// </summary>
        /// <returns> 是否通过 </returns>
        public bool CheckInput(out string errorLocation)
        {
            errorLocation = "";

            if (string.IsNullOrEmpty(Name))
            {
                errorLocation = "消息体名为空";
                return false;
            }

            if (ProtoHelper.IsBadName(Name))
            {
                errorLocation = "消息体名 不允许出现(字母)(数字)(下划线)以外的字符";
                return false;
            }

            //字段检查
            List<uint> tempCodeList = new List<uint>();
            for (int i = 0; i < FieldsList.Count; i++)
            {
                var item = FieldsList[i];
                string vError = "";
                //基础检查
                if (!item.CheckInput(out vError))
                {
                    errorLocation = $"字段({item.Name})的 - {vError}";
                    return false;
                }

                //字段编号必须从1开始的非负数
                if(item.Code <= 0)
                {
                    errorLocation = $"字段({item.Name})的 字段编号{item.Code} 必须大于0";
                    return false;
                }

                //Code是否有重复
                if (tempCodeList.Contains(item.Code))
                {
                    errorLocation = $"字段({item.Name})的 字段编号{item.Code}重复";
                    return false;
                }
                else
                    tempCodeList.Add(item.Code);
            }

            //内部枚举值检查
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
                        sb.Append($"{str}【错误】内部枚举中索引[{index}]的({info.TypeName})发现错误: {errorStr}");
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
        /// 引用原数据
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


        [PropertyOrder(1), TableColumnWidth(85, false), ShowInInspector, VerticalGroup("修饰符"), HideLabel, DisableContextMenu(DisableForCollectionElements = true)]
        public ModifierType Modifier
        { get { return this._info.modifier; } set { this._info.modifier = value; } }

        [PropertyOrder(2), TableColumnWidth(50), ShowInInspector, VerticalGroup("类型"), HideLabel, DisableContextMenu]
        public FieldsType Type
        { get { return this._info.type; } set { this._info.type = value; } }

        [PropertyOrder(3), TableColumnWidth(50), ShowInInspector, VerticalGroup("类型"), HideLabel, DisableContextMenu, ShowIf("@Type == FieldsType.Custom"), DelayedProperty, ValueDropdown("GetRefMsgOrEnum", AppendNextDrawer = true, DisableGUIInAppendedDrawer = false)]
        public string CustomTypeName
        { get { return this._info.customTypeName; } set { this._info.customTypeName = value; } }


        [PropertyOrder(6), TableColumnWidth(50), ShowInInspector, VerticalGroup("命名"), HideLabel, DisableContextMenu, DelayedProperty, Required("请填写该项"), InlineButton("OnRandomFieldName", SdfIconType.Dice5Fill, Label = "", ShowIf = "@Name==\"\""), InlineButton("OnDisplayAnnotation", SdfIconType.ChatLeftTextFill, Label = "")]
        public string Name
        { get { return this._info.name; } set { this._info.name = value; } }

        [PropertyOrder(7), TableColumnWidth(50), ShowInInspector, VerticalGroup("命名"), LabelText("注释"), LabelWidth(30), DisableContextMenu, ShowIf("_showAnnotation"), MultiLineProperty(5)]
        public string Annotation
        { get { return this._info.annotation; } set { this._info.annotation = value; } }

        [PropertyOrder(10), TableColumnWidth(35, false), ShowInInspector, VerticalGroup("编号"), HideLabel, DisableContextMenu, MinValue(0)]
        public uint Code
        { get { return this._info.code; } set { this._info.code = value; } }


        #region 内部字段

        /// <summary>
        /// 显示注释字段
        /// </summary>
        private bool _showAnnotation = false;

        #endregion


        #region 内部方法

        /// <summary>
        /// 获得所有可以引用的消息体和枚举
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

            //获得自己文件的数据
            ProtoMenuEditorWindow.UpdateRefCollectionData(ProtoMenuEditorWindow.sLastSelecetedMenuItemData.FileName, true);//强制更新自己的
            RefCollectionData refData = ProtoMenuEditorWindow.GetRefCollectionData(ProtoMenuEditorWindow.sLastSelecetedMenuItemData.FileName);
            if (refData != null)
                totalDataList.Add(refData);

            //加入引用其他文件的数据
            List<string> fileNames = ProtoMenuEditorWindow.sLastSelecetedMenuItemData.EditorInfo.ImportProtoFileName;
            foreach (var name in fileNames)
            {
                ProtoMenuEditorWindow.UpdateRefCollectionData(name, false);//非强制更新
                refData = ProtoMenuEditorWindow.GetRefCollectionData(name);
                if (refData != null)
                    totalDataList.Add(refData);
            }

            if (totalDataList.Count == 0)
                return outList;

            RefCollectionData all = totalDataList.CombineData();

            foreach (var i in all.enumRefString)
            {
                outList.Add($"枚举/{i.Replace('.', '/')}", i);
            }
            foreach (var i in all.messageRefString)
            {
                outList.Add($"消息体/{i.Replace('.', '/')}", i);
            }

            return outList;
        }

        /// <summary>
        /// 随机一个字段命名
        /// </summary>
        void OnRandomFieldName()
        {
            Name = string.Format($"Field{UnityEngine.Random.Range(10, 99)}");
        }

        /// <summary>
        /// 开关注释显示
        /// </summary>
        void OnDisplayAnnotation()
        {
            _showAnnotation = !_showAnnotation;
        }

        /// <summary>
        /// 检查填写
        /// </summary>
        /// <param name="errorLocation"></param>
        /// <returns></returns>
        public bool CheckInput(out string errorLocation)
        {
            errorLocation = "";

            if (string.IsNullOrEmpty(Name))
            {
                errorLocation = "字段名为空";
                return false;
            }

            if (ProtoHelper.IsBadName(Name))
            {
                errorLocation = "字段名 不允许出现(字母)(数字)(下划线)以外的字符";
                return false;
            }


            if (Type == FieldsType.Custom)
            {
                if (string.IsNullOrEmpty(CustomTypeName))
                {
                    errorLocation = "自定义类型名为空";
                    return false;
                }

                if (ProtoHelper.IsBadName(CustomTypeName))
                {
                    errorLocation = "自定义类型名 不允许出现(字母)(数字)(下划线)以外的字符";
                    return false;
                }

                //根据当前已有数据检查是否存在消息体枚举，因为重新读取所有文件负担太重，这里只能轻度检查
                bool found = false;
                List<RefCollectionData> checkRefList = ProtoMenuEditorWindow.s_RefCollectionDataCacheDic.Values.ToArray().ToList();
                RefCollectionData checkData = checkRefList.CombineData();
                found = checkData.enumRefString.Find(e => e == CustomTypeName) != null;
                if (!found)
                    found = checkData.messageRefString.Find(e => e == CustomTypeName) != null;
                if (!found)
                {
                    errorLocation = $"没找到可用的名字为{CustomTypeName}的消息体或枚举，是否没引用相关文件？";
                    return false;
                }
            }

            if (Code < 0)
            {
                errorLocation = "字段编号为负数";
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
        /// 引用原数据
        /// </summary>
        private EnumInfo _info;
        public EnumInfo Info { get { return this._info; } set { this._info = value; } }

        public EnumInfoWrapper()
        {
            this._info = new EnumInfo("");//此new的数据是临时的，不是真正数据,外部会赋予新数据
        }

        public EnumInfoWrapper(ref EnumInfo info)
        {
            this._info = info;

            //列表包装
            this.ValueList = info.valueList.Select(i => new EnumValueInfoWrapper(ref i)).ToList();
        }

        [PropertyOrder(0), TableColumnWidth(200, false), ShowInInspector, VerticalGroup("枚举类型"), LabelText("名称"), LabelWidth(30), InlineButton("OnRandomTypeName", SdfIconType.Dice5Fill, Label = "", ShowIf = "@TypeName==\"\""), InlineButton("OnDisplayAnnotation", SdfIconType.ChatLeftTextFill, Label = ""), DelayedProperty, Required("请填入该项")]
        public string TypeName
        { get { return this._info.typeName; } set { this._info.typeName = value; } }


        [PropertyOrder(1), TableColumnWidth(100), ShowInInspector, VerticalGroup("枚举类型"), LabelText("注释"), LabelWidth(30), MultiLineProperty(5), ShowIf("_showAnnotation")]
        public string Annotation
        { get { return this._info.annotation; } set { this._info.annotation = value; } }

        [PropertyOrder(2), ShowInInspector, VerticalGroup("值列表"), LabelText("点击展开"), TableList(ShowIndexLabels = true, AlwaysExpanded = false, DrawScrollView = false, CellPadding = 4), OnCollectionChanged("OnValueListChangedBefore", "OnValueListChangedAfter")]
        public List<EnumValueInfoWrapper> ValueList = new List<EnumValueInfoWrapper>();


        #region 内部字段

        /// <summary>
        /// 显示注释字段
        /// </summary>
        private bool _showAnnotation = false;

        #endregion

        #region 内部方法

        /// <summary>
        /// ValueList 枚举列表发生变化时 Before
        /// </summary>
        void OnValueListChangedBefore(CollectionChangeInfo info, object value)
        {
            //删除之前把对应的引用数据删除
            if (info.ChangeType == CollectionChangeType.RemoveIndex)
            {
                List<EnumValueInfoWrapper> list = value as List<EnumValueInfoWrapper>;
                EnumValueInfoWrapper deleteItem = list[info.Index];
                _info.valueList.RemoveAt(_info.valueList.IndexOf(deleteItem.Info));
            }
        }

        /// <summary>
        /// ValueList 枚举列表发生变化时 After
        /// </summary>
        void OnValueListChangedAfter(CollectionChangeInfo info, object value)
        {
            //新增之后同时新增引用的数据，并重新赋值
            if (info.ChangeType == CollectionChangeType.Add)
            {
                EnumValueInfoWrapper changedItem = info.Value as EnumValueInfoWrapper;
                EnumValueInfo newinfo = new EnumValueInfo("");
                _info.valueList.Add(newinfo);
                changedItem.Info = newinfo;
            }
        }

        /// <summary>
        /// 随机一个枚举名出来
        /// </summary>
        void OnRandomTypeName()
        {
            if (string.IsNullOrEmpty(TypeName))
                TypeName = $"Enum_{UnityEngine.Random.Range(100, 999)}";
        }

        /// <summary>
        /// 显示注释字段
        /// </summary>
        void OnDisplayAnnotation()
        {
            _showAnnotation = !_showAnnotation;
        }

        /// <summary>
        /// 基础的检查填写  - 值输入的正确性
        /// </summary>
        /// <returns> 是否通过 </returns>
        public bool CheckValueList(out string errorLocation)
        {
            errorLocation = "";

            if (string.IsNullOrEmpty(TypeName))
            {
                errorLocation = "枚举类型名为空";
                return false;
            }

            if (ProtoHelper.IsBadName(TypeName))
            {
                errorLocation = "枚举类型名不允许出现(字母)(数字)(下划线)以外的字符";
                return false;
            }

            if (ValueList.Count == 0)
            {
                errorLocation = "枚举必须至少包含一个值";
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
                        errorLocation = "proto3中的第一个枚举值必须为0";
                        return false;
                    }

                    string vError = "";
                    //值的基础检查
                    if (!item.CheckValue(out vError))
                    {
                        errorLocation = $"值列表中索引[{i}]的({item.ValueName}) - {vError}";
                        return false;
                    }

                    //值的特殊检查-值是否有重复
                    if (tempValueList.Contains(item.Value))
                    {
                        errorLocation = $"值列表中索引[{i}]的({item.ValueName}) - 枚举值重复";
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
        /// 引用原数据
        /// </summary>
        private EnumValueInfo _info;
        public EnumValueInfo Info { get { return this._info; } set { this._info = value; } }

        public EnumValueInfoWrapper()
        {
            this._info = new EnumValueInfo("");//此new是临时的，后面会赋予正式引用数据
        }

        public EnumValueInfoWrapper(ref EnumValueInfo info)
        {
            this._info = info;
        }


        [PropertyOrder(0), ShowInInspector, VerticalGroup("值名"), LabelText("名称"), LabelWidth(30), DelayedProperty, TableColumnWidth(150), InlineButton("OnRandomValueName", SdfIconType.Dice5Fill, Label = "", ShowIf = "@ValueName==\"\""), InlineButton("OnDisplayAnnotation", SdfIconType.ChatLeftTextFill, Label = ""), Required("请填写该项")]
        public string ValueName
        { get { return this._info.valueName; } set { this._info.valueName = value; } }


        [PropertyOrder(1), ShowInInspector, VerticalGroup("值名"), LabelText("注释"), LabelWidth(30), ShowIf("_showAnnotation")]
        public string Annotation
        { get { return this._info.annotation; } set { this._info.annotation = value; } }

        [PropertyOrder(2), ShowInInspector, VerticalGroup("实际值"), HideLabel, DelayedProperty, TableColumnWidth(100, false), MinValue(0)]
        public int Value
        { get { return this._info.value; } set { this._info.value = value; } }


        #region 内部字段

        /// <summary>
        /// 显示注释字段
        /// </summary>
        private bool _showAnnotation = false;

        #endregion

        #region 内部方法

        /// <summary>
        /// 随机一个值名出来
        /// </summary>
        void OnRandomValueName()
        {
            if (string.IsNullOrEmpty(ValueName))
                ValueName = $"VALUE_{UnityEngine.Random.Range(1000, 9999)}";
        }

        /// <summary>
        /// 显示注释字段
        /// </summary>
        void OnDisplayAnnotation()
        {
            _showAnnotation = !_showAnnotation;
        }

        /// <summary>
        /// 基础的检查填写 
        /// </summary>
        /// <returns>是否通过</returns>
        public bool CheckValue(out string errorLocation)
        {
            errorLocation = "";
            if (string.IsNullOrEmpty(ValueName))
            {
                errorLocation = "值命名为空";
                return false;
            }

            if (ProtoHelper.IsBadName(ValueName))
            {
                errorLocation = "值命名 不允许出现(字母)(数字)(下划线)以外的字符";
                return false;
            }

            if (Value < 0)
            {
                errorLocation = "值为负数";
                return false;
            }

            return true;
        }

        #endregion

    }

    /// <summary>
    /// 消息体内部枚举显示 （仅内部枚举编辑窗口使用）
    /// </summary>
    [Serializable]
    public class InternalEnumDisplayInfo
    {
        public MessageInfo Info { get; set; }

        public InternalEnumDisplayInfo(ref MessageInfo pInfo)
        {
            Info = pInfo;
        }


        [ShowInInspector, LabelText("定义内部枚举"), InfoBox("enum 枚举值的字段编号直接表示了实际值", SdfIconType.ArrowDownCircle, IconColor = "#DCE741"), TableList(ShowIndexLabels = true, AlwaysExpanded = true, DrawScrollView = false, CellPadding = 6), DisableContextMenu(DisableForCollectionElements = true), InlineButton("CheckEnumList", SdfIconType.BugFill, ""), InfoBox("$_enumErrorTip", InfoMessageType.Error, VisibleIf = "@_enumErrorTip!=\"\""), OnCollectionChanged("OnEnumInfoListChangedBefore", "OnEnumInfoListChangedAfter"), OnValueChanged("@ProtoHelper.SetDirty()", IncludeChildren = true)]
        public List<EnumInfoWrapper> InternalEnumInfoList = new List<EnumInfoWrapper>();


        #region 内部字段方法

        /*检查 enum 的错误信息*/
        private string _enumErrorTip = "";

        /*enum 的错误信息的列表索引*/
        private List<int> _enumErrorIndex = new List<int>();

        /// <summary>
        /// [检查填写错误] - Enum
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
                    sb.Append($"{str}【错误】索引[{index}]的({info.TypeName})中发现错误: {errorStr}");
                    _enumErrorIndex.Add(index);
                }
                index++;
            }

            _enumErrorTip = sb.ToString();
        }


        /// <summary>
        /// EnumInfoList 枚举列表发生变化时 Before  
        /// </summary>
        void OnEnumInfoListChangedBefore(CollectionChangeInfo info, object value)
        {
            Debug.Log(info);
            //删除之前把对应的引用数据删除
            if (info.ChangeType == CollectionChangeType.RemoveIndex)
            {
                List<EnumInfoWrapper> list = value as List<EnumInfoWrapper>;
                EnumInfoWrapper deleteItem = list[info.Index];
                Info.enumInfoList.RemoveAt(Info.enumInfoList.IndexOf(deleteItem.Info));
            }
        }

        /// <summary>
        /// EnumInfoList 枚举列表发生变化时 After   
        /// </summary>
        void OnEnumInfoListChangedAfter(CollectionChangeInfo info, object value)
        {
            Debug.Log(info);
            //新增之后同时新增引用的数据，并重新赋值
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
    /// 代码预览显示（仅用于预览代码窗口使用）
    /// </summary>
    [Serializable]
    public class CodePreviewDisplayInfo
    {
        public CodePreviewDisplayInfo(string fileName, string content)
        {
            _CodeContent = content;
            titleText2 = $"打开于{DateTime.Now.ToString()}";
            titleText = $"正在预览{fileName}";
        }

        [ShowInInspector, TitleGroup("$titleText", "$titleText2", TitleAlignments.Centered, GroupID = "TG"), BoxGroup("TG/B", ShowLabel = false), HideLabel, DisplayAsString(false), GUIColor("@this.codeColor")]
        public string CodeContent { get { return _CodeContent; } }
        private string _CodeContent = "";

        /// <summary>
        /// 文本颜色
        /// </summary>
        Color codeColor = Color.yellow;

        /// <summary>
        /// 副标题
        /// </summary>
        private string titleText2;

        /// <summary>
        /// 主标题
        /// </summary>
        private string titleText;
    }


    /// <summary>
    /// 当前可以引用的数据（消息体、枚举）（用于编辑字段的自定义类型）
    /// </summary>
    public class RefCollectionData
    {
        /// <summary>
        /// 关联文件名
        /// </summary>
        public string aboutFileName;

        public List<string> messageRefString = new List<string>();

        public List<string> enumRefString = new List<string>();

        /// <summary>
        /// 解析出一个FileDisplayInfo可用的消息体枚举的集合
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

            //枚举
            foreach (var i in wrapper.EnumInfoList)
                data.enumRefString.Add($"{packName}{i.TypeName}");

            //消息体
            foreach (var msg in wrapper.MessageInfoList)
            {
                data.messageRefString.Add($"{packName}{msg.Name}");

                //内部枚举
                foreach (var i in msg.EnumInfoList)
                    data.enumRefString.Add($"{packName}{msg.Name}.{i.TypeName}");
            }
            return data;
        }

    }

    public static class RefCollectionDataHelper
    {
        /// <summary>
        /// 合并起来
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

