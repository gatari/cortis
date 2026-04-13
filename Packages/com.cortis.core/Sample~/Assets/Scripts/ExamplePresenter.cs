using System;
using Cortis;
using Cortis.Sample;
using UniRx;
using UnityEngine;

namespace Example
{
    // Command+Event パターン（デフォルト: Subject）
    // Generator が生成するもの: Handle dispatch, SendEvent, IEventSource interface
    // ユーザーが書くもの: _events, Events, Handle*, OnInitialize でパイプライン接続
    [ProtoHandler(typeof(Cube.Types.Command), typeof(Cube.Types.Event))]
    public sealed partial class ExamplePresenter
    {
        readonly Transform _target;
        readonly Subject<Cube.Types.Event> _events = new();
        public IObservable<Cube.Types.Event> Events => _events;

        public ExamplePresenter(Transform target)
        {
            _target = target;
        }

        void HandleSetScale(Cube.Types.Command.Types.SetScale cmd)
        {
            var scale = new Vector3(cmd.X, cmd.Y, cmd.Z);
            _target.localScale = scale;

            _events.OnNext(new Cube.Types.Event
            {
                ScaleChanged = new Cube.Types.Event.Types.ScaleChanged
                {
                    X = scale.x, Y = scale.y, Z = scale.z,
                }
            });
        }

        void HandleReset(Cube.Types.Command.Types.Reset cmd)
        {
            _target.localScale = Vector3.one;

            _events.OnNext(new Cube.Types.Event
            {
                ScaleChanged = new Cube.Types.Event.Types.ScaleChanged
                {
                    X = 1f, Y = 1f, Z = 1f,
                }
            });
        }

        private partial void OnInitialize()
        {
            _events
                .DistinctUntilChanged()
                .Subscribe(evt => SendEvent(evt));
        }

        private partial void OnDispose()
        {
            _events.Dispose();
        }
    }

    // ReactiveProperty を使う例
    // DistinctUntilChanged が内蔵されている
    [ProtoHandler(typeof(Cube.Types.Command), typeof(Cube.Types.Event))]
    public sealed partial class ReactivePresenter
    {
        readonly ReactiveProperty<Cube.Types.Event> _state = new();
        public IObservable<Cube.Types.Event> Events => _state;

        void HandleSetScale(Cube.Types.Command.Types.SetScale cmd)
        {
            _state.Value = new Cube.Types.Event
            {
                ScaleChanged = new Cube.Types.Event.Types.ScaleChanged
                {
                    X = cmd.X, Y = cmd.Y, Z = cmd.Z,
                }
            };
        }

        void HandleReset(Cube.Types.Command.Types.Reset cmd)
        {
            _state.Value = new Cube.Types.Event
            {
                ScaleChanged = new Cube.Types.Event.Types.ScaleChanged
                {
                    X = 1f, Y = 1f, Z = 1f,
                }
            };
        }

        private partial void OnInitialize()
        {
            _state.Subscribe(evt => SendEvent(evt));
        }

        private partial void OnDispose()
        {
            _state.Dispose();
        }
    }
}
