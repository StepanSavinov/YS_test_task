using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LocationWeb.Server.Data;
using LocationWeb.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace LocationWeb.Server.Controllers
{
    public class FileProgectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FileProgectsController> _logger;
        private readonly IFileProgectService _fileProgectService;

        public FileProgectsController(
            ApplicationDbContext context,
            ILogger<FileProgectsController> logger,
            IFileProgectService fileProgectService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileProgectService = fileProgectService ?? throw new ArgumentNullException(nameof(fileProgectService));
        }

        // GET: FileProgects
        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = await _fileProgectService.GetAllFileProgectsAsync();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка файлов проектов");
                return View("Error", new ErrorViewModel 
                { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = "Не удалось загрузить список файлов"
                });
            }
        }

        // GET: FileProgects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Попытка просмотра деталей без указания ID");
                return NotFound();
            }

            try
            {
                var viewModel = await _fileProgectService.GetFileProgectDetailsAsync(id.Value);
                if (viewModel == null)
                {
                    _logger.LogWarning("Файл проекта с ID {Id} не найден", id);
                    return NotFound();
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении деталей файла проекта с ID {Id}", id);
                return View("Error", new ErrorViewModel 
                { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = $"Ошибка при загрузке файла проекта {id}"
                });
            }
        }

        // GET: FileProgects/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var viewModel = await _fileProgectService.GetCreateViewModelAsync();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при подготовке формы создания файла проекта");
                return View("Error", new ErrorViewModel 
                { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = "Ошибка при подготовке формы создания"
                });
            }
        }

        // POST: FileProgects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FileProgectCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await _fileProgectService.PrepareCreateViewModelAsync(viewModel);
                return View(viewModel);
            }

            try
            {
                var result = await _fileProgectService.CreateFileProgectAsync(viewModel);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Файл проекта успешно создан";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                await _fileProgectService.PrepareCreateViewModelAsync(viewModel);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании файла проекта");
                return View("Error", new ErrorViewModel 
                { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = "Ошибка при создании файла проекта"
                });
            }
        }

        // GET: FileProgects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Попытка редактирования без указания ID");
                return NotFound();
            }

            try
            {
                var viewModel = await _fileProgectService.GetEditViewModelAsync(id.Value);
                if (viewModel == null)
                {
                    _logger.LogWarning("Файл проекта с ID {Id} не найден для редактирования", id);
                    return NotFound();
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при подготовке формы редактирования файла проекта с ID {Id}", id);
                return View("Error", new ErrorViewModel 
                { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = $"Ошибка при загрузке файла проекта {id} для редактирования"
                });
            }
        }

        // POST: FileProgects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FileProgectEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                _logger.LogWarning("Несоответствие ID при редактировании: {RouteId} != {ModelId}", id, viewModel.Id);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await _fileProgectService.PrepareEditViewModelAsync(viewModel);
                return View(viewModel);
            }

            try
            {
                var result = await _fileProgectService.UpdateFileProgectAsync(viewModel);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Файл проекта успешно обновлён";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                await _fileProgectService.PrepareEditViewModelAsync(viewModel);
                return View(viewModel);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Конфликт параллелизма при редактировании файла проекта с ID {Id}", id);
                if (!await _fileProgectService.FileProgectExistsAsync(id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении файла проекта с ID {Id}", id);
                return View("Error", new ErrorViewModel 
                { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = $"Ошибка при обновлении файла проекта {id}"
                });
            }
        }

        // GET: FileProgects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Попытка просмотра формы удаления без указания ID");
                return NotFound();
            }

            try
            {
                var viewModel = await _fileProgectService.GetDeleteViewModelAsync(id.Value);
                if (viewModel == null)
                {
                    _logger.LogWarning("Файл проекта с ID {Id} не найден для удаления", id);
                    return NotFound();
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при подготовке формы удаления файла проекта с ID {Id}", id);
                return View("Error", new ErrorViewModel 
                { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = $"Ошибка при загрузке файла проекта {id} для удаления"
                });
            }
        }

        // POST: FileProgects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _fileProgectService.DeleteFileProgectAsync(id);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Файл проекта успешно удалён";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении файла проекта с ID {Id}", id);
                return View("Error", new ErrorViewModel 
                { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = $"Ошибка при удалении файла проекта {id}"
                });
            }
        }

        // POST: FileProgects/UpdateFileUrl/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFileUrl(int id, [Bind("UrlFile")] FileProgectUrlUpdateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var fileProgect = await _fileProgectService.GetFileProgectForUpdateAsync(id);
                if (fileProgect == null)
                {
                    _logger.LogWarning("Файл проекта с ID {Id} не найден для обновления URL", id);
                    return NotFound();
                }
                
                return View("Edit", fileProgect);
            }

            try
            {
                var result = await _fileProgectService.UpdateFileUrlAsync(id, viewModel.UrlFile);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "URL файла успешно обновлён";
                    return RedirectToAction(nameof(Edit), new { id });
                }

                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Edit), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении URL файла проекта с ID {Id}", id);
                return View("Error", new ErrorViewModel 
                { 
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = $"Ошибка при обновлении URL файла проекта {id}"
                });
            }
        }
    }

    // Интерфейс сервиса для инверсии зависимостей
    public interface IFileProgectService
    {
        Task<IEnumerable<FileProgectViewModel>> GetAllFileProgectsAsync();
        Task<FileProgectDetailsViewModel> GetFileProgectDetailsAsync(int id);
        Task<FileProgectCreateViewModel> GetCreateViewModelAsync();
        Task PrepareCreateViewModelAsync(FileProgectCreateViewModel viewModel);
        Task<OperationResult> CreateFileProgectAsync(FileProgectCreateViewModel viewModel);
        Task<FileProgectEditViewModel> GetEditViewModelAsync(int id);
        Task PrepareEditViewModelAsync(FileProgectEditViewModel viewModel);
        Task<OperationResult> UpdateFileProgectAsync(FileProgectEditViewModel viewModel);
        Task<FileProgectDeleteViewModel> GetDeleteViewModelAsync(int id);
        Task<OperationResult> DeleteFileProgectAsync(int id);
        Task<bool> FileProgectExistsAsync(int id);
        Task<FileProgectEditViewModel> GetFileProgectForUpdateAsync(int id);
        Task<OperationResult> UpdateFileUrlAsync(int id, string urlFile);
    }

    // Модели представлений
    public class FileProgectViewModel
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public string UrlFile { get; set; }
    }

    public class FileProgectDetailsViewModel
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public string UrlFile { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class FileProgectCreateViewModel
    {
        [Required]
        [Display(Name = "Проект")]
        public int ProjectId { get; set; }

        [Required]
        [Url]
        [Display(Name = "URL файла")]
        public string UrlFile { get; set; }

        public SelectList Projects { get; set; }
    }

    public class FileProgectEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Проект")]
        public int ProjectId { get; set; }

        [Required]
        [Url]
        [Display(Name = "URL файла")]
        public string UrlFile { get; set; }

        public SelectList Projects { get; set; }
    }

    public class FileProgectDeleteViewModel
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public string UrlFile { get; set; }
    }

    public class FileProgectUrlUpdateViewModel
    {
        [Required]
        [Url]
        [Display(Name = "Новый URL файла")]
        public string UrlFile { get; set; }
    }

    // Результат операций
    public class OperationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}