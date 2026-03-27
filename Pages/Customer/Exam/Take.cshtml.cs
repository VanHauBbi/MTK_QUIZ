using DALTWNC_QUIZ.Data;
using DALTWNC_QUIZ.Models;
using DALTWNC_QUIZ.Patterns.Decorator.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DALTWNC_QUIZ.Patterns.Structural; 

namespace DALTWNC_QUIZ.Pages.Customer.Exam
{
    public class TakeModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IQuizFacade _quizFacade; 

   
        public TakeModel(ApplicationDbContext context, IQuizFacade quizFacade)
        private readonly ISubmissionService _submissionService;
        public TakeModel(ApplicationDbContext context, ISubmissionService submissionService)
        {
            _context = context;
            _quizFacade = quizFacade;
            _submissionService = submissionService;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public DALTWNC_QUIZ.Models.Quiz Quiz { get; set; } = default!;
        public List<Question> Questions { get; set; } = new();
        public DALTWNC_QUIZ.Patterns.State.ITimerState TimerUIState { get; set; }
        public int Duration { get; set; }
        public int RemainingSeconds { get; set; }
        public int CurrentAttemptID { get; set; }

        public bool IsExamStarted { get; set; } = false;

        [BindProperty]
        public Dictionary<string, string> Answers { get; set; } = new();

        [BindProperty]
        public int ElapsedSeconds { get; set; }

        private int CalculateDuration(int totalQuestions)
        {
            int minutes = (int)Math.Ceiling(totalQuestions * 1.5);
            return minutes > 0 ? minutes : 15;
        }

        //public async Task<IActionResult> OnGetAsync()
        //{
        //    var username = User.FindFirstValue(ClaimTypes.Name);
        //    if (string.IsNullOrEmpty(username)) return RedirectToPage("/Account/Login");

        //    var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Username == username);
        //    if (customer == null) return RedirectToPage("/Index");

        //    Quiz = await _context.Quizzes.AsNoTracking().FirstOrDefaultAsync(q => q.QuizID == Id);
        //    if (Quiz == null) return NotFound();

        //    Duration = CalculateDuration(Quiz.TotalQuestions);

        //    var existingAttempt = await _context.QuizAttempts
        //        .Include(qa => qa.QuizAttemptQuestions).ThenInclude(qaq => qaq.Question).ThenInclude(q => q.Choices)
        //        .FirstOrDefaultAsync(qa => qa.QuizID == Id && qa.CustomerID == customer.CustomerID && qa.IsCompleted == false);

        //    if (existingAttempt != null)
        //    {
        //        IsExamStarted = true;
        //        CurrentAttemptID = existingAttempt.QuizAttemptID;

        //        Questions = existingAttempt.QuizAttemptQuestions
        //                        .OrderBy(q => q.QuizAttemptQuestionID)
        //                        .Select(q => q.Question)
        //                        .ToList();

        //        var timePassed = DateTime.UtcNow - existingAttempt.StartTime;
        //        int totalSecondsAllowed = Duration * 60;
        //        RemainingSeconds = totalSecondsAllowed - (int)timePassed.TotalSeconds;

        //        if (RemainingSeconds < 0) RemainingSeconds = 0;
        //    }
        //    else
        //    {
        //        IsExamStarted = false;
        //        RemainingSeconds = Duration * 60;
        //    }

        //    return Page();
        //}


        public async Task<IActionResult> OnGetAsync()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(username)) return RedirectToPage("/Account/Login");

            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Username == username);
            if (customer == null) return RedirectToPage("/Index");

            Quiz = await _context.Quizzes.AsNoTracking().FirstOrDefaultAsync(q => q.QuizID == Id);
            if (Quiz == null) return NotFound();

            Duration = CalculateDuration(Quiz.TotalQuestions);

            var existingAttempt = await _context.QuizAttempts
                .Include(qa => qa.QuizAttemptQuestions).ThenInclude(qaq => qaq.Question).ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(qa => qa.QuizID == Id && qa.CustomerID == customer.CustomerID && qa.IsCompleted == false);

            if (existingAttempt != null)
            {
                IsExamStarted = true;
                CurrentAttemptID = existingAttempt.QuizAttemptID;

                Questions = existingAttempt.QuizAttemptQuestions
                                .OrderBy(q => q.QuizAttemptQuestionID)
                                .Select(q => q.Question)
                                .ToList();

                var timePassed = DateTime.UtcNow - existingAttempt.StartTime;
                int totalSecondsAllowed = Duration * 60;
                RemainingSeconds = totalSecondsAllowed - (int)timePassed.TotalSeconds;

                if (RemainingSeconds < 0) RemainingSeconds = 0;
            }
            else
            {
                IsExamStarted = false;
                RemainingSeconds = Duration * 60;
            }

            
            
            double percentRemaining = (Duration > 0)
                ? ((double)RemainingSeconds / (Duration * 60)) * 100
                : 100;

            if (RemainingSeconds <= 60)
            {
                
                TimerUIState = new DALTWNC_QUIZ.Patterns.State.UrgentState();
            }
            else if (percentRemaining < 30) 
            {
                TimerUIState = new DALTWNC_QUIZ.Patterns.State.WarningState();
            }
            else
            {
                TimerUIState = new DALTWNC_QUIZ.Patterns.State.RelaxedState();
            }
           
            return Page();
        }

        public async Task<IActionResult> OnPostStartAsync()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizID == Id);

            if (customer == null || quiz == null) return RedirectToPage("/Index");

            List<int> questionIds;
            if (quiz.IsDynamic)
            {
                var random = new Random();
                var allIds = await _context.Questions.Where(q => q.SubjectID == quiz.SubjectID).Select(q => q.QuestionID).ToListAsync();
                int count = Math.Min(quiz.TotalQuestions, allIds.Count);
                questionIds = allIds.OrderBy(x => random.Next()).Take(count).ToList();
            }
            else
            {
                questionIds = await _context.QuizQuestions.Where(qq => qq.QuizID == Id).OrderBy(qq => qq.QuestionID).Select(qq => qq.QuestionID).ToListAsync();
            }

            var attempt = new QuizAttempt
            {
                CustomerID = customer.CustomerID,
                QuizID = Id,
                StartTime = DateTime.UtcNow,
                IsCompleted = false,
                TotalQuestions = questionIds.Count
            };
            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            var attemptQuestions = questionIds.Select(qId => new QuizAttemptQuestion
            {
                QuizAttemptID = attempt.QuizAttemptID,
                QuestionID = qId
            }).ToList();

            _context.QuizAttemptQuestions.AddRange(attemptQuestions);
            await _context.SaveChangesAsync();

            return RedirectToPage("Take", new { id = Id });
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!int.TryParse(Request.Form["CurrentAttemptID"], out int currentAttemptId))
            {
                return RedirectToPage("/Index");
            var currentAttemptIdStr = Request.Form["CurrentAttemptID"];
            if (string.IsNullOrEmpty(currentAttemptIdStr)) return RedirectToPage("/Index");

            int currentAttemptId = int.Parse(currentAttemptIdStr);
            int.TryParse(Request.Form["ElapsedSeconds"], out int elapsedSecondsFromForm);

            var username = User.FindFirstValue(ClaimTypes.Name);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);
            if (customer == null) return RedirectToPage("/Index");

            List<int> selectedChoiceIds = Answers.Values
                .Where(v => int.TryParse(v, out _))
                .Select(int.Parse)
                .ToList();

            
            var resultAttempt = _submissionService.ProcessSubmission(Id, customer.CustomerID, selectedChoiceIds);
            var draftAttempt = await _context.QuizAttempts.FindAsync(currentAttemptId);
            if (draftAttempt != null)
            {
                _context.QuizAttempts.Remove(draftAttempt);
            }
            int.TryParse(Request.Form["ElapsedSeconds"], out int elapsedSecondsFromForm);
            var result = await _quizFacade.SubmitQuizAsync(currentAttemptId, Answers, elapsedSecondsFromForm);
            if (result == null) return RedirectToPage("/Index");
           
            var finalAttempt = await _context.QuizAttempts.FindAsync(resultAttempt.QuizAttemptID);
            if (finalAttempt != null)
            {
                finalAttempt.DurationMinute = elapsedSecondsFromForm;
                finalAttempt.IsCompleted = true;
                await _context.SaveChangesAsync();
            }
            TempData["SuccessMessage"] = "Bạn đã nộp bài thành công!";
            return RedirectToPage("/Customer/Quiz_Result/Result", new { id = result.QuizAttemptID });
            return RedirectToPage("/Customer/Quiz_Result/Result", new { id = resultAttempt.QuizAttemptID });
        }
    }
}