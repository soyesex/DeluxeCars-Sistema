using DeluxeCarsDesktop.Interfaces;
using DeluxeCarsDesktop.Utils;
using DeluxeCarsDesktop.View;
using DeluxeCarsDesktop.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace DeluxeCarsDesktop.Services
{
    public class NavigationService : ViewModelBase, INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Stack<ViewModelBase> _history = new Stack<ViewModelBase>();
        private ViewModelBase _currentMainView;

        public ViewModelBase CurrentMainView
        {
            get => _currentMainView;
            private set
            {
                SetProperty(ref _currentMainView, value);
                OnPropertyChanged(nameof(CanGoBack));
            }
        }

        public bool CanGoBack => _history.Count > 0;
        public event Action CurrentMainViewChanged;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task OpenSugerenciasDialogAsync()
        {
            // Usamos el DI container para crear una instancia del ViewModel con todas sus dependencias.
            var vm = _serviceProvider.GetRequiredService<SugerenciasCompraViewModel>();

            // Creamos la vista del diálogo.
            var view = new SugerenciasCompraView { DataContext = vm };

            // Le pasamos la acción de cierre al ViewModel.
            // vm.CloseAction = view.Close; // Si tuvieras un CloseAction en SugerenciasCompraViewModel

            // Cargamos sus datos antes de mostrarla.
            await vm.LoadAsync();

            // Mostramos la ventana como un diálogo modal (bloquea la ventana principal).
            view.ShowDialog();
        }
        public async Task NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            if (CurrentMainView != null)
            {
                _history.Push(CurrentMainView);
            }

            var newViewModel = _serviceProvider.GetRequiredService<TViewModel>();

            if (newViewModel is IAsyncLoadable vmWithLoad)
            {
                await vmWithLoad.LoadAsync();
            }

            CurrentMainView = newViewModel;
            CurrentMainViewChanged?.Invoke();
        }
        public void GoBack()
        {
            if (_history.Count > 0)
            {
                CurrentMainView = _history.Pop();
                CurrentMainViewChanged?.Invoke();
            }
        }

        public async Task OpenFormWindow(FormType formType, int entityId = 0)
        {
            // 1. Crea la ventana anfitriona.
            var formWindow = new FormularioView();

            // 2. Crea el ViewModel anfitrión con 'new', porque necesita parámetros.
            // Esta es la corrección clave para el error CS1501.
            var formViewModel = new FormularioViewModel(formType, _serviceProvider);

            // 3. Inicializamos el ViewModel "hijo" que se creó dentro.
            if (formViewModel.CurrentChildView is IFormViewModel childWithLoad)
            {
                await childWithLoad.LoadAsync(entityId);
            }

            // 4. Conectamos la acción de cierre.
            if (formViewModel.CurrentChildView is ICloseable childWithClose)
            {
                childWithClose.CloseAction = formWindow.Close;
            }

            // 5. Asignamos el DataContext y mostramos la ventana.
            formWindow.DataContext = formViewModel;
            formWindow.Show();
        }
    }
}
