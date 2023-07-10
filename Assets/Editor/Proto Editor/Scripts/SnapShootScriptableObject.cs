using EditProto;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

/// <summary>
/// ���յı���ṹ
/// </summary>
public class SnapShootScriptableObject : ScriptableObject
{

    /// <summary>
    /// ��Դ����
    /// </summary>
    [ReadOnly, LabelText("���������ļ�")]
    public string fromFileName;

    /// <summary>
    /// ����ʱ��
    /// </summary>
    [ReadOnly, LabelText("����ʱ��")]
    public string createTime;

    /// <summary>
    /// ��������
    /// </summary>
    [TextArea, LabelText("ע��")]
    public string desc;



    /// <summary>
    /// ��������
    /// </summary>
    [HideInInspector]
    public ProtoEditorInfoWrapper wrapper;
}