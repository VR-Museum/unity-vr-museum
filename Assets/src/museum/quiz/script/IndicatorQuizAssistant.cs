using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using src.museum.quiz.model.item;
using TMPro;
using UnityEngine;

namespace src.museum.quiz.script
{
    public class IndicatorQuizAssistant : MonoBehaviour
    {
        private List<MineralName> _expectedMinerals;
        public GameObject mineralNameText;

        private readonly Dictionary<QuizItem, List<Collision>> _contactsMinerals = new Dictionary<QuizItem, List<Collision>>();

        public Renderer highlightRenderer;
        
        public Material success;
        public Material partialSuccess;
        public Material failure;
    
        public void SetLoadsText()
        {
            mineralNameText.GetComponent<TextMeshPro>().text = "Выбирается минерал...";
        }

        public void SetExpectedMinerals(List<MineralName> newExpectedMinerals)
        {
            _expectedMinerals = newExpectedMinerals;
            mineralNameText.GetComponent<TextMeshPro>().text = BuildString();
            QuizComposer.IndicatorsItems ??= new List<IndicatorMineralsPairItem>();
            QuizComposer.IndicatorsItems.Add(new IndicatorMineralsPairItem(gameObject, new HashSet<MineralName>(_expectedMinerals)));
        }

        public void Hide()
        {
            foreach (var collider in GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
            foreach (var light in GetComponentsInChildren<Light>())
            {
                light.enabled = false;
            }
        }
    
        public void CheckMinerals()
        {
            var requiredQuizItems = QuizComposer.SavedItems[gameObject];

            var containsCorrectMinerals = requiredQuizItems.Any(quizItem => _contactsMinerals.Keys.Contains(quizItem));
            var containsAllCorrectMinerals = requiredQuizItems.All(quizItem => _contactsMinerals.Keys.Contains(quizItem));
            var containsWrongMinerals = _contactsMinerals.Keys.Any(quizItem => !requiredQuizItems.Contains(quizItem));
            
            if (containsAllCorrectMinerals && !containsWrongMinerals)
            {
                highlightRenderer.material = success;
                return;
            }
            if (containsCorrectMinerals)
            {
                highlightRenderer.material = partialSuccess;
                return;
            }
            highlightRenderer.material = failure;
        }

        public void Reset() 
        {
            ColorSelf(Color.clear);
        }

        private void ColorSelf(Color newColor)
        {
            foreach (var t in gameObject.GetComponents<Renderer>())
            {
                t.material.color = newColor;
            }
        }

        private String BuildString()
        {
            StringBuilder builder = null;
            foreach (var mineralName in _expectedMinerals)
            {
                if (builder == null)
                {
                    builder = new StringBuilder(MineralNameConverter.Convert(mineralName));
                }
                else
                {
                    builder.Append(", ");
                    builder.Append(MineralNameConverter.Convert(mineralName));
                }
            }

            if (builder != null)
            {
                return builder.ToString();
            }
            else
            {
                throw new Exception("was not found mineral's names");
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out QuizItem quizItem))
            {
                if (_contactsMinerals.TryGetValue(quizItem, out var collisions))
                {
                    collisions.Add(collision);
                }
                else
                {
                    _contactsMinerals.Add(quizItem, new List<Collision>{collision});
                }
            }
        }

        
        private void OnCollisionExit(Collision other)
        {
            if (other.gameObject.TryGetComponent(out QuizItem quizItem))
            {
                var collisions = _contactsMinerals[quizItem];
                collisions.Remove(other);
                if (collisions.Count == 0)
                {
                    _contactsMinerals.Remove(quizItem);
                }
            }
        }
    }
}
