namespace bài_4._6
{
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.IO;
    using System.Text;
    using System.Drawing;

    public partial class Form1 : Form
    {
        private BindingList<Student> students;
        private BindingSource bs;

        public Form1()
        {
            InitializeComponent();
            InitializeDataBinding();
        }

        private void InitializeDataBinding()
        {
            students = new BindingList<Student>();
            bs = new BindingSource();
            bs.DataSource = students;

            // DataGridView -> BindingSource
            dgvStudents.DataSource = bs;

            // Controls -> BindingSource (two-way)
            txtStudentId.DataBindings.Add("Text", bs, "StudentId", true, DataSourceUpdateMode.OnPropertyChanged);
            txtFullName.DataBindings.Add("Text", bs, "FullName", true, DataSourceUpdateMode.OnPropertyChanged);
            txtAge.DataBindings.Add("Text", bs, "Age", true, DataSourceUpdateMode.OnPropertyChanged);
            cmbGender.DataBindings.Add("Text", bs, "Gender", true, DataSourceUpdateMode.OnPropertyChanged);

            // Events
            bs.CurrentChanged += Bs_CurrentChanged;

            // sample data
            students.Add(new Student { StudentId = "SV001", FullName = "Nguyễn Văn A", Age = 20, Gender = "Nam", AvatarPath = string.Empty });
            students.Add(new Student { StudentId = "SV002", FullName = "Trần Thị B", Age = 21, Gender = "Nữ", AvatarPath = string.Empty });
        }

        private void Bs_CurrentChanged(object? sender, System.EventArgs e)
        {
            LoadCurrentAvatar();
        }

        private void LoadCurrentAvatar()
        {
            var s = bs.Current as Student;
            if (s == null)
            {
                pbAvatar.Image = null;
                return;
            }

            if (string.IsNullOrEmpty(s.AvatarPath) || !File.Exists(s.AvatarPath))
            {
                // clear image
                if (pbAvatar.Image != null)
                {
                    var old = pbAvatar.Image;
                    pbAvatar.Image = null;
                    old.Dispose();
                }
                return;
            }

            try
            {
                // Avoid locking file by creating a copy of the image
                using (var fs = File.OpenRead(s.AvatarPath))
                {
                    var img = Image.FromStream(fs);
                    if (pbAvatar.Image != null)
                    {
                        var old = pbAvatar.Image;
                        pbAvatar.Image = new Bitmap(img);
                        old.Dispose();
                    }
                    else
                    {
                        pbAvatar.Image = new Bitmap(img);
                    }
                }
            }
            catch
            {
                // ignore load errors
                if (pbAvatar.Image != null)
                {
                    var old = pbAvatar.Image;
                    pbAvatar.Image = null;
                    old.Dispose();
                }
            }
        }

        private void btnChooseAvatar_Click(object sender, EventArgs e)
        {
            if (bs.Current is not Student s) return;

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    s.AvatarPath = ofd.FileName;
                    LoadCurrentAvatar();
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var s = new Student();
            s.StudentId = txtStudentId.Text.Trim();
            s.FullName = txtFullName.Text.Trim();
            if (!int.TryParse(txtAge.Text.Trim(), out var age)) age = 0;
            s.Age = age;
            s.Gender = cmbGender.Text;
            s.AvatarPath = string.Empty;
            students.Add(s);
            bs.Position = students.Count - 1;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (bs.Current != null)
            {
                bs.RemoveCurrent();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (bs.Current is not Student s) return;
            s.StudentId = txtStudentId.Text.Trim();
            s.FullName = txtFullName.Text.Trim();
            if (!int.TryParse(txtAge.Text.Trim(), out var age)) age = 0;
            s.Age = age;
            s.Gender = cmbGender.Text;
            // AvatarPath already set when choosing image
            // Refresh grid
            dgvStudents.Refresh();
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var sw = new StreamWriter(sfd.FileName, false, new UTF8Encoding(true)))
                        {
                            // Header
                            sw.WriteLine("StudentId,FullName,Age,Gender,AvatarPath");
                            foreach (var st in students)
                            {
                                sw.WriteLine(string.Join(",", EscapeCsv(st.StudentId), EscapeCsv(st.FullName), st.Age.ToString(), EscapeCsv(st.Gender), EscapeCsv(st.AvatarPath)));
                            }
                        }
                        MessageBox.Show("Export completed", "CSV Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Export failed: " + ex.Message, "CSV Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private static string EscapeCsv(string? field)
        {
            if (field == null) return "";
            if (field.Contains('"') || field.Contains(',') || field.Contains(';') || field.Contains('\n') || field.Contains('\r'))
            {
                return '"' + field.Replace("\"", "\"\"") + '"';
            }
            return field;
        }
    }
}
