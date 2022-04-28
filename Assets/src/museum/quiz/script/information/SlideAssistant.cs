using System;
using System.Collections.Generic;
using System.Linq;
using src.museum.quiz.model.item;
using TMPro;
using UnityEngine;

namespace src.museum.quiz.script.random
{
    public class SlideAssistant : MonoBehaviour
    {
        public GameObject mineralName;
        private Dictionary<GameObject, KeyValuePair<Boolean, HashSet<Collider>>> _enteredMinerals;

        private void Start()
        {
            _enteredMinerals ??= new Dictionary<GameObject, KeyValuePair<bool, HashSet<Collider>>>();
        }

        private void OnTriggerEnter(Collider other)
        {
            var mineral = other.gameObject;
            if (mineral.TryGetComponent<MineralAsButton>(out var component))
            {
                var mineralQuizItem = mineral.GetComponent<QuizItem>();
                mineralName.GetComponent<TextMeshPro>().text = MineralNameConverter.Convert(mineralQuizItem.mineral);
                if (_enteredMinerals.TryGetValue(mineral, out var information))
                {
                    information.Value.Add(other);
                }
                else
                {
                    _enteredMinerals.Add(mineral, new KeyValuePair<bool, HashSet<Collider>>(component.enabled, new HashSet<Collider>{other}));
                }
                component.enabled = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var mineral = other.gameObject;
            if (mineral.TryGetComponent<MineralAsButton>(out var component))
            {
                var information = _enteredMinerals[mineral];
                information.Value.Remove(other);
                if (information.Value.Count == 0)
                {
                    component.enabled = information.Key;
                    _enteredMinerals.Remove(mineral);
                }
                
                var mineralQuizItem = mineral.GetComponent<QuizItem>();
                var text = mineralName.GetComponent<TextMeshPro>().text;
                if (text == MineralNameConverter.Convert(mineralQuizItem.mineral))
                {
                    if (_enteredMinerals.Count == 0)
                    {
                        mineralName.GetComponent<TextMeshPro>().text = "Информационный \nстенд";
                    }
                    else
                    {
                        var otherMineral = _enteredMinerals.Keys.First();
                        var otherMineralInformation = otherMineral.GetComponent<QuizItem>();
                        mineralName.GetComponent<TextMeshPro>().text = MineralNameConverter.Convert(otherMineralInformation.mineral);
                    }
                }
                component.HideInformation();
            }
        }
    }
}