using System.Collections;
using TMPro;
using UnityEngine;

namespace Entities
{
    public class PlayerSpaceship : Spaceship
    {
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private TextMeshProUGUI livesText;
        private float _horizontalInput;
      
        private void Start()
        {
            GetComponentInChildren<Canvas>().worldCamera = Camera.main;
        }

        private void Update()
        {
            _horizontalInput = Input.GetAxisRaw("Horizontal");
            if (Input.GetKeyDown(KeyCode.Space))
                Shoot(Vector2.up, "Ally");

            if (Physics2D.Linecast(transform.position + Vector3.left + Vector3.down * .25f, transform.position + Vector3.right + Vector3.down * .25f, enemyLayer))
            {
                GameManager.GameOver();
            }
        }

        private void FixedUpdate()
        {
            Move(Vector2.right * _horizontalInput);
        }

        private void Move(Vector3 direction)
        {
            transform.position += direction.normalized * (speed * Time.fixedDeltaTime);
        }

        private IEnumerator RespawnRoutine()
        {
            transform.position = new Vector3(15, 2);
            Sr.enabled = Box.enabled = this.enabled = false;
            if (lives <= 0) yield break;
            yield return new WaitForSeconds(1f);
            Sr.enabled = Box.enabled = this.enabled = true;
        }

        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
            livesText.text = string.Concat("<sprite=0> ", lives);
            StartCoroutine(RespawnRoutine());
        }

        public override void OnDie()
        {
            GameManager.GameOver();
        }
    }
}