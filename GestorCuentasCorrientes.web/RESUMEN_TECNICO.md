# RESUMEN TÉCNICO - GESTOR DE CUENTAS CORRIENTES

**Proyecto**: GestorCuentasCorrientes  
**Framework**: ASP.NET Core MVC (.NET 10)  
**Base de Datos**: SQL Server  
**Autenticación**: ASP.NET Identity (Custom Usuario)  
**PDF**: QuestPDF Community (v2026.9.1)  
**UI**: Bootstrap 5, jQuery  
**Fecha**: Octubre 2026

---

## TABLA DE CONTENIDOS

1. [Estructura de Carpetas](#1-estructura-de-carpetas)
2. [Entidades del Modelo](#2-entidades-del-modelo)
3. [ApplicationDbContext](#3-applicationdbcontext)
4. [Controllers y Acciones](#4-controllers-y-acciones)
5. [ViewModels](#5-viewmodels)
6. [Servicios](#6-servicios)
7. [Vistas Razor](#7-vistas-razor)
8. [Migraciones](#8-migraciones)
9. [Módulo Presupuestos](#9-módulo-presupuestos-estado-actual)
10. [Código Incompleto / TODOs](#10-código-incompleto--todos--pendientes)
11. [Configuración](#11-configuración-del-proyecto)
12. [Puntos Críticos](#12-puntos-críticos--consideraciones)
13. [Diagrama de Relaciones](#13-diagrama-de-relaciones-resumido)

---

## 1. ESTRUCTURA DE CARPETAS

```
GestorCuentasCorrientes.web/
├── Areas/
│   └── Identity/
│       └── Pages/
│           └── Account/
│               ├── Login.cshtml + Login.cshtml.cs
│               ├── Logout.cshtml + Logout.cshtml.cs
│               ├── Register.cshtml + Register.cshtml.cs
│               └── _ViewImports.cshtml, _ViewStart.cshtml
├── App_Data/
│   └── Comprobantes/ (PDFs generados por movimientos)
├── Controllers/
│   ├── ClientesController.cs
│   ├── HomeController.cs
│   ├── MovimientosController.cs
│   ├── PresupuestosController.cs
│   └── ProductosController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Migrations/
│       ├── 00000000000000_CreateIdentitySchema.cs
│       ├── 20260823155524_Inicial.cs
│       ├── 20260823171711_SeedTipoPagoyMovimiento.cs
│       ├── 20260930175224_AgregarPresupuestos.cs
│       ├── 20261002175926_AgregarProductos.cs
│       └── ApplicationDbContextModelSnapshot.cs
├── Models/
│   ├── Cliente.cs
│   ├── Localidad.cs
│   ├── TipoMovimiento.cs
│   ├── MedioPago.cs
│   ├── Usuario.cs (extends IdentityUser)
│   ├── Movimiento.cs
│   ├── Pago.cs
│   ├── Cheque.cs
│   ├── Comprobante.cs
│   ├── Presupuesto.cs
│   ├── PresupuestoDetalle.cs
│   ├── Producto.cs
│   ├── ErrorViewModel.cs
│   └── ViewModels/
│       ├── ClienteDetalleVm.cs
│       ├── MovimientoDetalleVm.cs
│       ├── MovimientoSimpleCreateVm.cs
│       ├── MovimientoRecibosCreateVm.cs (extends MovimientoSimpleCreateVm)
│       ├── PresupuestoCreateVm.cs
│       ├── PresupuestoDetalleVm.cs (nested)
│       ├── PagoLineaVm.cs (nested en ReciboCreateVm)
│       └── ReciboCreateVm.cs
├── Services/
│   └── RecibosPdfService.cs (QuestPDF)
├── Views/
│   ├── Clientes/
│   │   ├── Index.cshtml
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   └── Edit.cshtml
│   ├── Home/
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   ├── Movimientos/
│   │   └── Create.cshtml
│   ├── Presupuestos/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   └── Details.cshtml
│   ├── Productos/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   └── Edit.cshtml
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _Layout.cshtml.css
│   │   ├── _LoginPartial.cshtml
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── wwwroot/
│   ├── css/site.css
│   ├── js/site.js
│   ├── lib/ (Bootstrap 5, jQuery, jQuery Validation)
│   └── favicon.ico
├── Properties/
│   ├── launchSettings.json
│   └── serviceDependencies.json
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

---

## 2. ENTIDADES DEL MODELO

### Localidad

```
Propiedades:
- Id (int, PK)
- Nombre (string, 100 chars, Required)
- Provincia (string?, 100 chars)

Relaciones:
- 1→N con Cliente (Localidad.Clientes)
```

### TipoMovimiento

```
Propiedades:
- Id (int, PK)
- Codigo (string, 10 chars, Required, Unique)
- Nombre (string, 50 chars, Required)
- Signo (short, Required; valores: 1 o -1 via CHECK constraint)

Seed Data (6 registros):
1. FACT - Factura (Signo: 1)
2. REC - Recibo (Signo: -1)
3. NC - Nota de crédito (Signo: -1)
4. ND - Nota de débito (Signo: 1)
5. AJU_D - Ajuste débito (Signo: 1)
6. AJU_C - Ajuste crédito (Signo: -1)

Relaciones:
- 1→N con Movimiento (TipoMovimiento.Movimientos)
```

### MedioPago

```
Propiedades:
- Id (int, PK)
- Nombre (string, 50 chars, Required, Unique)

Seed Data (6 registros):
1. Efectivo
2. Transferencia
3. Cheque
4. E-cheque
5. Tarjeta débito
6. Tarjeta crédito

Relaciones:
- 1→N con Pago (MedioPago.Pagos)
```

### Usuario (extends IdentityUser)

```
Propiedades:
- Id (string, inherited from IdentityUser)
- UserName (inherited)
- Email (inherited)
- Nombre (string, 100 chars, Required)
- Apellido (string, 100 chars, Required)

Relaciones:
- 1→N con Movimiento (Usuario.Movimientos)
```

### Cliente

```
Propiedades:
- Id (int, PK)
- RazonSocial (string, 150 chars, Required)
- CuitDni (string?, 20 chars; índice único filtrado para NULL)
- Direccion (string?, 200 chars)
- Telefono (string?, 30 chars)
- Email (string?, 100 chars; validated as email)
- Activo (bool, default: true)
- FechaAlta (DateTime, default SQL: GETDATE())
- LocalidadId (int, FK Required)

Relaciones:
- N→1 con Localidad (via LocalidadId, DeleteBehavior.Restrict)
- 1→N con Movimiento (Cliente.Movimientos, DeleteBehavior.Restrict)
```

### Movimiento ⚠️ CRÍTICO

```
Propiedades:
- Id (int, PK)
- Fecha (DateTime, Required, default: DateTime.Now)
- NumeroComprobante (string?, 30 chars)
- Importe (decimal, precision 12,2, Required, CHECK > 0)
- Observaciones (string?, 500 chars)
- FechaRegistro (DateTime, default SQL: GETDATE())
- Anulado (bool, default: false)
- ClienteId (int, FK Required)
- TipoMovimientoId (int, FK Required)
- UsuarioId (string, FK Required)
- MovimientoOrigenId (int?, FK nullable; auto-referencia)

Relaciones (TODAS con DeleteBehavior.Restrict):
- N→1 con Cliente (via ClienteId)
- N→1 con TipoMovimiento (via TipoMovimientoId)
- N→1 con Usuario (via UsuarioId)
- N→1 con Movimiento (auto-referencia MovimientoOrigen)
- 1→N con Movimiento (MovimientosOriginados; colección de movimientos originados)
- 1→N con Pago (Movimiento.Pagos)
- 1→N con Comprobante (Movimiento.Comprobantes)

Nota importante:
"Esto me gustaria entenderlo mejor y que el chat me lo explique" (comentario en OnModelCreating)
- La auto-referencia permite vincular movimientos originador (ej: factura → nota de crédito)
```

### Pago

```
Propiedades:
- Id (int, PK)
- Importe (decimal, precision 12,2, Required, CHECK > 0)
- MovimientoId (int, FK Required)
- MedioPagoId (int, FK Required)

Relaciones:
- N→1 con Movimiento (via MovimientoId, DeleteBehavior.Restrict)
- N→1 con MedioPago (via MedioPagoId, DeleteBehavior.Restrict)
- 1:1 con Cheque (Pago.Cheque, nullable, DeleteBehavior.Restrict)
```

### Cheque

```
Propiedades:
- Id (int, PK)
- Numero (string, 20 chars, Required)
- Banco (string?, 100 chars)
- Titular (string?, 150 chars)
- FechaEmision (DateTime, Required)
- FechaCobro (DateTime, Required)
- Estado (string, 20 chars, default: "EnCartera")
  CHECK constraint: Estado IN ('EnCartera', 'Depositado', 'Acreditado', 'Rechazado')
- PagoId (int, FK Required; unique index)

Relaciones:
- 1:1 con Pago (via PagoId, DeleteBehavior.Restrict)
```

### Comprobante

```
Propiedades:
- Id (int, PK)
- NombreArchivo (string, 255 chars, Required)
- RutaArchivo (string, 500 chars, Required)
- TipoArchivo (string?, 10 chars)
- FechaCarga (DateTime, default SQL: GETDATE())
- MovimientoId (int, FK Required)

Relaciones:
- N→1 con Movimiento (via MovimientoId, DeleteBehavior.Restrict)
```

### Presupuesto

```
Propiedades:
- Id (int, PK)
- Fecha (DateTime, Required, default: DateTime.Now)
- Observaciones (string?, 500 chars)
- Estado (string, default: "Pendiente")
  CHECK constraint: Estado IN ('Pendiente', 'Aprobado', 'Rechazado', 'Anulado')
- FechaRegistro (DateTime, default SQL: GETDATE())
- ClienteId (int, FK Required)
- UsuarioId (string, FK Required)

Relaciones:
- N→1 con Cliente (via ClienteId, DeleteBehavior.Restrict)
- N→1 con Usuario (via UsuarioId, DeleteBehavior.Restrict)
- 1→N con PresupuestoDetalle (Presupuesto.Detalles)

Nota: Presupuesto nunca se borra, solo se anula (Estado = "Anulado")
```

### PresupuestoDetalle

```
Propiedades:
- Id (int, PK)
- Descripcion (string, 200 chars, Required)
- Cantidad (decimal, precision 18,3, Required)
- PrecioUnitario (decimal, precision 18,2, Required)
- PresupuestoId (int, FK Required)

Relaciones:
- N→1 con Presupuesto (via PresupuestoId; SIN DeleteBehavior.Restrict a propósito)

Nota: Sin Restrict porque Presupuesto nunca se borra, solo se anula
```

### Producto

```
Propiedades:
- Id (int, PK)
- Nombre (string, 200 chars, Required)
- PrecioUnitario (decimal, precision 18,2, Required, CHECK > 0)
- Activo (bool, default: true)

Relaciones:
- Sin FKs. Se referencia por ProductoId en PresupuestoDetalle (no es FK en DB)
- Se utiliza en presupuestos: se copia Nombre + Precio al detalle, luego el detalle es independiente
```

---

## 3. APPLICATIONDBCONTEXT

**Archivo**: `Data/ApplicationDbContext.cs`

### DbSets Registrados

```csharp
DbSet<Localidad> Localidades
DbSet<TipoMovimiento> TiposMovimiento
DbSet<MedioPago> MediosPago
DbSet<Usuario> Usuarios
DbSet<Cliente> Clientes
DbSet<Movimiento> Movimientos
DbSet<Pago> Pagos
DbSet<Cheque> Cheques
DbSet<Comprobante> Comprobantes
DbSet<Presupuesto> Presupuestos
DbSet<PresupuestoDetalle> PresupuestoDetalles
DbSet<Producto> Productos
```

### Configuraciones en OnModelCreating (Fluent API)

**Localidad**:
- Key: Id
- MaxLength: Nombre (100), Provincia (100)
- Relación: 1→N con Cliente

**TipoMovimiento**:
- Key: Id
- Unique Index: Codigo
- CHECK Constraint: Signo IN (1, -1)
- Seed Data: 6 registros (FACT, REC, NC, ND, AJU_D, AJU_C)
- Relación: 1→N con Movimiento

**MedioPago**:
- Key: Id
- Unique Index: Nombre
- Seed Data: 6 registros (Efectivo, Transferencia, Cheque, E-cheque, Tarjeta débito, Tarjeta crédito)
- Relación: 1→N con Pago

**Usuario**:
- MaxLength: Nombre (100), Apellido (100)

**Cliente**:
- Key: Id
- Unique Filtered Index: CuitDni (permite múltiples NULL)
- Relación N→1: Localidad (OnDelete: Restrict)
- Relación 1→N: Movimiento (OnDelete: Restrict)

**Movimiento**:
- Key: Id
- CHECK Constraint: Importe > 0
- 4 Relaciones N→1 (TODAS con Restrict):
  - Cliente (OnDelete: Restrict)
  - TipoMovimiento (OnDelete: Restrict)
  - Usuario (OnDelete: Restrict)
  - Movimiento/Auto-referencia (OnDelete: Restrict)
- Relación 1→N: Pago (OnDelete: Restrict)
- Relación 1→N: Comprobante (OnDelete: Restrict)

**Pago**:
- Key: Id
- CHECK Constraint: Importe > 0
- Relación N→1: Movimiento (OnDelete: Restrict)
- Relación N→1: MedioPago (OnDelete: Restrict)
- Relación 1:1: Cheque (OnDelete: Restrict)

**Cheque**:
- Key: Id
- CHECK Constraint: Estado IN ('EnCartera', 'Depositado', 'Acreditado', 'Rechazado')
- Unique Index: PagoId (1:1 relationship)
- Relación 1:1: Pago (OnDelete: Restrict)

**Comprobante**:
- Key: Id
- Relación N→1: Movimiento (OnDelete: Restrict)

**Presupuesto**:
- CHECK Constraint: Estado IN ('Pendiente', 'Aprobado', 'Rechazado', 'Anulado')
- Relación N→1: Cliente (OnDelete: Restrict)
- Relación N→1: Usuario (OnDelete: Restrict)
- Relación 1→N: PresupuestoDetalle

**PresupuestoDetalle**:
- Precision: Cantidad (18,3), PrecioUnitario (18,2)
- Relación N→1: Presupuesto (sin Restrict por diseño)

**Producto**:
- Precision: PrecioUnitario (18,2)

---

## 4. CONTROLLERS Y ACCIONES

### ClientesController ([Authorize])

| Acción | Método | Parámetros | Descripción |
|--------|--------|-----------|-------------|
| Index | GET | searchTerm?, localidadId?, estado? | Listado con filtros: búsqueda (RazonSocial/CuitDni), localidad, estado (1=Activo, 0=Inactivo). Ordenado por RazonSocial. |
| Details | GET | id | Detalle de cliente + tabla de movimientos (todos, incluidos anulados) con saldo acumulado. Retorna ClienteDetalleVm. |
| Create | GET | - | Formulario de creación. Carga Localidades en ViewBag.LocalidadId. |
| Create | POST | [Bind] Cliente | Crea cliente. Bind: RazonSocial, CuitDni, Direccion, Telefono, Email, LocalidadId. Asigna Activo=true, FechaAlta=DateTime.Now. |
| Edit | GET | id | Formulario de edición con datos preexistentes. |
| Edit | POST | id, [Bind] Cliente | Edita cliente preservando FechaAlta original. Bind incluye Activo. |
| CambiarEstado | POST | id | [Authorize(Roles="Admin")] Alterna Activo. |

### HomeController (sin [Authorize])

| Acción | Método | Descripción |
|--------|--------|-------------|
| Index | GET | Vista pública de inicio. |
| Privacy | GET | Página de privacidad. |
| Error | GET | [ResponseCache] Página de error genérica. |

### MovimientosController ([Authorize])

| Acción | Método | Parámetros | Descripción |
|--------|--------|-----------|-------------|
| Create | GET | - | Formulario para crear movimiento simple o recibo. Carga TiposMovimiento, Clientes en ViewBag. Retorna form con MovimientoRecibosCreateVm. |
| Create | POST | [Bind] MovimientoRecibosCreateVm | Crea movimiento según tipo: si TipoMovimientoId=2 (Recibo), llama a CrearRecibo(); si otro, crea Movimiento simple (Factura, NC, ND, Ajuste). Maneja upload comprobante (IFormFile). |
| CrearRecibo | private | viewModel, clienteId, usuarioId | Helper private async para crear Movimiento Recibo con N Pagos. Opcionalmente genera PDF. |
| PruebaPdf | GET | - | [Sin [Authorize]] Endpoint de prueba. Genera PDF de recibo de prueba. Retorna FileContentResult. |
| VerComprobante | GET | id | Descarga comprobante (PDF) asociado a movimiento. Retorna FileStreamResult. |

**Flujo de Movimientos**:
- **Movimiento Simple** (Factura, NC, ND, Ajuste): 1 Movimiento + 0 o 1 Comprobante
- **Recibo** (TipoMovimientoId=2): 1 Movimiento + N Pagos + 0 o 1 Comprobante + opcionalmente N Cheques (si pago es tipo Cheque/E-cheque)

### PresupuestosController ([Authorize])

| Acción | Método | Parámetros | Descripción |
|--------|--------|-----------|-------------|
| Index | GET | - | Listado de presupuestos con estados (Pendiente, Aprobado, Rechazado, Anulado). |
| Create | GET | - | Formulario para crear presupuesto. Carga Clientes, Productos en ViewBag. Retorna PresupuestoCreateVm. |
| Create | POST | [Bind] PresupuestoCreateVm | Crea Presupuesto + N PresupuestoDetalles. Busca Producto por Id, copia Nombre + Precio al detalle. |
| Details | GET | id | Detalle de presupuesto con líneas y botones de acción (Aceptar, Rechazar). |
| Pdf | GET | id | Genera PDF del presupuesto usando QuestPDF. Retorna FileContentResult. |
| Aceptar | POST | id | Cambia Estado a "Aprobado". |
| Rechazar | POST | id | Cambia Estado a "Rechazado". |

**Nota**: No existe funcionalidad para convertir presupuesto aprobado en factura.

### ProductosController ([Authorize])

| Acción | Método | Parámetros | Descripción |
|--------|--------|-----------|-------------|
| Index | GET | searchTerm? | Listado de productos con búsqueda por Nombre. Ordenado por Nombre. |
| Create | GET | - | Formulario de creación. |
| Create | POST | [Bind] Producto | Crea con Bind: Nombre, PrecioUnitario. Asigna Activo=true. |
| Edit | GET | id | Formulario de edición. |
| Edit | POST | id, [Bind] Producto | Edita Nombre, PrecioUnitario. Bind incluye Activo. |
| CambiarEstado | POST | id | Alterna Activo (Activar/Desactivar). |

---

## 5. VIEWMODELS

| ViewModel | Propiedad | Tipo | Nullable | Notas |
|-----------|-----------|------|----------|-------|
| **ClienteDetalleVm** | | | | |
| | Cliente | Cliente | false | Objeto completo del cliente |
| | SaldoActual | decimal | false | Saldo acumulado |
| | Movimientos | List<MovimientoDetalleVm> | false | Historial de movimientos |
| **MovimientoDetalleVm** | | | | |
| | Id | int | false | |
| | Fecha | DateTime | false | |
| | TipoMovimientoNombre | string | false | Nombre del tipo (denormalizado) |
| | NumeroComprobante | string | true | |
| | Importe | decimal | false | |
| | Signo | short | false | 1 o -1 |
| | Anulado | bool | false | |
| | SaldoAcumulado | decimal | false | Para tabla de detalles |
| | ComprobanteId | int? | true | ID del comprobante si existe |
| **MovimientoSimpleCreateVm** | | | | BASE para crear movimientos |
| | ClienteId | int | false | |
| | ClienteNombre | string | true | [BindNever] - solo lectura |
| | TipoMovimientoId | int | false | |
| | Fecha | DateTime | false | default: DateTime.Now |
| | NumeroComprobante | string | true | |
| | Importe | decimal | false | |
| | Observaciones | string | true | |
| | ArchivoComprobante | IFormFile | true | Upload |
| **MovimientoRecibosCreateVm** | | | | EXTIENDE MovimientoSimpleCreateVm |
| | (heredadas de MovimientoSimpleCreateVm) | | | |
| | Pagos | List<PagoLineaVm> | false | Lista de pagos (recibos) |
| **PagoLineaVm** | | | | NESTED en ReciboCreateVm |
| | MedioPagoId | int | false | |
| | Importe | decimal | false | |
| | ChequeNumero | string | true | Si MedioPago es Cheque/E-cheque |
| | ChequeBanco | string | true | |
| | ChequeTitular | string | true | |
| | ChequeFechaEmision | DateTime? | true | |
| | ChequeFechaCobro | DateTime? | true | |
| **ReciboCreateVm** | | | | ALTERNATIVA a MovimientoRecibosCreateVm |
| | ClienteId | int | false | |
| | ClienteNombre | string | true | [BindNever] |
| | Fecha | DateTime | false | |
| | NumeroComprobante | string | true | |
| | Observaciones | string | true | |
| | ArchivoComprobante | IFormFile | true | |
| | Pagos | List<PagoLineaVm> | false | |
| **PresupuestoCreateVm** | | | | Para crear presupuesto |
| | ClienteId | int | false | |
| | Fecha | DateTime | false | |
| | Observaciones | string | true | |
| | Detalles | List<PresupuestoDetalleVm> | false | |
| **PresupuestoDetalleVm** | | | | NESTED en PresupuestoCreateVm |
| | ProductoId | int | false | Referencia a Producto |
| | Cantidad | decimal | false | default: 1 |

**Nota**: MovimientoRecibosCreateVm Y ReciboCreateVm parecen redundantes. El controller elige cuál usar según contexto.

---

## 6. SERVICIOS

### ReciboPdfService

**Ubicación**: `Services/RecibosPdfService.cs`

**Librería**: QuestPDF 2026.9.1 (Community License)

**Método público**:

```csharp
public byte[] GenerarPdf(
	MovimientoRecibosCreateVm viewModel,
	Cliente cliente,
	Dictionary<int, string> mediosPago,
	int movimientoId)
```

**Resultado**: byte[] (PDF binario)

**Contenido del PDF**:
- Encabezado: "RECIBO" (24pt bold), "Gestor de Cuentas Corrientes"
- Línea horizontal separadora
- Datos:
  - N° de Recibo (de NumeroComprobante o "REC-{movimientoId}")
  - Fecha (dd/MM/yyyy)
  - Cliente (RazonSocial)
  - CUIT/DNI (si existe)
- Tabla "Detalle de pagos":
  - Columnas: Medio de Pago, Importe
  - Una fila por PagoLineaVm
- Si hay Cheques asociados:
  - Subsección con datos: Número, Banco, Titular, Fechas emisión/cobro
- Observaciones (si existen)
- Pie de página: "Página X"
- Nota: "Documento generado automáticamente por el sistema"
- Formato: A4, margen 2cm
- Fuente: 10pt por defecto

**Uso**: Llamado desde MovimientosController.Create (POST) para generar PDF de recibo si se marca opción.

---

## 7. VISTAS RAZOR

### Views/Clientes/

**Index.cshtml**:
- Tabla de clientes
- Filtros: búsqueda por nombre/CUIT, localidad (dropdown), estado (Activos/Inactivos)
- Botones: Ver Detalles, Editar, Cambiar Estado (POST form)
- Columnas: Razón Social, CUIT/DNI, Tel/Email, Localidad, Estado, Acciones

**Details.cshtml**:
- Datos del cliente: RazonSocial, CUIT, Dirección, Tel, Email, Localidad, Activo, FechaAlta
- Tabla de movimientos (ClienteDetalleVm.Movimientos):
  - Columnas: Fecha, Tipo Movimiento, Comprobante, Importe, Signo, Anulado, Saldo Acumulado
  - Botones: Ver Comprobante (si existe)
- Saldo Actual destacado

**Create.cshtml**:
- Formulario: RazonSocial (required), CUIT/DNI, Dirección, Teléfono, Email, Localidad (dropdown)
- Validación client/server
- Botones: Crear Cliente, Cancelar

**Edit.cshtml**:
- Mismo que Create pero con datos preexistentes
- Campos adicionales: Activo (checkbox), FechaAlta (readonly)

### Views/Home/

**Index.cshtml**: Página de bienvenida

**Privacy.cshtml**: Política de privacidad

### Views/Movimientos/

**Create.cshtml**:
- Formulario grande y unificado para:
  - Movimientos simples (Factura, NC, ND, Ajuste): 1 movimiento sin pagos
  - Recibos (REC): 1 movimiento + N pagos
- Campos comunes:
  - Cliente (dropdown)
  - Tipo Movimiento (dropdown, Required)
  - Fecha
  - Número Comprobante
  - Importe
  - Observaciones
  - Archivo Comprobante (upload, opcional)
- Sección condicional (si TipoMovimientoId = 2 / Recibo):
  - Tabla dinámmica de pagos:
	- Medio de Pago (dropdown)
	- Importe
	- Si Cheque/E-cheque: Número, Banco, Titular, Fechas
  - Botón "Agregar Pago" (JavaScript)
  - Checkbox "Generar PDF"
- Botones: Crear Movimiento, Cancelar

**Lógica**: Muy compleja (280+ líneas). Model binding automático para array Pagos (Pagos[0].MedioPagoId, etc.)

### Views/Presupuestos/

**Index.cshtml**:
- Tabla de presupuestos
- Columnas: Fecha, Cliente, Estado (badge color), Observaciones, Acciones
- Botones: Ver Detalles, Editar (si Pendiente), Eliminar (si no Aprobado)
- Botón "Nuevo Presupuesto"

**Create.cshtml**:
- Formulario para crear presupuesto
- Campos:
  - Cliente (dropdown, Required)
  - Fecha
  - Observaciones
- Tabla dinánmica de líneas:
  - Producto (dropdown, Required)
  - Cantidad (number, Required, default 1)
  - Precio Unitario (readonly, se llena desde Producto)
  - Total Línea (readonly, Cantidad × Precio)
- Botón "Agregar Línea" (JavaScript)
- Botones: Crear Presupuesto, Cancelar
- Lógica JS: Al cambiar Producto, obtiene precio y actualiza campos

**Details.cshtml**:
- Datos: Fecha, Cliente, Estado (badge), Observaciones, FechaRegistro, Usuario
- Tabla de líneas (read-only):
  - Descripción, Cantidad, Precio Unitario, Total
- Total Presupuesto destacado
- Botones (condicionales según Estado):
  - [Si Pendiente] Aceptar (POST form)
  - [Si Pendiente] Rechazar (POST form)
  - [Siempre] Descargar PDF
  - [Siempre] Volver

**Nota**: PDF se genera dinámicamente vía GET /Presupuestos/Pdf/{id}

### Views/Productos/

**Index.cshtml**:
- Tabla de productos
- Columnas: Nombre, Precio Unitario (currency), Activo (badge), Acciones
- Filtro: búsqueda por Nombre
- Botones: Editar, Activar/Desactivar (POST form)
- Botón "Nuevo Producto"

**Create.cshtml**:
- Formulario: Nombre (required, 200 chars), Precio Unitario (required, currency, > 0)
- Botones: Crear Producto, Cancelar

**Edit.cshtml**:
- Mismo que Create pero con datos preexistentes
- Campo adicional: Checkbox "Activo"

### Views/Shared/

**_Layout.cshtml**:
- Master layout con Bootstrap 5
- Navbar fija/sticky con navegación:
  - Clientes, Movimientos, Presupuestos, Productos
  - User menu (login/logout)
- Container principal
- Footer

**_Layout.cshtml.css**: Estilos personalizados

**_LoginPartial.cshtml**: Partial que muestra usuario autenticado + botón Logout

**_ValidationScriptsPartial.cshtml**: jQuery Validation bundled

**Error.cshtml**: Página de error genérica con ErrorViewModel

### Views/

**_ViewImports.cshtml**: 
```csharp
@using GestorCuentasCorrientes.web
@using GestorCuentasCorrientes.web.Models
@using GestorCuentasCorrientes.web.Models.ViewModels
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

**_ViewStart.cshtml**: 
```csharp
@{
	Layout = "_Layout";
}
```

---

## 8. MIGRACIONES

| Nombre | Fecha | Descripción |
|--------|-------|-------------|
| **00000000000000_CreateIdentitySchema** | (inicial) | Crea tablas Identity de ASP.NET Core (AspNetUsers, AspNetRoles, AspNetUserRoles, etc.) |
| **20260823155524_Inicial** | 23/08/2026 | Crea todas las entidades base: Localidad, Cliente, TipoMovimiento (sin seed), MedioPago (sin seed), Usuario personalizado, Movimiento, Pago, Cheque, Comprobante |
| **20260823171711_SeedTipoPagoyMovimiento** | 23/08/2026 | Inserta seed data: 6 TiposMovimiento (FACT, REC, NC, ND, AJU_D, AJU_C) + 6 MediosPago (Efectivo, Transferencia, Cheque, E-cheque, Tarjeta débito, Tarjeta crédito) |
| **20260930175224_AgregarPresupuestos** | 30/09/2026 | Agrega tablas Presupuesto y PresupuestoDetalle |
| **20261002175926_AgregarProductos** | 02/10/2026 | Agrega tabla Producto |

**Snapshot actual**: `ApplicationDbContextModelSnapshot.cs` (registro de schema en EF Core)

**Estado**: Todas las migraciones aplicadas a base de datos

---

## 9. MÓDULO PRESUPUESTOS: ESTADO ACTUAL

### ✅ QUÉ EXISTE

**1. Modelos** (completos):
- Presupuesto: Todas las propiedades, relaciones, validaciones
- PresupuestoDetalle: Todas las propiedades, relaciones, precisiones

**2. ViewModels** (completos):
- PresupuestoCreateVm: Estructura lista
- PresupuestoDetalleVm: Nested, con Cantidad default 1

**3. Controller** (funcional):
- Index: GET Presupuestos
- Create: GET/POST con creación de Presupuesto + N PresupuestoDetalles
- Details: GET con detalles
- Pdf: GET genera PDF de presupuesto
- Aceptar: POST cambia Estado a "Aprobado"
- Rechazar: POST cambia Estado a "Rechazado"

**4. Vistas** (funcionales):
- Index: Tabla de presupuestos con estados
- Create: Formulario con líneas dinámicas de productos
- Details: Muestra presupuesto con botones Aceptar/Rechazar y enlace PDF

**5. Migraciones** (aplicadas):
- 20260930175224_AgregarPresupuestos creó las tablas

**6. Base de datos**: Presupuestos y PresupuestoDetalles tablas creadas y funcionales

### ❌ QUÉ FALTA O NO ESTÁ CLARO

**1. Conversión a Factura**:
- No existe funcionalidad para convertir presupuesto Aprobado en Movimiento de tipo Factura
- Imposibilita el flujo: Presupuesto → Factura → Recibos

**2. Gestión de PDF**:
- Método Pdf() genera PDF pero Details.cshtml no tiene botón "Descargar PDF"
- PDF se genera cada vez (sin cacheo)

**3. Validaciones de Estado**:
- No hay restricción para impedir aceptar/rechazar si ya está en determinado estado
- No hay validación de que solo Presupuestos Pendientes puedan cambiar de estado

**4. Auditoría**:
- No hay registro de quién aprobó/rechazó
- No hay timestamp de aprobación/rechazo separado de FechaRegistro

**5. Relación con Movimientos**:
- Presupuesto no referencia Movimiento
- Si se convirtiera un Presupuesto en Factura, necesitaría una FK para la auditoría

**6. PresupuestoDetalle sin FK a Producto**:
- PresupuestoDetalle tiene Descripcion + PrecioUnitario pero no ProductoId FK
- Es intencional (copia los datos y se desvincula), pero puede causar confusión

---

## 10. CÓDIGO INCOMPLETO / TODOs / PENDIENTES

### Encontrado en ApplicationDbContext.cs (línea ~150)

```csharp
//Esto me gustaria entenderlo mejor y que el chat me lo explique.
// Auto-referencia: Movimiento puede tener un MovimientoOrigen (N:1)
// Este movimiento origina otros movimientos (1:N)
entity.HasOne(e => e.MovimientoOrigen)
	.WithMany(m => m.MovimientosOriginados)
	.HasForeignKey(e => e.MovimientoOrigenId)
	.OnDelete(DeleteBehavior.Restrict);
```

**Descripción**: Solicita explicación sobre la auto-referencia.

**Propósito**: Vincular movimientos originador (ej: factura original de una nota de crédito). Permite auditoría de relaciones entre movimientos.

### Otros comentarios de contexto

- DbContext línea 23-24: "Agrego 2 nuevas tablas 30/09 para la nueva funcionalidad." (Presupuestos)
- DbContext línea 26: "Agrego nueva tabla 2/10 para tener precios y productos en la base de datos." (Productos)

### Sin encontrar

- No hay TODOs explícitos en Controllers
- No hay comentarios FIXME o PENDIENTE
- PresupuestosController tiene lógica completa

---

## 11. CONFIGURACIÓN DEL PROYECTO

### Framework & Dependencias

```
.NET 10
Entity Framework Core (SQL Server provider)
ASP.NET Identity
QuestPDF 2026.9.1 (Community License)
Bootstrap 5.x
jQuery 3.x
jQuery Validation
```

### Program.cs (Configuración)

```csharp
// QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;

// Database
services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));

// Identity con Usuario personalizado
services.AddIdentity<Usuario, IdentityRole>(options => {
	// Password: 6 chars mínimo, no requiere dígitos, mayúsculas, especiales
	// Account: no requiere confirmación de email
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Services
services.AddScoped<ReciboPdfService>();

// Controllers & Views
services.AddControllersWithViews();
services.AddRazorPages();

// Culture
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("es-AR");
```

### Connection String

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=.;Database=GestorCuentasCorrientes;Trusted_Connection=true;TrustServerCertificate=true;Encrypt=false;"
  }
}
```

### Autenticación

- ASP.NET Identity
- [Authorize] en controllers (requiere login)
- [Authorize(Roles="Admin")] para acciones sensibles (CambiarEstado de Cliente)
- Areas/Identity/Pages para Login/Logout/Register

---

## 12. PUNTOS CRÍTICOS / CONSIDERACIONES

### DeleteBehavior.Restrict en Movimiento

Las 4 foreign keys (Cliente, TipoMovimiento, Usuario, MovimientoOrigen) tienen `OnDelete(DeleteBehavior.Restrict)`.

**Implicación**: No se puede borrar clientes, usuarios o tipos de movimiento que tengan registros asociados.

**Justificación**: Auditoría histórica. Los movimientos deben ser inmutables.

### Auto-referencia en Movimiento

Permite modelar relaciones complejas:
- Factura original → Nota de Crédito (referencia MovimientoOrigenId)
- Cambio → Factura rectificatoria

**Estructura**:
- MovimientoOrigen: Movimiento? (la factura original)
- MovimientosOriginados: ICollection<Movimiento> (notas/cambios que refieren a este)

**Patrón**: Restricción en cascada (Restrict) pero permite auditoría completa.

### Presupuesto nunca se borra

Solo se anula (`Estado = "Anulado"`).

**Implicación**: PresupuestoDetalle NO tiene `Restrict` en FK porque nunca habrá cascada de borrado real.

**Validación**: Implementada en UI pero no en DB constraints (validar en controller).

### Cheque es 1:1 con Pago

Un Pago de MedioPago = Cheque/E-cheque genera 1 Cheque.

**Validación**: En el controller MovimientosController.Create (POST), se valida que si há línea de pago con MedioPago Cheque, se cree el Cheque correspondiente.

### PDF de Recibo

Genera en memoria (byte[] retornado al cliente).

**Almacenamiento**: El archivo comprobante (IFormFile original) se guarda en App_Data/Comprobantes/{MovimientoId}/. El PDF generado puede retenerse o descartarse (actualmente se descarita).

### Vista Create de Movimientos

Gigante (~280 líneas). Lógica unificada para:
- Movimientos simples (Factura, NC, ND, Ajuste)
- Recibos (REC)

**Mejora futura**: Refactorizar en 2 vistas separadas o usar Partial Views.

### ViewBag vs ViewModel

Se usa `ViewBag` para dropdowns (Localidades, Clientes, Productos) en lugar de pasar datos por ViewModels.

**Ventaja**: Flexibilidad
**Desventaja**: Menos tipado, reflejo en compilación

---

## 13. DIAGRAMA DE RELACIONES (resumido)

```
[Localidad] 
	↓ 1:N
[Cliente] ← Restrict (no se borra si tiene movimientos)
	↓ 1:N (Restrict)
[Movimiento] ← Restrict en 4 FKs
	├─ N:1 → [TipoMovimiento]
	├─ N:1 → [Usuario]
	├─ N:1 (auto-ref) → [Movimiento] (MovimientoOrigen)
	│   └─ 1:N (auto-ref) → [Movimiento] (MovimientosOriginados)
	├─ 1:N → [Pago] (Restrict)
	│   ├─ N:1 → [MedioPago]
	│   └─ 1:1 → [Cheque] (Restrict)
	└─ 1:N → [Comprobante] (Restrict)

[Usuario]
	└─ 1:N → [Movimiento]

[Usuario]
	└─ 1:N → [Presupuesto] (Restrict)

[Cliente]
	└─ 1:N → [Presupuesto] (Restrict)

[Presupuesto]
	└─ 1:N → [PresupuestoDetalle]
		└─ ref ProductoId (no FK en DB)

[Producto] (stand-alone, sin FKs)
```

---

## RESUMEN EJECUTIVO

**GestorCuentasCorrientes** es una aplicación ASP.NET Core MVC para gestionar cuentas corrientes de clientes.

**Funcionalidades**:
1. **Clientes**: CRUD con localidades, activo/inactivo, historial de movimientos con saldo acumulado
2. **Movimientos**: Factura, Recibos, Notas (crédito/débito), Ajustes. Recibos con N pagos y cheques
3. **Pagos**: Múltiples medios (efectivo, transferencia, cheque, e-cheque, tarjetas)
4. **Cheques**: Seguimiento de estado (En cartera, depositado, acreditado, rechazado)
5. **Presupuestos**: Cotizaciones a clientes, cambio de estado (Pendiente→Aprobado/Rechazado), generación de PDF
6. **Productos**: Catálogo de precios para presupuestos
7. **PDF**: Recibos y presupuestos con QuestPDF

**Arquitectura**:
- MVC estándar con Controllers, Models, Views
- EF Core Migrations con SQL Server
- ASP.NET Identity con roles (Admin, User)
- Fluent API para constraints y relaciones complejas
- Services pattern (ReciboPdfService)

**Estado**: Funcional, con posibles mejoras en presupuestos→factura conversion y auditoría.

---

**Generado**: Octubre 2026  
**Última modificación**: 02/10/2026 (Agrega Productos)  
**Rama Git**: master  
**Repositorio**: https://github.com/jonathanjordanferreyra/GestorCuentasCorrientes
