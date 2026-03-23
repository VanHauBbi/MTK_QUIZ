using DALTWNC_QUIZ.Models;
using System.Collections.Generic;

namespace DALTWNC_QUIZ.Patterns.Behavioral
{
    public interface IScoringStrategy
    {
        // Trả về Tuple gồm: (Số câu đúng, Điểm số)
        (int CorrectCount, decimal Score) Calculate(ICollection<QuizAttemptQuestion> attemptQuestions, int totalQuestions);
    }
}