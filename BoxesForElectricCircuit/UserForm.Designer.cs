namespace BoxesForElectricCircuit
{
    partial class UserForm
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
            this.equipmentTreeView = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.applyButton = new System.Windows.Forms.Button();
            this.conduitTypesComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.conduitTypesErrorLabel = new System.Windows.Forms.Label();
            this.equipmentErrorLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // equipmentTreeView
            // 
            this.equipmentTreeView.Location = new System.Drawing.Point(12, 29);
            this.equipmentTreeView.Name = "equipmentTreeView";
            this.equipmentTreeView.Size = new System.Drawing.Size(502, 195);
            this.equipmentTreeView.TabIndex = 0;
            this.equipmentTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.equipmentTreeView_NodeMouseClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(288, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Выберете электрические цепи для постройки коробов:";
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(372, 325);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(142, 27);
            this.applyButton.TabIndex = 2;
            this.applyButton.Text = "Применить";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // conduitTypesComboBox
            // 
            this.conduitTypesComboBox.FormattingEnabled = true;
            this.conduitTypesComboBox.Location = new System.Drawing.Point(12, 284);
            this.conduitTypesComboBox.Name = "conduitTypesComboBox";
            this.conduitTypesComboBox.Size = new System.Drawing.Size(502, 21);
            this.conduitTypesComboBox.TabIndex = 3;
            this.conduitTypesComboBox.Text = "--Выберете тип короба--";
            this.conduitTypesComboBox.SelectedIndexChanged += new System.EventHandler(this.conduitTypesComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 266);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Выберете тип короба:";
            // 
            // conduitTypesErrorLabel
            // 
            this.conduitTypesErrorLabel.AutoSize = true;
            this.conduitTypesErrorLabel.ForeColor = System.Drawing.Color.Maroon;
            this.conduitTypesErrorLabel.Location = new System.Drawing.Point(18, 312);
            this.conduitTypesErrorLabel.Name = "conduitTypesErrorLabel";
            this.conduitTypesErrorLabel.Size = new System.Drawing.Size(121, 13);
            this.conduitTypesErrorLabel.TabIndex = 5;
            this.conduitTypesErrorLabel.Text = "Не выбран тип короба";
            this.conduitTypesErrorLabel.Visible = false;
            // 
            // equipmentErrorLabel
            // 
            this.equipmentErrorLabel.AutoSize = true;
            this.equipmentErrorLabel.ForeColor = System.Drawing.Color.Maroon;
            this.equipmentErrorLabel.Location = new System.Drawing.Point(17, 231);
            this.equipmentErrorLabel.Name = "equipmentErrorLabel";
            this.equipmentErrorLabel.Size = new System.Drawing.Size(216, 13);
            this.equipmentErrorLabel.TabIndex = 6;
            this.equipmentErrorLabel.Text = "Не выбрана ни одна электрическая цепь";
            this.equipmentErrorLabel.Visible = false;
            // 
            // UserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 367);
            this.Controls.Add(this.equipmentErrorLabel);
            this.Controls.Add(this.conduitTypesErrorLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.conduitTypesComboBox);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.equipmentTreeView);
            this.Name = "UserForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView equipmentTreeView;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.ComboBox conduitTypesComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label conduitTypesErrorLabel;
        private System.Windows.Forms.Label equipmentErrorLabel;
    }
}