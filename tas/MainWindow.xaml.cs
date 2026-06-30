using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OfficeOpenXml;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using tas.Data;
using tas.DTOs;
using tas.Models;
using tas.Services;

namespace tas.UI
{
    public partial class MainWindow : Window
    {
        private readonly User _currentUser;
        private readonly TaskService _taskService;
        private readonly UserService _userService;
        private List<TaskDto> _allTasks;

        public MainWindow(User user)
{
    InitializeComponent();
    _currentUser = user;
    var context = new TasDbContext();
    _taskService = new TaskService(context);
    _userService = new UserService(context);
    this.Title = $"Учёт задач – {user.Login} ({user.Role})";

    // Настройка видимости кнопок по роли
    if (_currentUser.Role == "Admin")
    {
        btnEdit.Visibility = Visibility.Visible;
        btnDelit.Visibility = Visibility.Visible;
    }
    else
    {
        btnEdit.Visibility = Visibility.Collapsed;
        btnDelit.Visibility = Visibility.Collapsed;
    }

    // Загружаем статусы
    cbStatus.ItemsSource = context.Statuses.ToList();
    cbStatus.DisplayMemberPath = "Name";
    cbStatus.SelectedValuePath = "Id";

    LoadTasks();
}

        private async void LoadTasks()
        {
            var tasks = await _taskService.GetAllDtoAsync();
            _allTasks = tasks;
            dgTasks.ItemsSource = _allTasks;
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            var text = tbSearch.Text.Trim();
            if (string.IsNullOrEmpty(text))
                LoadTasks();
            else
            {
                var found = await _taskService.SearchByTitleAsync(text);
                dgTasks.ItemsSource = found.Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Deadline = t.Deadline,
                    CreatedAt = t.CreatedAt,
                    StatusName = t.Status?.Name,
                    ExecutorLogin = t.User?.Login,
                    UserId = t.UserId
                }).ToList();
            }
        }

        private async void Filter_Click(object sender, RoutedEventArgs e)
        {
            if (cbStatus.SelectedValue is int statusId)
            {
                var filtered = await _taskService.FilterByStatusAsync(statusId);
                dgTasks.ItemsSource = filtered.Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Deadline = t.Deadline,
                    CreatedAt = t.CreatedAt,
                    StatusName = t.Status?.Name,
                    ExecutorLogin = t.User?.Login,
                    UserId = t.UserId
                }).ToList();
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new TaskEditWindow(_taskService, _userService, _currentUser);
            if (editWindow.ShowDialog() == true)
                LoadTasks();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (dgTasks.SelectedItem is TaskDto selected)
            {
                if (_currentUser.Role != "Admin" && _currentUser.Id != selected.UserId)
                {
                    MessageBox.Show("Вы можете редактировать только свои задачи.", "Доступ запрещён");
                    return;
                }
                var editWindow = new TaskEditWindow(_taskService, _userService, _currentUser, selected.Id);
                if (editWindow.ShowDialog() == true)
                    LoadTasks();
            }
            else
                MessageBox.Show("Выберите задачу для редактирования.");
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (dgTasks.SelectedItem is TaskDto selected)
            {
                if (_currentUser.Role != "Admin" && _currentUser.Id != selected.UserId)
                {
                    MessageBox.Show("Вы можете удалять только свои задачи.", "Доступ запрещён");
                    return;
                }
                if (MessageBox.Show("Удалить задачу?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    await _taskService.DeleteAsync(selected.Id);
                    LoadTasks();
                }
            }
            else
                MessageBox.Show("Выберите задачу для удаления.");
        }

        private bool _isDark = false;

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            _isDark = !_isDark;
            App.SwitchTheme(_isDark ? "Dark" : "Light");
        }

        private async void ExportPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tasks = await _taskService.GetAllAsync();

                Settings.License = LicenseType.Community;

                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF файлы (*.pdf)|*.pdf",
                    FileName = "Задачи.pdf"
                };
                if (saveDialog.ShowDialog() != true) return;

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);
                        page.Size(PageSizes.A4);
                        page.Header().Text("Список задач").FontSize(20).Bold().AlignCenter();
                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Text("ID").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Название").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Описание").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Исполнитель").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Срок").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Статус").Bold();
                            });

                            foreach (var t in tasks)
                            {
                                table.Cell().Text(t.Id.ToString());
                                table.Cell().Text(t.Title ?? "");
                                table.Cell().Text(t.Description ?? "");
                                table.Cell().Text(t.User?.Login ?? "Не назначен");
                                table.Cell().Text(t.Deadline?.ToString("dd.MM.yyyy") ?? "");
                                table.Cell().Text(t.Status?.Name ?? "");
                            }
                        });
                        page.Footer().Text(x =>
                        {
                            x.Span("Сгенерировано: ");
                            x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                        });
                    });
                }).GeneratePdf(saveDialog.FileName);

                MessageBox.Show("Экспорт в PDF выполнен успешно!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var tasks = await _taskService.GetAllAsync();

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Задачи");

                    worksheet.Cells[1, 1].Value = "ID";
                    worksheet.Cells[1, 2].Value = "Название";
                    worksheet.Cells[1, 3].Value = "Описание";
                    worksheet.Cells[1, 4].Value = "Исполнитель";
                    worksheet.Cells[1, 5].Value = "Срок выполнения";
                    worksheet.Cells[1, 6].Value = "Статус";
                    worksheet.Cells[1, 7].Value = "Дата создания";

                    int row = 2;
                    foreach (var t in tasks)
                    {
                        worksheet.Cells[row, 1].Value = t.Id;
                        worksheet.Cells[row, 2].Value = t.Title;
                        worksheet.Cells[row, 3].Value = t.Description;
                        worksheet.Cells[row, 4].Value = t.User?.Login ?? "Не назначен";
                        worksheet.Cells[row, 5].Value = t.Deadline?.ToString("dd.MM.yyyy") ?? "";
                        worksheet.Cells[row, 6].Value = t.Status?.Name ?? "";
                        worksheet.Cells[row, 7].Value = t.CreatedAt.ToString("dd.MM.yyyy HH:mm");
                        row++;
                    }

                    worksheet.Cells.AutoFitColumns();

                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "Excel файлы (*.xlsx)|*.xlsx",
                        FileName = "Задачи.xlsx"
                    };
                    if (saveDialog.ShowDialog() == true)
                    {
                        File.WriteAllBytes(saveDialog.FileName, package.GetAsByteArray());
                        MessageBox.Show("Экспорт в Excel выполнен успешно!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}