using System.Collections.ObjectModel;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    internal class StatisticsViewModel : ViewModelBase
    {
        private readonly TaskService taskService;

        public ObservableCollection<HistoryRecord> HistoryRecords { get; } = new();

        public StatisticsViewModel()
        {
            taskService = TaskService.GetInstance();
            Refresh();
        }

        /// <summary>
        /// Reloads history from persisted storage (newest entries first).
        /// </summary>
        public void Refresh()
        {
            HistoryRecords.Clear();

            foreach (HistoryRecord item in taskService.GetHistoryRecords())
            {
                HistoryRecords.Add(item);
            }
        }
    }
}
