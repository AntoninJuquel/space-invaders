using System;
using System.Collections;
using UnityEngine;

namespace Entities
{
    public class Entity : MonoBehaviour
    {
        [SerializeField] protected int lives;
        [SerializeField] private Sprite deathSprite;
        public event Action<Entity, float> OnDieEvent;
        protected SpriteRenderer Sr;
        protected BoxCollider2D Box;
        protected bool Dead => lives <= 0;
        private Sprite[] _sprites;
        private int _spriteIndex;

        private void Awake()
        {
            Sr = GetComponent<SpriteRenderer>();
            Box = GetComponent<BoxCollider2D>();
        }

        protected void Setup(int livesAmount, Sprite sprite, Color color)
        {
            lives = livesAmount;
            Sr.sprite = sprite;
            Sr.color = color;
            Box.size = sprite.bounds.size;
            _sprites = new[] {sprite};
        }

        public void Setup(int livesAmount, Sprite[] sprites, Color color)
        {
            Setup(livesAmount, sprites[0], color);
            _sprites = sprites;
        }

        public void SwitchSprite()
        {
            if (Dead) return;
            _spriteIndex = (_spriteIndex + 1) % _sprites.Length;
            Sr.sprite = _sprites[_spriteIndex];
        }

        public virtual void TakeDamage(int amount)
        {
            lives -= amount;
            if (lives <= 0)
                OnDie();
        }

        public void OnDie()
        {
            StartCoroutine(OnDieRoutine());
        }

        private IEnumerator OnDieRoutine()
        {
            Box.enabled = false;
            Sr.sprite = deathSprite;
            OnDieEvent?.Invoke(this, .5f);
            yield return new WaitForSeconds(.5f);
            gameObject.SetActive(false);
        }
    }
}