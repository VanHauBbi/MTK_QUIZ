using DALTWNC_QUIZ.Models;
using System;
using System.IO;
using System.Text;

namespace DALTWNC_QUIZ.Patterns.Observer
{
    public class QuizLogObserver : IQuizObserver
    {
        public void OnQuizSubmitted(QuizAttempt attempt)
        {
            try
            {
                string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                
                string targetFolder = Path.Combine(projectRoot, "Patterns", "Observer");
                string filePath = Path.Combine(targetFolder, "quiz_history_log.txt");

                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                StringBuilder logContent = new StringBuilder();
                logContent.AppendLine("==================================================");
                logContent.AppendLine($"THỜI GIAN: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                logContent.AppendLine($"KẾT QUẢ: Attempt #{attempt.QuizAttemptID} | Customer: {attempt.CustomerID}");
                logContent.AppendLine($"ĐỀ THI: {attempt.QuizID} | ĐIỂM: {attempt.Score}/10");
                logContent.AppendLine("==================================================\n");

                File.AppendAllText(filePath, logContent.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi ghi log vào thư mục Pattern: " + ex.Message);
            }
        }
    }
}