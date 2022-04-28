using System;
using System.Collections.Generic;
using src.museum.quiz.model.decomposition;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace src.museum.quiz.script.decomposition
{
    public class DecompositionAssistant : MonoBehaviour
    {
        public Boolean destroyMineral = false;
        
        public static List<GameObject> DecomposableMinerals;
        
        void Decompose(DecompositionItem originalItems)
        {
            if (!originalItems.enabled)
            {
                return;
            }
            var items = new List<DecompositionItem.DecompositionData>(originalItems.MineralPart);
            foreach (var mineral in items)
            {
                DecompositionSpawner.MineralsToSpawn ??= new List<DecompositionItem.DecompositionData>();
                DecompositionSpawner.MineralsToSpawn.Add(mineral);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var mineral = other.gameObject;
            if (mineral.TryGetComponent<DecompositionItem>(out var decompositionItem))
            {
                Decompose(decompositionItem);
                DecomposableMinerals.Remove(mineral);
                decompositionItem.enabled = false;
                if (destroyMineral)
                {
                    DestroyMineral(mineral);
                }
            }
        }

        private void DestroyMineral(GameObject mineral)
        {
            QuizComposer.SavedItems.ForEach(pair => pair.Value.RemoveAll(item => item.MineralObject == mineral));
            QuizComposer.AllItems.RemoveAll(item => item.MineralObject == mineral);
            Destroy(mineral);
        }
        
        private void OnDestroy()
        {
            DecomposableMinerals = new List<GameObject>();
        }
    }
}
