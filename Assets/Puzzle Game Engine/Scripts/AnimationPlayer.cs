using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class AnimationPlayer : MonoBehaviour
    {
        private Animation anim;

        private void Awake()
        {
            anim = GetComponent<Animation>();
        }

        public void PlayAnimation(string nameOfAnimation)
        {
            if (anim != null)
            {
                anim = GetComponent<Animation>();

                if (anim != null)
                    anim.Play(nameOfAnimation);
            }
        }
    }
}
