using System;
using Cortis;
using Cortis.Sample;
using UniRx;
using UnityEngine;

namespace Example
{
    // Event-only: Command 受信なし、イベント発行のみ
    [ProtoHandler(null, typeof(Sensor.Types.Event))]
    public sealed partial class EventOnlyPresenter
    {
        readonly Transform _sensor;
        readonly Subject<Sensor.Types.Event> _events = new();
        public IObservable<Sensor.Types.Event> Events => _events;

        public EventOnlyPresenter(Transform sensor)
        {
            _sensor = sensor;
        }

        private partial void OnInitialize()
        {
            _events
                .DistinctUntilChanged()
                .Subscribe(evt => SendEvent(evt));
        }

        public void UpdatePosition()
        {
            var pos = _sensor.position;
            _events.OnNext(new Sensor.Types.Event
            {
                PositionUpdated = new Sensor.Types.Event.Types.PositionUpdated
                {
                    X = pos.x, Y = pos.y, Z = pos.z,
                }
            });
        }

        private partial void OnDispose()
        {
            _events.Dispose();
        }
    }
}
