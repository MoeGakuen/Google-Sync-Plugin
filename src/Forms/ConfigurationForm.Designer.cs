﻿namespace GoogleSyncPlugin
{
	partial class ConfigurationForm
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
			this.txtClientId = new System.Windows.Forms.TextBox();
			this.lnkGoogle = new System.Windows.Forms.LinkLabel();
			this.lblTitle = new System.Windows.Forms.Label();
			this.txtClientSecret = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.btnOk = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.btnCancel = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.txtUuid = new System.Windows.Forms.TextBox();
			this.cbAccount = new System.Windows.Forms.ComboBox();
			this.lnkHelp = new System.Windows.Forms.LinkLabel();
			this.lblVersion = new System.Windows.Forms.Label();
			this.lnkHome = new System.Windows.Forms.LinkLabel();
			this.label1 = new System.Windows.Forms.Label();
			this.cbAutoSync = new System.Windows.Forms.ComboBox();
			this.chkOAuth = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// txtClientId
			// 
			this.txtClientId.Location = new System.Drawing.Point(112, 102);
			this.txtClientId.Name = "txtClientId";
			this.txtClientId.Size = new System.Drawing.Size(340, 20);
			this.txtClientId.TabIndex = 7;
			// 
			// lnkGoogle
			// 
			this.lnkGoogle.AutoSize = true;
			this.lnkGoogle.Location = new System.Drawing.Point(88, 207);
			this.lnkGoogle.Name = "lnkGoogle";
			this.lnkGoogle.Size = new System.Drawing.Size(134, 13);
			this.lnkGoogle.TabIndex = 17;
			this.lnkGoogle.TabStop = true;
			this.lnkGoogle.Text = "Google 开发者控制台";
			this.lnkGoogle.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGoogle_LinkClicked);
			// 
			// lblTitle
			// 
			this.lblTitle.AutoSize = true;
			this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(12, 13);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(239, 16);
			this.lblTitle.TabIndex = 0;
			this.lblTitle.Text = "Google Drive 同步设置";
			// 
			// txtClientSecret
			// 
			this.txtClientSecret.Location = new System.Drawing.Point(112, 128);
			this.txtClientSecret.Name = "txtClientSecret";
			this.txtClientSecret.Size = new System.Drawing.Size(340, 20);
			this.txtClientSecret.TabIndex = 9;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 105);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(50, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "客户端 ID：";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 131);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(70, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "客户端密钥：";
			// 
			// btnOk
			// 
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Location = new System.Drawing.Point(296, 202);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 13;
			this.btnOk.Text = "&确定";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 52);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(87, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "Google 账户：";
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(377, 202);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 14;
			this.btnCancel.Text = "&取消";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(12, 79);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(82, 13);
			this.label5.TabIndex = 4;
			this.label5.Text = "KeePass UUID:";
			// 
			// txtUuid
			// 
			this.txtUuid.Location = new System.Drawing.Point(112, 76);
			this.txtUuid.Name = "txtUuid";
			this.txtUuid.Size = new System.Drawing.Size(340, 20);
			this.txtUuid.TabIndex = 5;
			// 
			// cbAccount
			// 
			this.cbAccount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbAccount.FormattingEnabled = true;
			this.cbAccount.Location = new System.Drawing.Point(112, 49);
			this.cbAccount.Name = "cbAccount";
			this.cbAccount.Size = new System.Drawing.Size(340, 21);
			this.cbAccount.TabIndex = 3;
			this.cbAccount.SelectedIndexChanged += new System.EventHandler(this.cbAccount_SelectedIndexChanged);
			// 
			// lnkHelp
			// 
			this.lnkHelp.AutoSize = true;
			this.lnkHelp.Location = new System.Drawing.Point(53, 207);
			this.lnkHelp.Name = "lnkHelp";
			this.lnkHelp.Size = new System.Drawing.Size(29, 13);
			this.lnkHelp.TabIndex = 16;
			this.lnkHelp.TabStop = true;
			this.lnkHelp.Text = "帮助";
			this.lnkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHelp_LinkClicked);
			// 
			// lblVersion
			// 
			this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblVersion.Location = new System.Drawing.Point(296, 13);
			this.lblVersion.Name = "lblVersion";
			this.lblVersion.Size = new System.Drawing.Size(156, 16);
			this.lblVersion.TabIndex = 1;
			this.lblVersion.Text = "v3.0.0.0";
			this.lblVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.lblVersion.DoubleClick += new System.EventHandler(this.lblVersion_DoubleClick);
			// 
			// lnkHome
			// 
			this.lnkHome.AutoSize = true;
			this.lnkHome.Location = new System.Drawing.Point(12, 207);
			this.lnkHome.Name = "lnkHome";
			this.lnkHome.Size = new System.Drawing.Size(35, 13);
			this.lnkHome.TabIndex = 15;
			this.lnkHome.TabStop = true;
			this.lnkHome.Text = "主页";
			this.lnkHome.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHome_LinkClicked);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 157);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(59, 13);
			this.label1.TabIndex = 10;
			this.label1.Text = "自动同步：";
			// 
			// cbAutoSync
			// 
			this.cbAutoSync.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbAutoSync.FormattingEnabled = true;
			this.cbAutoSync.Items.AddRange(new object[] {
            "禁用",
            "保存",
            "打开",
            "所有"});
			this.cbAutoSync.Location = new System.Drawing.Point(112, 154);
			this.cbAutoSync.Name = "cbAutoSync";
			this.cbAutoSync.Size = new System.Drawing.Size(110, 21);
			this.cbAutoSync.TabIndex = 11;
			// 
			// chkOAuth
			// 
			this.chkOAuth.AutoSize = true;
			this.chkOAuth.Location = new System.Drawing.Point(285, 156);
			this.chkOAuth.Name = "chkOAuth";
			this.chkOAuth.Size = new System.Drawing.Size(167, 17);
			this.chkOAuth.TabIndex = 12;
			this.chkOAuth.Text = "自定义 OAuth 2.0 凭证";
			this.chkOAuth.UseVisualStyleBackColor = true;
			this.chkOAuth.CheckedChanged += new System.EventHandler(this.chkOAuth_CheckedChanged);
			// 
			// ConfigurationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 237);
			this.Controls.Add(this.chkOAuth);
			this.Controls.Add(this.cbAutoSync);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lnkHome);
			this.Controls.Add(this.lblVersion);
			this.Controls.Add(this.lnkHelp);
			this.Controls.Add(this.cbAccount);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.txtUuid);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtClientSecret);
			this.Controls.Add(this.lblTitle);
			this.Controls.Add(this.lnkGoogle);
			this.Controls.Add(this.txtClientId);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.Name = "ConfigurationForm";
			this.ShowIcon = false;
			this.Text = "Google Drive 同步";
			this.Load += new System.EventHandler(this.GoogleOAuthCredentialsForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtClientId;
		private System.Windows.Forms.LinkLabel lnkGoogle;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.TextBox txtClientSecret;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox txtUuid;
		private System.Windows.Forms.ComboBox cbAccount;
		private System.Windows.Forms.LinkLabel lnkHelp;
		private System.Windows.Forms.Label lblVersion;
		private System.Windows.Forms.LinkLabel lnkHome;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cbAutoSync;
		private System.Windows.Forms.CheckBox chkOAuth;
	}
}