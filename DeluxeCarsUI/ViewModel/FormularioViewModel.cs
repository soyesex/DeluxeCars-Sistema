using DeluxeCarsUI.Model;
using DeluxeCarsUI.Properties;
using DeluxeCarsUI.Repositories;
using DeluxeCarsUI.Utils;
using DeluxeCarsUI.View;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeluxeCarsUI.ViewModel
{
    public class FormularioViewModel : ViewModelBase
    {
        private UserAccountModel _currentUserAccount;
        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _icon;

        private IUserRepository userRepository;
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

        // Commands
        public ICommand ShowProductoViewCommand { get; }
        public ICommand ShowCategoriaViewCommand { get; }
        public ICommand ShowNuevoClienteViewCommand { get; }

        public FormularioViewModel()
        {
            //// Initialize commands
            //ShowProductoViewCommand = new ViewModelCommand(ExecuteShowProductoViewCommand);
            //ShowCategoriaViewCommand = new ViewModelCommand(ExecuteShowCategoriaViewCommand);

            //ShowNuevoClienteViewCommand = new ViewModelCommand(ExecuteShowNuevoClienteViewCommand);


        }

        public FormularioViewModel(FormType formType)
        {
            Debug.WriteLine("→ FormularioViewModel creado con FormType: " + formType);

            switch (formType)
            {
                case FormType.Producto:
                    CurrentChildView = new ProductoFormViewModel();
                    Caption = "Gestión de Productos";
                    Icon = IconChar.Box;
                    break;
                case FormType.Categoria:
                    CurrentChildView = new CategoriaFormViewModel();
                    Caption = "Gestión de Categorías";
                    Icon = IconChar.Tags;
                    break;
                case FormType.Cliente:
                    CurrentChildView = new ClienteFormViewModel();
                    Caption = "Gestión de Clientes";
                    Icon = IconChar.UserFriends;
                    break;
                case FormType.Proveedor:
                    CurrentChildView = new ProveedorFormViewModel();
                    Caption = "Gestión de Proveedores";
                    Icon = IconChar.TruckLoading;
                    break;
                case FormType.Departamentos:
                    CurrentChildView = new DepartamentoFormViewModel();
                    Caption = "Gestión de Departamentos";
                    Icon = IconChar.MapMarkedAlt;
                    break;
                case FormType.Municipio:
                    CurrentChildView = new MunicipioFormViewModel();
                    Caption = "Gestión de Municipios";
                    Icon = IconChar.MapMarkerAlt;
                    break;
                case FormType.Pedido:
                    CurrentChildView = new PedidoFormViewModel();
                    Caption = "Gestión de Pedidos";
                    Icon = IconChar.ClipboardList;
                    break;
                case FormType.Factura:
                    CurrentChildView = new FacturaFormViewModel();
                    Caption = "Gestión de Facturas";
                    Icon = IconChar.FileInvoiceDollar;
                    break;
                case FormType.Servicio:
                    CurrentChildView = new ServicioFormViewModel();
                    Caption = "Gestión de Servicios";
                    Icon = IconChar.Cogs;
                    break;
                case FormType.MetodoPago:
                    CurrentChildView = new MetodoPagoFormViewModel();
                    Caption = "Gestión de Métodos de Pago";
                    Icon = IconChar.CreditCard;
                    break;
                case FormType.DetallesFactura:
                    CurrentChildView = new DetalleFacturaFormViewModel();
                    Caption = "Detalles de Factura";
                    Icon = IconChar.Receipt;
                    break;
                case FormType.Rol:
                    CurrentChildView = new RolFormViewModel();
                    Caption = "Gestión de Roles";
                    Icon = IconChar.UserTag;
                    break;
                case FormType.Usuario:
                    CurrentChildView = new UsuarioFormViewModel();
                    Caption = "Gestión de Usuarios";
                    Icon = IconChar.UserShield;
                    break;
                case FormType.CambiarPassword:
                    CurrentChildView = new CambiarPasswordViewModel();
                    Caption = "Cambio de Contraseña";
                    Icon = IconChar.Key;
                    break;
                default:
                    Caption = "Formulario";
                    Icon = IconChar.QuestionCircle;
                    break;
            }
        }
        //private void ExecuteShowNuevoClienteViewCommand(object obj)
        //{
        //    CurrentChildView = new NuevoClienteViewModel();
        //    Caption = "Nuevo Cliente";
        //    Icon = IconChar.Cake;
        //}

        //private void ExecuteShowProductoViewCommand(object obj)
        //{
        //    CurrentChildView = new DashboardViewModel();
        //    Caption = "Productos";
        //    Icon = IconChar.Home;
        //}

        //private void ExecuteShowCategoriaViewCommand(object obj)
        //{
        //    CurrentChildView = new CategoriaViewModel();
        //    Caption = "Categoria";
        //    Icon = IconChar.List;
        //}

    }
}
