using MySql.Data.MySqlClient;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;
using Npgsql.Internal;

namespace Projekt_bazy
{
    public partial class Form1 : Form
    {
        private void ExecuteSelectQuery(string selectQuery, string connString)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand(selectQuery, conn);
                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                dataGridView1.Rows.Clear();
                dataGridView1.DataSource = dataTable;
            }
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool connected = SSHConnectionManager.ConnectPostgreSQL();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (SSHConnectionManager.Connected())
            {

                string selectQuery = " SELECT id, nazwa, CASE WHEN dostepne < 1 THEN 'brak' ELSE dostepne::text END AS dostepne FROM narzedzia GROUP BY id ORDER BY id ASC;";

                if (!string.IsNullOrEmpty(selectQuery))
                {
                    string connString = SSHConnectionManager.GetConnectionString();
                    ExecuteSelectQuery(selectQuery, connString);
                }
                else
                {
                    MessageBox.Show("Podaj zapytanie SELECT.");
                }

                SSHConnectionManager.DisconnectSSH();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }

    public static class SSHConnectionManager
    {
        private static SshClient client = new SshClient("153.19.111.147", "s48412", "9Dj9Hx4Sy7Ro");
        private static ForwardedPortLocal portssh;

        private static string connString;



        public static bool ConnectSSH()
        {
            client.Connect();

            if (!client.IsConnected)
            {
                MessageBox.Show("Client not connected!");
                return false;
            }
            else
            {
                MessageBox.Show("Client connected!");
                return true;
            }
        }

        public static bool Connected()
        {
            return client.IsConnected;
        }

        public static void DisconnectSSH()
        {
            if (client != null && client.IsConnected)
            {
                client.Disconnect();
            }
        }

        public static bool ConnectPostgreSQL()
        {
            ConnectSSH();

            if (client == null || !client.IsConnected)
            {
                MessageBox.Show("SSH client not connected!");
                return false;
            }

            portssh = new ForwardedPortLocal("127.0.0.1", "127.0.0.1", 5432);
            client.AddForwardedPort(portssh);
            portssh.Start();

            String database = "s48412";
            String username = "s48412";
            String password = "9Dj9Hx4Sy7Ro";

            connString =
                $"Server={portssh.BoundHost};Database=" + database + $";Port={portssh.BoundPort};" +
                 "User Id=" + username + ";Password=" + password + ";";

            return true;
        }

        public static string GetConnectionString()
        {
            return connString;
        }
    }
}
