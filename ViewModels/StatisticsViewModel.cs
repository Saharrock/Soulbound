using System.Collections.ObjectModel;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    internal class StatisticsViewModel : ViewModelBase
    {
        private readonly AppService appService;
        private const double MaxChartBarWidth = 220;

        public ObservableCollection<HistoryRecord> HistoryRecords { get; } = new();
        public ObservableCollection<CategorySummaryItem> CategorySummaries { get; } = new();

        public StatisticsViewModel()
        {
            appService = AppService.GetInstance();
            _ = RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            await appService.EnsureGameDataLoadedAsync();
            HistoryRecords.Clear();
            CategorySummaries.Clear();

            List<HistoryRecord> records = appService.GetHistoryRecords();
            foreach (HistoryRecord item in records)
            {
                HistoryRecords.Add(item);
            }

            List<CategorySummaryItem> summaries = BuildCategorySummaries(records);
            foreach (CategorySummaryItem summary in summaries)
            {
                CategorySummaries.Add(summary);
            }
        }

        private static List<CategorySummaryItem> BuildCategorySummaries(List<HistoryRecord> records)
        {
            Dictionary<string, int> categoryCounts = new()
            {
                { "Physical", 0 },
                { "Intellectual", 0 },
                { "Mental", 0 }
            };

            foreach (HistoryRecord record in records)
            {
                if (categoryCounts.ContainsKey(record.Category))
                {
                    categoryCounts[record.Category]++;
                }
            }

            int maxValue = categoryCounts.Values.DefaultIfEmpty(0).Max();
            List<CategorySummaryItem> result = new();
            foreach (KeyValuePair<string, int> entry in categoryCounts)
            {
                double width = 0;
                if (maxValue > 0 && entry.Value > 0)
                {
                    width = Math.Max(10, (entry.Value / (double)maxValue) * MaxChartBarWidth);
                }

                result.Add(new CategorySummaryItem
                {
                    CategoryName = entry.Key,
                    Count = entry.Value,
                    BarColorHex = GetCategoryColor(entry.Key),
                    BarWidth = width
                });
            }

            return result;
        }

        private static string GetCategoryColor(string category)
        {
            return category switch
            {
                "Physical" => "#e03c31",
                "Intellectual" => "#3c6fe0",
                "Mental" => "#9b4de0",
                _ => "#888888"
            };
        }
    }

    internal class CategorySummaryItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Count { get; set; }
        public string BarColorHex { get; set; } = "#888888";
        public double BarWidth { get; set; }
    }
}
