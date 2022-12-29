namespace server2
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_port = new System.Windows.Forms.TextBox();
            this.textBox_num = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.richTextBox_info = new System.Windows.Forms.RichTextBox();
            this.button_startgame = new System.Windows.Forms.Button();
            this.button_listen = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "port:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(182, 25);
            this.label2.TabIndex = 1;
            this.label2.Text = "num of questions:";
            // 
            // textBox_port
            // 
            this.textBox_port.Location = new System.Drawing.Point(229, 34);
            this.textBox_port.Name = "textBox_port";
            this.textBox_port.Size = new System.Drawing.Size(142, 31);
            this.textBox_port.TabIndex = 3;
            // 
            // textBox_num
            // 
            this.textBox_num.Location = new System.Drawing.Point(229, 79);
            this.textBox_num.Name = "textBox_num";
            this.textBox_num.Size = new System.Drawing.Size(142, 31);
            this.textBox_num.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 163);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 25);
            this.label4.TabIndex = 7;
            this.label4.Text = "info:";
            // 
            // richTextBox_info
            // 
            this.richTextBox_info.Location = new System.Drawing.Point(26, 205);
            this.richTextBox_info.Name = "richTextBox_info";
            this.richTextBox_info.Size = new System.Drawing.Size(550, 558);
            this.richTextBox_info.TabIndex = 8;
            this.richTextBox_info.Text = "";
            // 
            // button_startgame
            // 
            this.button_startgame.Location = new System.Drawing.Point(403, 121);
            this.button_startgame.Name = "button_startgame";
            this.button_startgame.Size = new System.Drawing.Size(140, 67);
            this.button_startgame.TabIndex = 9;
            this.button_startgame.Text = "Start Game";
            this.button_startgame.UseVisualStyleBackColor = true;
            this.button_startgame.Click += new System.EventHandler(this.button_startgame_Click);
            // 
            // button_listen
            // 
            this.button_listen.Location = new System.Drawing.Point(403, 28);
            this.button_listen.Name = "button_listen";
            this.button_listen.Size = new System.Drawing.Size(140, 43);
            this.button_listen.TabIndex = 10;
            this.button_listen.Text = "listen";
            this.button_listen.UseVisualStyleBackColor = true;
            this.button_listen.Click += new System.EventHandler(this.button_listen_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(606, 785);
            this.Controls.Add(this.button_listen);
            this.Controls.Add(this.button_startgame);
            this.Controls.Add(this.richTextBox_info);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_num);
            this.Controls.Add(this.textBox_port);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_port;
        private System.Windows.Forms.TextBox textBox_num;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RichTextBox richTextBox_info;
        private System.Windows.Forms.Button button_startgame;
        private System.Windows.Forms.Button button_listen;
    }
}

