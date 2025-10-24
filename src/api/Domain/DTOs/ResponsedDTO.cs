using System.Dynamic;
using System.Net;

namespace BuscarEnderecos.API.DTOs
{
    public class ResponseDTO<T> where T : class
    {
        public HttpStatusCode HttpCode { get; set; }
        public T? ResponseData { get; set; } = default;
        public ExpandoObject? ResponseError { get; set; } = null;
    }
}