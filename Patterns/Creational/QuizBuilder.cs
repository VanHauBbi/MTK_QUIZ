using DALTWNC_QUIZ.Models;

namespace DALTWNC_QUIZ.Patterns.Creational
{
    public class QuizBuilder : IQuizBuilder
    {
        private Quiz _quiz = new();

        public QuizBuilder() => Reset();

        public void Reset()
        {
            _quiz = new Quiz
            {
                CreatedDate = DateTime.Now,
                QuizQuestions = new List<QuizQuestion>(),
                IsPublic = true,
                IsDynamic = false
            };
        }

        public IQuizBuilder SetBasicInfo(string title, string? description, string createdBy)
        {
            _quiz.QuizTitle = title;
            _quiz.Description = description;
            _quiz.CreatedBy = createdBy;
            return this;
        }

        public IQuizBuilder ForSubject(int subjectId)
        {
            _quiz.SubjectID = subjectId;
            return this;
        }

        public IQuizBuilder SetStatus(bool isPublic, bool isDynamic)
        {
            _quiz.IsPublic = isPublic;
            _quiz.IsDynamic = isDynamic;
            return this;
        }

        public IQuizBuilder AddQuestion(Question question)
        {
            // Vì Quiz và Question quan hệ qua bảng trung gian QuizQuestion
            _quiz.QuizQuestions.Add(new QuizQuestion { Question = question });
            return this;
        }

        public Quiz Build()
        {
            _quiz.TotalQuestions = _quiz.QuizQuestions.Count;
            var result = _quiz;
            Reset(); // Chuẩn bị cho lần tạo tiếp theo
            return result;
        }
    }
}
