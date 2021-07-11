using System;
using System.Threading.Tasks;
using DiegoG.WebWatcher;

namespace JalMamameElWebo
{
    [Watcher]
    public class JalMamameElWebo : IWebWatcher
    {
        public string Name => "JalMamameElWebo";

        public TimeSpan Interval => TimeSpan.FromSeconds(5);

        public Task Check()
        {
            OutputBot.SendTextMessage(-554172376, "Jal Mamame el Webo");
            return Task.CompletedTask;
        }
        public Task FirstCheck() => Task.CompletedTask;
    }
}
