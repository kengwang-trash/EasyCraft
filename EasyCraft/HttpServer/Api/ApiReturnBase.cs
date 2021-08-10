using EasyCraft.Utils;
using Newtonsoft.Json;

namespace EasyCraft.HttpServer.Api
{
    public struct ApiReturnBase
    {
        [JsonProperty("status")] public bool Status { get; set; }

        [JsonProperty("code")]
        public int Code
        {
            get { return _code * (Status ? 1 : -1); }
            set { _code = value; }
        }

        private int _code;
        [JsonProperty("msg")] public string Msg { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public object Data { get; set; }

        public static ApiReturnBase ApiNotFound = new ApiReturnBase()
            {Status = false, Code = (int) ApiErrorCode.NotFound, Msg = "所请求 API 未提供".Translate()};

        public static ApiReturnBase PermissionDenied = new ApiReturnBase()
            {Status = false, Code = (int) ApiErrorCode.PermissionDenied, Msg = "权限不足".Translate()};

        public static ApiReturnBase InternalError = new ApiReturnBase()
            {Status = false, Code = (int) ApiErrorCode.InternalError, Msg = "内部错误".Translate()};

        public static ApiReturnBase ApiNotImplemented = new ApiReturnBase()
            {Status = false, Code = (int) ApiErrorCode.NotImplemented, Msg = "API 未实现".Translate()};

        public static ApiReturnBase IncompleteParameters = new ApiReturnBase()
            {Status = false, Code = (int) ApiErrorCode.IncompleteParameters, Msg = "参数不全".Translate()};
    }

    public enum ApiErrorCode
    {
        None,
        PermissionDenied,
        IncompleteParameters,
        UserNotFound,

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