using UnityEngine;


    public class AnimationControllerDispatcher : MonoBehaviour
    {
        public interface IAttackFrameReceiver
        {
            void AttackFrame();
        }

        public interface IFootstepFrameReceiver
        {
            void FootstepFrame();
        }

        public MonoBehaviour AttackFrameReceiver;
        public MonoBehaviour FootstepFrameReceiver;

        IAttackFrameReceiver m_AttackReceiver;
        IFootstepFrameReceiver m_FootstepFrameReceiver;

        void Awake()
        {
            if (AttackFrameReceiver != null)
            {
                m_AttackReceiver = AttackFrameReceiver as IAttackFrameReceiver;

                if (m_AttackReceiver == null)
                {
                    Debug.LogError("The Monobehaviour set as Attack Frame Receiver don't implement the IAttackFrameReceiver interface!", AttackFrameReceiver);
                }
            }

            if (FootstepFrameReceiver)
            {
                m_FootstepFrameReceiver = FootstepFrameReceiver as IFootstepFrameReceiver;

                if (m_AttackReceiver == null)
                {
                    Debug.LogError("The Monobehaviour set as Footstep Frame Receiver don't implement the IFootstepFrameReceiver interface!", FootstepFrameReceiver);
                }
            }
        }

        void AttackEvent()
        {
            m_AttackReceiver?.AttackFrame();
        }

        void FootstepEvent()
        {
            m_FootstepFrameReceiver?.FootstepFrame();
        }
    }
