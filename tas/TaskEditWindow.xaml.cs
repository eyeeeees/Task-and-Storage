using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using tas.Models;
using tas.Services;
using tas.Data;

namespace tas.UI
{
    public partial class TaskEditWindow : Window
    {
        private readonly TaskService _taskService;
        private readonly UserService _userService;
        private readonly User _currentUser;
        private readonly int? _editId;

        public TaskEditWindow(TaskService taskService, UserService userService, User currentUser, int? editId = null)
        {
            InitializeComponent();
            _taskService = taskService;
            _userService = userService;
            _currentUser = currentUser;
            _editId = editId;
            this.Loaded += TaskEditWindow_Loaded;
        }

        private async void TaskEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var users = await _userService.GetAllUsersAsync();
            cbExecutor.ItemsSource = users;
            cbExecutor.SelectedValuePath = "Id";
            cbExecutor.DisplayMemberPath = "Login";
            using (var context = new TasDbContext())
            {
                var statuses = await context.Statuses.ToListAsync();
                cbStatus.ItemsSource = statuses;
                cbStatus.SelectedValuePath = "Id";
                cbStatus.DisplayMemberPath = "Name";
            }
            if (_editId.HasValue)
            {
                var task = await _taskService.GetByIdAsync(_editId.Value);
                if (task != null)
                {
                    tbTitle.Text = task.Title;
                    tbDescription.Text = task.Description;
                    dpDeadline.SelectedDate = task.Deadline;
                    cbStatus.SelectedValue = task.StatusId;
                    cbExecutor.SelectedValue = task.UserId;
                }
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbTitle.Text))
            {
                MessageBox.Show("Название обязательно");
                return;
            }

            DateTime? deadlineUtc = null;
            if (dpDeadline.SelectedDate.HasValue)
            {
                deadlineUtc = DateTime.SpecifyKind(dpDeadline.SelectedDate.Value, DateTimeKind.Utc);
            }

            if (_editId.HasValue) 
            {
                var existing = await _taskService.GetByIdAsync(_editId.Value);
                if (existing != null)
                {
                    existing.Title = tbTitle.Text;
                    existing.Description = tbDescription.Text;
                    existing.Deadline = deadlineUtc;
                    existing.StatusId = (int)cbStatus.SelectedValue;
                    existing.UserId = (int?)cbExecutor.SelectedValue;
                    await _taskService.UpdateAsync(existing);
                }
            }
            else 
            {
                var task = new AppTask
                {
                    Title = tbTitle.Text,
                    Description = tbDescription.Text,
                    Deadline = deadlineUtc,
                    StatusId = (int)cbStatus.SelectedValue,
                    UserId = (int?)cbExecutor.SelectedValue,
                    CreatedAt = DateTime.UtcNow
                };
                await _taskService.AddAsync(task);
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}