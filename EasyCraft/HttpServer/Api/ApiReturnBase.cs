using EasyCraft.Utils;
using Newtonsoft.Json;

namespace EasyCraft.HttpServer.Api
{
    public class ApiReturnBase
    {
        [JsonProperty("status")] public bool Status { get; set; }

        [JsonProperty("code")]
        public int Code
        {
            get => _code * (Status ? 1 : -1);
            set => _code = value;
        }

        private int _code;
        [JsonProperty("msg")] public string Msg { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public object Data { get; set; }

        public static ApiReturnBase ApiNotFound = new()
            { Status = false, Code = (int)ApiReturnCode.NotFound, Msg = "所请求 API 未提供".Translate() };

        public static ApiReturnBase PermissionDenied = new()
            { Status = false, Code = (int)ApiReturnCode.PermissionDenied, Msg = "权限不足".Translate() };

        public static ApiReturnBase NotLogin = new()
            { Status = false, Code = (int)ApiReturnCode.NeedLogin, Msg = "未登录".Translate() };

        
        public static ApiReturnBase InternalError = new()
            { Status = false, Code = (int)ApiReturnCode.InternalError, Msg = "内部错误".Translate() };

        public static ApiReturnBase ApiNotImplemented = new()
            { Status = false, Code = (int)ApiReturnCode.NotImplemented, Msg = "API 未实现".Translate() };

        public static ApiReturnBase IncompleteParameters = new()
            { Status = false, Code = (int)ApiReturnCode.IncompleteParameters, Msg = "参数不全".Translate() };
    }

    public enum ApiReturnCode
    {
        None,
        PermissionDenied,
        IncompleteParameters,
        RequestFailed,
        ServerExpired,
        CoreNotFound,
        PluginReject,
        UserNameOccupied,
        PasswordNotIdentical,
        IncorrectPasswordFormat,
        NeedLogin,
        SystemNotSupport,
        

        // 下方遵循 HTTP 响应码
        // Refer https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Status
        BadRequest = 400,
        Unauthorized = 401,
        NotFound = 404,
        ImaCreeper = 418,
        InternalError = 500,
        NotImplemented = 501
    }
}