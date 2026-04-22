using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Chemist : MonoBehaviour
{
    public int rpReward = 1;
    static readonly string[] allowedElements =
    {
        "Hydrogen",
        "Carbon",
        "Nitrogen",
        "Oxygen",
        "Iron",
        "Copper",
        "Tin",
        "Aluminum",
        "Silicon",
        "Calcium",
        "Sulfur",
        "Gold"
    };
    public void Talk()
    {
        var gs = GameState.Instance;
        if (gs == null || gs.AllElements == null)
        {
            DialogUI.Show("The Chemist", "My notes are scattered today — come back later.");
            return;
        }

        var pick = PickAllowedElement(gs.AllElements);

        if (pick == null)
        {
            DialogUI.Show("The Chemist",
                "Nothing new I can teach you right now.");
            return;
        }

        QuizQuestion currentQuestion = ElementQuizGenerator.GenerateQuestion(pick);

        string title = $"Chemist Quiz";
        string body = currentQuestion.question;

        DialogUI.Show(title, body, OnAnswerSubmitted);

        void OnAnswerSubmitted(string playerAnswer)
        {
            if (playerAnswer.Trim().ToLower() ==
                currentQuestion.correctAnswer.ToLower())
            {
                gs.AddRP(1);
                DialogUI.Show("Correct!", "You have gained one research point.");
            }
            else
            {
                DialogUI.Show("Incorrect",
                    $"Correct answer: {currentQuestion.correctAnswer}");
            }
        }
    }

    static ElementData PickAllowedElement(ElementData[] all)
    {
        List<ElementData> valid = new List<ElementData>();

        foreach (var e in all)
        {
            if (e == null) continue;

            foreach (var name in allowedElements)
            {
                if (e.elementName == name)
                {
                    valid.Add(e);
                    break;
                }
            }
        }

        if (valid.Count == 0)
            return null;

        return valid[Random.Range(0, valid.Count)];
    }
}
