using Cortis;
using Cortis.Sample;
using R3;
using UnityEngine;

namespace Example
{
    // ルーティング: inner 型を指定すると root 型が自動発見される
    [ProtoHandler(typeof(Player.Types.Command), typeof(Player.Types.Event))]
    public sealed partial class RoutedPresenter
    {
        int _hp = 100;
        readonly Subject<Player.Types.Event> _events = new();
        public Observable<Player.Types.Event> Events => _events;

        void HandleAttack(Player.Types.Command.Types.Attack cmd)
        {
            _hp -= cmd.Damage;
            Debug.Log($"Attacked! HP: {_hp}");

            _events.OnNext(new Player.Types.Event
            {
                HealthChanged = new Player.Types.Event.Types.HealthChanged { Hp = _hp }
            });
        }

        void HandleDefend(Player.Types.Command.Types.Defend cmd)
        {
            Debug.Log($"Defending with shield: {cmd.Shield}");
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
}
