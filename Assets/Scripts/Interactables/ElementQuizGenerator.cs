using System.Collections.Generic;
using UnityEngine;

public static class ElementQuizGenerator
{
    public static QuizQuestion GenerateQuestion(ElementData e)
    {
        List<QuizQuestion> pool = new List<QuizQuestion>();

        // --- BASIC QUESTIONS ---
        pool.Add(new QuizQuestion(
            $"What is the chemical symbol for {e.elementName}?",
            e.symbol));

        pool.Add(new QuizQuestion(
            $"{e.elementName} has how many valence electrons?",
            e.valenceElectrons.ToString()));

        pool.Add(new QuizQuestion(
            $"What is the atomic number of {e.elementName}?",
            e.atomicNumber.ToString()));

        pool.Add(new QuizQuestion(
            $"{e.elementName} has how many electron shells?",
            e.shellCount.ToString()));

        // --- ADVANCED QUESTIONS (only if applicable) ---
        if (e.elementName == "Nitrogen")
        {
            pool.Add(new QuizQuestion(
                "What element makes up about 78% of Earth's atmosphere?",
                "Nitrogen"));
        }

        if (e.elementName == "Carbon")
        {
            pool.Add(new QuizQuestion(
                "What element is the backbone of organic chemistry?",
                "Carbon"));
        }

        if (e.elementName == "Oxygen")
        {
            pool.Add(new QuizQuestion(
                "Which element is essential for respiration and combustion?",
                "Oxygen"));
        }

        if (e.elementName == "Silicon")
        {
            pool.Add(new QuizQuestion(
                "Which element is the backbone of rocks, sand, quartz, and computer chips?",
                "Silicon"));
        }

        if (e.elementName == "Aluminum")
        {
            pool.Add(new QuizQuestion(
                "Which element is the most abundant metal in Earth's crust?",
                "Aluminum"));
        }

        if (e.elementName == "Calcium")
        {
            pool.Add(new QuizQuestion(
                "Which element is the mineral of bones, shells, and limestone?",
                "Calcium"));
        }

        if (e.elementName == "Hydrogen")
        {
            pool.Add(new QuizQuestion(
                "Which element is the single most abundant element in the Universe?",
                "Hydrogen"));
        }

        if (e.elementName == "Copper")
        {
            pool.Add(new QuizQuestion(
                "Which element is known for its soft, malleable, and ductile properties, as well as being one of the few metals found in a form directly usable in nature? Hint: Many civilizations used this metal as their first tools.",
                "Copper"));
        }

        // --- FINAL STEP: PICK RANDOM VALID QUESTION ---
        return pool[Random.Range(0, pool.Count)];
    }
}