using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace EditProto
{
    /// <summary>
    /// ������ - ת���뷴ת��
    /// </summary>
    public static partial class ProtoHelper
    {
        const string linefeed1 = "\r\n";      //1�λ���
        const string linefeed2 = "\r\n\r\n";  //2�λ���
        const string tab1 = "\t";             //�Ʊ��

        /// <summary>
        /// ת���� Proto�ļ�
        /// </summary>
        /// <param name="wrapper"></param>
        /// <param name="fileFullname"> ȫ·�� </param>
        /// <returns></returns>
        public static bool ToProtoFile(ProtoEditorInfoWrapper wrapper, string fileFullname)
        {
            string pFolder = System.IO.Path.GetDirectoryName(fileFullname); //��ȡ�ļ�·��
            string pFile = System.IO.Path.GetFileName(fileFullname); //��ȡ�ļ���
            return ToProtoFile(wrapper, pFile, pFolder);
        }

        /// <summary>
        /// ת���� Proto�ļ�
        /// </summary>
        /// <param name="wrapper"> wrapper </param>
        /// <param name="fileName"> �ļ�������׺ </param>
        /// <param name="folderPath"> ����ļ���·�� </param>
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
                info = $"[���Ǳ���]";
            else
                info = $"[����]";

            File.WriteAllText(fullName, protoText);

            ProtoHelper.Log(string.Format($"{info} {fullName}"));

            //ˢ��(����·���ڹ����� ���Ա����ֶ�ˢ�²ſ���)
            UnityEditor.AssetDatabase.Refresh();

            ////�򿪸��ļ���
            //FileInfo fileInfo = new FileInfo(info.FullName);
            //System.Diagnostics.Process.Start(fileInfo.Directory.FullName);

            return true;
        }

        /// <summary>
        /// ��ת�� Proto�ļ�
        /// </summary>
        /// <param name="fileFullName"> �ļ�ȫ·�� </param>
        /// <returns> ����ʱ����Null </returns>
        public static ProtoEditorInfoWrapper FromProtoFile(string fileFullName)
        {
            return Proto2TextEditorInfoWrapper(fileFullName);
        }

        #region ת��

        /// <summary>
        /// EditorInfo ת��Ϊ Proto Text
        /// </summary>
        /// <param name="infoWrapper"></param>
        /// <returns> ����Proto Text </returns>
        public static string EditorInfo2ProtoText(ProtoEditorInfoWrapper infoWrapper)
        {
            if (infoWrapper == null)
                return null;

            return EditorInfo2ProtoText(infoWrapper.Info);
        }

        /// <summary>
        /// EditorInfo ת��Ϊ Proto Text
        /// </summary>
        /// <param name="info"> Editor Info </param>
        /// <returns>����ProtoProto Text</returns>
        public static string EditorInfo2ProtoText(ProtoEditorInfo info)
        {
            if (info == null)
                return null;

            StringBuilder outSb = new StringBuilder();

            //�﷨Э��汾
            outSb.Append($"syntax = \"{info.Yntax}\";{linefeed2}");

            //�����ռ�(����)
            if (!string.IsNullOrEmpty(info.PackageName))
                outSb.Append($"package {info.PackageName};{linefeed2}");

            //���������ļ�(.Proto) ����:import public "proto_cs_common.proto";
            if (info.ImportProtoFileName.Count > 0)
            {
                foreach (var name in info.ImportProtoFileName)
                    outSb.Append($"import public \"{name}\";{linefeed1}");
                outSb.Append($"{linefeed2}");
            }

            //��Ϣ��
            foreach (var msg in info.MessageInfoList)
            {
                string msgText = EI2PT_MessageInfo(msg);
                if (string.IsNullOrEmpty(msgText))
                    return null;
                else
                    outSb.Append(msgText.ToString());
            }

            //ö��
            // Log("[ת��]info.EnumInfoList.count:" + info.EnumInfoList.Count);
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

            //��������Text
            return outSb.ToString();
        }

        /// <summary>
        /// EditorInfo��Ϣ�� ת��Ϊ Proto Text
        /// </summary>
        /// <param name="msg"></param>
        /// <returns> ��������ʱ����Null </returns>
        static string EI2PT_MessageInfo(MessageInfo msg)
        {
            StringBuilder sb = new StringBuilder();

            //��Ϣ��ע��
            sb.Append($"/*{msg.annotation.Replace("\n", " ")}*/{linefeed1}");

            //��Ϣ������
            sb.Append($"message {msg.name}{linefeed1}");


            sb.Append($"{{{linefeed1}");//��ʼ-������
            {
                //�����ֶ�
                foreach (var field in msg.fieldsList)
                {
                    //Tab
                    sb.Append(tab1);

                    //���η�
                    if (field.modifier != ModifierType.None)
                        sb.Append($"{field.modifier.ToString().ToLower()} ");

                    //�ֶ�����
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
                                return null;//�˴�����ֱ������
                            }
                            break;

                        default:
                            string unexpectedType = field.type.ToString();
                            ProtoHelper.LogError($"EditorInfo2ProtoText message.field.typeName is unexpected! {unexpectedType}");
                            sb.Append(unexpectedType.ToLower());
                            break;
                    }

                    //�ֶ���
                    sb.Append($" {field.name} = ");

                    //�ֶα��
                    sb.Append($"{field.code};");

                    //�ֶ�ע��
                    sb.Append($"/*{field.annotation.Replace("\n", " ")}*/");

                    //����
                    sb.Append($"{linefeed2}");
                }

                //�����ڲ�ö��
                foreach (var info in msg.enumInfoList)
                {
                    string enumText = EI2PT_EnumInfo(info, tab1);
                    if (string.IsNullOrEmpty(enumText))
                        return null;
                    else
                        sb.Append(enumText);
                }
            }
            sb.Append($"}}//END{linefeed2}");//����-������ //END����������ж�Message����λ��

            return sb.ToString();
        }

        /// <summary>
        /// EditorInfoö�� ת��Ϊ Proto Text
        /// </summary>
        /// <param name="ei"></param>
        /// <param name="prefix"> ÿ��ǰ׺�ı� </param>
        /// <returns> ��������ʱ����Null </returns>
        static string EI2PT_EnumInfo(EnumInfo ei, string prefix = "")
        {
            StringBuilder sb = new StringBuilder();

            //ö������ע��(ע��һ�����ܻ���)
            sb.Append($"{prefix}/*{ei.annotation.Replace("\n", " ")}*/{linefeed1}");

            //ö����������
            sb.Append($"{prefix}enum {ei.typeName}{linefeed1}");

            sb.Append($"{prefix}{{{linefeed1}");//��ʼ-������
            {
                //����ö��ֵ
                foreach (var v in ei.valueList)
                {
                    //ö��ֵ��ע�ͣ��Զ�תΪ��д��ʽ�ˣ�
                    sb.Append($"{prefix}{tab1}{v.valueName.ToUpper()} = {v.value.ToString()};/*{v.annotation.Replace("\n", " ")}*/{linefeed1}");
                }
            }
            sb.Append($"{prefix}}}{linefeed2}");//����-������

            return sb.ToString();
        }

        #endregion

        #region ��ת��

        /// <summary>
        /// Proto�ļ� ת��Ϊ EditorInfo
        /// </summary>
        /// <param name="fileFullName"> Proto�ļ���FullName </param>
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
        /// Proto�ļ� ת��Ϊ EditorInfo
        /// </summary>
        /// <param name="fileFullName"> Proto�ļ���FullName </param>
        /// <returns></returns>
        public static ProtoEditorInfo Proto2TextEditorInfo(string fileFullName)
        {
            if (string.IsNullOrEmpty(fileFullName) || !File.Exists(fileFullName))
            {
                ProtoHelper.LogError($"Proto2TextEditorInfo error fileFullName:{fileFullName}");
                return null;
            }

            //���ļ��ж�ȡ�ı�����
            string textContent = File.ReadAllText(fileFullName);
            if (textContent == null)
            {
                ProtoHelper.LogError($"Proto2TextEditorInfo File.ReadAllText textContent is null :{fileFullName}");
                return null;
            }

            //���ļ�����û���ݵĵ�����Ϊnull,���Դ���һ���µ�ProtoEditorInfo�ӻ�ȥ
            if (textContent == "")
            {
                return new ProtoEditorInfo();
            }


            ProtoEditorInfo newInfo = new ProtoEditorInfo();

            //ƥ��-�﷨Э��(syntax = \"ABC\";)���ض��У�
            {
                string pattern_Yntax = @"^syntax\s=\s""([\w]*)"";";
                Match mat = Regex.Match(textContent, pattern_Yntax, RegexOptions.Multiline);
                if (!mat.Success)
                {
                    ProtoHelper.LogError("��ת�� ƥ��ʧ�� - �﷨Э��");
                    return null;
                }
                string YntaxName = mat.Groups[1].Value;
                Enum.TryParse<YntaxType>(YntaxName, out newInfo.Yntax);
            }


            //ƥ��-�����ռ�(����)(package ABC;)������û�У�
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

            //ƥ��-�����ļ�(import public "ABC.proto";)(�ж��)(����û��)
            {
                string @pattern_Import = @"^import\spublic\s""(.*?)"";";//[1]�����ļ�������׺
                Match mat = Regex.Match(textContent, pattern_Import, RegexOptions.Multiline);

                newInfo.ImportProtoFileName = new List<string>();
                while (mat.Success)
                {
                    string otherFileName = mat.Groups[1].Value;
                    newInfo.ImportProtoFileName.Add(otherFileName);
                    mat = mat.NextMatch();
                }
            }



            //ƥ��-ö��(enum ABC...)(�ж��)(����û��)
            string @pattern_enum1 = @"^/\*(.*)\*/\s*enum\s(\w*)\s*\{([\S\s]*?)\}";  //[1]ע������ [2]ö�������� [3]��������
            string @pattern_enum2 = @"^\s*(\w*)\s=\s(\w*?);/\*(.*?)\*/\s*$";        //���ڽ���ö��ֵ- [1]ö��ֵ�� [2]ö��ʵ��ֵ [3]ע������
            {
                Match mat = Regex.Match(textContent, @pattern_enum1, RegexOptions.Multiline);
                newInfo.EnumInfoList = new List<EnumInfo>();
                // Log("��ת��ö�ٽ����" + mat.Success);
                while (mat.Success)
                {
                    string ann = mat.Groups[1].Value;
                    string enumName = mat.Groups[2].Value;
                    string valueText = mat.Groups[3].Value;

                    //δ��װ���ö�����ݣ���������ö��ֵ�ı�
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

                    //��װ���
                    newInfo.EnumInfoList.Add(tempE);
                    mat = mat.NextMatch();
                }
                // Log("��ת��������ö��������" + newInfo.EnumInfoList.Count);
            }

            //ƥ��-��Ϣ���б�(message ABC)(�ж��)(����û��)
            {
                string @pattern_message1 = @"^/\*(.*)\*/\s*message\s(\w*)\s*\{([\S\s]*?)}//END";//[1]ע�� [2]��Ϣ������ [2]�ں��ı�
                //string @pattern_field1 = @"^\s*(\w*)\s(\w*)\s(\w*)\s=\s(\d*);/\*(.*?)\*/";      //�ɵģ�ƥ�䲻�������ռ�
                  string @pattern_field1 = @"^\s*(\w*)\s(\w*[.]*\w*)\s(\w*)\s=\s(\d*);/\*(.*?)\*/"; //[1]���η� [2]�ֶ����� [3]�ֶ��� [4]ֵ [5]�ֶ�ע��

                Match mat = Regex.Match(textContent, @pattern_message1, RegexOptions.Multiline);
                newInfo.MessageInfoList = new List<MessageInfo>();
                // Log("��ת����Ϣ������" + mat.Success);
                while (mat.Success)
                {
                    string ann = mat.Groups[1].Value;
                    string name = mat.Groups[2].Value;
                    string otherText = mat.Groups[3].Value;

                    //δ��װ�����Ϣ�����ݣ����������ں��ı�
                    MessageInfo tempM = new MessageInfo();
                    tempM.annotation = ann;
                    tempM.name = name;
                    tempM.fieldsList = new List<FieldsInfo>();
                    tempM.enumInfoList = new List<EnumInfo>();

                    //�ڲ�ö��ֵ����
                    {
                        string @pattern_enum3 = @"^\s*/\*(.*)\*/\s*enum\s(\w*)\s*\{([\S\s]*?)\}";  //���ڽ�����Ϣ���ڲ�ö��- [1]ע������ [2]ö�������� [3]��������
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
                            //ö����װ���
                            tempM.enumInfoList.Add(tempE);
                            mat2 = mat2.NextMatch();
                        }

                        //LogError("������-���ڲ�ö��������" + tempM.enumInfoList.Count);
                        //foreach (var i in tempM.enumInfoList)
                        //    LogError("������-�ڲ�ö�٣�" + i.typeName);
                    }

                    //�ֶν���
                    {
                        //���ڲ�ö���ı��޳���
                        string @replaceP = @"^\s*/\*.*\*/\s*enum\s*\w*\s*\{[\S\s]*?\}";
                        string replaceText = otherText;
                        Match mat_replace = Regex.Match(replaceText, @replaceP, RegexOptions.Multiline);
                        while (mat_replace.Success)
                        {
                            string matStr = mat_replace.Groups[0].Value;
                            replaceText = replaceText.Replace(matStr, " ");
                            mat_replace = mat_replace.NextMatch();
                        }

                        // Log("��Ϣ���ڲ�ö���޳�����\n" + replaceText);

                        //�����ֶ�
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

                            //�ֶ����η�������
                            switch (modi)
                            {
                                case "repeated":
                                    tempF.modifier = ModifierType.Repeated; break;
                                case "":
                                    tempF.modifier = ModifierType.None; break;
                                default:
                                    LogError("ע�⣡���η�������ʧ�ܣ�" + modi);
                                    tempF.modifier = ModifierType.None;
                                    break;
                            }

                            //     Log($"���η�������[{modi}] - >[{tempF.modifier}]");

                            //�ֶ����ͷ�����
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

                            //   Log($"��Ϣ���ֶη�������{typ} -> {tempF.type}");

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
        /// �ı��� ����ƥ�� MessageInfo
        /// </summary>
        /// <param name="oneMessageText">������Ϣ���ı�</param>
        /// <returns></returns>
        static MessageInfo MatchMessageInfo(string oneMessageText)
        {
            return null;
        }

        #endregion

    }
}
