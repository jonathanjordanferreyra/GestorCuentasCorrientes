# DOCUMENTACIÓN DEL PROYECTO - GUÍA RÁPIDA

Estos archivos contienen un resumen técnico completo del proyecto **GestorCuentasCorrientes** para que puedas compartir con otros desarrolladores.

## 📄 Archivos disponibles

### 1. **RESUMEN_TECNICO.md** (Markdown)
   - **Mejor para**: Lectura en GitHub, wikis, documentación online
   - **Formato**: Markdown con tablas, código, estructura jerárquica
   - **Contenido**: Idéntico al .txt pero con mejor formato visual
   - **Tamaño**: ~25 KB
   - **Recomendado para**: Compartir en repositorio, visualizar en navegadores

### 2. **RESUMEN_TECNICO.txt** (Texto plano)
   - **Mejor para**: Cualquier aplicación, transportable, compatible universal
   - **Formato**: Texto ASCII con líneas separadoras claras
   - **Contenido**: Idéntico al .md pero en texto puro
   - **Tamaño**: ~20 KB
   - **Recomendado para**: Pasar a cualquier IA, PDF conversión, portabilidad

### 3. **INDICE.md** (Este archivo)
   - **Descripción**: Guía rápida de documentación

---

## 📋 Tabla de Contenidos de ambos archivos

Ambos RESUMEN_TECNICO contienen:

| Sección | Descripción |
|---------|-------------|
| 1. Estructura de Carpetas | Organización del proyecto |
| 2. Entidades y Propiedades | Modelos DB con tipos y relaciones |
| 3. ApplicationDbContext | DbSets y configuraciones Fluent API |
| 4. Controllers y Acciones | Métodos HTTP y funcionalidades |
| 5. ViewModels | Estructuras de datos para vistas |
| 6. Servicios | RecibosPdfService y otros helpers |
| 7. Vistas Razor | Archivos .cshtml por funcionalidad |
| 8. Migraciones | Historial de cambios BD |
| 9. Módulo Presupuestos | Estado actual: qué existe y qué falta |
| 10. TODO / Pendientes | Código incompleto o notas del dev |
| 11. Configuración | Framework, dependencias, Program.cs |
| 12. Puntos Críticos | Decisiones de diseño importantes |
| 13. Diagrama de Relaciones | Mapa visual de entidades |

---

## 🚀 Cómo usar estos archivos

### Para una IA (ChatGPT, Claude, GitHub Copilot, etc.)
1. Descarga **RESUMEN_TECNICO.txt** (texto puro)
2. Copia y pega el contenido en la IA
3. Pregunta sobre arquitectura, cambios, mejoras
4. La IA tendrá contexto completo del proyecto

### Para un navegador web
1. Sube **RESUMEN_TECNICO.md** a GitHub / GitLab
2. Se visualizará automáticamente con formato bonito
3. Comparte el enlace con el equipo

### Para conversión a PDF
1. Abre **RESUMEN_TECNICO.md** en navegador
2. Imprime a PDF (Ctrl+P → "Guardar como PDF")
3. O usa herramienta como Pandoc

### Para documentación Confluence/Notion
1. Copia **RESUMEN_TECNICO.md**
2. Pega en Confluence/Notion
3. Formatea según el estilo del espacio

---

## 🔍 Información clave extraída

### Tecnologías principales
- **.NET 10** ASP.NET Core MVC
- **SQL Server** con Entity Framework Core
- **QuestPDF** para generación de PDFs
- **Bootstrap 5** + jQuery para UI
- **ASP.NET Identity** para autenticación

### Modelos importantes
- **Movimiento**: Factura, Recibo, Notas (crédito/débito), Ajustes
- **Presupuesto**: Cotizaciones a clientes
- **Cliente**: Gestión con localidades y cuentas corrientes
- **Producto**: Catálogo de precios

### Controllers
- `ClientesController`: CRUD clientes
- `MovimientosController`: Crear movimientos y recibos
- `PresupuestosController`: Gestión de presupuestos
- `ProductosController`: CRUD productos
- `HomeController`: Páginas públicas

### ViewModels
7 ViewModels para modelar datos complejos (Recibos con N pagos, Presupuestos con N líneas)

### Migraciones
5 migraciones aplicadas: Identity base → Inicial → Seed TPOs/Medios → Presupuestos → Productos

---

## ⚠️ Puntos críticos encontrados

1. **Auto-referencia en Movimiento**: Permite vincular Factura → Nota de Crédito
   - Comentario en código: *"Esto me gustaria entenderlo mejor"*

2. **DeleteBehavior.Restrict en 4 FKs de Movimiento**: Impide borrar datos con auditoría

3. **Presupuesto no convierte a Factura**: Falta funcionalidad e integración

4. **Vista Create Movimientos**: Muy grande (280 líneas), unificada para 2 tipos

5. **PresupuestoDetalle** copia datos de Producto (no tiene FK)

---

## 📚 Para developers que continúen

### Próximos pasos sugeridos
- [ ] Implementar conversión Presupuesto Aprobado → Movimiento Factura
- [ ] Agregar auditoría (quién/cuándo aprobó presupuesto)
- [ ] Refactorizar Movimientos/Create.cshtml (separar lógica)
- [ ] Validaciones de estado (no aceptar si no está Pendiente)
- [ ] Agregar botón "Descargar PDF" en Details presupuesto

### Testing recomendado
- Crear Factura con Recibo múltiple (N pagos, con cheques)
- Auto-referencia: Factura → Nota de Crédito
- Conversión Presupuesto → Factura (cuando se implemente)
- Validaciones de estados y restricciones

---

## 📞 Información del proyecto

**Repositorio**: https://github.com/jonathanjordanferreyra/GestorCuentasCorrientes  
**Rama**: master  
**Última actualización**: 02/10/2026  
**Generado**: Octubre 2026

---

**Preguntas recurrentes**:
- ¿Dónde está el schema DB? → Migraciones + OnModelCreating en ApplicationDbContext.cs
- ¿Cómo generar PDFs? → ReciboPdfService (QuestPDF)
- ¿Cómo autenticar? → [Authorize] en controllers + ASP.NET Identity
- ¿Flujo de Recibo? → MovimientosController.Create (POST) → CrearRecibo() → ReciboPdfService

---

**Última parte del checklist**: ✅ Documentación completada y exportada
