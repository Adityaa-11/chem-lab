public class QuizQuestion
{
    public string question;
    public string correctAnswer;
    public string[] choices;   // optional multiple choice

    public QuizQuestion(string q, string correct, string[] options = null)
    {
        question = q;
        correctAnswer = correct;
        choices = options;
    }
}