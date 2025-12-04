namespace APi.Services
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next,ILogger<GlobalExceptionMiddleware> logeer)
        {
            _next = next;
            _logger = logeer;
        }
        public async Task Invoke(HttpContext context) {
            try
            {
                await _next(context);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Đã xảy ra lỗi không mong muốn");
                await HandleExceptionAsync(context, ex);
            }

        }
        public static Task HandleExceptionAsync(HttpContext context,Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var result = new
            {
                Success = false,
                Message = "Có lỗi xảy ra ",
                error = exception.Message,
            };
            return  context.Response.WriteAsJsonAsync(result);
        }
    }
}
