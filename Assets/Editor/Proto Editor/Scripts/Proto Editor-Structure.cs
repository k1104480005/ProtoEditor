using System;
using System.Collections.Generic;

/// <summary>
/// ����ṹ
/// -Protoע��
///   - �ֶα����ͬ��Ϣ���в����ظ������Ϊ1��15���ֶ��ڱ���ʱ���õ��Ż�����������repeated�ֶα��λ��1��15֮��
///   - ö��ֵ֮��ķָ����ǷֺŶ����Ƕ��ţ�ö��ֵ��Ӧ����ʵ������ֵ
///   - ʹ������.proto�ļ�������ʱʹ��Improt���ã����� import "CommonMessages.proto"
///   - int32��uint32��int64��uint64��bool������֮���Ǽ��ݵģ�sint32��sint64�Ǽ��ݵģ�string��bytes�Ǽ��ݵģ�fixed32��sfixed32���Լ�fixed64��sfixed64֮���Ǽ��ݵģ�optional��repeated�޶���Ҳ���໥���ݵ�
///   - ����ʹ���﷨MessageType.EnumType������Ϣ����������ö����������������Ϣ�����ֶε�����  
/// 
///   -�����б��빤�ߣ����� protoc --proto_path=IMPORT_PATH --cpp_out=DST_DIR --java_out=DST_DIR --python_out=DST_DIR path/to/file.proto
///      1.--proto_path��ͬ��-Iѡ���Ҫ����ָ���������.proto��Ϣ�����ļ����ڵ�Ŀ¼����ѡ����Ա�ͬʱָ�����
///      2.--cpp_outѡ���ʾ����C++���룬--java_out��ʾ����Java���룬--python_out���ʾ����Python���룬����Ŀ¼Ϊ���ɺ�Ĵ�������ŵ�Ŀ¼
///      3.path/to/file.proto��ʾ���������Ϣ�����ļ�
/// </summary>
namespace EditProto
{
    /// <summary>
    /// Proto Editor Info���� ���������ݣ�
    /// <Wrapper��װ�� cref="ProtoEditorInfoWrapper"/>.
    /// </summary>
    public class ProtoEditorInfo
    {
        /// <summary>
        /// �﷨Э�����
        /// </summary>
        public YntaxType Yntax;

        /// <summary>
        /// �����ռ�
        /// </summary>
        public string PackageName;

        /// <summary>
        /// ���������ļ����ļ���
        /// </summary>
        public List<string> ImportProtoFileName = new List<string>();

        /// <summary>
        /// �������Ϣ���б�
        /// </summary>
        public List<MessageInfo> MessageInfoList = new List<MessageInfo>();

        /// <summary>
        /// �����ö���б����ڹ���ö�٣�
        /// </summary>
        public List<EnumInfo> EnumInfoList = new List<EnumInfo>();
    }

    /// <summary>
    /// ������Ϣ��
    /// <Wrapper��װ�� cref="MessageInfoWrapper"/>.
    /// 
    /// </summary>
    public class MessageInfo
    {
        /// <summary>
        ///  ��Ϣ����
        /// </summary>
        public string name = "";

        /// <summary>
        /// ע��
        /// </summary>
        public string annotation = "";

        /// <summary>
        /// ��Ϣ�����ֶ�
        /// </summary>
        public List<FieldsInfo> fieldsList = new List<FieldsInfo>(0);

        /// <summary>
        /// ��Ϣ���ڵ�ö�ٶ��壨�����飩
        /// </summary>
        public List<EnumInfo> enumInfoList = new List<EnumInfo>();
    }

    /// <summary>
    /// ������Ϣ���ڵ��ֶ�
    /// <Wrapper��װ�� cref="FieldsInfoWrapper"/>.
    /// </summary>
    public class FieldsInfo
    {
        public FieldsInfo(uint pCode)
        {
            modifier = ModifierType.None;
            type = FieldsType.String;
            customTypeName = "";
            name = "";
            annotation = "";
            code = pCode;
        }

        /// <summary>
        /// �ֶα��
        /// </summary>
        public uint code;

        /// <summary>
        /// ���η�
        /// </summary>
        public ModifierType modifier;

        /// <summary>
        /// ����
        /// </summary>
        public FieldsType type;

        /// <summary>
        /// �Զ����������
        /// </summary>
        public string customTypeName="";

        /// <summary>
        /// �ֶ���
        /// </summary>
        public string name="";

        /// <summary>
        /// �ֶ�ע��
        /// </summary>
        public string annotation;

    }

    /// <summary>
    /// ����ö��
    /// <Wrapper��װ�� cref="EnumInfoWrapper"/>.
    /// </summary>
    public class EnumInfo
    {
        public EnumInfo(string pTypeName)
        {
            typeName = pTypeName;
            annotation = "";
        }

        /// <summary>
        /// ö����������
        /// </summary>
        public string typeName;

        /// <summary>
        /// ö��ע��
        /// </summary>
        public string annotation;

        /// <summary>
        /// ö��ֵ�б�
        /// </summary>
        public List<EnumValueInfo> valueList = new List<EnumValueInfo>(0);
    }

    /// <summary>
    /// ����ö��ֵ
    /// <Wrapper��װ�� cref="EnumValueInfoWrapper"/>.
    /// </summary>
    public class EnumValueInfo
    {
        public EnumValueInfo(string pValueName)
        {
            valueName = pValueName;
            annotation = "";
        }

        /// <summary>
        /// ö��ֵ������
        /// </summary>
        public string valueName;

        /// <summary>
        /// ö��ֵ
        /// </summary>
        public int value;

        /// <summary>
        /// ע��
        /// </summary>
        public string annotation;
    }


    /// <summary>
    /// ģ���ļ���JSON��
    /// </summary>
    public class TemplateJsonInfo
    {
        /// <summary>
        /// ģ����
        /// </summary>
        public string TemplateName;

        /// <summary>
        /// ģ������
        /// </summary>
        public string TemplateDesc;

        /// <summary>
        /// Editor Info
        /// </summary>
        public ProtoEditorInfo data;
    }

    /// <summary>
    /// ���η�����
    /// </summary>
    public enum ModifierType
    {
        /// <summary>
        /// ��
        /// </summary>
        None,

        /// <summary>
        /// ���ظ��ֶ�
        /// </summary>
        Repeated,

        /// <summary>
        /// �����ֶΣ�proto3ȡ����
        /// </summary>
        //Required,
        /// <summary>
        /// ��ѡ�ֶΣ�proto3ȡ����
        /// </summary>
        //Optional
    }

    /// <summary>
    /// �ֶ�����
    /// </summary>
    public enum FieldsType
    {
        Double,
        Float,
        Int32,      //ʹ�ÿɱ䳤���뷽ʽ������ʱ������ЧӦ��ʹ��sint32
        Uint32,     //ʹ�ÿɱ䳤���뷽ʽ
        Sint32,     //ʹ�ÿɱ䳤���뷽ʽ���з��ŵ�����ֵ����������ʱ��ͨ����int32��Ч
        Int64,
        Uint64,
        Sint64,
        Bool,
        String,     //һ���ַ���������utf-8�������7-bit��ascii������ı�
        Custom,         //�Զ�����д ���Զ������Ϣ�� ���Զ����ö��
    }

    /// <summary>
    /// �﷨Э�����ö��
    /// </summary>
    public enum YntaxType
    {
        proto3,     //ǿ���Ƽ�
       // proto2,
    }


}