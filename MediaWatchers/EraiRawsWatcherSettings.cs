using DiegoG.Utilities.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiegoG.WebWatcher
{
    public class EraiRawsWatcherSettings : ISettings
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string SettingsType => "Erai-raws Watcher Settings";
        public ulong Version => 0;

        public long ChatId { get; set; }
        public List<string> MatchPatterns { get; set; } = new();
    }
}