using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace dll2lib
{
    internal class Program
    {
        static void GenerateDef(string inputDll,string outputDef)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "tools", "dumpbin.exe");
            startInfo.ArgumentList.Add("/exports");
            startInfo.ArgumentList.Add(inputDll);
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            using (Process process = Process.Start(startInfo))
            {
                if (process != null)
                {
                    // 获取程序的标准输出
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();  // 获取标准错误输出

                    // 等待程序执行完毕
                    process.WaitForExit();

                    // 获取程序的退出代码
                    int exitCode = process.ExitCode;
                    if (exitCode == 0)
                    {
                        var outputLines = output.Split("\n").Select(p=>p.Trim()).ToList();
                        List<string> s = new List<string>();
                        bool isStart = false;
                        for(int i=0;i<outputLines.Count;i++)
                        {
                            if (isStart) {
                                if (outputLines[i] == "Summary") break;
                                var sL = outputLines[i].Split(" ").Where(p => p.Length > 0).ToList();
                                if (sL.Count == 4) s.Add(sL[3]);
                            }else
                            if (outputLines[i]== "ordinal hint RVA      name")isStart = true;
                        }
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("LIBRARY \""+Path.GetFileNameWithoutExtension(inputDll)+"\"");//\r\nEXPORTS\r\n")
                        sb.AppendLine("EXPORTS");
                        foreach(var im in s)
                        {
                            sb.AppendLine("    "+im);
                        }
                        using (FileStream fs = new FileStream(outputDef, FileMode.Create))
                        {
                            using (TextWriter tw = new StreamWriter(fs))
                            {
                                tw.Write(sb.ToString());
                            }
                        }
                    }
                }
            }
        }

        static void GenerateLib(string inputDef, string outputLib)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "tools", "lib.exe");
            startInfo.ArgumentList.Add("/def:"+inputDef);
            startInfo.ArgumentList.Add("/out:"+outputLib);
            startInfo.ArgumentList.Add("/dll");
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            using (Process process = Process.Start(startInfo))
            {
                if (process != null)
                {
                    // 获取程序的标准输出
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();  // 获取标准错误输出

                    // 等待程序执行完毕
                    process.WaitForExit();

                    // 获取程序的退出代码
                    int exitCode = process.ExitCode;
                    if (exitCode == 0)
                    {
                        Console.WriteLine(output);
                    }
                    Console.WriteLine(error);
                }
            }
        }
        static void Main(string[] args)
        {
            string dllfile = args[0];
            string outputdir = args[1];

            if (File.Exists(dllfile))
            {
                if (!(outputdir.Length>3 && outputdir[1]==':' && outputdir[2]== '\\'))
                {
                    outputdir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),outputdir);
                }
                if(!Directory.Exists(outputdir))
                {
                    Directory.CreateDirectory(outputdir);
                }
                string fn = Path.GetFileNameWithoutExtension(dllfile);
                GenerateDef(dllfile, Path.Combine(outputdir, fn + ".def"));
                GenerateLib(Path.Combine(outputdir, fn + ".def"), Path.Combine(outputdir, fn + ".lib"));
                Console.WriteLine("DONE");
                return;
            }

            Console.WriteLine("dll2lib dllfile outputdir");
        }
    }
}
