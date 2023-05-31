using Drastic.Services;

namespace Drastic.Gpt4All.Net.MauiSample.Services
{
    public class MauiAppDispatcherService : IAppDispatcher
    {
        private IDispatcher dispatcher;

        public MauiAppDispatcherService(IServiceProvider provider)
        {
            this.dispatcher = provider.GetRequiredService<IDispatcher>();
        }

        public bool Dispatch(Action action)
        {
            return this.dispatcher.Dispatch(action);
        }
    }
}
