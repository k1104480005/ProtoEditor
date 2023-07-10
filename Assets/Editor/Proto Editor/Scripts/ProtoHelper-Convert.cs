using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace EditProto
{
    /// <summary>
    /// 助手类 - 转换与反转换
    /// </summary>
    public static partial class ProtoHelper
    {
        const string linefeed1 = "\r\n";      //1次换行
        const string linefeed2 = "\r\n\r\n";  //2次换行
        const string tab1 = "\t";             //制表符

        /// <summary>
        /// 转换成 Proto文件
        /// </summary>
        /// <param name="wrapper"></param>
        /// <param name="fileFullname"> 全路径 </param>
        /// <returns></returns>
        public static bool ToProtoFile(ProtoEditorInfoWrapper wrapper, string fileFullname)
        {
            string pFolder = System.IO.Path.GetDirectoryName(fileFullname); //获取文件路径
            string pFile = System.IO.Path.GetFileName(fileFullname); //获取文件名
            return ToProtoFile(wrapper, pFile, pFolder);
        }

        /// <summary>
        /// 转换成 Proto文件
        /// </summary>
        /// <param name="wrapper"> wrapper </param>
        /// <param name="fileName"> 文件名带后缀 </param>
        /// <param name="folderPath"> 输出文件夹路径 </param>
        /// <returns></returns>
        public static bool ToProtoFile(ProtoEditorInfoWrapper wrapper, string fileName, string folderPath)
        {
            string error = null;

            if (string.IsNullOrEmpty(fileName) || !fileName.EndsWith(".proto"))
            {
                error = $"SaveProtoFile error fileName:{fileName}";
                ProtoHelper.LogError(error);
                return false;
            }

            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                error = $"SaveProtoFile error folderPath:{folderPath}";
                ProtoHelper.LogError(error);
                return false;
            }

            string protoText = EditorInfo2ProtoText(wrapper);
            if (string.IsNullOrEmpty(protoText))
            {
                error = "SaveProtoFile EditorInfo to ProtoText Failed! - protoText is null";
                ProtoHelper.LogError(error);
                return false;
            }

            string info;
            string fullName = Path.Combine(folderPath, fileName).Replace('\\', '/');
            if (File.Exists(fullName))
                info = $"[覆盖保存]";
            else
                info = $"[保存]";

            File.WriteAllText(fullName, protoText);

            ProtoHelper.Log(string.Format($"{info} {fullName}"));

            //刷新(假设路径在工程内 可以避免手动刷新才看到)
            UnityEditor.AssetDatabase.Refresh();

            ////打开该文件夹
            //FileInfo fileInfo = new FileInfo(info.FullName);
            //System.Diagnostics.Process.Start(fileInfo.Directory.FullName);

            return true;
        }

        /// <summary>
        /// 反转换 Proto文件
        /// </summary>
        /// <param name="fileFullName"> 文件全路径 </param>
        /// <returns> 错误时返回Null </returns>
        public static ProtoEditorInfoWrapper FromProtoFile(string fileFullName)
        {
            return Proto2TextEditorInfoWrapper(fileFullName);
        }

        #region 转换

        /// <summary>
        /// EditorInfo 转换为 Proto Text
        /// </summary>
        /// <param name="infoWrapper"></param>
        /// <returns> 返回Proto Text </returns>
        public static string EditorInfo2ProtoText(ProtoEditorInfoWrapper infoWrapper)
        {
            if (infoWrapper == null)
                return null;

            return EditorInfo2ProtoText(infoWrapper.Info);
        }

        /// <summary>
        /// EditorInfo 转换为 Proto Text
        /// </summary>
        /// <param name="info"> Editor Info </param>
        /// <returns>返回ProtoProto Text</returns>
        public static string EditorInfo2ProtoText(ProtoEditorInfo info)
        {
            if (info == null)
                return null;

            StringBuilder outSb = new StringBuilder();

            //语法协议版本
            outSb.Append($"syntax = \"{info.Yntax}\";{linefeed2}");

            //命名空间(包名)
            if (!string.IsNullOrEmpty(info.PackageName))
                outSb.Append($"package {info.PackageName};{linefeed2}");

            //引用其他文件(.Proto) 例如:import public "proto_cs_common.proto";
            if (info.ImportProtoFileName.Count > 0)
            {
                foreach (var name in info.ImportProtoFileName)
                    outSb.Append($"import public \"{name}\";{linefeed1}");
                outSb.Append($"{linefeed2}");
            }

            //消息体
            foreach (var msg in info.MessageInfoList)
            {
                string msgText = EI2PT_MessageInfo(msg);
                if (string.IsNullOrEmpty(msgText))
                    return null;
                else
                    outSb.Append(msgText.ToString());
            }

            //枚举
            // Log("[转换]info.EnumInfoList.count:" + info.EnumInfoList.Count);
            foreach (var eiinfo in info.EnumInfoList)
            {
                string enumText = EI2PT_EnumInfo(eiinfo);
                if (string.IsNullOrEmpty(enumText))
                {
                    LogError("EditorInfo2ProtoText EI2PT_EnumInfo error: enumText is null");
                    return null;
                }
                else
                    outSb.Append(enumText);
            }

            //返回完整Text
            return outSb.ToString();
        }

        /// <summary>
        /// EditorInfo消息体 转换为 Proto Text
        /// </summary>
        /// <param name="msg"></param>
        /// <returns> 发生错误时返回Null </returns>
        static string EI2PT_MessageInfo(MessageInfo msg)
        {
            StringBuilder sb = new StringBuilder();

            //消息体注释
            sb.Append($"/*{msg.annotation.Replace("\n", " ")}*/{linefeed1}");

            //消息体名称
            sb.Append($"message {msg.name}{linefeed1}");


            sb.Append($"{{{linefeed1}");//起始-花括号
            {
                //所有字段
                foreach (var field in msg.fieldsList)
                {
                    //Tab
                    sb.Append(tab1);

                    //修饰符
                    if (field.modifier != ModifierType.None)
                        sb.Append($"{field.modifier.ToString().ToLower()} ");

                    //字段类型
                    switch (field.type)
                    {
                        case FieldsType.Double: sb.Append("double"); break;
                        case FieldsType.Float: sb.Append("float"); break;
                        case FieldsType.Bool: sb.Append("bool"); break;
                        case FieldsType.String: sb.Append("string"); break;

                        case FieldsType.Int32: sb.Append("int32"); break;
                        case FieldsType.Uint32: sb.Append("uint32"); break;
                        case FieldsType.Sint32: sb.Append("snt32"); break;

                        case FieldsType.Int64: sb.Append("int64"); break;
                        case FieldsType.Uint64: sb.Append("uint64"); break;
                        case FieldsType.Sint64: sb.Append("sint64"); break;

                        case FieldsType.Custom:
                            if (!string.IsNullOrEmpty(field.customTypeName))
                                sb.Append(field.customTypeName);
                            else
                            {
                                ProtoHelper.LogError($"EditorInfo2ProtoText message.field.customTypeName error! {field.customTypeName}");
                                return null;//此错误则直接跳出
                            }
                            break;

                        default:
                            string unexpectedType = field.type.ToString();
                            ProtoHelper.LogError($"EditorInfo2ProtoText message.field.typeName is unexpected! {unexpectedType}");
                            sb.Append(unexpectedType.ToLower());
                            break;
                    }

                    //字段名
                    sb.Append($" {field.name} = ");

                    //字段编号
                    sb.Append($"{field.code};");

                    //字段注释
                    sb.Append($"/*{field.annotation.Replace("\n", " ")}*/");

                    //换行
                    sb.Append($"{linefeed2}");
                }

                //所有内部枚举
                foreach (var info in msg.enumInfoList)
                {
                    string enumText = EI2PT_EnumInfo(info, tab1);
                    if (string.IsNullOrEmpty(enumText))
                        return null;
                    else
                        sb.Append(enumText);
                }
            }
            sb.Append($"}}//END{linefeed2}");//结束-花括号 //END标记用用于判断Message结束位置

            return sb.ToString();
        }

        /// <summary>
        /// EditorInfo枚举 转换为 Proto Text
        /// </summary>
        /// <param name="ei"></param>
        /// <param name="prefix"> 每行前缀文本 </param>
        /// <returns> 发生错误时返回Null </returns>
        static string EI2PT_EnumInfo(EnumInfo ei, string prefix = "")
        {
            StringBuilder sb = new StringBuilder();

            //枚举类型注释(注释一定不能换行)
            sb.Append($"{prefix}/*{ei.annotation.Replace("\n", " ")}*/{linefeed1}");

            //枚举类型名称
            sb.Append($"{prefix}enum {ei.typeName}{linefeed1}");

            sb.Append($"{prefix}{{{linefeed1}");//起始-花括号
            {
                //所有枚举值
                foreach (var v in ei.valueList)
                {
                    //枚举值与注释（自动转为大写形式了）
                    sb.Append($"{prefix}{tab1}{v.valueName.ToUpper()} = {v.value.ToString()};/*{v.annotation.Replace("\n", " ")}*/{linefeed1}");
                }
            }
            sb.Append($"{prefix}}}{linefeed2}");//结束-花括号

            return sb.ToString();
        }

        #endregion

        #region 反转换

        /// <summary>
        /// Proto文件 转换为 EditorInfo
        /// </summary>
        /// <param name="fileFullName"> Proto文件的FullName </param>
        /// <returns></returns>
        public static ProtoEditorInfoWrapper Proto2TextEditorInfoWrapper(string fileFullName)
        {
            ProtoEditorInfoWrapper wapper;
            ProtoEditorInfo info = Proto2TextEditorInfo(fileFullName);
            if (info != null)
                wapper = new ProtoEditorInfoWrapper(ref info);
            else
            {
                wapper = null;
            }

            return wapper;
        }

        /// <summary>
        /// Proto文件 转换为 EditorInfo
        /// </summary>
        /// <param name="fileFullName"> Proto文件的FullName </param>
        /// <returns></returns>
        public static ProtoEditorInfo Proto2TextEditorInfo(string fileFullName)
        {
            if (string.IsNullOrEmpty(fileFullName) || !File.Exists(fileFullName))
            {
                ProtoHelper.LogError($"Proto2TextEditorInfo error fileFullName:{fileFullName}");
                return null;
            }

            //从文件中读取文本内容
            string textContent = File.ReadAllText(fileFullName);
            if (textContent == null)
            {
                ProtoHelper.LogError($"Proto2TextEditorInfo File.ReadAllText textContent is null :{fileFullName}");
                return null;
            }

            //新文件内是没内容的但不会为null,所以创建一个新的ProtoEditorInfo扔回去
            if (textContent == "")
            {
                return new ProtoEditorInfo();
            }


            ProtoEditorInfo newInfo = new ProtoEditorInfo();

            //匹配-语法协议(syntax = \"ABC\";)（必定有）
            {
                string pattern_Yntax = @"^syntax\s=\s""([\w]*)"";";
                Match mat = Regex.Match(textContent, pattern_Yntax, RegexOptions.Multiline);
                if (!mat.Success)
                {
                    ProtoHelper.LogError("反转换 匹配失败 - 语法协议");
                    return null;
                }
                string YntaxName = mat.Groups[1].Value;
                Enum.TryParse<YntaxType>(YntaxName, out newInfo.Yntax);
            }


            //匹配-命名空间(包名)(package ABC;)（可能没有）
            {
                string pattern_PackageName = @"^package\s([\w]*?);";
                Match mat = Regex.Match(textContent, pattern_PackageName, RegexOptions.Multiline);
                if (mat.Success)
                {
                    newInfo.PackageName = mat.Groups[1].Value;
                }
                else
                    newInfo.PackageName = "";
               // LogError("newInfo.PackageName:" + newInfo.PackageName);
            }

            //匹配-引用文件(import public "ABC.proto";)(有多个)(可能没有)
            {
                string @pattern_Import = @"^import\spublic\s""(.*?)"";";//[1]其他文件名带后缀
                Match mat = Regex.Match(textContent, pattern_Import, RegexOptions.Multiline);

                newInfo.ImportProtoFileName = new List<string>();
                while (mat.Success)
                {
                    string otherFileName = mat.Groups[1].Value;
                    newInfo.ImportProtoFileName.Add(otherFileName);
                    mat = mat.NextMatch();
                }
            }



            //匹配-枚举(enum ABC...)(有多个)(可能没有)
            string @pattern_enum1 = @"^/\*(.*)\*/\s*enum\s(\w*)\s*\{([\S\s]*?)\}";  //[1]注释内容 [2]枚举类型名 [3]具体内容
            string @pattern_enum2 = @"^\s*(\w*)\s=\s(\w*?);/\*(.*?)\*/\s*$";        //用于解析枚举值- [1]枚举值名 [2]枚举实际值 [3]注释内容
            {
                Match mat = Regex.Match(textContent, @pattern_enum1, RegexOptions.Multiline);
                newInfo.EnumInfoList = new List<EnumInfo>();
                // Log("反转换枚举结果：" + mat.Success);
                while (mat.Success)
                {
                    string ann = mat.Groups[1].Value;
                    string enumName = mat.Groups[2].Value;
                    string valueText = mat.Groups[3].Value;

                    //未组装完的枚举数据，继续解析枚举值文本
                    EnumInfo tempE = new EnumInfo(enumName) { annotation = ann };
                    tempE.valueList = new List<EnumValueInfo>();
                    Match mat2 = Regex.Match(valueText, @pattern_enum2, RegexOptions.Multiline);
                    while (mat2.Success)
                    {
                        string v_name = mat2.Groups[1].Value;
                        string v_value = mat2.Groups[2].Value;
                        string v_ann = mat2.Groups[3].Value;
                        tempE.valueList.Add(new EnumValueInfo(v_name) { value = int.Parse(v_value), annotation = v_ann });
                        mat2 = mat2.NextMatch();
                    }

                    //组装完成
                    newInfo.EnumInfoList.Add(tempE);
                    mat = mat.NextMatch();
                }
                // Log("反转换发现了枚举数量：" + newInfo.EnumInfoList.Count);
            }

            //匹配-消息体列表(message ABC)(有多个)(可能没有)
            {
                string @pattern_message1 = @"^/\*(.*)\*/\s*message\s(\w*)\s*\{([\S\s]*?)}//END";//[1]注释 [2]消息体名称 [2]内含文本
                //string @pattern_field1 = @"^\s*(\w*)\s(\w*)\s(\w*)\s=\s(\d*);/\*(.*?)\*/";      //旧的，匹配不到命名空间
                  string @pattern_field1 = @"^\s*(\w*)\s(\w*[.]*\w*)\s(\w*)\s=\s(\d*);/\*(.*?)\*/"; //[1]修饰符 [2]字段类型 [3]字段名 [4]值 [5]字段注释

                Match mat = Regex.Match(textContent, @pattern_message1, RegexOptions.Multiline);
                newInfo.MessageInfoList = new List<MessageInfo>();
                // Log("反转换消息体结果：" + mat.Success);
                while (mat.Success)
                {
                    string ann = mat.Groups[1].Value;
                    string name = mat.Groups[2].Value;
                    string otherText = mat.Groups[3].Value;

                    //未组装完的消息体数据，继续解析内含文本
                    MessageInfo tempM = new MessageInfo();
                    tempM.annotation = ann;
                    tempM.name = name;
                    tempM.fieldsList = new List<FieldsInfo>();
                    tempM.enumInfoList = new List<EnumInfo>();

                    //内部枚举值解析
                    {
                        string @pattern_enum3 = @"^\s*/\*(.*)\*/\s*enum\s(\w*)\s*\{([\S\s]*?)\}";  //用于解析消息体内部枚举- [1]注释内容 [2]枚举类型名 [3]具体内容
                        Match mat2 = Regex.Match(otherText, @pattern_enum3, RegexOptions.Multiline);
                        while (mat2.Success)
                        {
                            string ann2 = mat2.Groups[1].Value;
                            string enumName = mat2.Groups[2].Value;
                            string valueText = mat2.Groups[3].Value;
                            EnumInfo tempE = new EnumInfo(enumName) { annotation = ann2 };
                            tempE.valueList = new List<EnumValueInfo>();
                            Match mat3 = Regex.Match(valueText, @pattern_enum2, RegexOptions.Multiline);
                            while (mat3.Success)
                            {
                                string v_name = mat3.Groups[1].Value;
                                string v_value = mat3.Groups[2].Value;
                                string v_ann = mat3.Groups[3].Value;
                                tempE.valueList.Add(new EnumValueInfo(v_name) { value = int.Parse(v_value), annotation = v_ann });
                                mat3 = mat3.NextMatch();
                            }
                            //枚举组装完成
                            tempM.enumInfoList.Add(tempE);
                            mat2 = mat2.NextMatch();
                        }

                        //LogError("反解析-的内部枚举数量：" + tempM.enumInfoList.Count);
                        //foreach (var i in tempM.enumInfoList)
                        //    LogError("反解析-内部枚举：" + i.typeName);
                    }

                    //字段解析
                    {
                        //把内部枚举文本剔除掉
                        string @replaceP = @"^\s*/\*.*\*/\s*enum\s*\w*\s*\{[\S\s]*?\}";
                        string replaceText = otherText;
                        Match mat_replace = Regex.Match(replaceText, @replaceP, RegexOptions.Multiline);
                        while (mat_replace.Success)
                        {
                            string matStr = mat_replace.Groups[0].Value;
                            replaceText = replaceText.Replace(matStr, " ");
                            mat_replace = mat_replace.NextMatch();
                        }

                        // Log("消息体内部枚举剔除后结果\n" + replaceText);

                        //处理字段
                        Match mat4 = Regex.Match(replaceText, @pattern_field1, RegexOptions.Multiline);
                        while (mat4.Success)
                        {
                            string modi = mat4.Groups[1].Value;
                            string typ = mat4.Groups[2].Value;
                            string nam = mat4.Groups[3].Value;
                            string co = mat4.Groups[4].Value;
                            string annn = mat4.Groups[5].Value;
                            FieldsInfo tempF = new FieldsInfo(uint.Parse(co));
                            tempF.annotation = annn;
                            tempF.name = nam;

                            //字段修饰符反解析
                            switch (modi)
                            {
                                case "repeated":
                                    tempF.modifier = ModifierType.Repeated; break;
                                case "":
                                    tempF.modifier = ModifierType.None; break;
                                default:
                                    LogError("注意！修饰符反解析失败：" + modi);
                                    tempF.modifier = ModifierType.None;
                                    break;
                            }

                            //     Log($"修饰符反解析[{modi}] - >[{tempF.modifier}]");

                            //字段类型反解析
                            switch (typ)
                            {
                                case "double": tempF.type = FieldsType.Double; break;
                                case "float": tempF.type = FieldsType.Float; break;
                                case "bool": tempF.type = FieldsType.Bool; break;
                                case "string": tempF.type = FieldsType.String; break;
                                case "int32": tempF.type = FieldsType.Int32; break;
                                case "uint32": tempF.type = FieldsType.Uint32; break;
                                case "snt32": tempF.type = FieldsType.Sint32; break;
                                case "int64": tempF.type = FieldsType.Int64; break;
                                case "uint64": tempF.type = FieldsType.Uint64; break;
                                case "sint64": tempF.type = FieldsType.Sint64; break;
                                default:
                                    tempF.type = FieldsType.Custom;
                                    tempF.customTypeName = typ;
                                    break;
                            }

                            //   Log($"消息体字段反解析：{typ} -> {tempF.type}");

                            tempM.fieldsList.Add(tempF);
                            mat4 = mat4.NextMatch();
                        }

                    }

                    newInfo.MessageInfoList.Add(tempM);
                    mat = mat.NextMatch();
                }
            }


            return newInfo;
        }


        /// <summary>
        /// 文本中 正则匹配 MessageInfo
        /// </summary>
        /// <param name="oneMessageText">单个消息体文本</param>
        /// <returns></returns>
        static MessageInfo MatchMessageInfo(string oneMessageText)
        {
            return null;
        }

        #endregion

    }
}
