using DGASoporte.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using DGASoporte.Data;
using DGASoporte.Models;

namespace TuProyecto.Controllers
{
    public class DiagnosticadorController : Controller
    {
        private readonly DGADbContext _context;
        private readonly ILogger<DiagnosticadorController> _logger;

        public DiagnosticadorController(DGADbContext context, ILogger<DiagnosticadorController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Endpoint de diagnóstico
        public IActionResult Index()
        {
            var diagnostico = new
            {
                Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Usuario = User.Identity?.Name ?? "No autenticado",
                BaseDatos = new
                {
                    Conexion = _context.Database.CanConnect() ? "OK" : "FALLO",
                    Unidades = _context.Unidades?.Count() ?? -1,
                    Division = _context.Divisiones?.Count() ?? -1
                },
                Solicitudes = _context.Solicitudes?.Count() ?? -1
            };

            return Content(JsonSerializer.Serialize(diagnostico, new JsonSerializerOptions { WriteIndented = true }), "application/json");
        }

        // Simula la recepción de datos del formulario
        [HttpPost]
        public async Task<IActionResult> SimularGuardado([FromBody] JsonElement datos)
        {
            _logger.LogInformation("=== DIAGNÓSTICO 400: INICIO ===");
            _logger.LogInformation("Datos recibidos: {Datos}", datos.GetRawText());
            _logger.LogInformation("Headers: {Headers}", JsonSerializer.Serialize(Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())));

            try
            {
                // Intenta deserializar a SolicitudVM
                var jsonString = datos.GetRawText();
                var modelo = JsonSerializer.Deserialize<SolicitudVM>(jsonString);

                if (modelo == null)
                {
                    _logger.LogWarning("No se pudo deserializar el JSON");
                    return Json(new
                    {
                        error = "Error de deserialización",
                        mensaje = "No se pudieron leer los datos del formulario",
                        solucion = "Verificar que el JSON sea válido"
                    });
                }

                _logger.LogInformation("Modelo deserializado: {Titulo}", modelo.Titulo ?? "NULO");

                // Validación manual
                var errores = new List<string>();

                if (string.IsNullOrWhiteSpace(modelo.Titulo))
                    errores.Add("Titulo es requerido");

                if (string.IsNullOrWhiteSpace(modelo.Descripcion))
                    errores.Add("Descripcion es requerida");

                if (modelo.UnidadId <= 0)
                    errores.Add("UnidadId debe ser mayor a 0");

                if (modelo.Tipo == 0)
                    errores.Add("Tipo es requerido");

                if (errores.Any())
                {
                    _logger.LogWarning("Errores de validación: {Errores}", string.Join(", ", errores));
                    return Json(new
                    {
                        error = "Error de validación",
                        errores = errores,
                        modeloRecibido = modelo,
                        solucion = "Completar todos los campos requeridos"
                    });
                }

                // Verificar si la unidad existe
                var unidad = await _context.Unidades.FindAsync(modelo.UnidadId);
                if (unidad == null)
                {
                    _logger.LogWarning("Unidad no encontrada: {UnidadId}", modelo.UnidadId);
                    return Json(new
                    {
                        error = "Unidad no encontrada",
                        UnidadId = modelo.UnidadId,
                        // ToList() NUNCA devuelve null si se usa en una consulta LINQ
                        unidadesDisponibles = _context.Unidades
                        .Select(u => new { u.Id, u.Nombre })
                        .ToList(),
                        solucion = "Seleccionar una unidad válida de la lista"
                    });
                }

                _logger.LogInformation("Unidad encontrada: {Nombre}", unidad.Nombre);

                // Simular guardado exitoso
                var solicitud = new Solicitud
                {
                    Titulo = modelo.Titulo!,
                    Descripcion = modelo.Descripcion!,
                    UnidadId = modelo.UnidadId,
                    Tipo = modelo.Tipo!.Value,
                    FechaCreacion = DateTime.Now
                };

                _context.Solicitudes.Add(solicitud);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Solicitud guardada exitosamente: {Id}", solicitud.Id);

                return Json(new
                {
                    exito = true,
                    mensaje = "Simulación exitosa",
                    idGenerado = solicitud.Id,
                    datosGuardados = solicitud
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en diagnóstico");
                return Json(new
                {
                    error = "Excepción del servidor",
                    mensaje = ex.Message,
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        // Endpoint para probar el formulario real
        public IActionResult FormularioPrueba()
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Formulario de Prueba</title>
    <style>
        body { font-family: Arial; margin: 20px; }
        .form-group { margin: 10px 0; }
        label { display: inline-block; width: 100px; }
        input, select, textarea { width: 300px; padding: 5px; }
        button { padding: 10px 20px; margin: 10px 0; }
        .result { background: #f0f0f0; padding: 10px; margin: 10px 0; border: 1px solid #ccc; }
        .error { background: #ffebee; border-color: #f44336; }
        .success { background: #e8f5e8; border-color: #4caf50; }
    </style>
</head>
<body>
    <h2>Formulario de Prueba - Diagnóstico 400</h2>
    
    <div class='form-group'>
        <label>Título:</label>
        <input type='text' id='titulo' value='Incidencia de prueba'>
    </div>
    
    <div class='form-group'>
        <label>Descripción:</label>
        <textarea id='descripcion' rows='3'>Descripción de prueba para diagnóstico</textarea>
    </div>
    
    <div class='form-group'>
        <label>Unidad:</label>
        <select id='unidadId'>
            <option value='1'>Seleccionar unidad...</option>
        </select>
    </div>
    
    <div class='form-group'>
        <label>Tipo:</label>
        <select id='tipo'>
            <option value='0'>Seleccionar tipo...</option>
            <option value='1'>Hardware</option>
            <option value='2'>Software</option>
            <option value='3'>Red</option>
            <option value='4'>Seguridad</option>
        </select>
    </div>
    
    <button onclick='enviarDatos()'>Enviar a Diagnóstico</button>
    <button onclick='cargarUnidades()'>Cargar Unidades</button>
    
    <div id='resultado'></div>

    <script>
        function mostrarResultado(data, esError = false) {
            const div = document.getElementById('resultado');
            div.className = 'result ' + (esError ? 'error' : 'success');
            div.innerHTML = '<h3>Resultado:</h3><pre>' + JSON.stringify(data, null, 2) + '</pre>';
        }

        function enviarDatos() {
            const datos = {
                Titulo: document.getElementById('titulo').value,
                Descripcion: document.getElementById('descripcion').value,
                UnidadId: parseInt(document.getElementById('unidadId').value),
                Tipo: parseInt(document.getElementById('tipo').value)
            };

            console.log('Enviando datos:', datos);

            fetch('/Diagnosticador/SimularGuardado', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(datos)
            })
            .then(response => response.json())
            .then(data => {
                console.log('Respuesta:', data);
                if (data.error) {
                    mostrarResultado(data, true);
                } else {
                    mostrarResultado(data, false);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                mostrarResultado({error: 'Error de red', mensaje: error.message}, true);
            });
        }

        function cargarUnidades() {
            fetch('/Diagnosticador/Index')
            .then(response => response.json())
            .then(data => {
                console.log('Diagnóstico:', data);
                // No es necesario cargar unidades aquí, es solo para verificar conectividad
            })
            .catch(error => {
                console.error('Error:', error);
            });
        }

        // Cargar unidades al iniciar
        window.onload = function() {
            console.log('Formulario de prueba cargado');
        };
    </script>
</body>
</html>";

            return Content(html, "text/html");
        }
    }
}