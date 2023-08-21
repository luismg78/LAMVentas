using LAMInventarios.Configuraciones;
using System.Text.RegularExpressions;

namespace LAMVentas.Escritorio
{
    public partial class InicioDeSesionForm : Form
    {
        ErrorProvider _error;
        private readonly Configuracion _configuracion;

        public InicioDeSesionForm(Configuracion configuracion)
        {
            InitializeComponent();
            _error = new ErrorProvider();
            _configuracion = configuracion;
        }

        private void IniciarSesionButton_Click(object sender, EventArgs e)
        {
            var ok = ValidarCampos(CorreoElectronicoTextBox.Text, PasswordTextBox.Text);
            if (ok)
            {
                Global.UsuarioId = Guid.NewGuid();
                Global.AplicacionCerrada = false;
                VentasForm form = new(_configuracion);
                form.Show();
                this.Close();
            }
        }

        private bool ValidarCampos(string correoElectronico, string password)
        {
            bool valid = true;
            _error.Clear();
            if (string.IsNullOrEmpty(correoElectronico))
            {
                _error.SetError(CorreoElectronicoTextBox, "valor requerido");
                valid = false;
            }
            if (string.IsNullOrEmpty(password))
            {
                _error.SetError(PasswordTextBox, "valor requerido");
                valid = false;
            }
            if (valid)
            {
                if (!Regex.IsMatch(correoElectronico, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
                {
                    _error.SetError(CorreoElectronicoTextBox, "formato incorrecto");
                    valid = false;
                }
            }
            return valid;
        }

        private void SalirButton_Click(object sender, EventArgs e)
        {
            var resultado = MessageBox.Show("¿Desea cerrar la aplicación?", "Cerrar", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (resultado == DialogResult.OK)
            {
                Global.AplicacionCerrada = true;
                Application.Exit();
            }
        }
    }
}