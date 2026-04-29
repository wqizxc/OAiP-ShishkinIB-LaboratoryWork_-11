using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace WarehouseApp.Services
{
    public static class AuthService
    {
        private const int Iterations = 3;
        private const int MemorySizeKb = 65536;
        private const int Parallelism = 1;

        public static string HashPassword(string pass)
        {
            if (string.IsNullOrWhiteSpace(pass)) throw new ArgumentException("Пароль пустой!");
            var salt = RandomNumberGenerator.GetBytes(16);

            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(pass))
            {
                Salt = salt,
                DegreeOfParallelism = Parallelism,
                Iterations = Iterations,
                MemorySize = MemorySizeKb
            };
            var hash = argon2.GetBytes(32);
            var comb = new byte[salt.Length + hash.Length];
            Buffer.BlockCopy(salt, 0, comb, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, comb, salt.Length, hash.Length);
            return Convert.ToBase64String(comb);
        }

        public static bool VerifyPassword(string pass, string hPass)
        {
            if (string.IsNullOrWhiteSpace(pass) || string.IsNullOrEmpty(hPass)) return false;
            var combined = Convert.FromBase64String(hPass);
            if (combined.Length < 16 + 32) return false;

            var salt = combined.Take(16).ToArray();
            var originalHash = combined.Skip(16).Take(32).ToArray();

            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(pass))
            {
                Salt = salt,
                DegreeOfParallelism = Parallelism,
                Iterations = Iterations,
                MemorySize = MemorySizeKb
            };
            var newHash = argon2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(originalHash, newHash);
        }

        public static string GenerateCode() => new Random().Next(100000, 999999).ToString();

        public static void SendRecoveryEmail(string toEmail, string code)
        {
            string smtpEmail = "ilyashishkin00@mail.ru";
            string smtpPassword = "b3bUXyeVrneoJKLeBPpb";

            using var client = new SmtpClient("smtp.mail.ru", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpEmail, smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpEmail, "Склад - Восстановление пароля"),
                Subject = "Код восстановления пароля",
                Body = $"<h2 style='font-family:Arial, sans-serif'>Ваш код подтверждения: <b style='color:#d9534f; font-size:28px'>{code}</b></h2>" +
                       $"<p style='font-family:Arial, sans-serif'>Код действителен в течение 10 минут.</p>" +
                       $"<p style='font-family:Arial, sans-serif; color:#7f8c8d'>Если вы не запрашивали восстановление пароля, проигнорируйте это письмо.</p>",
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8
            };

            mailMessage.To.Add(toEmail);
            client.Send(mailMessage);
        }
    }
}