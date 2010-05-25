namespace TextEditorSWF
{
	partial class TextEditor
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && (components != null)) {
				components.Dispose ();
			}
			base.Dispose (disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
			this.menuStrip = new System.Windows.Forms.MenuStrip ();
			this.richTextBox = new System.Windows.Forms.RichTextBox ();
			this.toolStrip = new System.Windows.Forms.ToolStrip ();
			this.SuspendLayout ();
			// 
			// menuStrip
			// 
			this.menuStrip.Location = new System.Drawing.Point (0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size (676, 24);
			this.menuStrip.TabIndex = 0;
			this.menuStrip.Text = "menuStrip";
			// 
			// richTextBox
			// 
			this.richTextBox.AcceptsTab = true;
			this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox.Location = new System.Drawing.Point (0, 49);
			this.richTextBox.Name = "richTextBox";
			this.richTextBox.Size = new System.Drawing.Size (676, 391);
			this.richTextBox.TabIndex = 1;
			this.richTextBox.Text = "";
			// 
			// toolStrip
			// 
			this.toolStrip.Location = new System.Drawing.Point (0, 24);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = new System.Drawing.Size (676, 25);
			this.toolStrip.TabIndex = 2;
			this.toolStrip.Text = "toolStrip";
			// 
			// TextEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size (676, 440);
			this.Controls.Add (this.richTextBox);
			this.Controls.Add (this.toolStrip);
			this.Controls.Add (this.menuStrip);
			this.MainMenuStrip = this.menuStrip;
			this.Name = "TextEditor";
			this.Text = "Text Editor";
			this.ResumeLayout (false);
			this.PerformLayout ();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip;
		private System.Windows.Forms.RichTextBox richTextBox;
		private System.Windows.Forms.ToolStrip toolStrip;
	}
}

