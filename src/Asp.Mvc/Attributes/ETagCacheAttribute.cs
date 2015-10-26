using System;
using System.IO;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace Asp.Mvc.Attributes
{
    public class ETagCacheAttribute : ActionFilterAttribute
    {
        private readonly string _dependencyKeyPrefix;
        private readonly string[] _dependencyKeyParams;

        public ETagCacheAttribute(string dependencyKey, params string[] dependencyParameterNames)
        {
            _dependencyKeyPrefix = dependencyKey;
            _dependencyKeyParams = dependencyParameterNames;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.Filter = new ETagStream(filterContext.HttpContext.Response, filterContext.RequestContext.HttpContext.Request);
        }
    }

    public class ETagStream : MemoryStream
    {
        private readonly HttpResponseBase _response;
        private readonly HttpRequestBase _request;
        private readonly Stream _filter;

        public ETagStream(HttpResponseBase response, HttpRequestBase request)
        {
            _response = response;
            _request = request;
            _filter = response.Filter;
        }

        private static string GetToken(Stream stream)
        {
            byte[] checksum = MD5.Create().ComputeHash(stream);
            return Convert.ToBase64String(checksum, 0, checksum.Length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] data = new byte[count];
            Buffer.BlockCopy(buffer, offset, data, 0, count);
            var token = GetToken(new MemoryStream(data));

            string clientToken = _request.Headers["If-None-Match"];

            if (token != clientToken)
            {
                _response.Headers["ETag"] = token;
                _filter.Write(data, 0, count);
            }
            else
            {
                _response.SuppressContent = true;
                _response.StatusCode = 304;
                _response.StatusDescription = "Not Modified";
                _response.Headers["Content-Length"] = "0";
            }
        }
    }
}