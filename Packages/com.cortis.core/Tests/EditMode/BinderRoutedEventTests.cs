using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NUnit.Framework;
using UnityEngine;

namespace Cortis.Tests.EditMode
{
    /// <summary>
    /// ルーティングあり Event 送信テスト。
    /// Event 自動 Subscribe はなくなったため、ユーザーコードが
    /// inner → root へ wrap して gateway.Send を呼ぶパターンをテストする。
    /// </summary>
    public class BinderRoutedEventTests
    {
        const string FieldKey = "inner";

        TestGateway _gateway;
        RecordingHandler<Struct> _loopbackHandler;
        Binder _loopbackReceiver;

        static Struct WrapToStruct(StringValue inner)
        {
            var root = new Struct();
            root.Fields[FieldKey] = new Value { StringValue = inner.Value };
            return root;
        }

        [SetUp]
        public void SetUp()
        {
            _gateway = new TestGateway();
            _loopbackHandler = new RecordingHandler<Struct>();

            _loopbackReceiver = MessageBinding.Bind<Struct>(_loopbackHandler, _gateway);
        }

        [TearDown]
        public void TearDown()
        {
            _loopbackReceiver.Dispose();
            _gateway.Dispose();
        }

        [Test]
        public void inner型イベントをwrapしてgateway送信するとroot型としてループバックで受信できる()
        {
            var inner = new StringValue { Value = "hello" };
            var root = WrapToStruct(inner);
            _gateway.Send(Any.Pack(root));

            Assert.AreEqual(1, _loopbackHandler.Received.Count);
            Assert.IsTrue(_loopbackHandler.Received[0].Fields.ContainsKey(FieldKey));
            Assert.AreEqual("hello", _loopbackHandler.Received[0].Fields[FieldKey].StringValue);
        }

        [Test]
        public void 異なるイベントが連続した場合_それぞれ到達する()
        {
            _gateway.Send(Any.Pack(WrapToStruct(new StringValue { Value = "a" })));
            _gateway.Send(Any.Pack(WrapToStruct(new StringValue { Value = "b" })));
            _gateway.Send(Any.Pack(WrapToStruct(new StringValue { Value = "a" })));

            Assert.AreEqual(3, _loopbackHandler.Received.Count);
        }

        [Test]
        public void Dispose後_イベントが発行されてもループバックされない()
        {
            _loopbackReceiver.Dispose();

            _gateway.Send(Any.Pack(WrapToStruct(new StringValue { Value = "after dispose" })));

            Assert.AreEqual(0, _loopbackHandler.Received.Count);
        }
    }
}
