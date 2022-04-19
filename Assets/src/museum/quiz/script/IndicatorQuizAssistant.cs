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
        public float activatingDistance = 0.3f;
        public GameObject mineralNameText;
        
        public Renderer highlightRenderer;
        
        public Material success;
        public Material partialSuccess;
        public Material failure;
    
        void Start()
        {
            ColorSelf(Color.clear);
        }

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
        }
    
        public void CheckMinerals()
        {
            var wrongQuizItems = new List<QuizItem>(QuizComposer.AllItems);
            var requiredQuizItems = QuizComposer.SavedItems[gameObject];
            wrongQuizItems.RemoveAll(quizItem => requiredQuizItems.Contains(quizItem));
        
            var mineralIsHere = requiredQuizItems.Any(quizItem => CalculateDistance(transform.position, quizItem.mineralCenter.position) < activatingDistance);

            var otherMineralsAreNotHere = !wrongQuizItems.Any(quizItem => 
                CalculateDistance(transform.position, quizItem.mineralCenter.position) < activatingDistance);
        
            if (mineralIsHere && otherMineralsAreNotHere)
            {
                ColorSelf(Color.green);
                highlightRenderer.material = success;
                return;
            }

            if (mineralIsHere)
            {
                ColorSelf(Color.yellow);
                highlightRenderer.material = partialSuccess;
            }
            
            ColorSelf(Color.red);
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

        private float CalculateDistance(Vector3 firstPosition, Vector3 secondPosition)
        {
            var otherDx = firstPosition.x - secondPosition.x;
            var otherDy = firstPosition.y - secondPosition.y;
            var otherDz = firstPosition.z - secondPosition.z;
            return (float) Math.Sqrt(otherDx * otherDx + otherDy * otherDy + otherDz * otherDz);
        }
    }
}
