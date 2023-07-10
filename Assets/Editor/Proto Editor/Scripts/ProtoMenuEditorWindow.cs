using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EditProto
{
    /// <summary>
    /// Proto编辑器窗口
    /// </summary>
    public partial class ProtoMenuEditorWindow : OdinMenuEditorWindow
    {
        public static ProtoMenuEditorWindow Instance;
        public static FileDisplayInfo sLastSelecetedMenuItemData { get; set; }//最后选中MenuItem的值数据（可能并不是当前选中MenuItem的）

        public static Dictionary<string, RefCollectionData> s_RefCollectionDataCacheDic = new Dictionary<string, RefCollectionData>();/* 引用数据收集字典 KEY文件名带后缀 VALUE数据 */



        [MenuItem("GameTools/ProtoEditor v" + ProtoHelper.Version)]
        private static void OpenWindow()
        {
            Instance = GetWindow<ProtoMenuEditorWindow>();
            Instance.position = GUIHelper.GetEditorWindowRect().AlignCenter(1080, 700);
            Instance.titleContent = new GUIContent("ProtoEditor");
        }

        /// <summary>
        /// 搜索到的所有文件(有变化时需要手动刷新)
        /// </summary>
        public List<FileDisplayInfo> CurAllFileList = new List<FileDisplayInfo>();



        protected override void OnDisable()
        {
            base.OnDisable();

            Instance = null;
            sLastSelecetedMenuItemData = null;
        }

        protected override void OnDestroy()
        {
            _SettingPanelWindow?.Close();
            _CreateNewFilePanelWindow?.Close();
            _AboutPanelWindow?.Close();
            _SnapShootPanelWindow?.Close();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            Instance = this;

            //窗口本身也能定制
            this.MenuWidth = 200;
            this.ResizableMenuWidth = true;
            this.WindowPadding = new Vector4(10, 10, 10, 10);
            this.DrawUnityEditorPreview = true;
            this.DefaultEditorPreviewHeight = 20;
            this.UseScrollView = true;

            //关于面板数据初始赋值 并读取配置
            CurSettingPanelDisplay = new SettingPanelDisplay(ShowNotificationForPanelCallback, () => { RefreshAllProtoFiles(false); });
            CurSettingPanelDisplay.ReadSetting();

            //刷新文件数据
            RefreshAllProtoFiles();

            //创建新文件面板数据初始赋值
            CurQuickCreateFilePanelDisplay = new QuickCreateFilePanelDisplay(GetFileOutputPathCallback, SetFileOutputPathCallback, CloseFileCreatePanelCallback, CreateFileFinishCallback);

            //关于面板数据初始赋值
            CurAboutPanelDisplay = new AboutPanelDisplay();

            //快照面板数据初始赋值
            CurSnapShootPanelDisplay = new SnapShootPanelDisplay();

            s_RefCollectionDataCacheDic.Clear();
        }

        /// <summary>
        /// 树形菜单
        /// </summary>
        /// <returns></returns>
        protected override OdinMenuTree BuildMenuTree()
        {
            var _tree = new OdinMenuTree(false);

            _tree.DefaultMenuStyle = ProtoHelper.customMenuStyle;
            _tree.Config.DrawSearchToolbar = true;
            _tree.Config.SearchToolbarHeight = 25;

            _tree.Selection.SelectionChanged += OnMenuTreeSelectionChanged;

            //有文件时正常显示所有文件
            if (CurAllFileList.Count > 0)
            {
                for (int i = 0; i < CurAllFileList.Count; i++)
                {
                    var fileItem = new FileMenuItem(_tree, CurAllFileList[i], DeleteFileCallback);
                    _tree.AddMenuItemAtPath(ProtoHelper.MenuTreeRootName, fileItem);
                }
            }
            //无文件时特殊提示
            else
            {
                _tree.Add(ProtoHelper.MenuTreeRootName_Null, new ROOTDisplayInfo(), SdfIconType.QuestionLg);
            }

            return _tree;
        }

        /// <summary>
        /// 绘制右侧工具栏(Update)
        /// </summary>
        protected override void OnBeginDrawEditors()
        {
            if (this.MenuTree == null || this.MenuTree.Selection == null)
            {
                GUIUtility.ExitGUI();
                return;
            }

            var selected = this.MenuTree.Selection.FirstOrDefault();

            /* 通用工具区 -常驻显示 */
            SirenixEditorGUI.BeginHorizontalToolbar(SirenixGUIStyles.ToolbarBackground, 32);
            {
                Rect temp_btnRect;

                //创建文件
                if (SirenixEditorGUI.ToolbarButton(ProtoHelper.guiContent_panel_createBtn))
                {
                    _CreateNewFilePanelWindow = OdinEditorWindow.InspectObject(this.CurQuickCreateFilePanelDisplay);
                    _CreateNewFilePanelWindow.titleContent = new GUIContent($"创建");
                    _CreateNewFilePanelWindow.position = GUIHelper.GetEditorWindowRect().AlignCenter(500, 200);
                    _CreateNewFilePanelWindow.Focus();
                    _CreateNewFilePanelWindow.OnClose += () =>
                    {
                        //重新初始化创建面板数据
                        CurQuickCreateFilePanelDisplay = new QuickCreateFilePanelDisplay(GetFileOutputPathCallback, SetFileOutputPathCallback, CloseFileCreatePanelCallback, CreateFileFinishCallback);
                    };
                }

                //设置
                if (SirenixEditorGUI.ToolbarButton(ProtoHelper.guiContent_panel_settingBtn))
                {
                    _SettingPanelWindow = OdinEditorWindow.InspectObject(this.CurSettingPanelDisplay);
                    _SettingPanelWindow.titleContent = new GUIContent($"设置");
                    _SettingPanelWindow.position = GUIHelper.GetEditorWindowRect().AlignCenter(500, 500);
                    _SettingPanelWindow.Focus();
                    _SettingPanelWindow.OnBeginGUI += () =>
                    {
                        //鼠标移动时会运行多次
                        if (_SettingPanelWindow != null && EditorWindow.focusedWindow != _SettingPanelWindow)
                        {
                            _SettingPanelWindow.Close();
                            _SettingPanelWindow = null;
                            GUIUtility.ExitGUI(); //在这里添加这一句,报错消失
                        }
                    };
                }

                //关于
                if (SirenixEditorGUI.ToolbarButton(ProtoHelper.guiContent_panel_aboutBtn))
                {
                    _AboutPanelWindow = OdinEditorWindow.InspectObject(this.CurAboutPanelDisplay);
                    _AboutPanelWindow.titleContent = new GUIContent($"关于");
                    _AboutPanelWindow.position = GUIHelper.GetEditorWindowRect().AlignCenter(500, 500);
                    _AboutPanelWindow.Focus();
                    _AboutPanelWindow.OnBeginGUI += () =>
                    {
                        //鼠标移动时会运行多次
                        if (_AboutPanelWindow != null && EditorWindow.focusedWindow != _AboutPanelWindow)
                        {
                            _AboutPanelWindow.Close();
                            _AboutPanelWindow = null;
                            GUIUtility.ExitGUI(); //在这里添加这一句,报错消失
                        }
                    };

                }

                //快照管理
                if (SirenixEditorGUI.ToolbarButton(ProtoHelper.guiContent_panel_cameraBtn))
                {
                    _SnapShootPanelWindow = OdinEditorWindow.InspectObject(this.CurSnapShootPanelDisplay);
                    _SnapShootPanelWindow.titleContent = new GUIContent($"快照集");
                    _SnapShootPanelWindow.position = GUIHelper.GetEditorWindowRect().AlignCenter(600, 500);
                    _SnapShootPanelWindow.Focus();
                    _SnapShootPanelWindow.OnBeginGUI += () =>
                    {
                        //鼠标移动时会运行多次
                        if (_SnapShootPanelWindow != null && EditorWindow.focusedWindow != _SnapShootPanelWindow)
                        {
                            _SnapShootPanelWindow.Close();
                            _SnapShootPanelWindow = null;
                            GUIUtility.ExitGUI(); //在这里添加这一句,报错消失
                        }
                    };
                }

                //所有文件转C#协议
                if (SirenixEditorGUI.ToolbarButton(ProtoHelper.guiContent_panel_2csAllBtn))
                {
                    if (selected != null && (selected is FileMenuItem) && (selected as FileMenuItem).instance != null && (selected as FileMenuItem).instance.IsDrity)
                    {
                        EditorUtility.DisplayDialog("提示", $"当前文件的修改未保存，无法进行转换", "确定");
                    }
                    else if (HasSomeFileIsDrity())
                    {
                        EditorUtility.DisplayDialog("提示", $"无法转换！有些文件的修改未保存，请先保存", "确定");
                    }
                    else
                    {
                        List<OdinMenuItem> menuItems = MenuTree.MenuItems[0].ChildMenuItems;
                        //检查所有文件的错误
                        string error = CheckAllFileError(menuItems);
                        if (!string.IsNullOrEmpty(error))
                        {
                            EditorUtility.DisplayDialog("发现错误", error, "确定");
                            GUIUtility.ExitGUI();
                            return;
                        }


                        if (EditorUtility.DisplayDialog("提示", $"把所有Proto文件[{menuItems.Count}个]都转换为C#协议文件？", "确定", "取消"))
                        {
                            //找到所有文件              
                            List<string> tempList = new List<string>();
                            for (int i = 0; i < menuItems.Count; i++)
                            {
                                var fi = menuItems[i];
                                if (fi is FileMenuItem)
                                {
                                    FileMenuItem item = fi as FileMenuItem;
                                    if (item.instance != null)
                                        tempList.Add(item.instance.FileFullName);
                                }
                            }

                            string exeFullname = Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets")), CurSettingPanelDisplay.SETTING_ExeFullName).Replace('\\', '/');
                            string csOutput = Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets")), CurSettingPanelDisplay.SETTING_CSOutputFolder).Replace('\\', '/');
                            bool succ = ProtoHelper.Proto2CSFile(tempList, exeFullname, csOutput);
                            if (succ)
                            {
                                EditorUtility.DisplayDialog("提示", $"转换功能执行完毕", "确定");
                                // AssetDatabase.Refresh();//删除文件可以刷新资源，新增文件不要轻易刷新
                            }
                            else
                                EditorUtility.DisplayDialog("提示", $"转换发生错误，请查看控制台日志！", "确定");
                        }
                    }
                    GUIUtility.ExitGUI();
                }

                //刷新资源
                if (SirenixEditorGUI.ToolbarButton(ProtoHelper.guiContent_panel_refreshAssetBtn))
                {
                    if (HasSomeFileIsDrity())
                    {
                        EditorUtility.DisplayDialog("提示", $"禁止刷新！因为有些文件的修改未保存，请先保存!\n", "确定");
                        return;
                    }

                    if (EditorUtility.DisplayDialog("提示", $"项目资源将被刷新\n（如果有新增代码资源则会进行编译，未保存的修改可能将丢失）", "刷新", "取消"))
                    {
                        AssetDatabase.Refresh();//刷新资源，新增需要编译的代码文件就不要轻易刷新
                    }
                    GUIUtility.ExitGUI(); //在这里添加这一句,报错消失
                }

            }
            SirenixEditorGUI.EndHorizontalToolbar();

            /* 文件操作命令区 -选中文件时显示 */
            if (selected != null)
            {
                //不需要显示的判断
                if (selected.GetFullPath() == ProtoHelper.MenuTreeRootName || selected.GetFullPath() == ProtoHelper.MenuTreeRootName_Null)
                    return;

                SirenixEditorGUI.BeginHorizontalToolbar(SirenixGUIStyles.Label, 26);
                {
                    FileMenuItem menuItem = selected as FileMenuItem;//这里selected必定不会空

                    GUILayout.Label($" ");

                    //保存功能
                    if (SirenixEditorGUI.ToolbarButton(menuItem.instance.IsDrity ? ProtoHelper.guiContent_panel_saveBtn2 : ProtoHelper.guiContent_panel_saveBtn))
                    {
                        int i = EditorUtility.DisplayDialogComplex("提示", $"是否保存对{menuItem.instance.FileName}的修改？", "检查填写并保存", "取消", "强制保存");
                        if (i == 0)
                        {
                            string error = CheckWrapperAllError(menuItem.instance.EditorInfo);

                            //检查所有文件的冲突（出现所有文件错误解决后才能通过检查）
                            if (string.IsNullOrEmpty(error))
                            {
                                List<OdinMenuItem> menuItems = MenuTree.MenuItems[0].ChildMenuItems;
                                error = CheckAllFileError(menuItems);
                            }

                            if (!string.IsNullOrEmpty(error))
                                EditorUtility.DisplayDialog("提示", $"保存失败！\n{error}", "确定");
                            else
                            {
                                //检查完毕，正常保存
                                SaveFile(menuItem.instance);
                            }

                        }
                        else if (i == 1)
                        {
                            //取消操作
                        }
                        else if (i == 2)
                        {
                            //不检查正常保存
                            SaveFile(menuItem.instance);
                        }
                        else
                            ProtoHelper.LogError("DisplayDialogComplex i error :" + i);
                        GUIUtility.ExitGUI(); //在这里添加这一句,报错消失
                    }

                    //定位文件夹功能
                    if (SirenixEditorGUI.ToolbarButton(ProtoHelper.guiContent_panel_findfolderBtn))
                    {
                        //打开该文件夹
                        FileInfo fileInfo = new FileInfo(menuItem.instance.FileFullName);
                        System.Diagnostics.Process.Start(fileInfo.Directory.FullName);
                    }

                    //预览功能
                    if (SirenixEditorGUI.ToolbarButton(ProtoHelper.guiContent_panel_previewBtn))
                    {
                        //前往编辑内部枚举
                        var window = OdinEditorWindow.InspectObject(new CodePreviewDisplayInfo(menuItem.instance.FileName, ProtoHelper.EditorInfo2ProtoText(menuItem.instance.EditorInfo)));
                        window.titleContent = new GUIContent("预览结果");
                        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);

                    }

                    //检查功能
                    if (SirenixEditorGUI.ToolbarButton(ProtoHelper.guiContent_panel_checkBtn))
                    {
                        List<OdinMenuItem> menuItems = MenuTree.MenuItems[0].ChildMenuItems;

                        //更新所有文件的引用收集数据
                        UpdateAllRefCollectionData(menuItems);

                        //检查该文件所有错误
                        string error = CheckWrapperAllError(menuItem.instance.EditorInfo);

                        //检查所有文件的冲突（出现所有文件错误解决后才能通过检查）
                        if (string.IsNullOrEmpty(error))
                            error = CheckAllFileError(menuItems);

                        if (!string.IsNullOrEmpty(error))
                            EditorUtility.DisplayDialog("提示", error, "确定");
                        else
                            EditorUtility.DisplayDialog("提示", "恭喜，检查已通过！", "确定");
                        GUIUtility.ExitGUI();
                    }

                    //快照功能
                    if (SirenixEditorGUI.ToolbarButton(ProtoHelper.guiContent_fastCamera_checkBtn))
                    {
                        string folder = CurSettingPanelDisplay.SETTING_CachePath;

                        if (string.IsNullOrEmpty(folder))
                        {
                            EditorUtility.DisplayDialog("提示", "无法使用，因为未设置缓存文件目录", "确定");
                            return;
                        }

                        if (!Directory.Exists(folder))
                        {
                            EditorUtility.DisplayDialog("提示", $"无法使用，缓存文件目录不存在:{folder}", "确定");
                            return;
                        }

                        //组装数据
                        string fileName = menuItem.instance.FileName;
                        ProtoEditorInfoWrapper wp = menuItem.instance.EditorInfo;
                        string timeStr = DateTime.Now.ToString("yyyy-MM-dd (HH.mm.ss)");
                        string ssName = string.Format($"SnapShoot {timeStr} from [{fileName}]");
                        SnapShootScriptableObject config = ScriptableObject.CreateInstance<SnapShootScriptableObject>();
                        config.createTime = DateTime.Now.ToString();
                        config.fromFileName = fileName;
                        config.desc = $"此快照是从文件[{fileName}]复制生成";
                        config.wrapper = wp;

                        //保存快照资源
                        string saveFulleName = Path.Combine(folder, $"{ssName}.asset").Replace('\\', '/');
                        AssetDatabase.CreateAsset(config, saveFulleName);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();//刷新资源，新增需要编译的代码文件就不要轻易刷新

                        ProtoHelper.ShowNotification(this, new GUIContent("咔擦 生成快照成功"));
                        GUIUtility.ExitGUI();
                    }

                    //Proto转C#协议功能
                    if (SirenixEditorGUI.ToolbarButton(ProtoHelper.guiContent_2cs_btn))
                    {
                        if (menuItem.instance.IsDrity)
                        {
                            EditorUtility.DisplayDialog("提示", $"当前文件的修改未保存，无法转换！", "确定");
                        }
                        else if (HasSomeFileIsDrity())
                        {
                            EditorUtility.DisplayDialog("提示", $"无法转换！有些文件的修改未保存，请先保存", "确定");
                        }
                        else
                        {
                            //检查所有文件的错误
                            List<OdinMenuItem> menuItems = MenuTree.MenuItems[0].ChildMenuItems;
                            string error = CheckAllFileError(menuItems);
                            if (!string.IsNullOrEmpty(error))
                            {
                                EditorUtility.DisplayDialog("发现错误", error, "确定");
                                return;
                            }

                            if (EditorUtility.DisplayDialog("提示", $"把{menuItem.instance.FileName}转换为C#协议文件？", "确定", "取消"))
                            {
                                List<string> temp = new List<string>() { menuItem.instance.FileFullName };
                                string exeFullname = Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets")), CurSettingPanelDisplay.SETTING_ExeFullName).Replace('\\', '/');
                                string csOutput = Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets")), CurSettingPanelDisplay.SETTING_CSOutputFolder).Replace('\\', '/');
                                bool succ = ProtoHelper.Proto2CSFile(temp, exeFullname, csOutput);
                                if (succ)
                                {
                                    EditorUtility.DisplayDialog("提示", $"转换功能执行完毕", "确定");
                                    // AssetDatabase.Refresh();//刷新资源，新增需要编译的代码文件就不要轻易刷新
                                }
                                else
                                    EditorUtility.DisplayDialog("提示", $"转换发生错误，请查看控制台日志！", "确定");
                            }
                        }
                        GUIUtility.ExitGUI();
                    }

                }
                SirenixEditorGUI.EndHorizontalToolbar();
            }

        }


        #region 树形菜单-文件

        /// <summary>
        /// 文件ITEM 
        /// </summary>
        private class FileMenuItem : OdinMenuItem
        {
            public readonly FileDisplayInfo instance;

            private Action<string> deleteCallback;

            public FileMenuItem(OdinMenuTree tree, FileDisplayInfo instance, Action<string> pDeleteCallback) : base(tree, instance.FileName, instance)
            {
                this.instance = instance;
                deleteCallback = pDeleteCallback;
                SdfIcon = instance.IsDrity ? SdfIconType.FileEarmarkText : SdfIconType.FileEarmarkTextFill;
            }

            /// <summary>
            /// （Update）
            /// </summary>
            /// <param name="rect"></param>
            /// <param name="labelRect"></param>
            protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
            {
                base.OnDrawMenuItem(rect, labelRect);

                Rect r;
                //脏标记
                if (instance.IsDrity)
                {
                    r = new Rect(rect.x + 4, rect.y + 5, 14, 14);
                    SirenixEditorGUI.SDFIconButton(r, SdfIconType.Asterisk, ProtoHelper.guiStyle_2);
                }

                //删除文件按钮 (SirenixEditorGUI)
                r = new Rect(rect.x + rect.width - 20, rect.y + 5, 14, 14);
                if (SirenixEditorGUI.SDFIconButton(r, SdfIconType.TrashFill, ProtoHelper.guiStyle_1))
                {
                    if (EditorUtility.DisplayDialog("注意", $"是否删除 {instance.FileName} ？（删除文件后将无法恢复）\n{instance.FileFullName}", "删除", "取消"))
                    {
                        deleteCallback?.Invoke(instance.FileFullName);
                    }
                    GUIUtility.ExitGUI(); //在这里添加这一句,报错消失
                }

            }
        }

        /// <summary>
        /// File Display Info
        /// </summary>
        public class FileDisplayInfo
        {
            public FileDisplayInfo(string pFileName, string pFileFullName)
            {
                FileName = pFileName;
                FileFullName = pFileFullName;
            }

            /// <summary>
            /// 脏（数据修改后的未保存状态） 
            /// </summary>
            [HideInInspector]
            public bool IsDrity = false;

            /// <summary>
            /// 文件名（带后缀）
            /// </summary>
            [InfoBox("文件已被修改", InfoMessageType.Warning, VisibleIf = "IsDrity", GUIAlwaysEnabled = true), LabelText("文件名", SdfIconType.GeoAltFill, IconColor = "#B66DFD"), LabelWidth(60), DisableContextMenu, ReadOnly, PropertyOrder(0)]
            public string FileName;

            /// <summary>
            /// 文件全路径
            /// </summary>
            [HorizontalGroup("HG2", PaddingLeft = 34), LabelText("", SdfIconType.Link), LabelWidth(24), DisplayAsString, ReadOnly, PropertyOrder(1)]
            public string FileFullName;


            [BoxGroup("Tool", ShowLabel = false), ShowInInspector, PropertyOrder(3), LabelText("显示填写提示"), LabelWidth(80)]
            private bool _ShowSomeTip { get { return ProtoHelper.ShowEditorInfoTipToggle; } set { ProtoHelper.ShowEditorInfoTipToggle = value; } }

            /// <summary>
            /// 包装好的源数据 
            ///     - 后续的修改都是在修改此引用
            ///     - 需要时才主动加载出来
            ///     - 可能为Null
            /// </summary>
            [BoxGroup("BG2", ShowLabel = false), HideLabel, PropertyOrder(5), ShowIf("EditorInfo")]
            public ProtoEditorInfoWrapper EditorInfo;




            #region 内部方法

            /// <summary>
            /// 读取文件并反转换为EditorInfo
            ///     -需要读取时才读取
            ///     -脏 时不允许读取
            ///     -读取后如果还是为null则表示有错误
            ///     -新文件读取后并不会返回null
            /// </summary>
            public void ReadInfo()
            {
                if (IsDrity == false)
                    ForceReadInfo();
                else
                {
                    ProtoHelper.Log("FileDisplayInfo IsDrity = true , so can't Invoke ReadInfo( )");
                }
            }

            /// <summary>
            /// 读取文件并反转换为EditorInfo
            /// </summary>
            public void ForceReadInfo()
            {
                EditorInfo = ProtoHelper.FromProtoFile(FileFullName);
            }


            /// <summary>
            /// 设置保存时的处理
            /// </summary>
            public void SetSave()
            {
                IsDrity = false;
            }

            #endregion

        }

        /// <summary>
        /// ROOT Display Info
        /// </summary>
        public class ROOTDisplayInfo
        {
            [BoxGroup("G", ShowLabel = false), DisplayAsString, HideLabel, GUIColor("#FFF5D8")]
            public string Tip = "* 目前没找到任何文件，尝试【创建新文件】或检查【设置】面板中的<输出目录>是否正确！";

            [BoxGroup("G2", ShowLabel = false), DisplayAsString, HideLabel, GUIColor("#FFF5D8")]
            public string Tip2 = "* 无法解决？前往【关于】面板中查看使用说明或联系反馈！";
        }

        #endregion

        #region 方法

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="info"></param>
        void SaveFile(FileDisplayInfo info)
        {
            bool succ = ProtoHelper.ToProtoFile(info.EditorInfo, info.FileFullName);
            if (succ)
            {
                ProtoHelper.ShowNotification(this, new GUIContent("文件保存成功"));
                info.SetSave();//消除脏标记
                UpdateRefCollectionData(info, true);//强制刷新
            }
            else
                EditorUtility.DisplayDialog("提示", $"{info.FileName} 保存失败！", "确定");
            //GUIUtility.ExitGUI(); //在这里添加这一句,报错消失
        }

        /// <summary>
        /// 全局检查单个文件的错误
        /// </summary>
        /// <param name="info"></param>
        /// <returns> 返回错误字符串 </returns>
        string CheckWrapperAllError(ProtoEditorInfoWrapper info)
        {
            StringBuilder sb = new StringBuilder();

            if (!info.CheckImportFile())
                sb.AppendLine("引用文件的填写发现问题，请检查！");
            if (!info.CheckEnumList())
                sb.AppendLine("枚举的填写发现问题，请检查！");
            if (!info.CheckMessageList())
                sb.AppendLine("消息体的填写发现问题，请检查！");
            if (!info.CheckSelf())
                sb.AppendLine("内容填写发现问题，请检查！");
            return sb.ToString();
        }

        /// <summary>
        /// 检查所有文件是否有文件还处于被修改状态
        /// </summary>
        /// <returns></returns>
        static bool HasSomeFileIsDrity()
        {
            if (Instance == null || Instance.MenuTree.MenuItems[0] == null)
            {
                ProtoHelper.LogError("HasSomeFileIsDrity Error");
                return false;
            }

            List<OdinMenuItem> menuItems = Instance.MenuTree.MenuItems[0].ChildMenuItems;

            foreach (var mit in menuItems)
            {
                if (mit is FileMenuItem)
                {
                    FileMenuItem item = mit as FileMenuItem;
                    if (item.instance != null)
                    {
                        if (item.instance.IsDrity)
                            return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// 检查所有文件的错误
        /// </summary>
        /// <returns></returns>
        static string CheckAllFileError(List<OdinMenuItem> menuItems)
        {
            UpdateAllRefCollectionData(menuItems);

            Dictionary<string, string> messageList = new Dictionary<string, string>();  //key消息体名 value相关文件名
            Dictionary<string, string> enumList = new Dictionary<string, string>();     //key枚举类型名 value相关文件名

            //检查重复类或重复枚举的情况(该项检查前请更新所有引用收集字典 ProtoMenuEditorWindow.UpdateAllRefCollectionData)
            foreach (var mit in menuItems)
            {
                if (mit is FileMenuItem)
                {
                    FileMenuItem item = mit as FileMenuItem;
                    if (item.instance != null)
                    {
                        string nameSpace = item.instance.EditorInfo.PackageName;

                        foreach (var i in item.instance.EditorInfo.MessageInfoList)
                        {
                            string m_name = $"{nameSpace}{i.Name}";
                            if (messageList.ContainsKey(m_name))
                            {
                                string str = $"发现消息体重名冲突：[{item.instance.FileName}文件中的{m_name}] 与 [{messageList[m_name]}文件中的{m_name}]";
                                ProtoHelper.LogErrorInfo(str);
                                return str;
                            }
                            else
                                messageList.Add(m_name, item.instance.FileName);
                        }

                        foreach (var i in item.instance.EditorInfo.EnumInfoList)
                        {
                            string e_name = $"{nameSpace}{i.TypeName}";
                            if (enumList.ContainsKey(e_name))
                            {
                                string str = $"发现枚举类型重名冲突：[{item.instance.FileName}文件中的{e_name}] 与 [{enumList[e_name]}文件中的{e_name}]";
                                ProtoHelper.LogErrorInfo(str);
                                return str;
                            }
                            else
                                enumList.Add(e_name, item.instance.FileName);
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 检查所有文件的错误
        /// </summary>
        /// <returns></returns>
        static string CheckAllFileError()
        {
            if (Instance == null)
                return "无法检查，编辑器错误，请重新打开窗口";
            return CheckAllFileError(Instance.MenuTree.MenuItems[0].ChildMenuItems);
        }


        /// <summary>
        /// (刷新)重新搜索所有PROTO文件-（注意已被修改但未保存的文件不要进行重新读取）
        /// </summary>
        public void RefreshAllProtoFiles(bool showTip = false)
        {

            //从输出文件夹中找到所有.proto文件
            List<string> fullNames;
            List<string> names;
            string folderPath = Path.Combine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets")), CurSettingPanelDisplay.SETTING_ProtoOutputFolderPath).Replace('\\', '/');
            int foundCount = ProtoHelper.FindAllFiles(folderPath, out fullNames, out names, ".proto");
            ProtoHelper.LogInfo($"重新查找所有文件,在{folderPath}中找到{foundCount}个文件");

            //旧数据备份并清理
            List<FileDisplayInfo> _oldInfos = new List<FileDisplayInfo>();
            _oldInfos.AddRange(CurAllFileList);
            CurAllFileList.Clear();

            List<FileDisplayInfo> temps = new List<FileDisplayInfo>();
            for (int i = 0; i < fullNames.Count; i++)
            {
                FileDisplayInfo tempInfo;

                //如果找到旧数据就赋值旧数据
                FileDisplayInfo oldInfo = _oldInfos.Find(e => e.FileFullName == fullNames[i]);
                if (oldInfo != null)
                {
                    //如果脏了就不要用新数据覆盖了
                    if (oldInfo.IsDrity)
                    {
                        tempInfo = oldInfo;
                    }
                    //没脏就重新New吧(其实不重新New应该也行)
                    else
                    {
                        tempInfo = new FileDisplayInfo(names[i], fullNames[i])
                        {
                            IsDrity = false,
                        };
                    }
                }
                //如果没有旧数据就是新增的文件
                else
                {
                    tempInfo = new FileDisplayInfo(names[i], fullNames[i])
                    {
                        IsDrity = false,
                    };
                }

                temps.Add(tempInfo);
            }
            CurAllFileList.AddRange(temps);

            if (showTip)
                ProtoHelper.ShowNotification(this, new GUIContent($"发现{foundCount}个.proto文件"));

            //强制刷新树形菜单
            ForceMenuTreeRebuild();

        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fullname"> 全路径 </param>
        private void DeleteFileCallback(string fullname)
        {
            if (string.IsNullOrEmpty(fullname))
            {
                ProtoHelper.LogErrorInfo($"删除文件失败，fullname：{fullname}");
                return;
            }

            if (!File.Exists(fullname))
            {
                ProtoHelper.LogErrorInfo($"删除文件失败，文件不存在：{fullname}");
                return;
            }

            string pFile = System.IO.Path.GetFileName(fullname); //获取文件名

            File.Delete(fullname);
            ProtoHelper.LogInfo($"删除文件成功：{fullname}");
            ProtoHelper.ShowNotification(this, new GUIContent($"删除文件成功"));

            RefreshAllProtoFiles();
            AssetDatabase.Refresh();//刷新资源，新增需要编译的代码文件就不要轻易刷新

            TryRemoveRefCollectionData(pFile);
        }

        /// <summary>
        /// Panel ShowNotification 回调
        /// </summary>
        /// <param name="panel"> 为Null时调用this窗口 </param>
        /// <param name="content"></param>
        private void ShowNotificationForPanelCallback(ProtoHelper.BasePanelDisplay panel, string content)
        {
            OdinEditorWindow win = null;
            if (panel is QuickCreateFilePanelDisplay)
                win = _CreateNewFilePanelWindow;
            else if (panel is SettingPanelDisplay)
                win = _SettingPanelWindow;
            else if (panel is AboutPanelDisplay)
                win = _AboutPanelWindow;
            else if (panel is SnapShootPanelDisplay)
                win = _SnapShootPanelWindow;

            if (win != null)
                ProtoHelper.ShowNotification(win, new GUIContent(content));
            else
                ProtoHelper.ShowNotification(this, new GUIContent(content));

        }


        /// <summary>
        /// MenuTree 的 MenuItem 发生变化
        /// </summary>
        /// <param name="type"></param>
        private void OnMenuTreeSelectionChanged(SelectionChangedType type)
        {
            if (type == SelectionChangedType.ItemAdded)
            {
                //当前选中的MenuItem的值数 据
                if (MenuTree.Selection.SelectedValue is FileDisplayInfo)
                {
                    FileDisplayInfo fi = MenuTree.Selection.SelectedValue as FileDisplayInfo;

                    //1.sLastSelecetedMenuItemData为Null时 可能是因为编译重刷了，所以要强制读取，否则编辑器会报错
                    //2.sLastSelecetedMenuItemData为Null时 也可能未选择过MenuItem，这样强制读取也没有任何问题 
                    if (ProtoMenuEditorWindow.sLastSelecetedMenuItemData == null)
                        fi.ForceReadInfo();
                    else
                        fi.ReadInfo();
                    ProtoMenuEditorWindow.sLastSelecetedMenuItemData = fi;

                    UpdateRefCollectionData(fi, false);
                }
            }
        }


        #endregion

        #region 静态方法

        /// <summary>
        /// 更新引用数据收集字典
        /// </summary>
        /// <param name="menuItems"></param>
        public static void UpdateAllRefCollectionData(List<OdinMenuItem> menuItems)
        {
            //更新所有文件的引用收集数据
            EditorUtility.DisplayProgressBar("正在检查", "深度检查中...", 0f);
            for (int i = 0; i < menuItems.Count; i++)
            {
                EditorUtility.DisplayProgressBar("正在检查", "深度检查中...", 1f * ((float)i / (float)menuItems.Count));
                var temp = menuItems[i];
                if (temp is FileMenuItem)
                {
                    FileMenuItem item = temp as FileMenuItem;
                    if (item.instance != null)
                        UpdateRefCollectionData(item.instance, true);
                }
            }
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 更新引用数据收集字典
        /// </summary>
        /// <param name="fdi"></param>
        /// <param name="force"> 强制更新 设为false时只在不存在key时更新  </param>
        public static bool UpdateRefCollectionData(FileDisplayInfo fdi, bool force)
        {
            if (fdi == null)
                return false;

            //如果某FileDisplayInfo的EditorInfo为null，证明还没ReadInfo()过，所以要ReadInfo()！
            if (fdi.EditorInfo == null)
                fdi.ReadInfo();

            if (!s_RefCollectionDataCacheDic.ContainsKey(fdi.FileName))
            {
                var data = RefCollectionData.Parse(fdi);
                if (data != null)
                {
                    s_RefCollectionDataCacheDic.Add(fdi.FileName, data);
                    ProtoHelper.Log("UpdateRefCollectionData 成功" + fdi.FileName);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (force)
                {
                    ProtoHelper.Log("UpdateRefCollectionData 成功" + fdi.FileName);
                    var data = RefCollectionData.Parse(fdi);
                    if (data != null)
                    {
                        s_RefCollectionDataCacheDic[fdi.FileName] = data;
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// 更新引用数据收集字典
        /// </summary>
        /// <param name="fileName">文件名带后缀</param>
        /// <param name="force"></param>
        public static void UpdateRefCollectionData(string fileName, bool force)
        {
            OdinMenuItem item = Instance.MenuTree.GetMenuItem($"{ProtoHelper.MenuTreeRootName}/{fileName}");
            if (item is FileMenuItem)
            {
                FileMenuItem i = item as FileMenuItem;
                UpdateRefCollectionData(i.instance, force);
            }
        }

        /// <summary>
        /// 获得引用数据收集字典数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static RefCollectionData GetRefCollectionData(string fileName)
        {
            RefCollectionData temp;
            if (s_RefCollectionDataCacheDic.TryGetValue(fileName, out temp))
            {
                return temp;
            }
            else
            {
                ProtoHelper.LogError("Get RefCollection 失败:" + fileName);
                return null;
            }
        }

        /// <summary>
        /// 尝试删除数据
        /// </summary>
        /// <param name="fileName"></param>
        static void TryRemoveRefCollectionData(string fileName)
        {
            if (s_RefCollectionDataCacheDic.ContainsKey(fileName))
                s_RefCollectionDataCacheDic.Remove(fileName);
        }

        #endregion
    }
}

