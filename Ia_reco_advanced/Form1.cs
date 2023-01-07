using IA_reco;

namespace Ia_reco_advanced
{
    public partial class Form1 : Form
    {
        camera mycam;
        public Form1()
        {
            InitializeComponent();
            mycam = new camera(this);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Ouvrir image",
                Filter = "img|*.png;*.jpg"
            };
            using (dialog)
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    IA iA = new();
                   changeimg(iA.go(dialog.FileName));
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mycam.prendrephotoAsync();
        }

        public async void changeimg(Bitmap newimg)
        {
            Task.Run(() => this.pictureBox1.Image = newimg);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            mycam.eteindre();
            mycam = null;
            Application.Exit();
        }
    }
}