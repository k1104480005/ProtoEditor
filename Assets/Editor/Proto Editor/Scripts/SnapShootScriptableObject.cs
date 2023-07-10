using EditProto;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

/// <summary>
/// 快照的保存结构
/// </summary>
public class SnapShootScriptableObject : ScriptableObject
{

    /// <summary>
    /// 来源名称
    /// </summary>
    [ReadOnly, LabelText("创建依据文件")]
    public string fromFileName;

    /// <summary>
    /// 创建时间
    /// </summary>
    [ReadOnly, LabelText("创建时间")]
    public string createTime;

    /// <summary>
    /// 快照描述
    /// </summary>
    [TextArea, LabelText("注释")]
    public string desc;



    /// <summary>
    /// 快照数据
    /// </summary>
    [HideInInspector]
    public ProtoEditorInfoWrapper wrapper;
}