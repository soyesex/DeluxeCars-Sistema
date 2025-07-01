using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Services
{
    public interface IPinLockoutService
    {
        Task InitializeAsync();
        bool IsLockedOut { get; }
        int RemainingAttempts { get; }
        TimeSpan RemainingLockoutTime { get; }

        event Action StateChanged;

        Task RecordFailedAttempt();
        Task Reset();
    }
}
