using System;
using System.Collections.Generic;

/// <summary>
/// 定义结构
/// -Proto注意
///   - 字段编号在同消息体中不能重复，编号为1到15的字段在编码时将得到优化，尽可能让repeated字段编号位于1到15之间
///   - 枚举值之间的分隔符是分号而不是逗号，枚举值对应的是实际整型值
///   - 使用其他.proto文件的内容时使用Improt引用，例如 import "CommonMessages.proto"
///   - int32、uint32、int64、uint64和bool等类型之间是兼容的，sint32和sint64是兼容的，string和bytes是兼容的，fixed32和sfixed32，以及fixed64和sfixed64之间是兼容的，optional和repeated限定符也是相互兼容的
///   - 可以使用语法MessageType.EnumType，将消息体中声明的枚举类型用作其他消息体中字段的类型  
/// 
///   -命令行编译工具，例如 protoc --proto_path=IMPORT_PATH --cpp_out=DST_DIR --java_out=DST_DIR --python_out=DST_DIR path/to/file.proto
///      1.--proto_path等同于-I选项，主要用于指定待编译的.proto消息定义文件所在的目录，该选项可以被同时指定多个
///      2.--cpp_out选项表示生成C++代码，--java_out表示生成Java代码，--python_out则表示生成Python代码，其后的目录为生成后的代码所存放的目录
///      3.path/to/file.proto表示待编译的消息定义文件
/// </summary>
namespace EditProto
{
    /// <summary>
    /// Proto Editor Info数据 （核心数据）
    /// <Wrapper包装器 cref="ProtoEditorInfoWrapper"/>.
    /// </summary>
    public class ProtoEditorInfo
    {
        /// <summary>
        /// 语法协议规则
        /// </summary>
        public YntaxType Yntax;

        /// <summary>
        /// 命名空间
        /// </summary>
        public string PackageName;

        /// <summary>
        /// 引用其他文件的文件名
        /// </summary>
        public List<string> ImportProtoFileName = new List<string>();

        /// <summary>
        /// 定义的消息体列表
        /// </summary>
        public List<MessageInfo> MessageInfoList = new List<MessageInfo>();

        /// <summary>
        /// 定义的枚举列表（包内公共枚举）
        /// </summary>
        public List<EnumInfo> EnumInfoList = new List<EnumInfo>();
    }

    /// <summary>
    /// 定义消息体
    /// <Wrapper包装器 cref="MessageInfoWrapper"/>.
    /// 
    /// </summary>
    public class MessageInfo
    {
        /// <summary>
        ///  消息体名
        /// </summary>
        public string name = "";

        /// <summary>
        /// 注释
        /// </summary>
        public string annotation = "";

        /// <summary>
        /// 消息体内字段
        /// </summary>
        public List<FieldsInfo> fieldsList = new List<FieldsInfo>(0);

        /// <summary>
        /// 消息体内的枚举定义（不建议）
        /// </summary>
        public List<EnumInfo> enumInfoList = new List<EnumInfo>();
    }

    /// <summary>
    /// 定义消息体内的字段
    /// <Wrapper包装器 cref="FieldsInfoWrapper"/>.
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
        /// 字段编号
        /// </summary>
        public uint code;

        /// <summary>
        /// 修饰符
        /// </summary>
        public ModifierType modifier;

        /// <summary>
        /// 类型
        /// </summary>
        public FieldsType type;

        /// <summary>
        /// 自定义的类型名
        /// </summary>
        public string customTypeName="";

        /// <summary>
        /// 字段名
        /// </summary>
        public string name="";

        /// <summary>
        /// 字段注释
        /// </summary>
        public string annotation;

    }

    /// <summary>
    /// 定义枚举
    /// <Wrapper包装器 cref="EnumInfoWrapper"/>.
    /// </summary>
    public class EnumInfo
    {
        public EnumInfo(string pTypeName)
        {
            typeName = pTypeName;
            annotation = "";
        }

        /// <summary>
        /// 枚举类型名称
        /// </summary>
        public string typeName;

        /// <summary>
        /// 枚举注释
        /// </summary>
        public string annotation;

        /// <summary>
        /// 枚举值列表
        /// </summary>
        public List<EnumValueInfo> valueList = new List<EnumValueInfo>(0);
    }

    /// <summary>
    /// 定义枚举值
    /// <Wrapper包装器 cref="EnumValueInfoWrapper"/>.
    /// </summary>
    public class EnumValueInfo
    {
        public EnumValueInfo(string pValueName)
        {
            valueName = pValueName;
            annotation = "";
        }

        /// <summary>
        /// 枚举值的名称
        /// </summary>
        public string valueName;

        /// <summary>
        /// 枚举值
        /// </summary>
        public int value;

        /// <summary>
        /// 注释
        /// </summary>
        public string annotation;
    }


    /// <summary>
    /// 模板文件（JSON）
    /// </summary>
    public class TemplateJsonInfo
    {
        /// <summary>
        /// 模板名
        /// </summary>
        public string TemplateName;

        /// <summary>
        /// 模板描述
        /// </summary>
        public string TemplateDesc;

        /// <summary>
        /// Editor Info
        /// </summary>
        public ProtoEditorInfo data;
    }

    /// <summary>
    /// 修饰符类型
    /// </summary>
    public enum ModifierType
    {
        /// <summary>
        /// 无
        /// </summary>
        None,

        /// <summary>
        /// 可重复字段
        /// </summary>
        Repeated,

        /// <summary>
        /// 必需字段（proto3取消）
        /// </summary>
        //Required,
        /// <summary>
        /// 可选字段（proto3取消）
        /// </summary>
        //Optional
    }

    /// <summary>
    /// 字段类型
    /// </summary>
    public enum FieldsType
    {
        Double,
        Float,
        Int32,      //使用可变长编码方式，负数时不够高效应该使用sint32
        Uint32,     //使用可变长编码方式
        Sint32,     //使用可变长编码方式，有符号的整型值，负数编码时比通常的int32高效
        Int64,
        Uint64,
        Sint64,
        Bool,
        String,     //一个字符串必须是utf-8编码或者7-bit的ascii编码的文本
        Custom,         //自定义填写 或自定义的消息体 或自定义的枚举
    }

    /// <summary>
    /// 语法协议规则枚举
    /// </summary>
    public enum YntaxType
    {
        proto3,     //强烈推荐
       // proto2,
    }


}