using System.Linq;
using System.Windows;
using System.Windows.Input;
using WarehouseApp.Services;

namespace WarehouseApp
{
    public partial class MainWindow : Window
    {
        private readonly WarehouseContext _db = WarehouseContext.GetContext();

        public MainWindow() => InitializeComponent();

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginEmail.Text))
            { LoginMsg.Text = "❌ Введите Email"; return; }

            var user = _db.Employees.FirstOrDefault(u => u.Email == LoginEmail.Text);
            if (user == null || !AuthService.VerifyPassword(LoginPassword.Password, user.PasswordHash))
            { LoginMsg.Text = "❌ Неверный Email или пароль"; return; }

            new DashboardWindow(user.Id).Show();
            Close();
        }

        private void GoToRegistration(object sender, MouseButtonEventArgs e)
        {
            new RegistrationWindow().Show();
            Close();
        }

        private void GoToRecovery(object sender, MouseButtonEventArgs e)
        {
            new RecoveryWindow().Show();
            Close();
        }
    }
}