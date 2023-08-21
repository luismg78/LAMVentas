using LAMCatalogos.Comunes;
using LAMInventarios.Backend.Catalogo;
using LAMInventarios.Clases.DTO.Movimiento;
using LAMInventarios.Clases.Entidades.Movimiento;
using LAMInventarios.Configuraciones;
using LAMInventarios.Contextos;
using Microsoft.EntityFrameworkCore;

namespace LAMVentas.Backend
{
    public class VentasDetalles
    {
        private readonly ContextoInventarios _contexto;
        private readonly Productos _producto;
        private readonly Almacenes _almacen;

        public VentasDetalles(ContextoInventarios contexto, Configuracion configuracion)
        {
            _contexto = contexto;
            _almacen = new Almacenes(contexto);
            _producto = new Productos(contexto, configuracion);
        }

        public async Task<Resultado<VentaDetalleDTO>> AgregarAsync(VentaDetalleDTO? datos)
        {
            Resultado<VentaDetalleDTO> resultado = new();

            if (datos == null)
            {
                resultado.Error = true;
                resultado.Mensaje = "Datos nulos (proceso no realizado).";
                return resultado;
            }

            var resultadoDeProducto = await _producto.ObtenerRegistroPorCodigoAsync(datos.Codigo, datos.RazonSocialId);
            if (resultadoDeProducto.Error)
            {
                resultado.Error = true;
                resultado.Mensaje = resultadoDeProducto.Mensaje;
                return resultado;
            }
            var producto = resultadoDeProducto.Datos;

            var resultadoDeAlmacen = await _almacen.ObtenerRegistroAsync(datos.AlmacenId);
            if (resultadoDeAlmacen.Error)
            {
                resultado.Error = true;
                resultado.Mensaje = resultadoDeAlmacen.Mensaje;
                return resultado;
            }
            var almacen = resultadoDeAlmacen.Datos;

            if (datos.VentaDetalleId == null || datos.VentaDetalleId == Guid.Empty)
                datos.VentaDetalleId = Guid.NewGuid();

            VentaDetalle ventaDetalle = new()
            {
                VentaDetalleId = datos.VentaDetalleId,
                VentaId = datos.VentaId,
                AlmacenId = datos.AlmacenId,
                Cantidad = datos.Cantidad,
                ProductoId = producto!.ProductoId,
                PrecioDeCosto = datos.PrecioDeCosto,
            };

            try
            {
                _contexto.Add(ventaDetalle);
                int afectados = await _contexto.SaveChangesAsync();
                resultado.Mensaje = "Proceso finalizado correctamente.";
                datos.Productos = producto;
                datos.Almacenes = almacen;
                resultado.Datos = datos;
            }
            catch (Exception)
            {
                resultado.Error = true;
                resultado.Mensaje = "Error al crear el registro.";
            }

            return resultado;
        }

        public async Task<Resultado<VentaDetalleDTO>> ActualizarAsync(VentaDetalleDTO? datos)
        {
            Resultado<VentaDetalleDTO> resultado = new();

            if (datos == null)
            {
                resultado.Error = true;
                resultado.Mensaje = "Datos nulos (proceso no realizado).";
                return resultado;
            }
            if (datos.VentaDetalleId == null || datos.VentaDetalleId == Guid.Empty)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la venta (detalle) es incorrecto.";
                return resultado;
            }
            var ventaDetalle = await _contexto.VentasDetalles.FindAsync(datos.VentaDetalleId);
            if (ventaDetalle == null)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la venta (detalle) es incorrecto.";
                return resultado;
            }
            var venta = await _contexto.Ventas.FindAsync(ventaDetalle.VentaId);
            if (venta == null)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la venta es incorrecto.";
                return resultado;
            }
            var resultadoDeProducto = await _producto.ObtenerRegistroPorCodigoAsync(datos.Codigo, venta.RazonSocialId);
            if (resultadoDeProducto.Error)
            {
                resultado.Error = true;
                resultado.Mensaje = resultadoDeProducto.Mensaje;
                return resultado;
            }
            var producto = resultadoDeProducto.Datos;

            var resultadoDeAlmacen = await _almacen.ObtenerRegistroAsync(datos.AlmacenId);
            if (resultadoDeAlmacen.Error)
            {
                resultado.Error = true;
                resultado.Mensaje = resultadoDeAlmacen.Mensaje;
                return resultado;
            }
            var almacen = resultadoDeAlmacen.Datos;

            ventaDetalle.AlmacenId = datos.AlmacenId;
            ventaDetalle.ProductoId = producto!.ProductoId;
            ventaDetalle.Cantidad = datos.Cantidad;
            ventaDetalle.PrecioDeCosto = datos.PrecioDeCosto;

            try
            {
                _contexto.Update(ventaDetalle);
                int afectados = await _contexto.SaveChangesAsync();
                resultado.Mensaje = "Proceso finalizado correctamente.";
                datos.Productos = producto;
                datos.Almacenes = almacen;
                resultado.Datos = datos;
            }
            catch (Exception)
            {
                resultado.Error = true;
                resultado.Mensaje = "Error al actualizar el registro.";
            }

            return resultado;
        }

        public async Task<Resultado<VentaDetalleDTO>> EliminarAsync(Guid? id)
        {
            Resultado<VentaDetalleDTO> resultado = new();

            if (id == null || id == Guid.Empty)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la venta (detalle) es incorrecto.";
                return resultado;
            }
            var ventaDetalle = await _contexto.VentasDetalles.FindAsync(id);
            if (ventaDetalle == null)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la venta (detalle) es incorrecto.";
                return resultado;
            }

            try
            {
                _contexto.Remove(ventaDetalle);
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

        public async Task<Resultado<VentaDetalleDTO>> ObtenerRegistroAsync(Guid? id)
        {
            Resultado<VentaDetalleDTO> resultado = new();
            if (id == null || id == Guid.Empty)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la venta (detalle) es incorrecto.";
                return resultado;
            }
            var ventaDetalle = await _contexto.VentasDetalles
                .Include(p => p.Ventas)
                .Include(p => p.Productos)
                .Where(p => p.VentaDetalleId == id)
                .Select(p => new VentaDetalleDTO()
                {
                    VentaDetalleId = p.VentaDetalleId,
                    VentaId = p.VentaId,
                    AlmacenId = p.AlmacenId,
                    Codigo = p.Productos!.Codigo,
                    Cantidad = p.Cantidad,
                    Nombre = p.Productos!.Nombre,
                    PrecioDeCosto = p.PrecioDeCosto,
                    ProductoId = p.ProductoId,
                    Productos = p.Productos,
                    RazonSocialId = p.Ventas!.RazonSocialId
                }).FirstOrDefaultAsync();

            if (ventaDetalle == null)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la venta (detalle) es incorrecto.";
                return resultado;
            }

            resultado.Datos = ventaDetalle;
            return resultado;
        }

        public async Task<Resultado<Filtro<List<VentaDetalle>>>> ObtenerRegistrosAsync(Filtro<List<VentaDetalle>> filtro)
        {
            Resultado<Filtro<List<VentaDetalle>>> resultado = new();

            if (filtro.RazonSocialId == Guid.Empty)
            {
                resultado.Error = true;
                resultado.Mensaje = "El identificador de la razón social es incorrecto.";
                return resultado;
            }

            IQueryable<VentaDetalle> query = _contexto.VentasDetalles
                .Include(e => e.Productos)
                .Where(e => e.VentaId == filtro.GuidId)
                .Select(e => new VentaDetalle
                {
                    VentaDetalleId = e.VentaDetalleId,
                    VentaId = e.VentaId,
                    AlmacenId = e.AlmacenId,
                    Cantidad = e.Cantidad,
                    PrecioDeCosto = e.PrecioDeCosto,
                    ProductoId = e.ProductoId,
                    Productos = e.Productos,
                });

            if (!string.IsNullOrEmpty(filtro.Patron))
            {
                string[] palabras = filtro.Patron.Trim().Split(" ");
                if (palabras.Length > 0)
                {
                    foreach (var palabra in palabras)
                    {
                        string p = palabra.Trim();
                        query = query.Where(d => d.Productos!.Codigo.Contains(p)
                                              || d.Productos!.Nombre.Contains(p));
                    }
                }
            }

            filtro.TotalDeRegistros = await query.CountAsync();
            filtro.Datos = await query.AsNoTracking()
                .OrderByDescending(a => a.Productos!.Nombre)
                .Skip(filtro.Pagina * filtro.NumeroDeRegistrosPorLista)
                .Take(filtro.NumeroDeRegistrosPorLista)
                .ToListAsync();

            resultado.Datos = filtro;
            return resultado;
        }
    }
}