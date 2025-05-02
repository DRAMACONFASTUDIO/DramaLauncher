using System;
using System.Net;
using System.Net.Http;

namespace Nebula.UpdateResolver.Rest;

public sealed class RestRequestException(HttpContent content, HttpStatusCode statusCode) : Exception
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public HttpContent Content { get; } = content;
}