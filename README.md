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
[cite_start]La empresa "Deluxe Cars", dedicada a la venta de autopartes, enfrentaba serios desafíos operativos debido a su dependencia de procesos manuales[cite: 564, 576]. [cite_start]Su gestión de inventario se basaba en conteos físicos diarios, propensos a errores [cite: 566, 567][cite_start], y la facturación se realizaba en papel, causando inconsistencias y pérdidas de documentos[cite: 570]. [cite_start]Esta situación limitaba su eficiencia y potencial de crecimiento[cite: 564].

### La Solución
[cite_start]Para resolver esta problemática, se diseñó y desarrolló un sistema de información híbrido[cite: 848]. [cite_start]La solución se compone de dos aplicativos[cite: 573, 686, 848]:

1.  [cite_start]**Una Aplicación de Escritorio (WPF):** El núcleo administrativo para la gestión interna de inventario, compras, proveedores, ventas y facturación[cite: 587, 688, 690]. **Este es el software que se encuentra en este repositorio.**
2.  [cite_start]**Una Aplicación Web (.NET MVC):** Una vitrina digital para que los clientes puedan ver el catálogo de productos y realizar pedidos vía WhatsApp[cite: 587, 691, 718].

### Mi Rol en el Proyecto
[cite_start]Como parte del equipo de desarrollo, mi rol principal fue el de **Desarrollador de la Aplicación de Escritorio y Analista**[cite: 844]. Mis responsabilidades incluyeron:
- [cite_start]Diseño de la arquitectura de la aplicación de escritorio y la base de datos[cite: 844].
- [cite_start]Desarrollo completo de todos los módulos de la aplicación en **WPF y C#**[cite: 844].
- [cite_start]Colaboración en el levantamiento de requerimientos y la creación de los casos de uso del sistema[cite: 844].
- [cite_start]Realización de las pruebas funcionales y de integración del componente de escritorio[cite: 844].

---

## ✨ Características Principales

Este software de escritorio ofrece una solución completa para la administración del negocio, incluyendo los siguientes módulos:

* **🔐 Gestión de Usuarios y Seguridad:**
    * [cite_start]Autenticación por correo y contraseña[cite: 698].
    * [cite_start]Sistema basado en roles (Administrador y Empleado) con diferentes niveles de permisos[cite: 593, 699].
    * [cite_start]El administrador puede crear[cite: 700], editar y eliminar cuentas de usuario.

* **📦 Módulo de Inventario:**
    * [cite_start]Registro y edición de productos con detalles completos (código, categoría, precios, etc.)[cite: 599, 702, 81].
    * [cite_start]Control de stock en tiempo real, actualizado automáticamente con cada venta o compra[cite: 600, 703].
    * [cite_start]Alertas de stock mínimo para prevenir la escasez de productos[cite: 602, 704].
    * [cite_start]Ajuste manual de stock para corregir discrepancias[cite: 95].

* **🚚 Gestión de Compras y Proveedores:**
    * [cite_start]Directorio centralizado para registrar y gestionar proveedores[cite: 610, 712].
    * [cite_start]Creación y seguimiento de órdenes de compra por estado (Borrador, Aprobado, Recibido)[cite: 617, 619, 713].
    * [cite_start]Recepción de mercancía que actualiza el inventario automáticamente[cite: 621, 714, 286].
    * [cite_start]Gestión de pagos a proveedores para llevar un control de las cuentas por pagar[cite: 328, 623].

* **💰 Módulo de Facturación y Ventas:**
    * [cite_start]Generación ágil de facturas de venta, asociadas a un cliente[cite: 626, 707].
    * [cite_start]Cálculo automático de totales, validando la disponibilidad de stock[cite: 627].
    * [cite_start]Historial de ventas detallado, con filtros por cliente, fecha y estado de pago[cite: 628, 709].
    * [cite_start]Gestión de cuentas por cobrar, registrando los abonos de los clientes[cite: 401, 630].

* **📊 Reportes y Exportación:**
    * [cite_start]Generación de reportes de ventas, productos más vendidos y estado del inventario[cite: 677, 720, 721].
    * [cite_start]Funcionalidad para exportar el inventario completo y otros reportes a **Excel** y **PDF**[cite: 107, 311, 722].

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

* [cite_start]**Lenguaje de Programación:** C# [cite: 846]
* [cite_start]**Framework de Escritorio:** WPF (Windows Presentation Foundation) sobre .NET [cite: 688, 846, 848]
* [cite_start]**Base de Datos:** Microsoft SQL Server [cite: 830]
* **Arquitectura:** Modelo-Vista-VistaModelo (MVVM)
* **Herramientas de Desarrollo:** Visual Studio
