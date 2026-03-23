using DALTWNC_QUIZ.Data;
using DALTWNC_QUIZ.Models;
using DALTWNC_QUIZ.Patterns.Adapter;
using DALTWNC_QUIZ.Patterns.Creational;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DALTWNC_QUIZ.Pages.Admin.QuestionsBank
{
    [Authorize(Roles = "A")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IQuestionAdapter _adapter;

        public CreateModel(ApplicationDbContext context, IQuestionAdapter adapter)
        {
            _context = context;
            _adapter = adapter;
        }

        [BindProperty]
        public IFormFile ExcelFile { get; set; }

        public async Task<IActionResult> OnPostImportExcelAsync(int importSubjectId)
        {
            if (ExcelFile == null || ExcelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file Excel hợp lệ.";
                return RedirectToPage();
            }

            if (importSubjectId == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn môn học trước khi Import.";
                return RedirectToPage();
            }

            ExcelPackage.License.SetNonCommercialPersonal("MyQuizApp");

            try
            {
                var categories = await _context.QuestionCategories.ToListAsync();

                using (var stream = new MemoryStream())
                {
                    await ExcelFile.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension?.Rows ?? 0;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var ext = new ExternalQuestion
                            {
                                Text = worksheet.Cells[row, 1].Text,
                                Type = worksheet.Cells[row, 2].Text,
                                CategoryName = worksheet.Cells[row, 3].Text?.Trim(),
                                AnswerA = worksheet.Cells[row, 4].Text,
                                AnswerB = worksheet.Cells[row, 5].Text,
                                AnswerC = worksheet.Cells[row, 6].Text,
                                AnswerD = worksheet.Cells[row, 7].Text,
                                CorrectAnswer = worksheet.Cells[row, 8].Text
                            };

                            var question = _adapter.Convert(ext);
                            question.SubjectID = importSubjectId;

                            if (!string.IsNullOrEmpty(ext.CategoryName))
                            {
                                var existingCate = categories.FirstOrDefault(c =>
                                    c.CategoryName.Equals(ext.CategoryName, StringComparison.OrdinalIgnoreCase));

                                if (existingCate != null)
                                {
                                    question.CategoryID = existingCate.CategoryID;
                                }
                                else
                                {
                                    var newCate = new QuestionCategory { CategoryName = ext.CategoryName };
                                    _context.QuestionCategories.Add(newCate);
                                    await _context.SaveChangesAsync();

                                    categories.Add(newCate);
                                    question.CategoryID = newCate.CategoryID;
                                }
                            }

                            _context.Questions.Add(question);
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["SuccessMessage"] = "Import thành công! Các danh mục mới đã tự động được khởi tạo.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi xử lý: " + ex.Message;
                return RedirectToPage();
            }
        }

        [BindProperty]
        public Question Question { get; set; } = new();

        [BindProperty]
        public List<ChoiceInputModel> Choices { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public QuestionType SelectedQType { get; set; } = QuestionType.Standard;

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn một đáp án đúng cho câu hỏi.")]
        public int? CorrectChoiceIndex { get; set; }

        public SelectList SubjectSL { get; set; }
        public SelectList CategorySL { get; set; }

        public class ChoiceInputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập nội dung đáp án.")]
            public string ChoiceText { get; set; } = string.Empty;
            public bool IsCorrect { get; set; }
        }

        private async Task LoadDependenciesAsync()
        {
            SubjectSL = new SelectList(await _context.Subjects.AsNoTracking().ToListAsync(), "SubjectID", "SubjectName");
            CategorySL = new SelectList(await _context.QuestionCategories.AsNoTracking().ToListAsync(), "CategoryID", "CategoryName");
        }

        public async Task OnGetAsync()
        {
            ViewData["ActivePage"] = "QuestionsBank";
            await LoadDependenciesAsync();

            IQuestionFactory factory = QuestionCreator.GetFactory(SelectedQType);

            QuestionSetup setup = factory.CreateTemplate();

            Question = setup.Question;
            Choices = setup.Choices;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadDependenciesAsync();
            ModelState.Remove("Question.Subject");
            ModelState.Remove("Question.QuestionCategory");
            

            var validChoices = Choices.Where(c => !string.IsNullOrWhiteSpace(c.ChoiceText)).ToList();
            if (validChoices.Count < 2)
                ModelState.AddModelError(string.Empty, "Phải có ít nhất hai đáp án có nội dung.");

            if (!ModelState.IsValid)
            {
                if (CorrectChoiceIndex.HasValue) CorrectChoiceIndex = CorrectChoiceIndex.Value;
                return Page();
            }

            Choices.ForEach(c => c.IsCorrect = false);

            if (CorrectChoiceIndex.HasValue && CorrectChoiceIndex.Value >= 0 && CorrectChoiceIndex.Value < Choices.Count)
            {
                Choices[CorrectChoiceIndex.Value].IsCorrect = true;
            }

            validChoices = Choices.Where(c => !string.IsNullOrWhiteSpace(c.ChoiceText)).ToList();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Questions.Add(Question);
                    await _context.SaveChangesAsync();

                    foreach (var choiceInput in validChoices)
                    {
                        _context.Choices.Add(new Choice
                        {
                            QuestionID = Question.QuestionID,
                            ChoiceText = choiceInput.ChoiceText,
                            IsCorrect = choiceInput.IsCorrect
                        });
                    }
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    TempData["SuccessMessage"] = "Tạo câu hỏi mới thành công!";
                    return RedirectToPage("./Index");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi lưu: " + ex.Message);
                    return Page();
                }
            }
        }
    }
}