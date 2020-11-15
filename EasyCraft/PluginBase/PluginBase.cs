using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using EasyCraft.Core;

namespace EasyCraft.PluginBase
{
    public class PluginBase
    {
        #region DLLFuncs

        /// <summary>
        /// 原型是 :HMODULE LoadLibrary(LPCTSTR lpFileName);
        /// </summary>
        /// <param name="lpFileName">DLL 文件名 </param>
        /// <returns> 函数库模块的句柄 </returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadLibrary(string lpFileName);

        /// <summary>
        /// 原型是 : FARPROC GetProcAddress(HMODULE hModule, LPCWSTR lpProcName);
        /// </summary>
        /// <param name="hModule"> 包含需调用函数的函数库模块的句柄 </param>
        /// <param name="lpProcName"> 调用函数的名称 </param>
        /// <returns> 函数指针 </returns>
        [DllImport("kernel32.dll")]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        /// <summary>
        /// 原型是 : BOOL FreeLibrary(HMODULE hModule);
        /// </summary>
        /// <param name="hModule"> 需释放的函数库模块的句柄 </param>
        /// <returns> 是否已释放指定的 Dll</returns>
        [DllImport("kernel32", EntryPoint = "FreeLibrary", SetLastError = true)]
        static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        /// 装载 Dll
        /// </summary>
        /// <param name="lpFileName">DLL 文件名 </param>
        private IntPtr LoadDll(string lpFileName)
        {
            IntPtr hModule = LoadLibrary(lpFileName);
            if (hModule == IntPtr.Zero)
                throw (new Exception(" Not Found :" + lpFileName + "."));
            return hModule;
        }

        /// <summary>
        /// 获得函数指针
        /// </summary>
        /// <param name="lpProcName"> 调用函数的名称 </param>
        private IntPtr LoadFun(IntPtr hModule, string lpProcName)
        {
            // 若函数库模块的句柄为空，则抛出异常
            if (hModule == IntPtr.Zero)
                throw (new Exception(" 函数库模块的句柄为空 , 请确保已进行 LoadDll 操作 !"));
// 取得函数指针
            IntPtr farProc = GetProcAddress(hModule, lpProcName);
// 若函数指针，则抛出异常
            if (farProc == IntPtr.Zero)
                throw (new Exception(" 没有找到 :" + lpProcName + " 这个函数的入口点 "));
            return farProc;
        }

        /// <summary>
        /// 获得函数指针
        /// </summary>
        /// <param name="lpFileName"> 包含需调用函数的 DLL 文件名 </param>
        /// <param name="lpProcName"> 调用函数的名称 </param>
        private IntPtr LoadFun(string lpFileName, string lpProcName)
        {
            // 取得函数库模块的句柄
            IntPtr hModule = LoadLibrary(lpFileName);
// 若函数库模块的句柄为空，则抛出异常
            if (hModule == IntPtr.Zero)
                throw (new Exception(" 没有找到 :" + lpFileName + "."));
// 取得函数指针
            IntPtr farProc = GetProcAddress(hModule, lpProcName);
// 若函数指针，则抛出异常
            if (farProc == IntPtr.Zero)
                throw (new Exception(" 没有找到 :" + lpProcName + " 这个函数的入口点 "));
            return farProc;
        }

        /// <summary>
        /// 卸载 Dll
        /// </summary>
        private void UnLoadDll(IntPtr hModule)
        {
            FreeLibrary(hModule);
        }

        ///<summary>
        /// 通过非托管函数名转换为对应的委托 , by jingzhongrong
        ///</summary>
        ///<param name="dllModule"> 通过 LoadLibrary 获得的 DLL 句柄 </param>
        ///<param name="functionName"> 非托管函数名 </param>
        ///<param name="t"> 对应的委托类型 </param>
        ///<returns> 委托实例，可强制转换为适当的委托类型 </returns>
        private static Delegate GetFunctionAddress(IntPtr dllModule, string functionName, Type t)
        {
            IntPtr address = GetProcAddress(dllModule, functionName);
            if (address == IntPtr.Zero)
                return null;
            else
                return Marshal.GetDelegateForFunctionPointer(address, t);
        }

        ///<summary>
        /// 将表示函数地址的 IntPtr 实例转换成对应的委托 , by jingzhongrong
        ///</summary>
        private static Delegate GetDelegateFromIntPtr(IntPtr address, Type t)
        {
            if (address == IntPtr.Zero)
                return null;
            else
                return Marshal.GetDelegateForFunctionPointer(address, t);
        }

        ///<summary>
        /// 将表示函数地址的 int 转换成对应的委托，by jingzhongrong
        ///</summary>
        private static Delegate GetDelegateFromIntPtr(int address, Type t)
        {
            if (address == 0)
                return null;
            else
                return Marshal.GetDelegateForFunctionPointer(new IntPtr(address), t);
        }

        #endregion

        private static NamedPipeServerStream pipeServer = null;
        private static string pipeid = "3CE5CC8B-5FFA-4F47-8FCF-32D2092CBB0B";

        public static void InitializePipe()
        {
            pipeServer = new NamedPipeServerStream(pipeid, PipeDirection.InOut);
            pipeServer.BeginWaitForConnection(PipeReceived, null);
        }

        private static void PipeReceived(IAsyncResult result)
        {
        }

        public static void LoadPlugins()
        {
            foreach (string file in Directory.EnumerateFiles("plugin/", "*.dll").ToList())
            {
                try
                {
                    Plugin p = new Plugin();
                    IntPtr moduleIntPtr = LoadLibrary(Path.GetFullPath(file));
                    if (moduleIntPtr == IntPtr.Zero)
                        throw new Exception("Cannot Get Plugin's Library Pointer (" +
                                            Marshal.GetLastWin32Error().ToString() + ")");
                    PluginDelegate.Initialize initialize =
                        (PluginDelegate.Initialize) GetFunctionAddress(moduleIntPtr, "Initialize",
                            typeof(PluginDelegate.Initialize));
                    string authkey = Functions.GetRandomString();
                    string plugininfos = initialize(pipeid, authkey);
                    string[] plugininfo = plugininfos.Split('$');
                    FastConsole.PrintSuccess(string.Format(Language.t("成功加载插件: {0}"), plugininfo[1]));
                }
                catch (Exception e)
                {
                    FastConsole.PrintWarning(string.Format(Language.t("加载插件 {0} 失败: {1}"), file, e.Message));
                }
            }
        }
    }

    class Plugin
    {
        public string id = "";
        public string key = "";
        public string path = "";
        public IntPtr hModule = IntPtr.Zero;
    }
}