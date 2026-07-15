// using UnityEngine;

// public class AnimateObject : Interactable
// {
//     Animator animator;
//     private string startPrompt;

//     void Start()
//     {
//         animator = GetComponentInParent<Animator>();
//         startPrompt = promptMessage;
//     }

//     void Update()
//     {
//         if (animator.GetCurrentAnimatorStateInfo(0).IsName("Default"))
//         {
//             promptMessage = startPrompt;
//         }
//         else
//         {
//             promptMessage = "Animating...";
//         }
//     }
//     protected override void Interact()
//     {
//         animator.Play("Spin");
//     }
// }
