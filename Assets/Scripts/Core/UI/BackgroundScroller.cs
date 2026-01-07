using System.Collections;
using Core.SystemModules;
using UnityEngine;

namespace Core.UI
{
    public class BackgroundScroller : MonoBehaviour
    {
        private Material _material;

        [SerializeField] Vector2 scrollSpeed;
    
        void Awake()
        {
            _material = GetComponent<Renderer>().material;
        }

        private IEnumerator Start()
        {
            while (GameManager.GameState != GameState.GameOver)
            {
                _material.mainTextureOffset += scrollSpeed * Time.deltaTime;
                yield return null;
            }
        }


    }
}
