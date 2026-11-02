using System.Linq;
using System.Windows;
using WpfApp1.Models;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void B1_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new RestoranContext())
            {
                var u = db.Users.FirstOrDefault(x =>
                    x.Login == LoginBox.Text.Trim() &&
                    x.Parol == ParolBox.Password);

                if (u == null)
                {
                    ErrorText.Text = "Неверный логин или пароль";
                    return;
                }

                new MenuWindow(u.Id, (u.Fio ?? "—").Trim(), (u.Rol ?? "ofitsiant").Trim()).Show();
                this.Close();
            }
        }
    }
}