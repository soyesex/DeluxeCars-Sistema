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

                case FormType.Departamento:
                    CurrentChildView = serviceProvider.GetService<DepartamentoFormViewModel>();
                    Caption = "Gestión de Departamentos";
                    Icon = IconChar.Map;
                    break;

                case FormType.Municipio:
                    CurrentChildView = serviceProvider.GetService<MunicipioFormViewModel>();
                    Caption = "Gestión de Municipios";
                    Icon = IconChar.MapMarkedAlt;
                    break;

                case FormType.Pedido:
                    CurrentChildView = serviceProvider.GetService<PedidoFormViewModel>();
                    Caption = "Nuevo Pedido a Proveedor";
                    Icon = IconChar.ClipboardList;
                    break;

                case FormType.Factura:
                    // Asumimos que esto abre el Punto de Venta
                    CurrentChildView = serviceProvider.GetService<FacturacionViewModel>();
                    Caption = "Nueva Factura (Punto de Venta)";
                    Icon = IconChar.FileInvoiceDollar;
                    break;

                case FormType.Servicio:
                    CurrentChildView = serviceProvider.GetService<ServicioFormViewModel>();
                    Caption = "Gestión de Servicios";
                    Icon = IconChar.Wrench;
                    break;

                case FormType.TipoServicio:
                    // Asumiendo que tienes un TipoServicioFormViewModel
                    //CurrentChildView = serviceProvider.GetService<TipoServicioFormViewModel>();
                    Caption = "Gestión de Tipos de Servicio";
                    Icon = IconChar.Book;
                    break;

                case FormType.MetodoPago:
                    CurrentChildView = serviceProvider.GetService<MetodoPagoFormViewModel>();
                    Caption = "Gestión de Métodos de Pago";
                    Icon = IconChar.CreditCard;
                    break;

                case FormType.DetallesFactura:
                    // Este es poco común, pero se añade por si lo necesitas
                    CurrentChildView = serviceProvider.GetService<DetalleFacturaFormViewModel>();
                    Caption = "Detalles de Factura";
                    Icon = IconChar.ListOl;
                    break;

                case FormType.Rol:
                    // Asumiendo que tienes un RolFormViewModel
                    CurrentChildView = serviceProvider.GetService<RolFormViewModel>();
                    Caption = "Gestión de Roles";
                    Icon = IconChar.UserShield;
                    break;

                case FormType.Usuario:
                    CurrentChildView = serviceProvider.GetService<UsuarioFormViewModel>();
                    Caption = "Gestión de Usuarios";
                    Icon = IconChar.UserEdit;
                    break;

                case FormType.CambiarPassword:
                    // Asumiendo que tienes un CambiarPasswordViewModel
                    CurrentChildView = serviceProvider.GetService<CambiarPasswordViewModel>();
                    Caption = "Cambiar Contraseña";
                    Icon = IconChar.Key;
                    break;

                case FormType.GestionarProductosProveedor:
                    CurrentChildView = serviceProvider.GetService<GestionarProductosProveedorViewModel>();
                    Caption = "Gestionar Catálogo de Proveedor";
                    Icon = IconChar.Tasks; // Un ícono de ejemplo
                    break;

                default:
                    Caption = "Formulario Desconocido";
                    Icon = IconChar.QuestionCircle;
                    break;
            }
        }
    }
}
