using System;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NUnit.Framework;
using UniRx;
using UnityEngine;

namespace Cortis.Tests.EditMode
{
    /// <summary>
    /// Event 送信テスト。
    /// Event 自動 Subscribe はなくなったため、ユーザーコードが
    /// gateway.Send(Any.Pack(evt)) を呼ぶパターンをテストする。
    /// </summary>
    public class BinderEventTests
    {
        TestGateway _gateway;
        RecordingHandler<StringValue> _loopbackHandler;
        Binder _loopbackReceiver;

        [SetUp]
        public void SetUp()
        {
            _gateway = new TestGateway();
            _loopbackHandler = new RecordingHandler<StringValue>();

            // ループバック受信側をセットアップ
            _loopbackReceiver = MessageBinding.Bind<StringValue>(_loopbackHandler, _gateway);
        }

        [TearDown]
        public void TearDown()
        {
            _loopbackReceiver.Dispose();
            _gateway.Dispose();
        }

        [Test]
        public void gateway経由でイベントを送信するとprotobufシリアライズを経てループバックで受信できる()
        {
            _gateway.Send(Any.Pack(new StringValue { Value = "hello" }));

            Assert.AreEqual(1, _loopbackHandler.Received.Count);
            Assert.AreEqual("hello", _loopbackHandler.Received[0].Value);
        }

        [Test]
        public void 異なるイベントが連続した場合_それぞれ到達する()
        {
            _gateway.Send(Any.Pack(new StringValue { Value = "a" }));
            _gateway.Send(Any.Pack(new StringValue { Value = "b" }));
            _gateway.Send(Any.Pack(new StringValue { Value = "a" }));

            Assert.AreEqual(3, _loopbackHandler.Received.Count);
            Assert.AreEqual("a", _loopbackHandler.Received[0].Value);
            Assert.AreEqual("b", _loopbackHandler.Received[1].Value);
            Assert.AreEqual("a", _loopbackHandler.Received[2].Value);
        }

        [Test]
        public void Dispose後_イベントが発行されてもループバックされない()
        {
            _loopbackReceiver.Dispose();

            _gateway.Send(Any.Pack(new StringValue { Value = "after dispose" }));

            Assert.AreEqual(0, _loopbackHandler.Received.Count);
        }

        [Test]
        public void DistinctUntilChanged付きパイプラインで同じイベントを連続送信した場合_1回のみ到達する()
        {
            var subject = new Subject<StringValue>();
            subject
                .DistinctUntilChanged()
                .Subscribe(evt => _gateway.Send(Any.Pack(evt)));

            var same = new StringValue { Value = "dup" };
            subject.OnNext(same);
            subject.OnNext(same);
            subject.OnNext(same);

            Assert.AreEqual(1, _loopbackHandler.Received.Count);
            Assert.AreEqual("dup", _loopbackHandler.Received[0].Value);

            subject.Dispose();
        }

        [Test]
        public void DistinctUntilChangedなしで同じイベントを連続送信した場合_全て到達する()
        {
            var subject = new Subject<StringValue>();
            subject
                .Subscribe(evt => _gateway.Send(Any.Pack(evt)));

            var same = new StringValue { Value = "dup" };
            subject.OnNext(same);
            subject.OnNext(same);
            subject.OnNext(same);

            Assert.AreEqual(3, _loopbackHandler.Received.Count);
            Assert.AreEqual("dup", _loopbackHandler.Received[0].Value);
            Assert.AreEqual("dup", _loopbackHandler.Received[1].Value);
            Assert.AreEqual("dup", _loopbackHandler.Received[2].Value);

            subject.Dispose();
        }
    }
}
