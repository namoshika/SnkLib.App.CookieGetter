namespace Sample
{
    partial class MainWindow
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.checkBoxShowAll = new System.Windows.Forms.CheckBox();
            this.gbBrowser = new System.Windows.Forms.GroupBox();
            this.nicoSessionComboBox1 = new SunokoLibrary.Windows.Forms.NicoSessionComboBox();
            this.btnReload = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.btnOpenCookieFileDialog = new System.Windows.Forms.Button();
            this.txtCookiePath = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtUserSession = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.gbBrowser.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBoxShowAll
            // 
            this.checkBoxShowAll.AutoSize = true;
            this.checkBoxShowAll.Location = new System.Drawing.Point(6, 18);
            this.checkBoxShowAll.Name = "checkBoxShowAll";
            this.checkBoxShowAll.Size = new System.Drawing.Size(151, 16);
            this.checkBoxShowAll.TabIndex = 8;
            this.checkBoxShowAll.Text = "すべてのブラウザを表示する";
            this.checkBoxShowAll.UseVisualStyleBackColor = true;
            this.checkBoxShowAll.CheckedChanged += new System.EventHandler(this.checkBoxShowAll_CheckedChanged);
            // 
            // gbBrowser
            // 
            this.gbBrowser.Controls.Add(this.nicoSessionComboBox1);
            this.gbBrowser.Controls.Add(this.checkBoxShowAll);
            this.gbBrowser.Controls.Add(this.btnReload);
            this.gbBrowser.Controls.Add(this.label3);
            this.gbBrowser.Controls.Add(this.btnOpenCookieFileDialog);
            this.gbBrowser.Controls.Add(this.txtCookiePath);
            this.gbBrowser.Location = new System.Drawing.Point(12, 12);
            this.gbBrowser.Name = "gbBrowser";
            this.gbBrowser.Size = new System.Drawing.Size(326, 105);
            this.gbBrowser.TabIndex = 10;
            this.gbBrowser.TabStop = false;
            this.gbBrowser.Text = "ブラウザ";
            // 
            // nicoSessionComboBox1
            // 
            this.nicoSessionComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nicoSessionComboBox1.FormattingEnabled = true;
            this.nicoSessionComboBox1.Location = new System.Drawing.Point(6, 42);
            this.nicoSessionComboBox1.Name = "nicoSessionComboBox1";
            this.nicoSessionComboBox1.Size = new System.Drawing.Size(247, 20);
            this.nicoSessionComboBox1.TabIndex = 9;
            // 
            // btnReload
            // 
            this.btnReload.Location = new System.Drawing.Point(259, 40);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(46, 23);
            this.btnReload.TabIndex = 1;
            this.btnReload.Text = "更新";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "取得先";
            // 
            // btnOpenCookieFileDialog
            // 
            this.btnOpenCookieFileDialog.Enabled = false;
            this.btnOpenCookieFileDialog.Location = new System.Drawing.Point(259, 70);
            this.btnOpenCookieFileDialog.Name = "btnOpenCookieFileDialog";
            this.btnOpenCookieFileDialog.Size = new System.Drawing.Size(46, 23);
            this.btnOpenCookieFileDialog.TabIndex = 3;
            this.btnOpenCookieFileDialog.Text = "参照";
            this.btnOpenCookieFileDialog.UseVisualStyleBackColor = true;
            this.btnOpenCookieFileDialog.Click += new System.EventHandler(this.btnOpenCookieFileDialog_Click);
            // 
            // txtCookiePath
            // 
            this.txtCookiePath.Location = new System.Drawing.Point(51, 72);
            this.txtCookiePath.Name = "txtCookiePath";
            this.txtCookiePath.ReadOnly = true;
            this.txtCookiePath.Size = new System.Drawing.Size(202, 19);
            this.txtCookiePath.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtUserSession);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 123);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(326, 79);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "取得結果";
            // 
            // txtUserSession
            // 
            this.txtUserSession.Location = new System.Drawing.Point(25, 40);
            this.txtUserSession.Name = "txtUserSession";
            this.txtUserSession.Size = new System.Drawing.Size(280, 19);
            this.txtUserSession.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "ユーザーセッション";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(347, 216);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbBrowser);
            this.Name = "MainWindow";
            this.Text = "CookieGetter Demo";
            this.gbBrowser.ResumeLayout(false);
            this.gbBrowser.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxShowAll;
        private System.Windows.Forms.GroupBox gbBrowser;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnOpenCookieFileDialog;
        private System.Windows.Forms.TextBox txtCookiePath;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtUserSession;
        private System.Windows.Forms.Label label1;
        private SunokoLibrary.Windows.Forms.NicoSessionComboBox nicoSessionComboBox1;
    }
}

