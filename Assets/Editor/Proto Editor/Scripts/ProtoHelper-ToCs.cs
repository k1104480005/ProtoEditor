using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EditProto
{
    /// <summary>
    /// ������ - .Bat
    /// </summary>
    public static partial class ProtoHelper
    {
        /// <summary>
        /// Proto�ļ�תΪC#Э���ļ�
        /// </summary>
        /// <param name="fileFullNames"> Ҫת����proto�ļ���ȫ·��</param>
        /// <param name="exeFullName"> protoc.exeת����������ȫ·��</param>
        /// <param name="csOutDir"> ����C#Э���ļ���ŵ�Ŀ¼ </param>
        public static bool Proto2CSFile(List<string> fileFullNames, string exeFullName, string csOutDir)
        {
            if (fileFullNames == null || fileFullNames.Count == 0)
                return false;

            if (!File.Exists(exeFullName))
            {
                ProtoHelper.LogErrorInfo("Protoc.exe·������" + exeFullName);
                return false;
            }

            if (!Directory.Exists(csOutDir))
            {
                ProtoHelper.LogErrorInfo("CS�ļ�����ļ���·������" + csOutDir);
                return false;
            }

            foreach (var i in fileFullNames)
            {
                if (!File.Exists(i))
                {
                    ProtoHelper.LogErrorInfo(".proto�ļ������ڣ�" + i);
                    return false;
                }
            }

            string exeFolder = Path.GetDirectoryName(exeFullName);
            string exeName = Path.GetFileName(exeFullName);

            List<string> cmds = new List<string>();

            cmds.Add("@echo off");
            cmds.Add("echo ==================================");
            cmds.Add($"cd {exeFolder}"); //��cd��protoc.exe���ڵ��ļ���

            foreach (var fileFullName in fileFullNames)
            {
                //����ļ���·��
                string fileFolder = Path.GetDirectoryName(fileFullName);
                string fileName = Path.GetFileName(fileFullName);
                //���ָ��
                string cmd = exeName + " --csharp_out=" + csOutDir + " -I " + fileFolder + " " + fileFullName;
                cmds.Add(cmd);
                cmds.Add($"echo --------- {fileName}");
            }

            cmds.Add("echo ==================================");         
            cmds.Add($"pause"); //��ͣ

            //ִ��
            //Cmd(cmds);
            BAT(cmds);

            return true;
        }

        /// <summary>
        /// ����.bat�ļ�Ȼ��ִ��
        /// </summary>
        /// <param name="cmds"></param>
        static void BAT(List<string> cmds)
        {
            if (cmds == null) return;

            //����".bat�ļ�"
            string batPath = $"{Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets"))}/TEMPPBF2CS.bat";
            StringBuilder sb = new StringBuilder();
            foreach (var i in cmds)
            {
                sb.AppendLine(i);
            }
            File.WriteAllText(batPath, sb.ToString());

            //ִ�и�bat�ļ�
            Process.Start(batPath);
        }


        /*  (�޷��ɹ�����������)
cd D:/UnityProject/KonnerFramework/Assets/OEngine/Editor/Proto Editor/Program
protoc.exe --csharp_out=D:/UnityProject/KonnerFramework/Assets/GameScripts/HotFix/GameProto/GameProtocol -I D:\UnityProject\KonnerFramework\Proto\schemas D:\UnityProject\KonnerFramework\Proto\schemas\A.proto
pause
        */
        public static void Cmd(List<string> cmds)
        {
            //System.Console.InputEncoding = System.Text.Encoding.UTF8;
            //System.Console.OutputEncoding = System.Text.Encoding.UTF8;

            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WorkingDirectory = ".";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.StandardErrorEncoding = Encoding.Default;
            process.StartInfo.StandardOutputEncoding = Encoding.Default;
            process.OutputDataReceived += (a, b) =>
            {
                if (!string.IsNullOrEmpty(b.Data))
                {
                    ProtoHelper.LogInfo(b.Data);
                }
            };
            process.ErrorDataReceived += (a, b) =>
            {
                if (!string.IsNullOrEmpty(b.Data) && !b.Data.Contains("warning:"))
                {
                    ProtoHelper.LogErrorInfo(b.Data);
                }
            };
            process.Exited += (a, b) => { };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            for (int i = 0; i < cmds.Count; i++)
            {
                ProtoHelper.LogInfo("CMD:" + cmds[i]);
                process.StandardInput.WriteLine(cmds[i]);
            }
            process.StandardInput.WriteLine("exit");
            process.WaitForExit();
        }

    }


}