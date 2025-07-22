using DeluxeCarsDesktop.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.Services
{
    public abstract class PaginatedViewModel<T> : ViewModelBase where T : class
    {
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            protected set => SetProperty(ref _isLoading, value);
        }

        // --- PROPIEDADES DE PAGINACIÓN ---
        private int _numeroDePagina = 1;
        public int NumeroDePagina
        {
            get => _numeroDePagina;
            set
            {
                // Usamos tu método para ejecutar la lógica solo si el valor cambia.
                if (SetPropertyAndCheck(ref _numeroDePagina, value))
                {
                    LoadItemsWrapper();
                }
            }
        }

        private int _tamañoDePagina = 15;
        public int TamañoDePagina
        {
            get => _tamañoDePagina;
            set
            {
                if (SetPropertyAndCheck(ref _tamañoDePagina, value))
                {
                    ApplyFilterAndResetPage();
                }
            }
        }

        private int _totalItems;
        public int TotalItems
        {
            get => _totalItems;
            protected set
            {
                if (SetPropertyAndCheck(ref _totalItems, value))
                {
                    OnPropertyChanged(nameof(TotalPaginas));
                }
            }
        }

        public int TotalPaginas => TamañoDePagina > 0 ? (int)Math.Ceiling((double)TotalItems / TamañoDePagina) : 0;

        // --- COLECCIÓN Y COMANDOS ---
        public ObservableCollection<T> Items { get; }
        public ICommand IrAPaginaSiguienteCommand { get; }
        public ICommand IrAPaginaAnteriorCommand { get; }

        protected PaginatedViewModel()
        {
            Items = new ObservableCollection<T>();
            IrAPaginaSiguienteCommand = new ViewModelCommand(p => NumeroDePagina++, p => !IsLoading && NumeroDePagina < TotalPaginas);
            IrAPaginaAnteriorCommand = new ViewModelCommand(p => NumeroDePagina--, p => !IsLoading && NumeroDePagina > 1);
        }

        // --- LÓGICA CENTRAL ---

        /// <summary>
        /// Las clases hijas llamarán a este método en los setters de sus filtros.
        /// </summary>
        protected void ApplyFilterAndResetPage()
        {
            if (NumeroDePagina == 1)
            {
                LoadItemsWrapper();
            }
            else
            {
                NumeroDePagina = 1;
            }
        }

        /// <summary>
        /// Envuelve la llamada a LoadItemsAsync con control de estado de carga.
        /// </summary>
        private async void LoadItemsWrapper()
        {
            if (IsLoading) return;

            IsLoading = true;
            (IrAPaginaSiguienteCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            (IrAPaginaAnteriorCommand as ViewModelCommand)?.RaiseCanExecuteChanged();

            try
            {
                Items.Clear();
                await LoadItemsAsync();
            }
            catch (Exception ex)
            {
                // Usamos el manejador de errores de tu ViewModelBase
                await ShowTemporaryErrorMessage($"Error al cargar datos: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error al cargar items: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                (IrAPaginaSiguienteCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
                (IrAPaginaAnteriorCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// MÉTODO ABSTRACTO: La clase hija DEBE implementar esta lógica.
        /// </summary>
        protected abstract Task LoadItemsAsync();
    }
}
