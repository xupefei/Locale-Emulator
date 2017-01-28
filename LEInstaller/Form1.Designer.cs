namespace LEInstaller
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.buttonInstall = new System.Windows.Forms.Button();
            this.buttonUninstall = new System.Windows.Forms.Button();
            this.buttonUninstallAllUsers = new System.Windows.Forms.Button();
            this.buttonInstallAllUsers = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonInstall
            // 
            this.buttonInstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonInstall.Location = new System.Drawing.Point(30, 33);
            this.buttonInstall.Margin = new System.Windows.Forms.Padding(6);
            this.buttonInstall.Name = "buttonInstall";
            this.buttonInstall.Size = new System.Drawing.Size(329, 49);
            this.buttonInstall.TabIndex = 0;
            this.buttonInstall.Text = "Install for current user";
            this.buttonInstall.UseVisualStyleBackColor = true;
            this.buttonInstall.Click += new System.EventHandler(this.buttonInstall_Click);
            // 
            // buttonUninstall
            // 
            this.buttonUninstall.Location = new System.Drawing.Point(371, 33);
            this.buttonUninstall.Margin = new System.Windows.Forms.Padding(6);
            this.buttonUninstall.Name = "buttonUninstall";
            this.buttonUninstall.Size = new System.Drawing.Size(329, 49);
            this.buttonUninstall.TabIndex = 1;
            this.buttonUninstall.Text = "Uninstall for current user";
            this.buttonUninstall.UseVisualStyleBackColor = true;
            this.buttonUninstall.Click += new System.EventHandler(this.buttonUninstall_Click);
            // 
            // buttonUninstallAllUsers
            // 
            this.buttonUninstallAllUsers.Location = new System.Drawing.Point(371, 33);
            this.buttonUninstallAllUsers.Margin = new System.Windows.Forms.Padding(6);
            this.buttonUninstallAllUsers.Name = "buttonUninstallAllUsers";
            this.buttonUninstallAllUsers.Size = new System.Drawing.Size(329, 49);
            this.buttonUninstallAllUsers.TabIndex = 3;
            this.buttonUninstallAllUsers.Text = "Uninstall for all users";
            this.buttonUninstallAllUsers.UseVisualStyleBackColor = true;
            this.buttonUninstallAllUsers.Click += new System.EventHandler(this.buttonUninstallAllUsers_Click);
            // 
            // buttonInstallAllUsers
            // 
            this.buttonInstallAllUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonInstallAllUsers.Location = new System.Drawing.Point(30, 33);
            this.buttonInstallAllUsers.Margin = new System.Windows.Forms.Padding(6);
            this.buttonInstallAllUsers.Name = "buttonInstallAllUsers";
            this.buttonInstallAllUsers.Size = new System.Drawing.Size(329, 49);
            this.buttonInstallAllUsers.TabIndex = 2;
            this.buttonInstallAllUsers.Text = "Install for all users";
            this.buttonInstallAllUsers.UseVisualStyleBackColor = true;
            this.buttonInstallAllUsers.Click += new System.EventHandler(this.buttonInstallAllUsers_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(15, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(726, 184);
            this.label1.TabIndex = 4;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonInstall);
            this.groupBox1.Controls.Add(this.buttonUninstall);
            this.groupBox1.Location = new System.Drawing.Point(12, 206);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(729, 100);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "For current user";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonInstallAllUsers);
            this.groupBox2.Controls.Add(this.buttonUninstallAllUsers);
            this.groupBox2.Location = new System.Drawing.Point(12, 330);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(729, 100);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "For all users (requires admin)";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(753, 446);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LE Context Menu Installer";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonInstall;
        private System.Windows.Forms.Button buttonUninstall;
        private System.Windows.Forms.Button buttonUninstallAllUsers;
        private System.Windows.Forms.Button buttonInstallAllUsers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}

