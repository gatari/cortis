using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NUnit.Framework;

namespace Cortis.Tests.EditMode
{
    /// <summary>
    /// MessageBinding.Bind&lt;TCommand&gt; のテスト。
    /// Event 自動 Subscribe はなくなったため、Command のみをバインドする。
    /// ユーザーコードがイベント送信を行うパターンを検証する。
    /// </summary>
    public class BinderTwoTypeTests
    {
        TestGateway _gateway;
        RecordingHandler<StringValue> _commandHandler;
        RecordingHandler<Int32Value> _eventLoopbackHandler;
        Binder _binder;
        Binder _eventCatcher;

        [SetUp]
        public void SetUp()
        {
            _gateway = new TestGateway();
            _commandHandler = new RecordingHandler<StringValue>();
            _eventLoopbackHandler = new RecordingHandler<Int32Value>();

            // ループバックされたイベントを受信する Binder
            _eventCatcher = MessageBinding.Bind<Int32Value>(_eventLoopbackHandler, _gateway);

            // Command のみバインド（Event 自動 Subscribe はなし）
            _binder = MessageBinding.Bind<StringValue>(
                _commandHandler, _gateway);
        }

        [TearDown]
        public void TearDown()
        {
            _binder.Dispose();
            _eventCatcher.Dispose();
            _gateway.Dispose();
        }

        [Test]
        public void コマンドメッセージがハンドラに届く()
        {
            _gateway.SimulateReceive(Any.Pack(new StringValue { Value = "cmd" }));

            Assert.AreEqual(1, _commandHandler.Received.Count);
            Assert.AreEqual("cmd", _commandHandler.Received[0].Value);
        }

        [Test]
        public void ユーザーコードがgateway経由でイベントを送信するとループバックで受信できる()
        {
            _gateway.Send(Any.Pack(new Int32Value { Value = 42 }));

            Assert.AreEqual(1, _eventLoopbackHandler.Received.Count);
            Assert.AreEqual(42, _eventLoopbackHandler.Received[0].Value);
        }

        [Test]
        public void Dispose後_コマンドは処理されない()
        {
            _binder.Dispose();

            _gateway.SimulateReceive(Any.Pack(new StringValue { Value = "after" }));

            Assert.AreEqual(0, _commandHandler.Received.Count);
        }

        [Test]
        public void イベント型のバイト列はコマンドハンドラに届かない()
        {
            _gateway.SimulateReceive(Any.Pack(new Int32Value { Value = 100 }));

            Assert.AreEqual(0, _commandHandler.Received.Count);
        }
    }
}
