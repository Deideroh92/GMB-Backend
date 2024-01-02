using Microsoft.AspNetCore.Mvc;

namespace GMB.Sdk.Core.Types.Api
{
    /// <summary>
    /// Generic Response inherited.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class
    GenericResponse<T> where T : GenericResponse<T>, new()
    {
        static readonly string badAuthMessage = "Bad credentials or group ACL";

        /// <summary>
        /// Success.
        /// </summary>
        public bool Success { get; set; } = true;
        /// <summary>
        /// Rescode.
        /// </summary>
        public long Rescode { get; set; }
        /// <summary>
        /// Error message.
        /// </summary>
        public string? Message { get; set; }
        /// <summary>
        /// Used to return any Id (for creation or other).
        /// </summary>
        public long? ObjectId { get; set; }

        /// <summary>
        /// DO NOT USE THIS CONSTRUCTOR!
        /// </summary>
        public GenericResponse() { }

        /// <summary>
        /// Bad authentication (401).
        /// </summary>
        /// <returns></returns>
        public static ActionResult<T>
        BadAuth()
        {
            T response = new()
            {
                Success = false,
                Rescode = -100,
                Message = badAuthMessage
            };

            return response;

            // Return 401
            //return new UnauthorizedObjectResult(response);
        }

        /// <summary>
        /// Exception (500).
        /// </summary>
        /// <param name="message"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public static ActionResult<T>
        Exception(string message, long objectId = 0)
        {
            T response = new()
            {
                Success = false,
                Rescode = -1000,
                ObjectId = objectId,
                Message = message.Trim()
            };

            return response;

            // Return 500
            //return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Failure (400).
        /// </summary>
        /// <param name="rescode"></param>
        /// <param name="message"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public static ActionResult<T>
        Fail(long rescode, string message, long objectId = 0)
        {
            T response = new()
            {
                Success = false,
                Rescode = rescode,
                Message = message.Trim(),
                ObjectId = objectId
            };

            return response;

            // Return 400
            //return new BadRequestObjectResult(response);
        }

        /// <summary>
        /// CORS options.
        /// </summary>

        /// <returns></returns>
        public static T
        CorsOptions()
        {
            T response = new()
            {
                Success = false
            };
            return response;
        }
    };

    /// <summary>
    /// Generic Response.
    /// Used for success.
    /// </summary>
    public sealed class
    GenericResponse : GenericResponse<GenericResponse>
    {
        /// <summary>
        /// DO NOT USE THIS CONSTRUCTOR!
        /// </summary>
        public GenericResponse() { }

        /// <summary>
        /// Success constructor.
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="message"></param>
        public
        GenericResponse(long? objectId = null, string? message = null)
        {
            ObjectId = objectId;
            Message = message.Trim();
        }

        /// <summary>
        /// Success constructor.
        /// </summary>
        /// <param name="rescode"></param>
        /// <param name="message"></param>
        /// <param name="objectId"></param>
        public
        GenericResponse(long rescode, string message, long objectId)
        {
            Rescode = rescode;
            Message = message.Trim();
            ObjectId = objectId;
        }
    };

    /// <summary>
    /// Generic Responses.
    /// Used for success.
    /// </summary>
    public sealed class
    GenericResponses : GenericResponse<GenericResponses>
    {
        /// <summary>
        /// List of responses.
        /// </summary>
        public List<GenericResponse>? Responses { get; set; } = null;

        /// <summary>
        /// DO NOT USE THIS CONSTRUCTOR!
        /// </summary>
        public GenericResponses() { }

        /// <summary>
        /// Success constructor.
        /// </summary>
        /// <param name="responses"></param>
        public
        GenericResponses(List<GenericResponse> responses) => Responses = responses;
    };
}
