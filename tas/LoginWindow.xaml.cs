using System.Windows;
using tas.Services;
using tas.Data;

namespace tas.UI
{
    public partial class LoginWindow : Window
    {
        private readonly UserService _userService;

        public LoginWindow()
        {
            InitializeComponent();
            var context = new TasDbContext();
            _userService = new UserService(context);
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var user = await _userService.AuthenticateAsync(tbLogin.Text, pbPassword.Password);
            if (user != null)
            {
                var main = new MainWindow(user);
                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            var login = tbLogin.Text;
            var password = pbPassword.Password;
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните логин и пароль");
                return;
            }
            var success = await _userService.RegisterAsync(login, password);
            if (success)
                MessageBox.Show("Регистрация успешна! Теперь войдите.");
            else
                MessageBox.Show("Такой логин уже существует");
        }
    }
}