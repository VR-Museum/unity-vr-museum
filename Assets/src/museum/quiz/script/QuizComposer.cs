using System;
using System.Collections.Generic;
using src.museum.quiz.model;
using src.museum.quiz.model.item;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace src.museum.quiz.script
{
    public class QuizComposer : MonoBehaviour
    {
        public static Dictionary<GameObject, List<QuizItem>> SavedItems;
        public static List<QuizItem> AllItems;
        public static List<IndicatorMineralsPairItem> IndicatorsItems;
        
        private QuizEvent _nextEvent = QuizEvent.Check;
        private DateTime _lastCheck = DateTime.Now;
        private const float CheckDelay = 3f;

        public static QuizComposer _component = null;

        private void Awake()
        {
            SavedItems ??= new Dictionary<GameObject, List<QuizItem>>();
            IndicatorsItems ??= new List<IndicatorMineralsPairItem>();
            AllItems = new List<QuizItem>();
            if (_component == null)
            {
                _component = this;
            }
            else
            {
                throw new Exception("found more then one Quiz Composer");
            }
        }

        public void AddMineral(GameObject mineral)
        {
            if (mineral.TryGetComponent<QuizItem>(out var component))
            {
                foreach (var indicatorsItem in IndicatorsItems)
                {
                    if (indicatorsItem.UniqueExpectedMinerals.Contains(component.mineral))
                    {
                        if (!SavedItems.ContainsKey(indicatorsItem.Indicator))
                        {
                            SavedItems.Add(indicatorsItem.Indicator, new List<QuizItem>());
                        }
                        SavedItems[indicatorsItem.Indicator].Add(component);
                    }
                }
                AllItems.Add(component);
            }
        }

        public void Check()
        {
            if (!(DateTime.Now.Subtract(_lastCheck).TotalSeconds > CheckDelay)) return;
            _lastCheck = DateTime.Now;
            foreach (var savedItem in SavedItems)
            {
                switch (_nextEvent)
                {             
                    case QuizEvent.Check :
                        savedItem.Key.GetComponent<IndicatorQuizAssistant>().CheckMinerals();
                        foreach (var quizItem in savedItem.Value)
                        {
                            quizItem.MineralObject.GetComponent<MineralAsButton>().enabled = true;
                        }
                        break;
                    case QuizEvent.Reset :
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 0);
                        break;
                }
            }
            _nextEvent = (QuizEvent) ((int) (_nextEvent + 1) % 2);
        }

        private void OnDestroy()
        {
            SavedItems = new Dictionary<GameObject, List<QuizItem>>();
            IndicatorsItems = new List<IndicatorMineralsPairItem>();
            AllItems = new List<QuizItem>();
        }
    }
}