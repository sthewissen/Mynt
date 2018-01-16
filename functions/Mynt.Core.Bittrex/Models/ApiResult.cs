using System;
using Newtonsoft.Json;

namespace Mynt.Core.Bittrex.Models
{
    /// <summary>
    /// A general result wrapper for the bittrex api end points
    /// Every end point provides results in the same format containing a success flag, a message field to return any errors that may have occurred and the actual json result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T> 
    {
        public ApiResult(bool success, string message, T result)
        {
            Success = success;
            Message = message;
            Result = result;
        }     

        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
        [JsonProperty(PropertyName = "result")]
        public T Result { get; set; }
    }
}
