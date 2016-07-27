using System;
using System.Threading;
using System.Threading.Tasks;
using ImageSorter.Models.ModelInterfaces;

namespace ImageSorter.Models
{
    public class TaskManager
    {
        private ISelectedDirectory SourceDirectory { get; set; }
        private int MaxThreads { get; set; }
        private CancellationToken CancellationToken { get; set; }

        public TaskManager(ISelectedDirectory sourceDirectory)
        {
            SourceDirectory = sourceDirectory;
        }

        public void Start()
        {
            var discoveryTasks = SourceDirectory.CreateDiscoveryTasks();
            Parallel.Invoke(new ParallelOptions() {CancellationToken = CancellationToken,MaxDegreeOfParallelism = MaxThreads}, discoveryTasks.ToArray()); //this should block
            var filterTasks = SourceDirectory.CreateFilterTasks();
            Parallel.Invoke(new ParallelOptions() { CancellationToken = CancellationToken, MaxDegreeOfParallelism = MaxThreads }, filterTasks.ToArray()); //this should block
        }
    }