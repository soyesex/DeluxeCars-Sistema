using DeluxeCarsDesktop.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DeluxeCarsDesktop.Services
{
    public class PinLockoutService : IPinLockoutService
    {
        private const int MAX_ATTEMPTS = 3;
        private const int LOCKOUT_SECONDS = 180;

        private readonly IServiceProvider _serviceProvider;
        private readonly DispatcherTimer _lockoutTimer;
        private bool _isInitialized = false;

        private int _failedAttempts;
        private DateTime _lockoutEndTime;

        public bool IsLockedOut => DateTime.UtcNow < _lockoutEndTime;
        public int RemainingAttempts => IsLockedOut ? 0 : Math.Max(0, MAX_ATTEMPTS - _failedAttempts);
        public TimeSpan RemainingLockoutTime => IsLockedOut ? _lockoutEndTime - DateTime.UtcNow : TimeSpan.Zero;
        public event Action StateChanged;

        public PinLockoutService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _lockoutTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _lockoutTimer.Tick += LockoutTimer_Tick;
        }

        private async void LockoutTimer_Tick(object sender, EventArgs e)
        {
            if (IsLockedOut)
            {
                Debug.WriteLine($"[PinLockoutService] Tick: Aún bloqueado. Tiempo restante: {RemainingLockoutTime.TotalSeconds}s");

                // Si todavía estamos bloqueados, solo notificamos para que la UI actualice el contador de tiempo.
                StateChanged?.Invoke();
            }
            else
            {
                // Si el tiempo de bloqueo acaba de terminar, detenemos el timer y reseteamos el estado.
                _lockoutTimer.Stop();
                Debug.WriteLine("[PinLockoutService] Tick: Tiempo de bloqueo terminado. Reseteando...");

                await Reset(); // Llama al método que pone los intentos de vuelta a 3.
            }
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;
            Debug.WriteLine("[PinLockoutService] Inicializando desde la base de datos...");

            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var config = await unitOfWork.Configuraciones.GetByIdAsync(1);
                if (config != null)
                {
                    _failedAttempts = config.PinFailedAttempts;
                    _lockoutEndTime = config.LockoutEndTimeUtc ?? DateTime.MinValue;
                }
            }
            if (IsLockedOut) _lockoutTimer.Start();
            _isInitialized = true;
        }

        private async Task SaveStateToDatabase()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var config = await unitOfWork.Configuraciones.GetByIdAsync(1);
                if (config != null)
                {
                    config.PinFailedAttempts = _failedAttempts;
                    config.LockoutEndTimeUtc = IsLockedOut ? (DateTime?)_lockoutEndTime : null;
                    await unitOfWork.CompleteAsync();
                }
            }
        }

        public async Task RecordFailedAttempt()
        {
            if (IsLockedOut) return;
            _failedAttempts++;
            Debug.WriteLine($"[PinLockoutService] Intento fallido. Fallos: {_failedAttempts}. Restantes: {RemainingAttempts}");

            if (_failedAttempts >= MAX_ATTEMPTS)
            {
                _lockoutEndTime = DateTime.UtcNow.AddSeconds(LOCKOUT_SECONDS);
                _lockoutTimer.Start();
                Debug.WriteLine($"[PinLockoutService] Límite de intentos alcanzado. Bloqueado hasta: {_lockoutEndTime}");

            }
            await SaveStateToDatabase();
            StateChanged?.Invoke();
        }

        public async Task Reset()
        {
            Debug.WriteLine("[PinLockoutService] Reseteando estado de intentos a 0.");

            _failedAttempts = 0;
            _lockoutEndTime = DateTime.UtcNow; // Asegura que ya no esté bloqueado
            _lockoutTimer.Stop();
            await SaveStateToDatabase();
            StateChanged?.Invoke();
        }

        public void Dispose()
        {
            if (_lockoutTimer != null)
            {
                _lockoutTimer.Tick -= LockoutTimer_Tick; // Limpiamos la suscripción
                _lockoutTimer.Stop();
            }
        }
    }
}
