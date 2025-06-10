using DeluxeCarsDesktop.Models;
using DeluxeCarsDesktop.Utils;
using FontAwesome.Sharp;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsDesktop.ViewModel
{
    public class FormularioViewModel : ViewModelBase
    {
        private UserAccountModel _currentUserAccount;
        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _icon;

        public UserAccountModel CurrentUserAccount
        {
            get
            {
                return _currentUserAccount;
            }
            set
            {
                _currentUserAccount = value;
                OnPropertyChanged(nameof(CurrentUserAccount));
            }
        }

        public ViewModelBase CurrentChildView
        {
            get
            {
                return _currentChildView;
            }
            set
            {
                _currentChildView = value;
                OnPropertyChanged(nameof(CurrentChildView));
            }
        }
        public string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                _caption = value;
                OnPropertyChanged(nameof(Caption));
            }
        }
        public IconChar Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }

        public FormularioViewModel(FormType formType, IServiceProvider serviceProvider)
        {
            Debug.WriteLine("→ FormularioViewModel creado con FormType: " + formType);

            // --- CAMBIO #2: El switch ahora usa GetService para crear los ViewModels ---
            switch (formType)
            {
                case FormType.Producto:
                    CurrentChildView = serviceProvider.GetService<ProductoFormViewModel>();
                    Caption = "Gestión de Productos";
                    Icon = IconChar.Box;
                    break;
                case FormType.Categoria:
                    CurrentChildView = serviceProvider.GetService<CategoriaFormViewModel>();
                    Caption = "Gestión de Categorías";
                    Icon = IconChar.Tags;
                    break;
                case FormType.Cliente:
                    CurrentChildView = serviceProvider.GetService<ClienteFormViewModel>();
                    Caption = "Gestión de Clientes";
                    Icon = IconChar.UserFriends;
                    break;
                case FormType.Proveedor:
                    CurrentChildView = serviceProvider.GetService<ProveedorFormViewModel>();
                    Caption = "Gestión de Proveedores";
                    Icon = IconChar.TruckLoading;
                    break;
                // ... y así sucesivamente para TODOS los demás casos ...
                // Ejemplo:
                case FormType.Usuario:
                    CurrentChildView = serviceProvider.GetService<UsuarioFormViewModel>();
                    Caption = "Gestión de Usuarios";
                    Icon = IconChar.UserShield;
                    break;
                default:
                    Caption = "Formulario";
                    Icon = IconChar.QuestionCircle;
                    break;
            }
        }
    }
}
