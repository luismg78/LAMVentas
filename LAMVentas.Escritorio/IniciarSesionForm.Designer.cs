namespace LAMVentas.Escritorio
{
    partial class InicioDeSesionForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CorreoElectronicoLabel = new System.Windows.Forms.Label();
            this.IniciarSesionButton = new System.Windows.Forms.Button();
            this.CorreoElectronicoTextBox = new System.Windows.Forms.TextBox();
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.PasswordLabel = new System.Windows.Forms.Label();
            this.SalirButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CorreoElectronicoLabel
            // 
            this.CorreoElectronicoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CorreoElectronicoLabel.AutoSize = true;
            this.CorreoElectronicoLabel.Location = new System.Drawing.Point(39, 85);
            this.CorreoElectronicoLabel.Name = "CorreoElectronicoLabel";
            this.CorreoElectronicoLabel.Size = new System.Drawing.Size(132, 20);
            this.CorreoElectronicoLabel.TabIndex = 0;
            this.CorreoElectronicoLabel.Text = "Correo Electrónico";
            // 
            // IniciarSesionButton
            // 
            this.IniciarSesionButton.Location = new System.Drawing.Point(184, 281);
            this.IniciarSesionButton.Name = "IniciarSesionButton";
            this.IniciarSesionButton.Size = new System.Drawing.Size(127, 29);
            this.IniciarSesionButton.TabIndex = 3;
            this.IniciarSesionButton.Text = "Iniciar Sesión";
            this.IniciarSesionButton.UseVisualStyleBackColor = true;
            this.IniciarSesionButton.Click += new System.EventHandler(this.IniciarSesionButton_Click);
            // 
            // CorreoElectronicoTextBox
            // 
            this.CorreoElectronicoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CorreoElectronicoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
            this.CorreoElectronicoTextBox.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CorreoElectronicoTextBox.Location = new System.Drawing.Point(39, 109);
            this.CorreoElectronicoTextBox.MaxLength = 200;
            this.CorreoElectronicoTextBox.Name = "CorreoElectronicoTextBox";
            this.CorreoElectronicoTextBox.Size = new System.Drawing.Size(268, 30);
            this.CorreoElectronicoTextBox.TabIndex = 1;
            this.CorreoElectronicoTextBox.Text = "luismg78@gmail.com";
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PasswordTextBox.Location = new System.Drawing.Point(39, 184);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.PasswordChar = '*';
            this.PasswordTextBox.Size = new System.Drawing.Size(268, 27);
            this.PasswordTextBox.TabIndex = 2;
            this.PasswordTextBox.Text = "12345";
            // 
            // PasswordLabel
            // 
            this.PasswordLabel.AutoSize = true;
            this.PasswordLabel.Location = new System.Drawing.Point(39, 158);
            this.PasswordLabel.Name = "PasswordLabel";
            this.PasswordLabel.Size = new System.Drawing.Size(83, 20);
            this.PasswordLabel.TabIndex = 3;
            this.PasswordLabel.Text = "Contraseña";
            // 
            // SalirButton
            // 
            this.SalirButton.Location = new System.Drawing.Point(39, 281);
            this.SalirButton.Name = "SalirButton";
            this.SalirButton.Size = new System.Drawing.Size(127, 29);
            this.SalirButton.TabIndex = 4;
            this.SalirButton.Text = "Salir";
            this.SalirButton.UseVisualStyleBackColor = true;
            this.SalirButton.Click += new System.EventHandler(this.SalirButton_Click);
            // 
            // InicioDeSesionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.SalirButton;
            this.ClientSize = new System.Drawing.Size(350, 363);
            this.ControlBox = false;
            this.Controls.Add(this.SalirButton);
            this.Controls.Add(this.PasswordTextBox);
            this.Controls.Add(this.PasswordLabel);
            this.Controls.Add(this.CorreoElectronicoTextBox);
            this.Controls.Add(this.IniciarSesionButton);
            this.Controls.Add(this.CorreoElectronicoLabel);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(368, 410);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(368, 410);
            this.Name = "InicioDeSesionForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Iniciar Sesión";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label CorreoElectronicoLabel;
        private Button IniciarSesionButton;
        private TextBox CorreoElectronicoTextBox;
        private TextBox PasswordTextBox;
        private Label PasswordLabel;
        private Button SalirButton;
    }
}