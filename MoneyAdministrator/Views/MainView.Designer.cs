﻿namespace MyMoneyAdmin
{
    partial class MainView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainView));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this._tsddFile = new System.Windows.Forms.ToolStripDropDownButton();
            this._tsbFileNew = new System.Windows.Forms.ToolStripMenuItem();
            this._tsbFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._tsbFileClose = new System.Windows.Forms.ToolStripMenuItem();
            this._pnlAside = new System.Windows.Forms.Panel();
            this._btnCreditCards = new System.Windows.Forms.Button();
            this._btnDashboard = new System.Windows.Forms.Button();
            this._btnTransactions = new System.Windows.Forms.Button();
            this._pnlContainer = new System.Windows.Forms.Panel();
            this._pnlBackground = new System.Windows.Forms.Panel();
            this.toolStrip.SuspendLayout();
            this._pnlAside.SuspendLayout();
            this._pnlBackground.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.BackColor = System.Drawing.Color.White;
            this.toolStrip.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._tsddFile});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip.Size = new System.Drawing.Size(1051, 27);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip1";
            // 
            // _tsddFile
            // 
            this._tsddFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._tsddFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._tsbFileNew,
            this._tsbFileOpen,
            this.toolStripSeparator1,
            this._tsbFileClose});
            this._tsddFile.Image = global::MoneyAdministrator.Properties.Resources.documents;
            this._tsddFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._tsddFile.Name = "_tsddFile";
            this._tsddFile.Size = new System.Drawing.Size(72, 24);
            this._tsddFile.Text = "Archivo";
            // 
            // _tsbFileNew
            // 
            this._tsbFileNew.Image = global::MoneyAdministrator.Properties.Resources.document_plain_new_shadow;
            this._tsbFileNew.Name = "_tsbFileNew";
            this._tsbFileNew.Size = new System.Drawing.Size(182, 24);
            this._tsbFileNew.Text = "Nuevo archivo...";
            // 
            // _tsbFileOpen
            // 
            this._tsbFileOpen.Image = global::MoneyAdministrator.Properties.Resources.folder_document_shadow;
            this._tsbFileOpen.Name = "_tsbFileOpen";
            this._tsbFileOpen.Size = new System.Drawing.Size(182, 24);
            this._tsbFileOpen.Text = "Abrir archivo...";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(179, 6);
            // 
            // _tsbFileClose
            // 
            this._tsbFileClose.Image = global::MoneyAdministrator.Properties.Resources.document_forbidden_shadow;
            this._tsbFileClose.Name = "_tsbFileClose";
            this._tsbFileClose.Size = new System.Drawing.Size(182, 24);
            this._tsbFileClose.Text = "Cerrar archivo";
            // 
            // _pnlAside
            // 
            this._pnlAside.BackColor = System.Drawing.Color.White;
            this._pnlAside.Controls.Add(this._btnCreditCards);
            this._pnlAside.Controls.Add(this._btnDashboard);
            this._pnlAside.Controls.Add(this._btnTransactions);
            this._pnlAside.Dock = System.Windows.Forms.DockStyle.Left;
            this._pnlAside.Location = new System.Drawing.Point(0, 27);
            this._pnlAside.Name = "_pnlAside";
            this._pnlAside.Size = new System.Drawing.Size(200, 486);
            this._pnlAside.TabIndex = 1;
            // 
            // _btnCreditCards
            // 
            this._btnCreditCards.FlatAppearance.BorderSize = 0;
            this._btnCreditCards.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCreditCards.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this._btnCreditCards.Location = new System.Drawing.Point(0, 80);
            this._btnCreditCards.Name = "_btnCreditCards";
            this._btnCreditCards.Size = new System.Drawing.Size(200, 40);
            this._btnCreditCards.TabIndex = 2;
            this._btnCreditCards.Text = "Tarjetas de crédito";
            this._btnCreditCards.UseVisualStyleBackColor = true;
            // 
            // _btnDashboard
            // 
            this._btnDashboard.FlatAppearance.BorderSize = 0;
            this._btnDashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnDashboard.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this._btnDashboard.Location = new System.Drawing.Point(0, 0);
            this._btnDashboard.Name = "_btnDashboard";
            this._btnDashboard.Size = new System.Drawing.Size(200, 40);
            this._btnDashboard.TabIndex = 1;
            this._btnDashboard.Text = "Panel de resumen";
            this._btnDashboard.UseVisualStyleBackColor = true;
            // 
            // _btnTransactions
            // 
            this._btnTransactions.FlatAppearance.BorderSize = 0;
            this._btnTransactions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnTransactions.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this._btnTransactions.Location = new System.Drawing.Point(0, 40);
            this._btnTransactions.Name = "_btnTransactions";
            this._btnTransactions.Size = new System.Drawing.Size(200, 40);
            this._btnTransactions.TabIndex = 0;
            this._btnTransactions.Text = "Transacciones Mensuales";
            this._btnTransactions.UseVisualStyleBackColor = true;
            // 
            // _pnlContainer
            // 
            this._pnlContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._pnlContainer.BackColor = System.Drawing.Color.White;
            this._pnlContainer.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this._pnlContainer.Location = new System.Drawing.Point(5, 5);
            this._pnlContainer.Name = "_pnlContainer";
            this._pnlContainer.Size = new System.Drawing.Size(841, 476);
            this._pnlContainer.TabIndex = 2;
            // 
            // _pnlBackground
            // 
            this._pnlBackground.BackColor = System.Drawing.SystemColors.ControlDark;
            this._pnlBackground.Controls.Add(this._pnlContainer);
            this._pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlBackground.Location = new System.Drawing.Point(200, 27);
            this._pnlBackground.Name = "_pnlBackground";
            this._pnlBackground.Size = new System.Drawing.Size(851, 486);
            this._pnlBackground.TabIndex = 3;
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1051, 513);
            this.Controls.Add(this._pnlBackground);
            this.Controls.Add(this._pnlAside);
            this.Controls.Add(this.toolStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MyMoneyAdmin";
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this._pnlAside.ResumeLayout(false);
            this._pnlBackground.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ToolStrip toolStrip;
        private ToolStripDropDownButton _tsddFile;
        private ToolStripMenuItem _tsbFileNew;
        private ToolStripMenuItem _tsbFileOpen;
        private ToolStripMenuItem _tsbFileClose;
        private ToolStripSeparator toolStripSeparator1;
        private Panel _pnlAside;
        private Button _btnTransactions;
        private Panel _pnlContainer;
        private Button _btnCreditCards;
        private Button _btnDashboard;
        private Panel _pnlBackground;
    }
}