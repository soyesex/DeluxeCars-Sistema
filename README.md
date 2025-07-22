# 🚗 Deluxe Cars - Sistema de Gestión Empresarial (WPF)

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)

Este repositorio contiene la aplicación de escritorio del sistema de gestión para "Deluxe Cars", desarrollada como parte de nuestro proyecto final para el programa Tecnólogo en Análisis y Desarrollo de Software del SENA.

**(Aquí te recomiendo poner una captura de pantalla principal y atractiva de tu programa, como el Dashboard o la ventana de inventario)**
![Dashboard Deluxe Cars](URL_DE_LA_IMAGEN_PRINCIPAL)

---

## 📖 Sobre el Proyecto

### El Problema
La empresa "Deluxe Cars", dedicada a la venta de autopartes, enfrentaba serios desafíos operativos debido a su dependencia de procesos manuales. Su gestión de inventario se basaba en conteos físicos diarios, propensos a errores, y la facturación se realizaba en papel, causando inconsistencias y pérdidas de documentos. Esta situación limitaba su eficiencia y potencial de crecimiento.

### La Solución
Para resolver esta problemática, se diseñó y desarrolló un sistema de información híbrido. La solución se compone de dos aplicativos:

1.  **Una Aplicación de Escritorio (WPF):** El núcleo administrativo para la gestión interna de inventario, compras, proveedores, ventas y facturación. **Este es el software que se encuentra en este repositorio.**
2.  **Una Aplicación Web (.NET MVC):** Una vitrina digital para que los clientes puedan ver el catálogo de productos y realizar pedidos vía WhatsApp.

### Mi Rol en el Proyecto
Como parte del equipo de desarrollo, mi rol principal fue el de **Desarrollador de la Aplicación de Escritorio y Analista**. Mis responsabilidades incluyeron:
- Diseño de la arquitectura de la aplicación de escritorio y la base de datos.
- Desarrollo completo de todos los módulos de la aplicación en **WPF y C#**.
- Colaboración en el levantamiento de requerimientos y la creación de los casos de uso del sistema.
- Realización de las pruebas funcionales y de integración del componente de escritorio.

---

## ✨ Características Principales

Este software de escritorio ofrece una solución completa para la administración del negocio, incluyendo los siguientes módulos:

* **🔐 Gestión de Usuarios y Seguridad:**
    * Autenticación por correo y contraseña.
    * Sistema basado en roles (Administrador y Empleado) con diferentes niveles de permisos.
    * El administrador puede crear, editar y eliminar cuentas de usuario.

* **📦 Módulo de Inventario:**
    * Registro y edición de productos con detalles completos (código, categoría, precios, etc.).
    * Control de stock en tiempo real, actualizado automáticamente con cada venta o compra.
    * Alertas de stock mínimo para prevenir la escasez de productos.
    * Ajuste manual de stock para corregir discrepancias.

* **🚚 Gestión de Compras y Proveedores:**
    * Directorio centralizado para registrar y gestionar proveedores.
    * Creación y seguimiento de órdenes de compra por estado (Borrador, Aprobado, Recibido).
    * Recepción de mercancía que actualiza el inventario automáticamente.
    * Gestión de pagos a proveedores para llevar un control de las cuentas por pagar.

* **💰 Módulo de Facturación y Ventas:**
    * Generación ágil de facturas de venta, asociadas a un cliente.
    * Cálculo automático de totales, validando la disponibilidad de stock.
    * Historial de ventas detallado, con filtros por cliente, fecha y estado de pago.
    * Gestión de cuentas por cobrar, registrando los abonos de los clientes.

* **📊 Reportes y Exportación:**
    * Generación de reportes de ventas, productos más vendidos y estado del inventario.
    * Funcionalidad para exportar el inventario completo y otros reportes a **Excel** y **PDF**.

---

## 📸 Capturas de Pantalla

¡Aquí es donde tu proyecto cobra vida! Agrega las mejores capturas de pantalla que tienes en tu documento de testing para mostrar cómo se ve el software.

**(Ejemplo de cómo añadir imágenes)**

| Inicio de Sesión | Módulo de Inventario |
| :---: | :---: |
| ![Login](URL_DE_LA_IMAGEN_DEL_LOGIN) | ![Inventario](URL_DE_LA_IMAGEN_DEL_INVENTARIO) |

| Ventana de Venta | Reporte en PDF |
| :---: | :---: |
| ![Facturación](URL_DE_LA_IMAGEN_DE_FACTURACION) | ![Reporte](URL_DE_LA_IMAGEN_DEL_REPORTE) |


---

## 🛠️ Stack Tecnológico

* **Lenguaje de Programación:** C# 
* **Framework de Escritorio:** WPF (Windows Presentation Foundation) sobre .NET
* **Base de Datos:** Microsoft SQL Server
* **Arquitectura:** Modelo-Vista-VistaModelo (MVVM)
* **Herramientas de Desarrollo:** Visual Studio
