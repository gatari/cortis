using System;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using R3;
using UnityEngine;

namespace Example
{
    /// <summary>
    /// IMessageGateway のサンプル実装。
    /// 実運用では FlutterUnityIntegration 経由でバイト列を送受信する。
    /// </summary>
    public sealed class FlutterMessageGateway : Cortis.IMessageGateway, IDisposable
    {
        readonly Subject<Any> _messages = new();

        public Observable<Any> Messages => _messages;

        public void Send(Any packed)
        {
            Debug.Log($"[FlutterMessageGateway] Send: {packed.TypeUrl}");
        }

        public void OnMessageReceived(string base64)
        {
            try
            {
                var bytes = Convert.FromBase64String(base64);
                var any = Any.Parser.ParseFrom(bytes);
                _messages.OnNext(any);
            }
            catch (Exception e)
            {
                Debug.LogError($"[FlutterMessageGateway] Failed to parse message: {e}");
            }
        }

        public void Dispose()
        {
            _messages.OnCompleted();
            _messages.Dispose();
        }
    }
}
