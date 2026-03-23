using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Collections.Generic;

namespace CadastroNotasApp
{
    public partial class Form1 : Form
    {
        private string connectionString = "Data Source=cadastro_notas.db;Version=3;";
        private string alunoEditandoMatricula = null;

        // Componentes da Interface
        private DataGridView dgvAlunos;
        private TextBox txtMatricula, txtNome, txtNota1, txtNota2, txtNota3, txtNota4, txtBusca;
        private Button btnAdicionar, btnExcluir, btnEditar, btnSalvar, btnBuscar;

        public Form1()
        {
            InitializeDatabase();
            SetupUI();
            ExibirResultados();
        }

        private void InitializeDatabase()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS alunos (
                                matricula TEXT PRIMARY KEY,
                                nome TEXT NOT NULL,
                                nota1 REAL NOT NULL,
                                nota2 REAL NOT NULL,
                                nota3 REAL NOT NULL,
                                nota4 REAL NOT NULL)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void SetupUI()
        {
            // Configurações da Janela
            this.Text = "Sistema de Gestão Escolar - Cadastro de Notas";
            this.Size = new Size(900, 700);
            this.BackColor = ColorTranslator.FromHtml("#f4f4f9");
            this.Font = new Font("Segoe UI", 10);

            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 250, Padding = new Padding(20) };
            
            // Labels e Inputs
            AddInputGroup(pnlInput, "Matrícula:", out txtMatricula, 0);
            AddInputGroup(pnlInput, "Nome:", out txtNome, 35);
            AddInputGroup(pnlInput, "Nota 1:", out txtNota1, 70);
            AddInputGroup(pnlInput, "Nota 2:", out txtNota2, 105);
            AddInputGroup(pnlInput, "Nota 3:", out txtNota3, 140);
            AddInputGroup(pnlInput, "Nota 4:", out txtNota4, 175);

            // Botões com as cores do Python original
            btnAdicionar = CreateButton("Adicionar Aluno", "#4CAF50", 300, 10, AdicionarAluno);
            btnExcluir = CreateButton("Excluir Aluno", "#f44336", 300, 50, ExcluirAluno);
            btnEditar = CreateButton("Iniciar Edição", "#00acc1", 300, 90, IniciarEdicao);
            btnSalvar = CreateButton("Salvar Edição", "#00796b", 300, 130, SalvarEdicao);

            pnlInput.Controls.AddRange(new Control[] { btnAdicionar, btnExcluir, btnEditar, btnSalvar });

            // Campo de Busca
            Label lblBusca = new Label { Text = "Buscar por Nome:", Location = new Point(550, 10), AutoSize = true };
            txtBusca = new TextBox { Location = new Point(550, 35), Width = 200 };
            btnBuscar = CreateButton("Buscar", "#4CAF50", 550, 65, (s, e) => ExibirResultados(txtBusca.Text));
            pnlInput.Controls.AddRange(new Control[] { lblBusca, txtBusca, btnBuscar });

            // Grid de Resultados (Tabela)
            dgvAlunos = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true
            };
            dgvAlunos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);

            this.Controls.Add(dgvAlunos);
            this.Controls.Add(pnlInput);
        }

        private void AddInputGroup(Panel p, string labelText, out TextBox tb, int y)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(20, y), Width = 80 };
            tb = new TextBox { Location = new Point(110, y), Width = 150 };
            p.Controls.Add(lbl);
            p.Controls.Add(tb);
        }

        private Button CreateButton(string text, string hexColor, int x, int y, EventHandler action)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(150, 35),
                BackColor = ColorTranslator.FromHtml(hexColor),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += action;
            return btn;
        }

        private void ExibirResultados(string filtro = "")
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Matrícula");
            dt.Columns.Add("Nome");
            dt.Columns.Add("Nota 1");
            dt.Columns.Add("Nota 2");
            dt.Columns.Add("Nota 3");
            dt.Columns.Add("Nota 4");
            dt.Columns.Add("Média");
            dt.Columns.Add("Situação Final");

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM alunos";
                if (!string.IsNullOrEmpty(filtro)) sql += " WHERE nome LIKE @nome";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(filtro)) cmd.Parameters.AddWithValue("@nome", $"%{filtro}%");
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            double n1 = reader.GetDouble(2), n2 = reader.GetDouble(3), n3 = reader.GetDouble(4), n4 = reader.GetDouble(5);
                            double media = (n1 + n2 + n3 + n4) / 4;
                            string situacao = media >= 6 ? "APROVADO" : "REPROVADO";
                            
                            dt.Rows.Add(reader[0], reader[1], n1, n2, n3, n4, media.ToString("F1"), situacao);
                        }
                    }
                }
            }
            dgvAlunos.DataSource = dt;
            
            // Colorir a situação no Grid
            foreach (DataGridViewRow row in dgvAlunos.Rows)
            {
                if (row.Cells["Situação Final"].Value?.ToString() == "REPROVADO")
                    row.Cells["Situação Final"].Style.ForeColor = Color.Red;
                else
                    row.Cells["Situação Final"].Style.ForeColor = Color.Green;
            }
        }

        private void AdicionarAluno(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMatricula.Text) || string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("Todos os campos são obrigatórios.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string sql = "INSERT INTO alunos (matricula, nome, nota1, nota2, nota3, nota4) VALUES (@mat, @nome, @n1, @n2, @n3, @n4)";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@mat", txtMatricula.Text);
                        cmd.Parameters.AddWithValue("@nome", txtNome.Text);
                        cmd.Parameters.AddWithValue("@n1", double.Parse(txtNota1.Text));
                        cmd.Parameters.AddWithValue("@n2", double.Parse(txtNota2.Text));
                        cmd.Parameters.AddWithValue("@n3", double.Parse(txtNota3.Text));
                        cmd.Parameters.AddWithValue("@n4", double.Parse(txtNota4.Text));
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Aluno adicionado com sucesso!");
                ExibirResultados();
                LimparCampos();
            }
            catch (Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
        }

        private void ExcluirAluno(object sender, EventArgs e)
        {
            if (dgvAlunos.SelectedRows.Count == 0) return;

            string matricula = dgvAlunos.SelectedRows[0].Cells["Matrícula"].Value.ToString();
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM alunos WHERE matricula = @mat", conn))
                {
                    cmd.Parameters.AddWithValue("@mat", matricula);
                    cmd.ExecuteNonQuery();
                }
            }
            ExibirResultados();
        }

        private void IniciarEdicao(object sender, EventArgs e)
        {
            if (dgvAlunos.SelectedRows.Count == 0) return;

            var row = dgvAlunos.SelectedRows[0];
            alunoEditandoMatricula = row.Cells["Matrícula"].Value.ToString();
            
            txtMatricula.Text = row.Cells["Matrícula"].Value.ToString();
            txtNome.Text = row.Cells["Nome"].Value.ToString();
            txtNota1.Text = row.Cells["Nota 1"].Value.ToString();
            txtNota2.Text = row.Cells["Nota 2"].Value.ToString();
            txtNota3.Text = row.Cells["Nota 3"].Value.ToString();
            txtNota4.Text = row.Cells["Nota 4"].Value.ToString();
        }

        private void SalvarEdicao(object sender, EventArgs e)
        {
            if (alunoEditandoMatricula == null) return;

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "UPDATE alunos SET matricula=@mat, nome=@nome, nota1=@n1, nota2=@n2, nota3=@n3, nota4=@n4 WHERE matricula=@oldMat";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@mat", txtMatricula.Text);
                    cmd.Parameters.AddWithValue("@nome", txtNome.Text);
                    cmd.Parameters.AddWithValue("@n1", double.Parse(txtNota1.Text));
                    cmd.Parameters.AddWithValue("@n2", double.Parse(txtNota2.Text));
                    cmd.Parameters.AddWithValue("@n3", double.Parse(txtNota3.Text));
                    cmd.Parameters.AddWithValue("@n4", double.Parse(txtNota4.Text));
                    cmd.Parameters.AddWithValue("@oldMat", alunoEditandoMatricula);
                    cmd.ExecuteNonQuery();
                }
            }
            alunoEditandoMatricula = null;
            ExibirResultados();
            LimparCampos();
        }

        private void LimparCampos()
        {
            txtMatricula.Clear(); txtNome.Clear(); txtNota1.Clear();
            txtNota2.Clear(); txtNota3.Clear(); txtNota4.Clear();
        }
    }
}