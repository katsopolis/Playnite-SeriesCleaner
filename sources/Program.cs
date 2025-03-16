using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;

namespace SeriesCleaner
{
    public class Program
    {
        // Required method for Playnite to load the plugin
        public static GenericPlugin GetPluginInstance(IPlayniteAPI api)
        {
            return new SeriesCleanerPlugin(api);
        }
    }
}