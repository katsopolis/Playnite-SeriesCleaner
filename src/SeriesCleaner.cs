using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeriesCleaner
{
    public class SeriesCleaner : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public SeriesCleaner(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public override Guid Id { get; } = Guid.Parse("12345678-1234-1234-1234-123456789012");

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = ResourceProvider.GetString("LOCCleanSingleGameSeries"),
                    MenuSection = "@Series Cleaner",
                    Action = _ => CleanSingleGameSeries()
                }
            };
        }

        private sealed class SeriesRemovalCandidate
        {
            public Series Series { get; }
            public Game Game { get; }

            public SeriesRemovalCandidate(Series series, Game game)
            {
                Series = series;
                Game = game;
            }
        }

        private void CleanSingleGameSeries()
        {
            try
            {
                var removalCandidates = new List<SeriesRemovalCandidate>();

                foreach (var series in PlayniteApi.Database.Series)
                {
                    var gamesInSeries = PlayniteApi.Database.Games
                        .Where(g => g.SeriesIds != null && g.SeriesIds.Contains(series.Id))
                        .ToList();

                    if (gamesInSeries.Count == 1)
                    {
                        removalCandidates.Add(new SeriesRemovalCandidate(series, gamesInSeries[0]));
                    }
                }

                if (!removalCandidates.Any())
                {
                    PlayniteApi.Dialogs.ShowMessage(
                        "No single-game series found.",
                        "Series Cleaner"
                    );
                    return;
                }

                ShowPreviewDialog(removalCandidates);

                // Ask for confirmation
                var result = PlayniteApi.Dialogs.ShowMessage(
                    $"Found {removalCandidates.Count} series with only one game.\n\n" +
                    "Do you want to remove these series from the database?\n\n" +
                    "This will:\n" +
                    "- Remove the series metadata\n" +
                    "- Remove the series association from affected games\n\n" +
                    "This action cannot be undone without a database backup.",
                    "Series Cleaner",
                    System.Windows.MessageBoxButton.YesNo
                );

                if (result != System.Windows.MessageBoxResult.Yes)
                {
                    return;
                }

                // Remove series from games
                using (PlayniteApi.Database.BufferedUpdate())
                {
                    foreach (var candidate in removalCandidates)
                    {
                        var game = PlayniteApi.Database.Games.Get(candidate.Game.Id);
                        if (game?.SeriesIds != null && game.SeriesIds.Contains(candidate.Series.Id))
                        {
                            game.SeriesIds.Remove(candidate.Series.Id);
                            PlayniteApi.Database.Games.Update(game);
                        }

                        PlayniteApi.Database.Series.Remove(candidate.Series.Id);
                    }
                }

                PlayniteApi.Dialogs.ShowMessage(
                    $"Successfully removed {removalCandidates.Count} single-game series.",
                    "Series Cleaner"
                );

                logger.Info($"Removed {removalCandidates.Count} single-game series from the database.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while cleaning single-game series");
                PlayniteApi.Dialogs.ShowErrorMessage(
                    $"An error occurred while cleaning series:\n{ex.Message}",
                    "Series Cleaner"
                );
            }
        }

        private void ShowPreviewDialog(IReadOnlyList<SeriesRemovalCandidate> candidates)
        {
            var previewCount = Math.Min(20, candidates.Count);
            var previewLines = candidates
                .Take(previewCount)
                .Select(candidate =>
                {
                    var seriesName = string.IsNullOrWhiteSpace(candidate.Series?.Name)
                        ? "<Unnamed Series>"
                        : candidate.Series.Name;

                    var gameName = string.IsNullOrWhiteSpace(candidate.Game?.Name)
                        ? "Unknown Game"
                        : candidate.Game.Name;

                    return $"• {seriesName} (Game: {gameName})";
                });

            var message = string.Join("\n", previewLines);

            if (candidates.Count > previewCount)
            {
                message += $"\n…and {candidates.Count - previewCount} more series";
            }

            PlayniteApi.Dialogs.ShowMessage(
                "Series scheduled for removal:\n\n" + message,
                "Series Cleaner"
            );
        }
    }
}
