namespace FunPayAlert.Forms
{
    partial class SetupForm_One
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
            this.buttonAuthFP = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonAuthFP
            // 
            this.buttonAuthFP.Location = new System.Drawing.Point(29, 57);
            this.buttonAuthFP.Name = "buttonAuthFP";
            this.buttonAuthFP.Size = new System.Drawing.Size(169, 23);
            this.buttonAuthFP.TabIndex = 0;
            this.buttonAuthFP.Text = "Авторизоваться на FunPay";
            this.buttonAuthFP.UseVisualStyleBackColor = true;
            this.buttonAuthFP.Click += new System.EventHandler(this.buttonAuthFP_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(172, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Шаг 1. Авторизуйтесь на FunPay";
            // 
            // StartForm_One
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(220, 108);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonAuthFP);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::FunPayAlert.Properties.Resources.favicon;
            this.MaximizeBox = false;
            this.Name = "StartForm_One";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FPAlert";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StartForm_One_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonAuthFP;
        private System.Windows.Forms.Label label1;
    }
}