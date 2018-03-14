using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Lykke.Service.Decred.SignService.Models
{
    /// <summary>
    /// Returned as response to all invalid controllers
    /// </summary>
    public class ErrorResponse
    {
        [DataMember(Name = "errorMessage")]
        public string Message { get; }

        [DataMember(Name = "modelErrors")]
        public Dictionary<string, string[]> Errors { get; }

        public ErrorResponse(string message = null)
        {
            Message = message;
            Errors = new Dictionary<string, string[]>();
        }
        
        public ErrorResponse(string message, ModelStateDictionary modelState) : this(message)
        {
            Message = message;            
            AddErrors(modelState);
        }
        
        private void AddErrors(string key, params string[] message)
        {
            if (Errors.ContainsKey(key))
                return;
            Errors.Add(key, message);
        }

        private void AddErrors(ModelStateDictionary modelState)
        {
            var errors =
                from entry in modelState
                select new {
                    entry.Key, 
                    Value = entry.Value.Errors.Select(error => error.ErrorMessage ?? error.Exception.Message)
                };
            
            foreach(var error in errors)
                AddErrors(error.Key, error.Value.ToArray());
        }
    }
}
