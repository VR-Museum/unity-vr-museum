using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using src.museum.quiz.model.item;
using src.museum.quiz.script.random;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace src.museum.quiz.script
{
    
    [RequireComponent( typeof( MeshCollider ) )]
    [RequireComponent( typeof( Throwable ) )]
    [RequireComponent( typeof( VelocityEstimator) )]
    [RequireComponent( typeof( QuizItem ) )]
    public class MineralAsButton : HoverButton
    {
        public Boolean startEnabled = false;
        
        private static List<MineralAsButton> _allObjectsInfo = new List<MineralAsButton>();
        
        private List<GameObject> _slides;
        private Boolean _showingSlides;
        private int _nextSlideIndex;
        
        private bool _hovering;

        private DateTime _lastButtonDownTime;

        private float buttonDownDelay = 0.5f;

        private void Start()
        {
            if (movingPart == null && transform.childCount > 0)
                movingPart = transform.GetChild(0);

            _slides = SlideComposer.GetSlides(GetComponent<QuizItem>().mineral).Select(slideItem => slideItem.Slide).ToList();
            _lastButtonDownTime = DateTime.Now;
            _nextSlideIndex = 0;
            _showingSlides = false;
            UpdateAllSlidesRenderers(false);
            
            enabled = startEnabled;
            
            _allObjectsInfo.Add(this);
        }
        
        [UsedImplicitly]
        private void HandHoverUpdate(Hand hand)
        {
            if (enabled && !_hovering && hand is {handType: SteamVR_Input_Sources.RightHand})
            {
                HideInformation();
                return;
            }

            if (enabled && !_hovering && (hand is {handType: SteamVR_Input_Sources.Any} || hand is {handType: SteamVR_Input_Sources.LeftHand}))
            {
                InvokeEvents();
            }
            _hovering = true;
        }

        private void LateUpdate()
        {
            _hovering = false;
        }

        private void InvokeEvents()
        {   
            if (DateTime.Now.Subtract(_lastButtonDownTime).TotalSeconds > buttonDownDelay)
            {
                _lastButtonDownTime = DateTime.Now;
                if (_slides.Count == 0)
                {
                    return;
                }
                if (_showingSlides)
                {
                    var prevSlideIndex = (_nextSlideIndex + _slides.Count - 1) % _slides.Count;
                    UpdateSlideRenderers(false, prevSlideIndex);
                }
                else
                {
                    foreach (var objectInfo in _allObjectsInfo)
                    {
                        if (objectInfo._showingSlides)
                        {
                            objectInfo.HideInformation();
                        }
                    }
                }
                UpdateSlideRenderers(true, _nextSlideIndex);
                _nextSlideIndex = (_nextSlideIndex + 1) % _slides.Count;
                _showingSlides = true;
            }
        }

        private void setNewRendererEnabled(Boolean newEnabled, Renderer[] renderers)
        {
            foreach (var textRenderer in renderers)
            {
                textRenderer.enabled = newEnabled;
            }
        }

        private void setCollidersEnabled(Boolean newEnabled, Collider[] colliders)
        {
            foreach (var textRenderer in colliders)
            {
                textRenderer.enabled = newEnabled;
            }
        }

        private void UpdateSlideRenderers(Boolean newEnabled, int slideIndex)
        {
            var slide = _slides[slideIndex];
            var renderers = slide.GetComponentsInChildren<Renderer>();
            setNewRendererEnabled(newEnabled, renderers);
            var colliders = slide.GetComponentsInChildren<Collider>();
            setCollidersEnabled(newEnabled, colliders);
        }

        private void UpdateAllSlidesRenderers(Boolean newEnabled)
        {
            for (int slideIndex = 0; slideIndex < _slides.Count; slideIndex++)
            {
                UpdateSlideRenderers(newEnabled, slideIndex);
            }
        }

        private void HideInformation()
        {
            UpdateAllSlidesRenderers(false);
            _showingSlides = false;
            _nextSlideIndex = 0;
        }
    }
}