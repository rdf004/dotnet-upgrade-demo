using System;

namespace ContosoCommerce.Core.DTOs
{
    /// <summary>
    /// Standard API response wrapper for all
    /// endpoints in the ContosoCommerce platform.
    /// </summary>
    [Serializable]
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the request succeeded.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Human-readable status message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The response payload.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// UTC timestamp of the response.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Creates a successful response.
        /// </summary>
        public static ApiResponse<T> Ok(
            T data,
            string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a failure response.
        /// </summary>
        public static ApiResponse<T> Fail(
            string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default(T),
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
