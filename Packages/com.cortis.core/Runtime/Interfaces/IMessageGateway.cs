using System;
using Google.Protobuf.WellKnownTypes;
using UniRx;

namespace Cortis
{
    public interface IMessageGateway
    {
        IObservable<Any> Messages { get; }
        void Send(Any packed);
    }
}
