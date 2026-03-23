using DALTWNC_QUIZ.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DALTWNC_QUIZ.Patterns.Behavioral
{
    public class StandardScoringStrategy : IScoringStrategy
    {
        public (int CorrectCount, decimal Score) Calculate(ICollection<QuizAttemptQuestion> attemptQuestions, int totalQuestions)
        {
            if (totalQuestions == 0) return (0, 0);

            int correctCount = 0;

            foreach (var aq in attemptQuestions)
            {
                // Tìm xem đáp án user chọn có đúng không dựa vào danh sách Choices
                if (aq.SelectedChoiceID.HasValue)
                {
                    var selectedChoice = aq.Question?.Choices?.FirstOrDefault(c => c.ChoiceID == aq.SelectedChoiceID.Value);
                    if (selectedChoice != null && selectedChoice.IsCorrect)
                    {
                        correctCount++;
                    }
                }
            }

            // Tính điểm hệ số 10
            decimal score = ((decimal)correctCount / totalQuestions) * 10;

            return (correctCount, Math.Round(score, 2)); // Làm tròn 2 chữ số thập phân
        }
    }
}