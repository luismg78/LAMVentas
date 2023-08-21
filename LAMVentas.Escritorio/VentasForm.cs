using LAMCatalogos.Comunes;
using LAMInventarios.Backend.Catalogo;
using LAMInventarios.Clases.DTO.Catalogo;
using LAMInventarios.Clases.Entidades.Catalogo;
using LAMInventarios.Configuraciones;
using LAMInventarios.Contextos;
using LAMVentas.Backend;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace LAMVentas.Escritorio
{
    public partial class VentasForm : Form
    {
        #region Variables globales del formulario
        private readonly Ventas _ventas;
        private readonly Productos _productos;
        private readonly Guid _razonSocialId = new Guid("E9212EB2-697A-4358-9CDE-9123B66676EB");
        private readonly Configuracion _configuracion;
        private decimal _cantidad;
        #endregion

        #region Constructor
        public VentasForm(Configuracion configuracion)
        {
            InitializeComponent();
            ContextoInventarios contexto = new(configuracion);
            _ventas = new Ventas(contexto);
            _productos = new Productos(contexto, configuracion);
            _configuracion = configuracion;
            _cantidad = 1;
        }
        #endregion

        #region Inicio y cierre del formulario
        private void VentasForm_Load(object sender, EventArgs e)
        {
            if (Global.UsuarioId == null || Global.UsuarioId == Guid.Empty)
            {
                InicioDeSesionForm form = new(_configuracion);
                Hide();
                form.Show();
            }
        }
        private void VentasForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Global.AplicacionCerrada)
            {
                var resultado = MessageBox.Show("¿Desea cerrar la aplicación?", "Cerrar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resultado == DialogResult.Yes)
                {
                    Global.AplicacionCerrada = true;
                    Application.Exit();
                }
                else
                    e.Cancel = true;
            }
        }
        #endregion

        #region Funcionalidad de código (textbox)
        private async void CodigoTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            Resultado resultado = new();
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    resultado = await ObtenerProductoAsync();
                    if (resultado.Error)
                        MensajeDeError(resultado.Mensaje);
                    break;
                case Keys.Escape:
                    CodigoTextBox.Text = string.Empty;
                    break;
                case Keys.Multiply:
                case Keys.Oemplus:
                    resultado = ValidarCantidad();
                    if (resultado.Error)
                        MensajeDeError(resultado.Mensaje);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.F5:
                    MessageBox.Show("F5");
                    break;
                case Keys.F7:
                    MessageBox.Show("F7");
                    break;
            }
        }
        #endregion

        #region Funcionalidad del datagrid
        private void ProductosDataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            DataGridView row = (DataGridView)sender;
            int i = row.CurrentRow.Index;
            decimal cantidad = (decimal)ProductosDataGridView.Rows[i].Cells[0].Value * -1;
            string pregunta = "¿Desea eliminar el detalle del producto?";
            Color color = Color.Red;
            if(cantidad > 0)
            {
                color = Color.Black;
                pregunta = "¿Desea restablecer el detalle del producto?";
            }

            if(MessageBox.Show(pregunta, "Ventas", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                ProductosDataGridView.Rows[i].Cells[0].Value =  cantidad;
                ProductosDataGridView.Rows[i].Cells[4].Value = (decimal)ProductosDataGridView.Rows[i].Cells[3].Value * cantidad;
                ProductosDataGridView.Rows[i].DefaultCellStyle.ForeColor = color;
                ObtenerTotal();
            }
        }
        #endregion

        #region Helpers
        private async Task<Resultado> ObtenerProductoAsync()
        {
            Resultado resultado = new();

            var resultadoProducto = await _productos.ObtenerRegistroPorCodigoAsync(CodigoTextBox.Text.Trim(), _razonSocialId);
            if (resultadoProducto.Error)
            {
                resultado.Error = true;
                resultado.Mensaje = resultadoProducto.Mensaje;
                return resultado;
            }

            if (resultadoProducto.Datos == null)
            {
                resultado.Error = true;
                resultado.Mensaje = "El código del producto es incorrecto.";
                return resultado;
            }

            var producto = resultadoProducto.Datos;
            ProductosDataGridView.Rows.Add(_cantidad, producto.Codigo, producto.Nombre, producto.PrecioDeVenta, producto.PrecioDeVenta * _cantidad);
            ProductosDataGridView.Rows[ProductosDataGridView.Rows.Count - 1].Selected = true;
            ProductosDataGridView.FirstDisplayedScrollingRowIndex = ProductosDataGridView.Rows.Count - 1;
            //si la cantidad es negativa, establecer el texto en color rojo.
            if (_cantidad < 0)
            {
                ProductosDataGridView.Rows[ProductosDataGridView.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.Red;
            }

            CodigoTextBox.Text = string.Empty;
            _cantidad = 1;
            ObtenerTotal();

            return resultado;
        }
        #endregion

        #region Importes totales de la venta
        private decimal CalcularTotal()
        {
            decimal total = 0;
            for (var i = 0; i < ProductosDataGridView.Rows.Count; i++)
            {
                total += Convert.ToDecimal(ProductosDataGridView.Rows[i].Cells[4].Value);
            }

            return total;
        }
        private void ObtenerTotal()
        {
            decimal total = CalcularTotal();
            if (total > 0)
            {
                TotalLabel.Text = $"Total {total:$0.00}";
            }
            else
            {
                TotalLabel.Text = "Total $0.00";
            }
        }
        #endregion

        #region Mensajes
        private static void MensajeDeError(string mensaje)
        {
            MessageBox.Show(mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion

        #region Validaciones
        private Resultado ValidarCantidad()
        {
            Resultado resultado = new();
            _cantidad = 1;

            if (string.IsNullOrEmpty(CodigoTextBox.Text))
            {
                return resultado;
            }

            if (!Regex.IsMatch(CodigoTextBox.Text, @"^-?\d+(?:\.\d+)?$"))
            {
                resultado.Error = true;
                resultado.Mensaje = $"Cantidad ({CodigoTextBox.Text}) incorrecta";
            }

            _cantidad = Convert.ToDecimal(CodigoTextBox.Text);

            if (_cantidad == 0)
            {
                resultado.Error = true;
                resultado.Mensaje = $"La Cantidad no puede ser cero.";
            }

            CodigoTextBox.Text = string.Empty;
            return resultado;
        }
        #endregion        
    }
}
