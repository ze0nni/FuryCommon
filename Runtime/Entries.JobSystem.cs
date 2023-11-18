using System.Runtime.CompilerServices;
using Unity.Jobs;
using UnityEngine.Profiling;

namespace Fury
{
    public interface IEntriesJobParallelFor<T>
        where T : class
    {
        void Execute(T entry);
    }

    public sealed partial class Entries<T>
    {
        private struct ForJob<TEntriesJob> : IJobParallelFor
            where TEntriesJob : struct, IEntriesJobParallelFor<T>
        {
            public static string SampleName = $"Entries<{typeof(T)}.Schedule<{typeof(TEntriesJob).Name}>()>";

            public static Entries<T> _entries;
            public static TEntriesJob _job;

            public void Execute(int index)
            {
                var e = _entries._list[index];
                if (e.Entry != null)
                {
                    _job.Execute(e.Entry);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Schedule<TEntriesJob>(int innerloopBatchCount)
            where TEntriesJob : struct, IEntriesJobParallelFor<T>
        {
            Schedule<TEntriesJob>(default(TEntriesJob), innerloopBatchCount);
        }

        public void Schedule<TEntriesJob>(TEntriesJob entriesJob, int innerloopBatchCount)
            where TEntriesJob : struct, IEntriesJobParallelFor<T>
        {
            ForJob<TEntriesJob>._entries = this;
            ForJob<TEntriesJob>._job = entriesJob;
            this._readonly++;
            try
            {
                Profiler.BeginSample(ForJob < TEntriesJob >.SampleName);
                default(ForJob<TEntriesJob>).Schedule(_list.Count, innerloopBatchCount).Complete();
            } finally
            {
                Profiler.EndSample();
                ForJob<TEntriesJob>._entries = null;
                ForJob<TEntriesJob>._job = default;
                this._readonly--;
            }
        }
    }
}
