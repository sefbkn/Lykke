using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Lykke.Service.Decred.Api.Middleware
{    
    public class ApiErrorHandler
    {
        private readonly RequestDelegate _next;

        public ApiErrorHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {            
            var code = HttpStatusCode.InternalServerError;            
            var result = JsonConvert.SerializeObject(new { error = exception.Message, stacktrace = exception.ToString() });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int) code;
            return context.Response.WriteAsync(result);
        }
    }
}

