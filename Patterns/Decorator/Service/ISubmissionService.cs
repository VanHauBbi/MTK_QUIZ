using DALTWNC_QUIZ.Models;
using System.Collections.Generic;
namespace DALTWNC_QUIZ.Patterns.Decorator.Service
{
    public interface ISubmissionService
    {
        QuizAttempt ProcessSubmission(int quizId, int customerId, List<int> selectedChoiceIds);
    }
}
