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
    /// 助手类 - .Bat
    /// </summary>
    public static partial class ProtoHelper
    {
        /// <summary>
        /// Proto文件转为C#协议文件
        /// </summary>
        /// <param name="fileFullNames"> 要转换的proto文件的全路径</param>
        /// <param name="exeFullName"> protoc.exe转换程序所在全路径</param>
        /// <param name="csOutDir"> 生成C#协议文件存放的目录 </param>
        public static bool Proto2CSFile(List<string> fileFullNames, string exeFullName, string csOutDir)
        {
            if (fileFullNames == null || fileFullNames.Count == 0)
                return false;

            if (!File.Exists(exeFullName))
            {
                ProtoHelper.LogErrorInfo("Protoc.exe路径错误：" + exeFullName);
                return false;
            }

            if (!Directory.Exists(csOutDir))
            {
                ProtoHelper.LogErrorInfo("CS文件输出文件夹路径错误：" + csOutDir);
                return false;
            }

            foreach (var i in fileFullNames)
            {
                if (!File.Exists(i))
                {
                    ProtoHelper.LogErrorInfo(".proto文件不存在：" + i);
                    return false;
                }
            }

            string exeFolder = Path.GetDirectoryName(exeFullName);
            string exeName = Path.GetFileName(exeFullName);

            List<string> cmds = new List<string>();

            cmds.Add("@echo off");
            cmds.Add("echo ==================================");
            cmds.Add($"cd {exeFolder}"); //先cd到protoc.exe所在的文件夹

            foreach (var fileFullName in fileFullNames)
            {
                //获得文件夹路径
                string fileFolder = Path.GetDirectoryName(fileFullName);
                string fileName = Path.GetFileName(fileFullName);
                //添加指令
                string cmd = exeName + " --csharp_out=" + csOutDir + " -I " + fileFolder + " " + fileFullName;
                cmds.Add(cmd);
                cmds.Add($"echo --------- {fileName}");
            }

            cmds.Add("echo ==================================");         
            cmds.Add($"pause"); //暂停

            //执行
            //Cmd(cmds);
            BAT(cmds);

            return true;
        }

        /// <summary>
        /// 生成.bat文件然后执行
        /// </summary>
        /// <param name="cmds"></param>
        static void BAT(List<string> cmds)
        {
            if (cmds == null) return;

            //生成".bat文件"
            string batPath = $"{Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets"))}/TEMPPBF2CS.bat";
            StringBuilder sb = new StringBuilder();
            foreach (var i in cmds)
            {
                sb.AppendLine(i);
            }
            File.WriteAllText(batPath, sb.ToString());

            //执行该bat文件
            Process.Start(batPath);
        }


        /*  (无法成功，报错乱码)
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