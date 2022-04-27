using System;
using System.Collections.Generic;
using src.museum.quiz.model.decomposition;
using src.museum.quiz.model.item;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace src.museum.quiz.script.decomposition
{
    public class DecompositionAssistant : MonoBehaviour
    {
        public static List<GameObject> DecomposableMinerals;
        
        public float triggerDistance = 1.5f;

        private void Update()
        {
            var mineralsToDecompose = FindItemsToDecompose();
            foreach (var mineral in mineralsToDecompose)
            {
                var decompositionItem = mineral.GetComponent<DecompositionItem>();
                Decompose(decompositionItem);
                DecomposableMinerals.Remove(mineral);
                QuizComposer.SavedItems.ForEach(pair => pair.Value.RemoveAll(item => item.MineralObject == mineral));
                QuizComposer.AllItems.RemoveAll(item => item.MineralObject == mineral);
                decompositionItem.enabled = false;
                // Destroy(mineral);
            }
        }

        void Decompose(DecompositionItem originalItems)
        {
            if (!originalItems.enabled)
            {
                return;
            }
            var items = new List<DecompositionItem.DecompositionData>(originalItems.MineralPart);
            foreach (var mineral in items)
            {
                if (DecompositionSpawner.MineralsToSpawn == null)
                {
                    DecompositionSpawner.MineralsToSpawn = new List<DecompositionItem.DecompositionData>();
                }
                DecompositionSpawner.MineralsToSpawn.Add(mineral);
            }
        }

        List<GameObject> FindItemsToDecompose()
        {
            var items = new List<GameObject>();
            if (DecomposableMinerals == null || DecomposableMinerals.Count == 0)
            {
                return items;
            }
            foreach (var mineral in DecomposableMinerals)
            {
                var distance = CalculateDistance(mineral.GetComponent<QuizItem>().mineralCenter.position);
                var attachedToHand = mineral.GetComponent<Interactable>().attachedToHand;
                if (distance < triggerDistance && attachedToHand == null)
                {
                    items.Add(mineral);
                }
            }
            return items;
        }

        private float CalculateDistance(Vector3 otherPosition)
        {
            var position = transform.position;
            var otherDx = otherPosition.x - position.x;
            var otherDy = otherPosition.y - position.y;
            var otherDz = otherPosition.z - position.z;
            return (float) Math.Sqrt(otherDx * otherDx + otherDy * otherDy + otherDz * otherDz);
        }

        private void OnDestroy()
        {
            DecomposableMinerals = new List<GameObject>();
        }
    }
}
