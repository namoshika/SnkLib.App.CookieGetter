using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    static class SemaphoreSlimEx
    {
        public static async Task WaitAsync(this SemaphoreSlim target)
        { await Task.Factory.StartNew(target.Wait); }
    }
}
