using LAMCatalogos.Comunes;
using LAMInventarios.Clases.DTO.Movimiento;
using LAMInventarios.Clases.Entidades.Movimiento;
using LAMInventarios.Contextos;
using Microsoft.EntityFrameworkCore;

namespace LAMVentas.Backend
{
    public class Ventas
    {
        private readonly ContextoInventarios _contexto;

        public Ventas(ContextoInventarios contexto)
        {
            _contexto = contexto;
        }

        public async Task<Resultado<Venta>> AplicarAsync(Guid? id, Guid? usuarioId)
        {
            Resultado<Venta> resultado = new();

            if (id == null || id == Guid.Empty)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la venta es incorrecto.";
                return resultado;
            }
            if (usuarioId == null || usuarioId == Guid.Empty)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador del usuario es incorrecto.";
                return resultado;
            }
            var venta = await _contexto.Ventas.FindAsync(id);
            if (venta == null)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la venta es incorrecto.";
                return resultado;
            }
            if (venta.Aplicado)
            {
                resultado.Error = true;
                resultado.Mensaje = "El movimiento de venta ya fue aplicado, no se puede realizar cambios.";
                return resultado;
            }

            //actualizar valores de venta.
            venta.Aplicado = true;
            venta.FechaDeActualizacion = DateTime.Now;
            venta.UsuarioId = (Guid)usuarioId!;

            //obtener detalle de venta y validar que al menos haya un registro capturado.
            var detalles = await _contexto.VentasDetalles
                .Include(e => e.Almacenes)
                .Include(e => e.Productos)
                .ThenInclude(e => e!.Paquetes)
                .Include(e => e.Productos)
                .ThenInclude(e => e!.TiposDeContenedores)
                .Where(c => c.VentaId == id).ToListAsync();
            if (detalles == null || !detalles.Any())
            {
                resultado.Error = true;
                resultado.Mensaje = "Ingresar al menos un movimiento a la venta.";
                return resultado;
            }

            List<ExistenciaDTO> existencias = new();

            foreach (var detalle in detalles)
            {
                Guid almacenId = (Guid)detalle.AlmacenId!;
                Guid productoPiezaId = (Guid)detalle.ProductoId!;
                decimal cantidad = (decimal)detalle.Cantidad!;
                decimal precioDeCosto = (decimal)detalle.PrecioDeCosto!;

                if (detalle.Productos != null && detalle.Productos.TiposDeContenedores != null)
                {
                    //si el producto es pieza, quitar decimales.
                    if (detalle.Productos.TiposDeContenedores.Pieza)
                        cantidad = (int)detalle.Cantidad!;
                    //si el producto es paquete, obtener la cantidad de piezas del contenedor.
                    if (detalle.Productos.TiposDeContenedores.Paquete)
                    {
                        if (detalle.Productos.Paquetes != null)
                        {
                            detalle.Productos.PrecioDeCosto = precioDeCosto;
                            productoPiezaId = detalle.Productos.Paquetes.ProductoPiezaId;
                            precioDeCosto /= detalle.Productos.Paquetes.CantidadDeProductosPorPaquete;
                            cantidad *= detalle.Productos.Paquetes.CantidadDeProductosPorPaquete;
                        }
                    }

                    var existencia = existencias.FirstOrDefault(e => e.ProductoId == productoPiezaId && e.AlmacenId == detalle.AlmacenId);
                    if (existencia == null)
                    {
                        existencias.Add(new ExistenciaDTO()
                        {
                            AlmacenId = detalle.AlmacenId,
                            ExistenciaEnAlmacen = cantidad,
                            ExistenciaId = Guid.NewGuid(),
                            PrecioDeCosto = precioDeCosto,
                            ProductoId = productoPiezaId
                        });
                    }
                    else
                    {
                        existencia.PrecioDeCosto = ((existencia.ExistenciaEnAlmacen * precioDeCosto) + (cantidad * precioDeCosto)) / (existencia.ExistenciaEnAlmacen + cantidad);
                        existencia.ExistenciaEnAlmacen -= cantidad;
                    }
                }
            }

            foreach (var existencia in existencias)
            {
                var existe = await _contexto.Existencias
                    .Include(e => e.Almacenes)
                    .Include(e => e.Productos)
                    .FirstOrDefaultAsync(e => e.AlmacenId == existencia.AlmacenId && e.ProductoId == existencia.ProductoId);
                if (existe == null)
                {
                    _contexto.Existencias.Add(new Existencia()
                    {
                        AlmacenId = existencia.AlmacenId,
                        ExistenciaEnAlmacen = existencia.ExistenciaEnAlmacen * -1,
                        ExistenciaId = Guid.NewGuid(),
                        ProductoId = existencia.ProductoId
                    });
                }
                else
                {
                    existe.ExistenciaEnAlmacen -= existencia.ExistenciaEnAlmacen;
                }
            }

            var productos = existencias.GroupBy(e => e.ProductoId)
                .Select(e => new
                {
                    ProductoId = e.Key,
                    Existencia = e.Sum(x => x.ExistenciaEnAlmacen),
                    PrecioDeCosto = e.Sum(x => x.ExistenciaEnAlmacen * x.PrecioDeCosto) / e.Sum(x => x.ExistenciaEnAlmacen)
                }).ToList();

            foreach (var p in productos)
            {
                var producto = await _contexto.Productos.FirstOrDefaultAsync(p => p.ProductoId == p.ProductoId);

                var existenciaActual = await _contexto.Existencias
                    .Where(p => p.ProductoId == p.ProductoId)
                    .SumAsync(e => e.ExistenciaEnAlmacen);

                if (producto != null)
                {
                    producto.PrecioDeCosto = decimal.Round(((p.Existencia * p.PrecioDeCosto) + (producto.PrecioDeCosto * existenciaActual)) / (p.Existencia + existenciaActual), 4);
                    _contexto.Update(producto);
                }
            }

            try
            {
                _contexto.Update(venta);
                int afectados = await _contexto.SaveChangesAsync();
                resultado.Mensaje = "Proceso finalizado correctamente.";
            }
            catch (Exception)
            {
                resultado.Error = true;
                resultado.Mensaje = "Error al aplicar la venta.";
            }

            return resultado;
        }

        public async Task<Resultado<Venta>> AgregarAsync(Venta? datos)
        {
            Resultado<Venta> resultado = new();

            if (datos == null)
            {
                resultado.Error = true;
                resultado.Mensaje = "Datos nulos (proceso no realizado).";
                return resultado;
            }
            if (datos.RazonSocialId == Guid.Empty)
            {
                resultado.Error = true;
                resultado.Mensaje = "EL identificador de la razón social es incorrecto.";
                return resultado;
            }
            if (datos.VentaId == null || datos.VentaId == Guid.Empty)
                datos.VentaId = Guid.NewGuid();

            DateTime fechaActual = DateTime.Now;

            Venta venta = new()
            {
                VentaId = datos.VentaId,
                Aplicado = false,
                Fecha = Convert.ToDateTime(datos.Fecha),
                FechaDeActualizacion = fechaActual,
                FechaDeRegistro = fechaActual,
                Folio = datos.Folio,
                Observaciones = datos.Observaciones == null ? "" : datos.Observaciones.Trim(),
                RazonSocialId = datos.RazonSocialId,
                UsuarioId = datos.UsuarioId
            };

            try
            {
                _contexto.Add(venta);
                int afectados = await _contexto.SaveChangesAsync();
                resultado.Mensaje = "Proceso finalizado correctamente.";
                resultado.Datos = datos;
            }
            catch (Exception)
            {
                resultado.Error = true;
                resultado.Mensaje = "Error al crear el registro.";
            }

            return resultado;
        }

        public async Task<Resultado<Venta>> ActualizarAsync(Venta? datos)
        {
            Resultado<Venta> resultado = new();

            if (datos == null)
            {
                resultado.Error = true;
                resultado.Mensaje = "Datos nulos (proceso no realizado).";
                return resultado;
            }
            if (datos.VentaId == null || datos.VentaId == Guid.Empty)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la venta es incorrecto.";
                return resultado;
            }
            var venta = await _contexto.Ventas.FindAsync(datos.VentaId);
            if (venta == null)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la venta es incorrecto.";
                return resultado;
            }
            if (venta.Aplicado)
            {
                resultado.Error = true;
                resultado.Mensaje = "El movimiento de venta ya fue aplicado, no se puede editar la información.";
                return resultado;
            }

            venta.Fecha = Convert.ToDateTime(datos.Fecha);
            venta.FechaDeActualizacion = DateTime.Now;
            venta.Folio = datos.Folio;
            venta.Observaciones = datos.Observaciones == null ? "" : datos.Observaciones.Trim();
            venta.UsuarioId = datos.UsuarioId;

            try
            {
                _contexto.Update(venta);
                int afectados = await _contexto.SaveChangesAsync();
                resultado = await ObtenerRegistroAsync(datos.VentaId, venta.RazonSocialId);
            }
            catch (Exception)
            {
                resultado.Error = true;
                resultado.Mensaje = "Error al actualizar el registro.";
            }

            return resultado;
        }

        public async Task<Resultado<Venta>> EliminarAsync(Guid? id)
        {
            Resultado<Venta> resultado = new();

            if (id == null || id == Guid.Empty)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la venta es incorrecto.";
                return resultado;
            }
            var venta = await _contexto.Ventas.FindAsync(id);
            if (venta == null)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la venta es incorrecto.";
                return resultado;
            }
            if (venta.Aplicado)
            {
                resultado.Error = true;
                resultado.Mensaje = "El movimiento de venta ya fue aplicado, no se puede eliminar la información.";
                return resultado;
            }

            var detalles = await _contexto.VentasDetalles.Where(c => c.VentaId == id).ToListAsync();
            if (detalles != null && detalles.Any())
            {
                foreach (var detalle in detalles)
                {
                    _contexto.Remove(detalle);
                }
            }

            try
            {
                _contexto.Remove(venta);
                int afectados = await _contexto.SaveChangesAsync();
                resultado.Mensaje = "Proceso finalizado correctamente.";
            }
            catch (Exception)
            {
                resultado.Error = true;
                resultado.Mensaje = "Error al eliminar el registro.";
            }

            return resultado;
        }

        public async Task<Resultado<Venta>> ObtenerRegistroAsync(Guid? id, Guid? razonSocialId)
        {
            Resultado<Venta> resultado = new();
            if (id == null || id == Guid.Empty)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la venta es incorrecto.";
                return resultado;
            }
            if (razonSocialId == null || razonSocialId == Guid.Empty)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la razón social es incorrecto.";
                return resultado;
            }
            var venta = await _contexto.Ventas
                .Include(p => p.VentasDetalles!)
                .ThenInclude(p => p.Almacenes)
                .Include(p => p.VentasDetalles!)
                .ThenInclude(p => p.Productos)
                .Where(p => p.VentaId == id)
                .Select(p => new Venta()
                {
                    VentaId = p.VentaId,
                    Aplicado = p.Aplicado,
                    VentasDetalles = p.VentasDetalles,
                    Fecha = p.Fecha,
                    FechaDeActualizacion = p.FechaDeActualizacion,
                    FechaDeRegistro = p.FechaDeRegistro,
                    Folio = p.Folio,
                    Observaciones = p.Observaciones,
                    RazonSocialId = p.RazonSocialId,
                    UsuarioId = p.UsuarioId
                }).FirstOrDefaultAsync();
            if (venta == null)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la venta es incorrecto.";
                return resultado;
            }
            if (venta.RazonSocialId != razonSocialId)
            {
                resultado.Error = true;
                resultado.Mensaje = "La venta de movimiento no pertenece a la Razón Social seleccionada.";
                return resultado;
            }

            //venta = await ObtenerRegistroConDatosDDLAsync(venta);

            resultado.Datos = venta;
            resultado.Mensaje = "Proceso finalizado correctamente.";
            return resultado;
        }

        public async Task<Resultado<Filtro<List<Venta>>>> ObtenerRegistrosAsync(Filtro<List<Venta>> filtro)
        {
            Resultado<Filtro<List<Venta>>> resultado = new();

            if (filtro.RazonSocialId == Guid.Empty)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la razón social es incorrecto.";
                return resultado;
            }

            IQueryable<Venta> query = _contexto.Ventas
                .Include(e => e.VentasDetalles)
                .Where(e => e.RazonSocialId == filtro.RazonSocialId)
                .Select(e => new Venta
                {
                    VentaId = e.VentaId,
                    Aplicado = e.Aplicado,
                    Fecha = e.Fecha,
                    FechaDeActualizacion = e.FechaDeActualizacion,
                    FechaDeRegistro = e.FechaDeRegistro,
                    Folio = e.Folio,
                    Observaciones = e.Observaciones,
                    RazonSocialId = e.RazonSocialId,
                    UsuarioId = e.UsuarioId,
                    VentasDetalles = e.VentasDetalles
                });

            if (!string.IsNullOrEmpty(filtro.Patron))
            {
                string[] palabras = filtro.Patron.Trim().Split(" ");
                if (palabras.Length > 0)
                {
                    foreach (var palabra in palabras)
                    {
                        string p = palabra.Trim();
                        query = query.Where(d => d.Folio.Contains(p));
                    }
                }
            }

            filtro.TotalDeRegistros = await query.CountAsync();
            filtro.Datos = await query.AsNoTracking()
                .OrderByDescending(a => a.FechaDeActualizacion)
                .ThenBy(a => a.Folio)
                .Skip(filtro.Pagina * filtro.NumeroDeRegistrosPorLista)
                .Take(filtro.NumeroDeRegistrosPorLista)
                .ToListAsync();

            resultado.Datos = filtro;
            return resultado;
        }
    }
}