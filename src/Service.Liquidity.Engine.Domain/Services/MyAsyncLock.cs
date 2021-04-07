using System;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Liquidity.Engine.Domain.Services
{
    public class MyAsyncLock
    {
        private readonly SemaphoreSlim _slim = new SemaphoreSlim(1, 1);

        public async Task<IDisposable> Lock()
        {
            await _slim.WaitAsync();
            return new Unlock(_slim);
        }

        public class Unlock: IDisposable
        {
            private SemaphoreSlim _slim;

            public Unlock(SemaphoreSlim slim)
            {
                _slim = slim;
            }

            public void Dispose()
            {
                _slim?.Release();
                _slim = null;
            }
        }
    }
}