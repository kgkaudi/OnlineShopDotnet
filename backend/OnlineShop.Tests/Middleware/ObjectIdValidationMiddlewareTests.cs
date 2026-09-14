using FluentAssertions;
using Microsoft.AspNetCore.Http;
using OnlineShop.Api.Middleware;
using Xunit;
using MongoDB.Bson;
using System.IO;

public class ObjectIdValidationMiddlewareTests
{
    private static DefaultHttpContext CreateContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_InvalidIdInRoute()
    {
        var context = CreateContext();
        context.Request.RouteValues["id"] = "invalid-id";

        var middleware = new ObjectIdValidationMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        new StreamReader(context.Response.Body).ReadToEnd()
            .Should().Contain("Invalid ObjectId");
    }

    [Fact]
    public async Task Should_Continue_When_ValidId()
    {
        var context = CreateContext();
        context.Request.RouteValues["id"] = ObjectId.GenerateNewId().ToString();

        var called = false;
        var middleware = new ObjectIdValidationMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        called.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_WhitespaceId()
    {
        var context = CreateContext();
        context.Request.RouteValues["id"] = "   ";

        var middleware = new ObjectIdValidationMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_EmptyStringId()
    {
        var context = CreateContext();
        context.Request.RouteValues["id"] = "";

        var middleware = new ObjectIdValidationMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_IdIsNotString()
    {
        var context = CreateContext();
        context.Request.RouteValues["id"] = 12345; // int

        var middleware = new ObjectIdValidationMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_MixedValidAndInvalidIds()
    {
        var context = CreateContext();
        context.Request.RouteValues["orderId"] = ObjectId.GenerateNewId().ToString();
        context.Request.RouteValues["itemId"] = "invalid-id";

        var middleware = new ObjectIdValidationMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Should_Ignore_NonIdRouteValues()
    {
        var context = CreateContext();
        context.Request.RouteValues["product"] = "whatever";

        var called = false;
        var middleware = new ObjectIdValidationMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        called.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Should_Handle_EmptyRouteValues()
    {
        var context = CreateContext();

        var called = false;
        var middleware = new ObjectIdValidationMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        called.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_NullRouteValue()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.RouteValues["id"] = null;

        var middleware = new ObjectIdValidationMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        new StreamReader(context.Response.Body).ReadToEnd()
            .Should().Contain("Invalid ObjectId");
    }

    [Fact]
    public async Task Should_Handle_CaseInsensitiveIdKey()
    {
        var context = CreateContext();
        context.Request.RouteValues["ProductID"] = "invalid-id";

        var middleware = new ObjectIdValidationMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
    }
}
