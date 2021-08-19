using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EasyCraft.Base.Core;
using EasyCraft.Base.Server;
using EasyCraft.Base.Starter;
using EasyCraft.Base.User;
using EasyCraft.Utils;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace EasyCraft.HttpServer.Api
{
    public static class HttpApis
    {
        private static int Version = 1;

        public static ApiReturnBase ApiLogin(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["username"]) ||
                string.IsNullOrEmpty(context.Request.Form["password"]))
                return ApiReturnBase.IncompleteParameters;
            var user =
                UserManager.Users.Values.FirstOrDefault(t => t.UserInfo.Name == context.Request.Form["username"]);
            if (user == null || user.UserInfo.Password !=
                context.Request.Form["password"].ToString().GetMD5())
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.Unauthorized,
                    Msg = "账号或密码错误"
                };
            if (user.UserRequest.Auth != null)
                UserManager.AuthToUid.Remove(user.UserRequest.Auth);
            user.UserRequest.Auth = Utils.Utils.CreateRandomString();
            UserManager.AuthToUid[user.UserRequest.Auth] = user.UserInfo.Id;
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功登录".Translate(),
                Data = user
            };
        }

        public static ApiReturnBase ApiLoginStatus(HttpContext context)
        {
            var auth = context.Request.Headers["Authorization"].ToString();
            if (auth == null || auth == "null" || !UserManager.AuthToUid.ContainsKey(auth))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.Unauthorized,
                    Msg = "未登录".Translate()
                };

            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功登录".Translate(),
                Data = UserManager.Users[UserManager.AuthToUid[auth]]
            };
        }

        public static ApiReturnBase ApiLogout(HttpContext context)
        {
            var auth = context.Request.Headers["Authorization"].ToString();
            if (auth is null or "null" || !UserManager.AuthToUid.ContainsKey(auth))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.Unauthorized,
                    Msg = "未登录".Translate()
                };

            UserManager.Users[UserManager.AuthToUid[auth]].UserRequest.Auth = "null";
            UserManager.AuthToUid.Remove(auth);
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功登出"
            };
        }

        public static ApiReturnBase ApiVersion(HttpContext _)
        {
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = new Dictionary<string, object>
                {
                    { "version", Common.VersionFull },
                    // ReSharper disable once StringLiteralTypo
                    { "vername", Common.VersionName },
                    // ReSharper disable once StringLiteralTypo
                    { "vershort", Common.VersionShort },
                    { "ApiVer", Version }
                }
            };
        }

        public static ApiReturnBase ApiServers(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            var data = ServerManager.Servers.Values.ToList();
            int page = 0;
            if (context.Request.HasFormContentType)
                int.TryParse(context.Request.Form["page"], out page);
            if (nowUser.UserInfo.Type < UserType.Technician)
                data = data.Where(t => t.BaseInfo.Owner == nowUser.UserInfo.Id).ToList();
            data = data.GetRange(page * 10, Math.Min(10, data.Count - page * 10));
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = data
            };
        }

        public static ApiReturnBase ApiServer(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = ServerManager.Servers[id]
            };
        }

        public static ApiReturnBase ApiServerInfoStart(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            var info = ServerManager.Servers[id].StartInfo;
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = new Dictionary<string, object>()
                {
                    { "id", info.Id },
                    { "core", info.Core },
                    { "world", info.World },
                    { "starter", info.Starter },
                    {
                        "starterInfo",
                        StarterManager.Starters.ContainsKey(info.Starter) ? StarterManager.Starters[info.Starter] : null
                    },
                    { "coreInfo", CoreManager.Cores.ContainsKey(info.Core) ? CoreManager.Cores[info.Core].Info : null }
                }
            };
        }

        public static ApiReturnBase ApiStarters(HttpContext context)
        {
            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = StarterManager.Starters.Values
            };
        }

        public static ApiReturnBase ApiRegister(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["username"]) ||
                string.IsNullOrEmpty(context.Request.Form["password"]) ||
                string.IsNullOrEmpty(context.Request.Form["repassword"]) ||
                string.IsNullOrEmpty(context.Request.Form["email"]))
                return ApiReturnBase.IncompleteParameters;
            if (UserManager.CheckUserNameOccupy(context.Request.Form["username"]))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.UserNameOccupied,
                    Msg = "用户名被占用".Translate()
                };
            if (context.Request.Form["password"] != context.Request.Form["repassword"])
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PasswordNotIdentical,
                    Msg = "两次密码不一致".Translate()
                };
            if (!Regex.IsMatch(context.Request.Form["password"], @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]*).{6,18}$"))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.IncorrectPasswordFormat,
                    Msg = "密码应为6-18位字母,数字,特殊符号的组合".Translate()
                };
            int ret = UserManager.AddUser(new UserInfoBase
            {
                Email = context.Request.Form["email"],
                Name = context.Request.Form["username"],
                Password = context.Request.Form["password"].ToString().GetMD5(),
                Type = UserType.Registered
            });
            if (ret == -1)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.RequestFailed,
                    Msg = "注册失败"
                };
            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Msg = "注册成功".Translate(),
                Data = UserManager.Users[ret]
            };
        }

        public static ApiReturnBase ChangePassword(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["username"]) ||
                string.IsNullOrEmpty(context.Request.Form["newpassword"]) ||
                string.IsNullOrEmpty(context.Request.Form["oldpassword"]))
                return ApiReturnBase.IncompleteParameters;
            var user = UserManager.Users.Values.FirstOrDefault(t =>
                t.UserInfo.Name == context.Request.Form["username"]);
            if (user == null)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "用户名不存在".Translate()
                };
            if (context.Request.Form["oldpassword"].ToString().GetMD5() != user.UserInfo.Password)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.Unauthorized,
                    Msg = "初始密码错误".Translate()
                };

            if (!Regex.IsMatch(context.Request.Form["newpassword"], @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]*).{6,18}$"))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.IncorrectPasswordFormat,
                    Msg = "密码应为6-18位字母,数字,特殊符号的组合".Translate()
                };
            user.UserInfo.Password = context.Request.Form["newpassword"].ToString().GetMD5();
            user.UserInfo.SyncToDatabase();
            _ = ApiLogout(context);
            return new ApiReturnBase()
            {
                Status = true,
                Msg = "成功修改密码".Translate(),
                Code = 200
            };
        }

        public static ApiReturnBase ApiServerBaseColumns(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = ServerManager.Servers[id].GetServerConfigItems(nowUser)
            };
        }

        public static async Task<ApiReturnBase> ApiServerPluginColumns(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = await ServerManager.Servers[id].GetServerPluginItems(nowUser)
            };
        }

        public static ApiReturnBase ApiCoresBranches(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["device"]))
                return ApiReturnBase.IncompleteParameters;
            int.TryParse(context.Request.Form["device"], out var i);
            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = CoreManager.Cores.Values.Where(t => t.Info.Device == i).Select(t => t.Info.Branch).ToHashSet()
            };
        }

        public static ApiReturnBase ApiCoresBranchItems(HttpContext context)
        {
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["branch"]))
                return ApiReturnBase.IncompleteParameters;
            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = CoreManager.Cores.Values.Where(t => t.Info.Branch == context.Request.Form["branch"])
            };
        }

        public static ApiReturnBase ApiServerConsole(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]) || string.IsNullOrEmpty(context.Request.Form["last"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };

            int last;
            int.TryParse(context.Request.Form["last"], out last);
            var logs = new List<ServerConsoleMessage>();
            if (last < ServerManager.Servers[id].StatusInfo.ConsoleMessages.Count)
            {
                int start = Math.Max(last,
                    ServerManager.Servers[id].StatusInfo.ConsoleMessages.Count - 100);
                logs = ServerManager.Servers[id].StatusInfo.ConsoleMessages.GetRange(start,
                    Math.Min(ServerManager.Servers[id].StatusInfo.ConsoleMessages.Count - start, 100));
            }

            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = new Dictionary<string, object>()
                {
                    { "lastid", ServerManager.Servers[id].StatusInfo.ConsoleMessages.Count },
                    { "logs", logs }
                }
            };
        }

        public static async Task<ApiReturnBase> ApiServerStart(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            var ret = await ServerManager.Servers[id].Start();
            return new ApiReturnBase
            {
                Status = ret.Code == 200,
                Code = ret.Code,
                Msg = ret.Message
            };
        }

        public static async Task<ApiReturnBase> ApiServerStop(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            var ret = await ServerManager.Servers[id].Stop();
            return new ApiReturnBase()
            {
                Status = ret,
                Code = ret ? 200 : (int)ApiReturnCode.RequestFailed,
                Msg = ret ? "关闭成功".Translate() : "关闭失败".Translate()
            };
        }

        public static ApiReturnBase ApiServerStatus(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = ServerManager.Servers[id].StatusInfo
            };
        }

        public static ApiReturnBase ApiServerBaseInfoUpdate(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["serverId"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["serverId"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            foreach (ServerConfigItem configItem in ServerManager.Servers[id].GetServerConfigItems(nowUser))
            {
                if (!configItem.NoEdit && context.Request.Form.ContainsKey(configItem.Id))
                {
                    switch (configItem.Id)
                    {
                        case "name":
                            ServerManager.Servers[id].BaseInfo.Name = context.Request.Form[configItem.Id];
                            break;
                        case "player":
                            if (int.TryParse(context.Request.Form[configItem.Id], out var player))
                                ServerManager.Servers[id].BaseInfo.Player = player;
                            break;
                        case "expireTime":
                            if (DateTime.TryParse(context.Request.Form[configItem.Id], out var date))
                                ServerManager.Servers[id].BaseInfo.ExpireTime = date;
                            break;
                        case "port":
                            if (int.TryParse(context.Request.Form[configItem.Id], out var port))
                                ServerManager.Servers[id].BaseInfo.Port = port;
                            break;
                        case "ram":
                            if (int.TryParse(context.Request.Form[configItem.Id], out var ram))
                                ServerManager.Servers[id].BaseInfo.Ram = ram;
                            break;
                        case "autoStart":
                            ServerManager.Servers[id].BaseInfo.AutoStart =
                                context.Request.Form[configItem.Id] == "true";
                            break;
                        case "world":
                            ServerManager.Servers[id].StartInfo.World = context.Request.Form[configItem.Id];
                            ServerManager.Servers[id].StartInfo.SyncToDatabase();
                            break;
                    }
                }
            }

            ServerManager.Servers[id].BaseInfo.SyncToDatabase();
            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Msg = "修改成功"
            };
        }

        public static ApiReturnBase ApiServerConsoleClean(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            ServerManager.Servers[id].StatusInfo.ConsoleMessages.Clear();
            ServerManager.Servers[id].StatusInfo.OnConsoleOutput("成功清理所有日志");
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功清除".Translate(),
            };
        }

        public static ApiReturnBase ApiServerConsoleInput(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]) || string.IsNullOrEmpty(context.Request.Form["cmd"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            ServerManager.Servers[id].RequestInput(context.Request.Form["cmd"]);
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功发送".Translate(),
            };
        }

        public static ApiReturnBase ApiServerStartInfoUpdate(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["serverId"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["serverId"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            if (nowUser.UserInfo.Can(PermissionId.ChangeCore))
            {
                ServerManager.Servers[id].StartInfo.Core = context.Request.Form["Core"];
            }

            if (nowUser.UserInfo.Can(PermissionId.ChangeStarter))
            {
                ServerManager.Servers[id].StartInfo.Starter = context.Request.Form["Starter"];
            }

            ServerManager.Servers[id].StartInfo.SyncToDatabase();
            return new ApiReturnBase
            {
                Status = true,
                Code = 200,
                Msg = "成功修改".Translate(),
            };
        }

        public static ApiReturnBase ApiServerConfigsList(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            if (!CoreManager.Cores.ContainsKey(ServerManager.Servers[id].StartInfo.Core))
            {
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "核心不存在".Translate(),
                };
            }

            if (!CoreManager.ConfigInfos.ContainsKey(ServerManager.Servers[id].Core.CoreConfig))
            {
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "核心配置不存在".Translate(),
                };
            }


            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Msg = "成功获取",
                Data = ServerManager.Servers[id].Core.ConfigInfo
            };
        }

        public static ApiReturnBase ApiServerConfigContent(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]) ||
                string.IsNullOrEmpty(context.Request.Form["config"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            if (!CoreManager.Cores.ContainsKey(ServerManager.Servers[id].StartInfo.Core))
            {
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "核心不存在".Translate(),
                };
            }

            if (!CoreManager.ConfigInfos.ContainsKey(ServerManager.Servers[id].Core.CoreConfig))
            {
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "核心配置不存在".Translate(),
                };
            }

            if (!int.TryParse(context.Request.Form["config"], out int configId) ||
                ServerManager.Servers[id].Core.ConfigInfo.Count <= configId)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "配置不存在".Translate(),
                };
            return new ApiReturnBase()
            {
                Status = true,
                Code = 200,
                Data = new Dictionary<string, object>()
                {
                    { "configInfo", ServerManager.Servers[id].Core.ConfigInfo[configId] },
                    {
                        "configValue",
                        ServerManager.Servers[id]
                            .GetConfigFileContent(ServerManager.Servers[id].Core.ConfigInfo[configId].File)
                    }
                }
            };
        }

        public static ApiReturnBase ApiServerConfigContentUpdate(HttpContext context)
        {
            var nowUser = ApiHandler.GetCurrentUser(context);
            if (!context.Request.HasFormContentType)
                return ApiReturnBase.IncompleteParameters;
            if (string.IsNullOrEmpty(context.Request.Form["id"]) ||
                string.IsNullOrEmpty(context.Request.Form["config"]) ||
                string.IsNullOrEmpty(context.Request.Form["values"]))
                return ApiReturnBase.IncompleteParameters;
            if (!int.TryParse(context.Request.Form["id"], out var id) || !ServerManager.Servers.ContainsKey(id))
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "服务器未找到".Translate(),
                };
            if (ServerManager.Servers[id].BaseInfo.Owner != nowUser.UserInfo.Id &&
                nowUser.UserInfo.Type < UserType.Technician)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.PermissionDenied,
                    Msg = "权限不足".Translate(),
                };
            if (!CoreManager.Cores.ContainsKey(ServerManager.Servers[id].StartInfo.Core))
            {
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "核心不存在".Translate(),
                };
            }

            if (!CoreManager.ConfigInfos.ContainsKey(ServerManager.Servers[id].Core.CoreConfig))
            {
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "核心配置不存在".Translate(),
                };
            }

            if (!int.TryParse(context.Request.Form["config"], out int configId) ||
                ServerManager.Servers[id].Core.ConfigInfo.Count <= configId)
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.NotFound,
                    Msg = "配置不存在".Translate(),
                };
            try
            {
                ServerManager.Servers[id].WriteConfigFile(ServerManager.Servers[id].Core.ConfigInfo[configId].File,
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(context.Request.Form["values"]));
                return new ApiReturnBase()
                {
                    Status = true,
                    Code = 200,
                    Msg = "成功修改"
                };
            }
            catch (Exception e)
            {
                return new ApiReturnBase
                {
                    Status = false,
                    Code = (int)ApiReturnCode.InternalError,
                    Msg = "保存失败".Translate(),
                    Data = e.Message
                };
            }
        }
    }
}