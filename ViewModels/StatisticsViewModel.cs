using System.Collections.ObjectModel;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    internal class StatisticsViewModel : ViewModelBase
    {
        private readonly AppService appService;

        public ObservableCollection<HistoryRecord> HistoryRecords { get; } = new();

        public StatisticsViewModel()
        {
            appService = AppService.GetInstance();
            _ = RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            await appService.EnsureGameDataLoadedAsync();
            HistoryRecords.Clear();

            foreach (HistoryRecord item in appService.GetHistoryRecords())
            {
                HistoryRecords.Add(item);
            }
        }
    }
}
