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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace Projekt_bazy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;
        }

        private void cleanScreen()
        {
            dataGridView1.ReadOnly = true;
            label2.Visible = false;
            label3.Visible = false;
            button2.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            textBox2.Clear();
            textBox2.Visible = false;
            textBox3.Clear();
            textBox3.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            button11.Visible = false;
            dateTimePicker1.Visible = false;
        }

        private void ExecuteSelectQuery(string selectQuery, string connString)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand(selectQuery, conn);
                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                dataTable.Rows.Clear();
                adapter.Fill(dataTable);
                dataGridView1.DataSource = dataTable;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        //------------------------------------------------------------------------------------------------------------------------------------//
        private void button1_Click(object sender, EventArgs e) //connect
        {
            bool connected = SSHConnectionManager.ConnectPostgreSQL();
            button1.Visible = false;
            button3.Visible = true;
            cleanScreen();

        }

        private void button3_Click(object sender, EventArgs e) //disconnect
        {
            if (SSHConnectionManager.Connected())
            {
                SSHConnectionManager.DisconnectSSH();
                MessageBox.Show("Client disconnected!");
            }
            else
            {
                MessageBox.Show("Client was disconnected!");
            }
            button3.Visible = false;
            button1.Visible = true;
            cleanScreen();
            button2.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
        }


        //------------------------------------------------------------------------------------------------------------------------------------//
        private void button2_Click(object sender, EventArgs e) //narzedzia
        {
            cleanScreen();
            button2.Enabled = false;
            button6.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = true;
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
            }
            button6.Visible = true;
            button7.Visible = true;
            button8.Visible = true;
        }

        private bool isFirstClickButton7 = true;
        private void button7_Click(object sender, EventArgs e) //dodaj
        {

            string nazwa = textBox2.Text.Trim();
            string dostepneStr = textBox3.Text.Trim();

            if (isFirstClickButton7)
            {
                label2.Visible = true;
                label2.Text = "Nazwa narzedzia:";
                label3.Visible = true;
                label3.Text = "Ilosc:";
                textBox2.Visible = true;
                textBox3.Visible = true;
                isFirstClickButton7 = false;
                button6.Enabled = false;
                button8.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                return;
            }

            if (SSHConnectionManager.Connected())
            {
                if (string.IsNullOrEmpty(nazwa) || string.IsNullOrEmpty(dostepneStr))
                {
                    MessageBox.Show("Podaj nazwe i ilossc dostepnych narzedzi.");
                    return;
                }

                if (!int.TryParse(dostepneStr, out int dostepne) || dostepne <= 0)
                {
                    MessageBox.Show("Nieprawidlowa wartosc dla ilosci dostepnych narzedzi. Podaj liczbe calkowita wieksza od zera.");
                    return;
                }

                string connString = SSHConnectionManager.GetConnectionString();

                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string insertQuery = $"INSERT INTO narzedzia (nazwa, dostepne) VALUES ('{nazwa}', {dostepne});";

                    using (var cmd = new NpgsqlCommand(insertQuery, conn))
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Narzedzie zostalo dodane.");
                            button2_Click(null, null);
                        }
                        else
                        {
                            MessageBox.Show("Nie udalo sie dodac narzedzia.");
                        }
                    }
                }
                isFirstClickButton7 = true;
                button4.Enabled = true;
                button5.Enabled = true;
            }
        }

        private bool isFirstClickButton6 = true;
        private void button6_Click(object sender, EventArgs e) //usun
        {
            if (isFirstClickButton6)
            {
                label2.Visible = true;
                label2.Text = "ID:";
                textBox2.Visible = true;
                isFirstClickButton6 = false;
                button7.Enabled = false;
                button8.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                return;
            }

            if (SSHConnectionManager.Connected())
            {
                string idStr = textBox2.Text.Trim();

                if (string.IsNullOrEmpty(idStr))
                {
                    MessageBox.Show("Podaj ID narzedzia do usuniecia.");
                    return;
                }

                if (!int.TryParse(idStr, out int id) || id <= 0)
                {
                    MessageBox.Show("Nieprawidlowa wartosc dla ID rekordu. Podaj liczbę calkowita większa od zera.");
                    return;
                }

                string connString = SSHConnectionManager.GetConnectionString();

                DialogResult result = MessageBox.Show("Czy na pewno chcesz usunac ten rekord?", "Potwierdzenie", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    using (var conn = new NpgsqlConnection(connString))
                    {
                        conn.Open();

                        string deleteQuery = $"DELETE FROM narzedzia WHERE id = {id};";

                        using (var cmd = new NpgsqlCommand(deleteQuery, conn))
                        {
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Rekord zostal usuniety.");
                                button2_Click(null, null);
                            }
                            else
                            {
                                MessageBox.Show("Nie udalo sie usunac rekordu.");
                            }
                        }
                    }
                }
                else if (result == DialogResult.No)
                {
                    MessageBox.Show("Operacja usuniecia rekordu zostala anulowana.");
                }
                isFirstClickButton6 = true;
                button4.Enabled = true;
                button5.Enabled = true;
            }
        }

        private DataGridViewCheckBoxColumn checkBoxColumn;
        private bool isFirstClickButton8 = true;
        private void button8_Click(object sender, EventArgs e) // wypozycz
        {
            if (isFirstClickButton8)
            {
                dataGridView1.ReadOnly = false;
                label2.Visible = true;
                label2.Text = "Nazwa ekipy::";
                label3.Visible = true;
                label3.Text = "Data oddania:";
                textBox2.Visible = true;
                dateTimePicker1.Visible = true;
                checkBoxColumn = new DataGridViewCheckBoxColumn();
                checkBoxColumn.HeaderText = "wypozycz";
                checkBoxColumn.Name = "checkBoxColumn";
                dataGridView1.Columns.Insert(0, checkBoxColumn);
                button6.Enabled = false;
                button7.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                isFirstClickButton8 = false;
                return;
            }

            if (SSHConnectionManager.Connected())
            {
                string idEkipyStr = textBox2.Text.Trim();
                string dataZwrotu = dateTimePicker1.Value.ToString("yyyy-MM-dd");
                DateTime dzisiejszaData = DateTime.Today;
                string dataWyslania = dzisiejszaData.ToString("yyyy-MM-dd");

                if (string.IsNullOrEmpty(idEkipyStr) || string.IsNullOrEmpty(dataZwrotu))
                {
                    MessageBox.Show("Podaj ID ekipy oraz wybierz date zwrotu narzedzi.");
                    return;
                }

                if (!int.TryParse(idEkipyStr, out int idEkipy) || idEkipy <= 0)
                {
                    MessageBox.Show("Nieprawidlowa wartosc dla ID ekipy. Podaj liczbe calkowita wieksza od zera.");
                    return;
                }

                string connString = SSHConnectionManager.GetConnectionString();

                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    bool niedostepneNarzedzieKomunikatWyswietlony = false;

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        DataGridViewCheckBoxCell checkBoxCell = row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell;
                        bool isChecked = false;
                        if (checkBoxCell.Value != null && checkBoxCell.Value.ToString() == "True")
                        {
                            isChecked = true;
                        }

                        string dostepneStr = row.Cells["dostepne"]?.Value?.ToString();
                        if ((string.IsNullOrEmpty(dostepneStr) || (!int.TryParse(dostepneStr, out int dostepne) || dostepne < 1) || dostepneStr == "Brak") && isChecked)
                        {
                            if (!niedostepneNarzedzieKomunikatWyswietlony)
                            {
                                MessageBox.Show("Narzedzie o ID " + row.Cells["id"].Value + " jest niedostepne i nie zostalo wypozyczone.");
                                niedostepneNarzedzieKomunikatWyswietlony = true;
                            }
                            continue;
                        }

                        if (isChecked)
                        {
                            int narzedzieId = Convert.ToInt32(row.Cells["id"].Value);

                            string insertQuery = $"INSERT INTO wyslanezestawy (narzedzie_id, ekipa_id, data_wyslania, data_oddania, zworcono) VALUES ({narzedzieId}, {idEkipy}, '{dataWyslania}', '{dataZwrotu}', false);";
                            using (var cmd = new NpgsqlCommand(insertQuery, conn))
                            {
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    row.Cells["checkBoxColumn"].Value = false;
                                }
                            }
                        }
                    }

                    MessageBox.Show("Narzedzia zostaly wypozyczone.");
                    button2_Click(null, null);
                }

                dateTimePicker1.Value = DateTime.Now;
                isFirstClickButton8 = true;
                dataGridView1.Columns.Remove(checkBoxColumn);
                button4.Enabled = true;
                button5.Enabled = true;
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------//
        private void button4_Click(object sender, EventArgs e) //ekipy
        {
            cleanScreen();
            button4.Enabled = false;
            button9.Enabled = true;
            button10.Enabled = true;
            if (SSHConnectionManager.Connected())
            {
                string selectQuery = " SELECT * FROM public.ekipyremontowe ORDER BY id ASC;";

                if (!string.IsNullOrEmpty(selectQuery))
                {
                    string connString = SSHConnectionManager.GetConnectionString();
                    ExecuteSelectQuery(selectQuery, connString);
                }
                else
                {
                    MessageBox.Show("Podaj zapytanie SELECT.");
                }
            }
            button9.Visible = true;
            button10.Visible = true;
        }

        private bool isFirstClickButton9 = true;
        private void button9_Click(object sender, EventArgs e) //dodaj
        {
            string nazwa = textBox2.Text.Trim();

            if (isFirstClickButton9)
            {
                label2.Visible = true;
                label2.Text = "Nazwa ekipy:";
                textBox2.Visible = true;
                isFirstClickButton9 = false;
                button10.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                return;
            }

            if (SSHConnectionManager.Connected())
            {
                if (string.IsNullOrEmpty(nazwa))
                {
                    MessageBox.Show("Podaj nazwe ekipy.");
                    return;
                }

                string connString = SSHConnectionManager.GetConnectionString();

                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string insertQuery = $"INSERT INTO ekipyremontowe (nazwa) VALUES ('{nazwa}');";

                    using (var cmd = new NpgsqlCommand(insertQuery, conn))
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Ekipa zostala dodana.");
                            button4_Click(null, null);
                        }
                        else
                        {
                            MessageBox.Show("Nie udało sie dodac ekipy.");
                        }
                    }
                }
                isFirstClickButton9 = true;
                button4.Enabled = true;
                button5.Enabled = true;
            }
        }

        private bool isFirstClickButton10 = true;

        private void button10_Click(object sender, EventArgs e) //usun
        {
            if (isFirstClickButton10)
            {
                label2.Visible = true;
                label2.Text = "ID:";
                textBox2.Visible = true;
                isFirstClickButton10 = false;
                button9.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                return;
            }

            if (SSHConnectionManager.Connected())
            {
                string idStr = textBox2.Text.Trim();

                if (string.IsNullOrEmpty(idStr))
                {
                    MessageBox.Show("Podaj ID ekipy do usuniecia.");
                    return;
                }

                if (!int.TryParse(idStr, out int id) || id <= 0)
                {
                    MessageBox.Show("Nieprawidlowa wartosc dla ID ekipy. Podaj liczbe calkowita wieksza od zera.");
                    return;
                }

                string connString = SSHConnectionManager.GetConnectionString();

                DialogResult result = MessageBox.Show("Czy na pewno chcesz usunac te ekipe?", "Potwierdzenie", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    using (var conn = new NpgsqlConnection(connString))
                    {
                        conn.Open();

                        string deleteQuery = $"DELETE FROM ekipyremontowe WHERE id = {id};";

                        using (var cmd = new NpgsqlCommand(deleteQuery, conn))
                        {
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Ekipa zostala usunieta.");
                                button4_Click(null, null);
                            }
                            else
                            {
                                MessageBox.Show("Nie udalo sie usunac ekipy.");
                            }
                        }
                    }
                }
                else if (result == DialogResult.No)
                {
                    MessageBox.Show("Operacja usuniccia ekipy zostala anulowana.");
                }
                isFirstClickButton10 = true;
                button4.Enabled = true;
                button5.Enabled = true;
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------//
        private void button5_Click(object sender, EventArgs e) //wyslanezstawy
        {
            cleanScreen();
            button5.Enabled = false;
            button11.Enabled = true;
            if (SSHConnectionManager.Connected())
            {

                string selectQuery = "SELECT * FROM public.wyslanezestawy ORDER BY id ASC ;";

                if (!string.IsNullOrEmpty(selectQuery))
                {
                    string connString = SSHConnectionManager.GetConnectionString();
                    ExecuteSelectQuery(selectQuery, connString);
                }
                else
                {
                    MessageBox.Show("Podaj zapytanie SELECT.");
                }
            }
            button11.Visible = true;
        }

        private bool isFirstClickButton11 = true;
        private void button11_Click(object sender, EventArgs e) //zwroc
        {
            if (isFirstClickButton11)
            {
                string query = "SELECT *FROM wyslanezestawy WHERE zworcono = false";
                DataTable dataTable = new DataTable();

                if (SSHConnectionManager.Connected())
                {
                    string connString = SSHConnectionManager.GetConnectionString();

                    using (var conn = new NpgsqlConnection(connString))
                    {
                        conn.Open();

                        using (var cmd = new NpgsqlCommand(query, conn))
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                dataTable.Load(reader);
                            }
                        }
                    }
                }

                // Wyświetl pobrane rekordy w DataGridView
                dataGridView1.DataSource = dataTable;
                dataGridView1.ReadOnly = false;
                MessageBox.Show("Zwroc narzedzia, zaznaczajac odpowiednie pola wyboru w kolumnie 'zworcono'.");
                isFirstClickButton11 = false;
                button4.Enabled = false;
                button5.Enabled = false;
            }
            else
            {
                button5_Click(null, null);
                isFirstClickButton11 = true;
                button4.Enabled = true;
                button5.Enabled = true;
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridView dataGridView = (DataGridView)sender;
                DataGridViewRow row = dataGridView.Rows[e.RowIndex];

                int id = Convert.ToInt32(row.Cells["id"].Value);
                bool isChecked = Convert.ToBoolean(row.Cells["zworcono"].Value);

                // Aktualizacja wartości w bazie danych
                string connString = SSHConnectionManager.GetConnectionString();
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string updateQuery = $"UPDATE wyslanezestawy SET zworcono = {isChecked} WHERE id = {id};";
                    using (var cmd = new NpgsqlCommand(updateQuery, conn))
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Zaktualizuj wartość w DataGridView
                            DataGridViewCheckBoxCell checkBoxCell = row.Cells["zworcono"] as DataGridViewCheckBoxCell;
                            checkBoxCell.Value = isChecked;
                        }
                    }
                }
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------------------------//
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
