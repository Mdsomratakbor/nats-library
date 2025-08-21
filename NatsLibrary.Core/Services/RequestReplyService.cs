using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NatsLibrary.Core.Interfaces;
using NatsLibrary.Core.Utils;

namespace NatsLibrary.Core.Services;

public class RequestReplyService : IRequestReplyHandler
{
    private readonly NatsService _natsService;

    public RequestReplyService(NatsService natsService)
    {
        _natsService = natsService ?? throw new ArgumentNullException(nameof(natsService));
    }

    /// <summary>
    /// Send request and await response
    /// </summary>
    public async Task<TResponse?> RequestAsync<TRequest, TResponse>(string subject, TRequest request, int timeoutMs = 5000)
    {
        var payload = JsonSerializerHelper.Serialize(request);
        var data = Encoding.UTF8.GetBytes(payload);

        if (_natsService.JetStream != null)
        {
            // JetStream request-response uses regular NATS request (JS context is mostly for persistence)
            var inbox = _natsService.Connection.NewInbox();
            var tcs = new TaskCompletionSource<TResponse?>();

            using var sub = _natsService.Connection.SubscribeAsync(inbox);
            sub.MessageHandler += (sender, args) =>
            {
                var respData = Encoding.UTF8.GetString(args.Message.Data);
                var obj = JsonSerializerHelper.Deserialize<TResponse>(respData);
                tcs.TrySetResult(obj);
            };
            sub.Start();

            _natsService.Connection.Publish(subject, inbox, data);
            _natsService.Connection.Flush();

            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));
            return completedTask == tcs.Task ? tcs.Task.Result : default;
        }
        else
        {
            // Normal NATS
            var msg = _natsService.Connection.Request(subject, data, timeoutMs);
            var respData = Encoding.UTF8.GetString(msg.Data);
            return JsonSerializerHelper.Deserialize<TResponse>(respData);
        }
    }

    /// <summary>
    /// Handle incoming requests on a subject
    /// </summary>
    public async Task ReplyAsync<TRequest, TResponse>(string subject, Func<TRequest, Task<TResponse>> handler)
    {
        if (_natsService.JetStream != null)
        {
            var sub = _natsService.Connection.SubscribeAsync(subject);
            sub.MessageHandler += async (sender, args) =>
            {
                try
                {
                    var reqData = Encoding.UTF8.GetString(args.Message.Data);
                    var reqObj = JsonSerializerHelper.Deserialize<TRequest>(reqData);
                    if (reqObj != null)
                    {
                        var respObj = await handler(reqObj);
                        var respData = Encoding.UTF8.GetBytes(JsonSerializerHelper.Serialize(respObj));
                        _natsService.Connection.Publish(args.Message.Reply, respData);
                        _natsService.Connection.Flush();
                    }
                }
                catch
                {
                    // optionally log errors
                }
            };
            sub.Start();
        }
        else
        {
            var sub = _natsService.Connection.SubscribeAsync(subject);
            sub.MessageHandler += async (sender, args) =>
            {
                try
                {
                    var reqData = Encoding.UTF8.GetString(args.Message.Data);
                    var reqObj = JsonSerializerHelper.Deserialize<TRequest>(reqData);
                    if (reqObj != null)
                    {
                        var respObj = await handler(reqObj);
                        var respData = Encoding.UTF8.GetBytes(JsonSerializerHelper.Serialize(respObj));
                        _natsService.Connection.Publish(args.Message.Reply, respData);
                        _natsService.Connection.Flush();
                    }
                }
                catch
                {
                    // optionally log errors
                }
            };
            sub.Start();
        }
    }
}
