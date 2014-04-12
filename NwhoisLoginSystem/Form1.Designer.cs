namespace NwhoisLoginSystem
{
	partial class Form1
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
			this.cbShowAll = new System.Windows.Forms.CheckBox();
			this.gbBrowser = new System.Windows.Forms.GroupBox();
			this.cmbBrowser = new System.Windows.Forms.ComboBox();
			this.btnCheck = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.btnRef = new System.Windows.Forms.Button();
			this.txtPath = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.txtUserSession = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.gbBrowser.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// cbShowAll
			// 
			this.cbShowAll.AutoSize = true;
			this.cbShowAll.Location = new System.Drawing.Point(6, 18);
			this.cbShowAll.Name = "cbShowAll";
			this.cbShowAll.Size = new System.Drawing.Size(151, 16);
			this.cbShowAll.TabIndex = 8;
			this.cbShowAll.Text = "すべてのブラウザを表示する";
			this.cbShowAll.UseVisualStyleBackColor = true;
			this.cbShowAll.CheckedChanged += new System.EventHandler(this.cbShowAll_CheckedChanged);
			// 
			// gbBrowser
			// 
			this.gbBrowser.Controls.Add(this.cbShowAll);
			this.gbBrowser.Controls.Add(this.cmbBrowser);
			this.gbBrowser.Controls.Add(this.btnCheck);
			this.gbBrowser.Controls.Add(this.label3);
			this.gbBrowser.Controls.Add(this.btnRef);
			this.gbBrowser.Controls.Add(this.txtPath);
			this.gbBrowser.Location = new System.Drawing.Point(12, 12);
			this.gbBrowser.Name = "gbBrowser";
			this.gbBrowser.Size = new System.Drawing.Size(326, 105);
			this.gbBrowser.TabIndex = 10;
			this.gbBrowser.TabStop = false;
			this.gbBrowser.Text = "ブラウザ";
			// 
			// cmbBrowser
			// 
			this.cmbBrowser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbBrowser.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cmbBrowser.FormattingEnabled = true;
			this.cmbBrowser.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.cmbBrowser.Location = new System.Drawing.Point(6, 41);
			this.cmbBrowser.Name = "cmbBrowser";
			this.cmbBrowser.Size = new System.Drawing.Size(247, 20);
			this.cmbBrowser.TabIndex = 0;
			this.cmbBrowser.SelectedIndexChanged += new System.EventHandler(this.cmbBrowser_SelectedIndexChanged);
			// 
			// btnCheck
			// 
			this.btnCheck.Location = new System.Drawing.Point(259, 40);
			this.btnCheck.Name = "btnCheck";
			this.btnCheck.Size = new System.Drawing.Size(46, 23);
			this.btnCheck.TabIndex = 1;
			this.btnCheck.Text = "更新";
			this.btnCheck.UseVisualStyleBackColor = true;
			this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
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
			// btnRef
			// 
			this.btnRef.Enabled = false;
			this.btnRef.Location = new System.Drawing.Point(259, 70);
			this.btnRef.Name = "btnRef";
			this.btnRef.Size = new System.Drawing.Size(46, 23);
			this.btnRef.TabIndex = 3;
			this.btnRef.Text = "参照";
			this.btnRef.UseVisualStyleBackColor = true;
			this.btnRef.Click += new System.EventHandler(this.btnRef_Click);
			// 
			// txtPath
			// 
			this.txtPath.Enabled = false;
			this.txtPath.Location = new System.Drawing.Point(51, 72);
			this.txtPath.Name = "txtPath";
			this.txtPath.ReadOnly = true;
			this.txtPath.Size = new System.Drawing.Size(202, 19);
			this.txtPath.TabIndex = 2;
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
			// backgroundWorker1
			// 
			this.backgroundWorker1.WorkerReportsProgress = true;
			this.backgroundWorker1.WorkerSupportsCancellation = true;
			this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
			this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
			this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			this.openFileDialog1.Title = "クッキーが保存されているファイルを指定してください。";
			// 
			// folderBrowserDialog1
			// 
			this.folderBrowserDialog1.Description = "クッキーが保存されているフォルダを指定してください。";
			this.folderBrowserDialog1.ShowNewFolderButton = false;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(347, 216);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.gbBrowser);
			this.Name = "Form1";
			this.Text = "Form1";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.gbBrowser.ResumeLayout(false);
			this.gbBrowser.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.CheckBox cbShowAll;
		private System.Windows.Forms.GroupBox gbBrowser;
		private System.Windows.Forms.ComboBox cmbBrowser;
		private System.Windows.Forms.Button btnCheck;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btnRef;
		private System.Windows.Forms.TextBox txtPath;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox txtUserSession;
		private System.Windows.Forms.Label label1;
		private System.ComponentModel.BackgroundWorker backgroundWorker1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;

	}
}

