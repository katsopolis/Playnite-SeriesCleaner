using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace SeriesCleaner
{
    public class SeriesCleanerPlugin : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        // Plugin details - required properties
        public override Guid Id { get; } = Guid.Parse("10000000-0000-0000-0000-000000000001");

        public SeriesCleanerPlugin(IPlayniteAPI api) : base(api)
        {
            // Different versions of the SDK may have different ways to set properties
            // We'll use the base constructor property initialization instead
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = "Clean Single-Game Series",
                    MenuSection = "@Series Cleaner",
                    Action = (mainContext) => CleanSingleGameSeries()
                }
            };
        }

        private void CleanSingleGameSeries()
        {
            try
            {
                // Get all games with Series data (with exactly 1 series only!)
                var gamesWithSingleSeries = PlayniteApi.Database.Games
                    .Where(g => g.Series != null && g.Series.Count == 1)
                    .ToList();

                // Group these games by their single series ID
                var singleSeriesGroups = gamesWithSingleSeries
                    .GroupBy(g => g.Series.First().Id)
                    .Where(group => group.Count() == 1)
                    .ToList();

                if (!singleSeriesGroups.Any())
                {
                    PlayniteApi.Dialogs.ShowMessage("No single-game series found in your library.", "Series Cleaner");
                    return;
                }

                var gamesToClean = singleSeriesGroups.Select(g => g.First()).ToList();

                var confirmationMessage = $"Found {gamesToClean.Count} games that are the only title in their (single) series:\n\n";

                foreach (var game in gamesToClean.Take(10))
                {
                    var seriesName = game.Series.FirstOrDefault()?.Name ?? "Unknown";
                    confirmationMessage += $"• {game.Name} (Series: {seriesName})\n";
                }

                if (gamesToClean.Count > 10)
                {
                    confirmationMessage += $"• And {gamesToClean.Count - 10} more...\n";
                }

                confirmationMessage += "\nWould you like to remove these single-game series from the database?";

                var result = PlayniteApi.Dialogs.ShowMessage(
                    confirmationMessage,
                    "Series Cleaner",
                    MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    foreach (var game in gamesToClean)
                    {
                        var seriesObj = game.Series.FirstOrDefault();
                        if (seriesObj != null)
                        {
                            PlayniteApi.Database.Series.Remove(seriesObj);

                            PlayniteApi.Notifications.Add(
                                new NotificationMessage(
                                    Guid.NewGuid().ToString(),
                                    $"Removed series '{seriesObj.Name}' from database (Game: {game.Name})",
                                    NotificationType.Info));
                        }
                    }

                    PlayniteApi.Dialogs.ShowMessage($"Successfully cleaned {gamesToClean.Count} single-game series entries.", "Series Cleaner");
                }


            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in Series Cleaner plugin");
                PlayniteApi.Dialogs.ShowErrorMessage($"An error occurred: {ex.Message}", "Series Cleaner Error");
            }
        }
    }
}
