namespace DALTWNC_QUIZ.Patterns.Creational
{
    public class QuizDirector
    {
        private readonly IQuizBuilder _builder;

        public QuizDirector(IQuizBuilder builder) => _builder = builder;

        // Tạo một đề thi mẫu nhanh với 1 câu hỏi ví dụ
        public void ConstructSampleQuiz(string title, string author, int subjectId)
        {
            _builder.SetBasicInfo(title, "Mô tả mặc định", author)
                    .ForSubject(subjectId)
                    .SetStatus(true, false);
        }
    }
}
