using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using WarehouseApp.Models;
using WarehouseApp.Services;

namespace WarehouseApp
{
    public partial class RegistrationWindow : Window
    {
        private readonly WarehouseContext _db = WarehouseContext.GetContext();

        public RegistrationWindow() => InitializeComponent();

        private string ValidateRegistration()
        {
            if (string.IsNullOrWhiteSpace(RegFirstName.Text)) return "Введите имя";
            if (string.IsNullOrWhiteSpace(RegLastName.Text)) return "Введите фамилию";
            if (RegDOB.SelectedDate > DateTime.Today) return "Дата рождения не может быть в будущем";
            if (!Regex.IsMatch(RegEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) return "Некорректный формат Email";
            if (!Regex.IsMatch(RegPhone.Text, @"^\+?[0-9\s\-\(\)]{7,18}$")) return "Некорректный номер телефона";
            if (string.IsNullOrWhiteSpace(RegPosition.Text)) return "Укажите должность";

            if (Regex.IsMatch(RegPassword.Password, @"[а-яА-ЯёЁ]"))
                return "Пароль не должен содержать русские буквы";

            if (RegPassword.Password.Length < 6) return "Пароль должен содержать минимум 6 символов";
            if (!RegPassword.Password.Any(char.IsUpper) || !RegPassword.Password.Any(char.IsDigit))
                return "Пароль должен содержать заглавную букву и цифру";

            return null;
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string err = ValidateRegistration();
            if (err != null) { RegMsg.Text = err; return; }

            var user = new Employee
            {
                FirstName = RegFirstName.Text,
                LastName = RegLastName.Text,
                BirthDate = RegDOB.SelectedDate ?? DateTime.Today,
                Email = RegEmail.Text,
                Phone = RegPhone.Text,
                Position = RegPosition.Text,
                PasswordHash = AuthService.HashPassword(RegPassword.Password)
            };

            _db.Employees.Add(user);
            _db.SaveChanges();

            RegMsg.Text = " Регистрация успешна!";
            RegFirstName.Text = RegLastName.Text = RegEmail.Text = RegPhone.Text = RegPosition.Text = "";
            RegPassword.Password = "";
        }

        private void GoToLogin(object sender, MouseButtonEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }
    }
}