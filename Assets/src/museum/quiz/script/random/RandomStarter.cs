using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using src.museum.quiz.model.item;
using UnityEngine;
using Random = System.Random;

namespace src.museum.quiz.script.random
{
    public class RandomStarter : MonoBehaviour
    {
        public static List<GameObject> SpawnPoints = new List<GameObject>();
        public List<string> prefabsPaths;
        private List<string> _unusedPrefabsPaths;
        
        private static readonly Dictionary<string, GameObject> LoadedPrefabs = new Dictionary<string, GameObject>();

        private List<LoadingInformation> _loadings;

        public List<GameObject> stands;
        private readonly Random _random = new Random((int) DateTime.Now.ToBinary());
        
        private float _spawnDelay = 0.3f;
        private DateTime _previousSpawnTime;

        private List<GameObject> _mineralsToSpawn;

        public void Start()
        {
            _unusedPrefabsPaths = new List<string>();
            _unusedPrefabsPaths.AddRange(prefabsPaths);
            _loadings = new List<LoadingInformation>();
            Application.backgroundLoadingPriority = ThreadPriority.High;
            SelectMinerals();
        }

        private void Update()
        {
            if (_loadings.Count != 0)
            {
                var loaded = new List<LoadingInformation>();
                foreach (var loadingInformation in _loadings)
                {
                    var loading = loadingInformation.Loading;
                    if (loading.isDone)
                    {
                        var prefab = loading.asset as GameObject;
                        if (prefab != null)
                        {
                            _mineralsToSpawn ??= new List<GameObject>();
                            _mineralsToSpawn.Add(prefab);
                            var expectedMinerals = new List<MineralName> {prefab.GetComponent<QuizItem>().mineral};
                            loadingInformation.Stand.GetComponent<IndicatorQuizAssistant>().SetExpectedMinerals(expectedMinerals);
                            LoadedPrefabs.Add(loadingInformation.Path, prefab);
                            loaded.Add(loadingInformation);
                        }
                        else
                        {
                            throw new Exception("Something went wrong while tried to load prefab " + loadingInformation.Path);
                        }
                    }
                }
                _loadings.RemoveAll(loading => loaded.Contains(loading));
            }
            else
            {
                
            }
            
            if (_mineralsToSpawn.Count > 0 && DateTime.Now.Subtract(_previousSpawnTime).TotalSeconds > _spawnDelay)
            {
                var prefab = _mineralsToSpawn.First();
                var textureOffset = CalculateOffset(prefab);
                _mineralsToSpawn.Remove(prefab);
                _previousSpawnTime = DateTime.Now;
                var spawnPoint = gameObject;
                if (SpawnPoints.Count > 0)
                {
                    var index = _random.Next() % SpawnPoints.Count;
                    spawnPoint = SpawnPoints[index];
                    SpawnPoints.Remove(spawnPoint);
                }

                var spawnPosition = spawnPoint.transform.position;
                Instantiate(
                    prefab, 
                    new Vector3(spawnPosition.x + textureOffset.x, spawnPosition.y + 0.3f + textureOffset.y, spawnPosition.z + textureOffset.z), 
                    prefab.transform.rotation
                );
            }
        }

        private void SelectMinerals()
        {
            foreach (var stand in stands)
            {
                if (_unusedPrefabsPaths.Count != 0)
                {
                    var index = _random.Next() % _unusedPrefabsPaths.Count;
                    var prefabPath = _unusedPrefabsPaths[index];
                    
                    _unusedPrefabsPaths.Remove(prefabPath);
                    
                    if (LoadedPrefabs.ContainsKey(prefabPath))
                    {
                        var prefab = LoadedPrefabs[prefabPath];
                        _mineralsToSpawn ??= new List<GameObject>();
                        _mineralsToSpawn.Add(prefab);
                        var expectedMinerals = new List<MineralName> {prefab.GetComponent<QuizItem>().mineral};
                        stand.GetComponent<IndicatorQuizAssistant>().SetExpectedMinerals(expectedMinerals);
                    }
                    else
                    {
                        var loading = Resources.LoadAsync(prefabPath);
                        _loadings.Add(new LoadingInformation(loading, stand, prefabPath));
                        stand.GetComponent<IndicatorQuizAssistant>().SetLoadsText();
                    }
                }
                else
                {
                    stand.GetComponent<IndicatorQuizAssistant>().Hide();
                }
            }
        }

        private Vector3 CalculateOffset(GameObject mineral)
        {
            var realCenter = mineral.GetComponent<QuizItem>().mineralCenter.position;
            var offsetCenter = mineral.transform.position;
            return offsetCenter - realCenter;
        }

        private struct LoadingInformation
        {
            public readonly ResourceRequest Loading;
            [CanBeNull] public readonly GameObject Stand;
            public readonly string Path;

            public LoadingInformation(ResourceRequest loading, GameObject stand, string path)
            {
                Path = path;
                Stand = stand;
                Loading = loading;
            }
        }

        private void OnDestroy()
        {
            SpawnPoints = new List<GameObject>();
        }
    }
}