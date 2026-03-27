using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json; // JSON Nativo do .NET
using System.Windows.Forms;

namespace CadastroNotasApp
{
    public partial class Form1 : Form
    {
        private string jsonPath = "alunos.json";
        private string alunoEditandoMatricula = null;

        // Componentes da Interface
        private DataGridView dgvAlunos;
        private TextBox txtMatricula, txtNome, txtNota1, txtNota2, txtNota3, txtNota4, txtBusca;
        private Button btnAdicionar, btnExcluir, btnEditar, btnSalvar, btnBuscar;

        public Form1()
        {
            InitializeNoSQL();
            SetupUI();
            ExibirResultados();
        }

        // Modelo NoSQL - Sem ID incremental
        public class Aluno
        {
            public string Matricula { get; set; }
            public string Nome { get; set; }
            public double Nota1 { get; set; }
            public double Nota2 { get; set; }
            public double Nota3 { get; set; }
            public double Nota4 { get; set; }
            
            // Propriedades calculadas para o Grid
            public double Media => (Nota1 + Nota2 + Nota3 + Nota4) / 4;
            public string Situacao => Media >= 7 ? "APROVADO" : "REPROVADO";
        }

        private void InitializeNoSQL()
        {
            if (!File.Exists(jsonPath))
            {
                File.WriteAllText(jsonPath, "[]");
            }
        }

        private List<Aluno> CarregarDados()
        {
            try
            {
                if (!File.Exists(jsonPath)) return new List<Aluno>();

                string json = File.ReadAllText(jsonPath);
                
                // Adicione esta opção para ignorar a diferença entre maiúsculas e minúsculas
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<List<Aluno>>(json, options) ?? new List<Aluno>();
            }
            catch 
            { 
                return new List<Aluno>(); 
            }
        }

        private void SalvarDados(List<Aluno> lista)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(lista, options);
            File.WriteAllText(jsonPath, json);
        }

        private void SetupUI()
        {
            this.Text = "Gestão Escolar)";
            this.Size = new Size(950, 700);
            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Segoe UI", 10);

            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 260, Padding = new Padding(20) };
            
            AddInputGroup(pnlInput, "Matrícula:", out txtMatricula, 10);
            AddInputGroup(pnlInput, "Nome:", out txtNome, 45);
            AddInputGroup(pnlInput, "Nota 1:", out txtNota1, 80);
            AddInputGroup(pnlInput, "Nota 2:", out txtNota2, 115);
            AddInputGroup(pnlInput, "Nota 3:", out txtNota3, 150);
            AddInputGroup(pnlInput, "Nota 4:", out txtNota4, 185);

            btnAdicionar = CreateButton("Adicionar", "#4CAF50", 320, 10, AdicionarAluno);
            btnExcluir = CreateButton("Excluir", "#E53935", 320, 55, ExcluirAluno);
            btnEditar = CreateButton("Editar", "#0288D1", 320, 100, IniciarEdicao);
            btnSalvar = CreateButton("Confirmar Edição", "#00897B", 320, 145, SalvarEdicao);

            Label lblBusca = new Label { Text = "Filtrar por Nome:", Location = new Point(550, 15), AutoSize = true };
            txtBusca = new TextBox { Location = new Point(550, 40), Width = 220 };
            btnBuscar = CreateButton("Buscar", "#757575", 550, 75, (s, e) => ExibirResultados(txtBusca.Text));

            pnlInput.Controls.AddRange(new Control[] { btnAdicionar, btnExcluir, btnEditar, btnSalvar, lblBusca, txtBusca, btnBuscar });

            dgvAlunos = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            this.Controls.Add(dgvAlunos);
            this.Controls.Add(pnlInput);
        }

        private void ExibirResultados(string filtro = "")
        {
            var lista = CarregarDados();

            var query = lista.AsQueryable();
            if (!string.IsNullOrWhiteSpace(filtro))
                query = query.Where(a => a.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase));

            dgvAlunos.DataSource = query.Select(a => new {
                a.Matricula,
                a.Nome,
                N1 = a.Nota1,
                N2 = a.Nota2,
                N3 = a.Nota3,
                N4 = a.Nota4,
                Média = a.Media.ToString("N1"),
                Situação = a.Situacao
            }).ToList();

            FormatarGrid();
        }

        private void AdicionarAluno(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMatricula.Text)) return;

            var lista = CarregarDados();
            if (lista.Any(a => a.Matricula == txtMatricula.Text))
            {
                MessageBox.Show("Esta matrícula já existe.");
                return;
            }

            lista.Add(new Aluno {
                Matricula = txtMatricula.Text,
                Nome = txtNome.Text,
                Nota1 = Convert.ToDouble(txtNota1.Text),
                Nota2 = Convert.ToDouble(txtNota2.Text),
                Nota3 = Convert.ToDouble(txtNota3.Text),
                Nota4 = Convert.ToDouble(txtNota4.Text)
            });

            SalvarDados(lista);
            ExibirResultados();
            LimparCampos();
        }

        private void IniciarEdicao(object sender, EventArgs e)
        {
            if (dgvAlunos.CurrentRow == null) return;

            alunoEditandoMatricula = dgvAlunos.CurrentRow.Cells["Matricula"].Value.ToString();
            var lista = CarregarDados();
            var aluno = lista.First(a => a.Matricula == alunoEditandoMatricula);

            txtMatricula.Text = aluno.Matricula;
            txtNome.Text = aluno.Nome;
            txtNota1.Text = aluno.Nota1.ToString();
            txtNota2.Text = aluno.Nota2.ToString();
            txtNota3.Text = aluno.Nota3.ToString();
            txtNota4.Text = aluno.Nota4.ToString();
        }

        private void SalvarEdicao(object sender, EventArgs e)
        {
            if (alunoEditandoMatricula == null) return;

            var lista = CarregarDados();
            var index = lista.FindIndex(a => a.Matricula == alunoEditandoMatricula);

            if (index != -1)
            {
                lista[index] = new Aluno {
                    Matricula = txtMatricula.Text,
                    Nome = txtNome.Text,
                    Nota1 = Convert.ToDouble(txtNota1.Text),
                    Nota2 = Convert.ToDouble(txtNota2.Text),
                    Nota3 = Convert.ToDouble(txtNota3.Text),
                    Nota4 = Convert.ToDouble(txtNota4.Text)
                };
                SalvarDados(lista);
            }

            alunoEditandoMatricula = null;
            ExibirResultados();
            LimparCampos();
        }

        private void ExcluirAluno(object sender, EventArgs e)
        {
            if (dgvAlunos.CurrentRow == null) return;
            string mat = dgvAlunos.CurrentRow.Cells["Matricula"].Value.ToString();

            var lista = CarregarDados();
            lista.RemoveAll(a => a.Matricula == mat);
            SalvarDados(lista);
            ExibirResultados();
        }

        // Helpers de UI
        private void AddInputGroup(Panel p, string labelText, out TextBox tb, int y)
        {
            p.Controls.Add(new Label { Text = labelText, Location = new Point(20, y), Width = 80 });
            tb = new TextBox { Location = new Point(110, y), Width = 180 };
            p.Controls.Add(tb);
        }

        private Button CreateButton(string text, string hex, int x, int y, EventHandler action)
        {
            Button btn = new Button { Text = text, Location = new Point(x, y), Size = new Size(160, 35), BackColor = ColorTranslator.FromHtml(hex), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btn.Click += action;
            return btn;
        }

        private void FormatarGrid()
        {
            foreach (DataGridViewRow row in dgvAlunos.Rows)
            {
                var sit = row.Cells["Situação"].Value?.ToString();
                row.Cells["Situação"].Style.ForeColor = (sit == "APROVADO") ? Color.Green : Color.Red;
                row.Cells["Situação"].Style.Font = new Font(dgvAlunos.Font, FontStyle.Bold);
            }
        }

        private void LimparCampos()
        {
            txtMatricula.Clear(); txtNome.Clear(); txtNota1.Clear();
            txtNota2.Clear(); txtNota3.Clear(); txtNota4.Clear();
            alunoEditandoMatricula = null;
        }
    }
}