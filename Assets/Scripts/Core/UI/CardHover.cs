using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Core.UI
{
    public class CardHover : MonoBehaviour,IPointerMoveHandler,IPointerExitHandler
    {
        private static readonly int Rot = Shader.PropertyToID("_Rot");
        private static readonly int Wei = Shader.PropertyToID("_Wei");
        private static readonly int Pos = Shader.PropertyToID("_Pos");

        Material _material;
        private float wei = 0f;
        private Vector2 pos = new(0,0);
        private bool isHovering = false;
        
        void Start()
        {
            _material = GetComponent<RawImage>().material;
        }

        private void Update()
        {
            if (!_material)
            {
                return;
            }

            if (isHovering)
            {
                return;
            }
            wei = Mathf.Lerp(wei - Time.deltaTime*0.5f, 0f, 0.4f);
            pos.x = pos.x <= 0f ? Mathf.Clamp(pos.x + Time.deltaTime*15, -1f, 0f) : Mathf.Lerp(pos.x - Time.deltaTime*15, 0f, 1f);
            _material.SetFloat(Wei, wei);
            _material.SetVector(Pos, pos);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!_material)
            {
                return;
            }
            isHovering = true;
            if (!Camera.main)
            {
                return;
            }

            if (!isHovering)
            {
                return;
            }

            var dir = (Vector2)transform.position - (Vector2)Camera.main.ScreenToWorldPoint(eventData.position);
                var angle = Mathf.Atan2(dir.y, dir.x);
                _material.SetFloat(Rot, angle);
            
            wei = Mathf.Lerp(wei,Vector2.Distance(transform.position, Camera.main.ScreenToWorldPoint(eventData.position))*0.1f, 0.3f);
            wei = Mathf.Clamp(wei, 0f, 0.4f);
            pos.x = -0.5f*Mathf.Lerp(pos.x,Vector2.Dot((Vector2)transform.position - (Vector2)Camera.main.ScreenToWorldPoint(eventData.position),new Vector2(-1,1)), 0.1f);
            pos.x = Mathf.Clamp(pos.x, -1f, 1f);
            _material.SetVector(Pos, pos);
            _material.SetFloat(Wei, wei);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
        }
    }
}
