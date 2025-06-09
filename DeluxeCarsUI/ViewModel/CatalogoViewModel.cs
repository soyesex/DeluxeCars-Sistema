using DeluxeCarsUI.Services;
using DeluxeCarsUI.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeluxeCarsUI.ViewModel
{
    public class CatalogoViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        private bool _isViewVisible = true;
        public bool IsViewVisible
        {
            get
            {
                return _isViewVisible;
            }
            set
            {
                _isViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }

        // commands
        public ICommand ShowCategoriaViewCommand { get; }
        public ICommand OpenNewProductoCommand { get; }
        public CatalogoViewModel() : this(new NavigationService())
        { }

        public CatalogoViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            ShowCategoriaViewCommand = new ViewModelCommand(ExecuteShowCategoriaView);
            OpenNewProductoCommand = new ViewModelCommand(ExecuteNuevoProducto);
        }


        private void ExecuteNuevoProducto(object obj)
        {
            _navigationService.OpenFormWindow(Utils.FormType.Producto);
        }

        private void ExecuteShowCategoriaView(object obj)
        {
            _navigationService.OpenFormWindow(Utils.FormType.Categoria);

            //var formularioVM = new FormularioViewModel();
            //formularioVM.CurrentChildView = new CategoriaViewModel();
            //formularioVM.Caption = "Gestión de Categorías";
            //formularioVM.Icon = FontAwesome.Sharp.IconChar.List;

            //var crudGenericView = new FormularioView
            //{
            //    DataContext = formularioVM
            //};

            //crudGenericView.Show();
        }
    }
}
