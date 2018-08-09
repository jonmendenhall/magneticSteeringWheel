using System;
using System.Windows.Forms;
using SlimDX.DirectInput;

public partial class CustomApp: Form
{

    private CustomManager manager;
    private Label label1;
    private Button stateButton;
    

    public CustomApp()
    {
        InitializeComponent();

        manager = new CustomManager(this);

        
    }

    private void InitializeComponent()
    {
            this.stateButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // stateButton
            // 
            this.stateButton.Location = new System.Drawing.Point(192, 157);
            this.stateButton.Name = "stateButton";
            this.stateButton.Size = new System.Drawing.Size(194, 67);
            this.stateButton.TabIndex = 0;
            this.stateButton.Text = "Start";
            this.stateButton.UseVisualStyleBackColor = true;
            this.stateButton.Click += new System.EventHandler(this.stateButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(68, 91);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(449, 24);
            this.label1.TabIndex = 1;
            this.label1.Text = "Connect the steering wheel via USB then press Start!";
            // 
            // CustomApp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(163F, 163F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(586, 299);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.stateButton);
            this.Name = "CustomApp";
            this.Text = "Steering Wheel Xbox 360 Converter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CustomApp_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    private void stateButton_Click(object sender, EventArgs e)
    {
        if(stateButton.Text == "Start")
        {
            if(manager.Start())
                stateButton.Text = "Stop";
        } else
        {
            if(manager.Stop())
                stateButton.Text = "Start";
        }
    }

    private void CustomApp_FormClosing(object sender, FormClosingEventArgs e)
    {
        System.Environment.Exit(1);
    }

}
