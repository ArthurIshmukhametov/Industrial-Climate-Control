using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MQTT
{
    public partial class SettingsForm : Form
    {
        public event EventHandler ButtonClicked;

        public SettingsForm()
        {
            InitializeComponent();
        }

        protected void OnButtonClicked(EventArgs e)
        {
            var evt = ButtonClicked;
            if (evt != null) evt(this, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label2.Text = "Schema";
            treeView1.Nodes.Clear();

            TreeNode sensorNode = new TreeNode(System.Net.IPAddress.Parse(maskedTextBox1.Text.Replace(',', '.')).ToString()+":"+ textBox6.Text);

            for (int i = 0; i < int.Parse(textBox1.Text) ; i++)
            {
                TreeNode sensor = new TreeNode("/sensor_" + (1 + i).ToString());
                sensor.Checked = true;
                sensorNode.Nodes.Add(sensor);                
            }
            treeView1.Nodes.Add(sensorNode);

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((!Char.IsDigit(e.KeyChar)) & (!Char.IsControl(e.KeyChar)))
            {
                e.KeyChar = '\0';
            }
        }
    }
}
