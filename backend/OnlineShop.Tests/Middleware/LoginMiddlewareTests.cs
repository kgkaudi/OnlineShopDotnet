using FluentAssertions;
using Microsoft.AspNetCore.Http;
using OnlineShop.Api.Middleware;
using Xunit;
using System.Security.Claims;
using MongoDB.Bson;

public class LoginMiddlewareTests
{
    private static DefaultHttpContext CreateContext()
    {
        return new DefaultHttpContext();
    }

    [Fact]
    public async Task Should_SetUserId_When_SubClaimExists()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var context = CreateContext();

        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("sub", userId) }, "test")
        );

        var called = false;
        var middleware = new LoginMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        called.Should().BeTrue();
        context.Items["UserId"].Should().Be(userId);
    }

    [Fact]
    public async Task Should_TrimUserId_When_WhitespacePresent()
    {
        var raw = $" {ObjectId.GenerateNewId()} ";
        var context = CreateContext();

        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("sub", raw) }, "test")
        );

        var middleware = new LoginMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Items["UserId"].Should().Be(raw.Trim());
    }

    [Fact]
    public async Task Should_NotSetUserId_When_SubClaimMissing()
    {
        var context = CreateContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity());

        var called = false;
        var middleware = new LoginMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        called.Should().BeTrue();
        context.Items.ContainsKey("UserId").Should().BeFalse();
    }

    [Fact]
    public async Task Should_NotSetUserId_When_SubClaimIsEmpty()
    {
        var context = CreateContext();

        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("sub", "") }, "test")
        );

        var middleware = new LoginMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Items.ContainsKey("UserId").Should().BeFalse();
    }

    [Fact]
    public async Task Should_NotSetUserId_When_SubClaimIsWhitespace()
    {
        var context = CreateContext();

        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("sub", "   ") }, "test")
        );

        var middleware = new LoginMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Items.ContainsKey("UserId").Should().BeFalse();
    }

    [Fact]
    public async Task Should_Handle_NullUserPrincipal()
    {
        var context = CreateContext();
        context.User = null!;

        var called = false;
        var middleware = new LoginMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        called.Should().BeTrue();
        context.Items.ContainsKey("UserId").Should().BeFalse();
    }

    [Fact]
    public async Task Should_NotThrow_When_NoIdentityPresent()
    {
        var context = CreateContext();
        context.User = new ClaimsPrincipal(); // empty principal

        var middleware = new LoginMiddleware(_ => Task.CompletedTask);

        Func<Task> act = async () => await middleware.InvokeAsync(context);

        await act.Should().NotThrowAsync();
        context.Items.ContainsKey("UserId").Should().BeFalse();
    }

    [Fact]
    public async Task Should_NotOverrideExistingUserId()
    {
        var context = CreateContext();
        context.Items["UserId"] = "existing-user";

        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("sub", ObjectId.GenerateNewId().ToString()) }, "test")
        );

        var middleware = new LoginMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Items["UserId"].Should().Be("existing-user");
    }

    [Fact]
    public async Task Should_ContinuePipeline_When_UserIdIsSet()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var context = CreateContext();

        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("sub", userId) }, "test")
        );

        var called = false;
        var middleware = new LoginMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        called.Should().BeTrue();
    }

    [Fact]
    public async Task Should_ContinuePipeline_When_UserIdIsNotSet()
    {
        var context = CreateContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity());

        var called = false;
        var middleware = new LoginMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        called.Should().BeTrue();
    }
}
