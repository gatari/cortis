using Cortis;
using Cortis.Sample;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Example
{
    public sealed class ExampleLifetimeScope : LifetimeScope
    {
        [SerializeField] Transform target;
        [SerializeField] Transform sensor;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<FlutterMessageGateway>(Lifetime.Singleton)
                .As<IMessageGateway>();

            ExamplePresenter.Register(builder, Lifetime.Singleton);
            ReactivePresenter.Register(builder, Lifetime.Singleton);
            EventOnlyPresenter.Register(builder, Lifetime.Singleton);
            RoutedPresenter.Register(builder, Lifetime.Singleton);
        }
    }
}
