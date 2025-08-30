using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Community.PowerToys.Run.Plugin.RustInterop;

namespace Community.PowerToys.Run.Plugin.MyPlugin
{
    internal unsafe class CallbackHolder : IDisposable
    {

        readonly void* cb;
        private bool disposedValue;

        public CallbackHolder(void* cb) { this.cb = cb; }

        public bool run()
        {
            if (!disposedValue)
            {
                return RustMethods.callback_trampoline(this.cb);
            }
            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                RustMethods.callback_dealloc(this.cb);
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~CallbackHolder()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
