using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace GPC.Model.Core
{
    /// <summary>Independent work on the elements of a model (each under its own lock, on data that no other step changes meanwhile), in
    /// parallel. A failure raises the exception of the first element that failed, as the loop one element after the other would.</summary>
    internal static class ParallelElements
    {
        private const int Sequential = 64;

        internal static void For(int count, Action<int> body)
        {
            if (count < Sequential) { for (int i = 0; i < count; i++) body(i); return; }
            var failures = new Exception[count]; int failed = 0;
            Parallel.For(0, count, i =>
            {
                try { body(i); }
                catch (Exception error) { failures[i] = error; System.Threading.Interlocked.Exchange(ref failed, 1); }
            });
            if (failed == 0) return;
            foreach (var failure in failures) if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }
}
