using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using WarehouseApp.Models;
using WarehouseApp.Services;

namespace WarehouseApp
{
    public partial class RecoveryWindow : Window
    {
        private readonly WarehouseContext _db = WarehouseContext.GetContext();
        private string _pendingEmail = "";

        public RecoveryWindow() => InitializeComponent();

        private void SendCode_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RecEmail.Text))
            { RecMsg.Text = " Введите Email"; return; }

            if (!_db.Employees.Any(u => u.Email == RecEmail.Text))
            { RecMsg.Text = " Email не найден"; return; }

            _pendingEmail = RecEmail.Text;
            var code = AuthService.GenerateCode();
            AuthService.SendRecoveryEmail(RecEmail.Text, code);

            VerifyPanel.Visibility = Visibility.Visible;
            RecMsg.Text = " Код отправлен на почту.";
            RecMsg.Foreground = System.Windows.Media.Brushes.Green;
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RecCode.Text))
            { RecMsg.Text = " Введите код"; return; }

            if (Regex.IsMatch(NewPassword.Password, @"[а-яА-ЯёЁ]"))
            { RecMsg.Text = " Пароль не должен содержать русские буквы"; return; }

            if (string.IsNullOrWhiteSpace(NewPassword.Password) || NewPassword.Password.Length < 6)
            { RecMsg.Text = " Пароль должен быть ≥ 6 символов"; return; }

            var user = _db.Employees.FirstOrDefault(u => u.Email == _pendingEmail);
            if (user == null) { RecMsg.Text = " Пользователь не найден"; return; }

            user.PasswordHash = AuthService.HashPassword(NewPassword.Password);
            _db.SaveChanges();

            RecMsg.Text = " Пароль изменён. Выполните вход.";
            RecMsg.Foreground = System.Windows.Media.Brushes.Green;

            System.Threading.Thread.Sleep(1500);
            new MainWindow().Show();
            Close();
        }

        private void GoToLogin(object sender, MouseButtonEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }
    }
}