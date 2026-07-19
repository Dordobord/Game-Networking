// using TMPro;
// using UnityEngine;

// public class FloatingDamageText : MonoBehaviour
// {
//     [SerializeField] private TMP_Text damageText;
//     [SerializeField] private float moveSpeed = 2f;
//     [SerializeField] private float lifeTime = 1f;

//     private float timer;

//     public void Setup(int damageAmount)
//     {
//         damageText.text = damageAmount.ToString();
//     }

//     private void Update()
//     {
//         transform.position += Vector3.up * moveSpeed * Time.deltaTime;

//         timer += Time.deltaTime;

//         if (timer >= lifeTime)
//         {
//             Destroy(gameObject);
//         }
//     }
// }