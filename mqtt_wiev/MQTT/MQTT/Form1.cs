using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using System.Diagnostics;
using System.IO;
using System.Data.OleDb;

namespace MQTT
{  
    public partial class Form1 : Form
    {
        public class Metric
        {
            public Stack<Values> lastValues;
            public string mainTopic
            {
                get => _mainTopic;
                set { _mainTopic = value; }
            }
            private string _mainTopic;
            public string[] topics
            {
                get => _topics;
                set { _topics = value; }
            }
            private string[] _topics;
            public float _avgTemp { get { return _avgForStack(_getTempStack(lastValues)); } }
            public float _avgPressure { get { return _avgForStack(_getPresStack(lastValues)); } }
            public float _avgHum { get { return _avgForStack(_getHumStack(lastValues)); } }
            public float _avgGas { get { return _avgForStack(_getGasStack(lastValues)); } }

            public Metric(string MainTopic, string[] Topic)
            {
                mainTopic = MainTopic;
                topics = Topic;
            }
            public string[] GetTopics()
            {
                string[] topic = new string[4];
                for (int i = 0; i < 4; i++)
                {
                    topic[i] = mainTopic + "/" + topics[i];
                }
                return topic;
            }
            public Metric()
            {
                mainTopic = "";
                topics = new string[4] { "", "", "", "" };
            }

            private float _avgForStack(Stack<float> stack)
            {
                float mean;
                float sum = 0;
                foreach (float value in stack)
                {
                    sum += value;
                }
                mean = sum / stack.Count;
                return mean;
            }
            private Stack<float> _getTempStack(Stack<Values> Value)
            {
                Stack<float> valueTempStack = new Stack<float>();
                foreach (Values value in Value)
                {
                    valueTempStack.Push(value.Temperature);
                }
                return valueTempStack;
            }

            private Stack<float> _getPresStack(Stack<Values> Value)
            {
                Stack<float> valuePresStack = new Stack<float>();
                foreach (Values value in Value)
                {
                    valuePresStack.Push(value.Pressure);
                }
                return valuePresStack;
            }

            private Stack<float> _getHumStack(Stack<Values> Value)
            {
                Stack<float> valueHumStack = new Stack<float>();
                foreach (Values value in Value)
                {
                    valueHumStack.Push(value.Humidity);
                }
                return valueHumStack;
            }

            private Stack<float> _getGasStack(Stack<Values> Value)
            {
                Stack<float> valueGasStack = new Stack<float>();
                foreach (Values value in Value)
                {
                    valueGasStack.Push(value.Gas);
                }
                return valueGasStack;
            }

            public class Values
            {
                float temperature;
                float pressure;
                float humidity;
                float gas;
                public Values(float Temperature, float Pressure, float Humidity, float Gas)
                {
                    temperature = Temperature;
                    pressure = Pressure;
                    humidity = Humidity;
                    gas = Gas;
                }
                public Values(float Temperature, float Humidity)
                {
                    temperature = Temperature;
                    humidity = Humidity;
                }

                public float Temperature { get { return temperature; } set { Temperature = value; } }
                public float Pressure { get { return pressure; } set { Pressure = value; } }
                public float Humidity { get { return humidity; } set { Humidity = value; } }
                public float Gas { get { return gas; } set { Gas = value; } }
            }
        }        
        Metric metric = new Metric();
        
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           // ????? 
        }

        private void Client_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            if (e.Topic == metric.GetTopics()[0])
            {
                if (InvokeRequired)
                    this.Invoke(new Action(() => label1.Text = "Received = " + Encoding.UTF8.GetString(e.Message)
                    + "°С on topic " + e.Topic 
                    + " time: " + DateTime.Now.ToLongTimeString()));
            }
            if (e.Topic == metric.GetTopics()[1])
            {
                if (InvokeRequired)
                    this.Invoke(new Action(() => label2.Text = "Received = " + Encoding.UTF8.GetString(e.Message) 
                    + "hPa on topic " + e.Topic 
                    + " time: " + DateTime.Now.ToLongTimeString()));
            }
            if (e.Topic == metric.GetTopics()[2])
            {
                if (InvokeRequired)
                    this.Invoke(new Action(() => label3.Text = "Received = " + Encoding.UTF8.GetString(e.Message) +
                    "% on topic " + e.Topic + 
                    " time: " + DateTime.Now.ToLongTimeString()));
            }
            if (e.Topic == metric.GetTopics()[3])
            {
                if (InvokeRequired)
                    this.Invoke(new Action(() => label4.Text = "Received = " + Encoding.UTF8.GetString(e.Message) + 
                    "KOhms on topic " + e.Topic + 
                    " time: " + DateTime.Now.ToLongTimeString()));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            maskedTextBox1.ValidatingType = typeof(System.Net.IPAddress);
            dataGridView1.DataSource = ReadCSVFile(@"C:\Users\aptyp\Desktop\ДИПЛОМ\mqtt_wiev\MQTT\values.csv");
            //   CSVReader scvDataSet = new CSVReader();
            //   DataSet datase =
            //       scvDataSet.GetCVSFile(@"C:\Users\aptyp\Desktop\ДИПЛОМ\mqtt_wiev\MQTT", "values.csv");

            var runMosquitto = new System.Diagnostics.ProcessStartInfo
            {
                FileName = @"C:\Program Files\mosquitto\mosquitto.exe",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            System.Diagnostics.Process.Start(runMosquitto);
        }

        public DataTable ReadCSVFile(string pathToCsvFile)
        {
            //создаём таблицу
            DataTable dt = new DataTable("values");
            //создаём колонки
            DataColumn colTime;
            colTime = new DataColumn(" ESP8266/time", typeof(DateTime));
            DataColumn colTemperature;
            colTemperature = new DataColumn("ESP8266/temperature", typeof(float));
            DataColumn colHumidity;
            colHumidity = new DataColumn("ESP8266/humidity", typeof(float));
            DataColumn colPressure;
            colPressure = new DataColumn("ESP8266/pressure", typeof(float));
            DataColumn colGas;
            colGas = new DataColumn("ESP8266/gas", typeof(float));
            //добавляем колонки в таблицу
            dt.Columns.AddRange(new DataColumn[] {colTime, colTemperature, colHumidity, colPressure, colGas });
            try
            {
                DataRow dr = null;
                string[] carValues = null;
                string[] cars = File.ReadAllLines(pathToCsvFile);
                for (int i = 0; i < cars.Length; i++)
                {
                    if (!String.IsNullOrEmpty(cars[i]))
                    {
                        carValues = cars[i].Split(',');
                        //создаём новую строку
                        dr = dt.NewRow();
                        dr["ESP8266/time"] = carValues[0];
                        dr["ESP8266/temperature"] = carValues[1];
                        dr["ESP8266/humidity"] = int.Parse(carValues[2]);
                        dr["ESP8266/pressure"] = int.Parse(carValues[3]);
                        dr["ESP8266/gas"] = Double.Parse(carValues[4]);
                        //добавляем строку в таблицу
                        dt.Rows.Add(dr);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dt;
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var f = new SettingsForm();
            f.ShowDialog(this);
            f.ButtonClicked += Form2ButtonClicked;
        }
        private void Form2ButtonClicked(object sender, EventArgs e)
        {
            string address = System.Net.IPAddress.Parse(maskedTextBox1.Text.Replace(',', '.')).ToString();
            MqttClient client = new MqttClient(address);
            bool state = subscribe(address, metric, client);
            
        }
        public bool subscribe(string address, Metric metric, MqttClient client)
        {
            bool subState = false;
            byte code = client.Connect(Guid.NewGuid().ToString());

            if (textBox2.Text != "" && textBox3.Text != "" && textBox4.Text != "" && textBox5.Text != "" && textBox6.Text != "")
            {
                metric.mainTopic = textBox2.Text;
                string[] topicValues = new string[4] { textBox3.Text, textBox4.Text, textBox5.Text, textBox6.Text };
                metric.topics = topicValues;
                string[] topics = new string[4];
                topics = metric.GetTopics();
                client.Subscribe(
                  topics,
                  new byte[] { 2, 2, 2, 2 });
                radioButton1.Checked = true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("You have to enter a topic to subscribe!");
            }
            client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);
            return subState;
        }
    }
}
