using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using tas.Data;
using tas.DTOs;
using tas.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace tas.Services
{
    public class TaskService
    {
        private readonly TasDbContext _context;

        public TaskService(TasDbContext context) => _context = context;

        public async Task<List<AppTask>> GetAllAsync() =>
            await _context.Tasks
                .Include(t => t.Status)
                .Include(t => t.User)
                .ToListAsync();

        public async Task<AppTask> GetByIdAsync(int id) =>
            await _context.Tasks
                .Include(t => t.Status)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

        public async Task AddAsync(AppTask task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AppTask task)
        {          
            var existing = await _context.Tasks.FindAsync(task.Id);
            if (existing != null)
            {
                existing.Title = task.Title;
                existing.Description = task.Description;
                existing.Deadline = task.Deadline;
                existing.StatusId = task.StatusId;
                existing.UserId = task.UserId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<AppTask>> SearchByTitleAsync(string title) =>
            await _context.Tasks
                .Where(t => t.Title.ToLower().Contains(title.ToLower()))
                .Include(t => t.Status)
                .Include(t => t.User)
                .ToListAsync();

        public async Task<List<AppTask>> FilterByStatusAsync(int statusId) =>
            await _context.Tasks
                .Where(t => t.StatusId == statusId)
                .Include(t => t.Status)
                .Include(t => t.User)
                .ToListAsync();

        public async Task<List<TaskDto>> GetAllDtoAsync()
        {
            var tasks = await GetAllAsync();
            return tasks.Select(t => new TaskDto
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

        public async Task<byte[]> ExportToExcelAsync()
        {
            var tasks = await GetAllDtoAsync();
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Задачи");

            // Заголовки
            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Название";
            worksheet.Cells[1, 3].Value = "Описание";
            worksheet.Cells[1, 4].Value = "Исполнитель";
            worksheet.Cells[1, 5].Value = "Срок";
            worksheet.Cells[1, 6].Value = "Статус";
            worksheet.Cells[1, 7].Value = "Создана";

            // Данные
            for (int i = 0; i < tasks.Count; i++)
            {
                var t = tasks[i];
                worksheet.Cells[i + 2, 1].Value = t.Id;
                worksheet.Cells[i + 2, 2].Value = t.Title;
                worksheet.Cells[i + 2, 3].Value = t.Description;
                worksheet.Cells[i + 2, 4].Value = t.ExecutorLogin;
                worksheet.Cells[i + 2, 5].Value = t.Deadline?.ToString("dd.MM.yyyy");
                worksheet.Cells[i + 2, 6].Value = t.StatusName;
                worksheet.Cells[i + 2, 7].Value = t.CreatedAt.ToString("dd.MM.yyyy");
            }

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportToPdfAsync()
        {
            var tasks = await GetAllDtoAsync();
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.Header().Text("Список задач").FontSize(20).Bold();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.5f); // ID
                            columns.RelativeColumn(2);    // Название
                            columns.RelativeColumn(3);    // Описание
                            columns.RelativeColumn(1.5f); // Исполнитель
                            columns.RelativeColumn(1.5f); // Срок
                            columns.RelativeColumn(1.5f); // Статус
                        });

                        // Заголовки
                        table.Header(header =>
                        {
                            header.Cell().Text("ID").Bold();
                            header.Cell().Text("Название").Bold();
                            header.Cell().Text("Описание").Bold();
                            header.Cell().Text("Исполнитель").Bold();
                            header.Cell().Text("Срок").Bold();
                            header.Cell().Text("Статус").Bold();
                        });

                        // Данные
                        foreach (var t in tasks)
                        {
                            table.Cell().Text(t.Id.ToString());
                            table.Cell().Text(t.Title);
                            table.Cell().Text(t.Description ?? "");
                            table.Cell().Text(t.ExecutorLogin ?? "");
                            table.Cell().Text(t.Deadline?.ToString("dd.MM.yyyy") ?? "");
                            table.Cell().Text(t.StatusName ?? "");
                        }
                    });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }
    }


}