using System;
using System.Collections.Generic;
using System.Text;

namespace Todobrayan.Test.Helpers
{
    public class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();

        public void Dispose(){ }

        private  NullScope() { }
    }
}
