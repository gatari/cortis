using System;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using UniRx;
using UnityEngine;

namespace Cortis
{
    public static class MessageBinding
    {
        public static Binder Bind<TCommand>(
            ICommandHandler<TCommand> handler,
            IMessageGateway gateway)
            where TCommand : IMessage<TCommand>, new()
        {
            return new Binder(
                SubscribeCommand(handler, gateway));
        }

        static IDisposable SubscribeCommand<TCommand>(
            ICommandHandler<TCommand> handler,
            IMessageGateway gateway)
            where TCommand : IMessage<TCommand>, new()
        {
            var descriptor = new TCommand().Descriptor;
            var typeName = typeof(TCommand).Name;

            return gateway.Messages
                .Where(any => any.Is(descriptor))
                .Select(any =>
                {
                    try
                    {
                        return any.Unpack<TCommand>();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[MessageBinding<{typeName}>] Failed to unpack: {e}");
                        return default;
                    }
                })
                .Where(msg => msg != null)
                .Subscribe(msg =>
                {
                    try { handler.Handle(msg); }
                    catch (Exception e) { Debug.LogError($"[MessageBinding<{typeName}>] Command error: {e}"); }
                });
        }

        // ---- Routed overloads ----

        /// <summary>
        /// Command のみをルーティング付きでバインドする。
        /// </summary>
        public static Binder BindRouted<TRootCommand, TCommand>(
            ICommandHandler<TCommand> handler,
            IMessageGateway gateway,
            Func<TRootCommand, TCommand> unwrapCommand)
            where TRootCommand : IMessage<TRootCommand>, new()
            where TCommand : IMessage<TCommand>
        {
            return new Binder(
                SubscribeRoutedCommand(handler, gateway, unwrapCommand));
        }

        static IDisposable SubscribeRoutedCommand<TRootCommand, TCommand>(
            ICommandHandler<TCommand> handler,
            IMessageGateway gateway,
            Func<TRootCommand, TCommand> unwrapCommand)
            where TRootCommand : IMessage<TRootCommand>, new()
            where TCommand : IMessage<TCommand>
        {
            var descriptor = new TRootCommand().Descriptor;
            var typeName = typeof(TCommand).Name;

            return gateway.Messages
                .Where(any => any.Is(descriptor))
                .Select(any =>
                {
                    try
                    {
                        return any.Unpack<TRootCommand>();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[MessageBinding<{typeName}>] Failed to unpack root: {e}");
                        return default;
                    }
                })
                .Where(msg => msg != null)
                .Select(root =>
                {
                    try
                    {
                        return unwrapCommand(root);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[MessageBinding<{typeName}>] Failed to unwrap command: {e}");
                        return default;
                    }
                })
                .Where(msg => msg != null)
                .Subscribe(msg =>
                {
                    try { handler.Handle(msg); }
                    catch (Exception e) { Debug.LogError($"[MessageBinding<{typeName}>] Command error: {e}"); }
                });
        }
    }
}
